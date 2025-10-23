using System;

namespace VRCollege.Net.Dto
{
    [Serializable]
    public sealed class FailIntervalResponseDto
    {
        public int windowMinutes;
        public int count;
        public string firstTsUtc;
        public string lastTsUtc;
        public float avgIntervalMinutes;
    }
}