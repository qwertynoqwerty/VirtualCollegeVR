using UnityEngine;
using VRCollege.Net;
using VRCollege.Net.Dto;

public sealed class WarehouseItemPlacementQuestTask : MonoBehaviour
{
    [System.Serializable]
    private sealed class PlacementTarget
    {
        public WarehouseQuestItem item;
        public GameObject targetZoneObject;
        [HideInInspector] public Collider targetZoneTrigger;
        [HideInInspector] public Renderer targetZoneHighlightRenderer;

        public void AutoAssignZoneComponents()
        {
            if (targetZoneObject == null)
            {
                targetZoneTrigger = null;
                targetZoneHighlightRenderer = null;
                return;
            }

            targetZoneTrigger = targetZoneObject.GetComponent<Collider>();
            targetZoneHighlightRenderer = targetZoneObject.GetComponent<Renderer>();
        }
    }

    [Header("Quest")]
    [SerializeField] private NpcQuestRuntime _npcQuestRuntime;
    [SerializeField] private ProgressStatesApi _progressStatesApi;

    [Header("Targets")]
    [SerializeField] private PlacementTarget[] _targets;

    [Header("Zone Highlight")]
    [SerializeField] private Color _zoneHighlightColor = new Color(1f, 1f, 0.5f, 1f);

    private const float SyncIntervalSeconds = 1.0f;

    private bool _isTaskActive;
    private bool _isTaskCompleted;
    private int _npcId;
    private float _nextSyncTime;
    private bool _syncInFlight;
    private bool[] _wasGrabbedLastFrame;
    private int _currentStep;
    private Material[] _zoneMaterials;
    private Color[] _zoneDefaultColors;

    private void Awake()
    {
        AutoAssignTargets();

        int count = _targets != null ? _targets.Length : 0;
        _wasGrabbedLastFrame = new bool[count];
        _currentStep = 0;
        _zoneMaterials = new Material[count];
        _zoneDefaultColors = new Color[count];

        CacheZoneMaterials();
        UpdateHighlights();
    }

    private void Start()
    {
        _npcId = _npcQuestRuntime != null ? _npcQuestRuntime.GetNpcId() : 0;
        _nextSyncTime = Time.time + 0.15f;
    }

    private void OnValidate()
    {
        AutoAssignTargets();
    }

    private void Update()
    {
        TryAutoActivateFromQuestState();
        if (_isTaskActive == false || _isTaskCompleted || _targets == null)
        {
            return;
        }

        if (_currentStep < 0 || _currentStep >= _targets.Length)
        {
            CompleteQuestTask();
            return;
        }

        PlacementTarget currentTarget = _targets[_currentStep];
        if (currentTarget == null || currentTarget.item == null)
        {
            _currentStep++;
            UpdateHighlights();
            return;
        }

        int stepIndex = _currentStep;
        bool isGrabbed = currentTarget.item.IsGrabbed;
        bool wasGrabbed = _wasGrabbedLastFrame[stepIndex];

        if (wasGrabbed && isGrabbed == false && IsInsideTargetZone(currentTarget))
        {
            currentTarget.item.SetHighlight(false);
            _currentStep++;
            if (_currentStep >= _targets.Length)
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

    private bool IsInsideTargetZone(PlacementTarget target)
    {
        if (target == null || target.item == null || target.item.ItemCollider == null || target.targetZoneTrigger == null)
        {
            return false;
        }

        return target.item.ItemCollider.bounds.Intersects(target.targetZoneTrigger.bounds);
    }

    private void CacheZoneMaterials()
    {
        if (_targets == null)
        {
            return;
        }

        for (int i = 0; i < _targets.Length; i++)
        {
            PlacementTarget target = _targets[i];
            if (target == null || target.targetZoneHighlightRenderer == null)
            {
                continue;
            }

            _zoneMaterials[i] = target.targetZoneHighlightRenderer.material;
            _zoneDefaultColors[i] = _zoneMaterials[i].color;
        }
    }

    private void AutoAssignTargets()
    {
        if (_targets == null)
        {
            return;
        }

        for (int i = 0; i < _targets.Length; i++)
        {
            if (_targets[i] != null)
            {
                _targets[i].AutoAssignZoneComponents();
            }
        }
    }

    private void SetZoneHighlight(int index, bool value)
    {
        if (_zoneMaterials == null || index < 0 || index >= _zoneMaterials.Length)
        {
            return;
        }

        if (_zoneMaterials[index] == null)
        {
            return;
        }

        _zoneMaterials[index].color = value ? _zoneHighlightColor : _zoneDefaultColors[index];
    }

    private void UpdateHighlights()
    {
        if (_targets == null)
        {
            return;
        }

        for (int i = 0; i < _targets.Length; i++)
        {
            PlacementTarget target = _targets[i];
            if (target == null || target.item == null)
            {
                continue;
            }

            bool showItem = _isTaskActive && _isTaskCompleted == false && i == _currentStep;
            target.item.SetHighlight(showItem);

            bool showZone = _isTaskActive &&
                            _isTaskCompleted == false &&
                            i == _currentStep &&
                            target.item.IsGrabbed;
            SetZoneHighlight(i, showZone);
        }
    }
}
