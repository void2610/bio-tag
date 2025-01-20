using ExitGames.Client.Photon;
using Photon.Realtime;

public static class PlayerProperty
{
    private const string KEY_IS_READY = "r";
    private const string KEY_SCORE = "s";

    private static readonly Hashtable _propsToSet = new Hashtable();

    public static bool IsReady(this Player player)
    {
        return player.CustomProperties[KEY_IS_READY] is bool value && value;
    }

    public static int GetScore(this Player player)
    {
        return player.CustomProperties[KEY_SCORE] is int value ? value : 0;
    }

    public static void SetReady(this Player player, bool value)
    {
        _propsToSet[KEY_IS_READY] = value;
        player.SetCustomProperties(_propsToSet);
    }

    public static void SetScore(this Player player, int value)
    {
        _propsToSet[KEY_SCORE] = value;
        player.SetCustomProperties(_propsToSet);
    }
}
