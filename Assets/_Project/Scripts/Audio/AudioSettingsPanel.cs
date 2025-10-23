using UnityEngine;
using UnityEngine.UI;

public sealed class AudioSettingsPanel : MonoBehaviour
{
    [SerializeField] private AudioSource _music;
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _sfxSlider;

    private const string _kMusic = "vrcollege.vol.music";
    private const string _kSfx   = "vrcollege.vol.sfx";

    private void Start()
    {
        float music = PlayerPrefs.GetFloat(_kMusic, 0.7f);
        float sfx   = PlayerPrefs.GetFloat(_kSfx, 0.8f);

        if (_musicSlider != null)
        {
            _musicSlider.value = music;
            _musicSlider.onValueChanged.AddListener(OnMusicChanged);
        }
        if (_sfxSlider != null)
        {
            _sfxSlider.value = sfx;
            _sfxSlider.onValueChanged.AddListener(OnSfxChanged);
        }

        ApplyVolumes(music, sfx);
    }

    private void OnDestroy()
    {
        if (_musicSlider != null) _musicSlider.onValueChanged.RemoveListener(OnMusicChanged);
        if (_sfxSlider != null)   _sfxSlider.onValueChanged.RemoveListener(OnSfxChanged);
    }

    private void OnMusicChanged(float v)
    {
        PlayerPrefs.SetFloat(_kMusic, v);
        PlayerPrefs.Save();
        ApplyVolumes(v, PlayerPrefs.GetFloat(_kSfx, 0.8f));
    }

    private void OnSfxChanged(float v)
    {
        PlayerPrefs.SetFloat(_kSfx, v);
        PlayerPrefs.Save();
        ApplyVolumes(PlayerPrefs.GetFloat(_kMusic, 0.7f), v);
    }

    private void ApplyVolumes(float music, float sfx)
    {
        if (_music != null) _music.volume = Mathf.Clamp01(music);
        AudioListener.volume = Mathf.Clamp01(sfx);
    }
}