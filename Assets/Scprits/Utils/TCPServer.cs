using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System;

public class TcpServer : MonoBehaviour
{
    //シングルトン実装
    public static TcpServer Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);
    }

    private int _value = -1;
    private TcpListener _tcpListener = null;
    private TcpClient _tcpClient = null;
    private NetworkStream _networkStream = null;

    public bool IsConnected() => _value != -1;

    public int GetValue() => _value;

    private async void Start()
    {
        try
        {
            await Task.Run(OnProcess);
        }
        catch (Exception ex)
        {
            Debug.LogError("サーバー処理中の例外: " + ex);
        }
    }
    
    private void OnProcess()
    {
        Debug.Log("サーバー起動");
        var ipAddress = IPAddress.Parse("100.64.1.17");
        _tcpListener = new TcpListener(ipAddress, 10001);
        _tcpListener.Start();
        Debug.Log("接続待機中");
        _tcpClient = _tcpListener.AcceptTcpClient();
        Debug.Log("接続完了");
        _networkStream = _tcpClient.GetStream();

        while (true)
        {
            try
            {
                var buffer = new byte[512];
                var count = _networkStream.Read(buffer, 0, buffer.Length);

                if (count == 0)
                {
                    Debug.Log("切断再試行");
                    Task.Run(OnProcess);
                    break;
                }
                else
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, count);
                    if (int.TryParse(message, out var result)) _value = result;
                    else _value = -1;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("エラー: " + ex.Message);
                if (ex is System.IO.IOException)
                {
                    Debug.Log("切断");
                    Destroy(this.gameObject);
                }
            }
        }
    }

    private void OnDestroy()
    {
        _networkStream?.Dispose();
        _tcpClient?.Dispose();
        _tcpListener?.Stop();
    }
}