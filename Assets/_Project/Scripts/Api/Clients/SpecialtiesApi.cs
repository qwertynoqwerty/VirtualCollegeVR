using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public sealed class SpecialtiesApi : MonoBehaviour
{
    [SerializeField] private ApiClient _client;
    private const string _route = "/api/specialties";

    public async Task<ApiResult<List<SpecialtyDto>>> GetAllAsync(CancellationToken ct = default)
    {
        var res = await _client.GetAsync<string>(_route, ct);
        if (!res.IsSuccess)
            return ApiResult<List<SpecialtyDto>>.Fail(res.StatusCode, res.Error);

        var arr = JsonArrays.FromJson<SpecialtyDto>(res.Data);
        var list = new List<SpecialtyDto>(arr);
        return ApiResult<List<SpecialtyDto>>.Success(list);
    }
}