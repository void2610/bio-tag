from numpy import linspace, pi, cos, sin, arctan, sqrt, square, mean
import pyqtgraph as pg
from pynput import keyboard as kb
import time
import threading
import sys
import asyncio
from bleak import BleakClient
import numpy as np  # 追加: numpyをインポート
import math

from PyQt5 import QtWidgets
from PyQt5.QtWidgets import QApplication


## 測定前に検査抵抗の大きさを説明する必要がある、さもなければ検査が正しく行われない
RCAL = 101000 * (1 + (4 / 512))

# 使用前に電流ピーク値を入力することを忘れないでください 単位 uA
AMPLITUDE_OF_CURRENT_PEAK = 0.452


# デバイスUUID
DEVICE_UUID = "9108929D-B8E4-0946-232F-7EE1DDD2654C"
# キャラクタリスティックUUID
CHARACTERISTIC_UUID = "2A56"
ble_input = ""

# シリアルポートオブジェクトを作成
# portName = "/dev/cu.usbserial-12BP0164"
# baudrate = 9600
# ser = serial.Serial(portName, baudrate)
# print("Connected to: " + ser.portstr)

app = QtWidgets.QApplication([])

win = pg.GraphicsLayoutWidget(show=True)  # ウィンドウを作成
p = win.addPlot(title="Realtime plot")  # ウィンドウ内にプロットのための空間を作成
curve = p.plot()  # 空の「プロット」を作成（プロットするための曲線）
curve2 = p.plot()

plotcountermark = 0

windowWidth = 200  # 曲線を表示するウィンドウの幅
Xm = linspace(0, 0, windowWidth)  # 関連する時間系列を含む配列を作成
Xm2 = linspace(0, 0, windowWidth)
ptr = -windowWidth  # 最初のx位置を設定

f1 = None
f2 = None

ifsamplingflag = False
buffer = ""
dataname = ""
countsamplingfile = 0
sigma = 1


