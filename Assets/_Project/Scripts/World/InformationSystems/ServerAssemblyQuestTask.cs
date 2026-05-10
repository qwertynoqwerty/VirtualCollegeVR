using UltimateXR.Manipulation;
using UnityEngine;
using VRCollege.Net;
using VRCollege.Net.Dto;

public sealed class ServerAssemblyQuestTask : MonoBehaviour
{
    [Header("Quest")]
    [SerializeField] private NpcQuestRuntime _npcQuestRuntime;
    [SerializeField] private ProgressStatesApi _progressStatesApi;

    [Header("Assembly Order")]
    [SerializeField] private ServerAssemblyPart[] _assemblyParts;

    private const float SyncIntervalSeconds = 1.0f;

    private bool _isTaskActive;
    private bool _isTaskCompleted;
    private int _npcId;
    private int _currentStep;
    private float _nextSyncTime;
    private bool _syncInFlight;
    private bool[] _partWasGrabbed;

    private void Awake()
    {
        ResetTaskVisuals();
    }

    private void Start()
    {
        _npcId = _npcQuestRuntime != null ? _npcQuestRuntime.GetNpcId() : 0;
        _nextSyncTime = Time.time + 0.15f;
    }

    private void Update()
    {
        TryAutoActivateFromQuestState();
        UpdateGrabState();
        UpdateHighlights();

        if (_isTaskActive == false || _isTaskCompleted)
        {
            return;
        }

        if (_currentStep < 0 || _assemblyParts == null || _currentStep >= _assemblyParts.Length)
        {
            return;
        }

        ServerAssemblyPart current = _assemblyParts[_currentStep];
        if (current == null || current.GrabbableObject == null)
        {
            return;
        }

        bool wasGrabbed = _partWasGrabbed != null && _currentStep >= 0 && _currentStep < _partWasGrabbed.Length && _partWasGrabbed[_currentStep];
        if (wasGrabbed && current.IsPartInsideSlot() && current.GrabbableObject.IsBeingGrabbed == false)
        {
            SnapPartToSlot(current);
            AdvanceStep();
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

        var states = await _progressStatesApi.GetStatesAsync();
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

    private void SnapPartToSlot(ServerAssemblyPart part)
    {
        UxrGrabbableObject grabbable = part.GrabbableObject;
        if (grabbable != null)
        {
            grabbable.ReleaseGrabs(true);
            if (grabbable.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.isKinematic = true;
            }
        }

        Transform partTransform = grabbable != null ? grabbable.transform : null;
        if (partTransform != null)
        {
            partTransform.SetPositionAndRotation(part.SlotSnapPoint.position, part.SlotSnapPoint.rotation);
        }
    }

    private void AdvanceStep()
    {
        _currentStep++;
        if (_currentStep >= _assemblyParts.Length)
        {
            CompleteQuestTask();
            return;
        }

        UpdateHighlights();
    }

    private void ResetTaskVisuals()
    {
        _currentStep = 0;
        if (_assemblyParts != null)
        {
            _partWasGrabbed = new bool[_assemblyParts.Length];
        }
        UpdateHighlights();
    }

    private void UpdateHighlights()
    {
        if (_assemblyParts == null)
        {
            return;
        }

        for (int i = 0; i < _assemblyParts.Length; i++)
        {
            ServerAssemblyPart part = _assemblyParts[i];
            if (part == null)
            {
                continue;
            }

            bool activeStep = _isTaskActive && _isTaskCompleted == false && i == _currentStep;
            bool currentPartGrabbed = activeStep && part.GrabbableObject != null && part.GrabbableObject.IsBeingGrabbed;
            part.SetPartHighlight(activeStep);
            part.SetSlotHighlight(currentPartGrabbed);
        }
    }

    private void UpdateGrabState()
    {
        if (_assemblyParts == null || _partWasGrabbed == null)
        {
            return;
        }

        for (int i = 0; i < _assemblyParts.Length; i++)
        {
            ServerAssemblyPart part = _assemblyParts[i];
            if (part == null || part.GrabbableObject == null)
            {
                continue;
            }

            bool isGrabbedNow = part.GrabbableObject.IsBeingGrabbed;
            if (isGrabbedNow)
            {
                _partWasGrabbed[i] = true;
            }
        }
    }

}
