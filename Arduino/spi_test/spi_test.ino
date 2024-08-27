#include <Arduino.h>
#include <SPI.h>

int csPin = 10;
void setup()
{
    Serial.begin(9600);
    SPI.begin();
    SPI.setClockDivider(SPI_CLOCK_DIV8);
    SPI.setDataMode(SPI_MODE0);
    SPI.setBitOrder(MSBFIRST);

    pinMode(csPin, OUTPUT);
    digitalWrite(csPin, HIGH);
}

void loop()
{
    byte data = read_reg(0x00);
    Serial.print("Data: ");
    Serial.println(data, HEX);
    delay(1000);
}

byte read_reg(byte regAddress)
{
    byte data;

    digitalWrite(csPin, LOW);
    SPI.transfer(regAddress);
    SPI.transfer(0b10000000);  // 読み取りコマンドを送信
    data = SPI.transfer(0x00); // ダミーデータを書き込んで読み取る
    digitalWrite(csPin, HIGH);

    return data;
}
