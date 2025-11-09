// LootBoxCommandHandler.cs
// Copyright (c) Captolamia. All rights reserved.
// Licensed under the AGPLv3 License. See LICENSE file in the project root for full license information.

using System;
using Verse;

namespace CAP_ChatInteractive.Commands.ViewerCommands
{
    internal class LootBoxCommandHandler
    {
        internal static string HandleLootboxCommand(ChatMessageWrapper user, string[] args)
        {
            var lootboxComponent = Current.Game?.GetComponent<LootBoxComponent>();
            if (lootboxComponent == null)
                return "Lootbox system is not available.";

            // Process viewer message to check for daily lootboxes
            lootboxComponent.ProcessViewerMessage(user.Username);

            if (args.Length > 0 && args[0].ToLower() == "count")
            {
                return HandleLootboxCountCommand(user, lootboxComponent);
            }

            return HandleOpenLootboxCommand(user, lootboxComponent);
        }

        private static string HandleLootboxCountCommand(ChatMessageWrapper user, LootBoxComponent lootboxComponent)
        {
            int count = lootboxComponent.HowManyLootboxesDoesViewerHave(user.Username);
            string plural = count != 1 ? "es" : "";
            return $"@{user.Username} you currently have {count} lootbox{plural}.";
        }

        private static string HandleOpenLootboxCommand(ChatMessageWrapper user, LootBoxComponent lootboxComponent)
        {
            var settings = CAPChatInteractiveMod.Instance?.Settings?.GlobalSettings;
            if (settings == null)
                return "Lootbox settings not available.";

            if (settings.LootBoxForceOpenAllAtOnce)
            {
                return OpenAllLootboxes(user, lootboxComponent, settings);
            }
            else
            {
                return OpenSingleLootbox(user, lootboxComponent, settings);
            }
        }

        private static string OpenSingleLootbox(ChatMessageWrapper user, LootBoxComponent lootboxComponent, CAPGlobalChatSettings settings)
        {
            if (lootboxComponent.HowManyLootboxesDoesViewerHave(user.Username) > 0)
            {
                int coins = Rand.Range(settings.LootBoxRandomCoinRange.min, settings.LootBoxRandomCoinRange.max);
                Viewer viewer = Viewers.GetViewer(user.Username);
                viewer.GiveCoins(coins);
                lootboxComponent.ViewersLootboxes[viewer.Username]--;

                return $"@{user.Username} you open a lootbox and discover: {coins} coins!";
            }
            else
            {
                return $"@{user.Username} you do not have any lootboxes.";
            }
        }

        private static string OpenAllLootboxes(ChatMessageWrapper user, LootBoxComponent lootboxComponent, CAPGlobalChatSettings settings)
        {
            int lootboxCount = lootboxComponent.HowManyLootboxesDoesViewerHave(user.Username);
            if (lootboxCount <= 0)
                return $"@{user.Username} you do not have any lootboxes.";

            int totalCoins = 0;
            for (int i = 0; i < lootboxCount; i++)
            {
                totalCoins += Rand.Range(settings.LootBoxRandomCoinRange.min, settings.LootBoxRandomCoinRange.max);
            }

            Viewer viewer = Viewers.GetViewer(user.Username);
            viewer.GiveCoins(totalCoins);
            lootboxComponent.ViewersLootboxes[viewer.Username] = 0;

            return $"@{user.Username} you open all your lootboxes and discover: {totalCoins} coins!";
        }
    }
}