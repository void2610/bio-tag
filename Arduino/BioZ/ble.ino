#include <ArduinoBLE.h>

BLEService customService("180C");  // カスタムサービスUUID
BLECharacteristic customCharacteristic("2A56", BLERead | BLENotify, 16);  // 読み取りと通知可能なキャラクタリスティック

void setup() {
  Serial.begin(9600);
  while (!Serial);

  if (!BLE.begin()) {
    Serial.println("Starting BLE failed!");
    while (1);
  }

  BLE.setLocalName("Nano33BLEDevice");
  BLE.setAdvertisedService(customService);
  customService.addCharacteristic(customCharacteristic);
  BLE.addService(customService);

  BLE.advertise();
  Serial.println("BLE device is now advertising...");
}

void loop() {
  // 中央デバイスが接続されたかどうか確認
  BLEDevice central = BLE.central();

  if (central) {
    Serial.print("Connected to central: ");
    Serial.println(central.address());

    // 仮のセンサー値（16進数: F00000000000000F）を送信
    byte sensorData[] = { 0xF0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0F };
    customCharacteristic.writeValue(sensorData, sizeof(sensorData));  // 書き込みと通知

    while (central.connected()) {
      // センサー値を継続して送信する
      customCharacteristic.writeValue(sensorData, sizeof(sensorData));
      delay(10);  // 1秒間隔で送信
    }

    Serial.println("Disconnected from central");
  }
}