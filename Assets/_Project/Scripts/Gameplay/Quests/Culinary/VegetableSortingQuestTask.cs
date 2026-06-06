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
    private ColliderHighlightHandle _freshZoneHighlight;
    private ColliderHighlightHandle _spoiledZoneHighlight;

    private void Awake()
    {
        int count = _targetItems != null ? _targetItems.Length : 0;
        _isSorted = new bool[count];
        _wasGrabbedLastFrame = new bool[count];
        _freshZoneHighlight = QuestHighlightUtility.CreateHighlight(
            _freshZoneTrigger,
            QuestHighlightUtility.DefaultHighlightColor,
            QuestHighlightUtility.DefaultHighlightAlpha,
            true);
        _spoiledZoneHighlight = QuestHighlightUtility.CreateHighlight(
            _spoiledZoneTrigger,
            QuestHighlightUtility.DefaultHighlightColor,
            QuestHighlightUtility.DefaultHighlightAlpha,
            true);
        UpdateHighlights();
    }

    private void Start()
    {
        _npcId = _npcQuestRuntime != null ? _npcQuestRuntime.GetNpcId() : 0;
        _nextSyncTime = Time.time + 0.15f;
    }

    private void OnValidate()
    {
        if (_freshZoneHighlight != null)
        {
            _freshZoneHighlight.SetColor(QuestHighlightUtility.DefaultHighlightColor, QuestHighlightUtility.DefaultHighlightAlpha);
        }

        if (_spoiledZoneHighlight != null)
        {
            _spoiledZoneHighlight.SetColor(QuestHighlightUtility.DefaultHighlightColor, QuestHighlightUtility.DefaultHighlightAlpha);
        }
    }

    private void OnDestroy()
    {
        if (_freshZoneHighlight != null)
        {
            _freshZoneHighlight.Dispose();
            _freshZoneHighlight = null;
        }

        if (_spoiledZoneHighlight != null)
        {
            _spoiledZoneHighlight.Dispose();
            _spoiledZoneHighlight = null;
        }
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
            return;
        }

        UpdateHighlights();
    }

    public void ActivateQuestTask()
    {
        if (_isTaskCompleted)
        {
            return;
        }

        _isTaskActive = true;
        UpdateHighlights();
    }

    public void CompleteQuestTask()
    {
        if (_isTaskCompleted)
        {
            return;
        }

        _isTaskCompleted = true;
        UpdateHighlights();

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
        try
        {
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
        }
        finally
        {
            _syncInFlight = false;
        }
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

    private void UpdateHighlights()
    {
        bool showFreshZone = false;
        bool showSpoiledZone = false;

        if (_targetItems == null)
        {
            SetZoneHighlights(false, false);
            return;
        }

        for (int i = 0; i < _targetItems.Length; i++)
        {
            if (_targetItems[i] == null || _targetItems[i].item == null)
            {
                continue;
            }

            bool isPending = _isSorted == null || i >= _isSorted.Length || _isSorted[i] == false;
            bool showItem = _isTaskActive && _isTaskCompleted == false && isPending && _targetItems[i].item.IsGrabbed == false;
            _targetItems[i].item.SetHighlight(showItem);

            if (_isTaskActive && _isTaskCompleted == false && isPending && _targetItems[i].item.IsGrabbed)
            {
                if (_targetItems[i].isFresh)
                {
                    showFreshZone = true;
                }
                else
                {
                    showSpoiledZone = true;
                }
            }
        }

        SetZoneHighlights(showFreshZone, showSpoiledZone);
    }

    private void SetZoneHighlights(bool showFresh, bool showSpoiled)
    {
        if (_freshZoneHighlight != null)
        {
            _freshZoneHighlight.SetColor(QuestHighlightUtility.DefaultHighlightColor, QuestHighlightUtility.DefaultHighlightAlpha);
            _freshZoneHighlight.SetVisible(showFresh);
        }

        if (_spoiledZoneHighlight != null)
        {
            _spoiledZoneHighlight.SetColor(QuestHighlightUtility.DefaultHighlightColor, QuestHighlightUtility.DefaultHighlightAlpha);
            _spoiledZoneHighlight.SetVisible(showSpoiled);
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
