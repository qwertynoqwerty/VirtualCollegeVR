using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using VRCollege.Net;
using VRCollege.Net.Dto;

public sealed class QuestJournalPanelController : MonoBehaviour
{
    private ProgressStatesApi _statesApi;

    [Header("Metrics")]
    [SerializeField] private TMP_Text _pointsText;
    [SerializeField] private TMP_Text _percentText;
    [SerializeField] private TMP_Text _failAttemptsText;
    
    [Header("UI")]
    [SerializeField] private TMP_Text _emptyStateText;
    [SerializeField] private Transform _gridRoot;
    [SerializeField] private QuestJournalItemView _itemPrefab;

    private const int _totalQuests = 9;
    private bool _isLoading;

    private const string _failLogKey = "vrcollege.fail.attempts_log";
    private const int _failLogMax = 200;

    [Serializable]
    private class _TsList { public List<long> items = new List<long>(); }

    public static void LogFailNow()
    {
        _TsList ts = _LoadFailTs();
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        ts.items.Add(now);
        _PruneFailTs(ts, 24 * 60);
        _SaveFailTs(ts);
    }

    public static bool TryGetAvgFailIntervalMinutes(int windowMinutes, out double avgMin)
    {
        avgMin = 0d;
        if (windowMinutes <= 0)
        {
            return false;
        }

        _TsList ts = _LoadFailTs();
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long minTs = now - windowMinutes * 60L;

        List<long> list = new List<long>();
        for (int i = 0; i < ts.items.Count; i++)
        {
            long t = ts.items[i];
            if (t >= minTs)
            {
                list.Add(t);
            }
        }
        list.Sort();

        if (list.Count <= 1)
        {
            return false;
        }

        long spanSec = list[list.Count - 1] - list[0];
        if (spanSec <= 0)
        {
            return false;
        }

        int gaps = list.Count - 1;
        double spanMin = spanSec / 60.0;
        avgMin = spanMin / gaps;
        return true;
    }

    private static _TsList _LoadFailTs()
    {
        string json = PlayerPrefs.GetString(_failLogKey, "");
        if (string.IsNullOrEmpty(json) == true)
        {
            return new _TsList();
        }
        try
        {
            return JsonUtility.FromJson<_TsList>(json);
        }
        catch
        {
            return new _TsList();
        }
    }

    private static void _SaveFailTs(_TsList ts)
    {
        if (ts == null)
        {
            return;
        }
        string json = JsonUtility.ToJson(ts);
        PlayerPrefs.SetString(_failLogKey, json);
        PlayerPrefs.Save();
    }

    private static void _PruneFailTs(_TsList ts, int windowMinutes)
    {
        if (ts == null)
        {
            return;
        }

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long minTs = now - windowMinutes * 60L;

        List<long> kept = new List<long>();
        for (int i = 0; i < ts.items.Count; i++)
        {
            long t = ts.items[i];
            if (t >= minTs)
            {
                kept.Add(t);
            }
        }

        if (kept.Count > _failLogMax)
        {
            int start = kept.Count - _failLogMax;
            List<long> slice = new List<long>();
            for (int i = start; i < kept.Count; i++)
            {
                slice.Add(kept[i]);
            }
            kept = slice;
        }

        ts.items = kept;
    }

    private void Awake()
    {
        if (_statesApi == null)
        {
            _statesApi = FindObjectOfType<ProgressStatesApi>(true);
        }
    }

    private void OnEnable()
    {
        _ = Refresh();
    }

    public async Task Refresh()
    {
        if (_isLoading == true)
        {
            return;
        }

        _isLoading = true;

        ApiResult<ProgressStateDto[]> res = await _statesApi.GetAllAsync();
        if (res.IsSuccess == false)
        {
            RenderEmpty();
            _isLoading = false;
            return;
        }

        if (res.Data == null)
        {
            RenderEmpty();
            _isLoading = false;
            return;
        }

        ProgressStateDto[] all = res.Data;

        List<ProgressStateDto> list = new List<ProgressStateDto>();
        for (int i = 0; i < all.Length; i++)
        {
            ProgressStateDto s = all[i];
            if (s.state == "in_progress")
            {
                list.Add(s);
            }
            else
            {
                if (s.state == "completed")
                {
                    list.Add(s);
                }
            }
        }

        int completed = 0;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].state == "completed")
            {
                completed = completed + 1;
            }
        }

        if (_pointsText != null)
        {
            _pointsText.text = "Очки: " + completed + "/" + _totalQuests;
        }

        if (_percentText != null)
        {
            int pct = 0;
            if (_totalQuests > 0)
            {
                float f = (float)completed * 100f / (float)_totalQuests;
                pct = Mathf.RoundToInt(f);
            }
            _percentText.text = "Процент: " + pct + "%";
        }

        if (_failAttemptsText != null)
        {
            double avg;
            bool ok = TryGetAvgFailIntervalMinutes(60, out avg);
            if (ok == true)
            {
                _failAttemptsText.text = "Интервал: " + avg.ToString("0.0") + " мин";
            }
            else
            {
                _failAttemptsText.text = "Интервал: —";
            }
        }

        if (list.Count == 0)
        {
            RenderEmpty();
            _isLoading = false;
            return;
        }

        if (_emptyStateText != null)
        {
            _emptyStateText.gameObject.SetActive(false);
        }
        if (_gridRoot != null)
        {
            _gridRoot.gameObject.SetActive(true);
        }

        Clear(_gridRoot);

        for (int i = 0; i < list.Count; i++)
        {
            ProgressStateDto s = list[i];
            if (_itemPrefab != null && _gridRoot != null)
            {
                QuestJournalItemView view = Instantiate(_itemPrefab, _gridRoot);
                view.Bind(s);
            }
        }

        _isLoading = false;
    }

    private void RenderEmpty()
    {
        if (_gridRoot != null)
        {
            _gridRoot.gameObject.SetActive(false);
            Clear(_gridRoot);
        }
        if (_emptyStateText != null)
        {
            _emptyStateText.gameObject.SetActive(true);
            _emptyStateText.text = "Квесты ещё не взяты";
        }

        if (_pointsText != null)
        {
            _pointsText.text = "Очки: 0/" + _totalQuests;
        }
        if (_percentText != null)
        {
            _percentText.text = "Процент: 0%";
        }
        if (_failAttemptsText != null)
        {
            _failAttemptsText.text = "Интервал: —";
        }
    }

    private static void Clear(Transform root)
    {
        if (root == null)
        {
            return;
        }

        List<GameObject> toDestroy = new List<GameObject>();
        for (int i = 0; i < root.childCount; i++)
        {
            Transform c = root.GetChild(i);
            toDestroy.Add(c.gameObject);
        }
        for (int i = 0; i < toDestroy.Count; i++)
        {
            Destroy(toDestroy[i]);
        }
    }
}
