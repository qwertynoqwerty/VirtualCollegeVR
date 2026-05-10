using UltimateXR.Manipulation;
using UnityEngine;

public sealed class WarehouseQuestItem : MonoBehaviour
{
    [Header("Target Zone")]
    [SerializeField] private GameObject _targetZoneObject;
    [SerializeField, HideInInspector] private Collider _targetZoneCollider;
    [SerializeField, HideInInspector] private Renderer _targetZoneRenderer;

    [Header("Highlight")]
    [SerializeField] private Color _highlightColor = new Color(1f, 1f, 0.5f, 1f);

    [Header("Auto Refs")]
    [SerializeField, HideInInspector] private UxrGrabbableObject _grabbableObject;
    [SerializeField, HideInInspector] private Collider _itemCollider;
    [SerializeField, HideInInspector] private Renderer _itemRenderer;

    private Material _material;
    private Color _defaultColor;

    public bool IsGrabbed => _grabbableObject != null && _grabbableObject.IsBeingGrabbed;
    public Collider ItemCollider => _itemCollider;
    public Collider TargetZoneCollider => _targetZoneCollider;
    public Renderer TargetZoneRenderer => _targetZoneRenderer;

    private void OnValidate()
    {
        AutoAssign();
        AutoAssignTargetZone();
    }

    private void Awake()
    {
        AutoAssign();

        if (_itemRenderer != null)
        {
            _material = _itemRenderer.material;
            _defaultColor = _material.color;
        }

        AutoAssignTargetZone();
    }

    public void SetHighlight(bool value)
    {
        if (_material != null)
        {
            _material.color = value ? _highlightColor : _defaultColor;
        }
    }

    private void AutoAssign()
    {
        if (_grabbableObject == null)
        {
            _grabbableObject = GetComponent<UxrGrabbableObject>();
        }

        if (_itemCollider == null)
        {
            _itemCollider = GetComponent<Collider>();
        }

        if (_itemRenderer == null)
        {
            _itemRenderer = GetComponent<Renderer>();
        }
    }

    private void AutoAssignTargetZone()
    {
        if (_targetZoneObject == null)
        {
            _targetZoneCollider = null;
            _targetZoneRenderer = null;
            return;
        }

        _targetZoneCollider = _targetZoneObject.GetComponent<Collider>();
        _targetZoneRenderer = _targetZoneObject.GetComponent<Renderer>();
    }
}
