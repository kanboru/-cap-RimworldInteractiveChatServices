// ButtonUtils.cs
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
// Utility methods for modders to easily add buttons to RICS
using CAP_ChatInteractive.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace CAP_ChatInteractive
{
    public static class ButtonUtils
    {
        /// <summary>
        /// Easy method for modders to add a settings button for their mod
        /// </summary>
        public static void AddModSettingsButton(string modName, Type settingsDialogType,
            string buttonLabel = null, string iconPath = null, int displayOrder = 1000)
        {
            if (settingsDialogType == null)
            {
                Logger.Error($"Cannot add settings button for {modName}: settingsDialogType is null");
                return;
            }

            if (!typeof(Window).IsAssignableFrom(settingsDialogType))
            {
                Logger.Error($"Cannot add settings button for {modName}: {settingsDialogType.Name} must inherit from Window");
                return;
            }

            // Create button definition
            var buttonDef = new EnhancedChatInteractiveAddonDef
            {
                defName = $"{modName.Replace(" ", "")}_Settings",
                label = buttonLabel ?? $"{modName} Settings",
                description = $"Open {modName} settings",
                dialogClass = settingsDialogType,
                buttonType = ButtonType.DirectDialogButton,
                sourceMod = modName,
                enabled = true,
                displayOrder = displayOrder,
                showInToolbar = true,
                iconPath = iconPath
            };

            // Register the button
            ToolbarButtonManager.AddToolbarButton(buttonDef);

            Logger.Debug($"Added settings button for mod: {modName}");
        }

        /// <summary>
        /// Add a custom button that opens a dialog directly
        /// </summary>
        public static void AddDirectDialogButton(string modName, string defName, string label,
            Type dialogClass, string description = null, string iconPath = null,
            int displayOrder = 1000, bool showInToolbar = true)
        {
            ValidateDialogType(dialogClass, modName, label);

            var buttonDef = new EnhancedChatInteractiveAddonDef
            {
                defName = $"{modName.Replace(" ", "")}_{defName}",
                label = label,
                description = description ?? $"Open {label}",
                dialogClass = dialogClass,
                buttonType = ButtonType.DirectDialogButton,
                sourceMod = modName,
                enabled = true,
                displayOrder = displayOrder,
                showInToolbar = showInToolbar,
                iconPath = iconPath
            };

            ToolbarButtonManager.AddToolbarButton(buttonDef);
            Logger.Debug($"Added dialog button: {label} for mod: {modName}");
        }

        /// <summary>
        /// Add a button that toggles a window open/closed
        /// </summary>
        public static void AddToggleWindowButton(string modName, string defName, string label,
            Type windowClass, string description = null, string iconPath = null,
            int displayOrder = 1000, bool showInToolbar = true)
        {
            ValidateWindowType(windowClass, modName, label);

            var buttonDef = new EnhancedChatInteractiveAddonDef
            {
                defName = $"{modName.Replace(" ", "")}_{defName}",
                label = label,
                description = description ?? $"Toggle {label} window",
                windowClass = windowClass,
                buttonType = ButtonType.ToggleWindowButton,
                sourceMod = modName,
                enabled = true,
                displayOrder = displayOrder,
                showInToolbar = showInToolbar,
                iconPath = iconPath
            };

            ToolbarButtonManager.AddToolbarButton(buttonDef);
            Logger.Debug($"Added toggle window button: {label} for mod: {modName}");
        }

        /// <summary>
        /// Add a menu button with custom options
        /// </summary>
        public static void AddMenuButton(string modName, string defName, string label,
            Type menuClass, string description = null, string iconPath = null,
            int displayOrder = 1000, bool showInToolbar = true)
        {
            if (menuClass == null)
            {
                Logger.Error($"Cannot add menu button {label} for {modName}: menuClass is null");
                return;
            }

            if (!typeof(IAddonMenu).IsAssignableFrom(menuClass))
            {
                Logger.Error($"Cannot add menu button {label} for {modName}: {menuClass.Name} must implement IAddonMenu");
                return;
            }

            var buttonDef = new EnhancedChatInteractiveAddonDef
            {
                defName = $"{modName.Replace(" ", "")}_{defName}",
                label = label,
                description = description ?? $"Open {label} menu",
                menuClass = menuClass,
                buttonType = ButtonType.MenuButton,
                sourceMod = modName,
                enabled = true,
                displayOrder = displayOrder,
                showInToolbar = showInToolbar,
                iconPath = iconPath
            };

            ToolbarButtonManager.AddToolbarButton(buttonDef);
            Logger.Debug($"Added menu button: {label} for mod: {modName}");
        }

        /// <summary>
        /// Remove all buttons from a specific mod
        /// </summary>
        public static void RemoveAllButtonsFromMod(string modName)
        {
            var buttonsToRemove = ToolbarButtonManager.GetAllToolbarButtons()
                .Where(b => b.sourceMod == modName)
                .ToList();

            foreach (var button in buttonsToRemove)
            {
                ToolbarButtonManager.RemoveToolbarButton(button.defName);
            }

            Logger.Debug($"Removed {buttonsToRemove.Count} buttons from mod: {modName}");
        }

        /// <summary>
        /// Remove a specific button by defName
        /// </summary>
        public static void RemoveButton(string defName)
        {
            ToolbarButtonManager.RemoveToolbarButton(defName);
            Logger.Debug($"Removed button: {defName}");
        }

        /// <summary>
        /// Get all buttons from a specific mod
        /// </summary>
        public static List<EnhancedChatInteractiveAddonDef> GetButtonsFromMod(string modName)
        {
            return ToolbarButtonManager.GetAllToolbarButtons()
                .Where(b => b.sourceMod == modName)
                .ToList();
        }

        /// <summary>
        /// Check if RICS is available for button integration
        /// </summary>
        public static bool IsRICSLoaded()
        {
            return ModLister.GetActiveModWithIdentifier("Captolamia.CAPChatInteractive") != null;
        }

        private static void ValidateDialogType(Type dialogClass, string modName, string label)
        {
            if (dialogClass == null)
            {
                Logger.Error($"Cannot add button {label} for {modName}: dialogClass is null");
                return;
            }

            if (!typeof(Window).IsAssignableFrom(dialogClass))
            {
                Logger.Error($"Cannot add button {label} for {modName}: {dialogClass.Name} must inherit from Window");
            }
        }

        private static void ValidateWindowType(Type windowClass, string modName, string label)
        {
            if (windowClass == null)
            {
                Logger.Error($"Cannot add button {label} for {modName}: windowClass is null");
                return;
            }

            if (!typeof(Window).IsAssignableFrom(windowClass))
            {
                Logger.Error($"Cannot add button {label} for {modName}: {windowClass.Name} must inherit from Window");
            }
        }
    }
}