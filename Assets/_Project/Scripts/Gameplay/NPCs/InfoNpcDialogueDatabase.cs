using UnityEngine;

[CreateAssetMenu(menuName = "VRCollege/Info NPC Dialogue Database")]
public sealed class InfoNpcDialogueDatabase : ScriptableObject
{
    [System.Serializable]
    private sealed class Entry
    {
        public int infoId;
        public string npcName;
        public DialogueLine[] lines;
    }

    [SerializeField] private Entry[] _entries;

    public DialogueLine[] GetLines(int infoId)
    {
        Entry entry = GetEntry(infoId);
        return entry != null ? entry.lines : null;
    }

    public string GetNpcName(int infoId)
    {
        Entry entry = GetEntry(infoId);
        return entry != null ? entry.npcName : null;
    }

    private Entry GetEntry(int infoId)
    {
        if (_entries == null)
        {
            return null;
        }

        for (int i = 0; i < _entries.Length; i++)
        {
            Entry e = _entries[i];
            if (e == null || e.infoId != infoId)
            {
                continue;
            }

            return e;
        }

        return null;
    }
}
