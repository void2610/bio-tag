from math import log


if __name__ == "__main__":
    print(
        "このプログラムは、指定された周波数でのさまざまなパラメータをレジスタの具体的な値に変換するためのもので、これらのパラメータの制限と要件についてはMAX30009データシートを参照してください"
    )
    reg0x17 = "00000000"
    reg0x18 = "00000000"
    reg0x20 = "00000100"
    M = int(
        input(
            "PLL_CLKとしてクロックM倍周波数を使用するために必要なMの値を入力してください"
        )
    )

    MDIV = M - 1
    MDIV = bin(MDIV)[2:]
    KDIV_ = float(
        input("インピーダンス測定同期クロックのために必要なKDIVの値を入力してください")
    )
    KDIV = KDIV_
    DAC_OSR_ = float(
        input("DDS出力周波数を計算するために必要なDAC_OSRの値を入力してください")
    )
    DAC_OSR = DAC_OSR_
    NDIV_ = float(
        input("サンプリングレートを計算するために必要なNDIVの値を入力してください")
    )
    NDIV = NDIV_
    ADC_OSR_ = float(
        input("サンプリングレートを計算するために必要なADC_OSRの値を入力してください")
    )
    ADC_OSR = ADC_OSR_

    temp = 10 - len(MDIV)
    while temp:
        MDIV = "0" + MDIV
        temp = temp - 1
    print(MDIV)
    reg0x17 = MDIV[0:2]
    reg0x18 = MDIV[2:]
    if NDIV == 512:
        NDIV = "0"
    elif NDIV == 1024:
        NDIV = "1"
    print(NDIV)
    KDIV = bin(int(log(KDIV, 2)))[2:]
    temp = 4 - len(KDIV)
    while temp:
        KDIV = "0" + KDIV
        temp = temp - 1
    print(KDIV)
    reg0x17 += NDIV
    reg0x17 += KDIV
    reg0x17 += "0"
    DAC_OSR = bin(int(log(DAC_OSR / 32, 2)))[2:]
    temp = 2 - len(DAC_OSR)
    while temp:
        DAC_OSR = "0" + DAC_OSR
        temp = temp - 1
    print(DAC_OSR)
    ADC_OSR = bin(int(log(ADC_OSR / 8, 2)))[2:]
    temp = 3 - len(ADC_OSR)
    while temp:
        ADC_OSR = "0" + ADC_OSR
        temp = temp - 1
    print(ADC_OSR)
    reg0x20 = DAC_OSR
    reg0x20 += ADC_OSR
    reg0x20 += "100"
    print("クロック周波数は：", 32768 * M)
    print("出力周波数は：", 32768 * M / (DAC_OSR_ * KDIV_))
    print("サンプリング周波数は：", 32768 * M / (ADC_OSR_ * NDIV_))
    print("PLL_CLK/N：", 32768 * M / (NDIV_))
    print("BIOZ_INA_CHOP_EN と BIOZ_CH_FSEL は：")
    print(
        "1" if 32768 * M / (NDIV_ * 2) != 32768 * M / (DAC_OSR_ * KDIV_) else "0",
        "0" if 32768 * M / (NDIV_ * 8) != 32768 * M / (DAC_OSR_ * KDIV_) else "1",
    )
    print("================================")
    print("0x17=", reg0x17, "=", hex(int(reg0x17, 2)))
    print("0x18=", reg0x18, "=", hex(int(reg0x18, 2)))
    print("0x20=", reg0x20, "=", hex(int(reg0x20, 2)))


# PyCharmのヘルプはhttps://www.jetbrains.com/help/pycharm/を参照してください。
