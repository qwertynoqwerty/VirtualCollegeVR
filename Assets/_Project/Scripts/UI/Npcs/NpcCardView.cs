using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class NpcCardView : MonoBehaviour
{
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _completeButton;
    [SerializeField] private GameObject _doneBadge;

    private NpcDto _npc;
    private ProgressDto _progress;
    private ProgressApi _progressApi;
    private NpcListPanelController _panel;

    public void Bind(NpcDto npc, ProgressDto progress, ProgressApi api, NpcListPanelController panel)
    {
        _npc = npc;
        _progress = progress;
        _progressApi = api;
        _panel = panel;

        if (_nameText != null)
        {
            _nameText.text = npc.name;
        }

        UpdateState();
        SetButtons(true);
    }

    public async void OnStartClick()
    {
        SetButtons(false);

        var res = await _progressApi.StartQuestAsync(_npc.id);
        if (res.IsSuccess == true && res.Data != null)
        {
            if (_progress == null)
            {
                _progress = new ProgressDto
                {
                    npcId = _npc.id,
                    npcName = "",
                    isActive = true,
                    completedAt = null
                };
            }
            else
            {
                _progress.isActive = true;
                _progress.completedAt = null;
            }

            if (_panel != null)
            {
                _panel.SetStatus("Квест начат: " + _npc.name);
            }

            var player = FindFirstObjectByType<NpcAudioPlayer>(FindObjectsInactive.Include);
            if (player != null)
            {
                player.PlayStart(res.Data.startDialogue, res.Data.startAudio);
            }
        }
        else
        {
            if (_panel != null)
            {
                _panel.SetStatus("Ошибка: " + res.Error);
            }
        }

        UpdateState();
        SetButtons(true);
    }

    public async void OnCompleteClick()
    {
        SetButtons(false);

        var res = await _progressApi.CompleteQuestAsync(_npc.id);
        if (res.IsSuccess == true && res.Data != null)
        {
            if (_progress == null)
            {
                _progress = new ProgressDto { npcId = _npc.id, npcName = "" };
            }

            _progress.isActive = false;
            if (string.IsNullOrEmpty(res.Data.completedAt) == true)
            {
                _progress.completedAt = "done";
            }
            else
            {
                _progress.completedAt = res.Data.completedAt;
            }

            if (_panel != null)
            {
                _panel.SetStatus("Квест завершён: " + _npc.name);
            }

            var player = FindFirstObjectByType<NpcAudioPlayer>(FindObjectsInactive.Include);
            if (player != null)
            {
                player.PlayComplete(res.Data.completeDialogue, res.Data.completeAudio);
            }

            var notifier = FindFirstObjectByType<AchievementsNotifier>(FindObjectsInactive.Include);
            if (notifier != null)
            {
                await notifier.CheckAndNotifyAsync();
            }
        }
        else
        {
            if (_panel != null)
            {
                _panel.SetStatus("Ошибка: " + res.Error);
            }
        }

        UpdateState();
        SetButtons(true);
    }

    private void UpdateState()
    {
        bool completed = false;

        if (_progress != null)
        {
            if (string.IsNullOrEmpty(_progress.completedAt) == false)
            {
                completed = true;
            }
        }

        if (_doneBadge != null)
        {
            _doneBadge.SetActive(completed);
        }
    }

    private void SetButtons(bool interactable)
    {
        if (_startButton != null)
        {
            _startButton.interactable = interactable;
        }
        if (_completeButton != null)
        {
            _completeButton.interactable = interactable;
        }
    }
}