def threading_of_update():
    global \
        curve, \
        curve2, \
        ptr, \
        Xm, \
        Xm2, \
        buffer, \
        ifsamplingflag, \
        dataname, \
        countsamplingfile, \
        data1, \
        data2, \
        my_widget, \
        send0, \
        ble_input
    TheFirstMeasurementDataFlag = False
    # ser.flushInput()
    iii = 0
    mean_I_offset = []
    mean_Q_offset = []
    mean_I_rcal_in = []
    mean_Q_rcal_in = []
    mean_I_rcal_quad = []
    mean_Q_rcal_quad = []
    I_cal_in = []
    Q_cal_in = []
    I_cal_quad = []
    Q_cal_quad = []
    I_coef = []
    Q_coef = []
    I_phase_coef = []
    Q_phase_coef = []
    I_offset = []
    Q_offset = []
    I_rcal_in = []
    Q_rcal_in = []
    I_rcal_quad = []
    Q_rcal_quad = []

    while True:
        # c = ser.inWaiting()
        c = len(ble_input)
        flag = 0
        if c >= 8:
            # line = ser.read(16)
            # namadata = str(line)[2:-1]  # 一度に8個のデータを受信（16進数で16個）
            namadata = ble_input
            ble_input = ""
            buffer += namadata  # バッファにデータを保存
            if (
                len(buffer) == 16
            ):  # バッファが空のとき、最初に16個の16進数を受信したときに初期処理を行う
                flag = buffer.find("F0")  # f0に対応する点の開始位置をマーク
                print("flag=", flag, "c=", c)
                print(buffer[flag + 1 :].find("F0"))
                if (
                    buffer[flag + 1 :].find("F0") == -1
                ):  # このグループに複数のf0がある場合、次のグループを探すためにこのグループを削除
                    buffer = buffer[flag:]
                else:
                    buffer = ""
            if (
                len(buffer) > 16
            ):  # 2グループ目から、前のグループの16-flag個のデータとこのグループのflagデータを組み合わせて、最初のグループを出力
                # 次に来るデータ形式が前回と変わった場合の異常処理
                if (
                    namadata[flag : flag + 2] != "F0" and namadata.count("F0") == 1
                ):  # もし伝送にエラーが発生した場合、flagの位置は大抵f0ではないため、こう判断するが、伝送エラーが発生した場合にこの位置がちょうどf0である可能性もあるため、判断が不完全だが、確率が低いため、暫定的に未実施
                    print(
                        "データの順序が変わりました\n変動前flag=",
                        flag,
                        "\n変動後flag=",
                        namadata.find("F0"),
                    )
                    # バッファをクリアし、f0に対応する点の開始位置を再マーク
                    buffer = ""
                    flag = namadata.find("F0")
                    buffer = (
                        buffer + namadata[flag:]
                    )  # f0の後をバッファに保存し、未完成のフレームとして、次のフレームと組み合わせて完全なフレームを作成する準備
                    continue

                elif namadata.find("F0") == -1:
                    buffer = buffer[
                        :-16
                    ]  # 毎回完全なフレームを出力した後、元のフレームを削除し、バッファの長さを制御
                    continue

                # すべてが順調であれば、データを正常に接続
                Xm[:-1] = Xm[1:]  # 時間平均のデータを1サンプル左にシフト
                Xm2[:-1] = Xm2[1:]
                # print(Xm[-1])
                data = buffer[-(16 - flag) - 16 : -(16 - flag)]

                data1 = (
                    int(data[3:8], 16)
                    if int(data[3:8], 16) < 2**19
                    else int(data[3:8], 16) - 2**20
                )
                data2 = (
                    int(data[9:14], 16)
                    if int(data[9:14], 16) < 2**19
                    else int(data[9:14], 16) - 2**20
                )

                buffer = buffer[
                    16:
                ]  # 毎回完全なフレームを出力した後、元のフレームを削除し、バッファのサイズを小さくする
                if data[2] == "0":
                    # gapシンボルが来たとき
                    if not TheFirstMeasurementDataFlag:
                        # 正式に測定が始まる最初のデータが来たとき、以前のキャリブレーションデータを使用する、iiiは何番目の周波数のキャリブレーションを示す
                        mean_I_offset.append(
                            mean(I_offset[400:])
                            / (
                                (2**19)
                                * (2 / pi)
                                * AMPLITUDE_OF_CURRENT_PEAK
                                * (10**-6)
                            )
                        )
                        mean_Q_offset.append(
                            mean(Q_offset[400:])
                            / (
                                (2**19)
                                * (2 / pi)
                                * AMPLITUDE_OF_CURRENT_PEAK
                                * (10**-6)
                            )
                        )
                        mean_I_rcal_in.append(
                            mean(I_rcal_in[400:])
                            / (
                                (2**19)
                                * (2 / pi)
                                * AMPLITUDE_OF_CURRENT_PEAK
                                * (10**-6)
                            )
                        )
                        mean_Q_rcal_in.append(
                            mean(Q_rcal_in[400:])
                            / (
                                (2**19)
                                * (2 / pi)
                                * AMPLITUDE_OF_CURRENT_PEAK
                                * (10**-6)
                            )
                        )
                        mean_I_rcal_quad.append(
                            mean(I_rcal_quad[400:])
                            / (
                                (2**19)
                                * (2 / pi)
                                * AMPLITUDE_OF_CURRENT_PEAK
                                * (10**-6)
                            )
                        )
                        mean_Q_rcal_quad.append(
                            mean(Q_rcal_quad[400:])
                            / (
                                (2**19)
                                * (2 / pi)
                                * AMPLITUDE_OF_CURRENT_PEAK
                                * (10**-6)
                            )
                        )
                        I_cal_in.append(mean_I_rcal_in[iii] - mean_I_offset[iii])
                        Q_cal_in.append(mean_Q_rcal_in[iii] - mean_Q_offset[iii])
                        I_cal_quad.append(mean_I_rcal_quad[iii] - mean_I_offset[iii])
                        Q_cal_quad.append(mean_Q_rcal_quad[iii] - mean_Q_offset[iii])
                        I_coef.append(
                            ((I_cal_in[iii] ** 2 + I_cal_quad[iii] ** 2) ** (1 / 2))
                            / RCAL
                        )
                        Q_coef.append(
                            ((Q_cal_in[iii] ** 2 + Q_cal_quad[iii] ** 2) ** (1 / 2))
                            / RCAL
                        )
                        I_phase_coef.append(
                            arctan(I_cal_quad[iii] / I_cal_in[iii]) * 180 / pi
                        )
                        Q_phase_coef.append(
                            arctan(-Q_cal_quad[iii] / -Q_cal_in[iii]) * 180 / pi
                        )
                        I_offset = []
                        Q_offset = []
                        I_rcal_in = []
                        Q_rcal_in = []
                        I_rcal_quad = []
                        Q_rcal_quad = []
                        iii = iii + 1
                    elif TheFirstMeasurementDataFlag:
                        iii = iii + 1
                        if iii == 1:
                            iii = 0

                if data[2] == "1":
                    if not TheFirstMeasurementDataFlag:
                        TheFirstMeasurementDataFlag = True
                        iii = 0
                    I_load_offset = (
                        data1
                        / ((2**19) * (2 / pi) * AMPLITUDE_OF_CURRENT_PEAK * (10**-6))
                        - mean_I_offset[iii]
                    )
                    Q_load_offset = (
                        data2
                        / ((2**19) * (2 / pi) * AMPLITUDE_OF_CURRENT_PEAK * (10**-6))
                        - mean_Q_offset[iii]
                    )
                    I_cal_real = (I_load_offset / I_coef[iii]) * cos(
                        I_phase_coef[iii] * pi / 180
                    )
                    Q_cal_real = (Q_load_offset / Q_coef[iii]) * sin(
                        Q_phase_coef[iii] * pi / 180
                    )
                    I_cal_imag = (I_load_offset / I_coef[iii]) * sin(
                        I_phase_coef[iii] * pi / 180
                    )
                    Q_cal_imag = (Q_load_offset / Q_coef[iii]) * cos(
                        Q_phase_coef[iii] * pi / 180
                    )
                    Load_real = I_cal_real - Q_cal_real
                    Load_imag = I_cal_imag + Q_cal_imag
                    Load_mag = sqrt(square(Load_real) + square(Load_imag))
                    Load_angle = arctan(Load_imag / Load_real) * (180 / pi)
                    if ifsamplingflag:
                        f1.write(str(Load_mag) + ",")
                        f2.write(str(Load_angle) + ",")

                    Xm[-1] = Load_real  # 瞬時値を含むベクトル
                    Xm2[-1] = Load_imag
                    ptr += 1  # 曲線を表示するためのx位置を更新

                # Zeroシンボルが来たときの特別なマークと特別な操作
                if data[2] == "e":
                    if ifsamplingflag:
                        f1.write(str("Z") + ",")
                        f2.write(str("Z") + ",")

                # データキャリブレーション時に使用される識別子と操作
                elif data[2] == "3":
                    I_offset.append(data1)
                    Q_offset.append(data2)

                    Xm[-1] = data2  # 瞬時値を含むベクトル
                    Xm2[-1] = data1
                    ptr += 1  # 曲線を表示するためのx位置を更新
                elif data[2] == "5":
                    I_rcal_in.append(data1)
                    Q_rcal_in.append(data2)

                    Xm[-1] = data2  # 瞬時値を含むベクトル
                    Xm2[-1] = data1
                    ptr += 1  # 曲線を表示するためのx位置を更新
                elif data[2] == "7":
                    I_rcal_quad.append(data1)
                    Q_rcal_quad.append(data2)

                    Xm[-1] = data2  # 瞬時値を含むベクトル
                    Xm2[-1] = data1
                    ptr += 1
        else:
            time.sleep(0.001)


