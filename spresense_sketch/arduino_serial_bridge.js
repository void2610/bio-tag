/**
 * Arduino Serial to TCP Bridge
 * ArduinoからUSBシリアル経由でGSRデータを受信し、TCPでUnityに転送
 */

const { SerialPort } = require("serialport");
const { ReadlineParser } = require("@serialport/parser-readline");
const net = require("net");
const os = require("os");

// ローカルマシンのIPアドレスを自動検出
function getLocalIP() {
  const interfaces = os.networkInterfaces();
  for (const name of Object.keys(interfaces)) {
    for (const iface of interfaces[name]) {
      // IPv4、内部アドレスでない（ループバックでない）
      if (iface.family === 'IPv4' && !iface.internal) {
        return iface.address;
      }
    }
  }
  return '127.0.0.1'; // フォールバック
}

// コマンドライン引数からシリアルポート番号を取得
// 使用例: node arduino_serial_bridge.js [serial_port_number]
// TCP_HOSTは常に自動検出
const args = process.argv.slice(2);
const TCP_HOST = getLocalIP(); // 常に自動検出
const portNumber = args[0] || "101"; // デフォルトは101

// 設定
const SERIAL_PORT = `/dev/cu.usbmodem${portNumber}`; // Arduinoのシリアルポート
const BAUD_RATE = 9600;
const TCP_PORT = 10001;

// TCPクライアント
let tcpClient = null;
let isConnected = false;

// TCP接続を確立
function connectToTCP() {
  tcpClient = new net.Socket();

  tcpClient.connect(TCP_PORT, TCP_HOST, () => {
    console.log(`✓ TCPサーバーに接続しました: ${TCP_HOST}:${TCP_PORT}`);
    isConnected = true;
  });

  tcpClient.on("error", (err) => {
    console.error("TCP接続エラー:", err.message);
    isConnected = false;
  });

  tcpClient.on("close", () => {
    console.log("TCP接続が切断されました。5秒後に再接続します...");
    isConnected = false;
    setTimeout(connectToTCP, 5000);
  });
}

// シリアルポートを開く
const port = new SerialPort({
  path: SERIAL_PORT,
  baudRate: BAUD_RATE,
});

const parser = port.pipe(new ReadlineParser({ delimiter: "\n" }));

port.on("open", () => {
  console.log(`✓ シリアルポートを開きました: ${SERIAL_PORT} @ ${BAUD_RATE}bps`);
  console.log("Arduinoからのデータ受信を開始します...\n");

  // TCP接続を開始
  connectToTCP();
});

port.on("error", (err) => {
  console.error("シリアルポートエラー:", err.message);
  process.exit(1);
});

// データ受信時の処理
parser.on("data", (line) => {
  const trimmed = line.trim();
  console.log(`受信: ${trimmed}`);

  // TCP経由でUnityに送信
  if (isConnected && tcpClient) {
    try {
      tcpClient.write(trimmed);
      console.log(`→ Unity TCPサーバーに送信: ${trimmed}`);
    } catch (err) {
      console.error("TCP送信エラー:", err.message);
    }
  } else {
    console.warn("⚠ TCPサーバーに未接続のため送信できません");
  }
});

// 終了処理
process.on("SIGINT", () => {
  console.log("\n終了します...");
  if (tcpClient) tcpClient.destroy();
  port.close(() => {
    console.log("シリアルポートを閉じました");
    process.exit(0);
  });
});

console.log("=== Arduino Serial to TCP Bridge ===");
console.log(`シリアルポート: ${SERIAL_PORT}`);
console.log(`ボーレート: ${BAUD_RATE}`);
console.log(`TCP送信先: ${TCP_HOST}:${TCP_PORT}`);
console.log("=====================================\n");
