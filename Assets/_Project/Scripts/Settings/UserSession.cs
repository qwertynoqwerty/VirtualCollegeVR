using UnityEngine;

public static class UserSession
{
    private const string _specKey = "vrcollege.specId";
    private const string _firstLoginKey = "vrcollege.isFirstLogin";

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

    public static bool ConsumeIsFirstLogin()
    {
        bool flag = LoadIsFirstLogin();
        if (flag)
        {
            SaveIsFirstLogin(false);
        }

        return flag;
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

        PlayerPrefs.Save();
    }
}
