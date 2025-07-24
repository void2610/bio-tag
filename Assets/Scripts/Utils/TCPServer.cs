using UnityEngine;
using Cysharp.Threading.Tasks;    // UniTask
using R3;                        // Reactive Extensions for Unity (R3)
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public sealed class TcpServer : MonoBehaviour
{
    public static TcpServer Instance { get; private set; }
    public Observable<int> OnReceive => _onReceive;       // 外部公開（Subscribe 可能）
    public ReadOnlyReactiveProperty<int> LastValue => _lastValue;
    public ReadOnlyReactiveProperty<bool> IsConnected => _isConnected;  // リアクティブに購読
    
    private readonly Subject<int> _onReceive = new();      // 受信した int を流す
    private readonly ReactiveProperty<bool> _isConnected = new(false);
    private readonly ReactiveProperty<int> _lastValue = new(-1);

    // ────────── Lifecycle ──────────
    private CancellationTokenSource _cts;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        _cts = new CancellationTokenSource();
        // fire-and-forget で待ち受けループ開始
        ListenLoopAsync(_cts.Token).Forget();
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
        _onReceive.OnCompleted();
    }

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
                
                _isConnected.Value = true;         // 接続
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
            _isConnected.Value = false;        // 切断
            Debug.Log("TCP サーバー停止");
        }
    }

    /// <summary>1 クライアントとの受信ループ</summary>
    private async UniTaskVoid HandleClientAsync(TcpClient client, CancellationToken token)
    {
        await using var stream = client.GetStream();
        var buffer = new byte[512];

        try
        {
            while (!token.IsCancellationRequested && client.Connected)
            {
                int n = await stream.ReadAsync(buffer, 0, buffer.Length, token).AsUniTask();
                if (n == 0) break;                      // 切断

                var msg = Encoding.UTF8.GetString(buffer, 0, n);
                if (int.TryParse(msg, out var v))
                {
                    _lastValue.Value = v;               // 最新値を保持
                    _onReceive.OnNext(v);               // ストリームに流す
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
            client.Close();
            Debug.Log("クライアント切断");
        }
    }
}
