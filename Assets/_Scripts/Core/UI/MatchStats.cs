// Notice we remove ": MonoBehaviour" because this will not go on a GameObject
public static class MatchResultsData
{
    public static int WinnerPlayer;
    public static float Player1TotalDamageDealt;
    public static float Player2TotalDamageDealt;

    public static void ResetData()
    {
        WinnerPlayer = -1;
        Player1TotalDamageDealt = 0f;
        Player2TotalDamageDealt = 0f;
    }
}