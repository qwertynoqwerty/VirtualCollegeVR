using UnityEngine;

[CreateAssetMenu(menuName = "VRCollege/Quest NPC Dialogue Database")]
public sealed class QuestNpcDialogueDatabase : ScriptableObject
{
    [System.Serializable]
    private sealed class Entry
    {
        public int npcId;
        public DialogueLine[] startQuestLines;
        public DialogueLine[] inProgressRepeatLines;
        public DialogueLine[] completeQuestLines;
        public DialogueLine[] completedRepeatLines;
    }

    [SerializeField] private Entry[] _entries;

    public DialogueLine[] GetStartQuestLines(int npcId) => GetLines(npcId, 0);
    public DialogueLine[] GetInProgressRepeatLines(int npcId) => GetLines(npcId, 1);
    public DialogueLine[] GetCompleteQuestLines(int npcId) => GetLines(npcId, 2);
    public DialogueLine[] GetCompletedRepeatLines(int npcId) => GetLines(npcId, 3);

    private DialogueLine[] GetLines(int npcId, int kind)
    {
        if (_entries == null)
        {
            return null;
        }

        for (int i = 0; i < _entries.Length; i++)
        {
            Entry e = _entries[i];
            if (e == null || e.npcId != npcId)
            {
                continue;
            }

            switch (kind)
            {
                case 0: return e.startQuestLines;
                case 1: return e.inProgressRepeatLines;
                case 2: return e.completeQuestLines;
                case 3: return e.completedRepeatLines;
                default: return null;
            }
        }

        return null;
    }
}
