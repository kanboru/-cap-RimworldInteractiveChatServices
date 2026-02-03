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
using UnityEngine;
using Verse;
namespace CAP_ChatInteractive
{
    /// <summary>
    /// How we see what our filters are in game
    /// Filters turned off will prevent us from putting items in our locker
    /// </summary>
    public class ITab_ContainerStorage : ITab
    {
        private ThingFilterUI.UIState uiState = new ThingFilterUI.UIState();
        private static readonly Vector2 WinSize = new Vector2(420f, 480f);

        public ITab_ContainerStorage()
        {
            size = WinSize;
            labelKey = "TabStorage";
            tutorTag = "Storage";
        }

        public override bool IsVisible => SelThing is Building_RimazonLocker locker && locker.Spawned;

        protected override void FillTab()
        {
            //GUI.color = Color.blue;
            //Widgets.DrawBox(new Rect(0, 0, size.x, size.y), 2);
            //GUI.color = Color.white;
            try
            {
                var container = SelThing as Building_RimazonLocker;
                if (container == null || !container.Spawned || container.settings == null)
                {
                    Log.Warning("[DEBUG] ITab_ContainerStorage: No valid container");
                    return;
                }

                if (container == null || !container.Spawned || container.settings == null)
                    return;

                // Reset GUI state
                GUI.color = Color.white;
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.UpperLeft;

                Rect mainRect = new Rect(0f, 0f, size.x, size.y).ContractedBy(10f);

                // Draw priority We dont want to change this
                DrawPriority(new Rect(mainRect.x, mainRect.y, mainRect.width, 30f), container.settings);

                // Draw filter (below priority)
                Rect filterRect = new Rect(mainRect.x, mainRect.y + 35f, mainRect.width, mainRect.height - 35f);
                DrawFilter(filterRect, container.settings.filter, container.def.building?.defaultStorageSettings?.filter);
            }
            catch (Exception ex)
            {
                Log.Error($"[DEBUG] ITab_ContainerStorage.FillTab error: {ex}");
            }


        }

        private void DrawPriority(Rect rect, StorageSettings settings)
        {
            // Widgets.Label(rect.LeftHalf(), "Priority".Translate() + ":");

            // Remove Button to prevent Crash
            // Keep Player/Streamer from using the Locker as a storage point
            // prevents pawns from delivering to the box.
            Widgets.Label(rect.LeftHalf(), "Priority".Translate() + ":");
            Widgets.Label(rect.RightHalf(), "Unstored (fixed)");

            //Rect buttonRect = rect.RightHalf();
            //if (Widgets.ButtonText(buttonRect, settings.Priority.ToString()))
            //{
            //    List<FloatMenuOption> options = new List<FloatMenuOption>();
            //    foreach (StoragePriority priority in Enum.GetValues(typeof(StoragePriority)))
            //    {
            //        options.Add(new FloatMenuOption(priority.ToString(), () =>
            //        {
            //            settings.Priority = priority;
            //        }));
            //    }
            //    Find.WindowStack.Add(new FloatMenu(options));
            //}
        }

        private void DrawFilter(Rect rect, ThingFilter filter, ThingFilter parentFilter)
        {
            // Use RimWorld's built-in ThingFilterUI with the UIState
            // can we force "unstored" Here??
            // Or in the xml?
            ThingFilterUI.DoThingFilterConfigWindow(
                rect: rect,
                state: uiState,
                filter: filter,
                parentFilter: parentFilter,
                openMask: 1, // Force count check
                forceHiddenDefs: null,
                forceHiddenFilters: null,
                forceHideHitPointsConfig: false,
                forceHideQualityConfig: false,
                showMentalBreakChanceRange: false,
                suppressSmallVolumeTags: null,
                map: Find.CurrentMap
            );
        }
    }
}

