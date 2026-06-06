using UnityEngine;
using VRCollege.Net;
using VRCollege.Net.Dto;

public sealed class ServerBatteryAssemblyQuestTask : MonoBehaviour
{
    [System.Serializable]
    private sealed class BatterySlotBinding
    {
        public AssemblyComponentSocket slot;
        public AssemblyComponentItem requiredBattery;
    }

    [Header("Quest")]
    [SerializeField] private NpcQuestRuntime _npcQuestRuntime;
    [SerializeField] private ProgressStatesApi _progressStatesApi;

    [Header("Assembly")]
    [SerializeField] private BatterySlotBinding[] _slots = new BatterySlotBinding[4];

    private const float SyncIntervalSeconds = 1.0f;

    private bool _isTaskActive;
    private bool _isTaskCompleted;
    private int _npcId;
    private float _nextSyncTime;
    private bool _syncInFlight;

    private void Awake()
    {
        SetSlotsQuestActive(false);
    }

    private void Start()
    {
        _npcId = _npcQuestRuntime != null ? _npcQuestRuntime.GetNpcId() : 0;
        _nextSyncTime = Time.time + 0.15f;
    }

    private void Update()
    {
        TryAutoActivateFromQuestState();

        if (_isTaskActive == false || _isTaskCompleted)
        {
            return;
        }

        if (AreAllSlotsCorrect())
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
        SetSlotsQuestActive(true);
    }

    public void CompleteQuestTask()
    {
        if (_isTaskCompleted)
        {
            return;
        }

        _isTaskCompleted = true;
        _isTaskActive = false;
        SetSlotsQuestActive(false);
        LockInsertedBatteries();

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

    private bool AreAllSlotsCorrect()
    {
        if (_slots == null || _slots.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < _slots.Length; i++)
        {
            BatterySlotBinding binding = _slots[i];
            if (binding == null || binding.slot == null || binding.requiredBattery == null)
            {
                return false;
            }

            if (binding.slot.InsertedBattery != binding.requiredBattery)
            {
                return false;
            }
        }

        return true;
    }

    private void LockInsertedBatteries()
    {
        if (_slots == null)
        {
            return;
        }

        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i] != null && _slots[i].slot != null && _slots[i].slot.InsertedBattery != null)
            {
                _slots[i].slot.InsertedBattery.SetInteractionLocked(true);
            }
        }
    }

    private void SetSlotsQuestActive(bool value)
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
                _slots[i].slot.SetExpectedItem(_slots[i].requiredBattery);
            }
        }
    }
}
