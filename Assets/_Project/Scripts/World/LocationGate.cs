using UnityEngine;
using UnityEngine.Serialization;

public sealed class LocationGate : MonoBehaviour
{
    [FormerlySerializedAs("_zones")]
    [SerializeField] private GameObject[] _zoneBlockers;
    [SerializeField] private bool _autoApplyOnStart = true;

    private void Start()
    {
        if (_autoApplyOnStart == true)
        {
            bool isGuest = !JwtStorage.HasToken();
            int spec = UserSession.LoadSpecialtyId();
            Apply(spec, isGuest);
        }
    }

    public void Apply(int specialtyId, bool isGuest)
    {
        if (_zoneBlockers == null)
        {
            return;
        }

        for (int i = 0; i < _zoneBlockers.Length; i++)
        {
            GameObject blocker = _zoneBlockers[i];
            if (blocker == null)
            {
                continue;
            }

            if (isGuest == true)
            {
                blocker.SetActive(true);
            }
            else
            {
                bool isCurrentSpecialty = specialtyId > 0 && (i + 1) == specialtyId;
                blocker.SetActive(isCurrentSpecialty == false);
            }
        }
    }
}
