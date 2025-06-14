using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;

namespace UnityMCP
{
    public enum CompilationStatus
    {
        Idle,
        InProgress,
        Completed,
        Failed
    }

    [System.Serializable]
    public class CompilationMessage
    {
        public string type;
        public string message;
        public string file;
        public int line;
        public string timestamp;

        public CompilationMessage(string type, string message, string file = "", int line = 0)
        {
            this.type = type;
            this.message = message;
            this.file = file;
            this.line = line;
            this.timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        }
    }

    public class CompilationStatusTracker : IDisposable
    {
        public CompilationStatus CurrentStatus { get; private set; } = CompilationStatus.Idle;
        public List<CompilationMessage> Messages { get; private set; } = new List<CompilationMessage>();
        public DateTime LastCompletionTime { get; private set; } = DateTime.UtcNow;
        public int LastCompilationDurationMs { get; private set; } = 0;
        public int ErrorCount => GetMessageCount("error");
        public int WarningCount => GetMessageCount("warning");

        private DateTime compilationStartTime;
        private bool isCollectingMessages = false;

        public CompilationStatusTracker()
        {
            // Subscribe to compilation events
            CompilationPipeline.compilationStarted += OnCompilationStarted;
            CompilationPipeline.compilationFinished += OnCompilationFinished;
            
            // Subscribe to log messages to catch errors and warnings
            Application.logMessageReceived += OnLogMessageReceived;
            
            Debug.Log("CompilationStatusTracker initialized");
        }

        public void Dispose()
        {
            // Unsubscribe from events
            CompilationPipeline.compilationStarted -= OnCompilationStarted;
            CompilationPipeline.compilationFinished -= OnCompilationFinished;
            Application.logMessageReceived -= OnLogMessageReceived;
            
            Debug.Log("CompilationStatusTracker disposed");
        }

        private void OnCompilationStarted(object obj)
        {
            CurrentStatus = CompilationStatus.InProgress;
            compilationStartTime = DateTime.UtcNow;
            isCollectingMessages = true;
            
            // Clear previous messages
            Messages.Clear();
            
            Debug.Log("Compilation started");
        }

        private void OnCompilationFinished(object obj)
        {
            var endTime = DateTime.UtcNow;
            LastCompilationDurationMs = (int)(endTime - compilationStartTime).TotalMilliseconds;
            LastCompletionTime = endTime;
            isCollectingMessages = false;
            
            // Determine if compilation succeeded or failed based on error count
            CurrentStatus = ErrorCount > 0 ? CompilationStatus.Failed : CompilationStatus.Completed;
            
            Debug.Log($"Compilation finished. Status: {CurrentStatus}, Duration: {LastCompilationDurationMs}ms, Errors: {ErrorCount}, Warnings: {WarningCount}");
        }

        private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            // Only collect messages during compilation
            if (!isCollectingMessages) return;

            string messageType = "";
            switch (type)
            {
                case LogType.Error:
                case LogType.Exception:
                    messageType = "error";
                    break;
                case LogType.Warning:
                    messageType = "warning";
                    break;
                case LogType.Log:
                    messageType = "log";
                    break;
                default:
                    return; // Skip other types
            }

            // Parse file and line information from the condition if available
            string fileName = "";
            int lineNumber = 0;
            ParseFileAndLineFromMessage(condition, out fileName, out lineNumber);

            var message = new CompilationMessage(messageType, condition, fileName, lineNumber);
            Messages.Add(message);
            
            // Limit the number of stored messages to prevent memory issues
            if (Messages.Count > 1000)
            {
                Messages.RemoveAt(0);
            }
        }

        private void ParseFileAndLineFromMessage(string message, out string fileName, out int lineNumber)
        {
            fileName = "";
            lineNumber = 0;

            try
            {
                // Try to parse Unity's standard error format: "Assets/Scripts/File.cs(42,5): error CS0246: ..."
                if (message.Contains("(") && message.Contains("):"))
                {
                    int fileStart = message.IndexOf("Assets/");
                    if (fileStart >= 0)
                    {
                        int fileEnd = message.IndexOf("(", fileStart);
                        if (fileEnd > fileStart)
                        {
                            fileName = message.Substring(fileStart, fileEnd - fileStart);
                            
                            int lineStart = fileEnd + 1;
                            int lineEnd = message.IndexOf(",", lineStart);
                            if (lineEnd == -1) lineEnd = message.IndexOf(")", lineStart);
                            
                            if (lineEnd > lineStart)
                            {
                                string lineStr = message.Substring(lineStart, lineEnd - lineStart);
                                int.TryParse(lineStr, out lineNumber);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error parsing file and line from message: {e.Message}");
            }
        }

        private int GetMessageCount(string messageType)
        {
            int count = 0;
            foreach (var message in Messages)
            {
                if (message.type == messageType)
                    count++;
            }
            return count;
        }

        public void TriggerCompilation()
        {
            CompilationPipeline.RequestScriptCompilation();
        }
    }
}
