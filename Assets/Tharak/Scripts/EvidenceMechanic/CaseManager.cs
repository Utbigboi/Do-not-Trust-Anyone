using System.Collections.Generic;
using UnityEngine;

namespace CaseDesk
{
    // Meters + scoring for tipping and accusing. Set `culprit` per round.
    public class CaseManager : MonoBehaviour
    {
        public static CaseManager I { get; private set; }

        [Header("Hidden truth (None = both gangs innocent / Organisation framed)")]
        public Party culprit = Party.None;

        [Header("Meters 0..100")]
        public int susRed = 20;
        public int susBlue = 20;
        public int susOrg = 20;
        public int credibility = 50;

        [Header("Reset values (used by resetMetersEachRound)")]
        public int startSuspicion = 20;
        public int startCredibility = 50;

        [Header("Tuning — tipping")]
        public int tipWeight = 5;        // implicated party's suspicion += strength * tipWeight
        public int credTipLie = 6;       // credibility lost per PLANTED observation you tipped

        [Header("Tuning — accusation")]
        public int baseCorrect = 18;
        public int baseWrong = 20;
        public int credPerSolid = 3;     // per solid, on-point backing observation
        public int credPerPlanted = 6;   // per planted observation used as backing
        public int accusePublic = 25;    // suspicion added to the accused party
        public int clearedDrop = 10;     // suspicion removed from the cleared gang
        public int bothInnocentOrg = 22;
        public int bothInnocentGangDrop = 8;
        public bool retaliationLoss = true;

        int plantedTips;
        public string lastResult = "";

        void Awake() { I = this; }

        public void ResetMeters()
        {
            susRed = susBlue = susOrg = startSuspicion;
            credibility = startCredibility;
            plantedTips = 0;
        }

        int Clamp(int v) => Mathf.Clamp(v, 0, 100);
        int Sus(Party p) => p == Party.Red ? susRed : p == Party.Blue ? susBlue : p == Party.Organisation ? susOrg : 0;
        void AddSus(Party p, int d)
        {
            if (p == Party.Red) susRed = Clamp(susRed + d);
            else if (p == Party.Blue) susBlue = Clamp(susBlue + d);
            else if (p == Party.Organisation) susOrg = Clamp(susOrg + d);
        }
        void AddCred(int d) => credibility = Mathf.Clamp(credibility + d, 0, 100);

        // ---- Tipping: private nudge to a faction ----
        public void Tip(Observation o, Party toFaction)
        {
            if (o == null) return;
            if (o.implicates != Party.None) AddSus(o.implicates, o.strength * tipWeight);
            if (o.reliability == Reliability.Planted) plantedTips++;   // debunked later -> credibility hit
            lastResult = "Tipped " + toFaction + ": " + o.text;
        }

        // ---- Accusation: public, scored ----
        public void Accuse(Party verdict, List<Observation> backing)
        {
            bool correct = verdict == culprit;

            int planted = 0, solidOnPoint = 0;
            foreach (var o in backing)
            {
                if (o.reliability == Reliability.Planted) planted++;
                else if (Supports(verdict, o)) solidOnPoint++;
            }

            // credibility
            if (correct) AddCred(baseCorrect + solidOnPoint * credPerSolid - planted * credPerPlanted);
            else AddCred(-baseWrong - planted * credPerPlanted);
            if (plantedTips > 0) { AddCred(-plantedTips * credTipLie); }

            // public suspicion
            float scale = 0.5f + Mathf.Min(solidOnPoint, 4) * 0.25f;
            if (verdict == Party.Red) { AddSus(Party.Red, Mathf.RoundToInt(accusePublic * scale)); AddSus(Party.Blue, -clearedDrop); }
            else if (verdict == Party.Blue) { AddSus(Party.Blue, Mathf.RoundToInt(accusePublic * scale)); AddSus(Party.Red, -clearedDrop); }
            else { AddSus(Party.Organisation, bothInnocentOrg); AddSus(Party.Red, -bothInnocentGangDrop); AddSus(Party.Blue, -bothInnocentGangDrop); }

            lastResult = (correct ? "CORRECT — accused " : "WRONG — accused ") + verdict + ".  Credibility " + credibility + ".";
            if (credibility <= 0) lastResult += "  You're finished.";
            if (retaliationLoss && !correct && (verdict == Party.Red || verdict == Party.Blue) && Sus(verdict) >= 100)
                lastResult += "  You framed " + verdict + " to the hilt — they retaliate.";

            plantedTips = 0;
        }

        bool Supports(Party verdict, Observation o)
        {
            if (verdict == Party.None) return o.implicates == Party.Organisation || o.implicates == Party.None;
            return o.implicates == verdict;
        }
    }
}
