using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Devices;
using UnityEngine;

public sealed class PanelsUIController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject _profilePanel;
    [SerializeField] private GameObject _questsPanel;
    [SerializeField] private GameObject _achievementsPanel;

    [Header("Controllers (optional)")]
    [SerializeField] private QuestJournalPanelController _questsController;
    [SerializeField] private AchievementsPanelController _achievementsController;

    private void Update()
    {
        if (UxrAvatar.LocalAvatarInput.GetButtonsPressDown(UxrHandSide.Left, UxrInputButtons.Button1))
        {
            ToggleHud();
        }
    }

    private void ToggleHud()
    {
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
        SetActive(_profilePanel, false);
        SetActive(_questsPanel, false);
        SetActive(_achievementsPanel, true);
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

    private bool IsAnyOpen()
    {
        if (_profilePanel != null && _profilePanel.activeSelf == true) return true;
        if (_questsPanel != null && _questsPanel.activeSelf == true) return true;
        if (_achievementsPanel != null && _achievementsPanel.activeSelf == true) return true;
        return false;
    }

    private static void SetActive(GameObject go, bool state)
    {
        if (go != null)
        {
            go.SetActive(state);
        }
    }
}
