using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

public static class StreamingAudioLoader
{
    private static readonly Dictionary<string, AudioClip> _cache = new Dictionary<string, AudioClip>();
    
    public static async Task<AudioClip> LoadAsync(string dbPath, AudioType type = AudioType.OGGVORBIS)
    {
        string url = NormalizeToStreamingAssetsUrl(dbPath);
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogWarning("[StreamingAudioLoader] Empty DB path");
            return null;
        }

        string key = ExtractRelativeKey(dbPath);
        if (_cache.TryGetValue(key, out var cached))
            return cached;

        var resolvedType = ResolveType(dbPath);

        using (var req = UnityWebRequestMultimedia.GetAudioClip(url, resolvedType))
        {
            var op = req.SendWebRequest();
            while (!op.isDone) await Task.Yield();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("[StreamingAudioLoader] Load failed: " + url + " | " + req.error);
                return null;
            }

            var clip = DownloadHandlerAudioClip.GetContent(req);
            if (clip == null)
            {
                Debug.LogWarning("[StreamingAudioLoader] Null clip: " + url);
                return null;
            }

            _cache[key] = clip;
            return clip;
        }
    }

    public static void Unload(string dbPath)
    {
        string key = ExtractRelativeKey(dbPath);
        if (_cache.TryGetValue(key, out var clip))
        {
            _cache.Remove(key);
            if (clip != null) Object.Destroy(clip);
        }
    }

    public static void ClearCache()
    {
        foreach (var kv in _cache)
        {
            if (kv.Value != null) Object.Destroy(kv.Value);
        }
        _cache.Clear();
    }

    private static string NormalizeToStreamingAssetsUrl(string dbPath)
    {
        if (string.IsNullOrEmpty(dbPath)) return null;

        string p = dbPath.Replace('\\','/').Trim();

        int idx = p.IndexOf("StreamingAssets/", StringComparison.OrdinalIgnoreCase);
        if (idx >= 0) p = p.Substring(idx + "StreamingAssets/".Length);
        while (p.StartsWith("/")) p = p.Substring(1);

        string basePath = Application.streamingAssetsPath.Replace('\\','/');

        if (basePath.StartsWith("jar:", StringComparison.OrdinalIgnoreCase))
            return basePath.TrimEnd('/') + "/" + p;

        string abs = basePath.EndsWith("/") ? basePath + p : basePath + "/" + p;
        if (!abs.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
            abs = "file://" + abs;

        return abs;
    }


    private static string ExtractRelativeKey(string dbPath)
    {
        if (string.IsNullOrEmpty(dbPath))
        {
            return "";
        }

        string p = dbPath.Replace('\\', '/').Trim();

        int idx = p.IndexOf("StreamingAssets/", StringComparison.OrdinalIgnoreCase);
        if (idx >= 0)
        {
            p = p.Substring(idx + "StreamingAssets/".Length);
        }

        while (p.StartsWith("/")) p = p.Substring(1);
        return p.ToLowerInvariant();    
    }
    
    private static AudioType ResolveType(string path)
    {
        path = path.ToLowerInvariant();
        if (path.EndsWith(".wav")) return AudioType.WAV;
        if (path.EndsWith(".mp3")) return AudioType.MPEG;
        return AudioType.OGGVORBIS;
    }
}
