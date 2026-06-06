using UnityEngine;

public static class UserSession
{
    private const string _specKey = "vrcollege.specId";
    private const string _firstLoginKey = "vrcollege.isFirstLogin";
    private const string _tutorialCompletedKey = "vrcollege.tutorialCompleted";

    public static void SaveSpecialtyId(int id)
    {
        PlayerPrefs.SetInt(_specKey, id);
        PlayerPrefs.Save();
    }

    public static int LoadSpecialtyId()
    {
        if (PlayerPrefs.HasKey(_specKey))
        {
            return PlayerPrefs.GetInt(_specKey);
        }

        return 0;
    }

    public static void SaveIsFirstLogin(bool isFirst)
    {
        if (isFirst)
        {
            PlayerPrefs.SetInt(_firstLoginKey, 1);
        }
        else
        {
            PlayerPrefs.SetInt(_firstLoginKey, 0);
        }

        PlayerPrefs.Save();
    }

    public static bool LoadIsFirstLogin()
    {
        if (PlayerPrefs.HasKey(_firstLoginKey))
        {
            int v = PlayerPrefs.GetInt(_firstLoginKey);
            if (v == 1)
            {
                return true;
            }

            return false;
        }

        return false;
    }

    public static void SaveTutorialCompleted(bool completed)
    {
        PlayerPrefs.SetInt(_tutorialCompletedKey, completed ? 1 : 0);
        PlayerPrefs.Save();
    }

    public static bool LoadTutorialCompleted()
    {
        if (PlayerPrefs.HasKey(_tutorialCompletedKey))
        {
            return PlayerPrefs.GetInt(_tutorialCompletedKey) == 1;
        }

        return false;
    }

    public static void Clear()
    {
        if (PlayerPrefs.HasKey(_specKey))
        {
            PlayerPrefs.DeleteKey(_specKey);
        }

        if (PlayerPrefs.HasKey(_firstLoginKey))
        {
            PlayerPrefs.DeleteKey(_firstLoginKey);
        }

        if (PlayerPrefs.HasKey(_tutorialCompletedKey))
        {
            PlayerPrefs.DeleteKey(_tutorialCompletedKey);
        }

        PlayerPrefs.Save();
    }
}
