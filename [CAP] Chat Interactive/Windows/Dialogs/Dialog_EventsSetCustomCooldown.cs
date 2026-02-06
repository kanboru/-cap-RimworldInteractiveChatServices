// Filename: Dialog_EventsSetCustomCooldown.cs
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

// File description: A dialog window for setting a custom cooldown for events,
// with an option to apply it only to filtered events.
// Updated height to avoid overlap issues.

// Translation keys done

using System;
using UnityEngine;
using Verse;

namespace _CAP__Chat_Interactive.Windows.Dialogs
{
    // Update Dialog_EventSetCustomCooldown.cs - fix height
    public class Dialog_EventSetCustomCooldown : Window
    {
        private int cooldownDays = 5;
        private string buffer = "5";
        private Action<int, bool> onConfirm;
        private bool applyToFilteredOnly = false;
        private string filterDescription = "";

        // Increased height from 150f to 200f to avoid overlap
        public override Vector2 InitialSize => new Vector2(350f, 300f);

        public Dialog_EventSetCustomCooldown(Action<int, bool> onConfirm, string filterDescription = "")
        {
            this.onConfirm = onConfirm;
            this.filterDescription = filterDescription;
            doCloseButton = true;
            doCloseX = true;
            forcePause = true;
            absorbInputAroundWindow = true;
            closeOnClickedOutside = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 10f, inRect.width, 30f), "RICS.Label.SetCustomCooldown".Translate());
            Text.Font = GameFont.Small;

            // Cooldown input
            Widgets.Label(new Rect(0f, 45f, inRect.width, 30f), "RICS.Label.CooldownDays".Translate());

            Rect inputRect = new Rect(0f, 75f, 100f, 30f);
            Widgets.TextFieldNumeric(inputRect, ref cooldownDays, ref buffer, 0f, 1000f);

            // Apply to filter toggle
            Rect toggleRect = new Rect(0f, 110f, inRect.width, 30f);

            if (!string.IsNullOrEmpty(filterDescription))
            {
                Widgets.CheckboxLabeled(toggleRect,
                    "RICS.Label.ApplyToFilteredWithDesc".Translate(filterDescription),
                    ref applyToFilteredOnly);

                // Show info about what this means
                Rect infoRect = new Rect(0f, 140f, inRect.width, 40f);
                string infoText = applyToFilteredOnly ?
                    "RICS.Label.FilteredOnlyInfo".Translate() :
                    "RICS.Label.AllEventsInfo".Translate();
                GUI.color = applyToFilteredOnly ? Color.yellow : Color.white;
                Widgets.Label(infoRect, infoText);
                GUI.color = Color.white;
            }
            else
            {
                Widgets.CheckboxLabeled(toggleRect,
                    "RICS.Label.ApplyToFilteredOnly".Translate(),
                    ref applyToFilteredOnly);

                Rect warningRect = new Rect(0f, 140f, inRect.width, 40f);
                GUI.color = Color.yellow;
                Widgets.Label(warningRect, "RICS.Label.NoFilterWarning".Translate());
                GUI.color = Color.white;
            }

            // Note: Removed the help text at the bottom since it might still overlap
            // The important info is already in the UI
        }

        public override void PostClose()
        {
            base.PostClose();
            onConfirm?.Invoke(cooldownDays, applyToFilteredOnly);
        }
    }
}
