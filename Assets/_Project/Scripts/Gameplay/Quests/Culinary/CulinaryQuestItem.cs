using UnityEngine;

public sealed class CulinaryQuestItem : MonoBehaviour
{
    private Collider _itemCollider;
    private Collider[] _highlightColliders;
    private ColliderHighlightHandle _highlightHandle;

    public Collider ItemCollider => _itemCollider;
    public bool IsGrabbed => MetaGrabState.IsGrabbed(gameObject);

    private void OnValidate()
    {
        AutoAssign();
    }

    private void Awake()
    {
        AutoAssign();
        _highlightColliders = GetComponentsInChildren<Collider>(true);
        _highlightHandle = QuestHighlightUtility.CreateHighlight(
            _highlightColliders,
            QuestHighlightUtility.DefaultHighlightColor,
            QuestHighlightUtility.DefaultHighlightAlpha,
            false);
    }

    private void OnDestroy()
    {
        if (_highlightHandle != null)
        {
            _highlightHandle.Dispose();
            _highlightHandle = null;
        }
    }

    public void SetHighlight(bool visible)
    {
        if (_highlightHandle == null)
        {
            return;
        }

        _highlightHandle.SetColor(QuestHighlightUtility.DefaultHighlightColor, QuestHighlightUtility.DefaultHighlightAlpha);
        _highlightHandle.SetVisible(visible);
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
