using TMPro;
using UltimateXR.Avatar;
using UltimateXR.Core;
using UltimateXR.Devices;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public static bool IsPlayerInsideAnyInteractable => _playerInsideCount > 0;

    [Header("Trigger")]
    [SerializeField] private string _playerTag = "Player";

    [Header("Input")]
    [SerializeField] private UxrHandSide _handSide = UxrHandSide.Left;
    [SerializeField] private UxrInputButtons _inputButton = UxrInputButtons.Button1;
    [SerializeField] private bool _acceptButton2 = true;

    [Header("Prompt")]
    [SerializeField] private TMP_Text _promptLabel;
    [SerializeField] private string _promptText = "Нажмите кнопку контроллера, чтобы взаимодействовать";

    [Header("Highlight")]
    [SerializeField] private Renderer _highlightRenderer;
    [SerializeField] private Color _highlightColor = new Color(1f, 1f, 0.5f, 1f);
    [SerializeField] private bool _pulseHighlight = true;
    [SerializeField] private float _pulseSpeed = 3.0f;

    private Color _defaultColor;
    private bool _insideTrigger;
    private bool _isBusy;
    private bool _persistentVisible;
    private Material _highlightMaterial;
    private bool _countedInside;
    private static int _playerInsideCount;

    public delegate void InteractHandler();
    public event InteractHandler OnInteract;

    private void Awake()
    {
        if (_highlightRenderer != null)
        {
            _highlightMaterial = _highlightRenderer.material;
            _defaultColor = _highlightMaterial.color;
        }

        if (_promptLabel != null)
        {
            _promptLabel.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (_persistentVisible == true)
        {
            UpdateHighlightPulse();
        }

        if (_insideTrigger == false)
        {
            return;
        }

        if (_isBusy)
        {
            return;
        }

        bool pressedPrimary = UxrAvatar.LocalAvatarInput.GetButtonsPressDown(_handSide, _inputButton);
        bool pressedAlt = _acceptButton2 && UxrAvatar.LocalAvatarInput.GetButtonsPressDown(_handSide, UxrInputButtons.Button2);

        if (pressedPrimary || pressedAlt)
        {
            _isBusy = true;
            OnInteract?.Invoke();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsPlayerCollider(other) == false)
        {
            return;
        }

        _insideTrigger = true;
        if (_countedInside == false)
        {
            _countedInside = true;
            _playerInsideCount++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsPlayerCollider(other) == false)
        {
            return;
        }

        _insideTrigger = false;
        _isBusy = false;
        if (_countedInside == true)
        {
            _countedInside = false;
            _playerInsideCount = Mathf.Max(0, _playerInsideCount - 1);
        }
    }

    private void OnDisable()
    {
        if (_countedInside == true)
        {
            _countedInside = false;
            _insideTrigger = false;
            _isBusy = false;
            _playerInsideCount = Mathf.Max(0, _playerInsideCount - 1);
        }
    }

    public void ReleaseBusy()
    {
        _isBusy = false;
    }

    public void SetPersistentVisible(bool value)
    {
        _persistentVisible = value;

        if (_persistentVisible == true)
        {
            ShowPromptAndHighlight();
        }
        else
        {
            HidePromptAndHighlight();
        }
    }

    public void HideVisualNow()
    {
        _persistentVisible = false;
        HidePromptAndHighlight();
    }

    private void ShowPromptAndHighlight()
    {
        if (_promptLabel != null)
        {
            _promptLabel.text = _promptText;
            _promptLabel.gameObject.SetActive(true);
        }

        if (_highlightMaterial != null)
        {
            _highlightMaterial.color = _highlightColor;
        }
    }

    private void HidePromptAndHighlight()
    {
        if (_promptLabel != null)
        {
            _promptLabel.gameObject.SetActive(false);
        }

        if (_highlightMaterial != null)
        {
            _highlightMaterial.color = _defaultColor;
        }
    }

    private void UpdateHighlightPulse()
    {
        if (_pulseHighlight == false || _highlightMaterial == null)
        {
            return;
        }

        float t = Mathf.PingPong(Time.time * _pulseSpeed, 1f);
        _highlightMaterial.color = Color.Lerp(_defaultColor, _highlightColor, t);
    }

    private bool IsPlayerCollider(Collider other)
    {
        if (other == null)
        {
            return false;
        }

        if (other.CompareTag(_playerTag))
        {
            return true;
        }

        Transform t = other.transform;
        while (t != null)
        {
            if (t.CompareTag(_playerTag))
            {
                return true;
            }

            t = t.parent;
        }

        if (other.attachedRigidbody != null)
        {
            Transform r = other.attachedRigidbody.transform;
            while (r != null)
            {
                if (r.CompareTag(_playerTag))
                {
                    return true;
                }

                r = r.parent;
            }
        }

        return false;
    }
}
