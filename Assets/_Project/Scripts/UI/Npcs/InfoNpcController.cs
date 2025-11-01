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

        if (_audioSource == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(_audioRelativePath) == true)
        {
            return;
        }

        var clip = await StreamingAudioLoader.LoadAsync(_audioRelativePath);
        if (clip == null)
        {
            return;
        }

        _audioSource.Stop();
        _audioSource.clip = clip;
        _audioSource.Play();
    }
}