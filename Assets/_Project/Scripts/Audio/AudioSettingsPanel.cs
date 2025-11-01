using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class AudioSettingsPanel : MonoBehaviour
{
    private const string MusicKey = "music_volume";
    private const string SfxKey   = "sfx_volume";

    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _sfxSlider;
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _sfxPreviewSource;
    [SerializeField] private AudioClip _sfxTestClip;

    public static event Action<float, float> OnVolumesApplied;

    private bool _bound;

    private void Awake()
    {
        float music = PlayerPrefs.GetFloat(MusicKey, 0.6f);
        float sfx   = PlayerPrefs.GetFloat(SfxKey,   0.6f);

        if (_musicSlider) _musicSlider.SetValueWithoutNotify(Mathf.Clamp01(music));
        if (_sfxSlider)   _sfxSlider.SetValueWithoutNotify(Mathf.Clamp01(sfx));

        ApplyVolumes();
    }


    private void OnEnable()
    {
        if (_bound) return;
        if (_musicSlider) _musicSlider.onValueChanged.AddListener(OnMusicChanged);
        if (_sfxSlider)   _sfxSlider.onValueChanged.AddListener(OnSfxChanged);
        _bound = true;
    }
    
    private void OnDisable()
    {
        if (_bound == false) return;

        if (_musicSlider != null) _musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
        if (_sfxSlider   != null) _sfxSlider.onValueChanged.RemoveListener(OnSfxChanged);

        _bound = false;
    }

    private static float Clamp01(float v)
    {
        if (v < 0f) return 0f;
        if (v > 1f) return 1f;
        return v;
    }

    private void OnMusicChanged(float v)
    {
        float value = Clamp01(v);
        PlayerPrefs.SetFloat(MusicKey, value);
        PlayerPrefs.Save();
        ApplyVolumes();
    }


    private void OnSfxChanged(float v)
    {
        float value = Clamp01(v);
        PlayerPrefs.SetFloat(SfxKey, value);
        PlayerPrefs.Save();
        ApplyVolumes();

        if (_sfxPreviewSource != null && _sfxTestClip != null)
            _sfxPreviewSource.PlayOneShot(_sfxTestClip);
    }

    private void ApplyVolumes()
    {
        float music = Mathf.Clamp01(PlayerPrefs.GetFloat(MusicKey, 0.6f));
        float sfx   = Mathf.Clamp01(PlayerPrefs.GetFloat(SfxKey,   0.6f));

        if (_musicSource)       _musicSource.volume = music;
        if (_sfxPreviewSource)  _sfxPreviewSource.volume = sfx;

        OnVolumesApplied?.Invoke(music, sfx);
    }

}