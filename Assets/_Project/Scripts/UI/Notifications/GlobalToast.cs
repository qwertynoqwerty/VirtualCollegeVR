using System.Collections;
using TMPro;
using UnityEngine;

public class GlobalToast : MonoBehaviour
{
    public static GlobalToast Instance { get; private set; }

    [SerializeField] private GameObject _root;
    [SerializeField] private TMP_Text _text;
    [SerializeField] private AudioSource _audioSource;

    private float _defaultDelayBefore = 0f;
    private float _defaultVisible = 2.5f;
    private Coroutine _co;

    protected virtual void Awake()
    {
        if (Instance == null || Instance == this)
        {
            Instance = this;
        }

        if (_root != null)
        {
            _root.SetActive(false);
        }
    }

    protected virtual void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void Show(string message)
    {
        Show(message, _defaultDelayBefore, _defaultVisible);
    }

    public void Show(string message, float delayBefore, float visibleSeconds)
    {
        if (gameObject.activeInHierarchy == false)
        {
            gameObject.SetActive(true);
        }

        if (isActiveAndEnabled == false)
        {
            return;
        }

        if (_co != null)
        {
            StopCoroutine(_co);
            _co = null;
        }

        _co = StartCoroutine(ShowRoutine(message, delayBefore, visibleSeconds));
    }

    public static void ShowGlobal(string message, float delayBefore = 0f, float visibleSeconds = 2.5f)
    {
        if (Instance != null)
        {
            Instance.Show(message, delayBefore, visibleSeconds);
        }
    }

    private IEnumerator ShowRoutine(string message, float delayBefore, float visibleSeconds)
    {
        if (delayBefore > 0f)
        {
            yield return new WaitForSeconds(delayBefore);
        }

        if (_text != null)
        {
            _text.text = message;
        }

        if (_root != null)
        {
            _root.SetActive(true);
        }

        if (_audioSource != null)
        {
            _audioSource.Stop();
            _audioSource.Play();
        }

        if (visibleSeconds > 0f)
        {
            yield return new WaitForSeconds(visibleSeconds);
        }

        _co = null;

        if (_root != null)
        {
            _root.SetActive(false);
        }
    }
}
