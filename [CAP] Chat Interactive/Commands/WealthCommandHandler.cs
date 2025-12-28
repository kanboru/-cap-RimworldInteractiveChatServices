// ViewerCommands.cs
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
// Commands that viewers can use to interact with the game
using RimWorld;
using System;
using System.Text;
using Verse;

namespace CAP_ChatInteractive.Commands.CommandHandlers
{
    public static class WealthCommandHandler
    {
        public static string HandleWealthCommand(ChatMessageWrapper messageWrapper, string[] args)
        {
            try
            {
                var map = GetCurrentMap();
                if (map == null)
                    return "❌ No active colony found.";

                var wealthWatcher = map.wealthWatcher;

                var report = new StringBuilder();
                report.AppendLine("💰 **Colony Wealth Report**");

                // Total wealth
                report.AppendLine($"**Total Wealth:** {FormatWealth(wealthWatcher.WealthTotal)}");
                report.AppendLine();

                // Breakdown
                report.AppendLine("**Wealth Breakdown:**");
                report.AppendLine($"• Items: {FormatWealth(wealthWatcher.WealthItems)}");
                report.AppendLine($"• Buildings: {FormatWealth(wealthWatcher.WealthBuildings)}");
                report.AppendLine($"• Pawns: {FormatWealth(wealthWatcher.WealthPawns)}");

                // Optional: Raid point calculations (shows what kind of threats this wealth attracts)
                if (args.Length > 0 && args[0].ToLower() == "raid")
                {
                    report.AppendLine();
                    report.AppendLine("**Raid Threat Estimates:**");
                    float raidPoints = CalculateRaidPoints(map);
                    report.AppendLine($"• Raid Points: {raidPoints:F0}");
                    report.AppendLine("*(Higher wealth = stronger raids)*");
                }

                // Optional: Show wealth trend
                if (args.Length > 0 && args[0].ToLower() == "trend")
                {
                    report.AppendLine();
                    report.AppendLine("**Wealth Management Tips:**");
                    report.AppendLine("• Keep wealth growth sustainable");
                    report.AppendLine("• Convert items to useful buildings/defenses");
                    report.AppendLine("• Don't hoard unused valuables");
                }

                return report.ToString();
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in wealth command: {ex}");
                return "❌ Error retrieving colony wealth.";
            }
        }

        private static string FormatWealth(float wealth)
        {
            // Format with thousands separator and no decimals
            return wealth.ToString("N0") + " silver";
        }

        private static Map GetCurrentMap()
        {
            // Get the player's home map
            if (Current.Game == null)
                return null;

            // Try to get the main player map
            foreach (var map in Find.Maps)
            {
                if (map.IsPlayerHome)
                    return map;
            }

            // Fallback: any player-controlled map
            foreach (var map in Find.Maps)
            {
                if (map.ParentFaction == Faction.OfPlayer)
                    return map;
            }

            return null;
        }

        private static float CalculateRaidPoints(Map map)
        {
            try
            {
                // This is a simplified version of RimWorld's raid point calculation
                var wealthWatcher = map.wealthWatcher;
                float wealthFactor = wealthWatcher.WealthTotal / 10000f;

                // Base formula similar to RimWorld's
                float points = 35f + (wealthFactor * 40f);

                // Apply difficulty factor
                var difficulty = Find.Storyteller.difficulty;
                points *= difficulty.threatScale;

                // Apply wealth-independent progression
                var tickManager = Find.TickManager;
                if (tickManager != null)
                {
                    float daysPassed = tickManager.TicksGame / 60000f; // Convert ticks to days
                    points += daysPassed * 1.4f;
                }

                return Math.Max(points, 35f); // Minimum raid points
            }
            catch
            {
                return 0f;
            }
        }
    }
}