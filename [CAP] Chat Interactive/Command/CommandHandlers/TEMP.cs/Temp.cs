//// temp cs
//// if you see anything in here ignore it.. 
//// its test code

//// Copyright (c) Captolamia
//// This file is part of CAP Chat Interactive.
//// 
//// CAP Chat Interactive is free software: you can redistribute it and/or modify
//// it under the terms of the GNU Affero General Public License as published
//// by the Free Software Foundation, either version 3 of the License, or
//// (at your option) any later version.
//// 
//// CAP Chat Interactive is distributed in the hope that it will be useful,
//// but WITHOUT ANY WARRANTY; without even the implied warranty of
//// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//// GNU Affero General Public License for more details.
//// 
//// You should have received a copy of the GNU Affero General Public License
//// along with CAP Chat Interactive. If not, see <https://www.gnu.org/licenses/>.





//// Filename: RimazonLocker.cs

//using LudeonTK;
//using RimWorld;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using Verse;
//namespace CAP_ChatInteractive
//{
//    public class LockerExtension : DefModExtension
//    {
//        public int maxStacks = 24;  // Actually means "maximum stack groups/slots"
//    }
//    // Main Class
//    public class Building_RimazonLocker : Building, IThingHolder, IHaulDestination, IStoreSettingsParent
//    {
//        public string customName = null;
//        public ThingOwner innerContainer;
//        public int MaxStacks => def.GetModExtension<LockerExtension>().maxStacks;
//        public StorageSettings settings;

//        // Constructor
//        public Building_RimazonLocker()
//        {
//            innerContainer = new ThingOwner<Thing>(this, LookMode.Deep);
//            settings = new StorageSettings(this);
//        }

//        // === IThingHolder
//        public ThingOwner GetDirectlyHeldThings() => innerContainer;

//        public void GetChildHolders(List<IThingHolder> outChildren)
//        {
//            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
//        }

//        public IThingHolder ParentHolder => this;

//        // === IStoreSettingsParent
//        public bool StorageTabVisible => Spawned && Map != null;

//        public StorageSettings GetStoreSettings() => settings;

//        public StorageSettings GetParentStoreSettings() => def.building?.defaultStorageSettings;

//        public void Notify_SettingsChanged()
//        {
//            // Refresh haul jobs if needed
//        }

//        public override void PostMake()
//        {
//            base.PostMake();

//            if (settings == null)
//            {
//                settings = new StorageSettings(this);
//            }

//            if (def.building?.defaultStorageSettings != null)
//            {
//                settings.CopyFrom(def.building.defaultStorageSettings);
//            }
//        }

//        public override void SpawnSetup(Map map, bool respawningAfterReload)
//        {
//            base.SpawnSetup(map, respawningAfterReload);

//            if (settings == null)
//            {
//                settings = new StorageSettings(this);
//                if (def.building?.defaultStorageSettings != null)
//                {
//                    settings.CopyFrom(def.building.defaultStorageSettings);
//                }
//            }

//            if (innerContainer == null)
//            {
//                innerContainer = new ThingOwner<Thing>(this, LookMode.Deep);
//            }
//        }

//        public override IEnumerable<InspectTabBase> GetInspectTabs()
//        {
//            foreach (var tab in base.GetInspectTabs())
//            {
//                if (tab is ITab_Storage)
//                {
//                    // Skip vanilla storage tab
//                    continue;
//                }
//                yield return tab;
//            }

//            if (Spawned && Map != null)
//            {
//                // Add our custom storage tab (like graves have)
//                yield return new ITab_ContainerStorage();

//                // Add our custom contents tab
//                yield return new ITab_LockerContents();
//            }
//        }
//    }




//    // Locker Contents Window
//    public class Dialog_LockerContents : Window
//    {
//        private Building_RimazonLocker locker;
//        private Vector2 scrollPosition;
//        private List<Thing> cachedContents;

