using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.IO.Ports;
using System.Threading;
using VContainer.Unity;
using VitalRouter;
using BioTag.Biometric;

/// <summary>
/// シリアル通信サーバーサービス
/// VContainerで管理され、Arduino UnoからUSB経由でGSRデータを受信
/// VitalRouter Commandで配信
/// </summary>
public sealed class SerialServer : IStartable, IDisposable
{
    public bool IsConnected { get; private set; }

    private readonly string _portName;
    private readonly int _baudRate;
    private SerialPort _serialPort;
    private CancellationTokenSource _cts;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="portName">シリアルポート名（例: "/dev/cu.usbmodem14201" or "COM3"）</param>
    /// <param name="baudRate">ボーレート（デフォルト: 115200）</param>
    public SerialServer(string portName, int baudRate = 115200)
    {
        _portName = portName;
        _baudRate = baudRate;
    }

    /// <summary>シリアルポートからデータを読み取るループ</summary>
    private async UniTaskVoid ReadLoopAsync(CancellationToken token)
    {
        try
        {
            // シリアルポートを開く
            _serialPort = new SerialPort(_portName, _baudRate)
            {
                ReadTimeout = 1000,
                WriteTimeout = 1000,
                DtrEnable = true,  // Arduinoとの通信に必要
                RtsEnable = true
            };

            _serialPort.Open();
            IsConnected = true;
            Debug.Log($"シリアルポート接続成功: {_portName} @ {_baudRate}bps");

            // データ受信ループ
            while (!token.IsCancellationRequested && _serialPort.IsOpen)
            {
                try
                {
                    // 1行読み取り（改行までブロック）
                    // ReadLineAsyncがないため、TaskでラップしてUniTaskに変換
                    var line = await UniTask.RunOnThreadPool(() =>
                    {
                        return _serialPort.ReadLine();
                    }, cancellationToken: token);

                    // 空白をトリム
                    line = line.Trim();

                    // 整数値にパース
                    if (int.TryParse(line, out var value))
                    {
                        // VitalRouterでコマンド発行
                        await Router.Default.PublishAsync(new GsrDataReceivedCommand(value));
                    }
                    else
                    {
                        Debug.LogWarning($"シリアルデータ解析失敗: {line}");
                    }
                }
                catch (TimeoutException)
                {
                    // ReadLineがタイムアウトした場合は無視して続行
                    await UniTask.Yield();
                }
                catch (InvalidOperationException ex)
                {
                    // ポートが閉じられた
                    Debug.LogWarning($"シリアルポートが閉じられました: {ex.Message}");
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // 正常なキャンセル
            Debug.Log("シリアル受信ループをキャンセルしました");
        }
        catch (UnauthorizedAccessException ex)
        {
            Debug.LogError($"シリアルポートへのアクセスが拒否されました: {ex.Message}");
            Debug.LogError($"ポート {_portName} が他のアプリケーション（Arduino IDEなど）で使用中の可能性があります");
        }
        catch (Exception ex)
        {
            Debug.LogError($"シリアル通信エラー: {ex}");
        }
        finally
        {
            IsConnected = false;
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
            }
            Debug.Log("シリアルポート切断");
        }
    }

    public void Start()
    {
        _cts = new CancellationTokenSource();
        ReadLoopAsync(_cts.Token).Forget();
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();

        if (_serialPort != null && _serialPort.IsOpen)
        {
            _serialPort.Close();
        }
        _serialPort?.Dispose();
    }
}
