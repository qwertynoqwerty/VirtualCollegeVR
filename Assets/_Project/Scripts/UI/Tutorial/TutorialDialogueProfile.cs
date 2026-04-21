using UnityEngine;

[CreateAssetMenu(menuName = "VRCollege/Tutorial Dialogue Profile")]
public sealed class TutorialDialogueProfile : ScriptableObject
{
    [SerializeField] private DialogueLine[] _lines;

    public int Count
    {
        get
        {
            if (_lines == null)
            {
                return 0;
            }

            return _lines.Length;
        }
    }

    public DialogueLine GetLine(int index)
    {
        if (_lines == null || index < 0 || index >= _lines.Length)
        {
            return null;
        }

        return _lines[index];
    }
}
