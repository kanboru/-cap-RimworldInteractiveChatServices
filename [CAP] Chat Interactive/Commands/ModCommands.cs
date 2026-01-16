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
using System.Collections.Generic;
using System.Linq;
using Verse;

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
    // Cleans up orphaned lootbox entries from viewers no longer present in chat.
    // Usage: !cleanlootboxes [orphans|all|dryrun]
    // TODO: Add to command registry
    public class CleanLootboxes : ChatCommand
    {
        public override string Name => "cleanlootboxes";

        public override string Execute(ChatMessageWrapper messageWrapper, string[] args)
        {
            var component = Current.Game?.GetComponent<LootBoxComponent>();
            if (component == null) return "Lootbox system not available.";

            var mode = args.Length > 0 ? args[0].ToLowerInvariant() : "orphans";
            var realViewers = Viewers.All.Select(v => v.Username.ToLowerInvariant()).ToHashSet();

            int cleaned = 0;

            switch (mode)
            {
                case "dry":
                case "dryrun":
                    // Just count, don't delete
                    int wouldClean = component.ViewersLootboxes.Keys.Count(k => !realViewers.Contains(k));
                    return $"Dry run: {wouldClean} orphaned entries would be removed.";

                case "all":
                    // Dangerous – removes everything!
                    if (args.Length < 2 || args[1] != "confirm")
                        return "Dangerous operation! Use !cleanlootboxes all confirm to wipe ALL lootbox data.";

                    component.ViewersLootboxes.Clear();
                    component.ViewersLastSeenDate.Clear();
                    component.ViewersWhoHaveReceivedLootboxesToday.Clear();
                    return "ALL lootbox data has been wiped! (mod action)";

                case "orphans":
                default:
                    // Normal cleanup - remove entries not matching any current viewer
                    foreach (var key in component.ViewersLootboxes.Keys.ToList())
                    {
                        if (!realViewers.Contains(key))
                        {
                            Logger.Warning($"Removing orphaned lootbox entry: {key} ({component.ViewersLootboxes[key]} boxes)");
                            component.ViewersLootboxes.Remove(key);
                            cleaned++;
                        }
                    }

                    // Also clean other dictionaries
                    component.ViewersLastSeenDate.Keys
                        .Where(k => !realViewers.Contains(k))
                        .ToList()
                        .ForEach(k => component.ViewersLastSeenDate.Remove(k));

                    component.ViewersWhoHaveReceivedLootboxesToday.RemoveAll(u => !realViewers.Contains(u));

                    if (cleaned > 0)
                        return $"Cleaned {cleaned} orphaned/invalid lootbox entries.";

                    return "No orphaned entries found.";
            }
        }
    }

    public class CleanViewers : ChatCommand
    {
        public override string Name => "cleanviewers";
        public override string PermissionLevel => "moderator";
        public override string Description => "Clean viewer database. Usage: !cleanviewers [dry|noplat|all]";

        public override string Execute(ChatMessageWrapper messageWrapper, string[] args)
        {
            string mode = args.Length > 0 ? args[0].ToLowerInvariant() : "noplat";

            var assignmentMgr = CAPChatInteractiveMod.GetPawnAssignmentManager();
            var protectedPawnIds = assignmentMgr?.viewerPawnAssignments.Values.ToHashSet() ?? new HashSet<string>();

            int count = 0;
            var removed = new List<string>();

            lock (Viewers._lock)
            {
                var candidates = Viewers.All.ToList();

                foreach (var viewer in candidates)
                {
                    bool shouldRemove = false;

                    switch (mode)
                    {
                        case "dry":
                        case "dryrun":
                            if (viewer.PlatformUserIds.Count == 0)
                                count++;
                            continue;

                        case "noplat":
                        case "noplatform":
                            shouldRemove = viewer.PlatformUserIds.Count == 0;
                            break;

                        case "all":
                            if (args.Length < 2 || args[1] != "really")
                                return "DANGEROUS! Use !cleanviewers all really to wipe EVERY viewer";
                            shouldRemove = true;
                            break;

                        default:
                            return "Unknown mode. Use: dry | noplat | all really";
                    }

                    if (shouldRemove)
                    {
                        // Extra safety nets
                        if (viewer.Coins > 5 || viewer.Karma != 0)
                            continue;

                        if (protectedPawnIds.Contains(viewer.AssignedPawnId))
                            continue;

                        removed.Add(viewer.Username);
                        Viewers.All.Remove(viewer);
                        count++;
                    }
                }
            }

            if (mode == "dry" || mode == "dryrun")
            {
                return $"Dry run: {count} viewers without platform IDs would be removed.";
            }

            if (count > 0)
            {
                Viewers.SaveViewers();
                Logger.Message($"[CleanViewers] Removed {count} viewers: {string.Join(", ", removed)}");
                return $"Removed {count} invalid viewer entr{(count == 1 ? "y" : "ies")}. ({string.Join(", ", removed.Take(5))}... )";
            }

            return "No viewers matching cleanup criteria found.";
        }
    }
}