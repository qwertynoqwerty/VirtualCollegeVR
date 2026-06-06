using UnityEngine;
using VRCollege.Net;
using VRCollege.Net.Dto;

public sealed class WarehouseItemPlacementQuestTask : MonoBehaviour
{
    [System.Serializable]
    private sealed class PlacementTarget
    {
        public WarehouseQuestItem item;
        public GameObject targetZoneObject;
        [HideInInspector] public Collider targetZoneTrigger;
        [HideInInspector] public Renderer[] targetZoneRenderers;

        [System.NonSerialized] private Material[][] _previewMaterials;
        [System.NonSerialized] private Material[][] _originalMaterials;
        [System.NonSerialized] private bool _visualsInitialized;
        [System.NonSerialized] private bool _isPlaced;

        public void AutoAssignZoneComponents()
        {
            if (targetZoneObject == null)
            {
                targetZoneTrigger = null;
                targetZoneRenderers = null;
                return;
            }

            targetZoneTrigger = targetZoneObject.GetComponent<Collider>();
            if (targetZoneTrigger == null)
            {
                targetZoneTrigger = targetZoneObject.GetComponentInChildren<Collider>(true);
            }

            targetZoneRenderers = targetZoneObject.GetComponentsInChildren<Renderer>(true);
        }

        public bool HasZoneVisualRenderer
        {
            get
            {
                return targetZoneRenderers != null && targetZoneRenderers.Length > 0;
            }
        }

        public bool IsPlaced => _isPlaced;

        public void InitializeVisuals()
        {
            if (_visualsInitialized)
            {
                return;
            }

            _visualsInitialized = true;

            if (HasZoneVisualRenderer == false)
            {
                return;
            }

            _originalMaterials = new Material[targetZoneRenderers.Length][];
            _previewMaterials = new Material[targetZoneRenderers.Length][];

            for (int i = 0; i < targetZoneRenderers.Length; i++)
            {
                Renderer renderer = targetZoneRenderers[i];
                if (renderer == null)
                {
                    continue;
                }

                Material[] originalMaterials = renderer.sharedMaterials;
                _originalMaterials[i] = originalMaterials;
                _previewMaterials[i] = CreatePreviewMaterials(originalMaterials);
                renderer.enabled = false;
            }
        }

        public void DisposeVisuals()
        {
            if (_previewMaterials == null)
            {
                return;
            }

            for (int i = 0; i < _previewMaterials.Length; i++)
            {
                Material[] materials = _previewMaterials[i];
                if (materials == null)
                {
                    continue;
                }

                for (int j = 0; j < materials.Length; j++)
                {
                    if (materials[j] != null)
                    {
                        Object.Destroy(materials[j]);
                    }
                }
            }

            _previewMaterials = null;
            _originalMaterials = null;
            _visualsInitialized = false;
        }

        public void SetWaitingState()
        {
            if (_isPlaced)
            {
                ApplyPlacedState();
                return;
            }

            ApplyRendererState(false, false);
        }

        public void SetPreviewState()
        {
            if (_isPlaced)
            {
                ApplyPlacedState();
                return;
            }

            ApplyRendererState(true, true);
        }

        public void ApplyPlacedState()
        {
            _isPlaced = true;
            ApplyRendererState(true, false);
        }

        private void ApplyRendererState(bool visible, bool usePreviewMaterials)
        {
            if (HasZoneVisualRenderer == false)
            {
                return;
            }

            InitializeVisuals();

            for (int i = 0; i < targetZoneRenderers.Length; i++)
            {
                Renderer renderer = targetZoneRenderers[i];
                if (renderer == null)
                {
                    continue;
                }

                renderer.enabled = visible;
                if (visible == false)
                {
                    continue;
                }

                Material[] materials = usePreviewMaterials ? _previewMaterials[i] : _originalMaterials[i];
                if (materials != null && materials.Length > 0)
                {
                    renderer.sharedMaterials = materials;
                }
            }
        }

