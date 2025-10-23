using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public sealed class SpecialtiesApi : MonoBehaviour
{
    [SerializeField] private ApiClient _client;

    public async Task<ApiResult<List<SpecialtyDto>>> GetAllAsync(CancellationToken ct = default)
    {
        ApiResult<string> res = await _client.GetAsync<string>("/api/specialties", ct);

        if (res.IsSuccess == false)
        {
            ApiResult<List<SpecialtyDto>> fail = ApiResult<List<SpecialtyDto>>.Fail(res.StatusCode, res.Error);
            return fail;
        }

        SpecialtyDto[] arr = JsonArrays.FromJson<SpecialtyDto>(res.Data);
        List<SpecialtyDto> list = new List<SpecialtyDto>(arr);
        return ApiResult<List<SpecialtyDto>>.Success(list);
    }
}