//        public Dialog_LockerContents(Building_RimazonLocker locker)
//        {
//            this.locker = locker;
//            forcePause = true;
//            doCloseX = true;
//            absorbInputAroundWindow = true;
//            closeOnClickedOutside = true;
//            CacheContents();
//        }

//        private void CacheContents()
//        {
//            cachedContents = new List<Thing>();
//            if (locker?.innerContainer != null)
//            {
//                cachedContents.AddRange(locker.innerContainer);
//            }
//        }

//        public override Vector2 InitialSize => new Vector2(720f, 600f);

//        public override void DoWindowContents(Rect inRect)
//        {
//            Text.Font = GameFont.Medium;
//            string title = locker.customName.NullOrEmpty()
//                ? "Locker Contents"
//                : $"Contents: {locker.customName}";
//            Widgets.Label(new Rect(0f, 0f, inRect.width, 35f), title);

//            Text.Font = GameFont.Small;
//            Widgets.Label(new Rect(0f, 40f, inRect.width, 25f),
//                $"Stack slots: {locker.innerContainer.Count}/{locker.MaxStacks}");
//            Widgets.Label(new Rect(0f, 60f, inRect.width, 25f),
//                $"Total items: {locker.innerContainer.TotalStackCount}");

//            Rect viewRect = new Rect(0f, 90f, inRect.width, inRect.height - 120f);
//            Rect listRect = new Rect(0f, 0f, viewRect.width - 20f, cachedContents.Count * 35f);

//            // Draw column headers - USEFUL THREE-COLUMN LAYOUT
//            if (cachedContents.Count > 0)
//            {
//                Rect headerRect = new Rect(viewRect.x, viewRect.y, viewRect.width, 25f);
//                GUI.color = Color.gray;
//                Widgets.DrawLineHorizontal(headerRect.x, headerRect.y + 24f, headerRect.width);
//                GUI.color = Color.white;

//                Text.Anchor = TextAnchor.MiddleLeft;
//                // Item column (includes quantity in the label)
//                Widgets.Label(new Rect(headerRect.x + 30f, headerRect.y, 220f, 25f), "Item");
//                // Individual item value
//                Widgets.Label(new Rect(headerRect.x + 260f, headerRect.y, 100f, 25f), "Each Value");
//                // Total value (item value × quantity)
//                Widgets.Label(new Rect(headerRect.x + 370f, headerRect.y, 150f, 25f), "Total Value");
//                Text.Anchor = TextAnchor.UpperLeft;
//            }

//            Widgets.BeginScrollView(viewRect, ref scrollPosition, listRect);
//            float y = 25f; // Start below header

//            for (int i = 0; i < cachedContents.Count; i++)
//            {
//                Thing thing = cachedContents[i];
//                Rect rowRect = new Rect(0f, y, listRect.width, 32f);

//                // Highlight alternate rows
//                if (i % 2 == 0)
//                {
//                    Widgets.DrawLightHighlight(rowRect);
//                }

//                // Icon
//                Widgets.ThingIcon(new Rect(0f, y + 2f, 28f, 28f), thing);

//                // Name - LabelCap includes stack count (e.g., "Granite blocks x75")
//                Text.Anchor = TextAnchor.MiddleLeft;
//                Widgets.Label(new Rect(30f, y, 230f, 32f), thing.LabelCap);

//                // Individual item value (per unit)
//                string eachValue = thing.MarketValue.ToStringMoney();
//                Widgets.Label(new Rect(260f, y, 110f, 32f), eachValue);

//                // Total value (item value × quantity)
//                float totalValue = thing.MarketValue * thing.stackCount;
//                string totalValueText = totalValue.ToStringMoney();
//                Widgets.Label(new Rect(370f, y, 150f, 32f), totalValueText);

//                // Info button
//                if (Widgets.ButtonImage(new Rect(listRect.width - 24f, y + 4f, 24f, 24f), TexButton.Info))
//                {
//                    if (thing?.def != null)
//                    {
//                        // Prefer this version — much less likely to crash
//                        Find.WindowStack.Add(new Dialog_InfoCard(thing.def, thing.Stuff));
//                    }
//                    else
//                    {
//                        Messages.Message("Cannot show info for this item", MessageTypeDefOf.RejectInput);
//                    }
//                }

