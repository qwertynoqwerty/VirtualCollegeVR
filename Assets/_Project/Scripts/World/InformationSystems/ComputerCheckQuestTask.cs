using UnityEngine;
using UnityEngine.Events;
using VRCollege.Net;
using VRCollege.Net.Dto;

public sealed class ComputerCheckQuestTask : MonoBehaviour
{
    [Header("Quest")]
    [SerializeField] private NpcQuestRuntime _npcQuestRuntime;
    [SerializeField] private ProgressStatesApi _progressStatesApi;

    [Header("Computers")]
    [SerializeField] private ComputerCheckUnit[] _computers;
    [SerializeField] private int _faultyComputerIndex = 0;

    private const float SyncIntervalSeconds = 1.0f;

    private bool _isTaskActive;
    private bool _isTaskCompleted;
    private int _npcId;
    private float _nextSyncTime;
    private bool _syncInFlight;
    private UnityAction[] _powerHandlers;

    private void Awake()
    {
        BindPowerButtons();
        ResetComputers();
    }

    private void Start()
    {
        _npcId = _npcQuestRuntime != null ? _npcQuestRuntime.GetNpcId() : 0;
        _nextSyncTime = Time.time + 0.15f;
    }

    private void OnDestroy()
    {
        UnbindPowerButtons();
    }

    private void Update()
    {
        TryAutoActivateFromQuestState();
    }

    public void ActivateQuestTask()
    {
        if (_isTaskCompleted)
        {
            return;
        }

        _isTaskActive = true;
    }

    public void CompleteQuestTask()
    {
        if (_isTaskCompleted)
        {
            return;
        }

        _isTaskCompleted = true;

        if (_npcQuestRuntime != null)
        {
            _npcQuestRuntime.MarkQuestCompleted();
        }
    }

    private void BindPowerButtons()
    {
        if (_computers == null)
        {
            return;
        }

        _powerHandlers = new UnityAction[_computers.Length];

        for (int i = 0; i < _computers.Length; i++)
        {
            int index = i;
            ComputerCheckUnit unit = _computers[index];
            if (unit == null || unit.PowerButton == null)
            {
                continue;
            }

            _powerHandlers[index] = () => OnComputerPowerPressed(index);
            unit.PowerButton.onClick.AddListener(_powerHandlers[index]);
        }
    }

    private void UnbindPowerButtons()
    {
        if (_computers == null || _powerHandlers == null)
        {
            return;
        }

        for (int i = 0; i < _computers.Length; i++)
        {
            ComputerCheckUnit unit = _computers[i];
            if (unit == null || unit.PowerButton == null || i >= _powerHandlers.Length || _powerHandlers[i] == null)
            {
                continue;
            }

            unit.PowerButton.onClick.RemoveListener(_powerHandlers[i]);
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

    private void OnComputerPowerPressed(int computerIndex)
    {
        if (_isTaskActive == false)
        {
            return;
        }

        if (_computers == null || computerIndex < 0 || computerIndex >= _computers.Length)
        {
            return;
        }

        ComputerCheckUnit unit = _computers[computerIndex];
        if (unit == null)
        {
            return;
        }

        int faultyIndex = Mathf.Clamp(_faultyComputerIndex, 0, _computers.Length - 1);
        bool isFaulty = computerIndex == faultyIndex;

        if (isFaulty)
        {
            unit.SetPowerState(false);
            CompleteQuestTask();
            return;
        }

        unit.SetPowerState(true);
    }

    private void ResetComputers()
    {
        if (_computers == null)
        {
            return;
        }

        for (int i = 0; i < _computers.Length; i++)
        {
            ComputerCheckUnit unit = _computers[i];
            if (unit != null)
            {
                unit.SetPowerState(false);
            }
        }
    }

}
