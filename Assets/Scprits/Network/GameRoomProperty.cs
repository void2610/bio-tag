using ExitGames.Client.Photon;
using Photon.Realtime;

public static class GameRoomProperty
{
    public const string KEY_START_TIME = "s";
    public const string KEY_GAME_STATE = "g";
    public const string KEY_IT_INDEX = "i";
    public const string LAST_TAG_TIME_INDEX = "l";

    private static readonly Hashtable propsToSet = new Hashtable();

    private static void SetCustomProperties(Room room)
    {
        room.SetCustomProperties(propsToSet);
        propsToSet.Clear();
    }

    public static bool TryGetGameState(this Room room, out int gameState)
    {
        if (room.CustomProperties[KEY_GAME_STATE] is int value)
        {
            gameState = value;
            return true;
        }
        else
        {
            gameState = -1;
            return false;
        }
    }

    public static bool TryGetItIndex(this Room room, out int index)
    {
        if (room.CustomProperties[KEY_IT_INDEX] is int value)
        {
            index = value;
            return true;
        }
        else
        {
            index = -1;
            return false;
        }
    }

    public static bool TryGetStartTime(this Room room, out int timestamp)
    {
        if (room.CustomProperties[KEY_START_TIME] is int value)
        {
            timestamp = value;
            return true;
        }
        else
        {
            timestamp = 0;
            return false;
        }
    }

    public static bool TryGetLastTagTime(this Room room, out double time)
    {
        if (room.CustomProperties[LAST_TAG_TIME_INDEX] is double value)
        {
            time = value;
            return true;
        }
        else
        {
            time = 0;
            return false;
        }
    }

    public static void StartGame(this Room room, int timestamp)
    {
        propsToSet[KEY_START_TIME] = timestamp;
        propsToSet[KEY_GAME_STATE] = 1;

        SetCustomProperties(room);
    }

    public static void EndGame(this Room room)
    {
        propsToSet[KEY_GAME_STATE] = 2;

        SetCustomProperties(room);
    }

    public static void SetItIndex(this Room room, int index, double time)
    {
        propsToSet[KEY_IT_INDEX] = index;
        propsToSet[LAST_TAG_TIME_INDEX] = time;

        SetCustomProperties(room);
    }
}
