using UnityEngine;
using VRCollege.Net;
using VRCollege.Net.Dto;

public sealed class DishServingQuestTask : MonoBehaviour
{
    [Header("Quest")]
    [SerializeField] private NpcQuestRuntime _npcQuestRuntime;
    [SerializeField] private ProgressStatesApi _progressStatesApi;

    [Header("Serve Zone")]
    [SerializeField] private Collider _serveZoneTrigger;

    [Header("Plates")]
    [SerializeField] private CulinaryQuestItem[] _targetPlates;

    private const float SyncIntervalSeconds = 1.0f;

    private bool _isTaskActive;
    private bool _isTaskCompleted;
    private int _npcId;
    private int _currentStep;
    private float _nextSyncTime;
    private bool _syncInFlight;
    private bool[] _wasGrabbedLastFrame;

    private void Awake()
    {
        int count = _targetPlates != null ? _targetPlates.Length : 0;
        _wasGrabbedLastFrame = new bool[count];
        _currentStep = 0;
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

        if (_isTaskActive == false || _isTaskCompleted || _targetPlates == null)
        {
            return;
        }

        if (_currentStep < 0 || _currentStep >= _targetPlates.Length)
        {
            CompleteQuestTask();
            return;
        }

        CulinaryQuestItem currentPlate = _targetPlates[_currentStep];
        if (currentPlate == null)
        {
            _currentStep++;
            UpdateHighlights();
            return;
        }

        int stepIndex = _currentStep;
        bool isGrabbed = currentPlate.IsGrabbed;
        bool wasGrabbed = _wasGrabbedLastFrame[stepIndex];
        if (wasGrabbed && isGrabbed == false && IsInsideServeZone(currentPlate))
        {
            currentPlate.SetHighlight(false);
            _currentStep++;
            if (_currentStep >= _targetPlates.Length)
            {
                CompleteQuestTask();
                return;
            }
        }

        _wasGrabbedLastFrame[stepIndex] = isGrabbed;
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

    private bool IsInsideServeZone(CulinaryQuestItem plate)
    {
        if (plate == null || plate.ItemCollider == null || _serveZoneTrigger == null)
        {
            return false;
        }

        return plate.ItemCollider.bounds.Intersects(_serveZoneTrigger.bounds);
    }

    private void UpdateHighlights()
    {
        if (_targetPlates == null)
        {
            return;
        }

        for (int i = 0; i < _targetPlates.Length; i++)
        {
            CulinaryQuestItem plate = _targetPlates[i];
            if (plate == null)
            {
                continue;
            }

            bool show = _isTaskActive && _isTaskCompleted == false && i == _currentStep;
            plate.SetHighlight(show);
        }
    }
}
