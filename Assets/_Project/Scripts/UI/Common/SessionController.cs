using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRCollege.Net.Dto;

public sealed class SessionController : MonoBehaviour
{
    [SerializeField] private AuthApi _authApi;
    [SerializeField] private string _authScene = "Test";

    private CancellationTokenSource _cts;

    private void Awake()
    {
        _cts = new CancellationTokenSource();
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

    public async void LogoutAndGoToAuthScene()
    {
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
    }
}
