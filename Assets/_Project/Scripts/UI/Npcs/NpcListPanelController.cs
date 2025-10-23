using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class NpcListPanelController : MonoBehaviour
{
    [SerializeField] private RectTransform _content;
    [SerializeField] private NpcCardView _npcCardPrefab;
    [SerializeField] private TMP_Text _statusText;

    [SerializeField] private NpcsApi _npcsApi;
    [SerializeField] private ProgressApi _progressApi;

    private List<NpcDto> _npcs;
    private Dictionary<int, ProgressDto> _progressByNpc;

    private async void OnEnable()
    {
        await RefreshList();
    }

    public async Task RefreshList()
    {
        SetStatus("Загрузка...");

        ApiResult<List<NpcDto>> npcsRes = await _npcsApi.GetBySpecialtyAsync();
        if (npcsRes.IsSuccess == false)
        {
            SetStatus("Ошибка загрузки NPC");
            return;
        }

        if (npcsRes.Data != null)
        {
            _npcs = npcsRes.Data;
        }
        else
        {
            _npcs = new List<NpcDto>();
        }

        List<int> ids = new List<int>();
        int k = 0;
        while (k < _npcs.Count)
        {
            ids.Add(_npcs[k].id);
            k = k + 1;
        }

        ApiResult<List<ProgressDto>> progRes = await _progressApi.GetMineAsync(ids);
        if (progRes.IsSuccess == false)
        {
            SetStatus("Ошибка загрузки прогресса");
            return;
        }

        List<ProgressDto> my;
        if (progRes.Data != null)
        {
            my = progRes.Data;
        }
        else
        {
            my = new List<ProgressDto>();
        }

        _progressByNpc = new Dictionary<int, ProgressDto>();
        int i = 0;
        while (i < my.Count)
        {
            _progressByNpc[my[i].npcId] = my[i];
            i = i + 1;
        }

        for (int c = _content.childCount - 1; c >= 0; c--)
        {
            Transform t = _content.GetChild(c);
            Destroy(t.gameObject);
        }

        i = 0;
        while (i < _npcs.Count)
        {
            NpcCardView card = Instantiate(_npcCardPrefab, _content);

            ProgressDto p;
            bool has = _progressByNpc.TryGetValue(_npcs[i].id, out p);
            if (has == true)
            {
                card.Bind(_npcs[i], p, _progressApi, this);
            }
            else
            {
                ProgressDto empty = new ProgressDto();
                empty.npcId = _npcs[i].id;
                card.Bind(_npcs[i], empty, _progressApi, this);
            }

            i = i + 1;
        }

        SetStatus("Готово");
    }

    public void SetStatus(string msg)
    {
        if (_statusText != null)
        {
            _statusText.text = msg;
        }
    }
}
