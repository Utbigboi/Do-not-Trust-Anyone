using System.Collections.Generic;
using UnityEngine;

namespace SafehouseDesk
{
    // Spawns the intel for each shift and advances when the desk is clear.
    public class ShiftController : MonoBehaviour
    {
        public static ShiftController Instance { get; private set; }

        [Header("Where documents appear (assign 2-3 empty transforms)")]
        public Transform[] traySpots;
        [Header("Optional material for documents (leave empty for default)")]
        public Material docMaterial;

        List<List<IntelData>> shifts;
        int shiftIndex = -1;
        int remaining = 0;

        void Awake() { Instance = this; }
        void Start() { shifts = GameContent.Shifts(); }

        public void BeginRealGame()
        {
            GameManager.Instance.SetPhase(GamePhase.Playing);
            shiftIndex = -1;
            NextShift();
        }

        void NextShift()
        {
            shiftIndex++;
            if (shiftIndex >= shifts.Count) { GameManager.Instance.Win(); return; }

            var list = shifts[shiftIndex];
            remaining = list.Count;
            GameManager.Instance.Say($"SHIFT {shiftIndex + 1}. {remaining} items in the tray. Handle them all.", 4f);
            for (int i = 0; i < list.Count; i++) SpawnDoc(list[i], i);
        }

        void SpawnDoc(IntelData data, int i)
        {
            Vector3 pos;
            if (traySpots != null && traySpots.Length > 0)
                pos = traySpots[i % traySpots.Length].position + Vector3.up * (0.03f * (i / traySpots.Length));
            else
                pos = new Vector3(-0.25f + 0.25f * i, 0.9f, 0.5f);
            Document.Create(data, pos, docMaterial);
        }

        // Called whenever a document leaves play (reported or shredded).
        public void DecrementRemaining()
        {
            if (GameManager.Instance.Phase != GamePhase.Playing) return; // tutorial doc doesn't count
            remaining--;
            if (remaining <= 0) NextShift();
        }

        public int ShiftNumber => shiftIndex + 1;
    }
}
