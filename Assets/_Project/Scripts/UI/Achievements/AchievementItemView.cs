using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRCollege.Net.Dto;

public sealed class AchievementItemView : MonoBehaviour
{
    [SerializeField] private TMP_Text _numberText;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _hintText;
    [SerializeField] private Image[] _stateImages = new Image[2];

    public void Bind(AchievementDto dto)
    {
        if (_numberText != null)
        {
            _numberText.text = dto.achievementId.ToString("00");
        }

        if (_titleText != null)
        {
            _titleText.text = dto.name;
        }

        if (_hintText != null)
        {
            _hintText.text = dto.title;
        }

        bool got = false;
        if (dto.unlocked == true)
        {
            got = true;
        }
        else
        {
            got = false;
        }

        ApplyStateImage(got);
    }

    private void ApplyStateImage(bool isObtained)
    {
        if (_stateImages == null)
        {
            return;
        }

        int activeIndex = isObtained ? 1 : 0;
        for (int i = 0; i < _stateImages.Length; i++)
        {
            if (_stateImages[i] != null)
            {
                _stateImages[i].gameObject.SetActive(i == activeIndex);
            }
        }
    }
}
