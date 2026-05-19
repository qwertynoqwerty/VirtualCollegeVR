using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRCollege.Net.Dto;

public sealed class QuestJournalItemView : MonoBehaviour
{
    [SerializeField] private TMP_Text _numberText;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _hintText;
    [SerializeField] private Image[] _stateImages = new Image[2];

    public void Bind(ProgressStateDto dto)
    {
        if (_numberText != null)
        {
            _numberText.text = dto.npcId.ToString("00");
        }

        if (_titleText != null)
        {
            if (string.IsNullOrEmpty(dto.npcName))
            {
                _titleText.text = "NPC #" + dto.npcId;
            }
            else
            {
                _titleText.text = dto.npcName;
            }
        }

        bool isCompleted = false;
        if (dto.status == ProgressStateDto.CompletedPendingDialogue || dto.status == ProgressStateDto.CompletedRepeat)
        {
            isCompleted = true;
        }
        else
        {
            isCompleted = false;
        }

        if (_hintText != null)
        {
            if (isCompleted == true)
            {
                _hintText.text = "Завершено";
            }
            else
            {
                _hintText.text = "В процессе";
            }
        }

        ApplyStateImage(isCompleted);
    }

    private void ApplyStateImage(bool isCompleted)
    {
        if (_stateImages == null)
        {
            return;
        }

        int activeIndex = isCompleted ? 1 : 0;
        for (int i = 0; i < _stateImages.Length; i++)
        {
            if (_stateImages[i] != null)
            {
                _stateImages[i].gameObject.SetActive(i == activeIndex);
            }
        }
    }
}
