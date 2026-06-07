using UnityEngine;

public static class UserSession
{
    private const int TutorialStateUnknown = -1;
    private const int TutorialStateIncomplete = 0;
    private const int TutorialStateCompleted = 1;

    private const string UserIdKey = "vrcollege.userId";
    private const string SpecialtyIdKey = "vrcollege.specId";
    private const string LegacyTutorialCompletedKey = "vrcollege.tutorialCompleted";

    public static void SaveUserId(int id)
    {
        PlayerPrefs.SetInt(UserIdKey, id);
        PlayerPrefs.Save();
    }

    public static int LoadUserId()
    {
        if (PlayerPrefs.HasKey(UserIdKey))
        {
            return PlayerPrefs.GetInt(UserIdKey);
        }

        return 0;
    }

    public static void SaveSpecialtyId(int id)
    {
        PlayerPrefs.SetInt(SpecialtyIdKey, id);
        PlayerPrefs.Save();
    }

    public static int LoadSpecialtyId()
    {
        if (PlayerPrefs.HasKey(SpecialtyIdKey))
        {
            return PlayerPrefs.GetInt(SpecialtyIdKey);
        }

        return 0;
    }

    public static void SaveTutorialCompleted(bool completed)
    {
        string key = GetTutorialCompletedKey(LoadUserId());
        PlayerPrefs.SetInt(key, completed ? TutorialStateCompleted : TutorialStateIncomplete);
        if (key != LegacyTutorialCompletedKey && PlayerPrefs.HasKey(LegacyTutorialCompletedKey))
        {
            PlayerPrefs.DeleteKey(LegacyTutorialCompletedKey);
        }

        PlayerPrefs.Save();
    }

    public static bool LoadTutorialCompleted()
    {
        if (TryLoadTutorialCompleted(out bool completed))
        {
            return completed;
        }

        return false;
    }

    public static bool TryLoadTutorialCompleted(out bool completed)
    {
        int currentUserId = LoadUserId();
        if (TryReadTutorialCompleted(currentUserId, out completed))
        {
            return true;
        }

        completed = false;
        return false;
    }

    public static void Clear()
    {
        int currentUserId = LoadUserId();

        if (PlayerPrefs.HasKey(SpecialtyIdKey))
        {
            PlayerPrefs.DeleteKey(SpecialtyIdKey);
        }

        if (PlayerPrefs.HasKey(UserIdKey))
        {
            PlayerPrefs.DeleteKey(UserIdKey);
        }

        string tutorialKey = GetTutorialCompletedKey(currentUserId);
        if (PlayerPrefs.HasKey(tutorialKey))
        {
            PlayerPrefs.DeleteKey(tutorialKey);
        }

        if (PlayerPrefs.HasKey(LegacyTutorialCompletedKey))
        {
            PlayerPrefs.DeleteKey(LegacyTutorialCompletedKey);
        }

        PlayerPrefs.Save();
    }

    private static string GetTutorialCompletedKey(int userId)
    {
        if (userId > 0)
        {
            return "vrcollege.user." + userId + ".tutorialCompleted";
        }

        return LegacyTutorialCompletedKey;
    }

    private static bool TryReadTutorialCompleted(int userId, out bool completed)
    {
        string key = GetTutorialCompletedKey(userId);
        if (PlayerPrefs.HasKey(key))
        {
            completed = PlayerPrefs.GetInt(key, TutorialStateUnknown) == TutorialStateCompleted;
            return true;
        }

        if (userId <= 0 && TryReadLegacyTutorialCompleted(out completed))
        {
            return true;
        }

        completed = false;
        return false;
    }

    private static bool TryReadLegacyTutorialCompleted(out bool completed)
    {
        if (PlayerPrefs.HasKey(LegacyTutorialCompletedKey))
        {
            int state = PlayerPrefs.GetInt(LegacyTutorialCompletedKey, TutorialStateUnknown);
            if (state != TutorialStateUnknown)
            {
                completed = state == TutorialStateCompleted;
                return true;
            }
        }

        completed = false;
        return false;
    }
}
