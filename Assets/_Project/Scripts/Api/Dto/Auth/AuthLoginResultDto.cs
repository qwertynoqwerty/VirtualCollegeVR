using System;

namespace VRCollege.Net.Dto
{
    [Serializable]
    public sealed class AuthLoginResultDto
    {
        public string token;
        public string sid;
        public string spec;

        public int userId;
        public int specialtyId;
        public int logId;
        public bool tutorialCompleted;
    }
}
