using UnityEngine;

public sealed class WarehouseQuestItem : MonoBehaviour
{
    [Header("Auto Refs")]
    [SerializeField, HideInInspector] private Collider _itemCollider;

    private Collider[] _highlightColliders;
    private ColliderHighlightHandle _highlightHandle;

    public bool IsGrabbed => MetaGrabState.IsGrabbed(gameObject);
    public Collider ItemCollider => _itemCollider;

    private void OnValidate()
    {
        AutoAssign();

        if (_highlightHandle != null)
        {
            _highlightHandle.SetColor(QuestHighlightUtility.DefaultHighlightColor, QuestHighlightUtility.DefaultHighlightAlpha);
        }
    }

    private void Awake()
    {
        AutoAssign();
        _highlightColliders = GetComponentsInChildren<Collider>(true);
        _highlightHandle = QuestHighlightUtility.CreateHighlight(_highlightColliders, QuestHighlightUtility.DefaultHighlightColor, QuestHighlightUtility.DefaultHighlightAlpha, false);
    }

    private void OnDestroy()
    {
        if (_highlightHandle != null)
        {
            _highlightHandle.Dispose();
            _highlightHandle = null;
        }
    }

    public void SetHighlight(bool value)
    {
        if (_highlightHandle == null)
        {
            return;
        }

        _highlightHandle.SetColor(QuestHighlightUtility.DefaultHighlightColor, QuestHighlightUtility.DefaultHighlightAlpha);
        _highlightHandle.SetVisible(value);
    }

    private void AutoAssign()
    {
        if (_itemCollider == null)
        {
            _itemCollider = GetComponent<Collider>();
            if (_itemCollider == null)
            {
                _itemCollider = GetComponentInChildren<Collider>(true);
            }
        }
    }
}
