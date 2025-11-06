// ModCommands.cs
// Copyright (c) Captolamia. All rights reserved.
// Licensed under the AGPLv3 License. See LICENSE file in the project root for full license information.
//
// Defines moderator commands for giving coins, setting karma, and toggling coin earning.
using System;

namespace CAP_ChatInteractive.Commands.ModCommands
{
    public class GiveCoins : ChatCommand
    {
        public override string Name => "givecoins";

        public override string Execute(ChatMessageWrapper user, string[] args)
        {
            // Check if we have enough arguments
            if (args.Length < 2)
            {
                return "Usage: !givecoins <viewer> <amount>";
            }

            string targetUsername = args[0];

            // Parse the coin amount
            if (!int.TryParse(args[1], out int coinAmount) || coinAmount <= 0)
            {
                return "Please specify a valid positive number of coins to give.";
            }

            // Get the target viewer
            Viewer target = Viewers.GetViewer(targetUsername);
            if (target == null)
            {
                return $"Viewer '{targetUsername}' not found.";
            }

            // Give coins to the target
            target.GiveCoins(coinAmount);

            // Save the changes
            Viewers.SaveViewers();

            return $"Successfully gave {coinAmount} coins to {target.DisplayName}. They now have {target.GetCoins()} coins.";
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

    public class ToggleCoins : ChatCommand
    {
        public override string Name => "togglecoins";
        public override string Execute(ChatMessageWrapper user, string[] args)
        {
            // TODO: Implement coin toggling logic
            return "Coin toggling functionality coming soon!";
        }
    }

}