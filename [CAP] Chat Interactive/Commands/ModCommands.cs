// ModCommands.cs
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
// Defines moderator commands for giving coins, setting karma, and toggling coin earning.
using System;

namespace CAP_ChatInteractive.Commands.ModCommands
{
    public class GiveCoins : ChatCommand
    {
        public override string Name => "givecoins";

        public override string Execute(ChatMessageWrapper messageWrapper, string[] args)
        {
            // Check if we have enough arguments
            if (args.Length < 2)
            {
                return "Usage: !givecoins <viewer|all> <amount>";
            }

            string target = args[0].ToLowerInvariant();

            // Parse the coin amount
            if (!int.TryParse(args[1], out int coinAmount) || coinAmount <= 0)
            {
                return "Please specify a valid positive number of coins to give.";
            }

            // Handle "all" case
            if (target == "all")
            {
                // Give coins to all viewers
                Viewers.GiveAllViewersCoins(coinAmount);
                return $"Gave {coinAmount:N0} coins to all viewers.";
            }

            // Handle individual viewer case (original logic)
            string targetUsername = args.Length > 0 ? args[0].Replace("@", "") : "";

            Viewer targetViewer = Viewers.GetViewerNoAdd(targetUsername);
            if (targetViewer == null)
            {
                return $"Viewer '{targetUsername}' not found.";
            }

            // Give coins to the target
            targetViewer.GiveCoins(coinAmount);

            // Save the changes
            Viewers.SaveViewers();

            return $"Gave {coinAmount:N0} coins to {targetViewer.DisplayName}. {targetViewer.GetCoins()} now has coins.";
        }
    }

    public class SetKarma : ChatCommand
    {
        public override string Name => "setkarma";

        public override string Execute(ChatMessageWrapper user, string[] args)
        {
            // Check if we have enough arguments
            if (args.Length < 2)
            {
                return "Usage: !setkarma <viewer> <amount>";
            }

            string targetUsername = args[0];

            // Parse the karma amount
            if (!int.TryParse(args[1], out int karmaAmount))
            {
                return "Please specify a valid number for karma.";
            }

            // Get the target viewer
            Viewer target = Viewers.GetViewer(targetUsername);
            if (target == null)
            {
                return $"Viewer '{targetUsername}' not found.";
            }

            // Get current karma for the message
            int oldKarma = target.GetKarma();

            // Set karma (it will automatically clamp to min/max)
            target.SetKarma(karmaAmount);

            // Save the changes
            Viewers.SaveViewers();

            return $"Set {target.DisplayName}'s karma from {oldKarma} to {target.GetKarma()}.";
        }
    }

    public class ToggleStore : ChatCommand
    {
        public override string Name => "togglestore";
        public override string Execute(ChatMessageWrapper user, string[] args)
        {
            // TODO: Implement coin toggling logic
            return "TODO:  Tell Capto to fix this and make it work!";
        }
    }

    public class FixAllPawns : ChatCommand
    {
        public override string Name => "fixallpawns";

        public override string Execute(ChatMessageWrapper messageWrapper, string[] args)
        {
            var assignmentManager = CAPChatInteractiveMod.GetPawnAssignmentManager();
            assignmentManager.FixAllPawnAssignments();
            return $"MOD {messageWrapper.Username} Fix all pawns executed.";
        }
    }

}