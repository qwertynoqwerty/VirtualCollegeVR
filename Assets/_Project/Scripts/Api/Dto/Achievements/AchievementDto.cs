using System;

namespace VRCollege.Net.Dto
{
    [Serializable]
    public sealed class AchievementDto
    {
        public int achievementId;
        public string code;
        public string name;
        public string title;

        public bool unlocked;
        public string awardDate;
    }
}