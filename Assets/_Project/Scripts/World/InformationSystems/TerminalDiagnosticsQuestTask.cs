using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using VRCollege.Net;
using VRCollege.Net.Dto;

public sealed class TerminalDiagnosticsQuestTask : MonoBehaviour
{
    [System.Serializable]
    private sealed class ModuleCheckBinding
    {
        public string label = "КП";
        public TerminalBatterySocket moduleSocket;
        public Button checkButton;
    }

    [Header("Quest")]
    [SerializeField] private NpcQuestRuntime _npcQuestRuntime;
    [SerializeField] private ProgressStatesApi _progressStatesApi;

    [Header("Terminal")]
    [SerializeField] private ModuleCheckBinding[] _modules;
    [SerializeField] private Button _overallCheckButton;
    [SerializeField] private TMP_Text _statusText;

    [Header("Messages")]
    [SerializeField] private string _msgModuleEmpty = "пустой слот";
    [SerializeField] private string _msgModuleFaulty = "неисправный слот";
    [SerializeField] private string _msgModuleOk = "исправный слот";
    [SerializeField] private string _msgAllNotOk = "Модули неисправны или пусты";
    [SerializeField] private string _msgAllOk = "Все модули исправны";

    private const float SyncIntervalSeconds = 1.0f;

    private bool _isTaskActive;
    private bool _isTaskCompleted;
    private int _npcId;
    private float _nextSyncTime;
    private bool _syncInFlight;
    private UnityAction[] _moduleHandlers;

    private void Awake()
    {
        if (_modules != null)
        {
            _moduleHandlers = new UnityAction[_modules.Length];
            for (int i = 0; i < _modules.Length; i++)
            {
                int index = i;
                if (_modules[index] != null && _modules[index].checkButton != null)
                {
                    _moduleHandlers[index] = () => OnModuleCheckPressed(index);
                    _modules[index].checkButton.onClick.AddListener(_moduleHandlers[index]);
                }
            }
        }

        if (_overallCheckButton != null)
        {
            _overallCheckButton.onClick.AddListener(OnOverallCheckPressed);
        }
    }

    private void Start()
    {
        _npcId = _npcQuestRuntime != null ? _npcQuestRuntime.GetNpcId() : 0;
        _nextSyncTime = Time.time + 0.15f;
    }

    private void Update()
    {
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

    private void OnDestroy()
    {
        if (_modules != null)
        {
            for (int i = 0; i < _modules.Length; i++)
            {
                if (_modules[i] != null && _modules[i].checkButton != null && _moduleHandlers != null && i < _moduleHandlers.Length && _moduleHandlers[i] != null)
                {
                    _modules[i].checkButton.onClick.RemoveListener(_moduleHandlers[i]);
                }
            }
        }

        if (_overallCheckButton != null)
        {
            _overallCheckButton.onClick.RemoveListener(OnOverallCheckPressed);
        }
    }

    public void ActivateQuestTask()
    {
        if (_isTaskCompleted)
        {
            return;
        }

        _isTaskActive = true;
        SetSocketsQuestActive(true);
    }

    public void CompleteQuestTask()
    {
        if (_isTaskCompleted)
        {
            return;
        }

        _isTaskCompleted = true;
        SetSocketsQuestActive(false);

        if (_npcQuestRuntime != null)
        {
            _npcQuestRuntime.MarkQuestCompleted();
        }
    }

    private async void RefreshTaskStateFromApi()
    {
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

    private void OnModuleCheckPressed(int moduleIndex)
    {
        if (_isTaskActive == false || _isTaskCompleted)
        {
            return;
        }

        if (_modules == null || moduleIndex < 0 || moduleIndex >= _modules.Length)
        {
            return;
        }

        ModuleCheckBinding binding = _modules[moduleIndex];
        if (binding == null || binding.moduleSocket == null)
        {
            return;
        }

        if (binding.moduleSocket.IsEmpty)
        {
            SetStatus(GetModuleLabel(binding, moduleIndex) + ": " + _msgModuleEmpty);
            return;
        }

        if (binding.moduleSocket.IsFaulty)
        {
            SetStatus(GetModuleLabel(binding, moduleIndex) + ": " + _msgModuleFaulty);
            return;
        }

        SetStatus(GetModuleLabel(binding, moduleIndex) + ": " + _msgModuleOk);
    }

    private void OnOverallCheckPressed()
    {
        if (_isTaskActive == false || _isTaskCompleted)
        {
            return;
        }

        if (AreAllModulesOperational() == false)
        {
            SetStatus(_msgAllNotOk);
            return;
        }

        SetStatus(_msgAllOk);
        CompleteQuestTask();
    }

    private bool AreAllModulesOperational()
    {
        if (_modules == null || _modules.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < _modules.Length; i++)
        {
            if (_modules[i] == null || _modules[i].moduleSocket == null || _modules[i].moduleSocket.IsOperational == false)
            {
                return false;
            }
        }

        return true;
    }

    private void SetStatus(string message)
    {
        if (_statusText != null)
        {
            _statusText.text = message;
        }
    }

    private void SetSocketsQuestActive(bool value)
    {
        if (_modules == null)
        {
            return;
        }

        for (int i = 0; i < _modules.Length; i++)
        {
            if (_modules[i] != null && _modules[i].moduleSocket != null)
            {
                _modules[i].moduleSocket.SetQuestTaskActive(value);
            }
        }
    }

    private static string GetModuleLabel(ModuleCheckBinding binding, int moduleIndex)
    {
        return binding != null && string.IsNullOrEmpty(binding.label) == false
            ? binding.label
            : "Модуль " + (moduleIndex + 1);
    }

}
