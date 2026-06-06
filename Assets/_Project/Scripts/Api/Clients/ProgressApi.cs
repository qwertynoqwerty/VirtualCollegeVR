using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public sealed class ProgressApi : MonoBehaviour
{
    [SerializeField] private ApiClient _client;

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

    public async Task<ApiResult<ProgressAdvanceResponse>> AdvanceQuestStateAsync(int npcId, int fromStatus, CancellationToken ct = default)
    {
        var body = new ProgressAdvanceDto
        {
            npcId = npcId,
            fromStatus = fromStatus
        };

        var res = await _client.PostJsonAsync<ProgressAdvanceDto, ProgressAdvanceResponse>("/api/progress/advance", body, ct);
        return res;
    }
}
