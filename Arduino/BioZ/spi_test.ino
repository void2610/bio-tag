#include <Arduino.h>
#include <SPI.h>
#include <softSPI.h>

#define SEND_BUF_SIZE 8

SoftSPI mySPI(11, A0, 13);
int csPin = 10;

u8 SendBuff[SEND_BUF_SIZE] = {0xf0, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x0f};
u8 id = 0x00, i = 255, j = 255, flag = 0, a[10], counter = 0, counter2 = 0, tempREG;
u16 read_pointer, write_pointer;
u32 timecounter;
char received_msg[9] = {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
char reset_msg[9] = {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
char Sendingstr[3];
int data = 0;

void PLL_enable(void) // PLL(位相固定ループ)を有効にする
{
    u8 tempREG;
    tempREG = read_reg(0x17);
    tempREG = tempREG | 0x01;
    write_reg(0x17, tempREG);
}
void PLL_disable(void)
{
    u8 tempREG;
    tempREG = read_reg(0x17);
    tempREG = tempREG & 0xfe;
    write_reg(0x17, tempREG);
}
void BIOZ_OFF(void)
{
    u8 tempREG;
    tempREG = read_reg(0x20);
    tempREG = tempREG & 0xfB;
    write_reg(0x20, tempREG);
}
void BIOZ_ON(void)
{
    u8 tempREG;
    tempREG = read_reg(0x20);
    tempREG = tempREG | 0x04;
    write_reg(0x20, tempREG);
}
void bypass(void)
{
    u8 tempREG;
    tempREG = read_reg(0x25); // 0x25レジスタの値を確認し、内部出力を短絡測定偏置にする
    tempREG = tempREG | 0x20;
    write_reg(0x25, tempREG);
}
void NOT_bypass(void)
{
    u8 tempREG;
    tempREG = read_reg(0x25); // 0x25レジスタの値を確認し、短絡を解除する
    tempREG = tempREG & 0xdf;
    write_reg(0x25, tempREG);
}
void I_Q_open(void) // IQチャネルを開く
{
    u8 tempREG;
    tempREG = read_reg(0x20);
    tempREG = tempREG | 0x03;
    write_reg(0x20, tempREG);
}
void I_Q_close(void) // IQチャネルを閉じる
{
    u8 tempREG;
    tempREG = read_reg(0x20);
    tempREG = tempREG & 0xfc;
    write_reg(0x20, tempREG);
}
void FIFO_MARK(void)
{
    u8 temp = 0x00;
    temp = read_reg(0x0E);
    temp = temp | 0x20;
    write_reg(0x0E, temp);
}
void FLUSH_FIFO(void)
{
    u8 temp = 0x00;
    temp = read_reg(0x0E);
    temp = temp | 0x10;
    write_reg(0x0E, temp);
}
void BIOZ_STBYON(void)
{
    u8 tempREG;
    tempREG = read_reg(0x28);
    tempREG = tempREG | 0x10;
    write_reg(0x28, tempREG);
}
void BIOZ_STBYOFF(void)
{
    u8 tempREG;
    tempREG = read_reg(0x28);
    tempREG = tempREG & 0xEF;
    write_reg(0x28, tempREG);
}
void BIOZ_INA_CHOP_EN_ON(void) // 32608 * M / (NDIV_*2) != F_CLKの時にこれを起動するのが最適
{
    u8 tempREG;
    tempREG = read_reg(0x28);
    tempREG = tempREG | 0x02;
    write_reg(0x28, tempREG);
}
void BIOZ_INA_CHOP_EN_OFF(void)
{
    u8 tempREG;
    tempREG = read_reg(0x28);
    tempREG = tempREG & 0xFD;
    write_reg(0x28, tempREG);
}
void BIOZ_CH_FSEL_ON(void) // 32608 * M / (NDIV_*8) == F_CLKの時にこれを起動するのが最適
{
    u8 tempREG;
    tempREG = read_reg(0x28);
    tempREG = tempREG | 0x01;
    write_reg(0x28, tempREG);
}
void BIOZ_CH_FSEL_OFF(void)
{
    u8 tempREG;
    tempREG = read_reg(0x28);
    tempREG = tempREG & 0xFE;
    write_reg(0x28, tempREG);
}
void BIOZ_DRV_Current_Drive(void)
{
    u8 tempREG;
    tempREG = read_reg(0x22);
    tempREG = tempREG & 0xFC;
    write_reg(0x22, tempREG);
}
void BIOZ_DRV_Standby(void)
{
    u8 tempREG;
    tempREG = read_reg(0x22);
    tempREG = tempREG | 0x03;
    write_reg(0x22, tempREG);
}
void I_to_Q(void) // I解調時鐘をQと同向に変更し、Q解調時鐘は引き続きQと同向
{
    u8 tempREG;
    tempREG = read_reg(0x28);
    tempREG = tempREG | 0x04;
    tempREG = tempREG & 0xF7;
    write_reg(0x28, tempREG);
}
void Q_to_I(void) // Q解調時鐘をIと同向に変更し、I解調時鐘は引き続きIと同向
{
    u8 tempREG;
    tempREG = read_reg(0x28);
    tempREG = tempREG | 0x08;
    tempREG = tempREG & 0xFB;
    write_reg(0x28, tempREG);
}
void IQ_PHASE_NOT_change(void) // I解调时钟保持与I同向，Q解调时钟保持与Q同向
{
    u8 tempREG;
    tempREG = read_reg(0x28);
    tempREG = tempREG & 0xF3;
    write_reg(0x28, tempREG);
}

void FrequencyCalibrationGap(void)
{
    // 毎回特別な識別子をホストコンピュータに送信して、次に何をすべきかを識別する
    // while (1)
    //     if (DMA_GetFlagStatus(DMA2_Stream7, DMA_FLAG_TCIF7) != RESET) // DMA2_Stream7の転送完了を待つ
    //     {
    //         DMA_ClearFlag(DMA2_Stream7, DMA_FLAG_TCIF7); // DMA2_Stream7の転送完了フラグをクリア
    //         SendBuff[0] = 0xf0;
    //         SendBuff[1] = 0x00;
    //         SendBuff[2] = 0x00;
    //         SendBuff[3] = 0x00;
    //         SendBuff[4] = 0x00;
    //         SendBuff[5] = 0x00;
    //         SendBuff[6] = 0x00;
    //         SendBuff[7] = 0x0f;
    //         MYDMA_Enable(DMA2_Stream7, SEND_BUF_SIZE); // 一度DMA転送を開始する！
    //         break;
    //     }
    Serial.println("FrequencyCalibrationGap");
}

void NewFrequencyCalibration(u8 reg0x17, u8 reg0x18, u8 reg0x20)
{
    // ここから初期校正プロセスを開始する

    write_reg(0x20, reg0x20); // DAC/ADC OSR設定とVrefの有効化; PLL_ENを1に設定する前に、BioZ参照を有効にする必要がある(BIOZ_BG_EN[2](0x20)=1)、解決には6msの時間がかかる可能性がある。
    write_reg(0x18, reg0x18); // MDIV低位設定
    write_reg(0x17, reg0x17); // MDIV高位、NDIV=512、KDIV=1、およびPLLを有効化
    PLL_enable();

    bypass(); // 0x25レジスタの値を確認し、短絡測定オフセットを取得する

    I_Q_open();
    delay(2);
    FLUSH_FIFO(); // 読み取る前にFIFOをクリア

    // MAX30009が新しいデータを送信しているかを確認し、65535回の短絡データをホストコンピュータに送信する
    for (timecounter = 0; timecounter < 50;)
    {
        read_pointer = read_reg(0x09);
        write_pointer = read_reg(0x08);
        if (write_pointer > read_pointer || read_pointer - write_pointer >= 0x6f)
        {
            timecounter++;
            set_receive_msg();

            SendBuff[0] = 0xf0;
            tempREG = received_msg[0];
            // 元のMAX30009データは0x1ooooo2oooooで、0x3ooooo4oooooに変更して上位機が校正の進行状況を識別しやすくするためのものです。以降も同様です。
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
            show_send_buff();
            reset_receive_msg();
        }
    }

    I_Q_close();  // IQチャネルを閉じる、他はそのまま
    NOT_bypass(); // 0x25レジスタの値を確認し、短絡を解除する

    write_reg(0x41, 0x60); // チップ内部抵抗校正を有効化、0x44内の値が0x11の場合、実際の抵抗はR*(1+17/512)、詳細はマニュアルを参照。 （2つ目のチップ、つまりシルク印刷のないチップの値は0x13、実際の抵抗はR*(1+19/512)）
    Q_to_I();
    I_Q_open();
    delay(2);
    FLUSH_FIFO(); // 読み取る前にFIFOをクリア

    // 65535回のQクロック位相をIの補正抵抗データとして送信
    for (timecounter = 0; timecounter < 50;)
    {
        read_pointer = read_reg(0x09);
        write_pointer = read_reg(0x08);
        if (write_pointer > read_pointer || read_pointer - write_pointer >= 0x6f)
        {
            timecounter++;
            set_receive_msg();
            SendBuff[0] = 0xf0;
            tempREG = received_msg[0];
            // 原本MAX30009数据为0x1ooooo2ooooo，改为0x3ooooo4ooooo以方便上位机识别校验进行到哪一步，后面同理
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
    FLUSH_FIFO(); // 読み取る前にFIFOをクリア

    // 65535回のQクロック位相をIの補正抵抗データとして送信
    for (timecounter = 0; timecounter < 50;)
    {
        read_pointer = read_reg(0x09);
        write_pointer = read_reg(0x08);
        if (write_pointer > read_pointer || read_pointer - write_pointer >= 0x6f)
        {
            timecounter++;
            set_receive_msg();
            SendBuff[0] = 0xf0;
            tempREG = received_msg[0];
            // 原本MAX30009数据为0x1ooooo2ooooo，改为0x3ooooo4ooooo以方便上位机识别校验进行到哪一步，后面同理
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

u8 Sweap(u8 reg0x17, u8 reg0x18, u8 reg0x20)
{
    u16 read_pointer, write_pointer;
    u32 timecounter;
    char received_msg[9] = {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
    char reset_msg[9] = {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};

    // BIOZ_OFF();
    BIOZ_DRV_Standby();
    write_reg(0x20, reg0x20); // DAC/ADC OSR設定とVrefの有効化; PLL_ENを1に設定する前に、BioZ参照を有効にする必要がある(BIOZ_BG_EN[2](0x20)=1)、解決には6msの時間がかかる可能性がある。
    write_reg(0x18, reg0x18); // MDIV低位設定
    write_reg(0x17, reg0x17); // MDIV高位、NDIV=512、KDIV=1、およびPLLを有効化
    PLL_enable();
    // BIOZ_ON();
    BIOZ_DRV_Current_Drive();
    // FLUSH_FIFO();
    I_Q_open();
    FLUSH_FIFO(); // 読み取る前にFIFOをクリア

    while (1)
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
}

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

    for (i = 0; i < 9; i++)
        a[i] = 1;

    write_reg(0x1A, 0x20); // 内部クロック32.768K、微調整周波数
    write_reg(0x19, 0x01); // FCLK参照で高い揺らぎが発生することを避けるためにPHASE_UNLOCK中断のエラーを防ぐ。内部RC発振周波数はあまり安定していないようで、チップが常に位相ロック解除を報告する可能性があり、長時間はんだ付けされたためにチップの電気特性が悪化した可能性がある。とにかく、これを有効にすることで位相ロック解除を大幅に減少させることができる。

    write_reg(0x20, 0xA4); // DAC/ADC OSR設定とVrefの有効化; PLL_ENを1に設定する前に、BioZ参照を有効にする必要がある(BIOZ_BG_EN[2](0x20)=1)、解決には6msの時間がかかる可能性がある。

    delay(200); // 有効化後200ms待機

    write_reg(0x18, 0xFF); // MDIV低位設定
    write_reg(0x17, 0x40); // MDIV高位、NDIV=512、KDIV=1、およびPLLを有効化
    PLL_enable();
    delay(200); // 有効化後200ms待機

    write_reg(0x80, 0x00); // MAX30009の動作によって発生する中断をこのレジスタで制御
    write_reg(0x13, 0x04); // 中断トリガ方式の選択
    write_reg(0x12, 0x08); // 中断を有効化し、そのクリア方式を設定; このレジスタはTRIGトリガ方式が立ち上がりか立ち下がりかを制御
    delay(1);
    read_reg(0x00); // 中断設定後に自動的にフラグが立つため、少し待ってから任意のレジスタを読み取ることで消去できる
    delay(1);

    write_reg(0x22, 0x28); // 電流源出力、電圧源出力、またはHブリッジ出力を選択し、振幅を制御
    // write_reg(0x21,0x18); // フィルタリング、スイープ時にフィルタを追加すると応答速度に影響を与えることがある

    write_reg(0x25, 0xCF); //[7]=1は外部コンデンサを使用；[3:0]=1010はBIAと心抵抗測定において、BIOZ_AMP_RGEとBIOZ_AMP_BWを高い値に設定する必要がある

    write_reg(0x28, 0x02); //[3]Qクロック位相をIに移動，[2]Iクロック位相をQに移動，F_BIOZ>54668のとき，[0]は0，[1]は1；F_BIOZ<54668のとき，F_BIOZ=BIOZ_ADC_CLK/8ならば，[0]=1，そうでなければ0，F_BIOZ=BIOZ_ADC_CLK/2ならば，[1]=0，そうでなければ1
    // write_reg(0x58,0x03); //受信チャネルにリードバイアスを存在させる

    NewFrequencyCalibration(0x4c, 0xf3, 0xFC);
    FrequencyCalibrationGap();
    NewFrequencyCalibration(0x4A, 0xF3, 0xF4);
    FrequencyCalibrationGap();
    BIOZ_CH_FSEL_ON();
    NewFrequencyCalibration(0x48, 0xF3, 0xF4);
    FrequencyCalibrationGap();
    BIOZ_CH_FSEL_OFF();
    NewFrequencyCalibration(0x46, 0xF3, 0xEC);
    FrequencyCalibrationGap();
    BIOZ_INA_CHOP_EN_OFF();
    NewFrequencyCalibration(0x44, 0xF3, 0xEC);
    FrequencyCalibrationGap();
    BIOZ_INA_CHOP_EN_ON();
    NewFrequencyCalibration(0x42, 0xF3, 0xEC);
    FrequencyCalibrationGap();
    NewFrequencyCalibration(0x80, 0x0A, 0xE4);
    FrequencyCalibrationGap();
    NewFrequencyCalibration(0x40, 0xFF, 0xA4);
    FrequencyCalibrationGap();
    NewFrequencyCalibration(0x40, 0xE7, 0x64);
    FrequencyCalibrationGap();
    NewFrequencyCalibration(0x80, 0x37, 0x24);
    FrequencyCalibrationGap();
    NewFrequencyCalibration(0xE0, 0x15, 0x24);
    FrequencyCalibrationGap();

    I_Q_close();
    IQ_PHASE_NOT_change();

    write_reg(0x41, 0x02); // MUXを有効化
    //	write_reg(0x42,0x03);			// 外部保護トラッキングドライバ回路を有効化; BINとBIPの入力コンデンサ負荷を補償する回路を有効化。
    write_reg(0x43, 0xA0); // 測定端子をEL2B EL3Bに接続し、励起をEL1 EL4に接続
    I_Q_open();
    delay(2);

    while (1)
    {                            // この文からループスイープを開始; スイープの各ポイントは事前にキャリブレーションを行い、ホストコンピュータに送信する必要があります
        Sweap(0x4C, 0xF3, 0xFC); // 1KHZスイープの開始点
        FrequencyCalibrationGap();
        Sweap(0x4A, 0xF3, 0xF4); // 2KHZ
        FrequencyCalibrationGap();
        BIOZ_CH_FSEL_ON();
        Sweap(0x48, 0xF3, 0xF4); // 4K
        FrequencyCalibrationGap();
        BIOZ_CH_FSEL_OFF();
        Sweap(0x46, 0xF3, 0xEC); // 8K
        FrequencyCalibrationGap();
        BIOZ_INA_CHOP_EN_OFF();
        Sweap(0x44, 0xF3, 0xEC); // 16K
        FrequencyCalibrationGap();
        BIOZ_INA_CHOP_EN_ON();
        Sweap(0x42, 0xF3, 0xEC); // 30.976K
        FrequencyCalibrationGap();
        Sweap(0x80, 0x0A, 0xE4); // 66.944K
        FrequencyCalibrationGap();
        Sweap(0x40, 0xFF, 0xA4); // 131.072K
        FrequencyCalibrationGap();
        Sweap(0x40, 0xE7, 0x64); // 249.856K
        FrequencyCalibrationGap();
        Sweap(0x80, 0x37, 0x24); // 581.632K
        FrequencyCalibrationGap();
        Sweap(0xE0, 0x15, 0x24); // 808.960K
        FrequencyCalibrationGap();
    }
}

void loop()
{
    show_reg(0x1A);
    delay(1000);
}

void show_reg(byte regAddress)
{
    byte data;
    data = read_reg(regAddress);
    Serial.print("Register ");
    Serial.print(regAddress, HEX);
    Serial.print(" : ");
    Serial.println(data, HEX);
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
    Serial.print("SendBuff: ");
    for (int i = 0; i < SEND_BUF_SIZE; i++)
    {
        Serial.print(SendBuff[i], HEX);
        Serial.print(" ");
    }
    Serial.println();
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
