using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using VRCollege.Net;

public sealed class ProfilePanelController : MonoBehaviour
{
    private ProfileApi _profileApi;

    [Header("UI")]
    [SerializeField] private Transform _gridRoot;
    [SerializeField] private AchievementItemView _itemPrefab;

    private bool _isLoading;

    private void Awake()
    {
        if (_profileApi == null)
            _profileApi = FindFirstObjectByType<ProfileApi>(FindObjectsInactive.Include);
    }

    private void OnEnable()
    {
        _ = Refresh();
    }

    public async Task Refresh()
    {
        if (_isLoading) return;
        _isLoading = true;

        var res = await _profileApi.GetProfileAsync();
        if (!res.IsSuccess || res.Data == null)
        {
            Clear(_gridRoot);
            _isLoading = false;
            return;
        }

        Clear(_gridRoot);

        var p = res.Data;
        if (_itemPrefab != null && _gridRoot != null && p.achievements != null)
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
}