using System.Threading;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class TutorialController : MonoBehaviour
{
    private enum TutorialUiState
    {
        ReadyToStart = 0,
        ReadyToContinue = 1,
        ReadyToFinish = 2
    }

    [Header("UI")]
    [SerializeField] private GameObject _panel;
    [SerializeField] private Button _actionButton;
    [SerializeField] private TMP_Text _actionButtonText;

    [Header("Dialogue")]
    [SerializeField] private DialoguePlayer _dialoguePlayer;
    [SerializeField] private TutorialDialogueProfile _tutorialProfile;

    [Header("Flow")]
    [SerializeField] private bool _autoShowWhenTutorialIncomplete = true;

    [Header("Labels")]
    [SerializeField] private string _labelStart = "Начать знакомство";
    [SerializeField] private string _labelContinue = "Далее";
    [SerializeField] private string _labelFinish = "Завершить";

    private TutorialUiState _uiState = TutorialUiState.ReadyToStart;
    private int _currentStepIndex = -1;
    private bool _isActive;
    private CancellationTokenSource _cts;

    public event Action Completed;

    private void Awake()
    {
        _cts = new CancellationTokenSource();

        if (_actionButton != null)
        {
            _actionButton.onClick.AddListener(OnActionButtonPressed);
        }
    }

    private void OnDestroy()
    {
        if (_actionButton != null)
        {
            _actionButton.onClick.RemoveListener(OnActionButtonPressed);
        }

        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }

    private void Start()
    {
        HidePanel();

        if (_autoShowWhenTutorialIncomplete == false)
        {
            return;
        }

        if (UserSession.LoadTutorialCompleted() == false)
        {
            ShowPanel();
        }
    }

    public void ShowPanel()
    {
        _isActive = true;
        _currentStepIndex = -1;
        SetUiState(TutorialUiState.ReadyToStart);

        if (_panel != null)
        {
            _panel.SetActive(true);
        }
    }

    public void HidePanel()
    {
        _isActive = false;

        if (_panel != null)
        {
            _panel.SetActive(false);
        }
    }

    public async void OnActionButtonPressed()
    {
        if (_isActive == false)
        {
            return;
        }

        if (_dialoguePlayer != null && _dialoguePlayer.IsPlaying)
        {
            return;
        }

        switch (_uiState)
        {
            case TutorialUiState.ReadyToStart:
                _currentStepIndex = 0;
                await PlayCurrentStepAsync();
                break;
            case TutorialUiState.ReadyToContinue:
                _currentStepIndex = _currentStepIndex + 1;
                await PlayCurrentStepAsync();
                break;
            case TutorialUiState.ReadyToFinish:
                HidePanel();
                Completed?.Invoke();
                break;
        }
    }

    private async System.Threading.Tasks.Task PlayCurrentStepAsync()
    {
        if (_tutorialProfile == null || _dialoguePlayer == null)
        {
            SetUiState(TutorialUiState.ReadyToFinish);
            return;
        }

        DialogueLine line = _tutorialProfile.GetLine(_currentStepIndex);
        if (line == null)
        {
            SetUiState(TutorialUiState.ReadyToFinish);
            return;
        }

        await _dialoguePlayer.PlayLineAsync(line, _cts.Token);

        bool isLast = _currentStepIndex >= _tutorialProfile.Count - 1;
        if (isLast)
        {
            SetUiState(TutorialUiState.ReadyToFinish);
        }
        else
        {
            SetUiState(TutorialUiState.ReadyToContinue);
        }
    }

    private void SetUiState(TutorialUiState state)
    {
        _uiState = state;
        if (_actionButtonText == null)
        {
            return;
        }

        switch (_uiState)
        {
            case TutorialUiState.ReadyToStart:
                _actionButtonText.text = _labelStart;
                break;
            case TutorialUiState.ReadyToContinue:
                _actionButtonText.text = _labelContinue;
                break;
            case TutorialUiState.ReadyToFinish:
                _actionButtonText.text = _labelFinish;
                break;
        }
    }
}
