using System.Collections.Generic;
using UnityEngine;

namespace SafehouseDesk
{
    // Runtime state for one faction: the two meters plus your ledger of claims.
    public class FactionState
    {
        public FactionId id;
        public int value = 40;     // 0..100  (how useful they think you are)
        public int suspicion = 0;  // 0..100  (how close they are to burning you)
        public bool active = true; // line still open?
        public readonly List<Claim> ledger = new List<Claim>();

        public FactionState(FactionId id, int startValue) { this.id = id; this.value = startValue; }

        public void AddValue(int d)     => value = Mathf.Clamp(value + d, 0, 100);
        public void AddSuspicion(int d) => suspicion = Mathf.Clamp(suspicion + d, 0, 100);
    }
}
