using System.Threading.Tasks;
using UnityEngine;
using VRCollege.Net;
using VRCollege.Net.Dto;

public sealed class OnboardingFlowController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ProfileApi _profileApi;
    [SerializeField] private GlobalToast _toast;

    [Header("NPC")]
    [SerializeField] private OnboardingNpcController _curatorNpc;
    [SerializeField] private OnboardingNpcController _gymNpc;
    [SerializeField] private OnboardingNpcController _studyOfficeNpc;

    private string _completedToast = "Обучение пройдено!";
    private float _toastVisibleSeconds = 2.5f;

    private OnboardingStep _currentStep = OnboardingStep.Curator;
    private bool _isFinalCuratorStep;
    private bool _isCompleted;
    private bool _isCompleting;

    public bool IsCompleted => _isCompleted;
    public OnboardingStep CurrentStep => _currentStep;
    public bool IsFinalCuratorStep => _isFinalCuratorStep;

    private void Awake()
    {
        if (_profileApi == null)
        {
            _profileApi = FindFirstObjectByType<ProfileApi>(FindObjectsInactive.Include);
        }

        if (_toast == null)
        {
            _toast = FindFirstObjectByType<GlobalToast>(FindObjectsInactive.Include);
        }

        InitializeNpc(_curatorNpc);
        InitializeNpc(_gymNpc);
        InitializeNpc(_studyOfficeNpc);
    }

    private void Start()
    {
        _isCompleted = UserSession.LoadTutorialCompleted();
        ApplyNpcAvailability();
    }

    public async Task CompleteStepAsync(OnboardingNpcController npc)
    {
        if (_isCompleted || _isCompleting || npc == null || IsExpectedNpc(npc) == false)
        {
            return;
        }

        if (_isFinalCuratorStep)
        {
            await CompleteOnboardingAsync();
            return;
        }

        if (_currentStep == OnboardingStep.StudyOffice)
        {
            _isFinalCuratorStep = true;
        }
        else
        {
            _currentStep = (OnboardingStep)((int)_currentStep + 1);
        }

        ApplyNpcAvailability();
    }

    private async Task CompleteOnboardingAsync()
    {
        _isCompleting = true;

        bool completedOnServer = false;
        if (_profileApi != null)
        {
            ApiResult<TutorialCompleteResultDto> res = await _profileApi.CompleteTutorialAsync();
            completedOnServer = res.IsSuccess && res.Data != null && res.Data.tutorialCompleted;
        }

        if (completedOnServer == false)
        {
            Debug.LogWarning("[OnboardingFlowController] CompleteTutorialAsync failed. Tutorial remains incomplete.");
            _isCompleting = false;
            ApplyNpcAvailability();
            return;
        }

        _isCompleted = true;
        UserSession.SaveTutorialCompleted(true);
        ApplyNpcAvailability();

        if (_toast != null && string.IsNullOrEmpty(_completedToast) == false)
        {
            _toast.Show(_completedToast, 0f, _toastVisibleSeconds);
        }

        _isCompleting = false;
    }

    private void ApplyNpcAvailability()
    {
        if (_isCompleted)
        {
            SetAvailable(_curatorNpc, false);
            SetAvailable(_gymNpc, false);
            SetAvailable(_studyOfficeNpc, false);
            return;
        }

        SetAvailable(_curatorNpc, _currentStep == OnboardingStep.Curator || _isFinalCuratorStep);
        SetAvailable(_gymNpc, _currentStep == OnboardingStep.Gym);
        SetAvailable(_studyOfficeNpc, _currentStep == OnboardingStep.StudyOffice && _isFinalCuratorStep == false);
    }

    private bool IsExpectedNpc(OnboardingNpcController npc)
    {
        if (_isFinalCuratorStep)
        {
            return npc == _curatorNpc;
        }

        return npc.Step == _currentStep;
    }

    private void InitializeNpc(OnboardingNpcController npc)
    {
        if (npc != null)
        {
            npc.Initialize(this);
        }
    }

    private static void SetAvailable(OnboardingNpcController npc, bool value)
    {
        if (npc != null)
        {
            npc.SetAvailable(value);
        }
    }
}
