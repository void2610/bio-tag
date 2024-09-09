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
    static UdpClient udp;
    IPEndPoint remoteEP = null;
    int i = 0;

    // Use this for initialization
    void Start()
    {
        int LOCA_LPORT = 50007;
        udp = new UdpClient(LOCA_LPORT);
        udp.Client.ReceiveTimeout = 2000;

        // 非同期で受信開始
        udp.BeginReceive(new AsyncCallback(ReceiveCallback), null);
    }

    // 非同期受信のコールバック
    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            byte[] data = udp.EndReceive(ar, ref remoteEP);
            string text = Encoding.UTF8.GetString(data);
            Debug.Log(text);

            // 再度受信を開始
            udp.BeginReceive(new AsyncCallback(ReceiveCallback), null);
        }
        catch (SocketException e)
        {
            Debug.LogError("SocketException: " + e.Message);
        }
    }
}
