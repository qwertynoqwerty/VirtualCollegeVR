using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public sealed class DialoguePlayer : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject _dialogueRoot;
    [SerializeField] private TMP_Text _textLabel;
    [SerializeField] private AudioSource _audioSource;
    
    private float _typewriterCharsPerSecond = 18f;
    private float _fallbackStepSeconds = 0.25f;
    private bool _autoToggleRoot = true;
    public bool IsPlaying { get; private set; }
    private bool _suppressAutoHide;

    public void SetAutoToggleRoot(bool value)
    {
        _autoToggleRoot = value;
    }

    private void Awake()
    {
        if (_dialogueRoot == null)
        {
            _dialogueRoot = gameObject;
        }

        if (_textLabel != null)
        {
            _textLabel.text = string.Empty;
            _textLabel.gameObject.SetActive(false);
        }

        if (CanAutoToggleRoot())
        {
            _dialogueRoot.SetActive(false);
        }
    }

    public async Task PlayLinesAsync(DialogueLine[] lines, CancellationToken ct = default)
    {
        if (lines == null || lines.Length == 0)
        {
            return;
        }

        _suppressAutoHide = true;
        ShowDialogueUi();
        try
        {
            for (int i = 0; i < lines.Length; i++)
            {
                if (ct.IsCancellationRequested)
                {
                    return;
                }

                DialogueLine line = lines[i];
                await PlayLineAsync(line, ct);
            }
        }
        finally
        {
            _suppressAutoHide = false;
            HideDialogueUi();
        }
    }

    public async Task PlayLineAsync(DialogueLine line, CancellationToken ct = default)
    {
        if (line == null)
        {
            return;
        }

        IsPlaying = true;
        try
        {
            bool hasAudio = false;

            if (_audioSource != null)
            {
                _audioSource.Stop();
                _audioSource.clip = line.audioClip;
                if (_audioSource.clip != null)
                {
                    _audioSource.Play();
                    hasAudio = true;
                }
            }

            await TypeTextAsync(line.text, ct);

            if (hasAudio && _audioSource != null)
            {
                while (_audioSource.isPlaying)
                {
                    ct.ThrowIfCancellationRequested();
                    await Task.Delay(30, ct);
                }
            }
            else
            {
                int ms = Mathf.Max(1, Mathf.RoundToInt(_fallbackStepSeconds * 1000f));
                await Task.Delay(ms, ct);
            }
        }
        catch (TaskCanceledException)
        {
        }
        finally
        {
            IsPlaying = false;
            if (_suppressAutoHide == false)
            {
                HideDialogueUi();
            }
        }
    }

    private async Task TypeTextAsync(string fullText, CancellationToken ct)
    {
        if (_textLabel == null)
        {
            return;
        }

        string text = string.IsNullOrEmpty(fullText) ? string.Empty : fullText;
        _textLabel.text = string.Empty;
        _textLabel.gameObject.SetActive(true);

        if (text.Length == 0)
        {
            return;
        }

        float cps = _typewriterCharsPerSecond > 0f ? _typewriterCharsPerSecond : 35f;
        int delayMs = Mathf.Max(1, Mathf.RoundToInt(1000f / cps));

        for (int i = 1; i <= text.Length; i++)
        {
            ct.ThrowIfCancellationRequested();
            _textLabel.text = text.Substring(0, i);
            await Task.Delay(delayMs, ct);
        }
    }

    private void ShowDialogueUi()
    {
        if (CanAutoToggleRoot() && !_dialogueRoot.activeSelf)
        {
            _dialogueRoot.SetActive(true);
        }
    }

    private void HideDialogueUi()
    {
        if (_textLabel != null)
        {
            _textLabel.text = string.Empty;
            _textLabel.gameObject.SetActive(false);
        }

        if (_audioSource != null && _audioSource.isPlaying)
        {
            _audioSource.Stop();
        }

        if (CanAutoToggleRoot() && _dialogueRoot.activeSelf)
        {
            _dialogueRoot.SetActive(false);
        }
    }

    private bool CanAutoToggleRoot()
    {
        return _autoToggleRoot && _dialogueRoot != null && _dialogueRoot != gameObject;
    }
}
