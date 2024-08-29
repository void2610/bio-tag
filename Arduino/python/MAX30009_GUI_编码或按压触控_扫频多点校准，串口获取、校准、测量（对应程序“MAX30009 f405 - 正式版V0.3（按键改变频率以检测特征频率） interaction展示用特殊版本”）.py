# Import libraries
from numpy import *
from pyqtgraph.Qt import QtGui, QtCore
import pyqtgraph as pg
import serial
import binascii
import time
import threading
import keyboard  # using module keyboard
import sys
import pyautogui
from PyQt5 import QtWidgets


import sys
from PyQt5.QtWidgets import (
    QApplication,
    QWidget,
    QVBoxLayout,
    QProgressBar,
    QPushButton,
    QLineEdit,
    QStylePainter,
    QStyleOptionProgressBar,
)
from PyQt5.QtCore import pyqtSignal, pyqtSlot, Qt, QRectF, QObject
from PyQt5.QtGui import QPainter, QFont, QColor, QTextOption


# 消息发送部分
import socket

# Pygame程序的IP地址和端口号
pygame_host = "127.0.0.1"  # 这里假设Pygame程序在本地运行
pygame_port = 12345  # 与Pygame程序通信的端口号

# 创建套接字
client_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

# 连接到Pygame程序
client_socket.connect((pygame_host, pygame_port))


##在测量前必须要交代检验电阻的大小，不然会检验不对
RCAL = 900 * (1 + (4 / 512))

# 每次使用前记得输入电流峰值 单位 uA
AMPLITUDE_OF_CURRENT_PEAK = 45.25

# 创建串口对象
portName = (
    "/dev/cu.usbserial-12BP0164"  # このポート名をあなたのものに置き換えてください!
)
baudrate = 9600
ser = serial.Serial(portName, baudrate)


### START QtApp #####
app = QtWidgets.QApplication([])  # you MUST do this once (initialize things)
####################

win = pg.GraphicsLayoutWidget(show=True)  # creates a window
p = win.addPlot(title="Realtime plot")  # creates empty space for the plot in the window
curve = p.plot()  # create an empty "plot" (a curve to plot)
curve2 = p.plot()

plotcountermark = 0

