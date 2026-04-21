using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VRCollege.Net.Dto;

public sealed class QuestJournalItemView : MonoBehaviour
{
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _stateText;
    [SerializeField] private Image _background;

    [Header("Colors")]
    [SerializeField] private Color _inProgressColor = new Color(1f, 1f, 1f, 1f);
    [SerializeField] private Color _completedColor  = new Color(0.298f, 0.690f, 0.310f, 1f);

    public void Bind(ProgressStateDto dto)
    {
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

        if (_stateText != null)
        {
            if (isCompleted == true)
            {
                _stateText.text = "Завершено";
            }
            else
            {
                _stateText.text = "В процессе";
            }
        }

        if (_background != null)
        {
            if (isCompleted == true)
            {
                _background.color = _completedColor;
            }
            else
            {
                _background.color = _inProgressColor;
            }
        }
    }
}
