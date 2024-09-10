#include <Arduino.h>
#include <SPI.h>
#include <softSPI.h>
#include <ArduinoBLE.h>

#define SEND_BUF_SIZE 8

BLEService customService("180C");                                                   // カスタムサービスUUID
BLECharacteristic customCharacteristic("2A56", BLEWrite | BLERead | BLENotify, 16); // 読み取りと通知可能なキャラクタリスティック
BLEDevice central;
bool ble_connected = false;

SoftSPI mySPI(11, A0, 13);
int csPin = 10;

uint8_t SendBuff[SEND_BUF_SIZE] = {0xf0, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x0f}; // 送信データバッファ
uint8_t id = 0x00, i = 255, j = 255, flag = 0, a[10], counter = 0, counter2 = 0, tempREG;
uint16_t read_pointer, write_pointer;
uint32_t timecounter;
char received_msg[9] = {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
char reset_msg[9] = {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
char Sendingstr[3];
int data = 0;
int calib = 250;

void NewFrequencyCalibration(uint8_t reg0x17, uint8_t reg0x18, uint8_t reg0x20)
{
    Serial.println("Calibration start");
    uint8_t tempREG1;
    uint8_t tempREG2;
    write_reg(0x20, reg0x20); // DAC/ADC OSRの設定とVrefの有効化; PLL_ENが1に設定される前にBioZリファレンスを有効にする必要があります(BIOZ_BG_EN[2](0x20)=1)。解決には6msの時間がかかる可能性があります。
    write_reg(0x18, reg0x18); // MDIVの下位ビットの設定
    write_reg(0x17, reg0x17); // MDIVの上位ビット、NDIV=512、KDIV=1、およびPLLの有効化
    PLL_enable();
    tempREG1 = read_reg(0x22); // 電流を最小に設定し、保存された電流値を記録
    tempREG2 = tempREG1 & 0xC3;
    write_reg(0x22, tempREG2);
    bypass(); // 0x25レジスタの値を確認し、短絡測定バイアスを有効にする
    I_Q_open();
    delay(2);
    FLUSH_FIFO(); // FIFOをクリア
    // MAX30009が新しいデータを送信しているかどうかを確認し、上位機器に65535回の短絡データを送信
    for (timecounter = 0; timecounter < calib;)
    {
        read_pointer = read_reg(0x09);
        write_pointer = read_reg(0x08);
        if (write_pointer > read_pointer || read_pointer - write_pointer >= 0x6f)
        {
            timecounter++;
            set_receive_msg();
            SendBuff[0] = 0xf0;
            tempREG = received_msg[0];
            // 元のMAX30009データは0x1ooooo2oooooで、0x3ooooo4oooooに変更して上位機器がどこまで校正を行っているかを識別できるようにしました。以降も同様です。
            tempREG = tempREG & 0x0f;
            SendBuff[1] = tempREG | 0x30;
            SendBuff[2] = received_msg[1];
            SendBuff[3] = received_msg[2];
            tempREG = received_msg[3];
            tempREG = tempREG & 0x0f;
            SendBuff[4] = tempREG | 0x40;
            SendBuff[5] = received_msg[4];
            SendBuff[6] = received_msg[5];
            SendBuff[7] = 0x0f;
            reset_receive_msg();
            show_send_buff();
        }
    }
    I_Q_close();               // IQチャンネルを閉じる
    write_reg(0x22, tempREG1); // 出力電流を元に戻す
    NOT_bypass();              // 0x25レジスタの値を確認し、短絡測定バイアスを無効にする
    write_reg(0x41, 0x00);     // チップ内部の抵抗校正を有効にする。0x44の値が0x11であるため、実際の抵抗はR*(1+17/512)です。詳細はマニュアルを参照してください。(2番目のチップ、つまり印刷されていないチップの値は0x13で、実際の抵抗はR*(1+19/512)です)
    write_reg(0x42, 0x60);
    Q_to_I();
    I_Q_open();
    delay(2);
    FLUSH_FIFO(); // FIFOをクリア
    // 65535回のQクロック位相をIの校正抵抗に移動するデータを送信
    for (timecounter = 0; timecounter < calib;)
    {
        read_pointer = read_reg(0x09);
        write_pointer = read_reg(0x08);
        if (write_pointer > read_pointer || read_pointer - write_pointer >= 0x6f)
        {
            timecounter++;
            set_receive_msg();
            SendBuff[0] = 0xf0;
            tempREG = received_msg[0];
            // 元のMAX30009データは0x1ooooo2oooooで、0x3ooooo4oooooに変更して上位機器がどこまで校正を行っているかを識別できるようにしました。以降も同様です。
            tempREG = tempREG & 0x0f;
            SendBuff[1] = tempREG | 0x50;
            SendBuff[2] = received_msg[1];
            SendBuff[3] = received_msg[2];
            tempREG = received_msg[3];
            tempREG = tempREG & 0x0f;
            SendBuff[4] = tempREG | 0x60;
            SendBuff[5] = received_msg[4];
            SendBuff[6] = received_msg[5];
            SendBuff[7] = 0x0f;

            show_send_buff();
            reset_receive_msg();
        }
    }
    I_Q_close();
    I_to_Q();
    I_Q_open();
    FLUSH_FIFO(); // FIFOをクリア

    // 65535回のQクロック位相をIの校正抵抗に移動するデータを送信
    for (timecounter = 0; timecounter < calib;)
    {
        read_pointer = read_reg(0x09);
        write_pointer = read_reg(0x08);
        if (write_pointer > read_pointer || read_pointer - write_pointer >= 0x6f)
        {
            timecounter++;
            set_receive_msg();
            SendBuff[0] = 0xf0;
            tempREG = received_msg[0];
            // 元のMAX30009データは0x1ooooo2oooooで、0x3ooooo4oooooに変更して上位機器がどこまで校正を行っているかを識別できるようにしました。以降も同様です。
            tempREG = tempREG & 0x0f;
            SendBuff[1] = tempREG | 0x70;
            SendBuff[2] = received_msg[1];
            SendBuff[3] = received_msg[2];
            tempREG = received_msg[3];
            tempREG = tempREG & 0x0f;
            SendBuff[4] = tempREG | 0x80;
            SendBuff[5] = received_msg[4];
            SendBuff[6] = received_msg[5];
            SendBuff[7] = 0x0f;

            show_send_buff();
            reset_receive_msg();
        }
    }
}

uint8_t Sweap(uint8_t reg0x17, uint8_t reg0x18, uint8_t reg0x20)
{
    Serial.println("Sweap start");
    // BIOZ_OFF();
    BIOZ_DRV_Standby();
    write_reg(0x20, reg0x20); // DAC/ADC OSRの設定とVrefの有効化; PLL_ENが1に設定される前にBioZリファレンスを有効にする必要があります(BIOZ_BG_EN[2](0x20)=1)。解決には6msの時間がかかる可能性があります。
    write_reg(0x18, reg0x18); // MDIVの下位ビットの設定
    write_reg(0x17, reg0x17); // MDIVの上位ビット、NDIV=512、KDIV=1、およびPLLの有効化
    PLL_enable();
    // BIOZ_ON();
    BIOZ_DRV_Current_Drive();
    // FLUSH_FIFO();
    I_Q_open();
    FLUSH_FIFO(); // FIFOをクリア

    while (central.connected())
    {
        read_pointer = read_reg(0x09);
        write_pointer = read_reg(0x08);
        if (write_pointer > read_pointer || read_pointer - write_pointer >= 0x6f)
        {
            timecounter++;
            set_receive_msg();
            if (timecounter > 0)
            {
                SendBuff[0] = 0xf0;
                SendBuff[1] = received_msg[0];
                SendBuff[2] = received_msg[1];
                SendBuff[3] = received_msg[2];
                SendBuff[4] = received_msg[3];
                SendBuff[5] = received_msg[4];
                SendBuff[6] = received_msg[5];
                SendBuff[7] = 0x0f;
                show_send_buff();
            }
            reset_receive_msg();
        }
    }
    Serial.println("BLE is disconnected");
}

void MAX30009_setup()
{
    Serial.println("MAX30009 start");
    digitalWrite(csPin, HIGH);

    // MAX3000の初期化が終わるまで待つ
    while (1)
    {
        delay(500);
        id = read_reg(0xFF);

        if (id == 0x42)
        {
            Serial.print("MAX30009 Found id: ");
            Serial.println(id, HEX);
            delay(5000);
            break;
        }
    }
    write_reg(0x1A, 0x20); // 内部時計32.768K、微調周波数
    write_reg(0x19, 0x01); // FCLK参照が高いジッターを持つ場合にエラーが発生するのを避ける。内部RC振動周波数は安定しないように見えるため、チップが常に位相ロックを失うという問題があります。おそらくははんだ付けが長すぎてチップの電気特性を悪化させたため、とにかくオープンにすると位相ロックが大幅に減少します
    write_reg(0x20, 0xA4); // DAC/ADC OSRの設定とVrefの有効化; PLL_ENが1に設定される前にBioZリファレンスを有効にする必要があります(BIOZ_BG_EN[2](0x20)=1)。解決には6msの時間がかかる可能性があります。
    delay(200);            // 有効化後、200ms待つ
    write_reg(0x18, 0xFF); // MDIVの下位ビットの設定
    write_reg(0x17, 0x40); // MDIVの上位ビット、NDIV=512、KDIV=1、およびPLLの有効化
    PLL_enable();
    delay(200);            // 有効化後、200ms待つ
    write_reg(0x80, 0x00); // 割り込みはMAX30009のどの動作によって発生するかを制御するレジスタ
    write_reg(0x13, 0x04); // 割り込みトリガーモードの選択
    write_reg(0x12, 0x08); // 割り込みの有効化とリセット方法の設定; このレジスタはTRIGトリガーモードが立ち上がりエッジか立ち下がりエッジかを制御します
    delay(1);
    read_reg(0x00); // 割り込みが設定されると自動的に1回リセットされます。おそらくは少し待ってから任意のレジスタを1回読むだけでリセットできるようです
    delay(1);
    write_reg(0x22, 0x04); // 電流源出力か電圧源出力かHブリッジ出力かを選択し、振幅を制御
    // write_reg(0x21, 0x08); // フィルタリング、スイープ中にフィルタリングを追加すると応答速度がかなり低下することがわかりました
    write_reg(0x25, 0x52); //[7]=1外部キャパシタを使用;[3:0]=1010BIAと心電図測定では、BIOZ_AMP_RGEとBIOZ_AMP_BWを比較的高い値に設定する必要があります
    write_reg(0x28, 0x02); //[3]Qクロック位相をIに移動、[2]Iクロック位相をQに移動、F_BIOZ>54668の場合、[0]は0、[1]は1; F_BIOZ<54668の場合、F_BIOZ=BIOZ_ADC_CLK/8の場合、[0]=1、それ以外の場合は0、F_BIOZ=BIOZ_ADC_CLK/2の場合、[1]=0、それ以外は1
                           // write_reg(0x58,0x03);		//受信チャンネルにリードバイアスを1つ追加
    BIOZ_INA_CHOP_EN_ON();
    NewFrequencyCalibration(0x78, 0xff, 0xfc); // 16Hz
    // NewFrequencyCalibration(0x54, 0xab, 0xf4); // 53Hz
    FrequencyCalibrationGap();
    I_Q_close();
    IQ_PHASE_NOT_change();
    //	write_reg(0x42, 0x00);
    write_reg(0x41, 0x02); // MUXを有効にする
    write_reg(0x42, 0x03); // 外部保護トラッキングドライブ回路を有効にし、BINとBIPの入力キャパシタ負荷の補償を有効にします。
    write_reg(0x43, 0xA0); // 測定端をEL2B EL3Bに接続、励起をEL1 EL4に接続
    I_open();
    delay(2);

    // ここからは常にスイープを実行します。注意：スイープの【各ポイントで事前にキャリブレーションを行う必要があります】、上位機器に送信します
    BIOZ_INA_CHOP_EN_ON();
    Sweap(0x78, 0xff, 0xfc); // 16Hz
    // Sweap(0x54, 0xab, 0xf4); // 53z
    FrequencyCalibrationGap();
}

void setup()
{
    // シリアル通信の初期化
    // //Serial.begin(9600);
    // while (1)
    // {
    //     if (//Serial)
    //         break;
    // }
    // delay(1000);
    // Serial.println("//Serial Start");
    // SPIの初期化
    mySPI.begin();
    mySPI.setDataMode(SPI_MODE0);
    // mySPI.setClockDivider(SPI_CLOCK_DIV8);
    mySPI.setBitOrder(MSBFIRST);
    mySPI.setThreshold(256);
    // Serial.println("SPI Start");
    //  ピンの初期化
    pinMode(csPin, OUTPUT);
    digitalWrite(csPin, HIGH);
    // Serial.println("Pin Start");
    //  BLEの初期化
    if (!BLE.begin())
    {
        while (1)
        {
        }
    }
    BLE.setLocalName("Nano33BLEDevice");
    BLE.setAdvertisedService(customService);
    customService.addCharacteristic(customCharacteristic);
    BLE.addService(customService);
    BLE.advertise();
    // Serial.println("BLE Start");
    while (1)
    {
        central = BLE.central();
        if (central)
        {
            // Serial.print("Connected to central: ");
            // Serial.println(central.address());
            break;
        }
    }
}

void loop(void)
{
    // BLE通信の命令を受信した場合
    if (central.connected())
    {
        if (customCharacteristic.written())
        {
            char r[16];
            int length = customCharacteristic.valueLength();
            if (length > 15)
                length = 15;
            memcpy(r, customCharacteristic.value(), length);
            r[length] = '\0'; // NULL終端を追加

            // Serial.print("Received: ");
            // Serial.println(r);
            //  受信したデータが「START」の場合
            if (strcmp(r, "START") == 0)
            {
                // Serial.println("START!!!");
                ble_connected = true;
                MAX30009_setup(); // MAX30009のセットアップを呼び出す
            }
        }
    }
    else
    {
        // Serial.println("Not Connected");
    }
    delay(500);
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

void show_send_buff()
{
    // for (int i = 0; i < SEND_BUF_SIZE; i++)
    // {
    //     if (SendBuff[i] < 0x10)
    //     {
    //         //Serial.print("0"); // 1桁の場合は0を追加
    //     }
    //     // 2桁で表示するために、0埋めを行う
    //     //Serial.print(SendBuff[i], HEX);
    // }
    // //Serial.println();
    customCharacteristic.writeValue(SendBuff, sizeof(SendBuff));
}

void set_receive_msg()
{
    digitalWrite(csPin, LOW);
    mySPI.transfer(0x0C);
    mySPI.transfer(0x80);
    for (int i = 0; i < 9; i++)
    {
        received_msg[i] = mySPI.transfer(0x00);
    }
    digitalWrite(csPin, HIGH);
}

void reset_receive_msg()
{
    for (int i = 0; i < 9; i++)
    {
        received_msg[i] = 0;
    }
}

void FrequencyCalibrationGap(void)
{
    // 毎回特別な識別子をホストコンピュータに送信して、次に何をすべきかを識別する
    SendBuff[0] = 0xf0;
    SendBuff[1] = 0x00;
    SendBuff[2] = 0x00;
    SendBuff[3] = 0x00;
    SendBuff[4] = 0x00;
    SendBuff[5] = 0x00;
    SendBuff[6] = 0x00;
    SendBuff[7] = 0x0f;
    show_send_buff();
}

void FIFO_MARK(void)
{
    uint8_t temp = 0x00;
    temp = read_reg(0x0E);
    temp = temp | 0x20;
    write_reg(0x0E, temp);
}

void FLUSH_FIFO(void)
{
    uint8_t temp = 0x00;
    temp = read_reg(0x0E);
    temp = temp | 0x10;
    write_reg(0x0E, temp);
}

void BIOZ_STBYON(void)
{
    uint8_t tempREG;
    tempREG = read_reg(0x28);
    tempREG = tempREG | 0x10;
    write_reg(0x28, tempREG);
}

void BIOZ_STBYOFF(void)
{
    uint8_t tempREG;
    tempREG = read_reg(0x28);
    tempREG = tempREG & 0xEF;
    write_reg(0x28, tempREG);
}

void BIOZ_DRV_Standby(void)
{
    uint8_t tempREG;
    tempREG = read_reg(0x22);
    tempREG = tempREG | 0x03;
    write_reg(0x22, tempREG);
}

void BIOZ_DRV_Current_Drive(void)
{
    uint8_t tempREG;
    tempREG = read_reg(0x22);
    tempREG = tempREG & 0xFC;
    write_reg(0x22, tempREG);
}

void bypass(void)
{
    uint8_t tempREG;
    tempREG = read_reg(0x25); // 查看0x25寄存器值并让内部输出短路测量偏置
    tempREG = tempREG | 0x20;
    write_reg(0x25, tempREG);
}

void NOT_bypass(void)
{
    uint8_t tempREG;
    tempREG = read_reg(0x25); // 查看0x25寄存器值并让其不再短路
    tempREG = tempREG & 0xdf;
    write_reg(0x25, tempREG);
}

void I_Q_open(void) // IQ通道开启
{
    uint8_t tempREG;
    tempREG = read_reg(0x20);
    tempREG = tempREG | 0x03;
    write_reg(0x20, tempREG);
}

void I_Q_close(void) // IQ通道关闭
{
    uint8_t tempREG;
    tempREG = read_reg(0x20);
    tempREG = tempREG & 0xfc;
    write_reg(0x20, tempREG);
}

void Q_open(void) // IQ通道开启
{
    uint8_t tempREG;
    tempREG = read_reg(0x20);
    tempREG = tempREG | 0x02;
    write_reg(0x20, tempREG);
}

void Q_close(void) // IQ通道关闭
{
    uint8_t tempREG;
    tempREG = read_reg(0x20);
    tempREG = tempREG & 0xfd;
    write_reg(0x20, tempREG);
}

void I_open(void) // IQ通道开启
{
    uint8_t tempREG;
    tempREG = read_reg(0x20);
    tempREG = tempREG | 0x01;
    write_reg(0x20, tempREG);
}

void I_close(void) // IQ通道关闭
{
    uint8_t tempREG;
    tempREG = read_reg(0x20);
    tempREG = tempREG & 0xfe;
    write_reg(0x20, tempREG);
}

void BIOZ_INA_CHOP_EN_ON(void) // 32608 * M / (NDIV_*2) != F_CLK时这个最好启动
{
    uint8_t tempREG;
    tempREG = read_reg(0x28);
    tempREG = tempREG | 0x02;
    write_reg(0x28, tempREG);
}

void BIOZ_INA_CHOP_EN_OFF(void)
{
    uint8_t tempREG;
    tempREG = read_reg(0x28);
    tempREG = tempREG & 0xFD;
    write_reg(0x28, tempREG);
}

void BIOZ_CH_FSEL_ON(void) // 32608 * M / (NDIV_*8) == F_CLK时这个最好启动
{
    uint8_t tempREG;
    tempREG = read_reg(0x28);
    tempREG = tempREG | 0x01;
    write_reg(0x28, tempREG);
}

void BIOZ_CH_FSEL_OFF(void)
{
    uint8_t tempREG;
    tempREG = read_reg(0x28);
    tempREG = tempREG & 0xFE;
    write_reg(0x28, tempREG);
}

void I_to_Q(void) // I解调时钟改为与Q同向，Q解调时钟还是与Q同向
{
    uint8_t tempREG;
    tempREG = read_reg(0x28);
    tempREG = tempREG | 0x04;
    tempREG = tempREG & 0xF7;
    write_reg(0x28, tempREG);
}

void Q_to_I(void) // Q解调时钟改为与I同向，I解调时钟还是与I同向
{
    uint8_t tempREG;
    tempREG = read_reg(0x28);
    tempREG = tempREG | 0x08;
    tempREG = tempREG & 0xFB;
    write_reg(0x28, tempREG);
}

void IQ_PHASE_NOT_change(void) // I解调时钟保持与I同向，Q解调时钟保持与Q同向
{
    uint8_t tempREG;
    tempREG = read_reg(0x28);
    tempREG = tempREG & 0xF3;
    write_reg(0x28, tempREG);
}

void PLL_enable(void)
{
    uint8_t tempREG;
    tempREG = read_reg(0x17);
    tempREG = tempREG | 0x01;
    write_reg(0x17, tempREG);
}

void PLL_disable(void)
{
    uint8_t tempREG;
    tempREG = read_reg(0x17);
    tempREG = tempREG & 0xfe;
    write_reg(0x17, tempREG);
}

void BIOZ_OFF(void)
{
    uint8_t tempREG;
    tempREG = read_reg(0x20);
    tempREG = tempREG & 0xfB;
    write_reg(0x20, tempREG);
}

void BIOZ_ON(void)
{
    uint8_t tempREG;
    tempREG = read_reg(0x20);
    tempREG = tempREG | 0x04;
    write_reg(0x20, tempREG);
}
