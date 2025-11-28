// Settings/ChatWindowSettings.cs
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
// A serializable class to hold settings for the chat window
using RimWorld;
using UnityEngine;
using Verse;

namespace CAP_ChatInteractive
{
    public class ChatWindowSettings : IExposable
    {
        public Vector2 DefaultSize = new Vector2(400f, 300f);
        public bool AlwaysOnTop = false;
        public float Opacity = 0.9f;
        public bool ShowTimestamps = false;
        public int MaxMessageHistory = 1000;

        public void ExposeData()
        {
            Scribe_Values.Look(ref DefaultSize, "defaultSize", new Vector2(400f, 300f));
            Scribe_Values.Look(ref AlwaysOnTop, "alwaysOnTop", false);
            Scribe_Values.Look(ref Opacity, "opacity", 0.9f);
            Scribe_Values.Look(ref ShowTimestamps, "showTimestamps", false);
            Scribe_Values.Look(ref MaxMessageHistory, "maxMessageHistory", 1000);
        }
    }
}