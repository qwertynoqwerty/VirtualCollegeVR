using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public sealed class DialoguePlayer : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject _dialogueRoot;
    [SerializeField] private bool _autoToggleRoot = true;

    [SerializeField] private TMP_Text _textLabel;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private float _typewriterCharsPerSecond = 35f;
    [SerializeField] private float _fallbackStepSeconds = 2.5f;

    public bool IsPlaying { get; private set; }
    private bool _suppressAutoHide;

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

        if (_autoToggleRoot && _dialogueRoot != null)
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
            float waitSeconds = _fallbackStepSeconds;

            if (_audioSource != null)
            {
                _audioSource.Stop();
                _audioSource.clip = line.audioClip;
                if (_audioSource.clip != null)
                {
                    _audioSource.Play();
                    waitSeconds = _audioSource.clip.length > 0.05f ? _audioSource.clip.length : _fallbackStepSeconds;
                }
            }

            await TypeTextAsync(line.text, ct);

            int ms = Mathf.Max(1, Mathf.RoundToInt(waitSeconds * 1000f));
            await Task.Delay(ms, ct);
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
        if (_autoToggleRoot && _dialogueRoot != null && !_dialogueRoot.activeSelf)
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

        if (_autoToggleRoot && _dialogueRoot != null && _dialogueRoot.activeSelf)
        {
            _dialogueRoot.SetActive(false);
        }
    }
}
