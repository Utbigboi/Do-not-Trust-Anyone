using UnityEngine;

namespace CaseDesk
{
    public enum Party { Red, Blue, Organisation, None }
    public enum Reliability { Solid, Planted }   // Solid = true clue; Planted = false / red herring

    // A single piece of info found on an evidence piece and written to the notepad.
    [System.Serializable]
    public class Observation
    {
        public string id;
        public string text = "A detail.";
        public Party implicates = Party.None;      // who this points at
        public Reliability reliability = Reliability.Solid;
        [Range(1, 3)] public int strength = 1;     // how damning it is
        public string source = "";                 // which evidence it came from
    }
}
