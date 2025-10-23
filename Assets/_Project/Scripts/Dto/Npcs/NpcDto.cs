using System;

[Serializable]
public class NpcDto
{
    public int id;
    public string name;
    public int specialtyId;

    public string startDialogue;
    public string completeDialogue;
    public string startAudio;
    public string completeAudio;
}
