using System.Collections.Generic;
using UnityEngine;

public sealed class TerminalBatterySocket : MonoBehaviour
{
    [Header("Slot")]
    [SerializeField] private Color _highlightColor = new Color(1f, 1f, 0.5f, 1f);

    private readonly HashSet<TerminalBatteryItem> _insideSocket = new HashSet<TerminalBatteryItem>();
    private Collider _socketTrigger;
    private Transform _snapPoint;
    private Renderer _highlightRenderer;
    private TerminalBatteryItem _insertedBattery;
    private bool _questTaskActive;
    private Material _highlightMaterial;
    private Color _defaultColor;

    public bool HasBatteryInserted => _insertedBattery != null;
    public bool HasWorkingBattery => _insertedBattery != null && _insertedBattery.IsWorkingBattery;
    public bool IsEmpty => HasBatteryInserted == false;
    public bool IsFaulty => HasBatteryInserted && HasWorkingBattery == false;
    public bool IsOperational => HasWorkingBattery;

    private void Awake()
    {
        if (_socketTrigger == null)
        {
            _socketTrigger = GetComponent<Collider>();
        }

        if (_snapPoint == null)
        {
            _snapPoint = transform;
        }

        if (_socketTrigger != null)
        {
            _socketTrigger.isTrigger = true;
        }

        if (_highlightRenderer == null)
        {
            _highlightRenderer = GetComponent<Renderer>();
        }

        if (_highlightRenderer != null)
        {
            _highlightMaterial = _highlightRenderer.material;
            _defaultColor = _highlightMaterial.color;
        }

        UpdateSlotHighlight();
    }

    private void OnTriggerEnter(Collider other)
    {
        TerminalBatteryItem battery = GetBatteryFromCollider(other);
        if (battery != null)
        {
            _insideSocket.Add(battery);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        TerminalBatteryItem battery = GetBatteryFromCollider(other);
        if (battery != null)
        {
            _insideSocket.Remove(battery);
        }
    }

    private void Update()
    {
        if (_insertedBattery != null && _insertedBattery.GrabbableObject != null && _insertedBattery.GrabbableObject.IsBeingGrabbed)
        {
            _insertedBattery.SetKinematic(false);
            _insertedBattery = null;
        }

        if (_insertedBattery == null && _insideSocket.Count > 0)
        {
            foreach (TerminalBatteryItem battery in _insideSocket)
            {
                if (battery == null || battery.GrabbableObject == null)
                {
                    continue;
                }

                if (battery.GrabbableObject.IsBeingGrabbed == false)
                {
                    InsertBattery(battery);
                    break;
                }
            }
        }

        UpdateSlotHighlight();
    }

    public void SetQuestTaskActive(bool value)
    {
        _questTaskActive = value;
        UpdateSlotHighlight();
    }

    private void InsertBattery(TerminalBatteryItem battery)
    {
        _insertedBattery = battery;
        _insertedBattery.SnapTo(_snapPoint != null ? _snapPoint : transform);
        _insertedBattery.SetKinematic(true);
    }

    private void UpdateSlotHighlight()
    {
        if (_highlightMaterial == null)
        {
            return;
        }

        bool show = _questTaskActive && IsEmpty && TerminalBatteryItem.AnyBatteryGrabbed;
        _highlightMaterial.color = show ? _highlightColor : _defaultColor;
    }

    private static TerminalBatteryItem GetBatteryFromCollider(Collider other)
    {
        if (other == null)
        {
            return null;
        }

        TerminalBatteryItem battery = other.GetComponent<TerminalBatteryItem>();
        if (battery != null)
        {
            return battery;
        }

        return other.GetComponentInParent<TerminalBatteryItem>();
    }
}
