/*
 *  SPRESENSE_WiFi.ino - GainSpan WiFi Module Control Program
 *  Copyright 2019 Norikazu Goto
 *
 *  This work is free software; you can redistribute it and/or modify it under the terms 
 *  of the GNU Lesser General Public License as published by the Free Software Foundation; 
 *  either version 2.1 of the License, or (at your option) any later version.
 *
 *  This work is distributed in the hope that it will be useful, but without any warranty; 
 *  without even the implied warranty of merchantability or fitness for a particular 
 *  purpose. See the GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License along with 
 *  this work; if not, write to the Free Software Foundation, 
 *  Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
*/

#include <Audio.h>
#include <TelitWiFi.h>
#include <CCS811.h>
#include "config.h"

#define  CONSOLE_BAUDRATE  115200
#define MAX_LENGTH 10

const uint8_t TCP_Data[] = "GS2200 TCP Client Data Transfer Test.";
const uint16_t TCP_RECEIVE_PACKET_SIZE = 1500;
uint8_t TCP_Receive_Data[TCP_RECEIVE_PACKET_SIZE] = {0};

TelitWiFi gs2200;
TWIFI_Params gsparams;
CCS811 sensor;

AudioClass *theAudio;
static const int32_t buffer_size = 512;      // 256 samples per call at 16K, 2 bytes per, 1 ch mono
static char          s_buffer[buffer_size];

bool ErrEnd = false;
int volume = 0;

static void led_onoff(int num, bool stat)
{
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

static void led_effect(void)
{
	static int cur=0;
	int i;
	static bool direction=true; // which way to go
	

	for (i=-1; i<5; i++) {
		if (i==cur) {
			led_onoff(i, true);
			if (direction)
				led_onoff(i-1, false);
			else
				led_onoff(i+1, false);
		}
	}

	if (direction) { // 0 -> 1 -> 2 -> 3
		if (++cur > 4)
			direction = false;
	}
	else {
		if (--cur < -1)
			direction = true;
	}
		
}

struct IntArray{
    uint8_t array[MAX_LENGTH + 1];
    int length;
};

struct IntArray convert(int value) {
    IntArray result;
    result.length = 0;
    int v = value;
    while (v != 0) {
        v /= 10;
        result.length++;
    }
    for (int i = 0; i < result.length; i++) {
        result.array[result.length - i - 1] = '0' + value % 10;
        value /= 10;
    }
    result.array[result.length] = '\0';
    return result;
}

//エラーが起きたらこれがよばれる
void audio_attention_cb(const ErrorAttentionParam *atprm)
{
  puts("Attention!");
  
  if (atprm->error_code >= AS_ATTENTION_CODE_WARNING)
    {
      ErrEnd = true;
   }
}


// FIFOが空の場合にフレームを実行する
void execute_frames()
{
  uint32_t read_size = 0;
  do
    {
      err_t err = execute_aframe(&read_size);
      if ((err != AUDIOLIB_ECODE_OK)
       && (err != AUDIOLIB_ECODE_INSUFFICIENT_BUFFER_AREA))
        {
          break;
        }
    }
  while (read_size > 0);
}


//1フレーム分の処理
err_t execute_aframe(uint32_t* size)
{
  err_t err = theAudio->readFrames(s_buffer, buffer_size, size);

  if(((err == AUDIOLIB_ECODE_OK) || (err == AUDIOLIB_ECODE_INSUFFICIENT_BUFFER_AREA)) && (*size > 0)) 
    {
      signal_process(*size);
    }

  return err;
}

void signal_process(uint32_t size) {
  signed int sigmax = 0;
  uint16_t idx;
  union Combine {
    short target;
    char dest[sizeof(short)];
  };
  Combine cc;

  for (idx = 0; idx < size; idx += 2) {
    cc.dest[0] = s_buffer[idx];
    cc.dest[1] = s_buffer[idx + 1];
    if (cc.target > sigmax) {
      sigmax = cc.target;
    }
  }

  volume = sigmax;
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
		while(1);
	}

	/* GS2200 Association to AP */
	if (gs2200.activate_station(AP_SSID, PASSPHRASE)) {
		ConsoleLog("Association Fails");
		while(1);
	}
	digitalWrite(LED0, HIGH); // turn on LED

  theAudio = AudioClass::getInstance();
  theAudio->begin(audio_attention_cb);
  puts("initialization Audio Library");

  theAudio->setRecorderMode(AS_SETRECDR_STS_INPUTDEVICE_MIC,210);
  theAudio->initRecorder(AS_CODECTYPE_PCM, "/mnt/sd0/BIN", AS_SAMPLINGRATE_16000, AS_CHANNEL_MONO);
  puts("Init Recorder!");

  puts("Rec!");
  theAudio->startRecorder();
}

// the loop function runs over and over again forever
int count = 0;
void loop() {
  //ネットワーク処理
	char server_cid = 0;
	bool served = false;
	uint32_t timer=0;
	int receive_size = 0;

  IntArray DATA;
	while (1) {
		if (!served) {
			// Start a TCP client
			server_cid = gs2200.connect(TCPSRVR_IP, TCPSRVR_PORT);
			ConsolePrintf("server_cid: %d \r\n", server_cid);
			if (server_cid == ATCMD_INVALID_CID) {
				continue;
			}
			served = true;
		}
		else {
			ConsoleLog("Start to send TCP Data");
			// Prepare for the next chunck of incoming data
			WiFi_InitESCBuffer();

			// Start the infinite loop to send the data
			while (1) {
        static int32_t total_size = 0;
        uint32_t read_size =0;

        /* Execute audio data */
        err_t err = execute_aframe(&read_size);
        if (err != AUDIOLIB_ECODE_OK && err != AUDIOLIB_ECODE_INSUFFICIENT_BUFFER_AREA)
        {
          // theAudio->stopRecorder();
          // return 1;
        }
        else if (read_size>0)
        {
          total_size += read_size;
        }

        // if (ErrEnd)
        // {
        //   printf("Error End\n");
        //   theAudio->stopRecorder();
        //   return 1;
        // }
        
        delay(10);
          DATA = convert(volume);
          Serial.println(volume);
          if (false == gs2200.write(server_cid, DATA.array, strlen((const char*)DATA.array))) {
            // Data is not sent, we need to re-send the data
            Serial.println("faild");
            delay(10);
        }

				if (msDelta(timer) > 100) {
					timer = millis();
					led_effect();
				}
			}
		}
	}
}
