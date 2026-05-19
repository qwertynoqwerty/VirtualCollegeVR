using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using VRCollege.Net;
using VRCollege.Net.Dto;

public sealed class AchievementsNotifier : MonoBehaviour
{
    private ProfileApi _profileApi;

    [Header("UI")]
    [SerializeField] private GlobalToast _toast;

    [Header("Behavior")]
    [SerializeField] private bool _autoCheckOnStart = true;
    [SerializeField] private float _startDelaySeconds = 0.7f;

    [SerializeField] private float _postUnlockDelaySeconds = 0f;
    [SerializeField] private float _toastVisibleSeconds = 5f;
    [SerializeField] private string _message = "Получено достижение, проверьте профиль";

    private const string _prefsKey = "vrcollege.seen.ach.codes";
    private bool _busy;

    private void Awake()
    {
        if (_profileApi == null)
        {
            _profileApi = FindFirstObjectByType<ProfileApi>(FindObjectsInactive.Include);
        }

        if (_toast == null)
        {
            _toast = FindFirstObjectByType<GlobalToast>(FindObjectsInactive.Include);
        }
    }

    private void Start()
    {
        if (_autoCheckOnStart == true)
        {
            _ = StartRoutine();
        }
    }

    private async Task StartRoutine()
    {
        if (_startDelaySeconds > 0f)
        {
            int ms = Mathf.RoundToInt(_startDelaySeconds * 1000f);
            await Task.Delay(ms);
        }
        await CheckAndNotifyAsync();
    }

    public async Task CheckAndNotifyAsync()
    {
        await CheckAndNotifyAsync(_postUnlockDelaySeconds);
    }

    public async Task CheckAndNotifyAsync(float delayBeforeToast)
    {
        if (_busy == true)
        {
            return;
        }

        if (_profileApi == null)
        {
            Debug.LogError("[AchievementsNotifier] ProfileApi is null.");
            return;
        }

        if (_toast == null)
        {
            Debug.LogError("[AchievementsNotifier] GlobalToast is null.");
            return;
        }

        _busy = true;

        ApiResult<ProfileDto> res = await _profileApi.GetProfileAsync();
        if (res.IsSuccess == false || res.Data == null || res.Data.achievements == null)
        {
            Debug.LogError("[AchievementsNotifier] Failed to load achievements. Error: " + res.Error);
            _busy = false;
            return;
        }

        HashSet<string> seen = LoadSeen();
        bool changed = false;

        AchievementDto[] ach = res.Data.achievements;
        for (int i = 0; i < ach.Length; i++)
        {
            AchievementDto a = ach[i];
            bool unlocked = false;
            if (a.unlocked == true)
            {
                unlocked = true;
            }
            else
            {
                unlocked = false;
            }

            if (unlocked == true)
            {
                if (string.IsNullOrEmpty(a.code) == false)
                {
                    bool isNew = false;
                    if (seen.Contains(a.code) == false)
                    {
                        isNew = true;
                    }

                    if (isNew == true)
                    {
                        float delay = Mathf.Max(0f, delayBeforeToast);
                        _toast.Show(_message, delay, _toastVisibleSeconds);

                        seen.Add(a.code);
                        changed = true;

                        int ms = Mathf.RoundToInt((delay + _toastVisibleSeconds + 0.1f) * 1000f);
                        await Task.Delay(ms);
                    }
                }
            }
        }

        if (changed == true)
        {
            SaveSeen(seen);
        }

        _busy = false;
    }

    private static HashSet<string> LoadSeen()
    {
        HashSet<string> set = new HashSet<string>();
        string raw = PlayerPrefs.GetString(_prefsKey, "");
        if (string.IsNullOrEmpty(raw) == false)
        {
            string[] arr = raw.Split(',');
            for (int i = 0; i < arr.Length; i++)
            {
                string s = arr[i];
                if (string.IsNullOrEmpty(s) == false)
                {
                    if (set.Contains(s) == false)
                    {
                        set.Add(s);
                    }
                }
            }
        }
        return set;
    }

    private static void SaveSeen(HashSet<string> set)
    {
        List<string> list = new List<string>(set);
        string raw = string.Join(",", list);
        PlayerPrefs.SetString(_prefsKey, raw);
        PlayerPrefs.Save();
    }
}
