using UnityEngine;
using VRCollege.Net;
using VRCollege.Net.Dto;

public sealed class PcComponentAssemblyQuestTask : MonoBehaviour
{
    [System.Serializable]
    private sealed class ComponentSlotBinding
    {
        public AssemblyComponentSocket slot;
        public AssemblyComponentItem requiredComponent;
    }

    [Header("Quest")]
    [SerializeField] private NpcQuestRuntime _npcQuestRuntime;
    [SerializeField] private ProgressStatesApi _progressStatesApi;

    [Header("PC Assembly")]
    [SerializeField] private ComponentSlotBinding[] _slots = new ComponentSlotBinding[4];

    private const float SyncIntervalSeconds = 1.0f;

    private bool _isTaskActive;
    private bool _isTaskCompleted;
    private int _currentSlotIndex = -1;
    private int _npcId;
    private float _nextSyncTime;
    private bool _syncInFlight;

    private void Start()
    {
        _npcId = _npcQuestRuntime != null ? _npcQuestRuntime.GetNpcId() : 0;
        _nextSyncTime = Time.time + 0.15f;
        SetAllSlotsQuestActive(false);
        SetAllComponentsHighlight(false);
    }

    private void Update()
    {
        TryAdvanceAssemblySequence();

        if (_progressStatesApi == null || _npcId <= 0)
        {
            return;
        }

        if (_syncInFlight || Time.time < _nextSyncTime)
        {
            return;
        }

        _nextSyncTime = Time.time + SyncIntervalSeconds;
        RefreshTaskStateFromApi();
    }

    public void ActivateQuestTask()
    {
        if (_isTaskCompleted)
        {
            return;
        }

        _isTaskActive = true;
        if (_currentSlotIndex < 0)
        {
            _currentSlotIndex = FindNextPendingSlotIndex(0);
        }

        UpdateCurrentSlotHighlight();
    }

    public void CompleteQuestTask()
    {
        if (_isTaskCompleted)
        {
            return;
        }

        _isTaskCompleted = true;
        _isTaskActive = false;
        SetAllSlotsQuestActive(false);
        SetAllComponentsHighlight(false);
        LockInsertedComponents();

        if (_npcQuestRuntime != null)
        {
            _npcQuestRuntime.MarkQuestCompleted();
        }
    }

    private async void RefreshTaskStateFromApi()
    {
        _syncInFlight = true;
        try
        {
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
        }
        finally
        {
            _syncInFlight = false;
        }
    }

    private void TryAdvanceAssemblySequence()
    {
        if (_isTaskActive == false || _isTaskCompleted)
        {
            return;
        }

        if (_slots == null || _slots.Length == 0)
        {
            return;
        }

        if (_currentSlotIndex < 0 || _currentSlotIndex >= _slots.Length)
        {
            _currentSlotIndex = FindNextPendingSlotIndex(0);
            if (_currentSlotIndex < 0)
            {
                CompleteQuestTask();
            }

            return;
        }

        ComponentSlotBinding current = _slots[_currentSlotIndex];
        if (current == null || current.slot == null || current.requiredComponent == null)
        {
            return;
        }

        if (current.slot.InsertedBattery != current.requiredComponent)
        {
            return;
        }

        current.requiredComponent.SetInteractionLocked(true);
        _currentSlotIndex = FindNextPendingSlotIndex(_currentSlotIndex + 1);
        UpdateCurrentSlotHighlight();

        if (_currentSlotIndex < 0)
        {
            CompleteQuestTask();
        }
    }

    private int FindNextPendingSlotIndex(int startIndex)
    {
        if (_slots == null || _slots.Length == 0)
        {
            return -1;
        }

        int clampedStart = Mathf.Clamp(startIndex, 0, _slots.Length);
        for (int i = clampedStart; i < _slots.Length; i++)
        {
            ComponentSlotBinding binding = _slots[i];
            if (binding == null || binding.slot == null || binding.requiredComponent == null)
            {
                continue;
            }

            if (binding.slot.InsertedBattery == binding.requiredComponent)
            {
                continue;
            }

            return i;
        }

        return -1;
    }

    private void UpdateCurrentSlotHighlight()
    {
        if (_slots == null)
        {
            return;
        }

        for (int i = 0; i < _slots.Length; i++)
        {
            ComponentSlotBinding binding = _slots[i];
            if (binding == null || binding.slot == null)
            {
                continue;
            }

            bool isCurrent = _isTaskActive && _isTaskCompleted == false && i == _currentSlotIndex;
            binding.slot.SetQuestTaskActive(isCurrent);
            binding.slot.SetExpectedItem(binding.requiredComponent);
            if (binding.requiredComponent != null)
            {
                binding.requiredComponent.SetQuestHighlight(isCurrent);
            }
        }
    }

    private void SetAllSlotsQuestActive(bool value)
    {
        if (_slots == null)
        {
            return;
        }

        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i] != null && _slots[i].slot != null)
            {
                _slots[i].slot.SetQuestTaskActive(value);
                _slots[i].slot.SetExpectedItem(_slots[i].requiredComponent);
            }
        }
    }

    private void LockInsertedComponents()
    {
        if (_slots == null)
        {
            return;
        }

        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i] == null || _slots[i].slot == null || _slots[i].slot.InsertedBattery == null)
            {
                continue;
            }

            _slots[i].slot.InsertedBattery.SetInteractionLocked(true);
        }
    }

    private void SetAllComponentsHighlight(bool value)
    {
        if (_slots == null)
        {
            return;
        }

        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i] != null && _slots[i].requiredComponent != null)
            {
                _slots[i].requiredComponent.SetQuestHighlight(value);
            }
        }
    }
}