windowWidth = 1000  # width of the window displaying the curve
Xm = linspace(
    0, 0, windowWidth
)  # create array that will contain the relevant time series
Xm2 = linspace(0, 0, windowWidth)
ptr = -windowWidth  # set first x position


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
    "44": ".",  ###TODO:特殊键入 还未定义
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
        self.setFormat("")  # Clear existing format

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

        # Connect the custom signal to the slot for updating the text
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

        # 添加文本标签
        self.edit = QLineEdit()
        self.edit.setPlaceholderText("Watting for input")
        layout.addWidget(self.edit)

        # 添加进度条
        self.upper_progress_bar = CustomProgressBar(self)
        layout.addWidget(self.upper_progress_bar)

        # 添加下方的进度条
        # self.lower_progress_bar = LoerProgressBar(self)
        self.lower_progress_bar = QProgressBar(self)
        self.lower_progress_bar.setMinimum(0)
        self.lower_progress_bar.setMaximum(700)
        self.lower_progress_bar.setTextVisible(False)  # 不显示文本
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
        print(c)
        if c >= 8:
            namadata = str(binascii.b2a_hex(ser.read(8)))[
                2:-1
            ]  # 一次只收8个数据 即16个16进制数
            buffer += namadata  # 在buffer里面存储数据

            if (
                len(buffer) == 16
            ):  # 在buffer为空时第一次收到16个16进制数时进行一些初始处理
                print(namadata)

                flag = buffer.find("f0")  # 标记f0对应点的起始位
                print("flag=", flag, "c=", c)
                if (
                    buffer[flag + 1 :].find("f0") == -1
                ):  # 如果这一组里面有多个f0 我就懒得判断了直接去掉这一组 找下一组
                    buffer = buffer[flag:]
                else:
                    buffer = ""

            if (
                len(buffer) > 16
            ):  # 从第二组开始，将上一组的16-flag个数据和这一组的flag数据拼起来，作为第一组输出
                # 如果下一次来的数据格式跟上一次发生了变化的异常处理

                if (
                    namadata[flag : flag + 2] != "f0" and namadata.count("f0") == 1
                ):  # TODO 如果传输发生错误flag位置大概率不是f0 因此这样判断 但是有可能传输发生错误时这一位置刚好是f0 因此判断不全面 但因为概率低 暂时没做
                    print(namadata)
                    print(
                        "数据顺序发生了变化\n变动前flag=",
                        flag,
                        "\n变动后flag=",
                        namadata.find("f0"),
                    )
                    # 清空buffer 重新标记f0对应点的起始位
                    buffer = ""
                    flag = namadata.find("f0")
                    buffer = (
                        buffer + namadata[flag:]
                    )  # 把f0之后的保存到buffer，作为未完成的一帧,准备与下一帧结合成完整的一帧
                    continue

                elif namadata.find("f0") == -1:
                    buffer = buffer[
                        :-16
                    ]  # 每次输出一个完整的帧之后删掉原来的一帧，控制buffer的长度
                    continue

                # 若一切如意，则将数据正常拼接
                Xm[:-1] = Xm[1:]  # shift data in the temporal mean 1 sample left
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

                # print(data1)
                # print(data2)
                # Xm[-1] =  data1  # vector containing the instantaneous values
                # Xm2[-1] = data2
                # ptr += 1  # update x position for displaying the curve
                buffer = buffer[
                    16:
                ]  # 每次输出一个完整的帧之后删掉原来的一帧，减小buffer的规模
                if data[2] == "0":
                    # 当gap符号来临时
                    if TheFirstMeasurementDataFlag == False:
                        # 正式开始测量的第一个数据来临时把之前的校准数据用上，iii表示第几个频率的校准
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
                    if TheFirstMeasurementDataFlag == False:
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
                    if ifsamplingflag == True:
                        f1.write(str(Load_mag) + ",")
                        f2.write(str(Load_angle) + ",")

                    test1[:-1] = test1[1:]
                    test2[:-1] = test2[1:]
                    test1[-1] = Load_real
                    test2[-1] = Load_imag

                    Xm[-1] = Load_real  # vector containing the instantaneous values
                    Xm2[-1] = Load_imag
                    ptr += 1  # update x position for displaying the curve

                    if test1[-2] - test1[-1] >= 10:
                        counter = 0
                        touch_sensor = True
                        keyboard.press("space")
                        keyboard.release("space")

                    elif (
                        abs(test1[-1] - test1[-2]) >= 20
                        and counter == 30
                        and touch_sensor == True
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
                                if cap_flag == False:
                                    cap_flag = True
                                else:
                                    cap_flag = False
                            elif keyboard_state_machine[strbuffer] == "Shift":
                                if shift_flag == False:
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
                        and touch_sensor == True
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

                    if touch_sensor == True and counter != 30:
                        counter = counter + 1
                        if counter == 30:
                            record_value = smoothdata(test1[-20:], 20)[0]

                    if touch_sensor == True and counter == 30:
                        cancel_counter = 0
                        order = int(
                            (smoothdata(test1[-80:], 80)[0] - record_value) // 1
                        )
                        map_order = 350 + order * 10

                        ##TODO:这里是为了Interaction演示
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

                # Zero符号来临时的特殊标记和特殊操作
                if data[2] == "e":
                    if ifsamplingflag == True:
                        f1.write(str("Z") + ",")
                        f2.write(str("Z") + ",")
                    # print(Load_real)
                    # print(Load_imag)
                    # Xm[-1] =  Load_real   # vector containing the instantaneous values
                    # Xm2[-1] = Load_imag

                # 下面是数据校准时使用的一些识别符号和操作
                elif data[2] == "3":
                    I_offset.append(data1)
                    Q_offset.append(data2)
                elif data[2] == "5":
                    I_rcal_in.append(data1)
                    Q_rcal_in.append(data2)
                elif data[2] == "7":
                    I_rcal_quad.append(data1)
                    Q_rcal_quad.append(data2)

            # 按键处理、保存数据
            try:
                if ifsamplingflag == False:
                    if keyboard.is_pressed("s"):  # if key 's' is pressed
                        # 打开文件 准备写入
                        countsamplingfile += 1
                        dataname = "pysavedsampling" + str(countsamplingfile) + "data"
                        f1 = open(dataname + "1.txt", "w")
                        f2 = open(dataname + "2.txt", "w")
                        print("The start of sampling")
                        ifsamplingflag = True
                else:
                    pass

                if ifsamplingflag == True:
                    if keyboard.is_pressed("e"):  # if key 'q' is pressed
                        print("The end of sampling")
                        # 关闭文件
                        f1.close()
                        f2.close()
                        ifsamplingflag = False
                else:
                    pass
            except:
                break  # if user pressed a key other than the given key the loop will break
        else:
            time.sleep(0.001)


def threading_of_plot():
    global curve, curve2, ptr, Xm, Xm2, plotcountermark
    # 按键处理、保存数据
    try:
        if keyboard.is_pressed("c"):  # if key 's' is pressed
            plotcountermark = plotcountermark + 1
            if plotcountermark == 3:
                plotcountermark = 0
        else:
            pass
    except:
        pass
    if plotcountermark == 0:
        curve.setData(Xm, pen="b")  # set the curve with this data
        curve.setPos(ptr, 0)  # set x position in the graph to 0
        curve2.setData(Xm2, pen="r")  # set the curve with this data
        curve2.setPos(ptr, 0)  # set x position in the graph to 0
        QtWidgets.QApplication.processEvents()  # you MUST process the plot now

    if plotcountermark == 1:
        curve.setData(Xm, pen="b")  # set the curve with this data
        curve.setPos(ptr, 0)  # set x position in the graph to 0
        curve2.clear()
        QtWidgets.QApplication.processEvents()  # you MUST process the plot now

    if plotcountermark == 2:
        curve2.setData(Xm2, pen="r")  # set the curve with this data
        curve.clear()
        curve2.setPos(ptr, 0)  # set x position in the graph to 0
        QtWidgets.QApplication.processEvents()  # you MUST process the plot now


### MAIN PROGRAM #####
# this is a brutal infinite loop calling your realtime data plot
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
    timer.timeout.connect(threading_of_plot)  # 定时刷新数据显示
    timer.start(100)  # 多少ms调用一次

    ### END QtApp ####
    QtWidgets.QApplication.exec_()  # you MUST put this at the end
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
