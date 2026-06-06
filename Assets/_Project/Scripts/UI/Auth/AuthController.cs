using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRCollege.Net.Dto;

public sealed class AuthController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AuthApi _authApi;
    [SerializeField] private SpecialtiesApi _specialtiesApi;

    [Header("Register")]
    [SerializeField] private TMP_InputField _loginInputReg;
    [SerializeField] private TMP_InputField _passwordInputReg;
    [SerializeField] private TMP_InputField _nameInputReg;
    [SerializeField] private TMP_Dropdown _specialtyDropdown;
    [SerializeField] private TMP_Text _errorTextReg;
    [SerializeField] private GameObject _registrationPanel;

    [Header("Login")]
    [SerializeField] private TMP_InputField _loginInputAuth;
    [SerializeField] private TMP_InputField _passwordInputAuth;
    [SerializeField] private TMP_Text _errorTextAuth;
    [SerializeField] private GameObject _authPanel;

    [Header("Scenes")]
    [SerializeField] private string _sceneAfterLogin = "Build";
    [SerializeField] private string _guestScene = "Other";
    
    private bool _autoLoginAfterRegister = true;
    private CancellationTokenSource _cts;
    private List<SpecialtyDto> _specialties = new List<SpecialtyDto>();

    private void Awake()
    {
        _cts = new CancellationTokenSource();

        if (_errorTextReg != null)
        {
            _errorTextReg.text = "";
        }

        if (_errorTextAuth != null)
        {
            _errorTextAuth.text = "";
        }

        ShowAuthPanel();
        _ = LoadSpecialtiesAsync();
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

    public void ShowRegistrationPanel()
    {
        if (_authPanel != null)
        {
            _authPanel.SetActive(false);
        }

        if (_registrationPanel != null)
        {
            _registrationPanel.SetActive(true);
        }

        ClearInputs();
    }

    public void ShowAuthPanel()
    {
        if (_registrationPanel != null)
        {
            _registrationPanel.SetActive(false);
        }

        if (_authPanel != null)
        {
            _authPanel.SetActive(true);
        }

        ClearInputs();
    }

    private void ClearInputs()
    {
        if (_loginInputReg != null) { _loginInputReg.text = ""; }
        if (_passwordInputReg != null) { _passwordInputReg.text = ""; }
        if (_nameInputReg != null) { _nameInputReg.text = ""; }
        if (_loginInputAuth != null) { _loginInputAuth.text = ""; }
        if (_passwordInputAuth != null) { _passwordInputAuth.text = ""; }
    }

    public async void Register()
    {
        if (_errorTextReg != null)
        {
            _errorTextReg.text = "";
        }

        if (_loginInputReg == null || _passwordInputReg == null || _nameInputReg == null)
        {
            SetRegStatus("Не настроены поля регистрации");
            return;
        }

        string login = ReadText(_loginInputReg);
        string password = ReadText(_passwordInputReg);
        string name = ReadText(_nameInputReg);

        if (string.IsNullOrEmpty(login))
        {
            SetRegStatus("Логин не может быть пустым");
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            SetRegStatus("Пароль не может быть пустым");
            return;
        }

        if (string.IsNullOrEmpty(name))
        {
            SetRegStatus("Имя не может быть пустым");
            return;
        }

        int specialtyId = GetSelectedSpecialtyId();

        SetRegStatus("Регистрация...");
        ApiResult<AuthRegisterResultDto> res =
            await _authApi.RegisterAsync(login, password, name, specialtyId, _cts.Token);

        if (res.IsSuccess)
        {
            if (_autoLoginAfterRegister)
            {
                await LoginInternal(login, password);
            }
            else
            {
                SetAuthStatus("Регистрация успешна. Войдите.");
                ShowAuthPanel();
            }
        }
        else
        {
            SetRegStatus("Не удалось зарегистрироваться");
        }
    }

    public async void Authenticate()
    {
        if (_errorTextAuth != null)
        {
            _errorTextAuth.text = "";
        }

        if (_loginInputAuth == null || _passwordInputAuth == null)
        {
            SetAuthStatus("Не настроены поля входа");
            return;
        }

        string login = ReadText(_loginInputAuth);
        string password = ReadText(_passwordInputAuth);

        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
        {
            SetAuthStatus("Логин и пароль обязательны");
            return;
        }

        await LoginInternal(login, password);
    }

    private async Task LoginInternal(string login, string password)
    {
        SetAuthStatus("Вход...");
        ApiResult<AuthLoginResultDto> res = await _authApi.LoginAsync(login, password, _cts.Token);

        if (res.IsSuccess && res.Data != null)
        {
            if (!string.IsNullOrEmpty(res.Data.token))
            {
                JwtStorage.Save(res.Data.token);
            }

            UserSession.SaveSpecialtyId(res.Data.specialtyId);
            UserSession.SaveIsFirstLogin(res.Data.isFirstLogin);
            UserSession.SaveTutorialCompleted(res.Data.tutorialCompleted);

            SetAuthStatus("Вход успешен.");
            if (!string.IsNullOrEmpty(_sceneAfterLogin))
            {
                SceneManager.LoadScene(_sceneAfterLogin);
            }
        }
        else
        {
            SetAuthStatus("Неверный логин или пароль");
        }
    }

    public void LoginAsGuest()
    {
        if (string.IsNullOrEmpty(_guestScene) == false)
        {
            SceneManager.LoadScene(_guestScene);
        }
    }

    private async Task LoadSpecialtiesAsync()
    {
        if (_specialtyDropdown == null || _specialtiesApi == null)
        {
            _specialties = new List<SpecialtyDto>();
            return;
        }

        _specialtyDropdown.ClearOptions();

        ApiResult<List<SpecialtyDto>> res = await _specialtiesApi.GetAllAsync(_cts.Token);
        if (res.IsSuccess == false || res.Data == null)
        {
            _specialties = new List<SpecialtyDto>();
            return;
        }

        _specialties = res.Data;
        for (int i = 0; i < _specialties.Count; i++)
        {
            TMP_Dropdown.OptionData opt = new TMP_Dropdown.OptionData();
            opt.text = _specialties[i].name;
            _specialtyDropdown.options.Add(opt);
        }

        _specialtyDropdown.RefreshShownValue();
    }

    private int GetSelectedSpecialtyId()
    {
        if (_specialties == null || _specialtyDropdown == null)
        {
            return 1;
        }

        int idx = _specialtyDropdown.value;
        if (idx >= 0 && idx < _specialties.Count)
        {
            return _specialties[idx].id;
        }

        return 1;
    }

    private string ReadText(TMP_InputField field)
    {
        if (field == null)
        {
            return null;
        }

        string t = field.text;
        if (string.IsNullOrEmpty(t))
        {
            return null;
        }

        string trimmed = t.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }

        return trimmed;
    }

    private void SetRegStatus(string text)
    {
        if (_errorTextReg != null)
        {
            _errorTextReg.text = text;
        }
    }

    private void SetAuthStatus(string text)
    {
        if (_errorTextAuth != null)
        {
            _errorTextAuth.text = text;
        }
    }
}

