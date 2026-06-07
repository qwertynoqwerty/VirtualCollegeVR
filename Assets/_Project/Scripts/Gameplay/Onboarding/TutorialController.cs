using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRCollege.Net;
using VRCollege.Net.Dto;

public sealed class TutorialController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject _panel;
    [SerializeField] private Button _actionButton;

    [Header("Dialogue")]
    [SerializeField] private DialoguePlayer _dialoguePlayer;
    [SerializeField] private TutorialDialogueProfile _tutorialProfile;
    [SerializeField] private ProfileApi _profileApi;

    private bool _autoShowWhenTutorialIncomplete = true;

    private int _currentStepIndex = -1;
    private bool _isActive;
    private bool _isSequenceRunning;
    private CancellationTokenSource _cts;
    private Canvas _ownPanelCanvas;
    private GraphicRaycaster _ownPanelRaycaster;

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

        if (_profileApi == null)
        {
            _profileApi = FindFirstObjectByType<ProfileApi>(FindObjectsInactive.Include);
        }

        if (_panel == gameObject)
        {
            _ownPanelCanvas = GetComponent<Canvas>();
            _ownPanelRaycaster = GetComponent<GraphicRaycaster>();
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

    private async void Start()
    {
        if (_autoShowWhenTutorialIncomplete == false)
        {
            HidePanel();
            return;
        }

        bool tutorialCompleted = await ResolveTutorialCompletedAsync();
        if (_cts == null || _cts.IsCancellationRequested)
        {
            return;
        }

        if (tutorialCompleted)
        {
            HidePanel();
            return;
        }

        ShowPanel();
    }

    private void Update()
    {
        if (_isActive && IsPanelVisible() == false)
        {
            SetPanelVisible(true);
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

        SetPanelVisible(true);
    }

    public void HidePanel()
    {
        _isActive = false;
        SetPanelVisible(false);
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

    private async System.Threading.Tasks.Task<bool> ResolveTutorialCompletedAsync()
    {
        if (_profileApi != null)
        {
            ApiResult<ProfileDto> res = await _profileApi.GetProfileAsync();
            if (res.IsSuccess && res.Data != null)
            {
                UserSession.SaveTutorialCompleted(res.Data.tutorialCompleted);
                Debug.Log("[TutorialController] Tutorial state loaded from server: " + res.Data.tutorialCompleted);
                return res.Data.tutorialCompleted;
            }

            Debug.LogWarning("[TutorialController] Profile request failed. Falling back to local tutorial state. Error: " + res.Error);
        }

        bool hasCachedState = UserSession.TryLoadTutorialCompleted(out bool cachedCompleted);
        Debug.Log("[TutorialController] Tutorial state loaded from cache. HasState: " + hasCachedState + ", Completed: " + cachedCompleted);
        return hasCachedState && cachedCompleted;
    }

    private bool IsPanelVisible()
    {
        if (_panel == null)
        {
            return false;
        }

        if (_panel == gameObject && _ownPanelCanvas != null)
        {
            return _ownPanelCanvas.enabled;
        }

        return _panel.activeSelf;
    }

    private void SetPanelVisible(bool visible)
    {
        if (_panel == null)
        {
            return;
        }

        if (_panel == gameObject)
        {
            if (_ownPanelCanvas != null)
            {
                _ownPanelCanvas.enabled = visible;
            }

            if (_ownPanelRaycaster != null)
            {
                _ownPanelRaycaster.enabled = visible;
            }

            return;
        }

        _panel.SetActive(visible);
    }
}
