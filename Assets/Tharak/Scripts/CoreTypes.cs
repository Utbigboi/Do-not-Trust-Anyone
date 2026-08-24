using UnityEngine;

namespace SafehouseDesk
{
    public enum FactionId { Bureau, Syndicate }
    public enum TruthValue { True, False, Unknown }
    public enum ReportAction { ReportStraight, Lie, Downplay }

    // A single piece of intel. Plain serializable class so no ScriptableObject assets
    // are needed for the MVP (you can migrate to SOs later — see the guide).
    [System.Serializable]
    public class IntelData
    {
        public string id = "intel";
        [TextArea(2, 4)] public string text = "Intel text.";
        public string topicId = "";                 // links related intel for contradiction checks
        public FactionId source = FactionId.Bureau;  // who gave it to you
        public TruthValue truth = TruthValue.True;   // the real truth (hidden from player)
        public bool isBait = false;                  // canary trap: unique to the source
        [Range(0, 40)] public int valueToOther = 15; // usefulness to the rival faction
        public bool hasSeal = true;                  // visible "authentic" seal
        public bool sealIsForged = false;            // seal is fake (revealed by inspecting)
        public bool bureauKnows = false;             // can the Bureau independently verify it?
        public bool syndicateKnows = false;          // can the Syndicate?

        public bool VerifiableBy(FactionId f)
            => (f == FactionId.Bureau && bureauKnows) || (f == FactionId.Syndicate && syndicateKnows);
    }

    // A recorded statement you made to a faction, used by the consistency engine.
    public struct Claim
    {
        public FactionId toFaction;
        public string key;        // topicId (or id if no topic)
        public TruthValue stated;
        public Claim(FactionId f, string key, TruthValue s) { toFaction = f; this.key = key; stated = s; }
    }
}
