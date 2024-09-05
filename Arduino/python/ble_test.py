import asyncio
from bleak import BleakClient

# デバイスUUID
DEVICE_UUID = "9108929D-B8E4-0946-232F-7EE1DDD2654C"
# キャラクタリスティックUUID
CHARACTERISTIC_UUID = "2A56"


# データ受信時のコールバック関数
def notification_handler(sender, data):
    print(f"Notification from {sender}: {data.hex().upper()}")  # 16進数として出力


async def run():
    async with BleakClient(DEVICE_UUID) as client:
        print(f"Connected: {client.is_connected}")

        # キャラクタリスティックの通知を有効化
        await client.start_notify(CHARACTERISTIC_UUID, notification_handler)
        await asyncio.sleep(30)  # 30秒間通知を受け取る
        await client.stop_notify(CHARACTERISTIC_UUID)


loop = asyncio.get_event_loop()
loop.run_until_complete(run())
