using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using VRCollege.Net;
using VRCollege.Net.Dto;

public sealed class QuestJournalPanelController : MonoBehaviour
{
    private ProgressStatesApi _statesApi;
    private NpcsApi _npcsApi;

    [Header("UI")]
    [SerializeField] private TMP_Text _emptyStateText;
    [SerializeField] private Transform _gridRoot;
    [SerializeField] private QuestJournalItemView _itemPrefab;

    private bool _isLoading;

    private void Awake()
    {
        if (_statesApi == null)
            _statesApi = FindFirstObjectByType<ProgressStatesApi>(FindObjectsInactive.Include);
        if (_npcsApi == null)
            _npcsApi = FindFirstObjectByType<NpcsApi>(FindObjectsInactive.Include);
    }

    private void OnEnable()
    {
        _ = Refresh();
    }

    public async Task Refresh()
    {
        if (_isLoading) return;
        _isLoading = true;

        var res = await _statesApi.GetAllAsync();
        if (!res.IsSuccess || res.Data == null)
        {
            RenderEmpty();
            _isLoading = false;
            return;
        }

        var list = new List<ProgressStateDto>();
        for (int i = 0; i < res.Data.Length; i++)
        {
            var s = res.Data[i];
            if (s.status != ProgressStateDto.NotStarted)
                list.Add(s);
        }

        await AttachNpcNamesAsync(list);

        if (list.Count == 0)
        {
            RenderEmpty();
            _isLoading = false;
            return;
        }

        if (_emptyStateText) _emptyStateText.gameObject.SetActive(false);
        if (_gridRoot) _gridRoot.gameObject.SetActive(true);
        Clear(_gridRoot);

        for (int i = 0; i < list.Count; i++)
        {
            if (_itemPrefab && _gridRoot)
            {
                var view = Instantiate(_itemPrefab, _gridRoot);
                view.Bind(list[i]);
            }
        }

        _isLoading = false;
    }

    private async Task AttachNpcNamesAsync(List<ProgressStateDto> list)
    {
        if (_npcsApi == null || list == null || list.Count == 0)
        {
            return;
        }

        var npcsRes = await _npcsApi.GetBySpecialtyAsync();
        if (!npcsRes.IsSuccess || npcsRes.Data == null)
        {
            return;
        }

        var names = new Dictionary<int, string>(npcsRes.Data.Count);
        for (int i = 0; i < npcsRes.Data.Count; i++)
        {
            var n = npcsRes.Data[i];
            names[n.id] = n.role;
        }

        for (int i = 0; i < list.Count; i++)
        {
            var s = list[i];
            if (string.IsNullOrEmpty(s.npcName) && names.TryGetValue(s.npcId, out var name))
            {
                s.npcName = name;
            }
        }
    }

    private void RenderEmpty()
    {
        if (_gridRoot)
        {
            _gridRoot.gameObject.SetActive(false);
            Clear(_gridRoot);
        }
        if (_emptyStateText)
        {
            _emptyStateText.gameObject.SetActive(true);
            _emptyStateText.text = "Квесты ещё не взяты";
        }
    }

    private static void Clear(Transform root)
    {
        if (!root) return;
        var toDestroy = new List<GameObject>();
        for (int i = 0; i < root.childCount; i++)
            toDestroy.Add(root.GetChild(i).gameObject);
        for (int i = 0; i < toDestroy.Count; i++)
            Destroy(toDestroy[i]);
    }
}
