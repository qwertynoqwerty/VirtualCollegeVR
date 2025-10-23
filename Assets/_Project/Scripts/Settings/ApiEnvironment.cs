using UnityEngine;

[CreateAssetMenu(menuName = "VRCollege/Api Environment")]
public sealed class ApiEnvironment : ScriptableObject
{
    [SerializeField] private string _baseUrl = "http://localhost:5186";
    public string BaseUrl { get { return _baseUrl; } }
}