using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using VRCollege.Net;
using VRCollege.Net.Dto;

public class NpcListPanelController : MonoBehaviour
{
    [SerializeField] private RectTransform _content;
    [SerializeField] private NpcCardView _npcCardPrefab;
    [SerializeField] private TMP_Text _statusText;

    [SerializeField] private NpcsApi _npcsApi;
    [SerializeField] private ProgressStatesApi _progressStatesApi;

    private List<NpcDto> _npcs;
    private Dictionary<int, ProgressStateDto> _progressByNpc;

    private void Awake()
    {
        if (_npcsApi == null)
        {
            _npcsApi = FindFirstObjectByType<NpcsApi>(FindObjectsInactive.Include);
        }

        if (_progressStatesApi == null)
        {
            _progressStatesApi = FindFirstObjectByType<ProgressStatesApi>(FindObjectsInactive.Include);
        }
    }

    private async void OnEnable()
    {
        await RefreshList();
    }

    public async Task RefreshList()
    {
        SetStatus("Р—Р°РіСЂСѓР·РєР°...");

        ApiResult<List<NpcDto>> npcsRes = await _npcsApi.GetBySpecialtyAsync();
        if (npcsRes.IsSuccess == false)
        {
            SetStatus("РћС€РёР±РєР° Р·Р°РіСЂСѓР·РєРё NPC");
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

        ApiResult<ProgressStateDto[]> progRes = await _progressStatesApi.GetAllAsync();
        if (progRes.IsSuccess == false)
        {
            SetStatus("РћС€РёР±РєР° Р·Р°РіСЂСѓР·РєРё РїСЂРѕРіСЂРµСЃСЃР°");
            return;
        }

        ProgressStateDto[] my = progRes.Data ?? new ProgressStateDto[0];
        _progressByNpc = new Dictionary<int, ProgressStateDto>();
        int i = 0;
        while (i < my.Length)
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

            ProgressStateDto p;
            bool has = _progressByNpc.TryGetValue(_npcs[i].id, out p);
            if (has == true)
            {
                card.Bind(_npcs[i], p);
            }
            else
            {
                ProgressStateDto empty = new ProgressStateDto();
                empty.npcId = _npcs[i].id;
                empty.status = ProgressStateDto.NotStarted;
                card.Bind(_npcs[i], empty);
            }

            i = i + 1;
        }

        SetStatus("Р“РѕС‚РѕРІРѕ");
    }

    public void SetStatus(string msg)
    {
        if (_statusText != null)
        {
            _statusText.text = msg;
        }
    }
}
