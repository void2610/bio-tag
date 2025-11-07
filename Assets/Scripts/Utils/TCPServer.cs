using UnityEngine;
using Cysharp.Threading.Tasks;    // UniTask
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using VContainer.Unity;
using VitalRouter;
using BioTag.Biometric;

/// <summary>
/// TCPサーバーサービス (ポート10001)
/// VContainerで管理され、外部センサーからGSRデータを受信
/// VitalRouter Commandで配信、R3 Observableでも配信
/// </summary>
public sealed class TcpServer : IStartable, IDisposable
{
    private CancellationTokenSource _cts;

    /// <summary>クライアントが接続中かどうか</summary>
    public bool IsConnected { get; private set; } = false;

    // ────────── Core Logic ──────────
    /// <summary>TCPListener を立ててクライアント接続を待ち受けるループ</summary>
    private async UniTaskVoid ListenLoopAsync(CancellationToken token)
    {
        var listener = new TcpListener(IPAddress.Any, 10001);
        listener.Start();
        Debug.Log("TCP サーバー: 待ち受け開始 (port 10001)");

        try
        {
            while (!token.IsCancellationRequested)
            {
                // クライアントが来るまでノンブロッキングで待機
                // ★ AcceptTcpClientAsync ── token をひも付ける
                var client = await listener
                    .AcceptTcpClientAsync()          // Task<TcpClient>
                    .AsUniTask()                     // UniTask<TcpClient>
                    .AttachExternalCancellation(token);
                
                Debug.Log($"クライアント接続: {client.Client.RemoteEndPoint}");

                // 別タスクで受信処理（切断時に自動終了）
                HandleClientAsync(client, token).Forget();
            }
        }
        catch (OperationCanceledException) { /* 正常終了 */ }
        catch (Exception ex)
        {
            Debug.LogError($"TCP サーバー例外: {ex}");
        }
        finally
        {
            listener.Stop();
            Debug.Log("TCP サーバー停止");
        }
    }

    /// <summary>1 クライアントとの受信ループ</summary>
    private async UniTaskVoid HandleClientAsync(TcpClient client, CancellationToken token)
    {
        await using var stream = client.GetStream();
        var buffer = new byte[512];

        // クライアント接続時に状態を更新
        IsConnected = true;

        try
        {
            while (!token.IsCancellationRequested && client.Connected)
            {
                int n = await stream.ReadAsync(buffer, 0, buffer.Length, token).AsUniTask();
                if (n == 0) break;                      // 切断

                var msg = Encoding.UTF8.GetString(buffer, 0, n);
                if (int.TryParse(msg, out var v))
                {
                    // VitalRouter CommandでGSRデータを配信
                    await Router.Default.PublishAsync(new GsrDataReceivedCommand(v));
                }
                else
                {
                    Debug.LogWarning($"解析失敗: {msg}");
                }
            }
        }
        catch (OperationCanceledException) { /* 無視 */ }
        catch (Exception ex)
        {
            Debug.LogError($"クライアント処理例外: {ex}");
        }
        finally
        {
            IsConnected = false;
            client.Close();
            Debug.Log("クライアント切断");
        }
    }

    public void Start()
    {
        _cts = new CancellationTokenSource();
        // fire-and-forget で待ち受けループ開始
        ListenLoopAsync(_cts.Token).Forget();
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
    }
}
