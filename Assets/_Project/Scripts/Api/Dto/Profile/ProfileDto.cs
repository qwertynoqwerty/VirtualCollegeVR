using System;

namespace VRCollege.Net.Dto
{
    [Serializable]
    public sealed class ProfileDto
    {
        public int userId;
        public int specialtyId;
        public int questsCompleted;
        public int loginStreak;
        public AchievementDto[] achievements;

        public int daysSinceLastLogin;
        public int totalHoursPlayed;
        public bool tutorialCompleted;
    }
}
