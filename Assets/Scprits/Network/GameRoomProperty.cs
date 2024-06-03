using ExitGames.Client.Photon;
using Photon.Realtime;

public static class GameRoomProperty
{
    private const string KeyStartTime = "s";
    private const string KeyOnGame = "g";
    private const string KeyItIndex = "i";

    private static readonly Hashtable propsToSet = new Hashtable();

    public static bool GetOnGame(this Room room)
    {
        return room.CustomProperties[KeyOnGame] is bool value && value;
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

    public static void StartGame(this Room room, int timestamp)
    {
        propsToSet[KeyStartTime] = timestamp;
        propsToSet[KeyOnGame] = true;
        room.SetCustomProperties(propsToSet);
        propsToSet.Clear();
    }

    public static void SetItIndex(this Room room, int index)
    {
        propsToSet[KeyItIndex] = index;
        room.SetCustomProperties(propsToSet);
        propsToSet.Clear();
    }
}
