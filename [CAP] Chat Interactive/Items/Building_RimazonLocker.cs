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

using RimWorld;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;


// This should be unused.
namespace CAP_ChatInteractive
{
    public class Comp_RimazonLocker : ThingComp
    {
        public string customName = null;  // null = use default label

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref customName, "customName");
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            // Rename button (only visible if player owns it / in god mode etc.)
            yield return new Command_Action
            {
                defaultLabel = "Rename locker",
                defaultDesc = "Give this Rimazon locker a unique name for chat deliveries (e.g. 'lipstick').",
                icon = ContentFinder<Texture2D>.Get("UI/Commands/Rename", true),  // Reuse vanilla rename icon if exists, or your own
                action = () =>
                {
                    Find.WindowStack.Add(new Dialog_RenameLocker((Building_RimazonLocker)parent));
                }
            };
        }

        public override string CompInspectStringExtra()
        {
            if (!customName.NullOrEmpty())
            {
                return "Locker name: " + customName;
            }
            return null;
        }
    }

    // Simple rename dialog (like stockpile rename)
    public class Dialog_RenameLocker : Window
    {
        private string curName;
        private Building_RimazonLocker locker;

        public Dialog_RenameLocker(Building_RimazonLocker locker)
        {
            this.locker = locker;
            curName = locker.GetComp<Comp_RimazonLocker>()?.customName ?? "";
            forcePause = true;
            absorbInputAroundWindow = true;
        }

        public override Vector2 InitialSize => new Vector2(400f, 175f);

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(0f, 0f, inRect.width, 30f), "Rename locker (leave blank to reset):");

            curName = Widgets.TextField(new Rect(0f, 40f, inRect.width, 35f), curName);

            if (Widgets.ButtonText(new Rect(15f, inRect.height - 35f - 15f, inRect.width / 2 - 20f, 35f), "OK"))
            {
                var comp = locker.GetComp<Comp_RimazonLocker>();
                if (comp != null)
                {
                    comp.customName = curName.Trim().NullOrEmpty() ? null : curName.Trim();
                }
                Close();
            }

            if (Widgets.ButtonText(new Rect(inRect.width / 2 + 5f, inRect.height - 35f - 15f, inRect.width / 2 - 20f, 35f), "Cancel"))
            {
                Close();
            }
        }
    }
}