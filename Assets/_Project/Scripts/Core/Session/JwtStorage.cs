using UnityEngine;

public static class JwtStorage
{
    private const string Key = "vrcollege.jwt";

    public static void Save(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return;
        }

        PlayerPrefs.SetString(Key, token);
        PlayerPrefs.Save();
    }

    public static string Load()
    {
        if (PlayerPrefs.HasKey(Key))
        {
            return PlayerPrefs.GetString(Key);
        }

        return null;
    }

    public static void Clear()
    {
        if (PlayerPrefs.HasKey(Key))
        {
            PlayerPrefs.DeleteKey(Key);
            PlayerPrefs.Save();
        }
    }

    public static bool HasToken()
    {
        string token = Load();
        if (string.IsNullOrEmpty(token))
        {
            return false;
        }

        return true;
    }
}