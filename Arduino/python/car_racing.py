import pygame
import sys
import random
import socket
import threading

command = 0

#消息接收模块
def receive_data(client_socket):
    global command
    while True:
        try:
            command = server_socket.recv(1024).decode()
            if command:
                print("收到指令:", command)
                # 根据接收到的指令执行相应操作
                # 这里可以添加你的操作逻辑
        except socket.error as e:
            pass

        # 尝试接收数据
        command = client_socket.recv(1024).decode()
        if not command:
            continue
        else:
            print("收到指令:", command)
            # 在这里执行相应的操作，例如更新游戏状态或处理用户输入


# Pygame程序监听的IP地址和端口号
pygame_host = '127.0.0.1'  # 这里假设Pygame程序在本地运行
pygame_port = 12345  # 与Pygame程序通信的端口号

# 创建套接字
server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

# 绑定IP地址和端口号
server_socket.bind((pygame_host, pygame_port))

# 监听连接
server_socket.listen()

print("等待连接...")

# 接受连接
client_socket, client_address = server_socket.accept()

print(f"连接来自 {client_address}")

# 创建并启动接收数据的线程
receive_thread = threading.Thread(target=receive_data, args=(client_socket,))
receive_thread.start()




##pygame从这里开始
C, R = 9, 20  # 11列， 20行
CELL_SIZE = 40  # 格子尺寸

FPS=165 # 游戏帧率
MOVE_SPACE = 20  # 敌人移动速度（单位，帧）
Original_MOVE_SPACE = MOVE_SPACE

WIN_WIDTH = CELL_SIZE * C  # 窗口宽度
WIN_HEIGHT = CELL_SIZE * R  # 窗口高度

COLORS = {
    "bg": (200, 200, 200),
    "player": (65, 105, 225),  # RoyalBlue
    "enemy": (50, 50, 50),
    "line": (225, 225, 225),
    "score": (0,128,0),  # SpringGreen
    "over": (255,0,0)
}

CARS = {  # 车的形状，即格子位置
    "player": [
        [0, 1, 0],
        [0, 1, 0],
        [1, 1, 1],
    ],
    "enemy": [
        [1, 0, 1],
        [1, 1, 1],
        [0, 1, 0],
    ]
}


DIRECTIONS = {
    "UP": (0, -1),
    "DOWN": (0, 1),
    "LEFT": (-1, 0),
    "RIGHT": (1, 0),
}


pygame.init() # pygame初始化，必须有，且必须在开头
# 创建主窗体
clock=pygame.time.Clock() # 用于控制循环刷新频率的对象
win=pygame.display.set_mode((WIN_WIDTH,WIN_HEIGHT))
pygame.display.set_caption('Car Racing by Big Shuang')

# 大中小三种字体，48,36,24
FONTS = [
    pygame.font.Font(pygame.font.get_default_font(), font_size) for font_size in [48, 36, 24]
]


class Block(pygame.sprite.Sprite):
    def __init__(self, c, r, color="bg"):
        super().__init__()

        self.cr = [c, r]
        self.x = c * CELL_SIZE
        self.y = r * CELL_SIZE

        self.image  = pygame.Surface((CELL_SIZE, CELL_SIZE))
        self.image.fill(COLORS[color])
        # points = []
        # pygame.draw.polygon(self.image, COLOR_1, points)

        self.rect = self.image.get_rect()

        self.rect.move_ip(self.x, self.y)

    def move_cr(self, c, r):
        self.cr[0] = c
        self.cr[1] = r
        self.x = c * CELL_SIZE
        self.y = r * CELL_SIZE
        self.rect.left = self.x
        self.rect.top = self.y

    def is_out(self):
        if 0 <= self.cr[0] < C and 0 <= self.cr[1] < R:
            return False
        return False

    def move(self, direction=""):
        move_c, move_r = DIRECTIONS[direction]
        next_c, next_r = self.cr[0] + move_c, self.cr[1] + move_r
        self.move_cr(next_c, next_r)




    def check_move(self, direction=""):
        move_c, move_r = DIRECTIONS[direction]
        next_c, next_r = self.cr[0] + move_c, self.cr[1] + move_r

        if 0 <= next_c < C and 0 <= next_r < R:
            return True

        return False

    def check_collide(self, car):
        if tuple(self.cr) in car.get_locations():
            return True

        return False


