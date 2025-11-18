/*
 * Arduino Uno GSRセンサー読み取りプログラム
 * GROVE GSRセンサーからデータを読み取り、USBシリアル経由でUnityに送信
 *
 * ハードウェア接続:
 *   - VCC (赤) → Arduino 5V または 3.3V
 *   - GND (黒) → Arduino GND
 *   - SIG (黄/白) → Arduino A0
 *
 * Unity側設定:
 *   - SerialServer使用
 *   - ボーレート: 9600
 *   - データフォーマット: 改行区切りテキスト（整数値）
 */

const int GSR = A0;  // GROVE GSRセンサー接続ピン
int sensorValue = 0;
int gsr_average = 0;

void setup() {
  Serial.begin(9600);  // Unity SerialServerと同じボーレート
}

void loop() {
  long sum = 0;

  // 10回の測定を平均化してグリッチを除去
  for (int i = 0; i < 10; i++) {
    sensorValue = analogRead(GSR);
    sum += sensorValue;
    delay(5);
  }

  gsr_average = sum / 10;

  // Unity SerialServerに送信（改行区切り）
  Serial.println(gsr_average);
}