namespace CozyTown.Core
{
    public enum GameEventType
    {
        BuildingPlaced,
        BuildingRegistered,

        PersonSpawned,
        PersonDied,

        GoldAdded,
        GoldSpent,
        TaxCollected,
        BountyPosted,
        BountyAccepted,
        BountyCompleted,
        BountyFailed,

        MonsterSpawned,
        MonsterKilled,

        RelationshipDelta,  // hook
        RomanceMilestone,   // crush / dating / lovers / heartbreak
        Note                // free-form debug
    }
}
