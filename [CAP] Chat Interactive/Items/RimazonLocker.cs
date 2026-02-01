// Copyright (c) Captolamia
// This file is part of CAP Chat Interactive.
// 
// CAP Chat Interactive is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// CAP Chat Interactive is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with CAP Chat Interactive. If not, see <https://www.gnu.org/licenses/>.

// Filename: RimazonLocker.cs

using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
namespace CAP_ChatInteractive
{
    public class LockerExtension : DefModExtension
    {
        public int maxStacks = 24;  // Actually means "maximum stack groups/slots"
    }
    // Main Class
    public class Building_RimazonLocker : Building, IThingHolder, IHaulDestination, IStoreSettingsParent
    {
        public string customName = null;
        public ThingOwner innerContainer;
        public int MaxStacks => def.GetModExtension<LockerExtension>().maxStacks;
        // public int MaxStacksSlots => def.GetModExtension<LockerExtension>().maxStacks;  // Renamed for clarit
        public StorageSettings settings;

        

        // === IthingHolder
        public ThingOwner GetDirectlyHeldThings() => innerContainer;

        public new IThingHolder ParentHolder => null;

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            outChildren?.Clear();
        }

        //  === IHaulDestination
        public new IntVec3 Position => base.Position;           // Inherited from Thing, but explicit for clarity
        public new Map Map => base.Map;                         // Inherited from Thing
        public bool HaulDestinationEnabled => true;


        // IStoreSettingsParent
        public bool StorageTabVisible => true;
        public StorageSettings GetStoreSettings() => settings;
        public StorageSettings GetParentStoreSettings() => def.building.defaultStorageSettings;
        public void Notify_SettingsChanged()
        {
            // Optional: could invalidate haul jobs or refresh AI if needed
            // For now, empty is perfectly fine
        }

        // === Rename Locker
        public void RenameLocker(string newName)
        {
            customName = newName.NullOrEmpty() ? null : newName.Trim();
        }


