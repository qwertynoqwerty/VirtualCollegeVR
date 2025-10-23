using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class SpecialtyDropdownBinder : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown _dropdown;
    [SerializeField] private SpecialtiesApi _api;

    private List<SpecialtyDto> _data;

    private async void OnEnable()
    {
        await Load();
    }

    public async Task Load()
    {
        _dropdown.ClearOptions();

        ApiResult<List<SpecialtyDto>> res = await _api.GetAllAsync();
        if (res.IsSuccess == false || res.Data == null)
        {
            _data = new List<SpecialtyDto>();
            return;
        }

        _data = res.Data;

        int i = 0;
        while (i < _data.Count)
        {
            TMP_Dropdown.OptionData opt = new TMP_Dropdown.OptionData();
            opt.text = _data[i].name;
            _dropdown.options.Add(opt);
            i = i + 1;
        }

        _dropdown.RefreshShownValue();
    }

    public void SetById(int id)
    {
        if (_data == null)
        {
            return;
        }

        int i = 0;
        while (i < _data.Count)
        {
            if (_data[i].id == id)
            {
                _dropdown.value = i;
                _dropdown.RefreshShownValue();
                return;
            }
            i = i + 1;
        }
    }

    public int GetSelectedSpecialtyId()
    {
        if (_data == null)
        {
            return 0;
        }

        int idx = _dropdown.value;
        if (idx >= 0)
        {
            if (idx < _data.Count)
            {
                return _data[idx].id;
            }
            else
            {
                return 0;
            }
        }
        else
        {
            return 0;
        }
    }
}