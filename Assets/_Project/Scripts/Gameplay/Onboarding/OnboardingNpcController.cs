using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public sealed class OnboardingNpcController : MonoBehaviour
{
    [SerializeField] private InfoNpcDialogueDatabase _dialogueDatabase;
    [SerializeField] private Interactable _interactable;
    [SerializeField] private DialoguePlayer _dialoguePlayer;
    [SerializeField] private Animator _animator;
    [SerializeField] private TMP_Text _npcNameLabel;
    [SerializeField] private OnboardingStep _step;
    [SerializeField] private int _infoId;
    [SerializeField] private int _finalInfoId;
    
    private OnboardingFlowController _flow;
    private string _talkingParameter = "IsTalking";
    private string _defaultStateName = "Relax";
    private string _talkStateName = "Talk";
    private float _animationTransitionSeconds = 0.05f;
    private float _debounceSeconds = 0.4f;
    private bool _isPlaying;
    private bool _isAvailable;
    private int _talkingHash;
    private int _defaultStateHash;
    private int _talkStateHash;

    public OnboardingStep Step => _step;

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

        if (value)
        {
            ApplyNpcName();
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

        try
        {
            int infoId = ResolveInfoId();
            DialogueLine[] lines = _dialogueDatabase != null ? _dialogueDatabase.GetLines(infoId) : null;
            if (_dialoguePlayer != null && lines != null && lines.Length > 0)
            {
                SetTalking(true);
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

        int infoId = ResolveInfoId();
        string npcName = _dialogueDatabase != null ? _dialogueDatabase.GetNpcName(infoId) : null;
        _npcNameLabel.text = string.IsNullOrEmpty(npcName) ? string.Empty : npcName;
    }

    private int ResolveInfoId()
    {
        if (_flow != null &&
            _flow.IsFinalCuratorStep &&
            _step == OnboardingStep.Curator &&
            _finalInfoId > 0)
        {
            return _finalInfoId;
        }

        return _infoId;
    }
}
