// Source/Utilities/ChatUtility.cs
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
// along with CAP Chat Interactive. If not, see <https://www.gnu.org/licenses/>.// Part of RICS — custom input/camera helpers (RimWorld systems rule suspended for chat overlay)
// Extensible for future hotkeys, platform-specific focus, etc.

// Part of RICS — custom input/camera/hotkey helpers (RimWorld systems rule suspended for chat overlay)

using System.Linq;                    // ← REQUIRED for OfType + FirstOrDefault
using UnityEngine;
using Verse;                          // ← REQUIRED for Find.WindowStack
using CAP_ChatInteractive.Windows;    // ← REQUIRED so Window_LiveChat is visible from Utilities namespace

namespace CAP_ChatInteractive.Utilities
{
    /// <summary>
    /// Custom utility for live chat input/camera/hotkey passthrough.
    /// Expanded with global Ctrl+V toggle for perfect overlay UX.
    /// </summary>
    public static class ChatUtility
    {
        /// <summary>
        /// Returns true only when the player is actively typing in the chat input field.
        /// </summary>
        public static bool IsChatInputFocused(string controlName = "ChatInput")
        {
            return GUI.GetNameOfFocusedControl() == controlName;
        }

        /// <summary>
        /// True when Ctrl+V is pressed (Left OR Right Ctrl). Cheap and works globally.
        /// </summary>
        public static bool IsToggleHotkeyPressed()
        {
            return (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) &&
                   Input.GetKeyDown(KeyCode.V);
        }

        /// <summary>
        /// Safe toggle using correct RimWorld methods (Remove for instance windows).
        /// </summary>
        public static void ToggleLiveChatWindow()
        {
            var existing = Find.WindowStack.Windows.OfType<Window_LiveChat>().FirstOrDefault();
            if (existing != null)
            {
                Find.WindowStack.TryRemove(existing);
            }
            else
            {
                Find.WindowStack.Add(new Window_LiveChat());
            }
        }
        /// <summary>
        /// Camera blocked ONLY while typing (best UX).
        /// </summary>
        public static bool ShouldPreventCameraMovement()
        {
            return IsChatInputFocused();
        }
    }
}