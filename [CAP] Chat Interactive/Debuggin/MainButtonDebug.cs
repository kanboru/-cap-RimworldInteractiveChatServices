// MainButtonDebug.cs
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
// Debugging utility to log information about main buttons in the RimWorld UI
using RimWorld;
using Verse;

namespace CAP_ChatInteractive
{
    [StaticConstructorOnStartup]
    public static class MainButtonDebug
    {
        static MainButtonDebug()
        {
            return; // Disable debug logging by default
            Logger.Debug("=== MAIN BUTTON DEBUG ===");

            // Check if our main button def exists
            var ourButton = DefDatabase<MainButtonDef>.GetNamed("CAPChatInteractive", false);
            if (ourButton != null)
            {
                Logger.Debug($"✅ Found MainButtonDef: {ourButton.defName}");
                Logger.Debug($"  Label: {ourButton.label}");
                Logger.Debug($"  TabWindowClass: {ourButton.tabWindowClass?.Name}");
                Logger.Debug($"  Order: {ourButton.order}");
            }
            else
            {
                Logger.Debug("❌ MainButtonDef 'CAPChatInteractive' not found!");
            }

            // List all main buttons for debugging
            Logger.Debug("All MainButtonDefs:");
            foreach (var button in DefDatabase<MainButtonDef>.AllDefs)
            {
                Logger.Debug($"  - {button.defName} (Order: {button.order}, TabWindow: {button.tabWindowClass?.Name})");
            }

            Logger.Debug("=== END MAIN BUTTON DEBUG ===");
        }
    }
}