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
        public int MaxStackSlots => def.GetModExtension<LockerExtension>().maxStacks;  // Renamed for clarit
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
            Logger.Debug($"Checking if locker accepts {thing?.def?.defName} x{thing?.stackCount}");

            // Don't accept pawns via direct interaction
            if (thing is Pawn)
            {
                Logger.Debug($"Rejecting {thing.def.defName}: Cannot accept pawns");
                return false;
            }

            // Check storage settings filter
            if (!settings.filter.Allows(thing))
            {
                Logger.Debug($"Rejecting {thing.def.defName}: Not in filter");
                return false;
            }

            // FIXED: Check STACK capacity, not ITEM capacity
            // We need to see if we're adding a new stack or merging with existing

            // Check if we can merge with existing stack
            Thing existingStack = innerContainer.FirstOrDefault(t => t.def == thing.def && t.CanStackWith(thing));

            if (existingStack != null)
            {
                // Can merge with existing stack - check if existing stack has room
                int spaceInStack = existingStack.def.stackLimit - existingStack.stackCount;
                if (spaceInStack >= thing.stackCount)
                {
                    // Can fit entirely in existing stack - doesn't use a new stack slot
                    Logger.Debug($"Can merge {thing.stackCount} {thing.def.defName} into existing stack (space: {spaceInStack})");
                    return true;
                }
                else
                {
                    // Partial merge - will still use the same stack slot
                    // Stack is already counted, so just check if we can accept any
                    Logger.Debug($"Partial merge possible for {thing.def.defName}");
                    return innerContainer.CanAcceptAnyOf(thing);
                }
            }
            else
            {
                // New stack - check if we have room for another stack group
                if (innerContainer.Count >= MaxStacks)  // FIXED: Check stack count, not item count
                {
                    Logger.Debug($"Rejecting {thing.def.defName}: No room for new stack ({innerContainer.Count}/{MaxStacks} stacks)");
                    return false;
                }

                // Check if container can accept this new stack
                bool canAccept = innerContainer.CanAcceptAnyOf(thing);
                Logger.Debug($"Locker can accept new stack of {thing.def.defName} x{thing.stackCount}: {canAccept}");
                return canAccept;
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
            // Open button to view/access contents
            yield return new Command_Action
            {
                defaultLabel = "Open locker",
                defaultDesc = "View and access items in the locker.",
                icon = ContentFinder<Texture2D>.Get("UI/Commands/RICS_OpenLocker", true), // Reuse vanilla or adjust
                action = () => Open()
            };
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
                    try
                    {
                        Find.WindowStack.Add(new Dialog_InfoCard(thing));
                    }
                    catch (NullReferenceException)
                    {
                        Find.WindowStack.Add(new Dialog_InfoCard(thing.def, thing.Stuff));
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
    }
    // Locker Contents Tab
    public class ITab_LockerContents : ITab
    {
        private Vector2 scrollPosition;
        private float viewHeight;

        public ITab_LockerContents()
        {
            size = new Vector2(480f, 480f);
            labelKey = "TabLockerContents";
            tutorTag = "LockerContents";
        }

        protected override void FillTab()
        {
            Rect rect = new Rect(0f, 0f, size.x, size.y).ContractedBy(10f);
            Rect viewRect = new Rect(0f, 0f, rect.width - 16f, viewHeight);

            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);
            Rect rect2 = new Rect(0f, 0f, viewRect.width, 99999f);
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(rect2);

            var locker = SelThing as Building_RimazonLocker;
            if (locker != null)
            {
                // Header with custom name
                if (!locker.customName.NullOrEmpty())
                {
                    listing.Label($"Locker: {locker.customName}");
                    listing.GapLine();
                }

                // Capacity info
                listing.Label($"Stack slots: {locker.innerContainer.Count}/{locker.MaxStacks}");
                listing.Label($"Total items: {locker.innerContainer.TotalStackCount}");
                listing.Gap(12f);

                // List contents
                if (locker.innerContainer.Count == 0)
                {
                    listing.Label("(Empty)");
                }
                else
                {
                    foreach (Thing thing in locker.innerContainer)
                    {
                        // Draw item row
                        Rect rowRect = listing.GetRect(28f);

                        // Icon
                        Widgets.ThingIcon(new Rect(rowRect.x, rowRect.y, 28f, 28f), thing);

                        // Label with stack count
                        Text.Anchor = TextAnchor.MiddleLeft;
                        Rect labelRect = new Rect(rowRect.x + 32f, rowRect.y, rowRect.width - 32f, 28f);
                        Widgets.Label(labelRect, $"{thing.Label}");
                        Text.Anchor = TextAnchor.UpperLeft;

                        // Info button
                        if (Widgets.ButtonImage(new Rect(rowRect.x + rowRect.width - 24f, rowRect.y, 24f, 24f),
                            TexButton.Info))
                        {
                            try
                            {
                                Find.WindowStack.Add(new Dialog_InfoCard(thing));
                            }
                            catch (NullReferenceException)
                            {
                                Find.WindowStack.Add(new Dialog_InfoCard(thing.def, thing.Stuff));
                            }
                        }

                        // Tooltip
                        TooltipHandler.TipRegion(rowRect, thing.GetInspectString());

                        listing.Gap(2f);
                    }
                }

                // Buttons at bottom
                listing.Gap(12f);
                Rect buttonRect = listing.GetRect(30f);

                // Eject all button
                if (Widgets.ButtonText(new Rect(buttonRect.x, buttonRect.y, buttonRect.width / 2 - 5f, 30f),
                    "EjectAll".Translate()))
                {
                    locker.innerContainer.TryDropAll(locker.Position, locker.Map, ThingPlaceMode.Near);
                }

                // Open detailed view button (optional - connects to existing Dialog_LockerContents)
                if (Widgets.ButtonText(new Rect(buttonRect.x + buttonRect.width / 2 + 5f, buttonRect.y,
                    buttonRect.width / 2 - 5f, 30f), "OpenDetailed".Translate()))
                {
                    Find.WindowStack.Add(new Dialog_LockerContents(locker));
                }
            }

            listing.End();
            if (Event.current.type == EventType.Layout)
            {
                viewHeight = listing.CurHeight;
            }
            Widgets.EndScrollView();
        }
    }
}

