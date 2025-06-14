using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace UnityMCP
{
    [System.Serializable]
    public class CompilationResponse
    {
        public string status;
        public string timestamp;
        public int duration_ms;
        public int error_count;
        public int warning_count;
    }

    [System.Serializable]
    public class CompileActionResponse
    {
        public string status;
        public string timestamp;
    }

    [System.Serializable]
    public class DiagnosticsResponse
    {
        public CompilationMessage[] messages;
    }

    public class MCPServer
    {
        private HttpListener listener;
        private CompilationStatusTracker statusTracker;
        private CancellationTokenSource cancellationToken;
        private int port;
        private bool isRunning = false;

        public MCPServer(int port, CompilationStatusTracker statusTracker)
        {
            this.port = port;
            this.statusTracker = statusTracker;
        }

        public void Start()
        {
            if (isRunning) return;

            try
            {
                listener = new HttpListener();
                listener.Prefixes.Add($"http://localhost:{port}/");
                listener.Start();
                
                cancellationToken = new CancellationTokenSource();
                isRunning = true;
                
                Task.Run(async () => await StartServerAsync(), cancellationToken.Token);
                Debug.Log($"MCP Server started on http://localhost:{port}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to start server: {e.Message}");
                throw;
            }
        }

        public void Stop()
        {
            if (!isRunning) return;

            try
            {
                cancellationToken?.Cancel();
                listener?.Stop();
                listener?.Close();
                isRunning = false;
                Debug.Log("MCP Server stopped");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error stopping server: {e.Message}");
            }
        }

        private async Task StartServerAsync()
        {
            while (!cancellationToken.Token.IsCancellationRequested && listener.IsListening)
            {
                try
                {
                    var context = await listener.GetContextAsync();
                    _ = Task.Run(() => ProcessRequest(context), cancellationToken.Token);
                }
                catch (ObjectDisposedException)
                {
                    // Expected when stopping the server
                    break;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Server error: {e.Message}");
                }
            }
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            try
            {
                var request = context.Request;
                var response = context.Response;
                
                // Enable CORS
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");

                if (request.HttpMethod == "OPTIONS")
                {
                    response.StatusCode = 200;
                    response.Close();
                    return;
                }

                string responseString = "";
                response.ContentType = "application/json";

                switch (request.Url.AbsolutePath.ToLower())
                {
                    case "/api/compile":
                        if (request.HttpMethod == "POST")
                        {
                            responseString = HandleCompileRequest();
                        }
                        else
                        {
                            response.StatusCode = 405; // Method Not Allowed
                            responseString = "{\"error\":\"Method not allowed\"}";
                        }
                        break;

                    case "/api/compile_status":
                        if (request.HttpMethod == "GET")
                        {
                            responseString = HandleCompileStatusRequest();
                        }
                        else
                        {
                            response.StatusCode = 405;
                            responseString = "{\"error\":\"Method not allowed\"}";
                        }
                        break;

                    case "/api/diagnostics":
                        if (request.HttpMethod == "GET")
                        {
                            responseString = HandleDiagnosticsRequest();
                        }
                        else
                        {
                            response.StatusCode = 405;
                            responseString = "{\"error\":\"Method not allowed\"}";
                        }
                        break;

                    default:
                        response.StatusCode = 404;
                        responseString = "{\"error\":\"Endpoint not found\"}";
                        break;
                }

                byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                response.OutputStream.Write(buffer, 0, buffer.Length);
                response.Close();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error processing request: {e.Message}");
                try
                {
                    context.Response.StatusCode = 500;
                    string errorResponse = $"{{\"error\":\"{e.Message}\"}}";
                    byte[] errorBuffer = Encoding.UTF8.GetBytes(errorResponse);
                    context.Response.OutputStream.Write(errorBuffer, 0, errorBuffer.Length);
                    context.Response.Close();
                }
                catch
                {
                    // Best effort error handling
                }
            }
        }

        private string HandleCompileRequest()
        {
            try
            {
                // Trigger compilation on main thread
                EditorApplication.delayCall += () => {
                    UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
                };

                var response = new CompileActionResponse
                {
                    status = "started",
                    timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
                };

                return JsonUtility.ToJson(response, true);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error handling compile request: {e.Message}");
                return $"{{\"error\":\"{e.Message}\"}}";
            }
        }

        private string HandleCompileStatusRequest()
        {
            try
            {
                var response = new CompilationResponse
                {
                    status = statusTracker.CurrentStatus.ToString().ToLower(),
                    timestamp = statusTracker.LastCompletionTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                    duration_ms = statusTracker.LastCompilationDurationMs,
                    error_count = statusTracker.ErrorCount,
                    warning_count = statusTracker.WarningCount
                };

                return JsonUtility.ToJson(response, true);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error handling compile status request: {e.Message}");
                return $"{{\"error\":\"{e.Message}\"}}";
            }
        }

        private string HandleDiagnosticsRequest()
        {
            try
            {
                // JsonUtility doesn't support arrays directly, so we need to wrap it
                var messagesWrapper = new { messages = statusTracker.Messages.ToArray() };
                
                // Build JSON manually since JsonUtility has limitations with arrays
                var sb = new StringBuilder();
                sb.Append("{\"messages\":[");
                
                for (int i = 0; i < statusTracker.Messages.Count; i++)
                {
                    if (i > 0) sb.Append(",");
                    sb.Append(JsonUtility.ToJson(statusTracker.Messages[i]));
                }
                
                sb.Append("]}");
                return sb.ToString();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error handling diagnostics request: {e.Message}");
                return $"{{\"error\":\"{e.Message}\"}}";
            }
        }
    }
}