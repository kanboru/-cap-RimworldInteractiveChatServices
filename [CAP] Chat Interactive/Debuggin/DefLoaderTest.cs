// DefLoaderTest.cs
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
// A static constructor class to test if custom defs are loaded correctly
using RimWorld;
using System.Linq;
using Verse;

namespace CAP_ChatInteractive
{
    [StaticConstructorOnStartup]
    public static class DefLoaderTest
    {
        static DefLoaderTest()
        {
            return; // Disable by default, enable for debugging
            Logger.Debug("=== DEF LOADER TEST ===");

            // Test if our custom def type is recognized
            var ourDefs = DefDatabase<ChatInteractiveAddonDef>.AllDefs;
            Logger.Debug($"ChatInteractiveAddonDef count: {ourDefs.Count()}");

            // Test if we can find our specific def
            var ourDef = DefDatabase<ChatInteractiveAddonDef>.GetNamed("CAPChatInteractive", false);
            if (ourDef != null)
            {
                Logger.Debug($"✅ Found our AddonDef: {ourDef.defName}");
                Logger.Debug($"  Label: {ourDef.label}");
                Logger.Debug($"  MenuClass: {ourDef.menuClass?.Name}");
                Logger.Debug($"  Enabled: {ourDef.enabled}");
            }
            else
            {
                Logger.Debug("❌ Our AddonDef not found!");

                // List all defs of our type to see what's loading
                Logger.Debug("All ChatInteractiveAddonDefs:");
                foreach (var def in ourDefs)
                {
                    Logger.Debug($"  - {def.defName}: {def.label}");
                }
            }

            Logger.Debug("=== END DEF LOADER TEST ===");
        }
    }
}