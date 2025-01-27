from numpy import linspace, pi, cos, sin, arctan, mean
import pyqtgraph as pg
from pynput import keyboard as kb
import time
import threading
import asyncio
from bleak import BleakClient
import socket

# import os
import numpy as np
# from datetime import datetime

from functions import moving_average, diff_filter


## 測定前に検査抵抗の大きさを説明する必要がある、さもなければ検査が正しく行われない
RCAL = 101000 * (1 + (4 / 512))

# 使用前に電流ピーク値を入力することを忘れないでください 単位 uA
AMPLITUDE_OF_CURRENT_PEAK = 0.904


DEVICE_UUID = "9108929D-B8E4-0946-232F-7EE1DDD2654C"
CHARACTERISTIC_UUID = "2A56"
ble_input = ""

HOST = "127.0.0.1"
SEND_PORT = 50007
RECV_PORT = 50008
udp_client = None

plotcountermark = 0

windowWidth = 500  # 曲線を表示するウィンドウの幅
Xm = linspace(0, 0, windowWidth)  # 関連する時間系列を含む配列を作成
Xm2 = linspace(0, 0, windowWidth)
ptr = -windowWidth  # 最初のx位置を設定

f1 = None
f2 = None

ifsamplingflag = False
buffer = ""
dataname = ""
countsamplingfile = 0
sigma = 16
num_filter = 5
calib = 100
th = 5


