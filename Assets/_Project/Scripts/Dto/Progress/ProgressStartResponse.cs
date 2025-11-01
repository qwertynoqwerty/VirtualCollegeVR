using System;

[Serializable]
public sealed class ProgressStartResponse
{
    public int npcId;
    public bool isActive;
    public string startDialogue;
    public string startAudio;
}