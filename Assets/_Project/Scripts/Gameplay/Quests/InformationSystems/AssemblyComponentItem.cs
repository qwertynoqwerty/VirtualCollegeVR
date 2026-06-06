using System;
using UnityEngine;

public sealed class AssemblyComponentItem : MonoBehaviour
{
    [Header("Collider Auto Repair")]
    [SerializeField] private bool _autoRepairTinyBoxCollider = true;
    [SerializeField] private float _tinyColliderVolumeThreshold = 0.000001f;

    private Rigidbody _rigidbody;
    private Collider[] _colliders;
    private bool[] _collidersEnabled;
    private bool _interactionLocked;
    private bool _wasGrabbedLastFrame;
    private Behaviour[] _grabBehaviours;
    private bool[] _grabBehavioursEnabled;
    private ColliderHighlightHandle _highlightHandle;

    [Header("Release Stability")]
    [SerializeField] private bool _stabilizeOnRelease = true;
    [SerializeField] private float _maxReleaseLinearSpeed = 2.5f;
    [SerializeField] private float _maxReleaseAngularSpeed = 10f;

    public bool IsGrabbed => MetaGrabState.IsGrabbed(gameObject);

    public static bool AnyBatteryGrabbed { get; private set; }

    private void Awake()
    {
        TryRepairTinyBoxCollider();

        if (_rigidbody == null)
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        _colliders = GetComponentsInChildren<Collider>();
        if (_colliders != null)
        {
            _collidersEnabled = new bool[_colliders.Length];
            for (int i = 0; i < _colliders.Length; i++)
            {
                _collidersEnabled[i] = _colliders[i] != null && _colliders[i].enabled;
            }
        }

        CacheGrabBehaviours();
        _highlightHandle = QuestHighlightUtility.CreateHighlight(_colliders, QuestHighlightUtility.DefaultHighlightColor, QuestHighlightUtility.DefaultHighlightAlpha, false);
    }

    private void OnValidate()
    {
        if (_highlightHandle != null)
        {
            _highlightHandle.SetColor(QuestHighlightUtility.DefaultHighlightColor, QuestHighlightUtility.DefaultHighlightAlpha);
        }
    }

    private void OnDestroy()
    {
        if (_highlightHandle != null)
        {
            _highlightHandle.Dispose();
            _highlightHandle = null;
        }
    }

    private void TryRepairTinyBoxCollider()
    {
        if (_autoRepairTinyBoxCollider == false)
        {
            return;
        }

        BoxCollider boxCollider = GetComponent<BoxCollider>();
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (boxCollider == null || boxCollider.isTrigger || meshFilter == null || meshFilter.sharedMesh == null)
        {
            return;
        }

        Vector3 size = boxCollider.size;
        float volume = Mathf.Abs(size.x * size.y * size.z);
        if (volume >= _tinyColliderVolumeThreshold)
        {
            return;
        }

        Bounds meshBounds = meshFilter.sharedMesh.bounds;
        if (meshBounds.size.sqrMagnitude <= 0f)
        {
            return;
        }

        boxCollider.center = meshBounds.center;
        boxCollider.size = meshBounds.size;
    }

    public void SnapTo(Transform snapTransform)
    {
        if (snapTransform == null)
        {
            return;
        }

        SnapToPose(snapTransform.position, snapTransform.rotation);
    }

    public void SnapToPose(Vector3 position, Quaternion rotation)
    {
        if (_rigidbody != null)
        {
            _rigidbody.position = position;
            _rigidbody.rotation = rotation;
            return;
        }

        transform.SetPositionAndRotation(position, rotation);
    }

    public void SnapColliderCenterToPose(Vector3 targetCenter, Quaternion rotation)
    {
        Collider snapCollider = GetFirstEnabledCollider();
        if (snapCollider == null)
        {
            SnapToPose(targetCenter, rotation);
            return;
        }

        Vector3 currentCenter = snapCollider.bounds.center;
        Vector3 delta = transform.position - currentCenter;
        SnapToPose(targetCenter + delta, rotation);
    }

    public void SetKinematic(bool value)
    {
        if (_rigidbody != null)
        {
            _rigidbody.isKinematic = value;

            if (value)
            {
                _rigidbody.linearVelocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;
            }
        }
    }

    public void ReleaseFromSocket()
    {
        if (_rigidbody == null)
        {
            return;
        }

        _rigidbody.isKinematic = false;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _rigidbody.WakeUp();
    }

    public void SetInteractionLocked(bool value)
    {
        _interactionLocked = value;
        if (value)
        {
            MetaGrabState.Release(gameObject);
            SetKinematic(true);
        }

        ApplyGrabBehavioursLock(value);

        if (_colliders == null)
        {
            return;
        }

        for (int i = 0; i < _colliders.Length; i++)
        {
            if (_colliders[i] == null)
            {
                continue;
            }

            _colliders[i].enabled = value == false && _collidersEnabled != null && i < _collidersEnabled.Length
                ? _collidersEnabled[i]
                : false;
        }
    }

