using CozyTown.Core;
using CozyTown.UI;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CozyTown.Sim
{
    public enum RelCharge { Neutral, Alpha, Zeta }

    public class RelationshipSystem : MonoBehaviour
    {
        public static RelationshipSystem Instance { get; private set; }

        [Header("Growth Rates (slow sim)")]
        public float affinityPerSecondOfContact = 0.12f;
        public float irritationPerSecondBadLoop = 0.08f;

        [Header("Momentum / Charge")]
        [Tooltip("How quickly trend adapts. Lower = slower, more 'sim'.")]
        public float trendLerp = 0.04f;
        public float alphaThreshold = 0.22f;
        public float zetaThreshold = -0.22f;

        [Header("Milestones (tune later)")]
        public float acquaintanceAffinity = 10f;
        public float friendAffinity = 30f;
        public float romanceAffinityGate = 40f;
        public float soulmateAffinity = 85f;
        public float nemesisIrritation = 80f;

        [Header("Romance Gates")]
        public float crushAffinity = 40f;
        public float crushAttraction = 25f;
        public float datingAffinity = 55f;
        public float datingAttraction = 40f;
        public float datingTrust = 15f;
        public float loversAffinity = 70f;
        public float loversAttraction = 55f;
        public float loversTrust = 30f;

        [Header("Romance Growth")]
        public float attractionGrowthRate = 0.06f;
        public float trustGrowthRate = 0.04f;
        public float trustSkinTimeGate = 30f;
        public float heartbreakHysteresis = 10f;

        readonly Dictionary<ulong, RelState> _rels = new(8192);
        readonly List<string> _recentHighlights = new(256);
        readonly Dictionary<int, HashSet<int>> _romances = new(256);

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void OnEnable() => GameEventBus.OnEvent += OnGameEvent;
        void OnDisable() => GameEventBus.OnEvent -= OnGameEvent;

        void OnGameEvent(GameEvent e)
        {
            if (e.type == GameEventType.PersonDied)
                CleanUpRomancesForDeceased(e.aId);
        }

        public void RegisterContact(AgentBase a, AgentBase b, Vector3 contactPoint, float dt)
        {
            if (a?.pid == null || b?.pid == null) return;

            int ida = a.pid.id;
            int idb = b.pid.id;

            var rel = GetOrCreate(ida, idb);

            rel.skinTime += dt;
            rel.lastContactPoint = contactPoint;
            rel.lastContactAtGameTime01 = GameTime.Instance != null ? GameTime.Instance.dayTime01 : 0f;

            // Compute chemistry delta (trait-influenced)
            float delta = ComputeAffinityDelta(a, b, rel, dt);
            rel.affinity = Mathf.Clamp(rel.affinity + delta, -100f, 100f);

            // Trend = EMA of delta sign/magnitude
            float signed = Mathf.Clamp(delta, -1f, 1f);
            rel.trend = Mathf.Lerp(rel.trend, signed, trendLerp);

            rel.charge = (rel.trend >= alphaThreshold) ? RelCharge.Alpha :
                         (rel.trend <= zetaThreshold) ? RelCharge.Zeta :
                         RelCharge.Neutral;

            // Irritation grows during negative-trend contact (bad loop), decays slowly otherwise
            if (rel.trend < 0f)
                rel.irritation = Mathf.Clamp(rel.irritation + irritationPerSecondBadLoop * dt, 0f, 100f);
            else if (rel.irritation > 0f)
                rel.irritation = Mathf.Max(0f, rel.irritation - irritationPerSecondBadLoop * 0.5f * dt);

            // Attraction & trust (romance-eligible pairs only)
            bool eligible = IsRomanceEligible(a) && IsRomanceEligible(b);
            if (eligible)
            {
                ComputeAttraction(a, b, ref rel, dt);
                ComputeTrust(a, b, ref rel, dt);
            }

            EvaluateMilestones(a, b, ref rel, eligible);

            Set(rel);
        }

        float ComputeAffinityDelta(AgentBase a, AgentBase b, RelState rel, float dt)
        {
            // Base: contact gently increases affinity
            float baseGain = affinityPerSecondOfContact * dt;

            // Placeholder: if irritation is already high, contact can be net-negative (zeta loop feel)
            float friction = Mathf.InverseLerp(50f, 100f, rel.irritation);
            float net = Mathf.Lerp(baseGain, -baseGain * 0.6f, friction);

            // Trait chemistry
            var inspA = a.GetComponent<InspectablePerson>();
            var inspB = b.GetComponent<InspectablePerson>();
            if (inspA != null && inspB != null)
            {
                // Kindness average boosts affinity gain
                float kindAvg = (inspA.kindness + inspB.kindness) * 0.5f;
                net += baseGain * (kindAvg - 5f) * 0.06f;

                // Warmth similarity boosts; big mismatch adds friction
                float warmthDiff = Mathf.Abs(inspA.warmth - inspB.warmth);
                net += baseGain * (5f - warmthDiff) * 0.04f;

                // Wit mismatch adds friction
                float witDiff = Mathf.Abs(inspA.wit - inspB.wit);
                net -= baseGain * Mathf.Max(0f, witDiff - 3f) * 0.03f;
            }

            return net;
        }

        void ComputeAttraction(AgentBase a, AgentBase b, ref RelState rel, float dt)
        {
            var inspA = a.GetComponent<InspectablePerson>();
            var inspB = b.GetComponent<InspectablePerson>();
            if (inspA == null || inspB == null) return;

            // Charm + flirtiness compatibility drives attraction
            float charmAvg = (inspA.charm + inspB.charm) * 0.5f;
            float flirtAvg = (inspA.flirtiness + inspB.flirtiness) * 0.5f;
            float drive = (charmAvg + flirtAvg - 10f) * 0.1f; // centered around 0 when both traits are 5

            float growth = attractionGrowthRate * dt * (1f + drive);

            // Decay during negative trend
            if (rel.trend < 0f)
                growth = -attractionGrowthRate * 0.5f * dt;

            rel.attraction = Mathf.Clamp(rel.attraction + growth, 0f, 100f);
        }

        void ComputeTrust(AgentBase a, AgentBase b, ref RelState rel, float dt)
        {
            // Trust only starts growing after sustained contact
            if (rel.skinTime < trustSkinTimeGate) return;

            var inspA = a.GetComponent<InspectablePerson>();
            var inspB = b.GetComponent<InspectablePerson>();

            float loyaltyBoost = 1f;
            if (inspA != null && inspB != null)
            {
                float loyaltyAvg = (inspA.loyalty + inspB.loyalty) * 0.5f;
                loyaltyBoost = 1f + (loyaltyAvg - 5f) * 0.08f;
            }

            float growth;
            if (rel.trend >= 0f)
                growth = trustGrowthRate * dt * loyaltyBoost;
            else
                growth = -trustGrowthRate * 0.3f * dt;

            rel.trust = Mathf.Clamp(rel.trust + growth, -100f, 100f);
        }

        bool IsRomanceEligible(AgentBase agent)
        {
            return !(agent is MonsterAgent);
        }

        void EvaluateMilestones(AgentBase a, AgentBase b, ref RelState rel, bool romanceEligible)
        {
            if (!rel.isAcquaintances && rel.affinity >= acquaintanceAffinity)
            {
                rel.isAcquaintances = true;
                PushHighlight($"{NameOf(a)} and {NameOf(b)} became <b>acquaintances</b>.");
            }

            if (!rel.isFriends && rel.affinity >= friendAffinity)
            {
                rel.isFriends = true;
                PushHighlight($"{NameOf(a)} and {NameOf(b)} became <b>friends</b>.");
            }

            // Soulmate (rare, non-romantic allowed)
            if (!rel.isSoulmates && rel.affinity >= soulmateAffinity && rel.charge != RelCharge.Zeta)
            {
                rel.isSoulmates = true;
                PushHighlight($"{NameOf(a)} and {NameOf(b)} formed a rare <b>Soulmate Bond</b>.");
            }

            // Nemesis (mutual; obsession is separate later)
            if (!rel.isNemesis && rel.irritation >= nemesisIrritation)
            {
                rel.isNemesis = true;
                PushHighlight($"{NameOf(a)} and {NameOf(b)} became <b>Nemeses</b>.");
            }

            // Romance milestones
            if (romanceEligible)
            {
                EvaluateRomanceMilestones(a, b, ref rel);
                EvaluateHeartbreak(a, b, ref rel);
            }
        }

        void EvaluateRomanceMilestones(AgentBase a, AgentBase b, ref RelState rel)
        {
            int ida = rel.aId;
            int idb = rel.bId;

            // Crush
            if (!rel.isCrushing && rel.affinity >= crushAffinity && rel.attraction >= crushAttraction)
            {
                rel.isCrushing = true;
                string msg = $"{NameOf(a)} and {NameOf(b)} developed a <b>crush</b>!";
                PushHighlight(msg);
                EmitRomanceEvent(ida, idb, "Crush");
            }

            // Dating (requires crush)
            if (!rel.isDating && rel.isCrushing &&
                rel.affinity >= datingAffinity && rel.attraction >= datingAttraction && rel.trust >= datingTrust)
            {
                rel.isDating = true;
                RegisterRomance(ida, idb);
                CheckJealousy(a, b, ida, idb);
                string msg = $"{NameOf(a)} and {NameOf(b)} started <b>dating</b>!";
                PushHighlight(msg);
                EmitRomanceEvent(ida, idb, "Dating");
            }

            // Lovers (requires dating)
            if (!rel.isLovers && rel.isDating &&
                rel.affinity >= loversAffinity && rel.attraction >= loversAttraction && rel.trust >= loversTrust)
            {
                rel.isLovers = true;
                string msg = $"{NameOf(a)} and {NameOf(b)} became <b>lovers</b>!";
                PushHighlight(msg);
                EmitRomanceEvent(ida, idb, "Lovers");
            }
        }

        void EvaluateHeartbreak(AgentBase a, AgentBase b, ref RelState rel)
        {
            int ida = rel.aId;
            int idb = rel.bId;

            if (rel.isLovers && (rel.affinity < loversAffinity - heartbreakHysteresis ||
                                  rel.attraction < loversAttraction - heartbreakHysteresis ||
                                  rel.trust < loversTrust - heartbreakHysteresis))
            {
                rel.isLovers = false;
                rel.isDating = false;
                rel.isCrushing = false;
                UnregisterRomance(ida, idb);
                string msg = $"{NameOf(a)} and {NameOf(b)} <b>broke up</b> (were lovers).";
                PushHighlight(msg);
                EmitRomanceEvent(ida, idb, "Heartbreak");
            }
            else if (rel.isDating && !rel.isLovers &&
                     (rel.affinity < datingAffinity - heartbreakHysteresis ||
                      rel.attraction < datingAttraction - heartbreakHysteresis ||
                      rel.trust < datingTrust - heartbreakHysteresis))
            {
                rel.isDating = false;
                rel.isCrushing = false;
                UnregisterRomance(ida, idb);
                string msg = $"{NameOf(a)} and {NameOf(b)} <b>stopped dating</b>.";
                PushHighlight(msg);
                EmitRomanceEvent(ida, idb, "Heartbreak");
            }
            else if (rel.isCrushing && !rel.isDating &&
                     (rel.affinity < crushAffinity - heartbreakHysteresis ||
                      rel.attraction < crushAttraction - heartbreakHysteresis))
            {
                rel.isCrushing = false;
                string msg = $"{NameOf(a)} and {NameOf(b)}'s crush <b>faded</b>.";
                PushHighlight(msg);
                EmitRomanceEvent(ida, idb, "CrushFaded");
            }
        }

        void RegisterRomance(int ida, int idb)
        {
            if (!_romances.TryGetValue(ida, out var setA))
            {
                setA = new HashSet<int>();
                _romances[ida] = setA;
            }
            setA.Add(idb);

            if (!_romances.TryGetValue(idb, out var setB))
            {
                setB = new HashSet<int>();
                _romances[idb] = setB;
            }
            setB.Add(ida);
        }

        void UnregisterRomance(int ida, int idb)
        {
            if (_romances.TryGetValue(ida, out var setA)) setA.Remove(idb);
            if (_romances.TryGetValue(idb, out var setB)) setB.Remove(ida);
        }

        void CheckJealousy(AgentBase a, AgentBase b, int ida, int idb)
        {
            // If either party already has romances, boost irritation in existing relationships
            CheckJealousyFor(a, ida, idb);
            CheckJealousyFor(b, idb, ida);
        }

        void CheckJealousyFor(AgentBase agent, int agentId, int newPartnerId)
        {
            if (!_romances.TryGetValue(agentId, out var existing)) return;

            foreach (int partnerId in existing)
            {
                if (partnerId == newPartnerId) continue;

                var partnerRel = GetOrCreate(agentId, partnerId);
                partnerRel.irritation = Mathf.Clamp(partnerRel.irritation + 15f, 0f, 100f);
                Set(partnerRel);

                string agentName = NameOf(agent);
                string partnerName = NameOfId(partnerId);
                PushHighlight($"{partnerName} feels <b>jealous</b> about {agentName}'s new romance.");
                EmitRomanceEvent(agentId, partnerId, "Jealousy");
            }
        }

        void CleanUpRomancesForDeceased(int deceasedId)
        {
            if (!_romances.TryGetValue(deceasedId, out var partners)) return;

            foreach (int partnerId in partners)
            {
                if (_romances.TryGetValue(partnerId, out var partnerSet))
                    partnerSet.Remove(deceasedId);

                // Clear romance flags on the relationship
                var rel = GetOrCreate(deceasedId, partnerId);
                bool hadRomance = rel.isCrushing || rel.isDating || rel.isLovers;
                rel.isCrushing = false;
                rel.isDating = false;
                rel.isLovers = false;
                Set(rel);

                if (hadRomance)
                {
                    string partnerName = NameOfId(partnerId);
                    string deceasedName = NameOfId(deceasedId);
                    PushHighlight($"{partnerName} <b>mourns</b> the loss of {deceasedName}.");
                    EmitRomanceEvent(deceasedId, partnerId, "Mourning");
                }
            }

            _romances.Remove(deceasedId);
        }

        void EmitRomanceEvent(int ida, int idb, string stage)
        {
            GameEventBus.Emit(GameEvent.Make(
                GameEventType.RomanceMilestone,
                aId: ida, bId: idb,
                text: stage));
        }

        // --- Public accessors for UI ---

        public RelState GetRelation(int ida, int idb)
        {
            ulong k = Key(ida, idb);
            return _rels.TryGetValue(k, out var rel) ? rel : default;
        }

        public bool HasActiveRomance(int agentId)
        {
            return _romances.TryGetValue(agentId, out var set) && set.Count > 0;
        }

        public HashSet<int> GetRomancePartners(int agentId)
        {
            return _romances.TryGetValue(agentId, out var set) ? set : null;
        }

        // --- Highlights & queries ---

        void PushHighlight(string s)
        {
            _recentHighlights.Add(s);
            if (_recentHighlights.Count > 120) _recentHighlights.RemoveAt(0);
        }

        public void AppendHighlights(System.Text.StringBuilder sb, WorldUpdateKind kind)
        {
            int max = kind == WorldUpdateKind.FestivalMidyear ? 6 : 10;

            int start = Mathf.Max(0, _recentHighlights.Count - max);
            for (int i = start; i < _recentHighlights.Count; i++)
                sb.AppendLine("- " + _recentHighlights[i]);
        }


        public List<RelState> GetTopRelationshipsByTrend(int count)
        {
            var list = new List<RelState>(_rels.Values);
            list.Sort((x, y) => Mathf.Abs(y.trend).CompareTo(Mathf.Abs(x.trend)));
            if (list.Count > count) list.RemoveRange(count, list.Count - count);
            return list;
        }

        // --- Helpers ---

        string NameOf(AgentBase a)
        {
            var insp = a.GetComponent<InspectablePerson>();
            if (insp != null && !string.IsNullOrEmpty(insp.displayName)) return insp.displayName;
            return a.GetType().Name + $"#{a.pid.id}";
        }

        string NameOfId(int id)
        {
            if (AgentBase.TryGet(id, out var agent)) return NameOf(agent);
            return $"Agent#{id}";
        }

        static ulong Key(int a, int b)
        {
            uint lo = (uint)Mathf.Min(a, b);
            uint hi = (uint)Mathf.Max(a, b);
            return ((ulong)hi << 32) | lo;
        }

        RelState GetOrCreate(int a, int b)
        {
            ulong k = Key(a, b);
            if (_rels.TryGetValue(k, out var rel)) return rel;
            rel = new RelState { aId = Mathf.Min(a, b), bId = Mathf.Max(a, b) };
            _rels[k] = rel;
            return rel;
        }

        void Set(RelState rel) => _rels[Key(rel.aId, rel.bId)] = rel;
    }

    [System.Serializable]
    public struct RelState
    {
        public int aId, bId;

        public float affinity;     // -100..100
        public float attraction;   // 0..100
        public float trust;        // -100..100
        public float irritation;   // 0..100

        public float skinTime;     // seconds of contact
        public Vector3 lastContactPoint;
        public float lastContactAtGameTime01;

        public float trend;        // -1..1
        public RelCharge charge;

        public bool isAcquaintances;
        public bool isFriends;
        public bool isSoulmates;
        public bool isNemesis;

        // Romance stages (progressive: crush → dating → lovers)
        public bool isCrushing;
        public bool isDating;
        public bool isLovers;
    }

}
