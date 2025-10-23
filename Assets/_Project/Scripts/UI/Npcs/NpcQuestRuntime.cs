using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class NpcQuestRuntime : MonoBehaviour
{
    [Header("Bindings")]
    [SerializeField] private Interactable _interactable;
    [SerializeField] private TMP_Text _dialogueLabel;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private ProgressApi _progressApi;

    [Header("Content")]
    [SerializeField] private int _npcId;
    [SerializeField] private string _startDialogue = "Привет! Возьми задание.";
    [SerializeField] private string _completeDialogue = "Круто, выполнено!";
    [SerializeField] private AudioClip _startClip;
    [SerializeField] private AudioClip _completeClip;

    [Header("Flow")]
    [SerializeField] private float _dialogueSeconds = 3.5f;
    [SerializeField] private float _debounceSeconds = 0.6f;

    private bool _isProcessing;

    private void Awake()
    {
        _dialogueLabel.gameObject.SetActive(false);
        _interactable.OnInteract += HandleInteract;
    }

    private void OnDestroy()
    {
        _interactable.OnInteract -= HandleInteract;
    }

    private async void HandleInteract()
    {
        if (_isProcessing == true)
        {
            return;
        }

        _isProcessing = true;

        await _progressApi.StartQuestAsync(_npcId);

        Show(_startDialogue);
        Play(_startClip);
        int startMs = Mathf.RoundToInt(_dialogueSeconds * 1000f);
        await Task.Delay(startMs);

        await _progressApi.CompleteQuestAsync(_npcId);

        Show(_completeDialogue);
        Play(_completeClip);
        int endMs = Mathf.RoundToInt(_dialogueSeconds * 1000f);
        await Task.Delay(endMs);

        int debMs = Mathf.RoundToInt(_debounceSeconds * 1000f);
        await Task.Delay(debMs);

        _interactable.ReleaseBusy();
        _isProcessing = false;
    }

    private void Show(string text)
    {
        _dialogueLabel.text = text;
        _dialogueLabel.gameObject.SetActive(true);
    }

    private void Play(AudioClip clip)
    {
        _audioSource.Stop();
        _audioSource.clip = clip;
        _audioSource.Play();
    }
}
