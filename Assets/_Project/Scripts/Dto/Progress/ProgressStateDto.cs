using System;

namespace VRCollege.Net.Dto
{
    [Serializable]
    public sealed class ProgressStateDto
    {
        public int npcId;
        public string state;
        public string completedAt;
    }
}