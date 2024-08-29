# Import libraries
from numpy import *
from numpy import linspace, pi, cos, sin, arctan, sqrt, square, mean
import pyqtgraph as pg
import serial
import binascii
import time
import threading
import keyboard  # using module keyboard
import sys

from PyQt5 import QtWidgets

from PyQt5.QtWidgets import (
    QApplication,
    QWidget,
    QVBoxLayout,
    QProgressBar,
    QLineEdit,
)
from PyQt5.QtCore import pyqtSignal, pyqtSlot, Qt, QRectF, QObject
from PyQt5.QtGui import QPainter

# メッセージ送信部分
import socket

# PygameプログラムのIPアドレスとポート番号
pygame_host = "127.0.0.1"  # ここではPygameプログラムがローカルで実行されていると仮定
pygame_port = 12345  # Pygameプログラムと通信するためのポート番号

# ソケットを作成
client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

# Pygameプログラムに接続
client_socket.connect((pygame_host, pygame_port))


## 測定前に検査抵抗の大きさを説明する必要がある、さもなければ検査が正しく行われない
RCAL = 900 * (1 + (4 / 512))

# 使用前に電流ピーク値を入力することを忘れないでください 単位 uA
AMPLITUDE_OF_CURRENT_PEAK = 45.25

# シリアルポートオブジェクトを作成
portName = "/dev/cu.usbserial-12BP0164"  # このポート名をあなたのものに置き換えてください！  # このポート名をあなたのものに置き換えてください！
baudrate = 9600
ser = serial.Serial(portName, baudrate)


### START QtApp #####
app = QtWidgets.QApplication([])  # これは一度行う必要があります（初期化）
####################

win = pg.GraphicsLayoutWidget(show=True)  # ウィンドウを作成
p = win.addPlot(title="Realtime plot")  # ウィンドウ内にプロットのための空間を作成
curve = p.plot()  # 空の「プロット」を作成（プロットするための曲線）
curve2 = p.plot()

plotcountermark = 0

windowWidth = 1000  # 曲線を表示するウィンドウの幅
Xm = linspace(0, 0, windowWidth)  # 関連する時間系列を含む配列を作成
Xm2 = linspace(0, 0, windowWidth)
ptr = -windowWidth  # 最初のx位置を設定


ifsamplingflag = False
buffer = ""

dataname = ""
countsamplingfile = 0


keyboard_state_machine = {
    "": "",
    "1": "",
    "2": "",
    "3": "",
    "4": "",
    "5": "",
    "6": "",
    "21": "d",
    "22": "s",
    "23": "a",
    "24": "f",
    "25": "g",
    "26": "Cap",
    "61": "i",
    "62": "u",
    "63": "y",
    "64": "o",
    "65": "p",
    "66": "Del",
    "31": "e",
    "32": "w",
    "33": "q",
    "34": "r",
    "35": "t",
    "36": "Num",
    "51": "k",
    "52": "j",
    "53": "h",
    "54": "l",
    "55": ";",
    "56": "'",
    "11": "c",
    "12": "x",
    "13": "z",
    "14": "v",
    "15": "b",
    "16": " ",
    "41": ",",
    "42": "m",
    "43": "n",
    "44": ".",  ###TODO:特殊キー入力 まだ定義されていない
    "45": "?",
    "46": "Shift",
}


class Signal_of_PyQt(QObject):
    msg = pyqtSignal(int)

    def run(self, value):
        self.msg.emit(value)


