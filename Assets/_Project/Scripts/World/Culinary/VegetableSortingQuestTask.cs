using UnityEngine;
using VRCollege.Net;
using VRCollege.Net.Dto;

public sealed class VegetableSortingQuestTask : MonoBehaviour
{
    [System.Serializable]
    private sealed class TargetItem
    {
        public CulinaryQuestItem item;
        public bool isFresh;
    }

    [Header("Quest")]
    [SerializeField] private NpcQuestRuntime _npcQuestRuntime;
    [SerializeField] private ProgressStatesApi _progressStatesApi;

    [Header("Zones")]
    [SerializeField] private Collider _freshZoneTrigger;
    [SerializeField] private Collider _spoiledZoneTrigger;

    [Header("Items")]
    [SerializeField] private TargetItem[] _targetItems;

    private const float SyncIntervalSeconds = 1.0f;

    private bool _isTaskActive;
    private bool _isTaskCompleted;
    private int _npcId;
    private float _nextSyncTime;
    private bool _syncInFlight;
    private bool[] _isSorted;
    private bool[] _wasGrabbedLastFrame;

    private void Awake()
    {
        int count = _targetItems != null ? _targetItems.Length : 0;
        _isSorted = new bool[count];
        _wasGrabbedLastFrame = new bool[count];
        SetItemsHighlight(false);
    }

    private void Start()
    {
        _npcId = _npcQuestRuntime != null ? _npcQuestRuntime.GetNpcId() : 0;
        _nextSyncTime = Time.time + 0.15f;
    }

    private void Update()
    {
        TryAutoActivateFromQuestState();
        if (_isTaskActive == false || _isTaskCompleted || _targetItems == null)
        {
            return;
        }

        for (int i = 0; i < _targetItems.Length; i++)
        {
            if (_isSorted[i])
            {
                continue;
            }

            TargetItem target = _targetItems[i];
            if (target == null || target.item == null)
            {
                continue;
            }

            bool isGrabbed = target.item.IsGrabbed;
            bool wasGrabbed = _wasGrabbedLastFrame[i];

            if (wasGrabbed && isGrabbed == false)
            {
                bool releasedInFresh = IsInsideZone(target.item, _freshZoneTrigger);
                bool releasedInSpoiled = IsInsideZone(target.item, _spoiledZoneTrigger);
                bool correctZone = target.isFresh ? releasedInFresh : releasedInSpoiled;

                if (correctZone)
                {
                    _isSorted[i] = true;
                    target.item.SetHighlight(false);
                }
            }

            _wasGrabbedLastFrame[i] = isGrabbed;
        }

        if (AllItemsSorted())
        {
            CompleteQuestTask();
        }
    }

    public void ActivateQuestTask()
    {
        if (_isTaskCompleted)
        {
            return;
        }

        _isTaskActive = true;
        SetItemsHighlight(true);
    }

    public void CompleteQuestTask()
    {
        if (_isTaskCompleted)
        {
            return;
        }

        _isTaskCompleted = true;
        SetItemsHighlight(false);

        if (_npcQuestRuntime != null)
        {
            _npcQuestRuntime.MarkQuestCompleted();
        }
    }

    private async void TryAutoActivateFromQuestState()
    {
        if (_progressStatesApi == null || _npcId <= 0 || _syncInFlight || Time.time < _nextSyncTime)
        {
            return;
        }

        _nextSyncTime = Time.time + SyncIntervalSeconds;
        _syncInFlight = true;

        ApiResult<ProgressStateDto[]> states = await _progressStatesApi.GetStatesAsync();
        if (states.IsSuccess && states.Data != null)
        {
            for (int i = 0; i < states.Data.Length; i++)
            {
                ProgressStateDto state = states.Data[i];
                if (state.npcId == _npcId &&
                    (state.status == ProgressStateDto.InProgress || state.status == ProgressStateDto.InProgressRepeat))
                {
                    ActivateQuestTask();
                    break;
                }
            }
        }

        _syncInFlight = false;
    }

    private bool AllItemsSorted()
    {
        if (_isSorted == null || _isSorted.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < _isSorted.Length; i++)
        {
            if (_isSorted[i] == false)
            {
                return false;
            }
        }

        return true;
    }

    private void SetItemsHighlight(bool value)
    {
        if (_targetItems == null)
        {
            return;
        }

        for (int i = 0; i < _targetItems.Length; i++)
        {
            if (_targetItems[i] != null && _targetItems[i].item != null)
            {
                bool show = value && (_isSorted == null || i >= _isSorted.Length || _isSorted[i] == false);
                _targetItems[i].item.SetHighlight(show);
            }
        }
    }

    private static bool IsInsideZone(CulinaryQuestItem item, Collider zone)
    {
        if (item == null || item.ItemCollider == null || zone == null)
        {
            return false;
        }

        return item.ItemCollider.bounds.Intersects(zone.bounds);
    }

}
