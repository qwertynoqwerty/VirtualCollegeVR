using UnityEngine;

public sealed class PanelsUIController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject _profilePanel;
    [SerializeField] private GameObject _questsPanel;
    [SerializeField] private GameObject _achievementsPanel;

    [Header("Controllers (optional)")]
    [SerializeField] private QuestJournalPanelController _questsController;
    [SerializeField] private ProfilePanelController _achievementsController;

    [SerializeField] private ProfilePanelController _profileController;

    [Header("Input")]
    [SerializeField] private bool _toggleByKeyboard = true;
    [SerializeField] private KeyCode _toggleKey = KeyCode.E;

    private void Update()
    {
        if (_toggleByKeyboard == true)
        {
            if (Input.GetKeyDown(_toggleKey) == true)
            {
                ToggleHud();
            }
        }
    }
    
    public void ToggleHud()
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
    
    private bool IsAnyOpen()
    {
        bool open = false;

        if (_profilePanel != null)
        {
            if (_profilePanel.activeSelf == true)
            {
                open = true;
            }
            else
            {
                open = false;
            }
        }

        if (open == false)
        {
            if (_questsPanel != null)
            {
                if (_questsPanel.activeSelf == true)
                {
                    open = true;
                }
            }
        }

        if (open == false)
        {
            if (_achievementsPanel != null)
            {
                if (_achievementsPanel.activeSelf == true)
                {
                    open = true;
                }
            }
        }

        return open;
    }

    private static void SetActive(GameObject go, bool state)
    {
        if (go != null)
        {
            go.SetActive(state);
        }
    }
}
