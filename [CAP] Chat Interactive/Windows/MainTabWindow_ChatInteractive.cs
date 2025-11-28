// MainTabWindow_ChatInteractive.cs
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
// Main tab window for CAP Chat Interactive mod
using RimWorld;
using UnityEngine;
using Verse;

namespace CAP_ChatInteractive.Windows
{
    public class MainTabWindow_ChatInteractive : MainTabWindow
    {
        public MainTabWindow_ChatInteractive()
        {
            // Logger.Debug("MainTabWindow_ChatInteractive constructor called");
        }

        public override void DoWindowContents(Rect inRect)
        {
            // Logger.Debug("MainTabWindow_ChatInteractive.DoWindowContents called");

            var listing = new Listing_Standard();
            listing.Begin(inRect);

            listing.Label("RICS - Quick Menu");
            listing.GapLine();

            // Logger.Debug($"Found {AddonRegistry.AddonDefs.Count} addon defs");

            foreach (var addonDef in AddonRegistry.AddonDefs)
            {
                // Logger.Debug($"Processing addon: {addonDef.defName}");
                if (listing.ButtonText(addonDef.label))
                {
                    var menu = addonDef.GetAddonMenu();
                    if (menu != null)
                    {
                        var options = menu.MenuOptions();
                        if (options != null && options.Count > 0)
                        {
                            Find.WindowStack.Add(new FloatMenu(options));
                        }
                        else
                        {
                            Messages.Message("No menu options available for this addon", MessageTypeDefOf.NeutralEvent);
                        }
                    }
                }
            }

            listing.End();
        }

        public override Vector2 RequestedTabSize => new Vector2(300f, 100f + (AddonRegistry.AddonDefs.Count * 32f));

        // CHANGED: Force right anchor position
        public override MainTabWindowAnchor Anchor => MainTabWindowAnchor.Right;

        public override void PostOpen()
        {
            base.PostOpen();
            Logger.Debug("MainTabWindow_ChatInteractive opened");
        }
    }
}