using UnityEngine;
using VRCollege.Net;
using VRCollege.Net.Dto;

public sealed class IngredientPotSequenceQuestTask : MonoBehaviour
{
    [Header("Quest")]
    [SerializeField] private NpcQuestRuntime _npcQuestRuntime;
    [SerializeField] private ProgressStatesApi _progressStatesApi;

    [Header("Pot")]
    [SerializeField] private Collider _potTrigger;

    [Header("Ingredients")]
    [SerializeField] private CulinaryQuestItem[] _requiredIngredients;

    private const float SyncIntervalSeconds = 1.0f;

    private bool _isTaskActive;
    private bool _isTaskCompleted;
    private int _npcId;
    private float _nextSyncTime;
    private bool _syncInFlight;
    private bool[] _wasGrabbedLastFrame;
    private bool[] _isAdded;
    private int _addedCount;
    private ColliderHighlightHandle _potHighlight;

    private void Awake()
    {
        int count = _requiredIngredients != null ? _requiredIngredients.Length : 0;
        _wasGrabbedLastFrame = new bool[count];
        _isAdded = new bool[count];
        _potHighlight = QuestHighlightUtility.CreateHighlight(
            _potTrigger,
            QuestHighlightUtility.DefaultHighlightColor,
            QuestHighlightUtility.DefaultHighlightAlpha,
            true);
        UpdateHighlights();
    }

    private void Start()
    {
        _npcId = _npcQuestRuntime != null ? _npcQuestRuntime.GetNpcId() : 0;
        _nextSyncTime = Time.time + 0.15f;
    }

    private void OnValidate()
    {
        if (_potHighlight != null)
        {
            _potHighlight.SetColor(QuestHighlightUtility.DefaultHighlightColor, QuestHighlightUtility.DefaultHighlightAlpha);
        }
    }

    private void OnDestroy()
    {
        if (_potHighlight != null)
        {
            _potHighlight.Dispose();
            _potHighlight = null;
        }
    }

    private void Update()
    {
        TryAutoActivateFromQuestState();

        if (_isTaskActive == false || _isTaskCompleted || _requiredIngredients == null)
        {
            return;
        }

        for (int i = 0; i < _requiredIngredients.Length; i++)
        {
            if (_isAdded[i])
            {
                continue;
            }

            CulinaryQuestItem item = _requiredIngredients[i];
            if (item == null)
            {
                continue;
            }

            bool isGrabbed = item.IsGrabbed;
            bool wasGrabbed = _wasGrabbedLastFrame[i];

            if (wasGrabbed && isGrabbed == false && IsInsidePot(item))
            {
                _isAdded[i] = true;
                _addedCount++;
                item.SetHighlight(false);
            }

            _wasGrabbedLastFrame[i] = isGrabbed;
        }

        if (_requiredIngredients.Length > 0 && _addedCount >= _requiredIngredients.Length)
        {
            CompleteQuestTask();
            return;
        }

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

    private bool IsInsidePot(CulinaryQuestItem item)
    {
        if (item == null || item.ItemCollider == null || _potTrigger == null)
        {
            return false;
        }

        return item.ItemCollider.bounds.Intersects(_potTrigger.bounds);
    }

    private void UpdateHighlights()
    {
        bool showPot = false;

        if (_requiredIngredients == null)
        {
            if (_potHighlight != null)
            {
                _potHighlight.SetVisible(false);
            }

            return;
        }

        for (int i = 0; i < _requiredIngredients.Length; i++)
        {
            CulinaryQuestItem item = _requiredIngredients[i];
            if (item == null)
            {
                continue;
            }

            bool isPending = _isAdded != null && i < _isAdded.Length && _isAdded[i] == false;
            bool showItem = _isTaskActive && _isTaskCompleted == false && isPending && item.IsGrabbed == false;
            item.SetHighlight(showItem);

            if (_isTaskActive && _isTaskCompleted == false && isPending && item.IsGrabbed)
            {
                showPot = true;
            }
        }

        if (_potHighlight != null)
        {
            _potHighlight.SetColor(QuestHighlightUtility.DefaultHighlightColor, QuestHighlightUtility.DefaultHighlightAlpha);
            _potHighlight.SetVisible(showPot);
        }
    }
}
