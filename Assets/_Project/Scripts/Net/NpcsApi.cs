using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public sealed class NpcsApi : MonoBehaviour
{
    [SerializeField] private ApiClient _client;

    public async Task<ApiResult<List<NpcDto>>> GetBySpecialtyAsync(int? specialtyId = null, CancellationToken ct = default)
    {
        string path = "/api/npcs";
        if (specialtyId.HasValue)
        {
            path = path + "?specialtyId=" + specialtyId.Value;
        }

        ApiResult<string> res = await _client.GetAsync<string>(path, ct);

        if (res.IsSuccess == false)
        {
            ApiResult<List<NpcDto>> fail = ApiResult<List<NpcDto>>.Fail(res.StatusCode, res.Error);
            return fail;
        }

        NpcDto[] arr = JsonArrays.FromJson<NpcDto>(res.Data);
        List<NpcDto> list = new List<NpcDto>(arr);
        return ApiResult<List<NpcDto>>.Success(list);
    }
}