using UltimateXR.Manipulation;
using UnityEngine;

public sealed class ServerAssemblyPart : MonoBehaviour
{
    [Header("Part Highlight")]
    [SerializeField] private Color _partHighlightColor = new Color(1f, 1f, 0.5f, 1f);

    [Header("Slot")]
    [SerializeField] private Transform _slotObject;
    [SerializeField] private Color _slotHighlightColor = new Color(1f, 1f, 0.5f, 1f);

    private UxrGrabbableObject _grabbableObject;
    private Collider _partCollider;
    private Renderer _partHighlightRenderer;
    private Renderer _slotHighlightRenderer;
    private Collider _slotTrigger;
    private Transform _slotSnapPoint;
    private Material _partHighlightMaterial;
    private Material _slotHighlightMaterial;
    private Color _partDefaultColor;
    private Color _slotDefaultColor;

    public UxrGrabbableObject GrabbableObject => _grabbableObject;
    public Collider SlotTrigger => _slotTrigger;
    public Transform SlotSnapPoint => _slotSnapPoint != null ? _slotSnapPoint : transform;

    private void Awake()
    {
        if (_grabbableObject == null)
        {
            _grabbableObject = GetComponent<UxrGrabbableObject>();
        }

        if (_partCollider == null)
        {
            _partCollider = GetComponent<Collider>();
        }

        _partHighlightRenderer = GetComponent<Renderer>();
        if (_partHighlightRenderer != null)
        {
            _partHighlightMaterial = _partHighlightRenderer.material;
            _partDefaultColor = _partHighlightMaterial.color;
        }

        if (_slotObject == null)
        {
            _slotObject = transform;
        }

        _slotSnapPoint = _slotObject;
        _slotTrigger = _slotObject.GetComponent<Collider>();
        if (_slotTrigger != null)
        {
            _slotTrigger.isTrigger = true;
        }

        _slotHighlightRenderer = _slotObject.GetComponent<Renderer>();
        if (_slotHighlightRenderer != null)
        {
            _slotHighlightMaterial = _slotHighlightRenderer.material;
            _slotDefaultColor = _slotHighlightMaterial.color;
        }

        SetPartHighlight(false);
        SetSlotHighlight(false);
    }

    public bool IsPartInsideSlot()
    {
        return _partCollider != null && _slotTrigger != null && _partCollider.enabled && _slotTrigger.enabled && _partCollider.bounds.Intersects(_slotTrigger.bounds);
    }

    public void SetPartHighlight(bool visible)
    {
        if (_partHighlightMaterial != null)
        {
            _partHighlightMaterial.color = visible ? _partHighlightColor : _partDefaultColor;
        }
    }

    public void SetSlotHighlight(bool visible)
    {
        if (_slotHighlightMaterial != null)
        {
            _slotHighlightMaterial.color = visible ? _slotHighlightColor : _slotDefaultColor;
        }
    }
}
