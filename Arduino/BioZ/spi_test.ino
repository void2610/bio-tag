#include <Arduino.h>
#include <SPI.h>
#include <softSPI.h>

SoftSPI mySPI(11, A0, 13);
int csPin = 10;

void setup()
{
    Serial.begin(9600);
    mySPI.begin();
    mySPI.setDataMode(SPI_MODE0);
    mySPI.setClockDivider(SPI_CLOCK_DIV2);
    mySPI.setBitOrder(MSBFIRST);
    mySPI.setThreshold(256);

    pinMode(csPin, OUTPUT);
    digitalWrite(csPin, HIGH);
}

void loop()
{
    byte adrr = 0x0e;
    write_reg(0x1A, 0x20);
    write_reg(0x19, 0x01);
    byte data = read_reg(0x1A);
    Serial.print("Data: ");
    Serial.println(data, HEX);
    byte d2 = read_reg(0x19);
    Serial.print("Data: ");
    Serial.println(d2, HEX);
    delay(1000);
}

byte read_reg(byte regAddress)
{
    byte data;

    digitalWrite(csPin, LOW);
    mySPI.transfer(regAddress);
    mySPI.transfer(0x80);        // 読み取りコマンドを送信
    data = mySPI.transfer(0x00); // ダミーデータを書き込んで読み取る
    digitalWrite(csPin, HIGH);

    return data;
}

void write_reg(byte regAddress, byte v)
{
    digitalWrite(csPin, LOW);
    mySPI.transfer(regAddress);
    mySPI.transfer(0x00); // 書き込みコマンドを送信
    mySPI.transfer(v);
    digitalWrite(csPin, HIGH);
}
