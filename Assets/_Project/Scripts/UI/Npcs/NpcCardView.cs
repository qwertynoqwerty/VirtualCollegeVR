using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NpcCardView : MonoBehaviour
{
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _pointsText;
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
    }

    public async void OnStartClick()
    {
        SetButtons(false);

        ApiResult<ProgressDto> res = await _progressApi.StartQuestAsync(_npc.id);
        if (res.IsSuccess == true)
        {
            _progress = res.Data;
            _panel.SetStatus("Квест начат: " + _npc.name);

            var p = FindObjectOfType<NpcAudioPlayer>();
            if (p != null)
            {
                p.PlayStart(_npc.startDialogue, _npc.startAudio);
            }
        }
        else
        {
            _panel.SetStatus("Ошибка: " + res.Error);
            FailAttempts.Increment();
        }

        UpdateState();
        SetButtons(true);
    }

    public async void OnCompleteClick()
    {
        SetButtons(false);

        ApiResult<ProgressDto> res = await _progressApi.CompleteQuestAsync(_npc.id);
        if (res.IsSuccess == true)
        {
            _progress = res.Data;
            _panel.SetStatus("Квест завершён: " + _npc.name);

            var p = FindObjectOfType<NpcAudioPlayer>();
            if (p != null)
            {
                p.PlayComplete(_npc.completeDialogue, _npc.completeAudio);
            }

            AchievementsNotifier notifier = FindObjectOfType<AchievementsNotifier>();
            if (notifier != null)
            {
                await notifier.CheckAndNotifyAsync();
            }
        }
        else
        {
            _panel.SetStatus("Ошибка: " + res.Error);
            FailAttempts.Increment();
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
            if (completed == true)
            {
                _doneBadge.SetActive(true);
            }
            else
            {
                _doneBadge.SetActive(false);
            }
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
