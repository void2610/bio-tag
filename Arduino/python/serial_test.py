import serial
import time

buffer = ""
ser = serial.Serial("/dev/cu.usbserial-12BP0164", 9600)
print(ser)

while True:
    c = ser.inWaiting()
    print(c)
    if ser.inWaiting() > 0:
        line = ser.read(16)
        namadata = str(line)[2:-1]
        print(namadata)

    time.sleep(0.005)
