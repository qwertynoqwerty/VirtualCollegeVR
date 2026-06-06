using TMPro;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public static bool IsPlayerInsideAnyInteractable => _playerInsideCount > 0;

    [Header("Prompt")]
    [SerializeField] private GameObject _promptRoot;
    [SerializeField] private TMP_Text _promptLabel;

    private OVRInput.Controller _inputController = OVRInput.Controller.RTouch;
    private OVRInput.Button _inputButton = OVRInput.Button.PrimaryHandTrigger;
    private string _playerTag = "Player";

    private string _promptText = "Нажмите на курок, чтобы поговорить";
    private bool _insideTrigger;
    private bool _isBusy;
    private bool _persistentVisible;
    private bool _interactionEnabled = true;
    private bool _countedInside;
    private static int _playerInsideCount;

    public delegate void InteractHandler();
    public event InteractHandler OnInteract;

    private void Awake()
    {
        if (_promptRoot != null)
        {
            _promptRoot.SetActive(false);
        }
        else if (_promptLabel != null)
        {
            _promptLabel.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (_interactionEnabled == false)
        {
            return;
        }

        if (_insideTrigger == false)
        {
            return;
        }

        if (_isBusy)
        {
            return;
        }

        if (OVRInput.GetDown(_inputButton, _inputController))
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

    public void SetInteractionEnabled(bool value)
    {
        _interactionEnabled = value;

        if (_interactionEnabled == false)
        {
            _isBusy = false;
            HidePrompt();
        }
        else if (_persistentVisible == true)
        {
            ShowPrompt();
        }
    }

    public void SetPersistentVisible(bool value)
    {
        _persistentVisible = value;

        if (_persistentVisible == true)
        {
            ShowPrompt();
        }
        else
        {
            HidePrompt();
        }
    }

    public void HideVisualNow()
    {
        _persistentVisible = false;
        HidePrompt();
    }

    private void ShowPrompt()
    {
        if (_promptRoot != null)
        {
            _promptRoot.SetActive(true);
        }

        if (_promptLabel != null)
        {
            _promptLabel.text = _promptText;
            if (_promptRoot == null)
            {
                _promptLabel.gameObject.SetActive(true);
            }
        }

    }

    private void HidePrompt()
    {
        if (_promptLabel != null)
        {
            _promptLabel.text = string.Empty;
            if (_promptRoot == null)
            {
                _promptLabel.gameObject.SetActive(false);
            }
        }

        if (_promptRoot != null)
        {
            _promptRoot.SetActive(false);
        }
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
