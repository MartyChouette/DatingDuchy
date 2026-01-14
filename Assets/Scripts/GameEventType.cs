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
        Note                // free-form debug
    }
}
