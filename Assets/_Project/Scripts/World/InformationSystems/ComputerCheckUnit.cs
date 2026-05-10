using UnityEngine;
using UnityEngine.UI;

public sealed class ComputerCheckUnit : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _screenVisual;

    private Button _powerButton;

    public Button PowerButton
    {
        get
        {
            if (_powerButton == null)
            {
                _powerButton = GetComponent<Button>();
            }

            return _powerButton;
        }
    }

    private void Awake()
    {
        if (_powerButton == null)
        {
            _powerButton = GetComponent<Button>();
        }
    }

    public void SetPowerState(bool isOn)
    {
        if (_screenVisual != null)
        {
            _screenVisual.SetActive(isOn);
        }
    }
}
