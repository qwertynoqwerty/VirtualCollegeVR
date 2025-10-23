using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class StreamingAudioLoader
{
    public static async Task<AudioClip> LoadClipAsync(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
        {
            return null;
        }

        string trimmed = relativePath.TrimStart('/','\\');
        string full = Path.Combine(Application.streamingAssetsPath, trimmed);

        AudioType type = DetectType(trimmed);

        using (UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(full, type))
        {
            var op = req.SendWebRequest();
            while (!op.isDone)
            {
                await Task.Yield();
            }

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Audio load failed: " + full + " / " + req.error);
                return null;
            }

            AudioClip clip = DownloadHandlerAudioClip.GetContent(req);
            return clip;
        }
    }

    private static AudioType DetectType(string path)
    {
        string lower = path.ToLowerInvariant();
        if (lower.EndsWith(".ogg"))
        {
            return AudioType.OGGVORBIS;
        }

        if (lower.EndsWith(".wav"))
        {
            return AudioType.WAV;
        }

        if (lower.EndsWith(".mp3"))
        {
            return AudioType.MPEG;
        }

        return AudioType.UNKNOWN;
    }
}