class CustomProgressBar(QProgressBar):
    customSignal = pyqtSignal(int)

    def __init__(self, parent=None):
        super().__init__(parent)
        self.setMinimum(0)
        self.setMaximum(700)
        self.setValue(700)
        self.setTextVisible(False)
        self.paintMark = 0

        self.setStyleSheet("""
            QProgressBar::chunk {
                background-color: qlineargradient(x1:0, y1:0, x2:1, y2:0,
                                                  stop:0 #FFFFFF,
                                                  stop:0.1428 #FFFFFF,
                                                  stop:0.1429 #FFA500,
                                                  stop:0.2857 #FFA500,
                                                  stop:0.2858 #FFFF00,
                                                  stop:0.4286 #FFFF00,
                                                  stop:0.4287 #008000,
                                                  stop:0.5714 #008000,
                                                  stop:0.5715 #0000FF,
                                                  stop:0.7143 #0000FF,
                                                  stop:0.7144 #800080,
                                                  stop:0.8571 #800080,
                                                  stop:0.8572 #FF00FF,
                                                  stop:1 #FF00FF);
            }
        """)

    @pyqtSlot(int)
    def updateProgressBar(self, value):
        self.setValue(value)
        self.setFormat("")  # 既存のフォーマットをクリア

    def paintEvent(self, event):
        if self.paintMark == 0:
            paint_str_list = [
                "qwert[Num]",
                "asdfg[Cap]",
                "zxcvb[ ]",
                "",
                "nm,.?[Shift]",
                "hjkl;[']",
                "yuiop[Del]",
            ]
        elif self.paintMark == 2:
            paint_str_list = ["a", "s", "d", "[Cancel]", "f", "g", "[Cap]"]
        elif self.paintMark == 6:
            paint_str_list = ["y", "u", "i", "[Cancel]", "o", "p", "[Del]"]
        elif self.paintMark == 3:
            paint_str_list = ["q", "w", "e", "[Cancel]", "r", "t", "[Num]"]
        elif self.paintMark == 5:
            paint_str_list = ["h", "j", "k", "[Cancel]", "l", ";", "[\]"]
        elif self.paintMark == 1:
            paint_str_list = ["z", "x", "c", "[Cancel]", "v", "b", "[ ]"]
        elif self.paintMark == 4:
            paint_str_list = ["n", "m", ",", "[Cancel]", ".", "?", "[Shift]"]
        super().paintEvent(event)
        painter = QPainter(self)
        painter.setRenderHint(QPainter.Antialiasing)

        segment_width = self.width() / 7
        for i in range(7):
            segment_text = paint_str_list[i]  # Numbers from -3 to 3
            text_rect = QRectF(i * segment_width, 0, segment_width, self.height())
            painter.drawText(text_rect, Qt.AlignCenter, segment_text)


class LoerProgressBar(QProgressBar):
    customSignal = pyqtSignal(int)

    def __init__(self, parent=None):
        super().__init__(parent)
        self.setMinimum(0)
        self.setMaximum(700)
        self.setValue(700)
        self.setTextVisible(False)

        # カスタム信号をテキスト更新のスロットに接続
        self.customSignal.connect(self.updateText)

    @pyqtSlot(str)
    def updateText(self, text):
        self.custom_text = text
        self.update()


class MyWidget(QWidget):
    def __init__(self):
        super().__init__()

        self.initUI()

    def initUI(self):
        layout = QVBoxLayout(self)

        # テキストラベルを追加
        self.edit = QLineEdit()
        self.edit.setPlaceholderText("入力を待っています")
        layout.addWidget(self.edit)

        # プログレスバーを追加
        self.upper_progress_bar = CustomProgressBar(self)
        layout.addWidget(self.upper_progress_bar)

        # 下のプログレスバーを追加
        # self.lower_progress_bar = LoerProgressBar(self)
        self.lower_progress_bar = QProgressBar(self)
        self.lower_progress_bar.setMinimum(0)
        self.lower_progress_bar.setMaximum(700)
        self.lower_progress_bar.setTextVisible(False)  # テキストを表示しない
        layout.addWidget(self.lower_progress_bar)

        self.show()


