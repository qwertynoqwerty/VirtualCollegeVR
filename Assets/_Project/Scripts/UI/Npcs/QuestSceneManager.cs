using UnityEngine;

public class QuestSceneManager : MonoBehaviour
{
    [SerializeField] private int _requiredSpecialtyId = 1;
    [SerializeField] private GameObject[] _restrictObjects;

    private void Start()
    {
        int userSpec = UserSession.LoadSpecialtyId();
        bool allow = false;

        if (userSpec == _requiredSpecialtyId)
        {
            allow = true;
        }

        int i = 0;
        while (i < _restrictObjects.Length)
        {
            GameObject go = _restrictObjects[i];
            if (go != null)
            {
                if (allow == true)
                {
                    go.SetActive(true);
                }
                else
                {
                    go.SetActive(false);
                }
            }
            i = i + 1;
        }
    }
}