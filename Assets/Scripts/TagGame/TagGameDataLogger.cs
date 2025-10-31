using System;
using System.Collections.Generic;
using UnityEngine;
using Experiment;

namespace TagGame
{
    /// <summary>
    /// 鬼ごっこゲームのデータロギングを管理するクラス
    /// </summary>
    public class TagGameDataLogger : IDisposable
    {
        private ExperimentSession _session;
        private CsvWriter<GameSummary> _gameSummaryWriter;
        private CsvWriter<GameEventRecord> _gameEventWriter;

        private TagGameSessionInfo _sessionInfo;
        private float _gameStartTime;
        private int _itChangeCount;
        private readonly Dictionary<int, float> _playerItTimeStart = new();
        private readonly Dictionary<int, float> _playerTotalItTime = new();

        /// <summary>
        /// セッション開始
        /// </summary>
        public void StartSession(TagGameSessionInfo sessionInfo)
        {
            _sessionInfo = sessionInfo;
            _sessionInfo.datetime = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");

            // セッション名を生成
            var sessionName = $"{sessionInfo.participantID}_{sessionInfo.gameMode}_{sessionInfo.testType}";

            // ExperimentSessionを作成
            _session = new ExperimentSession(sessionName);

            // セッション情報をJSONで保存
            _session.SaveJson("session.json", _sessionInfo);

            // CSVファイルの初期化
            InitializeCsvFiles();

            Debug.Log($"[TagGameLog] Session started: {_session.SessionDirectory}");
        }

        /// <summary>
        /// CSVファイルの初期化
        /// </summary>
        private void InitializeCsvFiles()
        {
            _gameSummaryWriter = _session.CreateCsvWriter("game_summary.csv", new GameSummary());
            _gameEventWriter = _session.CreateCsvWriter("game_events.csv", new GameEventRecord());
        }

        /// <summary>
        /// ゲーム開始を記録
        /// </summary>
        public void RecordGameStart(int initialItIndex, List<Vector3> playerPositions)
        {
            _gameStartTime = Time.time;
            _itChangeCount = 0;
            _playerItTimeStart.Clear();
            _playerTotalItTime.Clear();

            // 初期鬼の時間計測開始
            _playerItTimeStart[initialItIndex] = Time.time;

            // ゲーム開始イベントを記録
            RecordEvent("GameStart", initialItIndex, playerPositions, 0f, false);

            Debug.Log($"[TagGameLog] Game started. Initial It: Player{initialItIndex}");
        }

        /// <summary>
        /// 鬼交代イベントを記録
        /// </summary>
        public void RecordItChange(int newItIndex, List<Vector3> playerPositions, float gsrRaw, bool isExcited)
        {
            // 前の鬼の時間を記録
            foreach (var kvp in _playerItTimeStart)
            {
                var playerIndex = kvp.Key;
                var startTime = kvp.Value;
                var duration = Time.time - startTime;

                if (!_playerTotalItTime.ContainsKey(playerIndex))
                    _playerTotalItTime[playerIndex] = 0;

                _playerTotalItTime[playerIndex] += duration;
            }
            _playerItTimeStart.Clear();

            // 新しい鬼の時間計測開始
            _playerItTimeStart[newItIndex] = Time.time;
            _itChangeCount++;

            // イベントを記録
            RecordEvent("ItChanged", newItIndex, playerPositions, gsrRaw, isExcited);

            Debug.Log($"[TagGameLog] It changed to Player{newItIndex}. Total changes: {_itChangeCount}");
        }

        /// <summary>
        /// 定期的なゲーム状態を記録（バイオメトリック + 位置）
        /// </summary>
        public void RecordGameTick(int currentItIndex, List<Vector3> playerPositions, float gsrRaw, bool isExcited)
        {
            RecordEvent("Tick", currentItIndex, playerPositions, gsrRaw, isExcited);
        }

        /// <summary>
        /// イベントを記録する共通メソッド
        /// </summary>
        private void RecordEvent(string eventType, int currentItIndex, List<Vector3> playerPositions, float gsrRaw, bool isExcited)
        {
            var timestamp = (int)((Time.time - _gameStartTime) * 1000); // ミリ秒

            var record = new GameEventRecord
            {
                ParticipantID = _sessionInfo.participantID,
                TimestampMS = timestamp,
                EventType = eventType,
                CurrentItIndex = currentItIndex,
                Player0PosX = playerPositions.Count > 0 ? playerPositions[0].x : 0,
                Player0PosZ = playerPositions.Count > 0 ? playerPositions[0].z : 0,
                Player1PosX = playerPositions.Count > 1 ? playerPositions[1].x : 0,
                Player1PosZ = playerPositions.Count > 1 ? playerPositions[1].z : 0,
                GsrRaw = gsrRaw,
                IsExcited = isExcited
            };

            _gameEventWriter.WriteRecord(record);
        }

        /// <summary>
        /// ゲーム終了を記録
        /// </summary>
        public void RecordGameEnd(List<string> playerNames, List<float> playerScores, List<Vector3> playerPositions)
        {
            // 最後の鬼の時間を記録
            foreach (var kvp in _playerItTimeStart)
            {
                var playerIndex = kvp.Key;
                var startTime = kvp.Value;
                var duration = Time.time - startTime;

                if (!_playerTotalItTime.ContainsKey(playerIndex))
                    _playerTotalItTime[playerIndex] = 0;

                _playerTotalItTime[playerIndex] += duration;
            }

            var gameDuration = Time.time - _gameStartTime;

            // ゲーム終了イベントを記録
            RecordEvent("GameEnd", -1, playerPositions, 0f, false);

            // ゲームサマリーを記録
            var summary = new GameSummary
            {
                ParticipantID = _sessionInfo.participantID,
                GameMode = _sessionInfo.gameMode.ToString(),
                GameDurationSeconds = gameDuration,
                ItChangeCount = _itChangeCount,
                Player0Name = playerNames.Count > 0 ? playerNames[0] : "",
                Player0Score = playerScores.Count > 0 ? playerScores[0] : 0,
                Player0ItTimeSeconds = _playerTotalItTime.ContainsKey(0) ? _playerTotalItTime[0] : 0,
                Player1Name = playerNames.Count > 1 ? playerNames[1] : "",
                Player1Score = playerScores.Count > 1 ? playerScores[1] : 0,
                Player1ItTimeSeconds = _playerTotalItTime.ContainsKey(1) ? _playerTotalItTime[1] : 0,
                Player2Name = playerNames.Count > 2 ? playerNames[2] : "",
                Player2Score = playerScores.Count > 2 ? playerScores[2] : 0,
                Player2ItTimeSeconds = _playerTotalItTime.ContainsKey(2) ? _playerTotalItTime[2] : 0
            };

            _gameSummaryWriter.WriteRecord(summary);
            _gameSummaryWriter.Flush();

            Debug.Log($"[TagGameLog] Game ended. Duration: {gameDuration:F2}s, It changes: {_itChangeCount}");
        }

        /// <summary>
        /// セッション終了とクリーンアップ
        /// </summary>
        public void EndSession()
        {
            _session?.Dispose();
            Debug.Log($"[TagGameLog] Session ended. Data saved to: {_session?.SessionDirectory}");
        }

        /// <summary>
        /// リソースを解放
        /// </summary>
        public void Dispose()
        {
            EndSession();
        }
    }
}
