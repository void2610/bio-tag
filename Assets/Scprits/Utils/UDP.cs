using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class UDP : MonoBehaviour
{
    public static UDP instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public bool isConcentrated = false;

    static UdpClient udp;
    IPEndPoint remoteEP = null;

    private float value = 0;
    private int count = 0;
    private string REMOTE_IP = "127.0.0.1"; // 受信側のIPアドレス
    private int REMOTE_PORT = 50008; // 受信側のポート
    int LOCA_LPORT = 50007;

    // Use this for initialization
    void Start()
    {

        udp = new UdpClient(LOCA_LPORT);
        udp.Client.ReceiveTimeout = 2000;

        // 非同期で受信開始
        udp.BeginReceive(new AsyncCallback(ReceiveCallback), null);
    }

    // 追加: 数値を送信するメソッド
    public void SendData(float value)
    {
        string message = value.ToString();
        byte[] data = Encoding.UTF8.GetBytes(message);
        udp.Send(data, data.Length, REMOTE_IP, REMOTE_PORT);
    }

    // 非同期受信のコールバック
    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            byte[] data = udp.EndReceive(ar, ref remoteEP);
            string text = Encoding.UTF8.GetString(data);
            value = float.Parse(text);
            count++;

            // 再度受信を開始
            udp.BeginReceive(new AsyncCallback(ReceiveCallback), null);
        }
        catch (SocketException e)
        {
            Debug.LogError("SocketException: " + e.Message);
        }
    }

    private void Update()
    {
        if (count > 0)
        {
            GSRGraph.instance.AddData(value);
            count--;
        }

    }

    private void OnDestroy()
    {
        udp.Close();
    }
}
