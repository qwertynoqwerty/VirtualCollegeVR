using System;
using UnityEngine;

[Serializable]
public sealed class DialogueLine
{
    [TextArea(2, 8)]
    public string text;
    public AudioClip audioClip;
}
