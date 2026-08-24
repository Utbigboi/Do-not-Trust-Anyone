using System;
using UnityEngine;

namespace SafehouseDesk
{
    public enum GamePhase { Tutorial, Playing, Won, Lost }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Starting meters")]
        public int startValue = 40;

        [Header("Balance (tune these)")]
        public int valueGainLie = 8;
        public int valueLossStall = 8;
        public int leakSuspicion = 12;
        public int baitCaughtSuspicion = 45;
        public int lieCaughtSuspicion = 30;
        public int contradictionSuspicion = 25;
        public int loseSuspicion = 100;

        public FactionState Bureau { get; private set; }
        public FactionState Syndicate { get; private set; }
        public GamePhase Phase { get; private set; } = GamePhase.Tutorial;

        public bool IsCallOpen { get; private set; }
        public FactionId CallFaction { get; private set; }
        public Document CallDocument { get; private set; }
        public bool Inspecting;  // set by DocumentInspector

        public bool UIOpen => IsCallOpen || Inspecting || Phase == GamePhase.Won || Phase == GamePhase.Lost;

        public string Subtitle = "";
        float subtitleClearAt = -1f;

        public event Action OnReportResolved;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            Bureau = new FactionState(FactionId.Bureau, startValue);
            Syndicate = new FactionState(FactionId.Syndicate, startValue);
        }

        void Update()
        {
            if (subtitleClearAt > 0 && Time.time >= subtitleClearAt) { Subtitle = ""; subtitleClearAt = -1f; }
        }

        public FactionState State(FactionId f) => f == FactionId.Bureau ? Bureau : Syndicate;
        public static FactionId Other(FactionId f) => f == FactionId.Bureau ? FactionId.Syndicate : FactionId.Bureau;
        public static TruthValue Flip(TruthValue t)
            => t == TruthValue.True ? TruthValue.False : (t == TruthValue.False ? TruthValue.True : TruthValue.True);

        public void SetPhase(GamePhase p) => Phase = p;

        public void Say(string msg, float seconds = 4f)
        {
            Subtitle = msg;
            subtitleClearAt = seconds > 0 ? Time.time + seconds : -1f;
        }

        public void OpenCall(FactionId faction, Document doc)
        {
            if (doc == null) return;
            IsCallOpen = true; CallFaction = faction; CallDocument = doc;
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
        }

        public void ResolveCall(ReportAction action)
        {
            if (!IsCallOpen) return;
            var faction = CallFaction;
            var doc = CallDocument;
            IsCallOpen = false;

            bool ended = Phase == GamePhase.Won || Phase == GamePhase.Lost;
            if (!ended) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }

            bool consume = true;
            if (Phase == GamePhase.Tutorial)
                consume = TutorialController.Instance != null &&
                          TutorialController.Instance.HandleTutorialReport(faction, doc, action);
            else
                ApplyReport(faction, doc.data, action);

            if (consume && PlayerInteractor.Instance != null)
                PlayerInteractor.Instance.ConsumeHeld();

            OnReportResolved?.Invoke();
            CheckLose();
        }

        void ApplyReport(FactionId target, IntelData item, ReportAction action)
        {
            var T = State(target);
            var src = item.source;

            switch (action)
            {
                case ReportAction.ReportStraight:
                    RecordClaim(target, item, item.truth);
                    T.AddValue(target != src ? item.valueToOther : 3);
                    if (target != src) State(src).AddSuspicion(leakSuspicion);
                    if (item.isBait && target != src)
                    {
                        State(src).AddSuspicion(baitCaughtSuspicion);
                        Say($"{src} seeded that as a trap. They know it leaked through you.", 5f);
                    }
                    break;

                case ReportAction.Lie:
                    RecordClaim(target, item, Flip(item.truth));
                    T.AddValue(valueGainLie);
                    if (item.VerifiableBy(target))
                    {
                        T.AddSuspicion(lieCaughtSuspicion);
                        Say($"The {target} can check that themselves. The lie won't hold.", 5f);
                    }
                    break;

                case ReportAction.Downplay:
                    T.AddValue(-valueLossStall);
                    break;
            }

            if (T.value <= 0 && T.active)
            {
                T.active = false;
                Say($"The {target} line went dead. They've stopped feeding you anything.", 5f);
            }
        }

        void RecordClaim(FactionId target, IntelData item, TruthValue stated)
        {
            string key = string.IsNullOrEmpty(item.topicId) ? item.id : item.topicId;
            var T = State(target);

            if (stated != TruthValue.Unknown)
            {
                foreach (var c in T.ledger)
                    if (c.key == key && c.stated != TruthValue.Unknown && c.stated != stated)
                    {
                        T.AddSuspicion(contradictionSuspicion);
                        Say($"That contradicts what you already told the {target}.", 5f);
                        break;
                    }
            }
            T.ledger.Add(new Claim(target, key, stated));
        }

        void CheckLose()
        {
            if (Phase == GamePhase.Won || Phase == GamePhase.Lost) return;
            if (Bureau.suspicion >= loseSuspicion) Lose(FactionId.Bureau);
            else if (Syndicate.suspicion >= loseSuspicion) Lose(FactionId.Syndicate);
        }

        public void Lose(FactionId who)
        {
            Phase = GamePhase.Lost;
            Say($"A knock at the door. The {who} worked out what you are.", 0f);
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
        }

        public void Win()
        {
            Phase = GamePhase.Won;
            Say("You made it through the week playing both sides. For now.", 0f);
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
        }
    }
}
