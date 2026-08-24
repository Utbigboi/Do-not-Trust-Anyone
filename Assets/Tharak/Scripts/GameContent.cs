using System.Collections.Generic;

namespace SafehouseDesk
{
    // Hardcoded sample content so the game is playable with zero Inspector setup.
    // Edit freely, or replace with ScriptableObjects later.
    public static class GameContent
    {
        public static IntelData Tutorial() => new IntelData
        {
            id = "tut_01",
            text = "Bureau courier uses the 4th St. locker at 0900.",
            source = FactionId.Bureau, truth = TruthValue.True,
            valueToOther = 15, hasSeal = true, sealIsForged = false,
            bureauKnows = true, syndicateKnows = false
        };

        public static List<List<IntelData>> Shifts()
        {
            var shifts = new List<List<IntelData>>();

            // ---- Shift 1: gentle. Learn the loop. ----
            shifts.Add(new List<IntelData>
            {
                new IntelData{ id="s1_a", text="Syndicate stash moved to Dock 7.",
                    source=FactionId.Syndicate, truth=TruthValue.True, valueToOther=18,
                    hasSeal=true, sealIsForged=false, syndicateKnows=true },
                new IntelData{ id="s1_b", text="Bureau agent 'Wren' is a plant. (sealed)",
                    source=FactionId.Bureau, truth=TruthValue.False, valueToOther=12,
                    hasSeal=true, sealIsForged=true, bureauKnows=true }, // forged seal -> false intel
                new IntelData{ id="s1_c", text="Payment drop Friday, midnight.",
                    source=FactionId.Syndicate, truth=TruthValue.True, valueToOther=14,
                    hasSeal=false },
            });

            // ---- Shift 2: a canary trap + a Kessler claim you may regret. ----
            shifts.Add(new List<IntelData>
            {
                new IntelData{ id="s2_a", text="Bureau safehouse at 22 Vane Rd. (EYES ONLY).",
                    source=FactionId.Bureau, truth=TruthValue.True, isBait=true, valueToOther=25,
                    hasSeal=true, sealIsForged=false, bureauKnows=true }, // BAIT: leak it and Bureau knows
                new IntelData{ id="s2_b", text="The mole inside the Bureau is 'Kessler'.",
                    topicId="kessler", source=FactionId.Syndicate, truth=TruthValue.Unknown, valueToOther=16,
                    hasSeal=true, sealIsForged=true }, // unverified rumor; lying asserts it as fact
                new IntelData{ id="s2_c", text="Weapons shipment delayed one week.",
                    source=FactionId.Syndicate, truth=TruthValue.True, valueToOther=13,
                    hasSeal=false, bureauKnows=true, syndicateKnows=true },
            });

            // ---- Shift 3: pressure. Contradiction payoff + a second canary. ----
            shifts.Add(new List<IntelData>
            {
                new IntelData{ id="s3_a", text="Bureau raids Dock 7 at dawn.",
                    source=FactionId.Bureau, truth=TruthValue.True, valueToOther=28,
                    hasSeal=true, sealIsForged=false, bureauKnows=true, syndicateKnows=true },
                new IntelData{ id="s3_b", text="Kessler cleared - he was never the mole.",
                    topicId="kessler", source=FactionId.Bureau, truth=TruthValue.False, valueToOther=10,
                    hasSeal=true, sealIsForged=false, bureauKnows=true }, // contradicts a Kessler lie
                new IntelData{ id="s3_c", text="Syndicate courier route: alley off Pike. (EYES ONLY)",
                    source=FactionId.Syndicate, truth=TruthValue.True, isBait=true, valueToOther=22,
                    hasSeal=true, sealIsForged=false, syndicateKnows=true }, // BAIT for the Syndicate
            });

            return shifts;
        }
    }
}
