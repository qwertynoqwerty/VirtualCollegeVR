using System.Threading.Tasks;
using UnityEngine;

public sealed class OnboardingNpcController : MonoBehaviour
{
    [SerializeField] private Interactable _interactable;
    [SerializeField] private DialoguePlayer _dialoguePlayer;
    [SerializeField] private InfoNpcDialogueDatabase _dialogueDatabase;
    [SerializeField] private int _infoId;
    [SerializeField] private OnboardingStep _step;
    [SerializeField] private int _specialtyId;
    [HideInInspector] [SerializeField] private float _debounceSeconds = 0.4f;

    private OnboardingFlowController _flow;
    private bool _isPlaying;
    private bool _isAvailable;

    public OnboardingStep Step => _step;
    public int SpecialtyId => _specialtyId;

    private void Awake()
    {
        if (_interactable != null)
        {
            _interactable.OnInteract += Play;
            _interactable.SetInteractionEnabled(false);
            _interactable.SetPersistentVisible(false);
        }
    }

    private void OnDestroy()
    {
        if (_interactable != null)
        {
            _interactable.OnInteract -= Play;
        }
    }

    public void Initialize(OnboardingFlowController flow)
    {
        _flow = flow;
    }

    public void SetAvailable(bool value)
    {
        _isAvailable = value;

        if (_interactable != null)
        {
            _interactable.SetInteractionEnabled(value);
            _interactable.SetPersistentVisible(value);
        }
    }

    private async void Play()
    {
        if (_isPlaying || _isAvailable == false)
        {
            return;
        }

        _isPlaying = true;
        _interactable?.HideVisualNow();

        DialogueLine[] lines = _dialogueDatabase != null ? _dialogueDatabase.GetLines(_infoId) : null;
        if (_dialoguePlayer != null && lines != null && lines.Length > 0)
        {
            await _dialoguePlayer.PlayLinesAsync(lines);
        }

        if (_flow != null)
        {
            await _flow.CompleteStepAsync(this);
        }

        if (_interactable != null)
        {
            int debounceMs = Mathf.Max(1, Mathf.RoundToInt(_debounceSeconds * 1000f));
            await Task.Delay(debounceMs);
            _interactable.ReleaseBusy();
            if (_isAvailable)
            {
                _interactable.SetPersistentVisible(true);
            }
        }

        _isPlaying = false;
    }
}