        private static Material[] CreatePreviewMaterials(Material[] sourceMaterials)
        {
            if (sourceMaterials == null || sourceMaterials.Length == 0)
            {
                return null;
            }

            Material[] previewMaterials = new Material[sourceMaterials.Length];
            for (int i = 0; i < sourceMaterials.Length; i++)
            {
                Material sourceMaterial = sourceMaterials[i];
                if (sourceMaterial == null)
                {
                    continue;
                }

                Material previewMaterial = new Material(sourceMaterial);
                Color previewColor = Color.white;
                if (previewMaterial.HasProperty("_BaseColor"))
                {
                    previewColor = previewMaterial.GetColor("_BaseColor");
                }
                else if (previewMaterial.HasProperty("_Color"))
                {
                    previewColor = previewMaterial.GetColor("_Color");
                }

                previewColor.a = QuestHighlightUtility.DefaultHighlightAlpha;
                QuestHighlightUtility.SetMaterialColor(previewMaterial, previewColor);
                QuestHighlightUtility.ConfigureMaterialAsTransparent(previewMaterial);
                previewMaterials[i] = previewMaterial;
            }

            return previewMaterials;
        }
    }

    [Header("Quest")]
    [SerializeField] private NpcQuestRuntime _npcQuestRuntime;
    [SerializeField] private ProgressStatesApi _progressStatesApi;

    [Header("Targets")]
    [SerializeField] private PlacementTarget[] _targets;

    private const float SyncIntervalSeconds = 1.0f;

    private bool _isTaskActive;
    private bool _isTaskCompleted;
    private int _npcId;
    private float _nextSyncTime;
    private bool _syncInFlight;
    private bool[] _wasGrabbedLastFrame;
    private int _currentStep;
    private ColliderHighlightHandle[] _zoneHighlights;

    private void Awake()
    {
        AutoAssignTargets();

        int count = _targets != null ? _targets.Length : 0;
        _wasGrabbedLastFrame = new bool[count];
        _currentStep = 0;
        _zoneHighlights = new ColliderHighlightHandle[count];

        CacheZoneHighlights();
        InitializeTargetVisuals();
        UpdateHighlights();
    }

    private void Start()
    {
        _npcId = _npcQuestRuntime != null ? _npcQuestRuntime.GetNpcId() : 0;
        _nextSyncTime = Time.time + 0.15f;
    }

    private void OnValidate()
    {
        AutoAssignTargets();

        if (_zoneHighlights == null)
        {
            return;
        }

        for (int i = 0; i < _zoneHighlights.Length; i++)
        {
            if (_zoneHighlights[i] != null)
            {
                _zoneHighlights[i].SetColor(QuestHighlightUtility.DefaultHighlightColor, QuestHighlightUtility.DefaultHighlightAlpha);
            }
        }
    }

    private void OnDestroy()
    {
        if (_zoneHighlights == null)
        {
            return;
        }

        for (int i = 0; i < _zoneHighlights.Length; i++)
        {
            if (_zoneHighlights[i] != null)
            {
                _zoneHighlights[i].Dispose();
                _zoneHighlights[i] = null;
            }
        }

        if (_targets == null)
        {
            return;
        }

        for (int i = 0; i < _targets.Length; i++)
        {
            if (_targets[i] != null)
            {
                _targets[i].DisposeVisuals();
            }
        }
    }

    private void Update()
    {
        TryAutoActivateFromQuestState();
        if (_isTaskActive == false || _isTaskCompleted || _targets == null)
        {
            return;
        }

        if (_currentStep < 0 || _currentStep >= _targets.Length)
        {
            CompleteQuestTask();
            return;
        }

        PlacementTarget currentTarget = _targets[_currentStep];
        if (currentTarget == null || currentTarget.item == null)
        {
            _currentStep++;
            UpdateHighlights();
            return;
        }

        int stepIndex = _currentStep;
        bool isGrabbed = currentTarget.item.IsGrabbed;
        bool wasGrabbed = _wasGrabbedLastFrame[stepIndex];

        if (wasGrabbed && isGrabbed == false && IsInsideTargetZone(currentTarget))
        {
            currentTarget.item.SetHighlight(false);
            currentTarget.item.gameObject.SetActive(false);
            currentTarget.ApplyPlacedState();
            _currentStep++;
            if (_currentStep >= _targets.Length)
            {
                CompleteQuestTask();
                return;
            }
        }

        _wasGrabbedLastFrame[stepIndex] = isGrabbed;
        UpdateHighlights();
    }

