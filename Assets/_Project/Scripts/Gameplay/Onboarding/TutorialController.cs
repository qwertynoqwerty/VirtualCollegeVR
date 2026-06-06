using System.Threading;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class TutorialController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject _panel;
    [SerializeField] private Button _actionButton;

    [Header("Dialogue")]
    [SerializeField] private DialoguePlayer _dialoguePlayer;
    [SerializeField] private TutorialDialogueProfile _tutorialProfile;

    private bool _autoShowWhenTutorialIncomplete = true;

    private int _currentStepIndex = -1;
    private bool _isActive;
    private bool _isSequenceRunning;
    private CancellationTokenSource _cts;

    public event Action Completed;

    private void Awake()
    {
        _cts = new CancellationTokenSource();

        if (_dialoguePlayer != null)
        {
            _dialoguePlayer.SetAutoToggleRoot(false);
        }

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

    private void Update()
    {
        if (_isActive && _panel != null && _panel.activeSelf == false)
        {
            _panel.SetActive(true);
        }
    }

    public void ShowPanel()
    {
        _isActive = true;
        _currentStepIndex = -1;

        if (_actionButton != null)
        {
            _actionButton.gameObject.SetActive(true);
        }

        if (_dialoguePlayer != null)
        {
            _dialoguePlayer.SetAutoToggleRoot(false);
        }

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

        if (_isSequenceRunning || (_dialoguePlayer != null && _dialoguePlayer.IsPlaying))
        {
            return;
        }

        if (_actionButton != null)
        {
            _actionButton.gameObject.SetActive(false);
        }

        await PlayFullTutorialAsync();
    }

    private async System.Threading.Tasks.Task PlayFullTutorialAsync()
    {
        if (_tutorialProfile == null || _dialoguePlayer == null)
        {
            HidePanel();
            Completed?.Invoke();
            return;
        }

        if (_tutorialProfile.Count <= 0)
        {
            HidePanel();
            Completed?.Invoke();
            return;
        }

        _isSequenceRunning = true;
        try
        {
            _currentStepIndex = 0;
            while (_currentStepIndex < _tutorialProfile.Count)
            {
                DialogueLine line = _tutorialProfile.GetLine(_currentStepIndex);
                if (line != null)
                {
                    await _dialoguePlayer.PlayLineAsync(line, _cts.Token);
                }

                _currentStepIndex++;
            }
        }
        finally
        {
            _isSequenceRunning = false;
            HidePanel();
            Completed?.Invoke();
        }
    }
}
