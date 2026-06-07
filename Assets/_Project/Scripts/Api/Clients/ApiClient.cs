using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public sealed class ApiClient : MonoBehaviour
{
    [SerializeField] private ApiEnvironment _env;
    private int _timeoutSeconds = 15;
    private bool _autoClearTokenOn401 = true;

    private void Awake()
    {
        if (_env == null)
        {
            Debug.LogError("ApiEnvironment is not assigned on ApiClient.");
        }
    }

    public async Task<ApiResult<T>> GetAsync<T>(string path, CancellationToken ct = default)
    {
        if (_env == null)
        {
            return ApiResult<T>.Fail(0, "ApiEnvironment is not assigned");
        }

        using (UnityWebRequest req = UnityWebRequest.Get(BuildUrl(path)))
        {
            AttachAuth(req);
            AttachEditorCertificateHandler(req);
            req.downloadHandler = new DownloadHandlerBuffer();
            if (_timeoutSeconds > 0)
            {
                req.timeout = _timeoutSeconds;
            }

            UnityWebRequestAsyncOperation op = req.SendWebRequest();
            while (op.isDone == false)
            {
                if (ct.IsCancellationRequested)
                {
                    req.Abort();
                    return ApiResult<T>.Canceled();
                }

                await Task.Yield();
            }

            return HandleResponse<T>(req);
        }
    }

    public async Task<ApiResult<TResponse>> PostJsonAsync<TRequest, TResponse>(string path, TRequest body, CancellationToken ct = default)
    {
        if (_env == null)
        {
            return ApiResult<TResponse>.Fail(0, "ApiEnvironment is not assigned");
        }

        string json = JsonUtility.ToJson(body);
        byte[] payload = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest req = new UnityWebRequest(BuildUrl(path), "POST"))
        {
            AttachAuth(req);
            AttachEditorCertificateHandler(req);
            req.uploadHandler = new UploadHandlerRaw(payload);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            if (_timeoutSeconds > 0)
            {
                req.timeout = _timeoutSeconds;
            }

            UnityWebRequestAsyncOperation op = req.SendWebRequest();
            while (op.isDone == false)
            {
                if (ct.IsCancellationRequested)
                {
                    req.Abort();
                    return ApiResult<TResponse>.Canceled();
                }

                await Task.Yield();
            }

            return HandleResponse<TResponse>(req);
        }
    }

    private string BuildUrl(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return _env.BaseUrl;
        }

        bool baseEndsWith = _env.BaseUrl.EndsWith("/");
        bool pathStartsWith = path.StartsWith("/");

        if (_env.BaseUrl.StartsWith("http") == false)
        {
            Debug.LogWarning("BaseUrl in ApiEnvironment should start with http/https.");
        }

        if (path.StartsWith("http"))
        {
            return path;
        }

        if (baseEndsWith && pathStartsWith)
        {
            return _env.BaseUrl + path.Substring(1);
        }

        if (baseEndsWith == false && pathStartsWith == false)
        {
            return _env.BaseUrl + "/" + path;
        }

        return _env.BaseUrl + path;
    }

    private void AttachAuth(UnityWebRequest req)
    {
        string token = JwtStorage.Load();
        if (string.IsNullOrEmpty(token) == false)
        {
            req.SetRequestHeader("Authorization", "Bearer " + token);
        }
    }

    private void AttachEditorCertificateHandler(UnityWebRequest req)
    {
#if UNITY_EDITOR
        if (_env != null && _env.AllowInsecureEditorCertificate)
        {
            req.certificateHandler = new EditorOnlyAcceptAllCertificateHandler();
        }
#endif
    }

    private ApiResult<T> HandleResponse<T>(UnityWebRequest req)
    {
        long code = req.responseCode;

        bool hasError = false;
        if (req.result == UnityWebRequest.Result.ConnectionError)
        {
            hasError = true;
        }
        else
        {
            if (req.result == UnityWebRequest.Result.ProtocolError)
            {
                hasError = true;
            }
            else
            {
                if (req.result == UnityWebRequest.Result.DataProcessingError)
                {
                    hasError = true;
                }
            }
        }

        if (code == 401)
        {
            if (_autoClearTokenOn401 == true)
            {
                JwtStorage.Clear();
            }
        }

        if (hasError == true)
        {
            string body;
            if (req.downloadHandler != null)
            {
                body = req.downloadHandler.text;
            }
            else
            {
                body = req.error;
            }
            return ApiResult<T>.Fail(code, body);
        }

        string text;
        if (req.downloadHandler != null)
        {
            text = req.downloadHandler.text;
        }
        else
        {
            text = "";
        }

        if (string.IsNullOrEmpty(text) == true)
        {
            return ApiResult<T>.Success(default(T));
        }

        if (typeof(T) == typeof(string))
        {
            object boxed = text;
            return ApiResult<T>.Success((T)boxed);
        }

        try
        {
            T data = JsonUtility.FromJson<T>(text);
            return ApiResult<T>.Success(data);
        }
        catch (Exception)
        {
            string err = "Invalid JSON: " + text;
            return ApiResult<T>.Fail(code, err);
        }
    }
}

#if UNITY_EDITOR
internal sealed class EditorOnlyAcceptAllCertificateHandler : CertificateHandler
{
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        return true;
    }
}
#endif

public struct ApiResult<T>
{
    public bool IsSuccess;
    public bool IsCanceled;
    public long StatusCode;
    public T Data;
    public string Error;

    public static ApiResult<T> Success(T data)
    {
        ApiResult<T> r = new ApiResult<T>();
        r.IsSuccess = true;
        r.IsCanceled = false;
        r.Data = data;
        r.StatusCode = 200;
        r.Error = null;
        return r;
    }

    public static ApiResult<T> Fail(long status, string error)
    {
        ApiResult<T> r = new ApiResult<T>();
        r.IsSuccess = false;
        r.IsCanceled = false;
        r.Data = default(T);
        r.StatusCode = status;

        if (string.IsNullOrEmpty(error) == false)
        {
            r.Error = error;
        }
        else
        {
            r.Error = "Request failed";
        }

        return r;
    }

    public static ApiResult<T> Canceled()
    {
        ApiResult<T> r = new ApiResult<T>();
        r.IsSuccess = false;
        r.IsCanceled = true;
        r.Data = default(T);
        r.StatusCode = 0;
        r.Error = "Canceled";
        return r;
    }
}