//                // Tooltip - shows detailed info including stack count
//                string tooltip = thing.GetInspectString();
//                if (!string.IsNullOrEmpty(tooltip))
//                {
//                    TooltipHandler.TipRegion(rowRect, tooltip);
//                }

//                y += 35f;
//            }

//            Widgets.EndScrollView();
//            Text.Anchor = TextAnchor.UpperLeft;

//            // Bottom buttons
//            Rect buttonRect = new Rect(0f, inRect.height - 30f, inRect.width, 30f);
//            if (Widgets.ButtonText(new Rect(buttonRect.x, buttonRect.y, 150f, 30f), "Eject All"))
//            {
//                locker.innerContainer.TryDropAll(locker.Position, locker.Map, ThingPlaceMode.Near);
//                CacheContents(); // Refresh
//            }
//        }

//        [DebugAction("CAP", "Open locker tab", allowedGameStates = AllowedGameStates.PlayingOnMap)]
//        public static void Debug_OpenLockerTab()
//        {
//            if (Find.Selector.SingleSelectedThing is Building_RimazonLocker locker)
//            {
//                Find.WindowStack.Add(new Dialog_LockerContents(locker)); // or just select & open tab
//                Messages.Message("Locker tab should now be visible", MessageTypeDefOf.TaskCompletion);
//            }
//        }
//    }

//    public class ITab_ContainerStorage : ITab
//    {
//        private Vector2 scrollPosition = Vector2.zero;
//        private static readonly Vector2 WinSize = new Vector2(420f, 480f);

//        public ITab_ContainerStorage()
//        {
//            size = WinSize;
//            labelKey = "TabStorage";
//            tutorTag = "Storage";
//        }

//        protected override void FillTab()
//        {
//            // Get the container
//            var container = SelThing as Building_RimazonLocker;
//            if (container == null || !container.Spawned || container.settings == null)
//                return;

//            // Reset GUI state
//            GUI.color = Color.white;
//            Text.Font = GameFont.Small;
//            Text.Anchor = TextAnchor.UpperLeft;

//            // Main rect
//            Rect mainRect = new Rect(0f, 0f, size.x, size.y).ContractedBy(10f);

//            // Draw storage settings
//            container.settings.filter.Draw(mainRect, out bool changed);

//            if (changed)
//            {
//                container.Notify_SettingsChanged();
//            }
//        }
//    }

//    public class ITab_LockerContents : ITab
//    {
//        private Vector2 scrollPosition = Vector2.zero;
//        private string searchString = "";

//        public ITab_LockerContents()
//        {
//            size = new Vector2(520f, 480f);
//            labelKey = "TabLockerContents";
//            tutorTag = "LockerContents";
//        }

//        protected override void FillTab()
//        {
//            // FIRST: Reset GUI state to prevent message rendering issues
//            GUI.color = Color.white;
//            Text.Anchor = TextAnchor.UpperLeft;
//            Text.Font = GameFont.Small;
//            GUI.skin = null; // Ensure we're using RimWorld's skin

//            var locker = SelThing as Building_RimazonLocker;
//            if (locker == null)
//            {
//                // Use Widgets directly instead of Messages.Message()
//                Widgets.Label(new Rect(10f, 10f, size.x - 20f, 30f), "No locker selected.");
//                return;
//            }

//            // Check if locker is still being built/constructed
//            if (locker.Map == null || !locker.Spawned)
//            {
//                Widgets.Label(new Rect(10f, 10f, size.x - 20f, 30f), "Locker not yet constructed.");
//                return;
//            }

//            // Check if innerContainer is initialized
//            if (locker.innerContainer == null)
//            {
//                Widgets.Label(new Rect(10f, 10f, size.x - 20f, 30f), "Locker initializing...");
//                return;
//            }