# Realtime data plot. Each time this function is called, the data display is updated
def threading_of_move_mouse(x):
    print(x)


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
        send0
    TheFirstMeasurementDataFlag = False
    ser.flushInput()
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
    test1 = [0] * 200
    test2 = [0] * 200
    touch_sensor = False
    strbuffer = ""
    order = 0
    counter = 0
    cap_flag = False
    shift_flag = False
    cancel_counter = 0

    while True:
        c = ser.inWaiting()
        flag = 0
        if c >= 8:
            line = ser.read(16)
            namadata = str(line)[2:-1]  # 一度に8個のデータを受信（16進数で16個）
            buffer += namadata  # バッファにデータを保存
            if (
                len(buffer) == 16
            ):  # バッファが空のとき、最初に16個の16進数を受信したときに初期処理を行う
                # print(namadata)

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
                ):  # TODO もし伝送にエラーが発生した場合、flagの位置は大抵f0ではないため、こう判断するが、伝送エラーが発生した場合にこの位置がちょうどf0である可能性もあるため、判断が不完全だが、確率が低いため、暫定的に未実施
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
                # print(data)

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

                # print(data1)
                # print(data2)
                # Xm[-1] =  data1  # 瞬時値を含むベクトル
                # Xm2[-1] = data2
                # ptr += 1  # 曲線を表示するためのx位置を更新
                buffer = buffer[
                    16:
                ]  # 毎回完全なフレームを出力した後、元のフレームを削除し、バッファのサイズを小さくする
                if data[2] == "0":
                    # gapシンボルが来たとき
                    if not TheFirstMeasurementDataFlag:
                        # 正式に測定が始まる最初のデータが来たとき、以前のキャリブレーションデータを使用する、iiiは何番目の周波数のキャリブレーションを示す
                        mean_I_offset.append(
                            mean(I_offset[40:])
                            / (
                                (2**19)
                                * (2 / pi)
                                * AMPLITUDE_OF_CURRENT_PEAK
                                * (10**-6)
                            )
                        )
                        mean_Q_offset.append(
                            mean(Q_offset[40:])
                            / (
                                (2**19)
                                * (2 / pi)
                                * AMPLITUDE_OF_CURRENT_PEAK
                                * (10**-6)
                            )
                        )
                        mean_I_rcal_in.append(
                            mean(I_rcal_in[40:])
                            / (
                                (2**19)
                                * (2 / pi)
                                * AMPLITUDE_OF_CURRENT_PEAK
                                * (10**-6)
                            )
                        )
                        mean_Q_rcal_in.append(
                            mean(Q_rcal_in[40:])
                            / (
                                (2**19)
                                * (2 / pi)
                                * AMPLITUDE_OF_CURRENT_PEAK
                                * (10**-6)
                            )
                        )
                        mean_I_rcal_quad.append(
                            mean(I_rcal_quad[40:])
                            / (
                                (2**19)
                                * (2 / pi)
                                * AMPLITUDE_OF_CURRENT_PEAK
                                * (10**-6)
                            )
                        )
                        mean_Q_rcal_quad.append(
                            mean(Q_rcal_quad[40:])
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
                        if iii == 11:
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

                    test1[:-1] = test1[1:]
                    test2[:-1] = test2[1:]
                    test1[-1] = Load_real
                    test2[-1] = Load_imag
                    Xm[-1] = Load_real  # 瞬時値を含むベクトル
                    Xm2[-1] = Load_imag
                    ptr += 1  # 曲線を表示するためのx位置を更新

                    if test1[-2] - test1[-1] >= 10:
                        counter = 0
                        touch_sensor = True
                        keyboard.press("space")
                        keyboard.release("space")

                    elif (
                        abs(test1[-1] - test1[-2]) >= 20
                        and counter == 30
                        and touch_sensor
                    ):
                        touch_sensor = False
                        if order < 5 and order > -5:
                            strbuffer += "0"
                        elif order >= 5 and order < 15:
                            strbuffer += "4"
                        elif order >= 15 and order < 25:
                            strbuffer += "5"
                        elif order >= 25:
                            strbuffer += "6"
                        elif order > -15 and order <= -5:
                            strbuffer += "1"
                        elif order > -25 and order <= -15:
                            strbuffer += "2"
                        elif order <= -25:
                            strbuffer += "3"
                        print(strbuffer)

                        send0.run(0)
                        if strbuffer not in keyboard_state_machine:
                            strbuffer = ""
                            my_widget.upper_progress_bar.paintMark = 0
                            my_widget.upper_progress_bar.update()
                        elif len(strbuffer) == 2:
                            if keyboard_state_machine[strbuffer] == "Del":
                                my_widget.edit.backspace()
                            elif keyboard_state_machine[strbuffer] == "Cap":
                                if not cap_flag:
                                    cap_flag = True
                                else:
                                    cap_flag = False
                            elif keyboard_state_machine[strbuffer] == "Shift":
                                if not shift_flag:
                                    shift_flag = True
                                else:
                                    shift_flag = False
                            else:
                                if cap_flag and shift_flag:
                                    my_widget.edit.insert(
                                        keyboard_state_machine[strbuffer]
                                    )
                                elif cap_flag or shift_flag:
                                    my_widget.edit.insert(
                                        keyboard_state_machine[strbuffer].upper()
                                    )
                                else:
                                    my_widget.edit.insert(
                                        keyboard_state_machine[strbuffer]
                                    )
                                shift_flag = False

                            strbuffer = ""
                            my_widget.upper_progress_bar.paintMark = 0
                            my_widget.upper_progress_bar.update()
                        elif len(strbuffer) == 1:
                            my_widget.upper_progress_bar.paintMark = eval(strbuffer)
                            my_widget.upper_progress_bar.update()

                    elif (
                        abs(test1[-1] - test1[-2]) >= 20
                        and counter != 30
                        and touch_sensor
                    ):
                        strbuffer = ""
                        cancel_counter += 1
                        if cancel_counter == 4:
                            my_widget.edit.backspace()
                            cancel_counter = 0
                        my_widget.upper_progress_bar.paintMark = 0
                        my_widget.upper_progress_bar.update()
                        touch_sensor = False
                        send0.run(0)

                    elif abs(test1[-1] - test1[-2]) >= 20:
                        touch_sensor = False
                        send0.run(0)

                    if touch_sensor and counter != 30:
                        counter = counter + 1
                        if counter == 30:
                            record_value = smoothdata(test1[-20:], 20)[0]

                    if touch_sensor and counter == 30:
                        cancel_counter = 0
                        order = int(
                            (smoothdata(test1[-80:], 80)[0] - record_value) // 1
                        )
                        map_order = 350 + order * 10

                        ##TODO: これはInteractionデモのため
                        if order < 5 and order > -5:
                            client_socket.send("3".encode())
                        elif order >= 5 and order < 15:
                            client_socket.send("4".encode())
                        elif order >= 15 and order < 25:
                            client_socket.send("5".encode())
                        elif order >= 25:
                            client_socket.send("6".encode())
                        elif order > -15 and order <= -5:
                            client_socket.send("2".encode())
                        elif order > -25 and order <= -15:
                            client_socket.send("1".encode())
                        elif order <= -25:
                            client_socket.send("0".encode())

                        send0.run(map_order)
                        # t4 = threading.Thread(target=threading_of_move_mouse(order))
                        # t4.start()
                        # print(order)

                # Zeroシンボルが来たときの特別なマークと特別な操作
                if data[2] == "e":
                    if ifsamplingflag:
                        f1.write(str("Z") + ",")
                        f2.write(str("Z") + ",")
                    # print(Load_real)
                    # print(Load_imag)
                    # Xm[-1] =  Load_real   # 瞬時値を含むベクトル
                    # Xm2[-1] = Load_imag

                # データキャリブレーション時に使用される識別子と操作
                elif data[2] == "3":
                    I_offset.append(data1)
                    Q_offset.append(data2)
                elif data[2] == "5":
                    I_rcal_in.append(data1)
                    Q_rcal_in.append(data2)
                elif data[2] == "7":
                    I_rcal_quad.append(data1)
                    Q_rcal_quad.append(data2)

            # キー処理、データ保存
            try:
                if not ifsamplingflag:
                    if keyboard.is_pressed("s"):  # if key 's' is pressed
                        # ファイルを開いて書き込みの準備
                        countsamplingfile += 1
                        dataname = "pysavedsampling" + str(countsamplingfile) + "data"
                        f1 = open(dataname + "1.txt", "w")
                        f2 = open(dataname + "2.txt", "w")
                        print("サンプリングの開始")
                        ifsamplingflag = True
                else:
                    pass

                if ifsamplingflag:
                    if keyboard.is_pressed("e"):  # if key 'q' is pressed
                        print("サンプリングの終了")
                        # ファイルを閉じる
                        f1.close()
                        f2.close()
                        ifsamplingflag = False
                else:
                    pass
            except Exception:
                pass
                # print(f"Error: {e}")
        else:
            time.sleep(0.001)


def threading_of_plot():
    global curve, curve2, ptr, Xm, Xm2, plotcountermark

    # キー処理、データ保存
    try:
        if keyboard.is_pressed("c"):  # if key 's' is pressed
            plotcountermark = plotcountermark + 1
            if plotcountermark == 3:
                plotcountermark = 0
        else:
            pass
    except Exception:
        pass
        # print(f"Error: {e}")
    if plotcountermark == 0:
        curve.setData(Xm, pen="b")  # このデータで曲線を設定
        curve.setPos(ptr, 0)  # グラフのx位置を0に設定
        curve2.setData(Xm2, pen="r")  # このデータで曲線を設定
        curve2.setPos(ptr, 0)  # グラフのx位置を0に設定
        QtWidgets.QApplication.processEvents()  # プロットを処理する必要があります

    if plotcountermark == 1:
        curve.setData(Xm, pen="b")  # このデータで曲線を設定
        curve.setPos(ptr, 0)  # グラフのx位置を0に設定
        curve2.clear()
        QtWidgets.QApplication.processEvents()  # プロットを処理する必要があります

    if plotcountermark == 2:
        curve2.setData(Xm2, pen="r")  # このデータで曲線を設定
        curve.clear()
        curve2.setPos(ptr, 0)  # グラフのx位置を0に設定
        QtWidgets.QApplication.processEvents()  # プロットを処理する必要があります


### MAIN PROGRAM #####
# これはリアルタイムデータプロットを呼び出す過酷な無限ループです
if __name__ == "__main__":
    t1 = threading.Thread(target=threading_of_update)
    t1.setDaemon(True)
    t1.start()

    app = QApplication(sys.argv)
    my_widget = MyWidget()

    send0 = Signal_of_PyQt()
    send1 = Signal_of_PyQt()
    send2 = Signal_of_PyQt()
    send3 = Signal_of_PyQt()
    send4 = Signal_of_PyQt()
    send5 = Signal_of_PyQt()
    send6 = Signal_of_PyQt()
    send0.msg.connect(my_widget.lower_progress_bar.setValue)

    timer = pg.QtCore.QTimer()
    timer.timeout.connect(threading_of_plot)  # 定期的にデータ表示を更新
    timer.start(100)  # 何msごとに呼び出すか

    ### END QtApp ####
    QtWidgets.QApplication.exec_()  # これは最後に置く必要があります
    ##################


def smoothdata(x, windowsize):
    length = len(x)
    output = []
    for i in range(0, length - windowsize + 1):
        temp = 0
        for j in range(0, windowsize):
            temp = temp + x[(i) + j]

        temp = temp / windowsize
        output.append(temp)

    return output
