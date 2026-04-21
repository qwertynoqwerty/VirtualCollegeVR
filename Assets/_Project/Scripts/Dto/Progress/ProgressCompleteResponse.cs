using System;
using VRCollege.Net.Dto;

[Serializable]
public sealed class ProgressCompleteResponse
{
    public int npcId;
    public bool isActive;
    public string completedAt;
    public AchievementDto[] newAchievements;
}
