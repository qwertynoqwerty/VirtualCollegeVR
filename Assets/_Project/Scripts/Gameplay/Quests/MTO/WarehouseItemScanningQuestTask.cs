using UnityEngine;
using VRCollege.Net;
using VRCollege.Net.Dto;

public sealed class WarehouseItemScanningQuestTask : MonoBehaviour
{
    [Header("Quest")]
    [SerializeField] private NpcQuestRuntime _npcQuestRuntime;
    [SerializeField] private ProgressStatesApi _progressStatesApi;

    [Header("Scanner")]
    [SerializeField] private WarehouseQuestItem _scannerItem;
    [SerializeField] private OVRInput.Controller _scanInputController = OVRInput.Controller.RTouch;
    [SerializeField] private OVRInput.Button _scanInputButton = OVRInput.Button.One;

    [Header("Scan Targets")]
    [SerializeField] private GameObject[] _targets;

    private const float SyncIntervalSeconds = 1.0f;

    private bool _isTaskActive;
    private bool _isTaskCompleted;
    private int _npcId;
    private float _nextSyncTime;
    private bool _syncInFlight;
    private bool[] _isScanned;
    private int _scannedCount;
    private Collider[] _targetColliders;
    private ColliderHighlightHandle[] _zoneHighlights;

    private void Awake()
    {
        int count = _targets != null ? _targets.Length : 0;
        _isScanned = new bool[count];
        _targetColliders = new Collider[count];
        _zoneHighlights = new ColliderHighlightHandle[count];

        CacheTargetComponents();
        UpdateHighlights();
    }

    private void Start()
    {
        _npcId = _npcQuestRuntime != null ? _npcQuestRuntime.GetNpcId() : 0;
        _nextSyncTime = Time.time + 0.15f;
    }

    private void OnValidate()
    {
        if (_zoneHighlights == null)
        {
            return;
        }

        for (int i = 0; i < _zoneHighlights.Length; i++)
        {
            if (_zoneHighlights[i] != null)
            {
                _zoneHighlights[i].SetColor(QuestHighlightUtility.DefaultHighlightColor, QuestHighlightUtility.DefaultHighlightAlpha);
            }
        }
    }

    private void OnDestroy()
    {
        if (_zoneHighlights == null)
        {
            return;
        }

        for (int i = 0; i < _zoneHighlights.Length; i++)
        {
            if (_zoneHighlights[i] != null)
            {
                _zoneHighlights[i].Dispose();
                _zoneHighlights[i] = null;
            }
        }
    }

    private void Update()
    {
        TryAutoActivateFromQuestState();
        if (_isTaskActive == false || _isTaskCompleted || _targets == null || _scannerItem == null)
        {
            return;
        }

        bool triggerPressed = OVRInput.GetDown(_scanInputButton, _scanInputController);

        if (triggerPressed && _scannerItem.IsGrabbed)
        {
            for (int i = 0; i < _targets.Length; i++)
            {
                if (_isScanned[i])
                {
                    continue;
                }

                if (IsScannerInsideTarget(i))
                {
                    _isScanned[i] = true;
                    _scannedCount++;
                    SetZoneHighlight(i, false);
                    break;
                }
            }
        }

        if (_targets.Length > 0 && _scannedCount >= _targets.Length)
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

    private bool IsScannerInsideTarget(int index)
    {
        if (_targetColliders == null || index < 0 || index >= _targetColliders.Length)
        {
            return false;
        }

        Collider targetZone = _targetColliders[index];
        if (targetZone == null || _scannerItem == null || _scannerItem.ItemCollider == null)
        {
            return false;
        }

        return _scannerItem.ItemCollider.bounds.Intersects(targetZone.bounds);
    }

    private void CacheTargetComponents()
    {
        if (_targets == null)
        {
            return;
        }

        for (int i = 0; i < _targets.Length; i++)
        {
            GameObject target = _targets[i];
            if (target == null)
            {
                continue;
            }

            _targetColliders[i] = target.GetComponent<Collider>();
            if (_targetColliders[i] == null)
            {
                _targetColliders[i] = target.GetComponentInChildren<Collider>(true);
            }

            if (_targetColliders[i] != null)
            {
                _zoneHighlights[i] = QuestHighlightUtility.CreateHighlight(_targetColliders[i], QuestHighlightUtility.DefaultHighlightColor, QuestHighlightUtility.DefaultHighlightAlpha, true);
            }
        }
    }

    private void SetZoneHighlight(int index, bool value)
    {
        if (_zoneHighlights == null || index < 0 || index >= _zoneHighlights.Length)
        {
            return;
        }

        if (_zoneHighlights[index] == null)
        {
            return;
        }

        _zoneHighlights[index].SetColor(QuestHighlightUtility.DefaultHighlightColor, QuestHighlightUtility.DefaultHighlightAlpha);
        _zoneHighlights[index].SetVisible(value);
    }

    private void UpdateHighlights()
    {
        if (_targets == null)
        {
            return;
        }

        for (int i = 0; i < _targets.Length; i++)
        {
            bool show = _isTaskActive && _isTaskCompleted == false && _isScanned != null && i < _isScanned.Length && _isScanned[i] == false;
            SetZoneHighlight(i, show);
        }
    }
}