def gaussian_filter(data, sigma):
    kernel_size = int(6 * sigma + 1)  # カーネルサイズを計算
    kernel_size = kernel_size + 1 if kernel_size % 2 == 0 else kernel_size  # 奇数に調整
    kernel = np.exp(
        -0.5 * (np.arange(kernel_size) - kernel_size // 2) ** 2 / sigma**2
    )  # ガウスカーネルを生成
    kernel /= kernel.sum()  # 正規化

    filtered_data = np.convolve(data, kernel, mode="same")  # 畳み込みを実行
    return filtered_data  # フィルタリングされたデータを返す


def moving_average_filter(data, window_size):
    filtered_data = np.convolve(
        data, np.ones(window_size) / window_size, mode="same"
    )  # 移動平均を計算
    return filtered_data  # フィルタリングされたデータを返す


def threading_of_plot():
    global curve, curve2, ptr, Xm, Xm2, plotcountermark, sigma

    Xm_smoothed = moving_average_filter(
        Xm, sigma
    )  # 変更: 自作のガウシアンフィルタを適用
    Xm2_smoothed = moving_average_filter(
        Xm2, sigma
    )  # 変更: 自作のガウシアンフィルタを適用

    if plotcountermark == 0:
        curve.setData(Xm_smoothed, pen="b")  # 更新: 平滑化されたデータで曲線を設定
        curve.setPos(ptr, 0)  # グラフのx位置を0に設定
        curve2.setData(Xm2_smoothed, pen="r")  # 更新: 平滑化されたデータで曲線を設定
        curve2.setPos(ptr, 0)  # グラフのx位置を0に設定
        QtWidgets.QApplication.processEvents()  # プロットを処理する必要があります
        curve.setData(Xm_smoothed, pen="b")
        curve.setPos(ptr, 0)
        curve2.setData(Xm2_smoothed, pen="r")
        curve2.setPos(ptr, 0)
        QtWidgets.QApplication.processEvents()

    if plotcountermark == 1:
        curve.setData(Xm_smoothed, pen="b")
        curve.setPos(ptr, 0)
        curve2.clear()
        QtWidgets.QApplication.processEvents()

    if plotcountermark == 2:
        curve2.setData(Xm2_smoothed, pen="r")
        curve.clear()
        curve2.setPos(ptr, 0)
        QtWidgets.QApplication.processEvents()


def on_press(key):
    global \
        ifsamplingflag, \
        countsamplingfile, \
        plotcountermark, \
        f1, \
        f2, \
        dataname, \
        windowWidth, \
        Xm, \
        Xm2, \
        ptr, \
        sigma

    try:
        if key.char == "c":
            plotcountermark = plotcountermark + 1
            if plotcountermark == 3:
                plotcountermark = 0
        elif key.char == "s":
            countsamplingfile += 1
            dataname = "pysavedsampling" + str(countsamplingfile) + "data"
            f1 = open(dataname + "1.txt", "w")
            f2 = open(dataname + "2.txt", "w")
            print("The start of sampling")
            ifsamplingflag = True
        elif key.char == "e":
            print("The end of sampling")
            f1.close()
            f2.close()
            ifsamplingflag = False
        elif key.char == "w":
            windowWidth += 100
            if windowWidth > 2000:
                windowWidth = 100
            tmp = Xm[-1]
            tmp2 = Xm2[-1]
            Xm = linspace(tmp, tmp, windowWidth)  # 関連する時間系列を含む配列を作成
            Xm2 = linspace(tmp2, tmp2, windowWidth)
            ptr = -windowWidth  # 最初のx位置を設定
        elif key.char == "g":
            sigma += 1
            if sigma > 500:
                sigma = 1
            print("sigma=", sigma)

    except AttributeError:
        # 特殊キー（例：Shift、Ctrlなど）の場合は無視
        pass


def start_key_listener():
    with kb.Listener(on_press=on_press) as listener:
        listener.join()


# データ受信時のコールバック関数
async def notification_handler(sender, data):
    global ble_input
    if data.hex().upper() != "5354415254":
        # print(data.hex().upper())  # 16進数として出力
        ble_input = data.hex().upper()


# BLE通信を行う関数
async def ble_run():
    while True:  # 無限ループで接続を試みる
        try:
            async with BleakClient(DEVICE_UUID) as client:
                print(f"Connected: {client.is_connected}")

                # キャラクタリスティックの通知を有効化
                await client.start_notify(CHARACTERISTIC_UUID, notification_handler)

                while True:
                    await asyncio.sleep(1)
                    # STARTと送信
                    await client.write_gatt_char(
                        CHARACTERISTIC_UUID, "START".encode("utf-8")
                    )

                await asyncio.sleep(99999)
                await client.stop_notify(CHARACTERISTIC_UUID)

        except Exception as e:
            print(f"Connection failed: {e}")  # エラーを表示
            print("Reconnecting...")
            await asyncio.sleep(2)  # 再接続までの待機時間


### MAIN PROGRAM #####
if __name__ == "__main__":
    key_listener_thread = threading.Thread(target=start_key_listener)
    key_listener_thread.daemon = True
    key_listener_thread.start()

    loop = asyncio.get_event_loop()
    loop.create_task(ble_run())  # ble_runをタスクとして作成

    t1 = threading.Thread(target=threading_of_update)
    t1.daemon = True  # setDaemon()の代わりにdaemon属性を使用
    t1.start()

    app = QApplication(sys.argv)

    timer = pg.QtCore.QTimer()
    timer.timeout.connect(threading_of_plot)
    timer.start(100)

    # イベントループをPyQt5のウィンドウと同時に実行
    async def run_app():
        while True:
            await asyncio.sleep(0.1)  # asyncioのイベントループを少し待機
            app.processEvents()  # PyQt5のイベントを処理

    loop.run_until_complete(run_app())  # run_appを実行
