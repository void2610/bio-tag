using ExitGames.Client.Photon;
using Photon.Realtime;

public static class GameRoomProperty
{
    public const string KeyStartTime = "s";
    public const string KeyGameState = "g";
    public const string KeyItIndex = "i";
    public const string LastTagTimeIndex = "l";

    private static readonly Hashtable propsToSet = new Hashtable();

    private static void SetCustomProperties(Room room)
    {
        room.SetCustomProperties(propsToSet);
        propsToSet.Clear();
    }

    public static bool TryGetGameState(this Room room, out int gameState)
    {
        if (room.CustomProperties[KeyGameState] is int value)
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
        if (room.CustomProperties[KeyItIndex] is int value)
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
        if (room.CustomProperties[KeyStartTime] is int value)
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
        if (room.CustomProperties[LastTagTimeIndex] is double value)
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
        propsToSet[KeyStartTime] = timestamp;
        propsToSet[KeyGameState] = 1;

        SetCustomProperties(room);
    }

    public static void EndGame(this Room room)
    {
        propsToSet[KeyGameState] = 2;

        SetCustomProperties(room);
    }

    public static void SetItIndex(this Room room, int index, double time)
    {
        propsToSet[KeyItIndex] = index;
        propsToSet[LastTagTimeIndex] = time;

        SetCustomProperties(room);
    }
}
