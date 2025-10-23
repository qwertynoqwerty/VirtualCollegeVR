using System.Collections;
using TMPro;
using UnityEngine;

public sealed class AchievementToast : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private TMP_Text _text;

    [Header("Defaults")]
    [SerializeField] private float _defaultDelayBefore = 0f;
    [SerializeField] private float _defaultVisible = 2.5f;

    private Coroutine _co;

    private void Awake()
    {
        if (_root != null)
        {
            _root.SetActive(false);
        }
    }

    public void Show(string message)
    {
        Show(message, _defaultDelayBefore, _defaultVisible);
    }

    public void Show(string message, float delayBefore, float visibleSeconds)
    {
        if (_co != null)
        {
            StopCoroutine(_co);
            _co = null;
        }

        _co = StartCoroutine(ShowRoutine(message, delayBefore, visibleSeconds));
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

        if (visibleSeconds > 0f)
        {
            yield return new WaitForSeconds(visibleSeconds);
        }

        if (_root != null)
        {
            _root.SetActive(false);
        }

        _co = null;
    }
}