class Car(pygame.sprite.Group):
    def __init__(self, c, r, car_kind):
        super().__init__()

        self.kind = car_kind

        for ri, row in enumerate(CARS[self.kind]):
            for ci, cell in enumerate(row):
                if cell == 1:
                    block = Block(c+ci, r+ri, car_kind)
                    self.add(block)

    def move(self, direction=""):
        if all(block.check_move(direction) for block in self.sprites()):
            self.free_move(direction)

    def moveto_leftmost(self):
        leftmost_column = 0
        min_c = min(block.cr[0] for block in self.sprites())
        distance_to_move = min_c - leftmost_column
        for block in self.sprites():
            block.move_cr(block.cr[0] - distance_to_move, block.cr[1])

    def moveto_rightmost(self):
        rightmost_column = C - len(CARS["player"][0])
        max_c = max(block.cr[0] for block in self.sprites())
        distance_to_move = rightmost_column - max_c
        for block in self.sprites():
            block.move_cr(block.cr[0] + distance_to_move, block.cr[1])


    def moveto(self, num):
        column = num
        min_c = min(block.cr[0] for block in self.sprites())
        distance_to_move = min_c - column
        for block in self.sprites():
            block.move_cr(block.cr[0] - distance_to_move, block.cr[1])
    def free_move(self, direction=""):
        for block in self.sprites():
            block.move(direction)

    def is_out(self):
        return all(block.is_out() for block in self.sprites())

    def check_collide(self, *cars):
        for car in cars:
            if any(block.check_collide(car) for block in self.sprites()):
                 return True

        return False

    def get_locations(self):
        return [tuple(block.cr) for block in self.sprites()]


class EnemyManager():
    def __init__(self):
        self.enemies = []

        self.move_count = 0

    def gen_new_enemies(self):
        if self.move_count % (4 * len(CARS["enemy"]) + 1) == 1:
            ec = random.randint(1, C - len(CARS["enemy"][0]))
            enemy = Car(ec, 0, "enemy")

            self.enemies.append(enemy)

    def move(self):
        to_delete = []
        for i, enemy in enumerate(self.enemies):
            enemy.free_move("DOWN")
            if enemy.is_out():
                to_delete.append(i)

        for di in to_delete[::-1]:  # 倒着按序号来删除
            self.enemies.pop(di)

        self.move_count += 1

        self.gen_new_enemies()

    def draw(self, master):
        for enemy in self.enemies:
            enemy.draw(master)


bottom_center_c = (C - len(CARS["player"][0])) // 2
bottom_center_r = R - len(CARS["player"])

player_car = Car(bottom_center_c, bottom_center_r, "player")
time_count = 0
emg = EnemyManager()

start_info = FONTS[2].render("Press any key to start game", True, COLORS["score"])
text_rect = start_info.get_rect(center=(WIN_WIDTH / 2, WIN_HEIGHT / 2))
win.blit(start_info, text_rect)

running = False

while True:

    player_car.moveto(int(str(command)[0]))
    # 获取所有事件
    for event in pygame.event.get():
        if event.type == pygame.QUIT:
            # 判断当前事件是否为点击右上角退出键
            pygame.quit()
            sys.exit()

        if event.type == pygame.KEYDOWN:
            if running:
                # if event.key == pygame.K_LEFT or event.key == ord('a'):
                #     player_car.move("LEFT")
                # if event.key == pygame.K_RIGHT or event.key == ord('d'):
                #     player_car.move("RIGHT")
                # if event.key == pygame.K_UP or event.key == ord('w'):
                #     player_car.move("UP")
                # if event.key == pygame.K_DOWN or event.key == ord('s'):
                #     player_car.move("DOWN")
                pass


            else:
                # reset game
                player_car = Car(bottom_center_c, bottom_center_r, "player")
                time_count = 0
                emg = EnemyManager()
                running =True

    if running:
        # Fill the screen with black
        win.fill(COLORS["bg"])

        if (time_count + 1) % MOVE_SPACE == 0:
            emg.move()

        for ci in range(C):
            cx = CELL_SIZE * ci
            pygame.draw.line(win, COLORS["line"], (cx, 0), (cx, R * CELL_SIZE))

        # Draw the player on the screen
        player_car.draw(win)
        emg.draw(win)
        text_info = FONTS[2].render("Scores: %d" % (time_count / FPS), True, COLORS["score"])
        if MOVE_SPACE > 5:
            MOVE_SPACE = Original_MOVE_SPACE - int(time_count / FPS)//10

        win.blit(text_info, dest=(0, 0))


        if player_car.check_collide(*emg.enemies):
            print("Game Over")
            texts = ["Game Over", "Scores: %d" % (time_count / FPS), "Press Any Key to Restart game"]
            for ti, text in enumerate(texts):
                over_info = FONTS[ti].render(text, True, COLORS["over"])
                text_rect = over_info.get_rect(center=(WIN_WIDTH / 2, WIN_HEIGHT / 2 + 48 * ti))
                win.blit(over_info, text_rect)

            running = False

        time_count += 1

    clock.tick(FPS) # 控制循环刷新频率,每秒刷新FPS对应的值的次数

    pygame.display.update()