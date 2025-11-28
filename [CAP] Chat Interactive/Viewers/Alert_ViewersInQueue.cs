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
using Verse;

namespace CAP_ChatInteractive
{
    public class Alert_ViewersInQueue : Alert
    {
        public Alert_ViewersInQueue()
        {
            this.defaultLabel = "Viewers Waiting in Queue";
            this.defaultPriority = AlertPriority.Medium;
        }

        public override TaggedString GetExplanation()
        {
            var queueManager = CAPChatInteractiveMod.GetPawnAssignmentManager();
            int queueSize = queueManager.GetQueueSize();

            if (queueSize == 0)
                return "No viewers are currently waiting in the pawn queue.";

            return $"{queueSize} viewer{(queueSize > 1 ? "s" : "")} {(queueSize > 1 ? "are" : "is")} waiting in the pawn queue for assignment.\n\nClick to open the Pawn Queue management dialog.";
        }

        public override AlertReport GetReport()
        {
            var queueManager = CAPChatInteractiveMod.GetPawnAssignmentManager();
            int queueSize = queueManager.GetQueueSize();

            // Only show alert if there are viewers in queue
            return queueSize > 0;
        }

        protected override void OnClick()
        {
            // Open the PawnQueue dialog when alert is clicked
            Find.WindowStack.Add(new Dialog_PawnQueue());
        }
    }
}