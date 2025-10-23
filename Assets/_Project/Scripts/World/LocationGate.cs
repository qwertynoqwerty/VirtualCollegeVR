using UnityEngine;

public sealed class LocationGate : MonoBehaviour
{
    [SerializeField] private GameObject[] _zones;
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
        if (_zones == null)
        {
            return;
        }

        for (int i = 0; i < _zones.Length; i++)
        {
            GameObject z = _zones[i];
            if (z == null)
            {
                continue;
            }

            if (isGuest == true)
            {
                z.SetActive(true);
            }
            else
            {
                if ((i + 1) == specialtyId)
                {
                    z.SetActive(true);
                }
                else
                {
                    z.SetActive(false);
                }
            }
        }
    }
}