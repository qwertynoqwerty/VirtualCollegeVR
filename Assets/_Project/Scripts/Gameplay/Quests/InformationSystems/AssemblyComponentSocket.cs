using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public sealed class AssemblyComponentSocket : MonoBehaviour
{
    [Header("Collider Auto Repair")]
    [SerializeField] private bool _autoRepairTinyTriggerCollider = true;
    [SerializeField] private float _tinyTriggerVolumeThreshold = 0.000001f;

    [Header("Slot")]
    [SerializeField] private Collider _socketTrigger;

    private readonly HashSet<AssemblyComponentItem> _insideSocket = new HashSet<AssemblyComponentItem>();
    private AssemblyComponentItem _insertedBattery;
    private AssemblyComponentItem _expectedItem;
    private bool _questTaskActive;
    private ColliderHighlightHandle _highlightHandle;
    private MeshRenderer _slotMeshRenderer;

    private void Reset()
    {
        TryAssignLocalComponents();
    }

    private void OnValidate()
    {
        TryAssignLocalComponents();
        _slotMeshRenderer = GetComponent<MeshRenderer>();
        UpdateHighlightVisual();
        UpdateInsertedStateVisuals();
    }

    public bool HasBatteryInserted => _insertedBattery != null;
    public AssemblyComponentItem InsertedBattery => _insertedBattery;
    public bool IsEmpty => HasBatteryInserted == false;
    public bool IsOperational => HasBatteryInserted;

    private void Awake()
    {
        TryRepairTinyTriggerCollider();
        TryAssignLocalComponents();
        _slotMeshRenderer = GetComponent<MeshRenderer>();

        if (_socketTrigger != null)
        {
            _socketTrigger.isTrigger = true;
        }

        if (_slotMeshRenderer != null)
        {
            _slotMeshRenderer.enabled = false;
        }

        UpdateInsertedStateVisuals();

        _highlightHandle = QuestHighlightUtility.CreateHighlight(_socketTrigger, QuestHighlightUtility.DefaultHighlightColor, QuestHighlightUtility.DefaultHighlightAlpha, true);
        UpdateSlotHighlight();
    }

    private void OnTriggerEnter(Collider other)
    {
        AssemblyComponentItem battery = GetBatteryFromCollider(other);
        if (battery != null)
        {
            _insideSocket.Add(battery);
            TryInsertFromInsideSet();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        AssemblyComponentItem battery = GetBatteryFromCollider(other);
        if (battery != null)
        {
            _insideSocket.Remove(battery);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (_insertedBattery != null)
        {
            return;
        }

        AssemblyComponentItem battery = GetBatteryFromCollider(other);
        if (battery == null)
        {
            return;
        }

        _insideSocket.Add(battery);
        TryInsertFromInsideSet();
    }

    private void Update()
    {
        TryInsertExpectedItemByBoundsFallback();
        UpdateSlotHighlight();
    }

    private void OnDestroy()
    {
        if (_highlightHandle != null)
        {
            _highlightHandle.Dispose();
            _highlightHandle = null;
        }
    }

    public void SetQuestTaskActive(bool value)
    {
        _questTaskActive = value;
        UpdateSlotHighlight();
    }

    public void SetExpectedItem(AssemblyComponentItem item)
    {
        _expectedItem = item;
        UpdateSlotHighlight();
    }

    private void InsertBattery(AssemblyComponentItem battery)
    {
        if (battery == null || _insertedBattery != null)
        {
            return;
        }

        _insertedBattery = battery;
        MetaGrabState.Release(_insertedBattery.gameObject);
        _insertedBattery.SetInteractionLocked(true);
        _insertedBattery.SetKinematic(true);
        _insertedBattery.gameObject.SetActive(false);
        _insideSocket.Remove(battery);
        UpdateInsertedStateVisuals();
        UpdateSlotHighlight();
    }

    private void UpdateSlotHighlight()
    {
        if (_highlightHandle == null)
        {
            return;
        }

        AssemblyComponentItem grabbedItem = AssemblyComponentItem.GetCurrentlyGrabbedItem();
        bool hasMatchingGrab = grabbedItem != null && (_expectedItem == null || grabbedItem == _expectedItem);
        bool show = _insertedBattery == null && _questTaskActive && IsEmpty && hasMatchingGrab;

        _highlightHandle.SetColor(QuestHighlightUtility.DefaultHighlightColor, QuestHighlightUtility.DefaultHighlightAlpha);
        _highlightHandle.SetVisible(show);
    }

    private void UpdateHighlightVisual()
    {
        if (_highlightHandle == null)
        {
            return;
        }

        _highlightHandle.SetColor(QuestHighlightUtility.DefaultHighlightColor, QuestHighlightUtility.DefaultHighlightAlpha);
    }

    private void UpdateInsertedStateVisuals()
    {
        if (_slotMeshRenderer == null)
        {
            return;
        }

        _slotMeshRenderer.enabled = _insertedBattery != null;
    }

    private static AssemblyComponentItem GetBatteryFromCollider(Collider other)
    {
        if (other == null)
        {
            return null;
        }

        AssemblyComponentItem battery = other.GetComponent<AssemblyComponentItem>();
        if (battery != null)
        {
            return battery;
        }

        return other.GetComponentInParent<AssemblyComponentItem>();
    }

    private void TryAssignLocalComponents()
    {
        if (_socketTrigger == null)
        {
            _socketTrigger = GetComponent<Collider>();
        }
    }

    private void TryRepairTinyTriggerCollider()
    {
        if (_autoRepairTinyTriggerCollider == false)
        {
            return;
        }

        BoxCollider boxCollider = GetComponent<BoxCollider>();
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (boxCollider == null || meshFilter == null || meshFilter.sharedMesh == null)
        {
            return;
        }

        Vector3 size = boxCollider.size;
        float volume = Mathf.Abs(size.x * size.y * size.z);
        if (volume >= _tinyTriggerVolumeThreshold)
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

    private void TryInsertFromInsideSet()
    {
        if (_insertedBattery != null)
        {
            return;
        }

        foreach (AssemblyComponentItem battery in _insideSocket)
        {
            if (battery == null)
            {
                continue;
            }

            if (_expectedItem == null || battery == _expectedItem)
            {
                InsertBattery(battery);
                break;
            }
        }
    }

    private void TryInsertExpectedItemByBoundsFallback()
    {
        if (_insertedBattery != null || _socketTrigger == null || _questTaskActive == false || _expectedItem == null)
        {
            return;
        }

        if (_expectedItem.IsGrabbed)
        {
            return;
        }

        Collider expectedCollider = GetFirstEnabledCollider(_expectedItem);
        if (expectedCollider == null)
        {
            return;
        }

        if (expectedCollider.bounds.Intersects(_socketTrigger.bounds))
        {
            InsertBattery(_expectedItem);
        }
    }

    private static Collider GetFirstEnabledCollider(AssemblyComponentItem item)
    {
        if (item == null)
        {
            return null;
        }

        Collider[] colliders = item.GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null && colliders[i].enabled)
            {
                return colliders[i];
            }
        }

        return null;
    }
}

public sealed class ColliderHighlightHandle
{
    private readonly GameObject[] _nodes;
    private readonly Material _material;

    public ColliderHighlightHandle(GameObject[] nodes, Material material)
    {
        _nodes = nodes;
        _material = material;
    }

    public void SetVisible(bool value)
    {
        if (_nodes == null)
        {
            return;
        }

        for (int i = 0; i < _nodes.Length; i++)
        {
            if (_nodes[i] != null)
            {
                _nodes[i].SetActive(value);
            }
        }
    }

    public void SetColor(Color color, float alpha)
    {
        if (_material == null)
        {
            return;
        }

        Color tinted = color;
        tinted.a = alpha;
        QuestHighlightUtility.SetMaterialColor(_material, tinted);
    }

    public void Dispose()
    {
        if (_nodes != null)
        {
            for (int i = 0; i < _nodes.Length; i++)
            {
                if (_nodes[i] != null)
                {
                    Object.Destroy(_nodes[i]);
                }
            }
        }

        if (_material != null)
        {
            Object.Destroy(_material);
        }
    }
}

public static class QuestHighlightUtility
{
    public static readonly Color DefaultHighlightColor = new Color(1f, 1f, 0.5f, 1f);
    public const float DefaultHighlightAlpha = 0.25f;

    public static ColliderHighlightHandle CreateHighlight(Collider collider, Color color, float alpha, bool includeTriggers)
    {
        if (collider == null)
        {
            return null;
        }

        return CreateHighlight(new[] { collider }, color, alpha, includeTriggers);
    }

    public static ColliderHighlightHandle CreateHighlight(Collider[] colliders, Color color, float alpha, bool includeTriggers)
    {
        if (colliders == null || colliders.Length == 0)
        {
            return null;
        }

        List<Collider> sourceColliders = CollectSourceColliders(colliders, includeTriggers);
        if (sourceColliders.Count == 0)
        {
            return null;
        }

        Material material = BuildHighlightMaterial(color, alpha);
        if (material == null)
        {
            return null;
        }

        List<GameObject> nodes = new List<GameObject>(sourceColliders.Count);
        for (int i = 0; i < sourceColliders.Count; i++)
        {
            GameObject node = CreateNode(sourceColliders[i], material);
            if (node != null)
            {
                nodes.Add(node);
            }
        }

        if (nodes.Count == 0)
        {
            Object.Destroy(material);
            return null;
        }

        ColliderHighlightHandle handle = new ColliderHighlightHandle(nodes.ToArray(), material);
        handle.SetVisible(false);
        return handle;
    }

    public static void SetMaterialColor(Material material, Color color)
    {
        if (material == null)
        {
            return;
        }

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }
    }

    public static void ConfigureMaterialAsTransparent(Material material)
    {
        ConfigureAsTransparent(material);
    }

    private static List<Collider> CollectSourceColliders(Collider[] colliders, bool includeTriggers)
    {
        List<Collider> preferred = new List<Collider>();
        List<Collider> fallback = new List<Collider>();

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            if (collider == null)
            {
                continue;
            }

            fallback.Add(collider);
            if (includeTriggers || collider.isTrigger == false)
            {
                preferred.Add(collider);
            }
        }

        return preferred.Count > 0 ? preferred : fallback;
    }

    private static Material BuildHighlightMaterial(Color color, float alpha)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Color");
        }
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }
        if (shader == null)
        {
            return null;
        }

        Material material = new Material(shader);
        Color tinted = color;
        tinted.a = alpha;
        SetMaterialColor(material, tinted);
        ConfigureAsTransparent(material);
        return material;
    }

    private static GameObject CreateNode(Collider collider, Material material)
    {
        if (collider == null)
        {
            return null;
        }

        if (collider is BoxCollider boxCollider)
        {
            return CreateBoxNode(boxCollider, material);
        }

        if (collider is SphereCollider sphereCollider)
        {
            return CreateSphereNode(sphereCollider, material);
        }

        if (collider is CapsuleCollider capsuleCollider)
        {
            return CreateCapsuleNode(capsuleCollider, material);
        }

        if (collider is MeshCollider meshCollider)
        {
            return CreateMeshNode(meshCollider, material);
        }

        return CreateBoundsNode(collider, material);
    }

    private static GameObject CreateBoxNode(BoxCollider collider, Material material)
    {
        GameObject node = GameObject.CreatePrimitive(PrimitiveType.Cube);
        PrepareNode(node, collider.transform, material);
        node.transform.localPosition = collider.center;
        node.transform.localRotation = Quaternion.identity;
        node.transform.localScale = collider.size;
        return node;
    }

    private static GameObject CreateSphereNode(SphereCollider collider, Material material)
    {
        GameObject node = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        PrepareNode(node, collider.transform, material);
        float diameter = collider.radius * 2f;
        node.transform.localPosition = collider.center;
        node.transform.localRotation = Quaternion.identity;
        node.transform.localScale = new Vector3(diameter, diameter, diameter);
        return node;
    }

    private static GameObject CreateCapsuleNode(CapsuleCollider collider, Material material)
    {
        GameObject node = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        PrepareNode(node, collider.transform, material);

        float diameter = collider.radius * 2f;
        Vector3 scale = new Vector3(diameter, collider.height * 0.5f, diameter);
        Quaternion rotation = Quaternion.identity;

        if (collider.direction == 0)
        {
            scale = new Vector3(collider.height * 0.5f, diameter, diameter);
            rotation = Quaternion.Euler(0f, 0f, 90f);
        }
        else if (collider.direction == 2)
        {
            scale = new Vector3(diameter, diameter, collider.height * 0.5f);
            rotation = Quaternion.Euler(90f, 0f, 0f);
        }

        node.transform.localPosition = collider.center;
        node.transform.localRotation = rotation;
        node.transform.localScale = scale;
        return node;
    }

    private static GameObject CreateMeshNode(MeshCollider collider, Material material)
    {
        if (collider.sharedMesh == null)
        {
            return null;
        }

        GameObject node = new GameObject("QuestHighlight_Mesh");
        node.transform.SetParent(collider.transform, false);
        node.transform.localPosition = Vector3.zero;
        node.transform.localRotation = Quaternion.identity;
        node.transform.localScale = Vector3.one;

        MeshFilter meshFilter = node.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = collider.sharedMesh;

        MeshRenderer meshRenderer = node.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = material;
        ConfigureRenderer(meshRenderer);
        SetNodeLayer(node);
        return node;
    }

    private static GameObject CreateBoundsNode(Collider collider, Material material)
    {
        GameObject node = GameObject.CreatePrimitive(PrimitiveType.Cube);
        PrepareNode(node, collider.transform, material);
        node.transform.position = collider.bounds.center;
        node.transform.rotation = Quaternion.identity;

        Vector3 lossyScale = collider.transform.lossyScale;
        node.transform.localScale = new Vector3(
            SafeDivide(collider.bounds.size.x, lossyScale.x),
            SafeDivide(collider.bounds.size.y, lossyScale.y),
            SafeDivide(collider.bounds.size.z, lossyScale.z));

        return node;
    }

    private static void PrepareNode(GameObject node, Transform parent, Material material)
    {
        node.name = "QuestHighlight";
        node.transform.SetParent(parent, false);

        Renderer renderer = node.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.sharedMaterial = material;
            ConfigureRenderer(renderer);
        }

        Collider createdCollider = node.GetComponent<Collider>();
        if (createdCollider != null)
        {
            createdCollider.enabled = false;
        }

        SetNodeLayer(node);
    }

    private static void ConfigureRenderer(Renderer renderer)
    {
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.lightProbeUsage = LightProbeUsage.Off;
        renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
    }

    private static void SetNodeLayer(GameObject node)
    {
        int ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
        if (ignoreRaycastLayer >= 0)
        {
            node.layer = ignoreRaycastLayer;
        }
    }

    private static float SafeDivide(float value, float divisor)
    {
        if (Mathf.Abs(divisor) <= 0.0001f)
        {
            return value;
        }

        return value / divisor;
    }

    private static void ConfigureAsTransparent(Material material)
    {
        if (material == null)
        {
            return;
        }

        if (material.HasProperty("_Surface"))
        {
            material.SetFloat("_Surface", 1f);
        }

        if (material.HasProperty("_Blend"))
        {
            material.SetFloat("_Blend", 0f);
        }

        if (material.HasProperty("_SrcBlend"))
        {
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        }

        if (material.HasProperty("_DstBlend"))
        {
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        }

        if (material.HasProperty("_ZWrite"))
        {
            material.SetFloat("_ZWrite", 0f);
        }

        if (material.HasProperty("_Cull"))
        {
            material.SetFloat("_Cull", (float)CullMode.Off);
        }

        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHATEST_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");

        material.SetOverrideTag("RenderType", "Transparent");
        material.renderQueue = 3000;
    }
}
