using TMPro;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField] private Transform _player;
    [SerializeField] private float _interactDistance = 2.25f;
    [SerializeField] private KeyCode _key = KeyCode.E;

    [SerializeField] private TMP_Text _promptLabel;
    [SerializeField] private string _promptText = "Нажмите [E], чтобы взаимодействовать";

    [SerializeField] private Renderer _highlightRenderer;
    [SerializeField] private Color _highlightColor = new Color(1f, 1f, 0.5f, 1f);

    private Color _defaultColor;
    private bool _isInside;
    private bool _isBusy;

    public delegate void InteractHandler();
    public event InteractHandler OnInteract;

    private void Awake()
    {
        _defaultColor = _highlightRenderer.material.color;
        _promptLabel.gameObject.SetActive(false);
    }

    private void Update()
    {
        float dist = Vector3.Distance(_player.position, transform.position);

        if (dist <= _interactDistance)
        {
            if (_isInside == false)
            {
                _isInside = true;
                _promptLabel.text = _promptText;
                _promptLabel.gameObject.SetActive(true);
                _highlightRenderer.material.color = _highlightColor;
            }

            if (_isBusy == false)
            {
                if (Input.GetKeyDown(_key))
                {
                    _isBusy = true;
                    if (OnInteract != null)
                    {
                        OnInteract();
                    }
                }
            }
        }
        else
        {
            if (_isInside == true)
            {
                _isInside = false;
                _promptLabel.gameObject.SetActive(false);
                _highlightRenderer.material.color = _defaultColor;
            }
        }
    }

    public void ReleaseBusy()
    {
        _isBusy = false;
    }
}