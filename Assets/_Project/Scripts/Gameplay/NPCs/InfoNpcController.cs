using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public sealed class InfoNpcController : MonoBehaviour
{
    [SerializeField] private InfoNpcDialogueDatabase _dialogueDatabase;
    [SerializeField] private Interactable _interactable;
    [SerializeField] private DialoguePlayer _dialoguePlayer;
    [SerializeField] private Animator _animator;
    [SerializeField] private TMP_Text _npcNameLabel;
    [SerializeField] private int _infoId;
    private string _talkingParameter = "IsTalking";
    private string _defaultStateName = "Relax";
    private string _talkStateName = "Talk";
    private float _debounceSeconds = 0.4f;
    private float _animationTransitionSeconds = 0.05f;
    private bool _isPlaying;
    private int _talkingHash;
    private int _defaultStateHash;
    private int _talkStateHash;

    private void Awake()
    {
        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>();
        }

        _talkingHash = Animator.StringToHash(_talkingParameter);
        _defaultStateHash = Animator.StringToHash(_defaultStateName);
        _talkStateHash = Animator.StringToHash(_talkStateName);
        SetTalking(false);
        ApplyNpcName();

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

        try
        {
            DialogueLine[] lines = _dialogueDatabase != null ? _dialogueDatabase.GetLines(_infoId) : null;
            if (_dialoguePlayer != null && lines != null && lines.Length > 0)
            {
                SetTalking(true);
                await _dialoguePlayer.PlayLinesAsync(lines);
            }

            if (_interactable != null)
            {
                int debounceMs = Mathf.Max(1, Mathf.RoundToInt(_debounceSeconds * 1000f));
                await Task.Delay(debounceMs);
                _interactable.ReleaseBusy();
            }
        }
        finally
        {
            SetTalking(false);
            _isPlaying = false;
        }
    }

    private void OnDisable()
    {
        SetTalking(false);
    }

    private void SetTalking(bool value)
    {
        if (_animator == null || string.IsNullOrEmpty(_talkingParameter))
        {
            return;
        }

        _animator.SetBool(_talkingHash, value);
        ForceAnimationState(value ? _talkStateHash : _defaultStateHash);
    }

    private void ForceAnimationState(int stateHash)
    {
        if (_animator == null || stateHash == 0 || _animator.HasState(0, stateHash) == false)
        {
            return;
        }

        float transitionSeconds = Mathf.Max(0f, _animationTransitionSeconds);
        _animator.CrossFadeInFixedTime(stateHash, transitionSeconds, 0, 0f);
    }

    private void ApplyNpcName()
    {
        if (_npcNameLabel == null)
        {
            return;
        }

        string npcName = _dialogueDatabase != null ? _dialogueDatabase.GetNpcName(_infoId) : null;
        _npcNameLabel.text = string.IsNullOrEmpty(npcName) ? string.Empty : npcName;
    }
}
