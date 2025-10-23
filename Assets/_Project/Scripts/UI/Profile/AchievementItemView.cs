using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VRCollege.Net.Dto;

public sealed class AchievementItemView : MonoBehaviour
{
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private Image _background;

    [Header("Colors")]
    [SerializeField] private Color _notObtainedColor = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private Color _obtainedColor    = new Color(0.298f, 0.690f, 0.310f, 1f);

    public void Bind(AchievementDto dto)
    {
        if (_nameText != null)
        {
            _nameText.text = dto.name;
        }

        if (_titleText != null)
        {
            _titleText.text = dto.title;
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

        if (_background != null)
        {
            if (got == true)
            {
                _background.color = _obtainedColor;
            }
            else
            {
                _background.color = _notObtainedColor;
            }
        }
    }
}