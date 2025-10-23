using System.Threading.Tasks;
using UnityEngine;
using VRCollege.Net.Dto;

namespace VRCollege.Net
{
    public sealed class ProgressAnalyticsApi : MonoBehaviour
    {
        [SerializeField] private ApiClient _client;

        private const string _route = "/api/progress/fail-interval";

        public async Task<ApiResult<FailIntervalResponseDto>> GetFailIntervalAsync(int minutes = 60)
        {
            string url = _route + "?minutes=" + minutes;
            ApiResult<FailIntervalResponseDto> res = await _client.GetJsonAsync<FailIntervalResponseDto>(url);
            return res;
        }
    }
}