using System;
using Experiment;

namespace TagGame
{
    /// <summary>
    /// ゲームモードの定義
    /// </summary>
    public enum GameMode
    {
        PlayerVsPlayer,  // プレイヤー対戦
        PlayerVsNPC      // NPC対戦
    }

    /// <summary>
    /// 鬼ごっこゲームのセッション情報（JSON形式で保存）
    /// </summary>
    [Serializable]
    public class TagGameSessionInfo
    {
        public string participantID;
        public string experimentGroup;
        public int trialNumber;
        public GameMode gameMode;
        public int playerCount;
        public int npcCount;
        public float gameLengthSeconds;
        public string datetime;
        public float roomTemperature;
        public float roomHumidity;
    }

    /// <summary>
    /// ゲーム全体のサマリーデータ（CSV形式で保存）
    /// </summary>
    public class GameSummary : ICsvSerializable
    {
        public string ParticipantID;
        public string GameMode;
        public float GameDurationSeconds;
        public int ItChangeCount;
        public string Player0Name;
        public float Player0Score;
        public float Player0ItTimeSeconds;
        public string Player1Name;
        public float Player1Score;
        public float Player1ItTimeSeconds;
        public string Player2Name;
        public float Player2Score;
        public float Player2ItTimeSeconds;

        public string GetCsvHeader() =>
            "participant_id,game_mode,game_duration_seconds,it_change_count," +
            "player0_name,player0_score,player0_it_time_seconds," +
            "player1_name,player1_score,player1_it_time_seconds," +
            "player2_name,player2_score,player2_it_time_seconds";

        public string ToCsvRow() =>
            $"{ParticipantID},{GameMode},{GameDurationSeconds:F2},{ItChangeCount}," +
            $"{Player0Name},{Player0Score:F1},{Player0ItTimeSeconds:F2}," +
            $"{Player1Name},{Player1Score:F1},{Player1ItTimeSeconds:F2}," +
            $"{Player2Name},{Player2Score:F1},{Player2ItTimeSeconds:F2}";
    }

    /// <summary>
    /// ゲームイベントの時系列記録（CSV形式で保存）
    /// </summary>
    public class GameEventRecord : ICsvSerializable
    {
        public string ParticipantID;
        public int TimestampMS;
        public string EventType;  // "GameStart", "ItChanged", "GameEnd", "Tick"
        public int CurrentItIndex;
        public float Player0PosX;
        public float Player0PosZ;
        public float Player1PosX;
        public float Player1PosZ;
        public float GsrRaw;
        public float GsrDerivative;
        public float GsrThreshold;
        public bool IsExcited;

        public string GetCsvHeader() =>
            "participant_id,timestamp_ms,event_type,current_it_index," +
            "player0_pos_x,player0_pos_z,player1_pos_x,player1_pos_z," +
            "gsr_raw,gsr_derivative,gsr_threshold,is_excited";

        public string ToCsvRow() =>
            $"{ParticipantID},{TimestampMS},{EventType},{CurrentItIndex}," +
            $"{Player0PosX:F2},{Player0PosZ:F2},{Player1PosX:F2},{Player1PosZ:F2}," +
            $"{GsrRaw:F2},{GsrDerivative:F2},{GsrThreshold:F2},{IsExcited}";
    }
}
