using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using VRCollege.Net;

public class AchievementsPanelController : MonoBehaviour
{
    private ProfileApi _profileApi;

    [Header("UI")]
    [SerializeField] private Transform _gridRoot;
    [SerializeField] private AchievementItemView _itemPrefab;

    private bool _isLoading;

    private void Awake()
    {
        TryResolveProfileApi();
    }

    private void OnEnable()
    {
        _ = Refresh();
    }

    public async Task Refresh()
    {
        if (_isLoading) return;
        _isLoading = true;

        if (_profileApi == null && TryResolveProfileApi() == false)
        {
            Debug.LogError("[AchievementsPanelController] Refresh aborted: ProfileApi is null.");
            _isLoading = false;
            return;
        }

        if (_gridRoot == null)
        {
            Debug.LogError("[AchievementsPanelController] Refresh aborted: GridRoot is not assigned.");
            _isLoading = false;
            return;
        }

        if (_itemPrefab == null)
        {
            Debug.LogError("[AchievementsPanelController] Refresh aborted: ItemPrefab is not assigned.");
            _isLoading = false;
            return;
        }

        var res = await _profileApi.GetProfileAsync();
        if (!res.IsSuccess || res.Data == null)
        {
            Debug.LogError("[AchievementsPanelController] Profile request failed. Error: " + res.Error);
            Clear(_gridRoot);
            _isLoading = false;
            return;
        }

        Clear(_gridRoot);

        var p = res.Data;
        if (p.achievements == null)
        {
            Debug.LogError("[AchievementsPanelController] Profile loaded, but achievements is null.");
            _isLoading = false;
            return;
        }

        Debug.Log("[AchievementsPanelController] Achievements loaded: " + p.achievements.Length);

        if (_itemPrefab != null && _gridRoot != null)
        {
            for (int i = 0; i < p.achievements.Length; i++)
            {
                var item = Instantiate(_itemPrefab, _gridRoot);
                item.Bind(p.achievements[i]);
            }
        }

        _isLoading = false;
    }

    private static void Clear(Transform root)
    {
        if (!root) return;
        var list = new List<GameObject>();
        for (int i = 0; i < root.childCount; i++)
            list.Add(root.GetChild(i).gameObject);
        for (int i = 0; i < list.Count; i++)
            Destroy(list[i]);
    }

    private bool TryResolveProfileApi()
    {
        if (_profileApi != null)
        {
            return true;
        }

        _profileApi = FindFirstObjectByType<ProfileApi>(FindObjectsInactive.Include);
        if (_profileApi == null)
        {
            Debug.LogError("[AchievementsPanelController] ProfileApi not found in scene.");
            return false;
        }

        return true;
    }
}
