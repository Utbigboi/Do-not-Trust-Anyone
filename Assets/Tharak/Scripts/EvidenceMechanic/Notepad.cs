using System.Collections.Generic;
using UnityEngine;

namespace CaseDesk
{
    // Holds every observation the player has found this round.
    public class Notepad : MonoBehaviour
    {
        public static Notepad I { get; private set; }
        public readonly List<Observation> entries = new List<Observation>();

        void Awake() { I = this; }

        public void Add(Observation o) { if (o != null) entries.Add(o); }
        public void Clear() { entries.Clear(); }
    }
}
