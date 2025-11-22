// Replace the entire TabDrawer_Economy.cs file content with this:
// TabDrawer_Economy.cs
// Copyright (c) Captolamia. All rights reserved.
// Licensed under the AGPLv3 License. See LICENSE file in the project root for full license information.
// Draws the Economy settings tab in the mod settings window
using CAP_ChatInteractive;
using UnityEngine;
using Verse;
using ColorLibrary = CAP_ChatInteractive.ColorLibrary;

namespace _CAP__Chat_Interactive
{
    public static class TabDrawer_Economy
    {
        public static void Draw(Rect rect)
        {
            var settings = CAPChatInteractiveMod.Instance.Settings.GlobalSettings;
            var listing = new Listing_Standard();
            listing.Begin(rect);

            Text.Font = GameFont.Medium;
            GUI.color = ColorLibrary.Orange;
            listing.Label("RICS.Economy.EconomySettingsHeader".Translate());
            GUI.color = ColorLibrary.White;
            Text.Font = GameFont.Small;
            listing.GapLine(6f);

            // Coin Settings
            GUI.color = ColorLibrary.SkyBlue;
            listing.Label("RICS.Economy.CoinEconomyHeader".Translate());
            GUI.color = ColorLibrary.White;
            UIUtilities.NumericField(listing, "RICS.Economy.StartingCoins".Translate(), "RICS.Economy.StartingCoinsDesc".Translate(), ref settings.StartingCoins, 0, 10000);
            UIUtilities.NumericField(listing, "RICS.Economy.BaseCoinReward".Translate(), "RICS.Economy.BaseCoinRewardDesc".Translate(), ref settings.BaseCoinReward, 1, 100);
            UIUtilities.NumericField(listing, "RICS.Economy.SubscriberExtraCoins".Translate(), "RICS.Economy.SubscriberExtraCoinsDesc".Translate(), ref settings.SubscriberExtraCoins, 0, 50);
            UIUtilities.NumericField(listing, "RICS.Economy.VIPExtraCoins".Translate(), "RICS.Economy.VIPExtraCoinsDesc".Translate(), ref settings.VipExtraCoins, 0, 50);
            UIUtilities.NumericField(listing, "RICS.Economy.ModExtraCoins".Translate(), "RICS.Economy.ModExtraCoinsDesc".Translate(), ref settings.ModExtraCoins, 0, 50);

            listing.Gap(12f);

            // Karma Settings
            GUI.color = ColorLibrary.SkyBlue;
            listing.Label("RICS.Economy.KarmaSystemHeader".Translate());
            GUI.color = ColorLibrary.White;
            UIUtilities.NumericField(listing, "RICS.Economy.StartingKarma".Translate(), "RICS.Economy.StartingKarmaDesc".Translate(), ref settings.StartingKarma, 0, 200);

            // Min Karma with validation
            int originalMinKarma = settings.MinKarma;
            UIUtilities.NumericField(listing, "RICS.Economy.MinimumKarma".Translate(), "RICS.Economy.MinimumKarmaDesc".Translate(), ref settings.MinKarma, 0, 200);
            if (settings.MinKarma != originalMinKarma && settings.MinKarma > settings.MaxKarma)
            {
                settings.MinKarma = settings.MaxKarma;
            }

            // Max Karma with validation  
            int originalMaxKarma = settings.MaxKarma;
            UIUtilities.NumericField(listing, "RICS.Economy.MaximumKarma".Translate(), "RICS.Economy.MaximumKarmaDesc".Translate(), ref settings.MaxKarma, 0, 200);
            if (settings.MaxKarma != originalMaxKarma && settings.MaxKarma < settings.MinKarma)
            {
                settings.MaxKarma = settings.MinKarma;
            }

            listing.Gap(12f);

            UIUtilities.NumericField(listing, "RICS.Economy.ActiveViewerMinutes".Translate(), "RICS.Economy.ActiveViewerMinutesDesc".Translate(), ref settings.MinutesForActive, 1, 1440);
            listing.Gap(12f);

            // Currency
            GUI.color = ColorLibrary.SkyBlue;
            listing.Label("RICS.Economy.CurrencyNameHeader".Translate());
            GUI.color = Color.white;
            listing.Gap(6f);

            Rect currencyLabelRect = listing.GetRect(Text.LineHeight);
            UIUtilities.LabelWithDescription(currencyLabelRect, "RICS.Economy.CurrencyNameDesc".Translate(), "RICS.Economy.CurrencyNameExample".Translate());
            // Current value 
            listing.Gap(6f);
            listing.Label(string.Format("RICS.Economy.CurrentCurrencyDisplay".Translate(), settings.CurrencyName));


            // Text entry field
            settings.CurrencyName = listing.TextEntry(settings.CurrencyName).Trim();
            listing.Gap(6f);

            listing.End();
        }
    }
}