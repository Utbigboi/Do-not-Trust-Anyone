using UnityEngine;

namespace CaseDesk
{
    // One case = one asset. Right-click in Project -> Create -> CaseDesk -> Round.
    [CreateAssetMenu(menuName = "CaseDesk/Round", fileName = "Round_")]
    public class RoundData : ScriptableObject
    {
        [Header("Case")]
        public string caseName = "Untitled Case";
        [TextArea] public string briefing = "";
        [Tooltip("Who's really guilty. None = both gangs innocent / the Board did it.")]
        public Party culprit = Party.None;

        [System.Serializable]
        public class EvidenceSpawn
        {
            public GameObject prefab;                 // must have EvidenceObject + collider + POIs
            public Vector3 localPosition;             // offset from the Evidence Root (0 = auto-row)
            public Vector3 euler;                     // optional starting rotation
        }

        [Header("Evidence (spawned on the table)")]
        public EvidenceSpawn[] evidence;
    }
}
