using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using VRCollege.Net.Dto;

namespace VRCollege.Net
{
    public sealed class ProgressStatesApi : MonoBehaviour
    {
        [SerializeField] private ApiClient _client;
        private const string _route = "/api/progress/states";

        public async Task<ApiResult<ProgressStateDto[]>> GetStatesAsync(CancellationToken ct = default)
        {
            var res = await _client.GetAsync<string>(_route, ct);
            if (!res.IsSuccess)
                return ApiResult<ProgressStateDto[]>.Fail(res.StatusCode, res.Error);

            var arr = JsonArrays.FromJson<ProgressStateDto>(res.Data);
            return ApiResult<ProgressStateDto[]>.Success(arr);
        }

        public Task<ApiResult<ProgressStateDto[]>> GetAllAsync(CancellationToken ct = default)
            => GetStatesAsync(ct);
    }
}