using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UltimateXR.UI.Helpers.Keyboard;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRCollege.Net.Dto;

public sealed class AuthController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpecialtyDropdownBinder _specialtyBinder;
    [SerializeField] private AuthApi _authApi;

    [Header("Register")]
    [SerializeField] private TMP_InputField _loginInputReg;
    [SerializeField] private TMP_InputField _passwordInputReg;
    [SerializeField] private TMP_InputField _nameInputReg;
    [SerializeField] private TMP_Text _errorTextReg;
    [SerializeField] private GameObject _registrationPanel;

    [Header("Login")]
    [SerializeField] private TMP_InputField _loginInputAuth;
    [SerializeField] private TMP_InputField _passwordInputAuth;
    [SerializeField] private TMP_Text _errorTextAuth;
    [SerializeField] private GameObject _authPanel;

    [Header("VR Keyboard")]
    [SerializeField] private UxrKeyboardUI _vrKeyboard;

    [Header("Scenes")]
    [SerializeField] private string _sceneAfterLogin = "Build";
    [SerializeField] private string _guestScene = "Other";
    [SerializeField] private bool _autoLoginAfterRegister = true;

    private CancellationTokenSource _cts;
    private TMP_InputField _activeInput;
    private EventHandler<string> _lineChangedHandler;

    private void Awake()
    {
        _cts = new CancellationTokenSource();

        BindVrKeyboardListeners();

        if (_errorTextReg != null)
        {
            _errorTextReg.text = "";
        }

        if (_errorTextAuth != null)
        {
            _errorTextAuth.text = "";
        }

        ShowAuthPanel();
    }

    private void OnDestroy()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        if (_vrKeyboard != null && _lineChangedHandler != null)
        {
            _vrKeyboard.CurrentLineChanged -= _lineChangedHandler;
        }

        _activeInput = null;
        _lineChangedHandler = null;
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
            SetRegStatus("РќРµ РЅР°СЃС‚СЂРѕРµРЅС‹ СЃСЃС‹Р»РєРё РЅР° РїРѕР»СЏ СЂРµРіРёСЃС‚СЂР°С†РёРё");
            return;
        }

        string login = ReadText(_loginInputReg);
        string password = ReadText(_passwordInputReg);
        string name = ReadText(_nameInputReg);

        if (string.IsNullOrEmpty(login))
        {
            SetRegStatus("Р›РѕРіРёРЅ РЅРµ РјРѕР¶РµС‚ Р±С‹С‚СЊ РїСѓСЃС‚С‹Рј");
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            SetRegStatus("РџР°СЂРѕР»СЊ РЅРµ РјРѕР¶РµС‚ Р±С‹С‚СЊ РїСѓСЃС‚С‹Рј");
            return;
        }

        if (string.IsNullOrEmpty(name))
        {
            SetRegStatus("РРјСЏ РЅРµ РјРѕР¶РµС‚ Р±С‹С‚СЊ РїСѓСЃС‚С‹Рј");
            return;
        }

        int specialtyId = 1;
        if (_specialtyBinder != null)
        {
            int s = _specialtyBinder.GetSelectedSpecialtyId();
            if (s > 0)
            {
                specialtyId = s;
            }
        }

        SetRegStatus("Р РµРіРёСЃС‚СЂР°С†РёСЏ...");
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
                SetAuthStatus("Р РµРіРёСЃС‚СЂР°С†РёСЏ СѓСЃРїРµС€РЅР°! Р’РѕР№РґРёС‚Рµ.");
                ShowAuthPanel();
            }
        }
        else
        {
            SetRegStatus("РќРµ СѓРґР°Р»РѕСЃСЊ Р·Р°СЂРµРіРёСЃС‚СЂРёСЂРѕРІР°С‚СЊСЃСЏ");
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
            SetAuthStatus("РќРµ РЅР°СЃС‚СЂРѕРµРЅС‹ СЃСЃС‹Р»РєРё РЅР° РїРѕР»СЏ РІС…РѕРґР°");
            return;
        }

        string login = ReadText(_loginInputAuth);
        string password = ReadText(_passwordInputAuth);

        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
        {
            SetAuthStatus("Р›РѕРіРёРЅ Рё РїР°СЂРѕР»СЊ РѕР±СЏР·Р°С‚РµР»СЊРЅС‹");
            return;
        }

        await LoginInternal(login, password);
    }

    private async Task LoginInternal(string login, string password)
    {
        SetAuthStatus("Р’С…РѕРґ...");
        ApiResult<AuthLoginResultDto> res = await _authApi.LoginAsync(login, password, _cts.Token);

        if (res.IsSuccess)
        {
            if (res.Data != null)
            {
                if (!string.IsNullOrEmpty(res.Data.token))
                    JwtStorage.Save(res.Data.token);

                UserSession.SaveSpecialtyId(res.Data.specialtyId);
                UserSession.SaveIsFirstLogin(res.Data.isFirstLogin);
                UserSession.SaveTutorialCompleted(res.Data.tutorialCompleted);
            }

            SetAuthStatus("Р’С…РѕРґ СѓСЃРїРµС€РµРЅ.");
            if (!string.IsNullOrEmpty(_sceneAfterLogin))
                SceneManager.LoadScene(_sceneAfterLogin);
        }

        else
        {
            SetAuthStatus("РќРµРІРµСЂРЅС‹Р№ Р»РѕРіРёРЅ РёР»Рё РїР°СЂРѕР»СЊ");
        }
    }


    public void LoginAsGuest()
    {
        if (string.IsNullOrEmpty(_guestScene) == false)
        {
            SceneManager.LoadScene(_guestScene);
        }
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

    private void BindVrKeyboardListeners()
    {
        if (_vrKeyboard == null)
        {
            return;
        }

        if (_loginInputReg != null)
        {
            _loginInputReg.onSelect.AddListener(_ => OpenVrKeyboard(_loginInputReg));
        }

        if (_passwordInputReg != null)
        {
            _passwordInputReg.onSelect.AddListener(_ => OpenVrKeyboard(_passwordInputReg));
        }

        if (_nameInputReg != null)
        {
            _nameInputReg.onSelect.AddListener(_ => OpenVrKeyboard(_nameInputReg));
        }

        if (_loginInputAuth != null)
        {
            _loginInputAuth.onSelect.AddListener(_ => OpenVrKeyboard(_loginInputAuth));
        }

        if (_passwordInputAuth != null)
        {
            _passwordInputAuth.onSelect.AddListener(_ => OpenVrKeyboard(_passwordInputAuth));
        }
    }

    private void OpenVrKeyboard(TMP_InputField field)
    {
        if (_vrKeyboard == null)
        {
            return;
        }

        if (_lineChangedHandler != null)
        {
            _vrKeyboard.CurrentLineChanged -= _lineChangedHandler;
            _lineChangedHandler = null;
        }

        _activeInput = field;

        _vrKeyboard.Clear();
        _vrKeyboard.EnableDefaultSymbols();

        if (field != null)
        {
            if (string.IsNullOrEmpty(field.text) == false)
            {
                _vrKeyboard.AddConsoleContent(field.text);
            }

            bool isPassword = field.contentType == TMP_InputField.ContentType.Password;
            _vrKeyboard.IsPassword = isPassword;
        }

        _vrKeyboard.AllowInput = true;

        _lineChangedHandler = (sender, value) =>
        {
            if (_activeInput != null)
            {
                _activeInput.text = value;
            }
        };
        _vrKeyboard.CurrentLineChanged += _lineChangedHandler;

    }
}

