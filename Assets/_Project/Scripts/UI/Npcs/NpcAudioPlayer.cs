using TMPro;
using UnityEngine;

public sealed class NpcAudioPlayer : MonoBehaviour
{
    [SerializeField] private TMP_Text _dialogueLabel;
    [SerializeField] private AudioSource _audioSource;

    public async void PlayStart(string dialogueText, string audioRelativePath)
    {
        if (_dialogueLabel != null)
        {
            _dialogueLabel.text = dialogueText;
            _dialogueLabel.gameObject.SetActive(true);
        }

        if (_audioSource != null)
        {
            if (string.IsNullOrEmpty(audioRelativePath) == false)
            {
                AudioClip clip = await StreamingAudioLoader.LoadClipAsync(audioRelativePath);
                if (clip != null)
                {
                    _audioSource.Stop();
                    _audioSource.clip = clip;
                    _audioSource.Play();
                }
            }
        }
    }

    public async void PlayComplete(string dialogueText, string audioRelativePath)
    {
        if (_dialogueLabel != null)
        {
            _dialogueLabel.text = dialogueText;
            _dialogueLabel.gameObject.SetActive(true);
        }

        if (_audioSource != null)
        {
            if (string.IsNullOrEmpty(audioRelativePath) == false)
            {
                AudioClip clip = await StreamingAudioLoader.LoadClipAsync(audioRelativePath);
                if (clip != null)
                {
                    _audioSource.Stop();
                    _audioSource.clip = clip;
                    _audioSource.Play();
                }
            }
        }
    }
}