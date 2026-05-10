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
        public DialogueLine[] wrongSpecialtyLines;
        public DialogueLine[] tutorialRequiredLines;
    }

    [HideInInspector] [SerializeField] private DialogueLine[] _defaultWrongSpecialtyLines;
    [HideInInspector] [SerializeField] private DialogueLine[] _defaultTutorialRequiredLines;
    [SerializeField] private Entry[] _entries;

    public DialogueLine[] GetStartQuestLines(int npcId) => GetLines(npcId, 0);
    public DialogueLine[] GetInProgressRepeatLines(int npcId) => GetLines(npcId, 1);
    public DialogueLine[] GetCompleteQuestLines(int npcId) => GetLines(npcId, 2);
    public DialogueLine[] GetCompletedRepeatLines(int npcId) => GetLines(npcId, 3);
    public DialogueLine[] GetWrongSpecialtyLines(int npcId)
    {
        DialogueLine[] lines = GetLines(npcId, 4);
        if (lines != null && lines.Length > 0)
        {
            return lines;
        }

        return _defaultWrongSpecialtyLines;
    }

    public DialogueLine[] GetTutorialRequiredLines(int npcId)
    {
        DialogueLine[] lines = GetLines(npcId, 5);
        if (lines != null && lines.Length > 0)
        {
            return lines;
        }

        return _defaultTutorialRequiredLines;
    }

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
                case 4: return e.wrongSpecialtyLines;
                case 5: return e.tutorialRequiredLines;
                default: return null;
            }
        }

        return null;
    }
}
