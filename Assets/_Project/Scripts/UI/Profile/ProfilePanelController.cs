using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using VRCollege.Net;
using VRCollege.Net.Dto;

public sealed class ProfilePanelController : MonoBehaviour
{
    private ProfileApi _profileApi;
    [SerializeField] private ProgressStatesApi _progressStatesApi;
    [SerializeField] private ProgressAnalyticsApi _analyticsApi;

    [SerializeField] private TMP_Text _daysText;
    [SerializeField] private TMP_Text _hoursText;

    [SerializeField] private Transform _gridRoot;
    [SerializeField] private AchievementItemView _itemPrefab;

    [Header("Recommendation Banners")]
    [SerializeField] private GameObject _bannerAbsence;
    [SerializeField] private GameObject _bannerFails;
    [SerializeField] private GameObject _bannerProgress;
    [SerializeField] private GameObject _bannerPoints;
    [SerializeField] private GameObject _bannerHours;

    [Header("Thresholds")]
    [SerializeField] private int _absenceDaysThreshold = 3;
    [SerializeField] private float _failRatePerHourThreshold = 3f;
    [SerializeField] private float _progressPercentThreshold = 30f;
    [SerializeField] private int _pointsThreshold = 2;
    [SerializeField] private float _totalHoursThreshold = 3f;

    private bool _isLoading;

    private void Awake()
    {
        if (_profileApi == null)
        {
            _profileApi = FindObjectOfType<ProfileApi>(true);
        }
        if (_progressStatesApi == null)
        {
            _progressStatesApi = FindObjectOfType<ProgressStatesApi>(true);
        }
        if (_analyticsApi == null)
        {
            _analyticsApi = FindObjectOfType<ProgressAnalyticsApi>(true);
        }

        SetActive(_bannerAbsence, false);
        SetActive(_bannerFails, false);
        SetActive(_bannerProgress, false);
        SetActive(_bannerPoints, false);
        SetActive(_bannerHours, false);
    }

    private void OnEnable()
    {
        _ = Refresh();
        _ = CheckAdditiveBanners();
    }

    public async Task Refresh()
    {
        if (_isLoading == true)
        {
            return;
        }
        _isLoading = true;

        ApiResult<ProfileDto> res = await _profileApi.GetProfileAsync();
        if (res.IsSuccess == false || res.Data == null)
        {
            ShowError("Ошибка профиля");
            _isLoading = false;
            return;
        }

        ProfileDto p = res.Data;

        if (_daysText != null)
        {
            _daysText.text = "Отсутствие: " + p.daysSinceLastLogin + " дн.";
        }
        if (_hoursText != null)
        {
            _hoursText.text = "Всего часов: " + p.totalHoursPlayed;
        }

        Clear(_gridRoot);

        if (_itemPrefab != null && _gridRoot != null && p.achievements != null)
        {
            for (int i = 0; i < p.achievements.Length; i++)
            {
                AchievementItemView item = Instantiate(_itemPrefab, _gridRoot);
                item.Bind(p.achievements[i]);
            }
        }

        _isLoading = false;
    }

    private async Task CheckAdditiveBanners()
    {
        ApiResult<ProfileDto> profileRes = await _profileApi.GetProfileAsync();
        int daysSince = 0;
        float totalHours = 0f;

        if (profileRes.IsSuccess == true && profileRes.Data != null)
        {
            daysSince = profileRes.Data.daysSinceLastLogin;
            totalHours = profileRes.Data.totalHoursPlayed;
        }

        int total = 0;
        int completed = 0;

        if (_progressStatesApi != null)
        {
            ApiResult<ProgressStateDto[]> statesRes = await _progressStatesApi.GetStatesAsync();
            if (statesRes.IsSuccess == true && statesRes.Data != null)
            {
                total = statesRes.Data.Length;
                for (int i = 0; i < statesRes.Data.Length; i++)
                {
                    ProgressStateDto st = statesRes.Data[i];
                    if (st.state == "completed")
                    {
                        completed = completed + 1;
                    }
                }
            }
        }

        float percent = 0f;
        if (total > 0)
        {
            percent = (completed * 100f) / total;
        }

        var failRes = await _analyticsApi.GetFailIntervalAsync();

        int cnt = -1;
        string avgStr = "null-data";
        if (failRes.Data != null)
        {
            cnt = failRes.Data.count;
            if (failRes.Data.avgIntervalMinutes > 0f)
            {
                avgStr = failRes.Data.avgIntervalMinutes.ToString("0.###");
            }
        }

        Debug.Log("[Additive:API] ok=" + failRes.IsSuccess + ", count=" + cnt + ", avg=" + avgStr);

        float failsPerHour = 0f;
        if (failRes.IsSuccess == true && failRes.Data != null)
        {
            float avg = failRes.Data.avgIntervalMinutes;
            if (avg > 0.0001f)
            {
                failsPerHour = 60f / avg;
            }
        }

        bool showAbsence = daysSince > _absenceDaysThreshold;
        bool showFails = failsPerHour > _failRatePerHourThreshold;
        bool showProgress = percent < _progressPercentThreshold;
        bool showPoints = completed < _pointsThreshold;
        bool showHours = totalHours > _totalHoursThreshold;

        SetActive(_bannerAbsence, showAbsence);
        SetActive(_bannerFails, showFails);
        SetActive(_bannerProgress, showProgress);
        SetActive(_bannerPoints, showPoints);
        SetActive(_bannerHours, showHours);

        Debug.Log("[Additive] days=" + daysSince +
                  ", hours=" + totalHours.ToString("0.0") +
                  ", done=" + completed + "/" + total +
                  ", %=" + percent.ToString("0.0") +
                  ", fails/h=" + failsPerHour.ToString("0.0") +
                  " | A=" + showAbsence + " F=" + showFails + " P%=" + showProgress +
                  " Pts=" + showPoints + " H=" + showHours);
    }
    
    private void ShowError(string msg)
    {
        if (_daysText != null)
        {
            _daysText.text = msg;
        }
        if (_hoursText != null)
        {
            _hoursText.text = "";
        }
        Clear(_gridRoot);

        SetActive(_bannerAbsence, false);
        SetActive(_bannerFails, false);
        SetActive(_bannerProgress, false);
        SetActive(_bannerPoints, false);
        SetActive(_bannerHours, false);
    }

    private static void Clear(Transform root)
    {
        if (root == null)
        {
            return;
        }

        List<GameObject> list = new List<GameObject>();
        for (int i = 0; i < root.childCount; i++)
        {
            Transform c = root.GetChild(i);
            list.Add(c.gameObject);
        }
        for (int i = 0; i < list.Count; i++)
        {
            Destroy(list[i]);
        }
    }

    private static void SetActive(GameObject go, bool state)
    {
        if (go != null)
        {
            go.SetActive(state);
        }
    }
}
