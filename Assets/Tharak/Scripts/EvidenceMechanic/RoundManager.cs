using System.Collections.Generic;
using UnityEngine;

namespace CaseDesk
{
    // Steps through RoundData assets. Add rounds by dropping assets into the list.
    public class RoundManager : MonoBehaviour
    {
        public static RoundManager I { get; private set; }
        public enum State { Playing, Won, Lost }

        [Header("Rounds (author as assets, order = play order)")]
        public RoundData[] rounds;

        [Header("Refs")]
        public Transform evidenceRoot;      // table centre at surface height (assign TableTop)
        public CaseManager caseManager;

        [Header("Options")]
        public bool resetMetersEachRound = false;
        public float autoRowSpacing = 0.32f;

        public State state { get; private set; } = State.Playing;
        public int index { get; private set; }
        public RoundData Current => (rounds != null && index < rounds.Length) ? rounds[index] : null;

        readonly List<GameObject> spawned = new List<GameObject>();

        void Awake() { I = this; if (!caseManager) caseManager = FindFirstObjectByType<CaseManager>(); }
        void Start() { index = 0; StartRound(0); }

        public void StartRound(int i)
        {
            index = i;
            var r = Current;
            if (r == null) { state = State.Won; return; }

            // clear last round's pieces + notes
            foreach (var go in spawned) if (go) Destroy(go);
            spawned.Clear();
            if (Notepad.I) Notepad.I.Clear();
            if (resetMetersEachRound && caseManager) caseManager.ResetMeters();
            if (caseManager) caseManager.culprit = r.culprit;

            // spawn evidence
            Vector3 origin = evidenceRoot ? evidenceRoot.position : Vector3.zero;
            for (int e = 0; e < (r.evidence?.Length ?? 0); e++)
            {
                var entry = r.evidence[e];
                if (!entry.prefab) continue;
                Vector3 pos = entry.localPosition == Vector3.zero
                    ? origin + new Vector3(-((r.evidence.Length - 1) * 0.5f) * autoRowSpacing + e * autoRowSpacing, 0.05f, 0f)
                    : origin + entry.localPosition;
                var go = Instantiate(entry.prefab, pos, Quaternion.Euler(entry.euler));
                spawned.Add(go);
            }
        }

        // called by the fax after a leak resolves
        public void Advance()
        {
            if (caseManager && caseManager.credibility <= 0) { state = State.Lost; return; }
            if (index + 1 >= rounds.Length) { state = State.Won; return; }
            StartRound(index + 1);
        }
    }
}
