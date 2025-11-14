/*
 *  SPRESENSE_WiFi.ino - GainSpan WiFi Module Control Program
 *  Copyright 2019 Norikazu Goto
 *
 *  Modified to send analog pin data instead of microphone data.
 */

#include <TelitWiFi.h>
#include "config.h"

#define CONSOLE_BAUDRATE  115200
#define MAX_LENGTH 10
#define ANALOG_PIN A0  // 読み取るアナログピンの定義

const uint8_t TCP_Data[] = "GS2200 TCP Client Data Transfer Test.";
const uint16_t TCP_RECEIVE_PACKET_SIZE = 1500;
uint8_t TCP_Receive_Data[TCP_RECEIVE_PACKET_SIZE] = {0};

TelitWiFi gs2200;
TWIFI_Params gsparams;


static void led_onoff(int num, bool stat) {
  switch (num) {
    case 0:
      digitalWrite(LED0, stat);
      break;
    case 1:
      digitalWrite(LED1, stat);
      break;
    case 2:
      digitalWrite(LED2, stat);
      break;
    case 3:
      digitalWrite(LED3, stat);
      break;
  }
}

static void led_effect(void) {
  static int cur = 0;
  int i;
  static bool direction = true; // which way to go

  for (i = -1; i < 5; i++) {
    if (i == cur) {
      led_onoff(i, true);
      if (direction)
        led_onoff(i - 1, false);
      else
        led_onoff(i + 1, false);
    }
  }

  if (direction) { // 0 -> 1 -> 2 -> 3
    if (++cur > 4)
      direction = false;
  } else {
    if (--cur < -1)
      direction = true;
  }
}

struct IntArray {
  uint8_t array[MAX_LENGTH + 1];
  int length;
};

struct IntArray convert(int value) {
  IntArray result;
  result.length = 0;

  // value=0の場合の特別処理
  if (value == 0) {
    result.array[0] = '0';
    result.array[1] = '\0';
    result.length = 1;
    return result;
  }

  // 桁数を計算
  int v = value;
  while (v != 0) {
    v /= 10;
    result.length++;
  }

  // 各桁を文字に変換
  for (int i = 0; i < result.length; i++) {
    result.array[result.length - i - 1] = '0' + value % 10;
    value /= 10;
  }
  result.array[result.length] = '\0';
  return result;
}

void setup() {
  /* initialize digital pin of LEDs as an output. */
  pinMode(LED0, OUTPUT);
  pinMode(LED1, OUTPUT);
  pinMode(LED2, OUTPUT);
  pinMode(LED3, OUTPUT);

  digitalWrite(LED0, LOW);   // turn the LED off (LOW is the voltage level)
  Serial.begin(CONSOLE_BAUDRATE); // talk to PC

  /* Initialize AT Command Library Buffer */
  AtCmd_Init();
  /* Initialize SPI access of GS2200 */
  Init_GS2200_SPI_type(iS110B_TypeC);
  /* Initialize AT Command Library Buffer */
  gsparams.mode = ATCMD_MODE_STATION;
  gsparams.psave = ATCMD_PSAVE_DEFAULT;
  if (gs2200.begin(gsparams)) {
    ConsoleLog("GS2200 Initilization Fails");
    while (1);
  }

  /* GS2200 Association to AP */
  if (gs2200.activate_station(AP_SSID, PASSPHRASE)) {
    ConsoleLog("Association Fails");
    while (1);
  }
  digitalWrite(LED0, HIGH); // turn on LED
}

// the loop function runs over and over again forever
void loop() {
  //ネットワーク処理
  char server_cid = 0;
  bool served = false;
  uint32_t timer = 0;

  while (1) {
    if (!served) {
      // Start a TCP client
      server_cid = gs2200.connect(TCPSRVR_IP, TCPSRVR_PORT);
      ConsolePrintf("server_cid: %d \r\n", server_cid);
      if (server_cid == ATCMD_INVALID_CID) {
        continue;
      }
      served = true;
    } else {
      ConsoleLog("Start to send TCP Data");
      // Prepare for the next chunk of incoming data
      WiFi_InitESCBuffer();

      // Start the infinite loop to send the data
      while (1) {
        // アナログ値を読み取る
        int analog_value = analogRead(ANALOG_PIN);
        IntArray DATA = convert(analog_value);

        // アナログ値をPCに送信 (result.lengthを使用)
        if (false == gs2200.write(server_cid, DATA.array, DATA.length)) {
          Serial.println("Failed to send data");
          delay(10);
        }

        // LEDエフェクトを更新
        if (msDelta(timer) > 100) {
          timer = millis();
          led_effect();
        }
        delay(100);
      }
    }
  }
}
