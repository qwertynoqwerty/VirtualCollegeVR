using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using VRCollege.Net;
using VRCollege.Net.Dto;

public class NpcQuestRuntime : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private QuestNpcDialogueDatabase _dialogueDatabase;
    [SerializeField] private Animator _animator;
    [SerializeField] private Interactable _interactable;
    [SerializeField] private DialoguePlayer _dialoguePlayer;
    [SerializeField] private ProgressApi _progressApi;
    [SerializeField] private ProgressStatesApi _progressStatesApi;
    [SerializeField] private GlobalToast _toast;

    [Header("NPC")]
    [SerializeField] private int _npcId;
    [SerializeField] private TMP_Text _npcNameLabel;

    private float _debounceSeconds = 0.4f;
    private float _toastVisibleSeconds = 2.0f;
    private string _questTakenToast = "Задание получено";
    private string _questCompletedToast = "Задание выполнено";
    private string _talkingParameter = "IsTalking";
    private string _defaultStateName = "Relax";
    private string _talkStateName = "Talk";
    private float _animationTransitionSeconds = 0.05f;
    private float _achievementToastDelayAfterQuestSeconds = 1.5f;

    private enum QuestState
    {
        Unknown = 0,
        NotStarted = ProgressStateDto.NotStarted,
        InProgress = ProgressStateDto.InProgress,
        InProgressRepeat = ProgressStateDto.InProgressRepeat,
        CompletedPendingDialogue = ProgressStateDto.CompletedPendingDialogue,
        CompletedRepeat = ProgressStateDto.CompletedRepeat
    }

    private QuestState _state = QuestState.Unknown;
    private bool _isBusy;
    private bool _specialtyAllowed = true;
    private int _talkingHash;
    private int _defaultStateHash;
    private int _talkStateHash;

    private void Awake()
    {
        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
        }

        _talkingHash = Animator.StringToHash(_talkingParameter);
        _defaultStateHash = Animator.StringToHash(_defaultStateName);
        _talkStateHash = Animator.StringToHash(_talkStateName);
        SetTalking(false);

        if (_toast == null)
        {
            _toast = FindFirstObjectByType<GlobalToast>(FindObjectsInactive.Include);
        }

        if (_progressStatesApi == null)
        {
            _progressStatesApi = FindFirstObjectByType<ProgressStatesApi>(FindObjectsInactive.Include);
        }

        if (_interactable != null)
        {
            _interactable.OnInteract += HandleInteract;
            _interactable.SetPersistentVisible(true);
        }
    }

    private async void Start()
    {
        await RefreshStateFromApiAsync();
        ApplyInteractableVisualState();
    }

    private void OnDestroy()
    {
        if (_interactable != null)
        {
            _interactable.OnInteract -= HandleInteract;
        }

        SetTalking(false);
    }

    private async void HandleInteract()
    {
        if (_isBusy)
        {
            return;
        }

        _isBusy = true;

        await RefreshStateFromApiAsync();

        if (_specialtyAllowed == false)
        {
            await DebounceAndReleaseAsync();
            _isBusy = false;
            return;
        }

        if (_state == QuestState.NotStarted || _state == QuestState.CompletedPendingDialogue)
        {
            _interactable?.HideVisualNow();
        }

        if (_state == QuestState.NotStarted)
        {
            await StartQuestFlowAsync();
        }
        else if (_state == QuestState.InProgress)
        {
            bool ok = await AdvanceStateAsync(ProgressStateDto.InProgress);
            if (ok)
            {
                await PlayDialogueAsync(_dialogueDatabase != null ? _dialogueDatabase.GetInProgressRepeatLines(_npcId) : null);
                _state = QuestState.InProgressRepeat;
                ApplyInteractableVisualState();
            }
        }
        else if (_state == QuestState.InProgressRepeat)
        {
            await PlayDialogueAsync(_dialogueDatabase != null ? _dialogueDatabase.GetInProgressRepeatLines(_npcId) : null);
        }
        else if (_state == QuestState.CompletedPendingDialogue)
        {
            bool ok = await AdvanceStateAsync(ProgressStateDto.CompletedPendingDialogue);
            if (ok)
            {
                await PlayDialogueAsync(_dialogueDatabase != null ? _dialogueDatabase.GetCompleteQuestLines(_npcId) : null);
                _state = QuestState.CompletedRepeat;
                ApplyInteractableVisualState();
            }
        }
        else if (_state == QuestState.CompletedRepeat)
        {
            await PlayDialogueAsync(_dialogueDatabase != null ? _dialogueDatabase.GetCompletedRepeatLines(_npcId) : null);
        }

        await DebounceAndReleaseAsync();
        _isBusy = false;
    }

    public async void MarkQuestCompleted()
    {
        if (_specialtyAllowed == false)
        {
            return;
        }

        if (_isBusy)
        {
            return;
        }

        _isBusy = true;
        await RefreshStateFromApiAsync();

        if (_state != QuestState.InProgress && _state != QuestState.InProgressRepeat)
        {
            await DebounceAndReleaseAsync();
            _isBusy = false;
            return;
        }

        ApiResult<ProgressCompleteResponse> complete = await _progressApi.CompleteQuestAsync(_npcId);
        if (complete.IsSuccess == false || complete.Data == null)
        {
            Debug.LogWarning("[NpcQuestRuntime] CompleteQuestAsync failed for npcId=" + _npcId);
            await DebounceAndReleaseAsync();
            _isBusy = false;
            return;
        }

        _state = QuestState.CompletedPendingDialogue;
        ApplyInteractableVisualState();

        if (_toast != null && string.IsNullOrEmpty(_questCompletedToast) == false)
        {
            _toast.Show(_questCompletedToast, 0f, _toastVisibleSeconds);
        }

        AchievementsNotifier notifier = FindFirstObjectByType<AchievementsNotifier>(FindObjectsInactive.Include);
        if (notifier != null)
        {
            await notifier.CheckAndNotifyAsync(_achievementToastDelayAfterQuestSeconds);
        }

        await DebounceAndReleaseAsync();
        _isBusy = false;
    }

    private async Task StartQuestFlowAsync()
    {
        ApiResult<ProgressStartResponse> start = await _progressApi.StartQuestAsync(_npcId);
        if (start.IsSuccess == false || start.Data == null)
        {
            await RefreshStateFromApiAsync();
            ApplyInteractableVisualState();
            Debug.LogWarning("[NpcQuestRuntime] StartQuestAsync failed for npcId=" + _npcId);
            return;
        }

        _state = QuestState.InProgress;
        ApplyInteractableVisualState();
        await PlayDialogueAsync(_dialogueDatabase != null ? _dialogueDatabase.GetStartQuestLines(_npcId) : null);

        if (_toast != null && string.IsNullOrEmpty(_questTakenToast) == false)
        {
            _toast.Show(_questTakenToast, 0f, _toastVisibleSeconds);
            int ms = Mathf.Max(1, Mathf.RoundToInt(_toastVisibleSeconds * 1000f));
            await Task.Delay(ms);
        }
    }

    private async Task RefreshStateFromApiAsync()
    {
        if (_progressStatesApi == null)
        {
            _state = QuestState.NotStarted;
            return;
        }

        ApiResult<ProgressStateDto[]> res = await _progressStatesApi.GetStatesAsync();
        if (res.IsSuccess == false || res.Data == null)
        {
            _state = QuestState.NotStarted;
            return;
        }

        ProgressStateDto match = null;
        for (int i = 0; i < res.Data.Length; i++)
        {
            if (res.Data[i].npcId == _npcId)
            {
                match = res.Data[i];
                break;
            }
        }

        if (match == null)
        {
            _state = QuestState.NotStarted;
            return;
        }

        if (string.IsNullOrEmpty(match.npcName) == false)
        {
            ApplyNpcName(match.npcName);
        }

        if (match.specialtyId > 0 && UserSession.LoadSpecialtyId() != match.specialtyId)
        {
            _specialtyAllowed = false;
            _state = QuestState.Unknown;
            ApplyInteractableVisualState();
            return;
        }

        _specialtyAllowed = true;

        if (match.status == ProgressStateDto.NotStarted)
        {
            _state = QuestState.NotStarted;
        }
        else if (match.status == ProgressStateDto.InProgress)
        {
            _state = QuestState.InProgress;
        }
        else if (match.status == ProgressStateDto.InProgressRepeat)
        {
            _state = QuestState.InProgressRepeat;
        }
        else if (match.status == ProgressStateDto.CompletedPendingDialogue)
        {
            _state = QuestState.CompletedPendingDialogue;
        }
        else if (match.status == ProgressStateDto.CompletedRepeat)
        {
            _state = QuestState.CompletedRepeat;
        }
        else
        {
            _state = QuestState.NotStarted;
        }
    }

    private async Task<bool> AdvanceStateAsync(int fromStatus)
    {
        if (_progressApi == null)
        {
            return false;
        }

        ApiResult<ProgressAdvanceResponse> res = await _progressApi.AdvanceQuestStateAsync(_npcId, fromStatus);
        if (res.IsSuccess == false || res.Data == null)
        {
            Debug.LogWarning("[NpcQuestRuntime] AdvanceQuestStateAsync failed for npcId=" + _npcId + " fromStatus=" + fromStatus);
            return false;
        }

        return true;
    }

    private async Task PlayDialogueAsync(DialogueLine[] lines)
    {
        if (_dialoguePlayer == null || lines == null || lines.Length == 0)
        {
            return;
        }

        SetTalking(true);
        try
        {
            await _dialoguePlayer.PlayLinesAsync(lines);
        }
        finally
        {
            SetTalking(false);
        }
    }

    private void OnDisable()
    {
        SetTalking(false);
    }

    private void SetTalking(bool value)
    {
        if (_animator == null || string.IsNullOrEmpty(_talkingParameter))
        {
            return;
        }

        _animator.SetBool(_talkingHash, value);
        ForceAnimationState(value ? _talkStateHash : _defaultStateHash);
    }

    private void ForceAnimationState(int stateHash)
    {
        if (_animator == null || stateHash == 0 || _animator.HasState(0, stateHash) == false)
        {
            return;
        }

        float transitionSeconds = Mathf.Max(0f, _animationTransitionSeconds);
        _animator.CrossFadeInFixedTime(stateHash, transitionSeconds, 0, 0f);
    }

    private async Task DebounceAndReleaseAsync()
    {
        int ms = Mathf.Max(1, Mathf.RoundToInt(_debounceSeconds * 1000f));
        await Task.Delay(ms);

        if (_interactable != null)
        {
            _interactable.ReleaseBusy();
        }
    }

    public int GetNpcId()
    {
        return _npcId;
    }

    public void ApplyExternalCompletedState()
    {
        _state = QuestState.CompletedPendingDialogue;
        ApplyInteractableVisualState();
    }

    private void ApplyInteractableVisualState()
    {
        if (_interactable == null)
        {
            return;
        }

        if (_specialtyAllowed == false)
        {
            _interactable.SetPersistentVisible(false);
            _interactable.HideVisualNow();
            return;
        }

        bool alwaysVisible = _state == QuestState.NotStarted || _state == QuestState.CompletedPendingDialogue;
        _interactable.SetPersistentVisible(alwaysVisible);
    }

    private void ApplyNpcName(string npcName)
    {
        if (_npcNameLabel == null)
        {
            return;
        }

        _npcNameLabel.text = string.IsNullOrEmpty(npcName) ? string.Empty : npcName;
    }
}