def threading_of_update():
    global ptr, Xm, Xm2, buffer, data1, data2, ble_input

    TheFirstMeasurementDataFlag = False
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
        c = len(ble_input)
        flag = 0
        if c >= 8:
            namadata = ble_input
            ble_input = ""
            buffer += namadata
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
                            mean(I_offset[calib:])
                            / (
                                (2**19)
                                * (2 / pi)
                                * AMPLITUDE_OF_CURRENT_PEAK
                                * (10**-6)
                            )
                        )
                        mean_Q_offset.append(
                            mean(Q_offset[calib:])
                            / (
                                (2**19)
                                * (2 / pi)
                                * AMPLITUDE_OF_CURRENT_PEAK
                                * (10**-6)
                            )
                        )
                        mean_I_rcal_in.append(
                            mean(I_rcal_in[calib:])
                            / (
                                (2**19)
                                * (2 / pi)
                                * AMPLITUDE_OF_CURRENT_PEAK
                                * (10**-6)
                            )
                        )
                        mean_Q_rcal_in.append(
                            mean(Q_rcal_in[calib:])
                            / (
                                (2**19)
                                * (2 / pi)
                                * AMPLITUDE_OF_CURRENT_PEAK
                                * (10**-6)
                            )
                        )
                        mean_I_rcal_quad.append(
                            mean(I_rcal_quad[calib:])
                            / (
                                (2**19)
                                * (2 / pi)
                                * AMPLITUDE_OF_CURRENT_PEAK
                                * (10**-6)
                            )
                        )
                        mean_Q_rcal_quad.append(
                            mean(Q_rcal_quad[calib:])
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
                    if len(mean_I_offset) <= iii:
                        print("mean_I_offset is not ready yet.")
                        continue
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
                    # Load_mag = sqrt(square(Load_real) + square(Load_imag))
                    # Load_angle = arctan(Load_imag / Load_real) * (180 / pi)
                    if ifsamplingflag:
                        f1.write(str(Load_real) + ",")
                        f2.write(str(Load_imag) + ",")

                    Xm[-1] = Load_real  # 瞬時値を含むベクトル
                    # Xm2[-1] = Load_imag
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
                    Xm[-1] = data1
                    ptr += 1
                elif data[2] == "5":
                    I_rcal_in.append(data1)
                    Q_rcal_in.append(data2)
                    Xm[-1] = data1
                    ptr += 1
                elif data[2] == "7":
                    I_rcal_quad.append(data1)
                    Q_rcal_quad.append(data2)
                    Xm[-1] = data1
                    ptr += 1
        else:
            time.sleep(0.001)


excited_carry = 0


# 興奮状態の判定
def is_excited(data, th):
    global excited_carry

    carry_c = 0.1

    if excited_carry > 0:
        excited_carry -= 0.1
        excited_carry = max(0.0, excited_carry)
    if abs(data[-1]) >= th:
        return True

    if abs(data[-1]) <= th and abs(data[-2]) >= th:
        start = len(data)
        sign = 1 if data[-2] > 0 else -1
        # 閾値を下回る部分まで遡る
        for i in range(2, len(data)):
            if abs(data[-i]) < th:
                start = len(data) - i + 1
                break
        area = abs(np.trapezoid(data[start:-1] - (sign * th)))
        excited_carry = min(excited_carry + area * carry_c, 10)
        return True
    elif abs(data[-2]) <= th and abs(data[-3]) >= th:
        start = len(data)
        sign = 1 if data[-3] > 0 else -1
        # 閾値を下回る部分まで遡る
        for i in range(3, len(data)):
            if abs(data[-i]) < th:
                start = len(data) - i + 1
                break
        area = abs(np.trapezoid(data[start:-1] - (sign * th)))
        excited_carry = min(excited_carry + area * carry_c, 10)
        return True

    if excited_carry > 0:
        return True
    return False


def threading_of_plot():
    Xm_smoothed = Xm

    for i in range(num_filter):
        Xm_smoothed = moving_average(Xm_smoothed, sigma)

    Xm_smoothed = diff_filter(Xm_smoothed)
    udp_client.sendto(str(Xm_smoothed[-1]).encode("utf-8"), (HOST, SEND_PORT))


def on_press(key):
    global ifsamplingflag, countsamplingfile, f1, f2, dataname

    # try:
    #     if key.char == "o":
    #         countsamplingfile += 1
    #         timestamp = datetime.now().strftime("%m%d_%H%M%S")
    #         dataname = f"{timestamp}"
    #         os.makedirs("saved_data", exist_ok=True)
    #         f1 = open("saved_data/" + dataname + "1.txt", "w")
    #         f2 = open("saved_data/" + dataname + "2.txt", "w")
    #         print("The start of sampling")
    #         ifsamplingflag = True
    #     elif key.char == "p":
    #         print("The end of sampling")
    #         f1.close()
    #         f2.close()
    #         ifsamplingflag = False
    # except AttributeError:
    #     # 特殊キー（例：Shift、Ctrlなど）の場合は無視
    #     pass


def start_key_listener():
    with kb.Listener(on_press=on_press) as listener:
        listener.join()


# データ受信時のコールバック関数
async def notification_handler(sender, data):
    global ble_input
    if data.hex().upper() != "5354415254":
        ble_input = data.hex().upper()


# BLE通信を行う関数
async def ble_run():
    while True:
        try:
            async with BleakClient(DEVICE_UUID) as client:
                print(f"Connected: {client.is_connected}")
                await client.start_notify(CHARACTERISTIC_UUID, notification_handler)
                while True:
                    await asyncio.sleep(0.5)
                    await client.write_gatt_char(
                        CHARACTERISTIC_UUID, "START".encode("utf-8")
                    )
                await asyncio.sleep(99999)
                await client.stop_notify(CHARACTERISTIC_UUID)
        except Exception as e:
            print(f"Connection failed: {e}")
            print("Reconnecting...")
            await asyncio.sleep(1)


def udp_receive():
    while True:
        data, addr = udp_client.recvfrom(1024)  # 受信バッファサイズを指定
        data = data.decode("utf-8")
        print(f"Received message: {data} from {addr}")
        Xm2[-1] = float(data)


### MAIN PROGRAM #####
if __name__ == "__main__":
    udp_client = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    udp_client.bind((HOST, RECV_PORT))  # UDPソケットをバインド
    # UDP受信スレッドを開始
    udp_thread = threading.Thread(target=udp_receive)
    udp_thread.daemon = True
    udp_thread.start()

    key_listener_thread = threading.Thread(target=start_key_listener)
    key_listener_thread.daemon = True
    key_listener_thread.start()

    loop = asyncio.get_event_loop()
    loop.create_task(ble_run())

    t1 = threading.Thread(target=threading_of_update)
    t1.daemon = True
    t1.start()

    async def run_app():
        while True:
            await asyncio.sleep(0.1)
            threading_of_plot()

    loop.run_until_complete(run_app())
