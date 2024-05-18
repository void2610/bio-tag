using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
public class AwakeController : MonoBehaviour
{
    void Awake()
    {
        string[] args = Environment.GetCommandLineArgs();
        string portNum = "7777";
        string serverAddress = "127.0.0.1";
        bool isListenServer = false;
        bool isServer = false;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-server")
            {
                Debug.Log("found -server");
                isServer = true;
                // -server の次がある前提
                serverAddress = args[i + 1];
            }
            if (args[i] == "-p")
            {
                Debug.Log("found -p");
                // -p の次がある前提
                portNum = args[i + 1];
            }
            if (args[i] == "-listen")
            {
                Debug.Log("found -listen");
                // -listen の次がある前提
                isListenServer = (args[i + 1] == "1");
            }
        }
        if (isServer)
        {
            // Invoke("startHostFn", 1.0f); //invoke時に引数でポートを渡すようにしてみる
            StartCoroutine(startHostFn(1.0f, serverAddress, Convert.ToUInt16(portNum), isListenServer));
        }
        else
        {
            //パラメータがない場合、1秒後にクライアントとして動作させる場合
            //StartCoroutine(startClientFn(1.0f));
        };
    }
    IEnumerator startHostFn(float delay, string serverAddress, ushort portNum, bool isListenServer)
    {
        yield return new WaitForSeconds(delay);
        if (isListenServer)
        {
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(
                serverAddress,  // The IP address is a string
                portNum, // The port number is an unsigned short
                "0.0.0.0"
            );
        }
        else
        {
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(
                serverAddress,  // The IP address is a string
                portNum // The port number is an unsigned short
            );
        }
        Debug.Log("started as Host: " + serverAddress + " Port:" + portNum + " isListenSever:" + isListenServer);
        NetworkManager.Singleton.StartHost();
    }
    IEnumerator startClientFn(float delay)
    {
        yield return new WaitForSeconds(delay);
        NetworkManager.Singleton.StartClient();
    }
}
