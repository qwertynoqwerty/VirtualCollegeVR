using System.Threading.Tasks;
using UnityEngine;

public sealed class InfoNpcController : MonoBehaviour
{
    [SerializeField] private Interactable _interactable;
    [SerializeField] private DialoguePlayer _dialoguePlayer;
    [SerializeField] private InfoNpcDialogueDatabase _dialogueDatabase;
    [SerializeField] private int _infoId;
    [HideInInspector] [SerializeField] private float _debounceSeconds = 0.4f;

    private bool _isPlaying;

    private void Awake()
    {
        if (_interactable != null)
        {
            _interactable.OnInteract += Play;
        }
    }

    private void OnDestroy()
    {
        if (_interactable != null)
        {
            _interactable.OnInteract -= Play;
        }
    }

    public async void Play()
    {
        if (_isPlaying)
        {
            return;
        }

        _isPlaying = true;

        DialogueLine[] lines = _dialogueDatabase != null ? _dialogueDatabase.GetLines(_infoId) : null;
        if (_dialoguePlayer != null && lines != null && lines.Length > 0)
        {
            await _dialoguePlayer.PlayLinesAsync(lines);
        }

        if (_interactable != null)
        {
            int debounceMs = Mathf.Max(1, Mathf.RoundToInt(_debounceSeconds * 1000f));
            await Task.Delay(debounceMs);
            _interactable.ReleaseBusy();
        }

        _isPlaying = false;
    }
}
