using TMPro;
using UnityEngine;
using VRCollege.Net.Dto;

public sealed class NpcCardView : MonoBehaviour
{
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private GameObject _doneBadge;

    public void Bind(NpcDto npc, ProgressStateDto progress)
    {
        if (_nameText != null)
        {
            _nameText.text = npc != null ? npc.role : string.Empty;
        }

        bool completed = false;
        if (progress != null && (progress.status == ProgressStateDto.CompletedPendingDialogue || progress.status == ProgressStateDto.CompletedRepeat))
        {
            completed = true;
        }

        if (_doneBadge != null)
        {
            _doneBadge.SetActive(completed);
        }
    }
}
