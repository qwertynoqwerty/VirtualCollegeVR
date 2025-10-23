using TMPro;
using UnityEngine;

public sealed class TutorialController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject _panel;
    [SerializeField] private TMP_Text _text;

    [Header("Flow")]
    [SerializeField] private bool _autoShowFromFirstLogin = true;
    [SerializeField] private KeyCode _continueKey = KeyCode.E;

    [Header("Teleport")]
    [SerializeField] private Transform _teleportTarget;
    [SerializeField] private CharacterController _player;

    private bool _isShown;

    private void Start()
    {
        if (_panel != null)
        {
            _panel.SetActive(false);
        }

        if (_autoShowFromFirstLogin)
        {
            bool isGuest = !JwtStorage.HasToken();
            if (isGuest)
            {
                return;
            }

            bool first = UserSession.ConsumeIsFirstLogin();
            if (first)
            {
                Show("Добро пожаловать в VRCollege! Нажмите E, чтобы продолжить.");
            }
        }
    }

    public void Show(string message)
    {
        if (_text != null)
        {
            _text.text = message;
        }

        if (_panel != null)
        {
            _panel.SetActive(true);
        }

        _isShown = true;
    }

    private void Update()
    {
        if (_isShown)
        {
            if (Input.GetKeyDown(_continueKey))
            {
                HideAndTeleport();
            }
        }
    }

    private void HideAndTeleport()
    {
        if (_panel != null)
        {
            _panel.SetActive(false);
        }

        _isShown = false;

        if (_player != null && _teleportTarget != null)
        {
            _player.enabled = false;
            _player.transform.position = _teleportTarget.position;
            _player.transform.rotation = _teleportTarget.rotation;
            _player.enabled = true;
        }
    }
}