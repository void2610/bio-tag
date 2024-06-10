using ExitGames.Client.Photon;
using Photon.Realtime;

public static class PlayerProperty
{
    private const string KeyIsReady = "r";
    private const string KeyScore = "s";

    private static readonly Hashtable propsToSet = new Hashtable();

    public static bool IsReady(this Player player)
    {
        return player.CustomProperties[KeyIsReady] is bool value && value;
    }

    public static int GetScore(this Player player)
    {
        return player.CustomProperties[KeyScore] is int value ? value : 0;
    }

    public static void SetReady(this Player player, bool value)
    {
        propsToSet[KeyIsReady] = value;
        player.SetCustomProperties(propsToSet);
    }

    public static void AddScore(this Player player, int value)
    {
        var score = player.GetScore() + value;
        propsToSet[KeyScore] = score;
        player.SetCustomProperties(propsToSet);
    }
}
