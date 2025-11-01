using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public sealed class NpcAudioPlayer : MonoBehaviour
{
    [SerializeField] private TMP_Text _dialogueLabel;
    [SerializeField] private AudioSource _audioSource;

    public async void PlayStart(string dialogueText, string audioRelativePath)
    {
        await PlayInternal(dialogueText, audioRelativePath);
    }

    public async void PlayComplete(string dialogueText, string audioRelativePath)
    {
        await PlayInternal(dialogueText, audioRelativePath);
    }

    private async Task PlayInternal(string dialogueText, string audioRelativePath)
    {
        if (_dialogueLabel != null)
        {
            _dialogueLabel.text = dialogueText;
            _dialogueLabel.gameObject.SetActive(true);
        }

        if (_audioSource == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(audioRelativePath) == true)
        {
            return;
        }

        var clip = await StreamingAudioLoader.LoadAsync(audioRelativePath);
        if (clip == null)
        {
            return;
        }

        _audioSource.Stop();
        _audioSource.clip = clip;
        _audioSource.Play();
    }
}