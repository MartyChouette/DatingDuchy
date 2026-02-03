using UnityEngine;

namespace CozyTown.Core
{
    [System.Serializable]
    public struct GameEvent
    {
        // Agent kind constants — use these instead of raw strings
        public const string KindPeasant = "Peasant";
        public const string KindHero = "Hero";
        public const string KindMonster = "Monster";
        public const string KindTaxCollector = "TaxCollector";

        public float time;
        public GameEventType type;

        public int aId;
        public int bId;

        public int amount;
        public Vector3 world;
        public string text;

        public static GameEvent Make(GameEventType type, int amount = 0, int aId = 0, int bId = 0, Vector3 world = default, string text = "")
        {
            return new GameEvent
            {
                time = Time.time,
                type = type,
                amount = amount,
                aId = aId,
                bId = bId,
                world = world,
                text = text
            };
        }

        public override string ToString()
        {
            var t = $"{time:0.0}s {type}";
            if (amount != 0) t += $" amt={amount}";
            if (aId != 0) t += $" a={aId}";
            if (bId != 0) t += $" b={bId}";
            if (!string.IsNullOrEmpty(text)) t += $" \"{text}\"";
            return t;
        }
    }
}
