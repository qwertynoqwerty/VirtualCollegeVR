using UnityEngine;
using VRCollege.Net;
using VRCollege.Net.Dto;

public sealed class WarehouseBoxTransferQuestTask : MonoBehaviour
{
    [Header("Quest")]
    [SerializeField] private NpcQuestRuntime _npcQuestRuntime;
    [SerializeField] private ProgressStatesApi _progressStatesApi;

    [Header("Drop Zone")]
    [SerializeField] private Collider _dropZoneTrigger;

    [Header("Boxes")]
    [SerializeField] private WarehouseQuestItem[] _targetBoxes;

    private const float SyncIntervalSeconds = 1.0f;

    private bool _isTaskActive;
    private bool _isTaskCompleted;
    private int _npcId;
    private float _nextSyncTime;
    private bool _syncInFlight;
    private bool[] _wasGrabbedLastFrame;
    private bool[] _isPlaced;
    private int _placedCount;

    private void Awake()
    {
        int count = _targetBoxes != null ? _targetBoxes.Length : 0;
        _wasGrabbedLastFrame = new bool[count];
        _isPlaced = new bool[count];
        UpdateHighlights();
    }

    private void Start()
    {
        _npcId = _npcQuestRuntime != null ? _npcQuestRuntime.GetNpcId() : 0;
        _nextSyncTime = Time.time + 0.15f;
    }

    private void Update()
    {
        TryAutoActivateFromQuestState();
        if (_isTaskActive == false || _isTaskCompleted || _targetBoxes == null)
        {
            return;
        }

        for (int i = 0; i < _targetBoxes.Length; i++)
        {
            if (_isPlaced[i])
            {
                continue;
            }

            WarehouseQuestItem box = _targetBoxes[i];
            if (box == null)
            {
                continue;
            }

            bool isGrabbed = box.IsGrabbed;
            bool wasGrabbed = _wasGrabbedLastFrame[i];

            if (wasGrabbed == false && isGrabbed)
            {
                box.SetHighlight(false);
            }

            if (wasGrabbed && isGrabbed == false && IsInsideDropZone(box))
            {
                _isPlaced[i] = true;
                _placedCount++;
                box.SetHighlight(false);
            }
            else if (wasGrabbed && isGrabbed == false)
            {
                box.SetHighlight(true);
            }

            _wasGrabbedLastFrame[i] = isGrabbed;
        }

        if (_targetBoxes.Length > 0 && _placedCount >= _targetBoxes.Length)
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

    private bool IsInsideDropZone(WarehouseQuestItem box)
    {
        if (box == null || box.ItemCollider == null || _dropZoneTrigger == null)
        {
            return false;
        }

        return box.ItemCollider.bounds.Intersects(_dropZoneTrigger.bounds);
    }

    private void UpdateHighlights()
    {
        if (_targetBoxes == null)
        {
            return;
        }

        for (int i = 0; i < _targetBoxes.Length; i++)
        {
            WarehouseQuestItem box = _targetBoxes[i];
            if (box == null)
            {
                continue;
            }

            bool show = _isTaskActive && _isTaskCompleted == false && _isPlaced != null && i < _isPlaced.Length && _isPlaced[i] == false;
            box.SetHighlight(show);
        }
    }
}
