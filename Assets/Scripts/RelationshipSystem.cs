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

        readonly Dictionary<ulong, RelState> _rels = new(8192);
        readonly List<string> _recentHighlights = new(256);

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void RegisterContact(AgentBase a, AgentBase b, Vector3 contactPoint, float dt)
        {
            if (a?.pid == null || b?.pid == null) return;

            int ida = a.pid.id;
            int idb = b.pid.id;

            var rel = GetOrCreate(ida, idb);

            rel.skinTime += dt;
            rel.lastContactPoint = contactPoint;
            rel.lastContactAtGameTime01 = CozyTown.Core.GameTime.Instance != null ? CozyTown.Core.GameTime.Instance.dayTime01 : 0f;

            // Compute chemistry delta (stub: contact = small positive; later boons/banes)
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

            EvaluateMilestones(a, b, ref rel);

            Set(rel);
        }

        float ComputeAffinityDelta(AgentBase a, AgentBase b, RelState rel, float dt)
        {
            // Base: contact gently increases affinity
            float baseGain = affinityPerSecondOfContact * dt;

            // Placeholder: if irritation is already high, contact can be net-negative (zeta loop feel)
            float friction = Mathf.InverseLerp(50f, 100f, rel.irritation);
            float net = Mathf.Lerp(baseGain, -baseGain * 0.6f, friction);

            // TODO next: boons/banes from 10 visible stats + hidden traits
            return net;
        }

        void EvaluateMilestones(AgentBase a, AgentBase b, ref RelState rel)
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

            // TODO: romance gates; monogamy/open emerges from events (not default)
        }

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

        string NameOf(AgentBase a)
        {
            var insp = a.GetComponent<InspectablePerson>();
            if (insp != null && !string.IsNullOrEmpty(insp.displayName)) return insp.displayName;
            return a.GetType().Name + $"#{a.pid.id}";
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
        public float attraction;   // 0..100 (later)
        public float trust;        // -100..100 (later)
        public float irritation;   // 0..100 (later)

        public float skinTime;     // seconds of contact
        public Vector3 lastContactPoint;
        public float lastContactAtGameTime01;

        public float trend;        // -1..1
        public RelCharge charge;

        public bool isAcquaintances;
        public bool isFriends;
        public bool isSoulmates;
        public bool isNemesis;

        // Later: boonedBy/banedBy tags, relationship type, meetup schedule, etc.
    }

}
