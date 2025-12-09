// Dialog_EventsEditorHelp.cs
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
// A help dialog for the Events Editor
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace CAP_ChatInteractive
{
    public class Dialog_EventsEditorHelp : Window
    {
        private Vector2 scrollPosition = Vector2.zero;

        public override Vector2 InitialSize => new Vector2(800f, 600f);

        public Dialog_EventsEditorHelp()
        {
            doCloseButton = true;
            forcePause = true;
            absorbInputAroundWindow = true;
            resizeable = true;
            draggable = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            // Title
            Rect titleRect = new Rect(0f, 0f, inRect.width, 35f);
            Text.Font = GameFont.Medium;
            GUI.color = ColorLibrary.Orange;
            Widgets.Label(titleRect, "Events Editor Help");
            Text.Font = GameFont.Small;
            GUI.color = Color.white;

            // Content area
            Rect contentRect = new Rect(0f, 40f, inRect.width, inRect.height - 40f - CloseButSize.y);
            DrawHelpContent(contentRect);
        }

        private void DrawHelpContent(Rect rect)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"<b>Events Editor Overview</b>");
            sb.AppendLine($"The Events Editor allows you to configure which incidents (events) are available for purchase via chat commands, and set their prices and karma types.");
            sb.AppendLine($"");

            sb.AppendLine($"<b>Main Interface Sections:</b>");
            sb.AppendLine($"1. <b>Left Panel</b> - Filter events by Mod Source or Category");
            sb.AppendLine($"   • Click the panel header to toggle between Mod Sources and Categories");
            sb.AppendLine($"   • Click any item to filter events");
            sb.AppendLine($"   • Numbers in parentheses show event counts");
            sb.AppendLine($"");

            sb.AppendLine($"2. <b>Right Panel</b> - Event list with editing controls");
            sb.AppendLine($"   • Events are listed with name, source mod, and category");
            sb.AppendLine($"   • Click an event name to view detailed information");
            sb.AppendLine($"   • Use checkboxes to enable/disable events");
            sb.AppendLine($"   • Set custom prices and karma types for each event");
            sb.AppendLine($"");

            sb.AppendLine($"<b>Header Controls:</b>");
            sb.AppendLine($"• <b>Search Bar</b> - Filter events by name, description, or mod");
            sb.AppendLine($"• <b>Sort Buttons</b> - Sort events by Name, Cost, or Karma type");
            sb.AppendLine($"   - Click once to sort by that field");
            sb.AppendLine($"   - Click again to reverse sort order");
            sb.AppendLine($"   - Current sort direction is shown by arrow/icons");
            sb.AppendLine($"");

            sb.AppendLine($"<b>Action Buttons:</b>");
            sb.AppendLine($"• <b>Reset Prices</b> - Reset all event prices to default values");
            sb.AppendLine($"• <b>Enable →</b> - Bulk enable events by mod source");
            sb.AppendLine($"• <b>Disable →</b> - Bulk disable events by mod source");
            sb.AppendLine($"");

            sb.AppendLine($"<b>Event Row Controls:</b>");
            sb.AppendLine($"• <b>Enabled Checkbox</b> - Toggle if event is available for purchase");
            sb.AppendLine($"   - Grayed out if event is not available via commands");
            sb.AppendLine($"• <b>Cost Field</b> - Set the price in points");
            sb.AppendLine($"   - Use the Reset button to restore default price");
            sb.AppendLine($"• <b>Karma Dropdown</b> - Set karma type: Good, Bad, or Neutral");
            sb.AppendLine($"   - Affects player karma when event is purchased");
            sb.AppendLine($"• <b>Type Indicator</b> - Shows current karma type in color");
            sb.AppendLine($"   - Green = Good, Red = Bad, Yellow = Neutral");
            sb.AppendLine($"");

            sb.AppendLine($"<b>Event Availability:</b>");
            sb.AppendLine($"• Some events may show as 'UNAVAILABLE' if they cannot be triggered via commands");
            sb.AppendLine($"• These events are grayed out and cannot be enabled");
            sb.AppendLine($"• Toggle visibility of unavailable events in Settings");
            sb.AppendLine($"");

            sb.AppendLine($"<b>Settings (Gear Icon):</b>");
            sb.AppendLine($"• Configure global settings for the Events system");
            sb.AppendLine($"• Show/Hide unavailable events");
            sb.AppendLine($"• Configure default pricing multipliers");
            sb.AppendLine($"• Set global event caps and cooldowns");
            sb.AppendLine($"");

            sb.AppendLine($"<b>Event Information Window:</b>");
            sb.AppendLine($"• Click any event name to open detailed information");
            sb.AppendLine($"• Shows all incident properties and analysis");
            sb.AppendLine($"• Useful for debugging or understanding event mechanics");
            sb.AppendLine($"");

            sb.AppendLine($"<b>Label Customization:</b>");
            sb.AppendLine($"• Click any event name to open the detailed information window");
            sb.AppendLine($"• Use the label edit box at the top to customize the display name");
            sb.AppendLine($"• Click 'Save' to save your custom label to JSON");
            sb.AppendLine($"• Click 'Reset' to restore the original name from the game files");
            sb.AppendLine($"• Custom labels will appear in the Events Editor and chat store");
            sb.AppendLine($"");

            sb.AppendLine($"<b>Tips:</b>");
            sb.AppendLine($"• Changes are saved automatically when you close the window");
            sb.AppendLine($"• Use bulk operations to quickly enable/disable mods");
            sb.AppendLine($"• Sort events by cost to find expensive or cheap events");
            sb.AppendLine($"• Search supports partial matching (case-insensitive)");
            sb.AppendLine($"• Events with 'UNAVAILABLE' tag won't appear in chat store");

            string fullText = sb.ToString();

            // Calculate text height with proper formatting
            float textHeight = Text.CalcHeight(fullText, rect.width - 30f);
            Rect viewRect = new Rect(0f, 0f, rect.width - 20f, textHeight + 20f);

            // Scroll view with background
            Widgets.DrawMenuSection(rect);
            Widgets.BeginScrollView(new Rect(rect.x + 5f, rect.y + 5f, rect.width - 10f, rect.height - 10f),
                                   ref scrollPosition, viewRect);

            GUI.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            Widgets.Label(new Rect(0f, 0f, viewRect.width, textHeight), fullText);
            GUI.color = Color.white;

            Widgets.EndScrollView();
        }
    }
}