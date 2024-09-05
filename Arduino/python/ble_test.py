import asyncio
from bleak import BleakClient

# デバイスUUID
DEVICE_UUID = "9108929D-B8E4-0946-232F-7EE1DDD2654C"
# キャラクタリスティックUUID
CHARACTERISTIC_UUID = "2A56"


# データ受信時のコールバック関数
async def notification_handler(sender, data):
    if data.hex().upper() != "5354415254":
        print(data.hex().upper())  # 16進数として出力


async def run():
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


loop = asyncio.get_event_loop()
loop.run_until_complete(run())
