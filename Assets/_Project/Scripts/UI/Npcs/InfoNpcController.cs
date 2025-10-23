using TMPro;
using UnityEngine;

public sealed class InfoNpcController : MonoBehaviour
{
    [SerializeField] private TMP_Text _label;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private string _text;
    [SerializeField] private string _audioRelativePath;

    public async void Play()
    {
        if (_label != null)
        {
            _label.text = _text;
            _label.gameObject.SetActive(true);
        }

        if (_audioSource != null)
        {
            if (string.IsNullOrEmpty(_audioRelativePath) == false)
            {
                AudioClip clip = await StreamingAudioLoader.LoadClipAsync(_audioRelativePath);
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