//            Rect rect = new Rect(0f, 0f, size.x, size.y).ContractedBy(10f);
//            var listing = new Listing_Standard();
//            listing.Begin(rect);

//            // Header
//            if (!locker.customName.NullOrEmpty())
//            {
//                listing.Label($"Locker: {locker.customName}");
//                listing.GapLine();
//            }

//            float totalMass = locker.innerContainer.Sum(t => t.GetStatValue(StatDefOf.Mass) * t.stackCount);
//            listing.Label($"Total mass: {totalMass:F2} kg");
//            listing.Label($"Stack slots: {locker.innerContainer.Count}/{locker.MaxStacks}");
//            listing.Label($"Total items: {locker.innerContainer.TotalStackCount}");
//            listing.Gap(12f);

//            // Search bar
//            Rect searchRect = listing.GetRect(30f);
//            searchString = Widgets.TextField(searchRect, searchString);

//            if (locker.innerContainer.Count == 0)
//            {
//                listing.Label("(Empty)");
//            }
//            else
//            {
//                // Filter items (case-insensitive)
//                var filtered = locker.innerContainer
//                    .Where(t => t != null && !t.Destroyed && t.LabelCap.ToLowerInvariant().Contains(searchString.ToLowerInvariant()))
//                    .ToList();

//                // Calculate scroll height
//                float headerUsed = listing.CurHeight;
//                float availableForScroll = rect.height - headerUsed - 50f;
//                if (availableForScroll < 100f) availableForScroll = 100f;

//                Rect scrollOuter = listing.GetRect(availableForScroll);
//                float estimatedContentHeight = filtered.Count * 35f + 20f;
//                Rect viewRect = new Rect(0f, 0f, scrollOuter.width - 16f, Mathf.Max(estimatedContentHeight, availableForScroll));

//                Widgets.BeginScrollView(scrollOuter, ref scrollPosition, viewRect);

//                try
//                {
//                    var innerListing = new Listing_Standard();
//                    innerListing.Begin(viewRect);

//                    foreach (Thing thing in filtered)
//                    {
//                        Rect row = innerListing.GetRect(28f);

//                        // Alternate row highlight
//                        if (filtered.IndexOf(thing) % 2 == 0)
//                        {
//                            Widgets.DrawLightHighlight(row);
//                        }

//                        Widgets.ThingIcon(new Rect(row.x, row.y, 28f, 28f), thing);

//                        Text.Anchor = TextAnchor.MiddleLeft;
//                        string label = thing.LabelCap ?? thing.def?.LabelCap ?? "[Unknown Item]";
//                        Widgets.Label(new Rect(row.x + 32f, row.y, row.width - 60f, 28f), label);
//                        Text.Anchor = TextAnchor.UpperLeft;

//                        // Info button - ADD NULL CHECK AND SAFE HANDLING
//                        if (Widgets.ButtonImage(new Rect(row.xMax - 24f, row.y + 2f, 24f, 24f), TexButton.Info))
//                        {
//                            try
//                            {
//                                if (thing?.def != null)
//                                {
//                                    Find.WindowStack.Add(new Dialog_InfoCard(thing.def, thing.Stuff));
//                                }
//                            }
//                            catch (Exception ex)
//                            {
//                                // Log error but don't show message inside tab
//                                Log.Error($"Failed to open info card: {ex.Message}");
//                            }
//                        }

//                        TooltipHandler.TipRegion(row, thing.GetInspectString() ?? label);

//                        innerListing.Gap(4f);
//                    }

//                    innerListing.End();
//                }
//                finally
//                {
//                    Widgets.EndScrollView();
//                    // RESTORE GUI STATE after scroll view
//                    GUI.color = Color.white;
//                    Text.Anchor = TextAnchor.UpperLeft;
//                    Text.Font = GameFont.Small;
//                }
//            }

//            listing.Gap(12f);
//            listing.End();

//            // FINAL GUI state reset
//            GUI.color = Color.white;
//            Text.Anchor = TextAnchor.UpperLeft;
//            Text.Font = GameFont.Small;
//        }
//    }
//}

