using UnityEngine;

[CreateAssetMenu(menuName = "VRCollege/Api Environment")]
public sealed class ApiEnvironment : ScriptableObject
{
    [SerializeField] private string _baseUrl = "https://159.194.227.65.sslip.io";
    [SerializeField] private bool _allowInsecureEditorCertificate = true;

    public string BaseUrl { get { return _baseUrl; } }
    public bool AllowInsecureEditorCertificate { get { return _allowInsecureEditorCertificate; } }
}
