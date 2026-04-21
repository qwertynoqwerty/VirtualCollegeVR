using System;

namespace VRCollege.Net.Dto
{
    [Serializable]
    public sealed class ProgressStateDto
    {
        public const int NotStarted = 0;
        public const int InProgress = 1;
        public const int InProgressRepeat = 2;
        public const int CompletedPendingDialogue = 3;
        public const int CompletedRepeat = 4;

        public int npcId;
        public string npcName;
        public int status;
    }
}