    public void ActivateQuestTask()
    {
        if (_isTaskCompleted)
        {
            return;
        }

        _isTaskActive = true;
        UpdateHighlights();
    }

    public void CompleteQuestTask()
    {
        if (_isTaskCompleted)
        {
            return;
        }

        _isTaskCompleted = true;
        UpdateHighlights();

        if (_npcQuestRuntime != null)
        {
            _npcQuestRuntime.MarkQuestCompleted();
        }
    }

    private async void TryAutoActivateFromQuestState()
    {
        if (_progressStatesApi == null || _npcId <= 0 || _syncInFlight || Time.time < _nextSyncTime)
        {
            return;
        }

        _nextSyncTime = Time.time + SyncIntervalSeconds;
        _syncInFlight = true;
        try
        {
            ApiResult<ProgressStateDto[]> states = await _progressStatesApi.GetStatesAsync();
            if (states.IsSuccess && states.Data != null)
            {
                for (int i = 0; i < states.Data.Length; i++)
                {
                    ProgressStateDto state = states.Data[i];
                    if (state.npcId == _npcId &&
                        (state.status == ProgressStateDto.InProgress || state.status == ProgressStateDto.InProgressRepeat))
                    {
                        ActivateQuestTask();
                        break;
                    }
                }
            }
        }
        finally
        {
            _syncInFlight = false;
        }
    }

    private bool IsInsideTargetZone(PlacementTarget target)
    {
        if (target == null || target.item == null || target.item.ItemCollider == null || target.targetZoneTrigger == null)
        {
            return false;
        }

        return target.item.ItemCollider.bounds.Intersects(target.targetZoneTrigger.bounds);
    }

    private void CacheZoneHighlights()
    {
        if (_targets == null)
        {
            return;
        }

        for (int i = 0; i < _targets.Length; i++)
        {
            PlacementTarget target = _targets[i];
            if (target == null || target.targetZoneTrigger == null)
            {
                continue;
            }

            _zoneHighlights[i] = QuestHighlightUtility.CreateHighlight(target.targetZoneTrigger, QuestHighlightUtility.DefaultHighlightColor, QuestHighlightUtility.DefaultHighlightAlpha, true);
        }
    }

    private void InitializeTargetVisuals()
    {
        if (_targets == null)
        {
            return;
        }

        for (int i = 0; i < _targets.Length; i++)
        {
            if (_targets[i] != null)
            {
                _targets[i].InitializeVisuals();
            }
        }
    }

    private void AutoAssignTargets()
    {
        if (_targets == null)
        {
            return;
        }

        for (int i = 0; i < _targets.Length; i++)
        {
            if (_targets[i] != null)
            {
                _targets[i].AutoAssignZoneComponents();
            }
        }
    }

    private void SetZoneHighlight(int index, bool value)
    {
        if (_targets != null &&
            index >= 0 &&
            index < _targets.Length &&
            _targets[index] != null &&
            _targets[index].HasZoneVisualRenderer)
        {
            return;
        }

        if (_zoneHighlights == null || index < 0 || index >= _zoneHighlights.Length)
        {
            return;
        }

        if (_zoneHighlights[index] == null)
        {
            return;
        }

        _zoneHighlights[index].SetColor(QuestHighlightUtility.DefaultHighlightColor, QuestHighlightUtility.DefaultHighlightAlpha);
        _zoneHighlights[index].SetVisible(value);
    }

    private void UpdateHighlights()
    {
        if (_targets == null)
        {
            return;
        }

        for (int i = 0; i < _targets.Length; i++)
        {
            PlacementTarget target = _targets[i];
            if (target == null || target.item == null)
            {
                continue;
            }

            bool isCurrent = _isTaskActive &&
                             _isTaskCompleted == false &&
                             i == _currentStep;

            if (target.IsPlaced)
            {
                target.ApplyPlacedState();
            }
            else if (isCurrent && target.item.IsGrabbed)
            {
                target.SetPreviewState();
            }
            else
            {
                target.SetWaitingState();
            }

            bool showItem = isCurrent && target.item.IsGrabbed == false;
            target.item.SetHighlight(showItem);

            bool showZone = isCurrent &&
                            target.item.IsGrabbed &&
                            target.HasZoneVisualRenderer == false;
            SetZoneHighlight(i, showZone);
        }
    }
}
