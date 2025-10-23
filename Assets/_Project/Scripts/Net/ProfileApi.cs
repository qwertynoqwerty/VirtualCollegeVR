using System.Threading.Tasks;
using UnityEngine;
using VRCollege.Net.Dto;

namespace VRCollege.Net
{
    public sealed class ProfileApi : MonoBehaviour
    {
        [SerializeField] private ApiClient _client;

        public async Task<ApiResult<ProfileDto>> GetProfileAsync()
        {
            string path = "/api/profile";
            return await _client.GetJsonAsync<ProfileDto>(path);
        }
    }
}