using System.Threading.Tasks;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Devices;
using UnityEngine;

public sealed class NpcQuestCompleteHotkey : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private int _npcId = 1;
    [SerializeField] private ProgressApi _progressApi;

    [Header("Input")]
    [SerializeField] private UxrHandSide _handSide = UxrHandSide.Left;
    [SerializeField] private UxrInputButtons _button = UxrInputButtons.Button1;

    private bool _busy;

    private void Awake()
    {
        if (_progressApi == null)
        {
            _progressApi = FindFirstObjectByType<ProgressApi>(FindObjectsInactive.Include);
        }
    }

    private void Update()
    {
        if (_busy || _progressApi == null || _npcId <= 0)
        {
            return;
        }

        if (UxrAvatar.LocalAvatarInput.GetButtonsPressDown(_handSide, _button))
        {
            _ = CompleteSelectedNpcAsync();
        }
    }

    private async Task CompleteSelectedNpcAsync()
    {
        _busy = true;
        try
        {
            var res = await _progressApi.CompleteQuestAsync(_npcId);
            if (res.IsSuccess == false)
            {
                Debug.LogWarning("[NpcQuestCompleteHotkey] Complete failed for npcId=" + _npcId + " | " + res.Error);
            }
            else
            {
                Debug.Log("[NpcQuestCompleteHotkey] Completed npcId=" + _npcId);
                SyncNpcRuntimeState();
            }
        }
        finally
        {
            _busy = false;
        }
    }

    private void SyncNpcRuntimeState()
    {
        var runtimes = FindObjectsByType<NpcQuestRuntime>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < runtimes.Length; i++)
        {
            var rt = runtimes[i];
            if (rt != null && rt.GetNpcId() == _npcId)
            {
                rt.ApplyExternalCompletedState();
            }
        }
    }
}
