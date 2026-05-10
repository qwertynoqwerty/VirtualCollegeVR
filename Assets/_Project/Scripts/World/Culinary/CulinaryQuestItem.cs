using UltimateXR.Manipulation;
using UnityEngine;

public sealed class CulinaryQuestItem : MonoBehaviour
{
    [Header("Highlight")]
    [SerializeField] private Color _highlightColor = new Color(1f, 1f, 0.5f, 1f);

    private UxrGrabbableObject _grabbableObject;
    private Collider _itemCollider;
    private Renderer _renderer;
    private Material _material;
    private Color _defaultColor;

    public UxrGrabbableObject GrabbableObject => _grabbableObject;
    public Collider ItemCollider => _itemCollider;
    public bool IsGrabbed => _grabbableObject != null && _grabbableObject.IsBeingGrabbed;

    private void Awake()
    {
        _grabbableObject = GetComponent<UxrGrabbableObject>();
        _itemCollider = GetComponent<Collider>();
        _renderer = GetComponent<Renderer>();

        if (_renderer != null)
        {
            _material = _renderer.material;
            _defaultColor = _material.color;
        }
    }

    public void SetHighlight(bool visible)
    {
        if (_material != null)
        {
            _material.color = visible ? _highlightColor : _defaultColor;
        }
    }

}
