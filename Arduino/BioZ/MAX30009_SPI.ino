#include <Arduino.h>
#include <SPI.h>
#include "softSPI.h"

#define SEND_BUF_SIZE 8

SoftSPI mySPI(11, A0, 13);
int csPin = 10;
int buttonPin = 5;
int powerPin = 6;
bool isSetuped = false;

uint8_t SendBuff[SEND_BUF_SIZE] = {0xf0, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0x0f}; // 发送数据缓冲区
uint8_t id = 0x00, i = 255, j = 255, flag = 0, a[10], counter = 0, counter2 = 0, tempREG;
uint16_t read_pointer, write_pointer;
uint32_t timecounter;
char received_msg[9] = {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
char reset_msg[9] = {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
char Sendingstr[3];
int data = 0;

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

void NewFrequencyCalibration(uint8_t reg0x17, uint8_t reg0x18, uint8_t reg0x20)
{
    uint8_t tempREG1;
    uint8_t tempREG2;
    write_reg(0x20, reg0x20); // DAC/ADC OSR设置并使能Vref; 在PLL_EN设置为1之前，BioZ参考必须被启用(BIOZ_BG_EN[2](0x20)=1)，并且可能需要6ms的时间来解决。
    write_reg(0x18, reg0x18); // MDIV低位配置
    write_reg(0x17, reg0x17); // MDIV高位、NDIV=512、KDIV=1，以及PLL使能
    PLL_enable();
    tempREG1 = read_reg(0x22); // 设定电流最小并记录保存的电流值
    tempREG2 = tempREG1 & 0xC3;
    write_reg(0x22, tempREG2);
    bypass(); // 查看0x25寄存器值并让其短路测量偏置
    I_Q_open();
    delay(2);
    FLUSH_FIFO(); // 读之前清空FIFO
    // 检测MAX30009是否发送新的数据，并发送65535次短路数据给上位机
    for (timecounter = 0; timecounter < 100;)
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
    I_Q_close();               // 关闭IQ通道，其他不变
    write_reg(0x22, tempREG1); // 输出电流调整回去
    NOT_bypass();              // 查看0x25寄存器值并让其不再短路
    write_reg(0x41, 0x00);     // 开启芯片内部电阻校验，由于0x44内的值为0x11，真实电阻为R*(1+17/512)，详见手册。 （第二块芯片，即没有丝印的那块值为0x13，真实电阻是R*(1+19/512)）
    write_reg(0x42, 0x60);
    Q_to_I();
    I_Q_open();
    delay(2);
    FLUSH_FIFO(); // 读之前清空FIFO
    // 发送65535次Q时钟相位移到I的矫正电阻的数据
    for (timecounter = 0; timecounter < 100;)
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
    FLUSH_FIFO(); // 读之前清空FIFO

    // 发送65535次Q时钟相位移到I的矫正电阻的数据
    for (timecounter = 0; timecounter < 100;)
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
    LED0 = !LED0; // 发送完成后点个灯，至此，初校验完成，接下来保持1Khz频率输出一次
}

uint8_t Sweap(uint8_t reg0x17, uint8_t reg0x18, uint8_t reg0x20)
{
    uint16_t read_pointer, write_pointer, key;
    uint32_t timecounter;

    // BIOZ_OFF();
    BIOZ_DRV_Standby();
    write_reg(0x20, reg0x20); // DAC/ADC OSR设置并使能Vref; 在PLL_EN设置为1之前，BioZ参考必须被启用(BIOZ_BG_EN[2](0x20)=1)，并且可能需要6ms的时间来解决。
    write_reg(0x18, reg0x18); // MDIV低位配置
    write_reg(0x17, reg0x17); // MDIV高位、NDIV=512、KDIV=1，以及PLL使能
    PLL_enable();
    // BIOZ_ON();
    BIOZ_DRV_Current_Drive();
    // FLUSH_FIFO();
    I_Q_open();
    FLUSH_FIFO(); // 读之前清空FIFO

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
        // TODO: キー入力
        // key = KEY_Scan(0); // 按键启动，若返回SPI值为0x42则找到芯片
        // if (key == KEY0_PRES)
        // {
        //     delay(500);
        //     break;
        // }
    }
}

void setup()
{
    Serial.begin(9600);
    mySPI.begin();
    mySPI.setDataMode(SPI_MODE0);
    mySPI.setClockDivider(SPI_CLOCK_DIV8);
    mySPI.setBitOrder(MSBFIRST);
    mySPI.setThreshold(256);
    pinMode(csPin, OUTPUT);
    digitalWrite(csPin, HIGH);
    pinMode(buttonPin, INPUT_PULLUP);

    uint8_t id = 0x00, i = 255, j = 255, flag = 0, a[10], counter = 0, counter2 = 0, tempREG;
    uint16_t read_pointer, write_pointer;
    uint32_t timecounter;
    int data = 0;
    uint8_t key;

    MAX30009_setup();
}

void loop(void)
{
}

void MAX30009_setup()
{
    digitalWrite(csPin, HIGH);

    // MAX3000の初期化が終わるまで待つ　なくてもいい？　
    // while (1)
    // {
    //     key = KEY_Scan(0); // 按键启动，若返回SPI值为0x42则找到芯片
    //     if (key == KEY0_PRES)
    //     {
    //         delay(500);
    //         id = read_reg(0xFF);
    //         if (id == 0x42)
    //         {
    //             LED1 = !LED1;
    //             delay(5000);
    //             break;
    //         }
    //     }
    //     else
    //         delay(10);
    // }
    write_reg(0x1A, 0x20); // 内部时钟32.768K，微调频率
    write_reg(0x19, 0x01); // 避免在FCLK参考有高抖动时出现错误的PHASE_UNLOCK中断。使用内部RC震荡频率似乎不太稳定的样子，使得芯片一直说相位失锁，也有可能是焊得太久把芯片电气属性弄差了，总之打开可以大幅减少相位失锁
    write_reg(0x20, 0xA4); // DAC/ADC OSR设置并使能Vref; 在PLL_EN设置为1之前，BioZ参考必须被启用(BIOZ_BG_EN[2](0x20)=1)，并且可能需要6ms的时间来解决。
    delay(200);            // 使能后等待200ms
    write_reg(0x18, 0xFF); // MDIV低位配置
    write_reg(0x17, 0x40); // MDIV高位、NDIV=512、KDIV=1，以及PLL使能
    PLL_enable();
    delay(200);            // 使能后等待200ms
    write_reg(0x80, 0x00); // 中断由MAX30009的什么行为发出，由这个寄存器控制
    write_reg(0x13, 0x04); // 中断触发方式选择
    write_reg(0x12, 0x08); // 中断开启，并设置其清零方式；这个寄存器同样控制TRIG触发方式是上升沿还是下降沿
    delay(1);
    read_reg(0x00); // 中断设置完后会自动置位一次，似乎等一小段时间后读一次任意寄存器即可消除
    delay(1);
    write_reg(0x22, 0x04); // 选择是电流源输出还是电压源输出还是H桥输出，并控制幅值
    //	write_reg(0x21,0x08);			//滤波，发现在扫频时，加入滤波会比较影响响应速度
    write_reg(0x25, 0xC4); //[7]=1使用外部电容；[3:0]=1010对于BIA和心阻抗测量中，BIOZ_AMP_RGE和BIOZ_AMP_BW需要被设置为较高的值
    write_reg(0x28, 0x02); //[3]Q时钟相位移到I，[2]I时钟相位移到Q，当F_BIOZ>54668时，[0]为0，[1]为1；当F_BIOZ<54668时，如果F_BIOZ=BIOZ_ADC_CLK/8,[0]=1，否则为0，如果F_BIOZ=BIOZ_ADC_CLK/2，[1]=0否则为1
                           // write_reg(0x58,0x03);		//让接受通道中存在一个lead bias
    BIOZ_INA_CHOP_EN_ON();
    NewFrequencyCalibration(0x78, 0xff, 0xfc); // 16HZ
    FrequencyCalibrationGap();
    I_Q_close();
    IQ_PHASE_NOT_change();
    //	write_reg(0x42, 0x00);
    write_reg(0x41, 0x02); // 启用MUX
    write_reg(0x42, 0x03); // 使能外部保护跟踪驱动电路;使能补偿BIN和BIP上的输入电容负载的电路。
    write_reg(0x43, 0xA0); // 测量端连接到EL2B EL3B，激励连接到EL1 EL4
    I_Q_open();
    delay(2);
    while (1)
    { // 从这一句开始一直开始循环扫频；千万注意，扫频的【每一个点都必须提前进行校准】并发送给上位机
        BIOZ_INA_CHOP_EN_ON();
        Sweap(0x78, 0xff, 0xfc); // 16Hz
        FrequencyCalibrationGap();
    }
    while (1)
    {
    }
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
    for (int i = 0; i < SEND_BUF_SIZE; i++)
    {
        if (SendBuff[i] < 0x10)
        {
            Serial.print("0"); // 1桁の場合は0を追加
        }
        // 2桁で表示するために、0埋めを行う
        Serial.print(SendBuff[i], HEX);
    }
    // Serial.println();
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
