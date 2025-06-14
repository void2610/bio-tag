using UnityEngine;
using UnityEditor;
using System;

namespace UnityMCP
{
    public class UnityMCPWindow : EditorWindow
    {
        private MCPServer mcpServer;
        private CompilationStatusTracker statusTracker;
        private bool isServerRunning = false;
        private int serverPort = 8080;
        private string serverStatus = "Stopped";

        [MenuItem("Tools/Unity MCP Server")]
        public static void ShowWindow()
        {
            GetWindow<UnityMCPWindow>("Unity MCP Server");
        }

        void OnEnable()
        {
            StartMCPServer();
        }

        void OnDisable()
        {
            StopMCPServer();
        }

        void OnDestroy()
        {
            StopMCPServer();
        }

        void OnGUI()
        {
            GUILayout.Label("Unity MCP Server", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            
            GUILayout.Label($"Status: {serverStatus}");
            GUILayout.Label($"Port: {serverPort}");
            
            if (statusTracker != null)
            {
                GUILayout.Label($"Compilation Status: {statusTracker.CurrentStatus}");
                GUILayout.Label($"Error Count: {statusTracker.ErrorCount}");
                GUILayout.Label($"Warning Count: {statusTracker.WarningCount}");
            }
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button(isServerRunning ? "Stop Server" : "Start Server"))
            {
                if (isServerRunning)
                    StopMCPServer();
                else
                    StartMCPServer();
            }
            
            if (GUILayout.Button("Trigger Compilation"))
            {
                TriggerCompilation();
            }
        }

        private void StartMCPServer()
        {
            try
            {
                statusTracker = new CompilationStatusTracker();
                mcpServer = new MCPServer(serverPort, statusTracker);
                mcpServer.Start();
                isServerRunning = true;
                serverStatus = "Running";
                Debug.Log("Unity MCP Server started on port " + serverPort);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to start MCP Server: " + e.Message);
                serverStatus = "Error: " + e.Message;
            }
        }

        private void StopMCPServer()
        {
            try
            {
                mcpServer?.Stop();
                statusTracker?.Dispose();
                isServerRunning = false;
                serverStatus = "Stopped";
                Debug.Log("Unity MCP Server stopped");
            }
            catch (Exception e)
            {
                Debug.LogError("Error stopping MCP Server: " + e.Message);
            }
        }

        private void TriggerCompilation()
        {
            if (statusTracker != null)
            {
                UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
                Debug.Log("Script compilation triggered");
            }
        }
    }
}