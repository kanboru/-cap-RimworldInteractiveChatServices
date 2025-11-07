using LudeonTK;
using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;
using CAP_ChatInteractive.Commands.CommandHandlers;

namespace CAP_ChatInteractive
{
    [StaticConstructorOnStartup]
    public static class DebugActions_CAP
    {
        [DebugAction("CAP", "Debug Player Faction Pawn Kinds", allowedGameStates = AllowedGameStates.Playing)]
        public static void DebugPlayerFactionPawnKinds()
        {
            Find.WindowStack.Add(new Dialog_DebugPawnKinds());
        }

        [DebugAction("CAP", "Debug Race Settings", allowedGameStates = AllowedGameStates.Playing)]
        public static void DebugRaceSettings()
        {
            Find.WindowStack.Add(new Dialog_DebugRaces());
        }
    }

    public class Dialog_DebugPawnKinds : Window
    {
        private Vector2 scrollPosition = Vector2.zero;
        private string searchQuery = "";
        private ThingDef selectedRace = null;

        public override Vector2 InitialSize => new Vector2(800f, 600f);

        public Dialog_DebugPawnKinds()
        {
            doCloseButton = true;
            forcePause = true;
            absorbInputAroundWindow = true;
            optionalTitle = "CAP - Pawn Kind Debug";
        }

        public override void DoWindowContents(Rect inRect)
        {
            // Header
            Rect headerRect = new Rect(0f, 0f, inRect.width, 30f);
            Text.Font = GameFont.Medium;
            Widgets.Label(headerRect, "Pawn Kind Debug - Player Factions");
            Text.Font = GameFont.Small;

            // Search
            Rect searchRect = new Rect(0f, 35f, 200f, 25f);
            searchQuery = Widgets.TextField(searchRect, searchQuery);

            // Race list
            Rect listRect = new Rect(0f, 65f, 250f, inRect.height - 100f);
            DrawRaceList(listRect);

            // Details
            if (selectedRace != null)
            {
                Rect detailsRect = new Rect(260f, 65f, inRect.width - 265f, inRect.height - 100f);
                DrawRaceDetails(detailsRect);
            }
        }

        private void DrawRaceList(Rect rect)
        {
            Widgets.DrawMenuSection(rect);

            var races = DefDatabase<ThingDef>.AllDefs
                .Where(d => d.race?.Humanlike ?? false)
                .Where(d => string.IsNullOrEmpty(searchQuery) ||
                           d.defName.ToLower().Contains(searchQuery.ToLower()) ||
                           d.label.ToLower().Contains(searchQuery.ToLower()))
                .OrderBy(d => d.defName)
                .ToList();

            float viewHeight = races.Count * 30f;
            Rect viewRect = new Rect(0f, 0f, rect.width - 20f, viewHeight);

            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
            {
                float y = 0f;
                foreach (var race in races)
                {
                    Rect buttonRect = new Rect(5f, y, viewRect.width - 10f, 25f);
                    if (Widgets.ButtonText(buttonRect, race.defName))
                    {
                        selectedRace = race;
                    }
                    y += 30f;
                }
            }
            Widgets.EndScrollView();
        }

        private void DrawRaceDetails(Rect rect)
        {
            Widgets.DrawMenuSection(rect);

            var pawnKinds = DefDatabase<PawnKindDef>.AllDefs
                .Where(pk => pk.race == selectedRace)
                .OrderBy(pk => pk.defName)
                .ToList();

            float y = 10f;
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(10f, y, rect.width, 30f), $"Pawn Kinds for: {selectedRace.defName}");
            Text.Font = GameFont.Small;
            y += 35f;

            // Column headers
            Widgets.Label(new Rect(10f, y, 200f, 25f), "Pawn Kind DefName");
            Widgets.Label(new Rect(220f, y, 200f, 25f), "Faction Def");
            Widgets.Label(new Rect(430f, y, 100f, 25f), "isPlayer");
            Widgets.Label(new Rect(540f, y, 100f, 25f), "Combat Power");
            y += 25f;

            Widgets.DrawLineHorizontal(10f, y, rect.width - 20f);
            y += 10f;

            foreach (var pk in pawnKinds)
            {
                string factionInfo = pk.defaultFactionDef?.defName ?? "None";
                string isPlayer = pk.defaultFactionDef?.isPlayer.ToString() ?? "False";
                string combatPower = pk.combatPower.ToString();

                // Highlight player factions
                if (pk.defaultFactionDef?.isPlayer == true)
                {
                    GUI.color = Color.green;
                }

                Widgets.Label(new Rect(10f, y, 200f, 25f), pk.defName);
                Widgets.Label(new Rect(220f, y, 200f, 25f), factionInfo);
                Widgets.Label(new Rect(430f, y, 100f, 25f), isPlayer);
                Widgets.Label(new Rect(540f, y, 100f, 25f), combatPower);

                GUI.color = Color.white;
                y += 25f;
            }

            // Show which one would be selected
            y += 10f;
            Widgets.DrawLineHorizontal(10f, y, rect.width - 20f);
            y += 15f;

            var selectedPawnKind = BuyPawnCommandHandler.GetPawnKindDefForRace(selectedRace.defName);
            if (selectedPawnKind != null)
            {
                Widgets.Label(new Rect(10f, y, rect.width - 20f, 30f),
                    $"SELECTED FOR PURCHASE: {selectedPawnKind.defName} (Faction: {selectedPawnKind.defaultFactionDef?.defName ?? "None"})");
            }
        }
    }
}