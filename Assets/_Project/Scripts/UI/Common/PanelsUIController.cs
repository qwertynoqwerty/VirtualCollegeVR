using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRCollege.Net.Dto;

public sealed class PanelsUIController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject _profilePanel;
    [SerializeField] private GameObject _questsPanel;
    [SerializeField] private GameObject _achievementsPanel;

    [Header("Controllers")]
    [SerializeField] private QuestJournalPanelController _questsController;
    [SerializeField] private AchievementsPanelController _achievementsController;

    [Header("Session")]
    [SerializeField] private AuthApi _authApi;
    [SerializeField] private string _authScene = "AuthScene";

    [Header("Input")]
    [SerializeField] private OVRInput.Button _toggleButton = OVRInput.Button.One;
    [SerializeField] private OVRInput.Controller _toggleController = OVRInput.Controller.LTouch;

    private CancellationTokenSource _cts;
    private bool _isLoggingOut;

    private void Awake()
    {
        _cts = new CancellationTokenSource();

        if (_authApi == null)
        {
            _authApi = FindFirstObjectByType<AuthApi>(FindObjectsInactive.Include);
        }
    }

    private void OnDestroy()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }

    private void Update()
    {
        if (OVRInput.GetDown(_toggleButton, _toggleController))
        {
            ToggleHud();
        }
    }

    private void ToggleHud()
    {
        if (CanOpenPanels() == false)
        {
            HideAll();
            return;
        }

        if (IsAnyOpen() == true)
        {
            HideAll();
        }
        else
        {
            ShowProfile();
        }
    }

    public void ShowProfile()
    {
        if (CanOpenPanels() == false)
        {
            HideAll();
            return;
        }

        SetActive(_profilePanel, true);
        SetActive(_questsPanel, false);
        SetActive(_achievementsPanel, false);

        if (_achievementsController != null)
        {
            _ = _achievementsController.Refresh();
        }
    }

    public void ShowQuests()
    {
        if (CanOpenPanels() == false)
        {
            HideAll();
            return;
        }

        SetActive(_profilePanel, false);
        SetActive(_achievementsPanel, false);
        SetActive(_questsPanel, true);

        if (_questsController != null)
        {
            _ = _questsController.Refresh();
        }
    }

    public void ShowAchievements()
    {
        if (CanOpenPanels() == false)
        {
            HideAll();
            return;
        }

        SetActive(_profilePanel, false);
        SetActive(_questsPanel, false);
        SetActive(_achievementsPanel, true);

        if (_achievementsController != null)
        {
            _ = _achievementsController.Refresh();
        }
    }

    public void HideAll()
    {
        SetActive(_profilePanel, false);
        SetActive(_questsPanel, false);
        SetActive(_achievementsPanel, false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public async void LogoutAndGoToAuthScene()
    {
        if (_isLoggingOut)
        {
            return;
        }

        _isLoggingOut = true;

        if (_authApi == null)
        {
            _authApi = FindFirstObjectByType<AuthApi>(FindObjectsInactive.Include);
        }

        if (_authApi != null)
        {
            ApiResult<AuthLogoutResultDto> _ = await _authApi.LogoutAsync(_cts.Token);
        }

        UserSession.Clear();
        JwtStorage.Clear();

        PlayerPrefs.DeleteKey("vrcollege.seen.ach.codes");
        PlayerPrefs.DeleteKey("vrcollege.fail.attempts");
        PlayerPrefs.Save();

        if (string.IsNullOrEmpty(_authScene) == false)
        {
            SceneManager.LoadScene(_authScene);
        }

        _isLoggingOut = false;
    }

    private bool IsAnyOpen()
    {
        if (_profilePanel != null && _profilePanel.activeSelf == true) return true;
        if (_questsPanel != null && _questsPanel.activeSelf == true) return true;
        if (_achievementsPanel != null && _achievementsPanel.activeSelf == true) return true;
        return false;
    }

    private static bool CanOpenPanels()
    {
        return UserSession.LoadTutorialCompleted();
    }

    private static void SetActive(GameObject go, bool state)
    {
        if (go != null)
        {
            go.SetActive(state);
        }
    }
}
