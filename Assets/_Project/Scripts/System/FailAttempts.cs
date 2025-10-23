using UnityEngine;

public static class FailAttempts
{
    private const string _key = "vrcollege.fail.attempts";

    public static void Increment()
    {
        int n = PlayerPrefs.GetInt(_key, 0);
        n = n + 1;
        PlayerPrefs.SetInt(_key, n);
        PlayerPrefs.Save();
    }

    public static int Get()
    {
        return PlayerPrefs.GetInt(_key, 0);
    }

    public static void Reset()
    {
        PlayerPrefs.DeleteKey(_key);
        PlayerPrefs.Save();
    }
}