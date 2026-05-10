using System.Threading.Tasks;
using UnityEngine;
using VRCollege.Net.Dto;

namespace VRCollege.Net
{
    public sealed class ProfileApi : MonoBehaviour
    {
        [SerializeField] private ApiClient _client;
        private const string ProfilePath = "/api/profile";
        private const string TutorialCompletePath = "/api/profile/tutorial/complete";

        public async Task<ApiResult<ProfileDto>> GetProfileAsync()
        {
            if (_client == null)
            {
                Debug.LogError("[ProfileApi] ApiClient is not assigned.");
                return ApiResult<ProfileDto>.Fail(0, "ApiClient is not assigned");
            }

            ApiResult<string> raw = await _client.GetAsync<string>(ProfilePath);
            if (!raw.IsSuccess)
            {
                Debug.LogError("[ProfileApi] Request failed: " + raw.Error);
                return ApiResult<ProfileDto>.Fail(raw.StatusCode, raw.Error);
            }

            if (string.IsNullOrEmpty(raw.Data))
            {
                Debug.LogError("[ProfileApi] Empty profile payload. Endpoint /api/profile returned empty body.");
                return ApiResult<ProfileDto>.Fail(raw.StatusCode, "Empty profile payload");
            }

            try
            {
                ProfileDto profile = JsonUtility.FromJson<ProfileDto>(raw.Data);
                if (profile == null)
                {
                    Debug.LogError("[ProfileApi] Profile JSON parsed to null: " + raw.Data);
                    return ApiResult<ProfileDto>.Fail(raw.StatusCode, "Invalid profile JSON");
                }

                return ApiResult<ProfileDto>.Success(profile);
            }
            catch
            {
                Debug.LogError("[ProfileApi] Invalid profile JSON: " + raw.Data);
                return ApiResult<ProfileDto>.Fail(raw.StatusCode, "Invalid profile JSON");
            }
        }

        public async Task<ApiResult<TutorialCompleteResultDto>> CompleteTutorialAsync()
        {
            if (_client == null)
            {
                Debug.LogError("[ProfileApi] ApiClient is not assigned.");
                return ApiResult<TutorialCompleteResultDto>.Fail(0, "ApiClient is not assigned");
            }

            return await _client.PostJsonAsync<EmptyBody, TutorialCompleteResultDto>(TutorialCompletePath, new EmptyBody());
        }

        [System.Serializable]
        private sealed class EmptyBody
        {
        }
    }
}
