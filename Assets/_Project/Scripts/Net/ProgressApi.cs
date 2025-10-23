using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public sealed class ProgressApi : MonoBehaviour
{
    [SerializeField] private ApiClient _client;

    public async Task<ApiResult<List<ProgressDto>>> GetMineAsync(List<int> npcIds, CancellationToken ct = default)
    {
        List<ProgressDto> result = new List<ProgressDto>();

        if (npcIds == null || npcIds.Count == 0)
        {
            return ApiResult<List<ProgressDto>>.Success(result);
        }

        int i = 0;
        while (i < npcIds.Count)
        {
            int id = npcIds[i];
            string path = "/api/progress/status?npcId=" + id;

            ApiResult<string> res = await _client.GetAsync<string>(path, ct);
            if (res.IsSuccess == true && string.IsNullOrEmpty(res.Data) == false)
            {
                ProgressStatus tmp = JsonUtility.FromJson<ProgressStatus>(res.Data);
                ProgressDto dto = new ProgressDto();
                dto.npcId = id;
                dto.npcName = "";
                if (tmp != null)
                {
                    if (tmp.state == "in_progress")
                    {
                        dto.isActive = true;
                        dto.completedAt = null;
                    }
                    else if (tmp.state == "completed")
                    {
                        dto.isActive = false;
                        dto.completedAt = "";
                    }
                    else
                    {
                        dto.isActive = false;
                        dto.completedAt = null;
                    }
                }
                result.Add(dto);
            }
            i = i + 1;
        }

        return ApiResult<List<ProgressDto>>.Success(result);
    }

    public async Task<ApiResult<ProgressDto>> StartQuestAsync(int npcId, CancellationToken ct = default)
    {
        ProgressStartDto dto = new ProgressStartDto();
        dto.npcId = npcId;

        ApiResult<string> res =
            await _client.PostJsonAsync<ProgressStartDto, string>("/api/progress/start", dto, ct);

        if (res.IsSuccess == false)
        {
            return ApiResult<ProgressDto>.Fail(res.StatusCode, res.Error);
        }

        ProgressDto ui = new ProgressDto();
        ui.npcId = npcId;
        ui.isActive = true;
        ui.completedAt = null;
        ui.npcName = "";
        return ApiResult<ProgressDto>.Success(ui);
    }

    public async Task<ApiResult<ProgressDto>> CompleteQuestAsync(int npcId, CancellationToken ct = default)
    {
        ProgressCompleteDto body = new ProgressCompleteDto();
        body.npcId = npcId;

        ApiResult<string> res =
            await _client.PostJsonAsync<ProgressCompleteDto, string>("/api/progress/complete", body, ct);

        if (res.IsSuccess == false)
        {
            return ApiResult<ProgressDto>.Fail(res.StatusCode, res.Error);
        }

        ProgressDto ui = new ProgressDto();
        ui.npcId = npcId;
        ui.isActive = false;

        ui.completedAt = ""; 
        ui.npcName = "";
        return ApiResult<ProgressDto>.Success(ui);
    }

    [System.Serializable]
    private class ProgressStatus
    {
        public int npcId;
        public string state;
    }
}
