using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using VRCollege.Net.Dto;

public sealed class ProgressApi : MonoBehaviour
{
    [SerializeField] private ApiClient _client;

    public async Task<ApiResult<List<ProgressDto>>> GetMineAsync(List<int> npcIds, CancellationToken ct = default)
    {
        var result = new List<ProgressDto>();
        if (npcIds == null || npcIds.Count == 0)
            return ApiResult<List<ProgressDto>>.Success(result);

        var statesRes = await _client.GetAsync<string>("/api/progress/states", ct);
        if (!statesRes.IsSuccess)
            return ApiResult<List<ProgressDto>>.Fail(statesRes.StatusCode, statesRes.Error);

        var all = JsonArrays.FromJson<ProgressStateDto>(statesRes.Data);

        var byId = new Dictionary<int, ProgressStateDto>(all.Length);
        for (int i = 0; i < all.Length; i++)
            byId[all[i].npcId] = all[i];

        for (int i = 0; i < npcIds.Count; i++)
        {
            int id = npcIds[i];
            if (byId.TryGetValue(id, out var s))
            {
                var dto = new ProgressDto
                {
                    npcId = id,
                    npcName = "",
                    isActive = s.state == "in_progress",
                    completedAt = s.state == "completed" ? (string.IsNullOrEmpty(s.completedAt) ? "done" : s.completedAt) : null
                };
                result.Add(dto);
            }
            else
            {
                result.Add(new ProgressDto { npcId = id, npcName = "", isActive = false, completedAt = null });
            }
        }

        return ApiResult<List<ProgressDto>>.Success(result);
    }

    public async Task<ApiResult<ProgressStartResponse>> StartQuestAsync(int npcId, CancellationToken ct = default)
    {
        var dto = new ProgressStartDto { npcId = npcId };

        var res = await _client.PostJsonAsync<ProgressStartDto, ProgressStartResponse>("/api/progress/start", dto, ct);
        return res;
    }

    public async Task<ApiResult<ProgressCompleteResponse>> CompleteQuestAsync(int npcId, CancellationToken ct = default)
    {
        var body = new ProgressCompleteDto { npcId = npcId };

        var res = await _client.PostJsonAsync<ProgressCompleteDto, ProgressCompleteResponse>("/api/progress/complete", body, ct);
        return res;
    }
}
