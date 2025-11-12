// MilitaryCommands.cs
// Copyright (c) Captolamia. All rights reserved.
// Licensed under the AGPLv3 License. See LICENSE file in the project root for full license information.
//
// Commands for military aid and raids in RimWorld via chat interaction.
using CAP_ChatInteractive.Commands.CommandHandlers;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CAP_ChatInteractive.Commands.ViewerCommands
{
    public class MilitaryAid : ChatCommand
    {
        public override string Name => "militaryaid";

        public override string Execute(ChatMessageWrapper user, string[] args)
        {
            Logger.Debug($"MilitaryAid command executed by {user.Username} with args: [{string.Join(", ", args)}]");

            // Get command settings
            var settings = GetCommandSettings();

            // Parse wager amount if provided, otherwise use default from settings
            int wager = settings.DefaultMilitaryAidWager;
            if (args.Length > 0 && int.TryParse(args[0], out int parsedWager))
            {
                // Clamp between min and max from settings
                wager = Math.Max(settings.MinMilitaryAidWager, Math.Min(settings.MaxMilitaryAidWager, parsedWager));
            }

            return MilitaryAidCommandHandler.HandleMilitaryAid(user, wager);
        }
    }

    public class Raid : ChatCommand
    {
        public override string Name => "raid";

        public override string Execute(ChatMessageWrapper user, string[] args)
        {
            Logger.Debug($"Raid command executed by {user.Username} with args: [{string.Join(", ", args)}]");

            // Get command settings
            var settingsCommand = GetCommandSettings();

            // Use settings defaults
            string raidType = "standard";
            string strategy = "default";
            int wager = settingsCommand.DefaultRaidWager;

            // Use allowed types from settings, fallback to all if empty
            var validRaidTypes = settingsCommand.AllowedRaidTypes.Count > 0
                ? settingsCommand.AllowedRaidTypes
                : new List<string> { "standard", "drop", "dropcenter", "dropedge", "dropchaos", "dropgroups", "mech", "mechcluster", "water", "wateredge" };

            var validStrategies = settingsCommand.AllowedRaidStrategies.Count > 0
                ? settingsCommand.AllowedRaidStrategies
                : new List<string> { "default", "immediate", "smart", "sappers", "breach", "breachsmart", "stage", "siege" };

            // Parse arguments
            foreach (var arg in args)
            {
                if (string.IsNullOrEmpty(arg)) continue;
                string lowerArg = arg.ToLower();

                if (validRaidTypes.Contains(lowerArg))
                {
                    raidType = lowerArg;
                }
                else if (validStrategies.Contains(lowerArg))
                {
                    strategy = lowerArg;
                }
                else if (int.TryParse(arg, out int parsedWager))
                {
                    wager = Math.Max(settingsCommand.MinRaidWager, Math.Min(settingsCommand.MaxRaidWager, parsedWager));
                }
                else
                {
                    return $"Unknown argument: {arg}. Use !raidinfo for available options.";
                }
            }

            // Check if this specific raid type is allowed
            if (!validRaidTypes.Contains(raidType))
            {
                return $"Raid type '{raidType}' is not allowed. Available types: {string.Join(", ", validRaidTypes)}";
            }

            // Check if this strategy is allowed
            if (!validStrategies.Contains(strategy))
            {
                return $"Strategy '{strategy}' is not allowed. Available strategies: {string.Join(", ", validStrategies)}";
            }

            return RaidCommandHandler.HandleRaidCommand(user, raidType, strategy, wager);
        }
    }

    public class RaidInfo : ChatCommand
    {
        public override string Name => "raidinfo";

        public override string Execute(ChatMessageWrapper user, string[] args)
        {
            return "Learn raids: https://tinyurl.com/RaidCommand | Usage: !raid [type] [strategy] [wager]";
        }
    }
}