        public override void PostMake()
        {
            base.PostMake();
            settings = new StorageSettings(this);
            if (def.building.defaultStorageSettings != null)
            {
                settings.CopyFrom(def.building.defaultStorageSettings);
            }
        }
        public override void SpawnSetup(Map map, bool respawningAfterReload)
        {
            base.SpawnSetup(map, respawningAfterReload);
            if (settings == null)
            {
                settings = new StorageSettings(this);
                if (def.building.defaultStorageSettings != null)
                {
                    settings.CopyFrom(def.building.defaultStorageSettings);
                }
            }
        }
        public Building_RimazonLocker()
        {
            innerContainer = new ThingOwner<Thing>(this, LookMode.Deep);
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref customName, "customName");
            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
            Scribe_Deep.Look(ref settings, "settings", this);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (innerContainer == null)
                {
                    innerContainer = new ThingOwner<Thing>(this, LookMode.Deep);
                }
                if (settings == null)
                {
                    settings = new StorageSettings(this);
                    if (def.building.defaultStorageSettings != null)
                    {
                        settings.CopyFrom(def.building.defaultStorageSettings);
                    }
                }
            }
        }
        public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
        {
            if (innerContainer != null && innerContainer.Count > 0)
            {
                innerContainer.TryDropAll(Position, Map, ThingPlaceMode.Near);
            }
            base.DeSpawn(mode);
        }
        // === IAcceptDropPod interface implementation
        public virtual bool Accepts(Thing thing)
        {
            // Debug logging
            // Logger.Debug($"Checking if locker accepts {thing?.def?.defName} x{thing?.stackCount}");

            if (thing == null) return false;

            // Don't accept pawns
            if (thing is Pawn)
            {
                // Logger.Debug($"Rejecting {thing.def.defName}: Cannot accept pawns");
                return false;
            }

            // Check storage settings filter
            if (!settings.filter.Allows(thing))
            {
                // Logger.Debug($"Rejecting {thing.def.defName}: Not in filter");
                return false;
            }

            // Find potential merge target (exact def + stackable)
            Thing existingStack = innerContainer.FirstOrDefault(t => t.def == thing.def && t.CanStackWith(thing));
            int spaceInExisting = existingStack != null ? existingStack.def.stackLimit - existingStack.stackCount : 0;

            if (existingStack != null && spaceInExisting > 0)
            {
                // True merge possible → no new slot consumed
                // Logger.Debug($"Accept: True merge into existing {thing.def.defName} (space left: {spaceInExisting}, adding up to {thing.stackCount})");
                return true;
            }
            else
            {
                // No merge possible (no existing OR existing full) → requires NEW stack slot
                if (innerContainer.Count >= MaxStacks)
                {
                    // Logger.Debug($"Reject: {thing.def.defName} needs new stack slot but at limit ({innerContainer.Count}/{MaxStacks})");
                    return false;
                }

                // Fallback safety: does the container even have capacity? (rarely fails post-filter)
                bool hasCapacity = innerContainer.CanAcceptAnyOf(thing);
                if (!hasCapacity)
                {
                    // Logger.Debug($"Reject: {thing.def.defName} fails container capacity check");
                    return false;
                }

                // Logger.Debug($"Accept: New stack slot available for {thing.def.defName} x{thing.stackCount} ({innerContainer.Count + 1}/{MaxStacks} after add)");
                return true;
            }
        }

        public void AcceptDropPod(DropPodIncoming dropPod, Thing[] contents)
        {
            foreach (Thing thing in contents)
            {
                if (Accepts(thing))
                {
                    innerContainer.TryAdd(thing, true);
                }
                else
                {
                    // Drop items that can't fit
                    GenPlace.TryPlaceThing(thing, Position, Map, ThingPlaceMode.Near);
                }
            }

            // Show delivery effect
            MoteMaker.ThrowText(this.DrawPos + new Vector3(0f, 0f, 0.25f), Map, "Delivery Received", Color.white, 2f);
        }


        public virtual bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
        {
            if (!Accepts(thing))
            {
                return false;
            }

            bool added = innerContainer.TryAdd(thing, allowSpecialEffects);
            if (added && allowSpecialEffects)
            {
                MoteMaker.ThrowText(this.DrawPos + new Vector3(0f, 0f, 0.25f), Map, "Delivery Received", Color.white, 2f);
            }
            return added;
        }
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            // Rename button
            yield return new Command_Action
            {
                defaultLabel = "Rename locker",
                defaultDesc = "Give this Rimazon locker a unique name for chat deliveries.",
                icon = ContentFinder<Texture2D>.Get("UI/Commands/RICS_Rename", true),
                action = () =>
                {
                    Find.WindowStack.Add(new Dialog_RenameLocker(this));
                }
            };
            //// Open button to view/access contents
            //yield return new Command_Action
            //{
            //    defaultLabel = "Open locker",
            //    defaultDesc = "View and access items in the locker.",
            //    icon = ContentFinder<Texture2D>.Get("UI/Commands/RICS_OpenLocker", true), // Reuse vanilla or adjust
            //    action = () => Open()
            //};
            // Eject button
            if (innerContainer.Count > 0)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Eject all contents",
                    defaultDesc = "Drop all items from the locker to the ground nearby.",
                    icon = ContentFinder<Texture2D>.Get("UI/Commands/RICS_Eject"),
                    action = () => innerContainer.TryDropAll(Position, Map, ThingPlaceMode.Near)
                };
            }
        }
        public bool CanOpen => true;
        public void Open()
        {
            Find.WindowStack.Add(new Dialog_LockerContents(this));
        }
        public override string GetInspectString()
        {
            string text = base.GetInspectString();
            if (!customName.NullOrEmpty())
            {
                if (!text.NullOrEmpty()) text += "\n";
                text += "Named: " + customName;
            }
            if (!text.NullOrEmpty())
            {
                text += "\n";
            }
            if (innerContainer.Count == 0)
            {
                text += "Empty";
            }
            else
            {
                text += "Contains: " + innerContainer.ContentsString.CapitalizeFirst();
                // Clarify: MaxStacks = maximum stack GROUPS
                text += $"\nStack slots: {innerContainer.Count}/{MaxStacks}";
                text += $"\nTotal items: {innerContainer.TotalStackCount}";
                int uniqueTypes = innerContainer.GroupBy(t => t.def).Count();
                text += $"\nStack slots: {innerContainer.Count}/{MaxStacks} (unique types: {uniqueTypes})";
            }
            return text;
        }

        public override string Label
        {
            get
            {
                if (!customName.NullOrEmpty())
                {
                    return customName + " (" + def.label + ")";
                }
                return base.Label;
            }
        }
        // Add this method to Building_RimazonLocker class
    }

    
    // Locker Contents Window
    public class Dialog_LockerContents : Window
    {
        private Building_RimazonLocker locker;
        private Vector2 scrollPosition;
        private List<Thing> cachedContents;

        public Dialog_LockerContents(Building_RimazonLocker locker)
        {
            this.locker = locker;
            forcePause = true;
            doCloseX = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;
            CacheContents();
        }

        private void CacheContents()
        {
            cachedContents = new List<Thing>();
            if (locker?.innerContainer != null)
            {
                cachedContents.AddRange(locker.innerContainer);
            }
        }

        public override Vector2 InitialSize => new Vector2(720f, 600f);

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            string title = locker.customName.NullOrEmpty()
                ? "Locker Contents"
                : $"Contents: {locker.customName}";
            Widgets.Label(new Rect(0f, 0f, inRect.width, 35f), title);

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(0f, 40f, inRect.width, 25f),
                $"Stack slots: {locker.innerContainer.Count}/{locker.MaxStacks}");
            Widgets.Label(new Rect(0f, 60f, inRect.width, 25f),
                $"Total items: {locker.innerContainer.TotalStackCount}");

            Rect viewRect = new Rect(0f, 90f, inRect.width, inRect.height - 120f);
            Rect listRect = new Rect(0f, 0f, viewRect.width - 20f, cachedContents.Count * 35f);

            // Draw column headers - USEFUL THREE-COLUMN LAYOUT
            if (cachedContents.Count > 0)
            {
                Rect headerRect = new Rect(viewRect.x, viewRect.y, viewRect.width, 25f);
                GUI.color = Color.gray;
                Widgets.DrawLineHorizontal(headerRect.x, headerRect.y + 24f, headerRect.width);
                GUI.color = Color.white;

                Text.Anchor = TextAnchor.MiddleLeft;
                // Item column (includes quantity in the label)
                Widgets.Label(new Rect(headerRect.x + 30f, headerRect.y, 220f, 25f), "Item");
                // Individual item value
                Widgets.Label(new Rect(headerRect.x + 260f, headerRect.y, 100f, 25f), "Each Value");
                // Total value (item value × quantity)
                Widgets.Label(new Rect(headerRect.x + 370f, headerRect.y, 150f, 25f), "Total Value");
                Text.Anchor = TextAnchor.UpperLeft;
            }

            Widgets.BeginScrollView(viewRect, ref scrollPosition, listRect);
            float y = 25f; // Start below header

            for (int i = 0; i < cachedContents.Count; i++)
            {
                Thing thing = cachedContents[i];
                Rect rowRect = new Rect(0f, y, listRect.width, 32f);

                // Highlight alternate rows
                if (i % 2 == 0)
                {
                    Widgets.DrawLightHighlight(rowRect);
                }

                // Icon
                Widgets.ThingIcon(new Rect(0f, y + 2f, 28f, 28f), thing);

                // Name - LabelCap includes stack count (e.g., "Granite blocks x75")
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(new Rect(30f, y, 230f, 32f), thing.LabelCap);

                // Individual item value (per unit)
                string eachValue = thing.MarketValue.ToStringMoney();
                Widgets.Label(new Rect(260f, y, 110f, 32f), eachValue);

                // Total value (item value × quantity)
                float totalValue = thing.MarketValue * thing.stackCount;
                string totalValueText = totalValue.ToStringMoney();
                Widgets.Label(new Rect(370f, y, 150f, 32f), totalValueText);

                // Info button
                if (Widgets.ButtonImage(new Rect(listRect.width - 24f, y + 4f, 24f, 24f), TexButton.Info))
                {
                    if (thing?.def != null)
                    {
                        // Prefer this version — much less likely to crash
                        Find.WindowStack.Add(new Dialog_InfoCard(thing.def, thing.Stuff));
                    }
                    else
                    {
                        Messages.Message("Cannot show info for this item", MessageTypeDefOf.RejectInput);
                    }
                }

                // Tooltip - shows detailed info including stack count
                string tooltip = thing.GetInspectString();
                if (!string.IsNullOrEmpty(tooltip))
                {
                    TooltipHandler.TipRegion(rowRect, tooltip);
                }

                y += 35f;
            }

            Widgets.EndScrollView();
            Text.Anchor = TextAnchor.UpperLeft;

            // Bottom buttons
            Rect buttonRect = new Rect(0f, inRect.height - 30f, inRect.width, 30f);
            if (Widgets.ButtonText(new Rect(buttonRect.x, buttonRect.y, 150f, 30f), "Eject All"))
            {
                locker.innerContainer.TryDropAll(locker.Position, locker.Map, ThingPlaceMode.Near);
                CacheContents(); // Refresh
            }
        }

        [DebugAction("CAP", "Open locker tab", allowedGameStates = AllowedGameStates.PlayingOnMap)]
        public static void Debug_OpenLockerTab()
        {
            if (Find.Selector.SingleSelectedThing is Building_RimazonLocker locker)
            {
                Find.WindowStack.Add(new Dialog_LockerContents(locker)); // or just select & open tab
                Messages.Message("Locker tab should now be visible", MessageTypeDefOf.TaskCompletion);
            }
        }
    }


    public class ITab_LockerContents : ITab
    {
        private Vector2 scrollPosition = Vector2.zero;

        public ITab_LockerContents()
        {
            size = new Vector2(520f, 480f);
            labelKey = "TabLockerContents";
            tutorTag = "LockerContents";
        }

        protected override void FillTab()
        {
            var locker = SelThing as Building_RimazonLocker;
            if (locker == null || locker.innerContainer == null)
            {
                Text.Font = GameFont.Small;
                Widgets.Label(new Rect(0f, 0f, size.x, size.y).ContractedBy(10f),
                    "Locker not available.\nTry re-selecting the building.");
                return;
            }

            Rect rect = new Rect(0f, 0f, size.x, size.y).ContractedBy(10f);
            var listing = new Listing_Standard();
            listing.Begin(rect);

            // Header
            if (!locker.customName.NullOrEmpty())
            {
                listing.Label($"Locker: {locker.customName}");
                listing.GapLine();
            }

            float totalMass = locker.innerContainer.Sum(t => t.GetStatValue(StatDefOf.Mass) * t.stackCount);
            listing.Label($"Total mass: {totalMass:F2} kg");
            listing.Label($"Stack slots: {locker.innerContainer.Count}/{locker.MaxStacks}");
            listing.Label($"Total items: {locker.innerContainer.TotalStackCount}");
            listing.Gap(12f);

            // Search bar
            Rect searchRect = listing.GetRect(30f);
            string searchString = ""; // You can make this a field if you want persistence
            searchString = Widgets.TextField(searchRect, searchString);

            if (locker.innerContainer.Count == 0)
            {
                listing.Label("(Empty)");
            }
            else
            {
                // Filter items (case-insensitive)
                var filtered = locker.innerContainer
                    .Where(t => t != null && !t.Destroyed && t.LabelCap.ToLowerInvariant().Contains(searchString.ToLowerInvariant()))
                    .ToList();

                // Calculate scroll height dynamically (prevents overflow/clipping buttons)
                float headerUsed = listing.CurHeight;
                float availableForScroll = rect.height - headerUsed - 50f; // leave room for buttons + gaps
                if (availableForScroll < 100f) availableForScroll = 100f; // minimum

                Rect scrollOuter = listing.GetRect(availableForScroll);
                float estimatedContentHeight = filtered.Count * 35f + 20f; // rough per-row height + padding
                Rect viewRect = new Rect(0f, 0f, scrollOuter.width - 16f, Mathf.Max(estimatedContentHeight, availableForScroll));

                Widgets.BeginScrollView(scrollOuter, ref scrollPosition, viewRect);

                try
                {
                    var innerListing = new Listing_Standard();
                    innerListing.Begin(viewRect);

                    foreach (Thing thing in filtered)
                    {
                        Rect row = innerListing.GetRect(28f);

                        // Alternate row highlight
                        if (filtered.IndexOf(thing) % 2 == 0)
                        {
                            Widgets.DrawLightHighlight(row);
                        }

                        Widgets.ThingIcon(new Rect(row.x, row.y, 28f, 28f), thing);

                        Text.Anchor = TextAnchor.MiddleLeft;
                        string label = thing.LabelCap ?? thing.def?.LabelCap ?? "[Unknown Item]";
                        Widgets.Label(new Rect(row.x + 32f, row.y, row.width - 60f, 28f), label);
                        Text.Anchor = TextAnchor.UpperLeft;  // CRITICAL RESET - prevents alignment error

                        // Info button (safe)
                        if (Widgets.ButtonImage(new Rect(row.xMax - 24f, row.y + 2f, 24f, 24f), TexButton.Info))
                        {
                            if (thing?.def != null)
                            {
                                Find.WindowStack.Add(new Dialog_InfoCard(thing.def, thing.Stuff));
                            }
                        }

                        TooltipHandler.TipRegion(row, thing.GetInspectString() ?? label);

                        innerListing.Gap(4f);
                    }

                    innerListing.End();
                }
                finally
                {
                    Widgets.EndScrollView();  // Always close
                }
            }

            listing.Gap(12f);

            // Buttons (now always visible)
            Rect btnRect = listing.GetRect(30f);
            if (Widgets.ButtonText(new Rect(btnRect.x, btnRect.y, btnRect.width / 2 - 5f, 30f), "Eject All"))
            {
                locker.innerContainer.TryDropAll(locker.Position, locker.Map, ThingPlaceMode.Near);
            }

            if (Widgets.ButtonText(new Rect(btnRect.x + btnRect.width / 2 + 5f, btnRect.y, btnRect.width / 2 - 5f, 30f), "Detailed View"))
            {
                Find.WindowStack.Add(new Dialog_LockerContents(locker));
            }

            listing.End();

            // Global safety reset (extra protection)
            Text.Anchor = TextAnchor.UpperLeft;
        }
    }
}

