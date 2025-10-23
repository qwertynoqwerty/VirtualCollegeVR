using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using VRCollege.Net.Dto;

public sealed class AuthApi : MonoBehaviour
{
    [SerializeField] private ApiClient _client;

    public async Task<ApiResult<AuthRegisterResultDto>> RegisterAsync(
        string login,
        string password,
        string name,
        int specialtyId,
        CancellationToken ct = default)
    {
        AuthRegisterDto req = new AuthRegisterDto();
        req.login = login;
        req.password = password;
        req.name = name;
        req.specialtyId = specialtyId;

        ApiResult<AuthRegisterResultDto> res =
            await _client.PostJsonAsync<AuthRegisterDto, AuthRegisterResultDto>("/api/auth/register", req, ct);

        return res;
    }

    public async Task<ApiResult<AuthLoginResultDto>> LoginAsync(string login, string password, CancellationToken ct = default)
    {
        AuthLoginDto req = new AuthLoginDto();
        req.login = login;
        req.password = password;

        ApiResult<AuthLoginResultDto> res =
            await _client.PostJsonAsync<AuthLoginDto, AuthLoginResultDto>("/api/auth/login", req, ct);

        if (res.IsSuccess && res.Data != null)
        {
            if (string.IsNullOrEmpty(res.Data.token) == false)
            {
                JwtStorage.Save(res.Data.token);
            }
        }

        return res;
    }

    public async Task<ApiResult<AuthLogoutResultDto>> LogoutAsync(CancellationToken ct = default)
    {
        ApiResult<AuthLogoutResultDto> res =
            await _client.PostJsonAsync<object, AuthLogoutResultDto>("/api/auth/logout", new object(), ct);

        if (res.IsSuccess)
        {
            JwtStorage.Clear();
        }

        return res;
    }
}