using UltimateXR.Manipulation;
using UnityEngine;

public sealed class TerminalBatteryItem : MonoBehaviour
{
    [Header("Battery")]
    [SerializeField] private bool _isWorkingBattery = true;

    private UxrGrabbableObject _grabbableObject;
    private Rigidbody _rigidbody;

    public UxrGrabbableObject GrabbableObject => _grabbableObject;
    public bool IsWorkingBattery => _isWorkingBattery;
    public bool IsGrabbed => _grabbableObject != null && _grabbableObject.IsBeingGrabbed;

    public static bool AnyBatteryGrabbed { get; private set; }

    private void Awake()
    {
        if (_grabbableObject == null)
        {
            _grabbableObject = GetComponent<UxrGrabbableObject>();
        }

        if (_rigidbody == null)
        {
            _rigidbody = GetComponent<Rigidbody>();
        }
    }

    public void SnapTo(Transform snapTransform)
    {
        if (snapTransform == null)
        {
            return;
        }

        transform.SetPositionAndRotation(snapTransform.position, snapTransform.rotation);
    }

    public void SetKinematic(bool value)
    {
        if (_rigidbody != null)
        {
            _rigidbody.isKinematic = value;
        }
    }

    private void OnEnable()
    {
        UpdateAnyGrabbedFlag();
    }

    private void Update()
    {
        if (IsGrabbed)
        {
            AnyBatteryGrabbed = true;
            return;
        }

        if (AnyBatteryGrabbed)
        {
            UpdateAnyGrabbedFlag();
        }
    }

    private static void UpdateAnyGrabbedFlag()
    {
        TerminalBatteryItem[] batteries = FindObjectsByType<TerminalBatteryItem>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
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
}