    public void SetQuestHighlight(bool value)
    {
        if (_highlightHandle == null)
        {
            return;
        }

        _highlightHandle.SetColor(QuestHighlightUtility.DefaultHighlightColor, QuestHighlightUtility.DefaultHighlightAlpha);
        _highlightHandle.SetVisible(value);
    }

    private void OnEnable()
    {
        _wasGrabbedLastFrame = IsGrabbed;
        UpdateAnyGrabbedFlag();
    }

    private void Update()
    {
        bool isGrabbed = IsGrabbed;

        if (_interactionLocked == false && isGrabbed)
        {
            AnyBatteryGrabbed = true;
        }

        if (_interactionLocked == false && isGrabbed == false && _wasGrabbedLastFrame)
        {
            StabilizeAfterRelease();
        }

        if (AnyBatteryGrabbed && isGrabbed == false)
        {
            UpdateAnyGrabbedFlag();
        }

        _wasGrabbedLastFrame = isGrabbed;
    }

    private static void UpdateAnyGrabbedFlag()
    {
        AssemblyComponentItem[] batteries = FindObjectsByType<AssemblyComponentItem>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < batteries.Length; i++)
        {
            if (batteries[i] != null && batteries[i].IsGrabbed)
            {
                AnyBatteryGrabbed = true;
                return;
            }
        }

        AnyBatteryGrabbed = false;
    }

    public static AssemblyComponentItem GetCurrentlyGrabbedItem()
    {
        AssemblyComponentItem[] items = FindObjectsByType<AssemblyComponentItem>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null && items[i].IsGrabbed)
            {
                return items[i];
            }
        }

        return null;
    }

    private void StabilizeAfterRelease()
    {
        if (_stabilizeOnRelease == false || _rigidbody == null || _rigidbody.isKinematic)
        {
            return;
        }

        Vector3 linearVelocity = _rigidbody.linearVelocity;
        float linearSpeed = linearVelocity.magnitude;
        if (linearSpeed > _maxReleaseLinearSpeed && linearSpeed > 0f)
        {
            _rigidbody.linearVelocity = linearVelocity.normalized * _maxReleaseLinearSpeed;
        }

        Vector3 angularVelocity = _rigidbody.angularVelocity;
        float angularSpeed = angularVelocity.magnitude;
        if (angularSpeed > _maxReleaseAngularSpeed && angularSpeed > 0f)
        {
            _rigidbody.angularVelocity = angularVelocity.normalized * _maxReleaseAngularSpeed;
        }
    }

    private void CacheGrabBehaviours()
    {
        Behaviour[] allBehaviours = GetComponentsInChildren<Behaviour>(true);
        int count = 0;
        for (int i = 0; i < allBehaviours.Length; i++)
        {
            if (IsGrabDriverBehaviour(allBehaviours[i]))
            {
                count++;
            }
        }

        _grabBehaviours = new Behaviour[count];
        _grabBehavioursEnabled = new bool[count];

        int index = 0;
        for (int i = 0; i < allBehaviours.Length; i++)
        {
            Behaviour behaviour = allBehaviours[i];
            if (IsGrabDriverBehaviour(behaviour) == false)
            {
                continue;
            }

            _grabBehaviours[index] = behaviour;
            _grabBehavioursEnabled[index] = behaviour != null && behaviour.enabled;
            index++;
        }
    }

    private void ApplyGrabBehavioursLock(bool locked)
    {
        if (_grabBehaviours == null)
        {
            return;
        }

        for (int i = 0; i < _grabBehaviours.Length; i++)
        {
            Behaviour behaviour = _grabBehaviours[i];
            if (behaviour == null)
            {
                continue;
            }

            behaviour.enabled = locked == false && _grabBehavioursEnabled != null && i < _grabBehavioursEnabled.Length
                ? _grabBehavioursEnabled[i]
                : false;
        }
    }

    private static bool IsGrabDriverBehaviour(Behaviour behaviour)
    {
        if (behaviour == null)
        {
            return false;
        }

        Type type = behaviour.GetType();
        string typeName = type.Name;
        string fullName = type.FullName ?? string.Empty;

        return typeName.IndexOf("Grab", StringComparison.OrdinalIgnoreCase) >= 0 ||
               fullName.IndexOf("Oculus.Interaction", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private Collider GetFirstEnabledCollider()
    {
        if (_colliders == null)
        {
            _colliders = GetComponentsInChildren<Collider>();
        }

        if (_colliders == null)
        {
            return null;
        }

        for (int i = 0; i < _colliders.Length; i++)
        {
            if (_colliders[i] != null && _colliders[i].enabled)
            {
                return _colliders[i];
            }
        }

        for (int i = 0; i < _colliders.Length; i++)
        {
            if (_colliders[i] != null)
            {
                return _colliders[i];
            }
        }

        return null;
    }
}
