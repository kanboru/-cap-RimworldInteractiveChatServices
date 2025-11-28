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
//
// This class defines the structure for Chat Interactive addons, including their menu class and enabled status.
using System;
using CAP_ChatInteractive.Interfaces;
using Verse;

namespace CAP_ChatInteractive
{
    public class ChatInteractiveAddonDef : Def
    {
        public Type menuClass = typeof(ChatInteractiveAddonMenu);
        public bool enabled = true;
        public int displayOrder = 10;

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            // Logger.Debug($"Resolving AddonDef: {defName}, MenuClass: {menuClass?.Name}");
        }

        public IAddonMenu GetAddonMenu()
        {
            try
            {
                if (!enabled)
                {
                    //Logger.Debug($"AddonDef {defName} is disabled");
                    return null;
                }

                // Logger.Debug($"Creating menu instance for {defName}");
                var menu = Activator.CreateInstance(menuClass) as IAddonMenu;
                // Logger.Debug($"Menu created: {menu != null}");
                return menu;
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to create addon menu for {defName}: {ex.Message}");
                return null;
            }
        }
    }
}