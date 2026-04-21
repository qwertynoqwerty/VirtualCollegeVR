using UnityEngine;

[CreateAssetMenu(menuName = "VRCollege/Info NPC Dialogue Database")]
public sealed class InfoNpcDialogueDatabase : ScriptableObject
{
    [System.Serializable]
    private sealed class Entry
    {
        public int infoId;
        public DialogueLine[] lines;
    }

    [SerializeField] private Entry[] _entries;

    public DialogueLine[] GetLines(int infoId)
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

            return e.lines;
        }

        return null;
    }
}
