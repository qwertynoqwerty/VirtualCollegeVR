using System.Threading.Tasks;
using UnityEngine;
using VRCollege.Net.Dto;

namespace VRCollege.Net
{
    public sealed class ProgressStatesApi : MonoBehaviour
    {
        [SerializeField] private ApiClient _client;

        private const string _route = "/api/progress/states";
        
        public async Task<ApiResult<ProgressStateDto[]>> GetStatesAsync()
        {
            return await _client.GetJsonAsync<ProgressStateDto[]>(_route);
        }
        
        public async Task<ApiResult<ProgressStateDto[]>> GetAllAsync()
        {
            return await GetStatesAsync();
        }
    }
}