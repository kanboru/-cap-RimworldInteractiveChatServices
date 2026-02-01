// StoreCommandHelper.cs
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
// Helper methods for store command handling
using _CAP__Chat_Interactive.Utilities;
using CAP_ChatInteractive.Store;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace CAP_ChatInteractive.Commands.CommandHandlers
{
    public static class StoreCommandHelper 
    {
        public static StoreItem GetStoreItemByName(string itemName)
        {
            if (string.IsNullOrWhiteSpace(itemName))
                return null;

            // Clean the item name
            string cleanItemName = itemName.Trim();
            cleanItemName = cleanItemName.TrimEnd('(', '[', '{').TrimStart(')', ']', '}').Trim();

            Logger.Debug($"Looking up store item for: '{itemName}' (cleaned: '{cleanItemName}')");

            // Check if this is a banned race first
            if (IsRaceBannedByName(cleanItemName))
            {
                Logger.Debug($"Item '{cleanItemName}' is a banned race, skipping store lookup");
                return null;
            }
            // Try exact matches first
            var exactMatch = StoreInventory.AllStoreItems.Values
                .FirstOrDefault(item =>
                    item.DefName.Equals(cleanItemName, StringComparison.OrdinalIgnoreCase) ||
                    item.CustomName?.Equals(cleanItemName, StringComparison.OrdinalIgnoreCase) == true);

            if (exactMatch != null)
            {
                Logger.Debug($"Found exact match: {exactMatch.DefName}");
                return exactMatch;
            }

            // Try partial match on thingDef label (case insensitive, whole word)
            var thingDef = DefDatabase<ThingDef>.AllDefs
                .FirstOrDefault(def =>
                    def.label != null &&
                    def.label.Equals(cleanItemName, StringComparison.OrdinalIgnoreCase));

            if (thingDef != null)
            {
                Logger.Debug($"Found via label exact match: {thingDef.defName}");
                return StoreInventory.GetStoreItem(thingDef.defName);
            }

            // Try label without spaces
            thingDef = DefDatabase<ThingDef>.AllDefs
                .FirstOrDefault(def =>
                {
                    if (def.label == null) return false;

                    string labelWithoutSpaces = def.label.Replace(" ", "");
                    return labelWithoutSpaces.Equals(cleanItemName.Replace(" ", ""), StringComparison.OrdinalIgnoreCase);
                });

            if (thingDef != null)
            {
                Logger.Debug($"Found via label without spaces: {thingDef.defName}");
                return StoreInventory.GetStoreItem(thingDef.defName);
            }

            // Try contains match as last resort, but only if we have at least 3 characters
            if (cleanItemName.Length >= 3)
            {
                thingDef = DefDatabase<ThingDef>.AllDefs
                    .FirstOrDefault(def => def.label?.ToLower().Contains(cleanItemName.ToLower()) == true);

                if (thingDef != null)
                {
                    Logger.Debug($"Found via contains match: {thingDef.defName}");
                    return StoreInventory.GetStoreItem(thingDef.defName);
                }
            }

            Logger.Debug($"No store item found for: '{cleanItemName}'");
            return null;
        }

        public static bool CanUserAfford(ChatMessageWrapper user, int price)
        {
            var viewer = Viewers.GetViewer(user);
            return viewer.Coins >= price;
        }

        public static bool HasRequiredResearch(StoreItem storeItem)
        {
            // Get settings from the mod instance
            var settings = CAPChatInteractiveMod.Instance?.Settings?.GlobalSettings;
            if (settings == null)
            {
                Logger.Debug($"HasRequiredResearch: No settings found, allowing purchase");
                return true;
            }

            // If research requirement is disabled, allow purchase
            if (!settings.RequireResearch)
            {
                Logger.Debug($"HasRequiredResearch: Research requirement disabled, allowing purchase");
                return true;
            }

            // Get the thing definition
            var thingDef = DefDatabase<ThingDef>.GetNamedSilentFail(storeItem.DefName);
            if (thingDef == null)
            {
                Logger.Debug($"HasRequiredResearch: ThingDef not found for {storeItem.DefName}, allowing purchase");
                return true;
            }

            // Check research prerequisites
            if (thingDef.researchPrerequisites != null && thingDef.researchPrerequisites.Count > 0)
            {
                foreach (var research in thingDef.researchPrerequisites)
                {
                    if (research != null && !research.IsFinished)
                    {
                        Logger.Debug($"HasRequiredResearch: Research prerequisite {research.defName} not completed for {storeItem.DefName}");
                        return false;
                    }
                }
            }

            // Also check recipe prerequisites if this is a building or complex item
            if (thingDef.recipeMaker != null && thingDef.recipeMaker.researchPrerequisite != null)
            {
                if (!thingDef.recipeMaker.researchPrerequisite.IsFinished)
                {
                    Logger.Debug($"HasRequiredResearch: Recipe research prerequisite {thingDef.recipeMaker.researchPrerequisite.defName} not completed for {storeItem.DefName}");
                    return false;
                }
            }

            Logger.Debug($"HasRequiredResearch: All research prerequisites met for {storeItem.DefName}");
            return true;
        }

        public static bool IsItemTypeValid(StoreItem storeItem, bool requireEquippable, bool requireWearable, bool requireUsable)
        {
            if (requireEquippable && !storeItem.IsEquippable)
                return false;

            if (requireWearable && !storeItem.IsWearable)
                return false;

            if (requireUsable && !storeItem.IsUsable)
                return false;

            return true;
        }

        public static string GetItemTypeDescription(StoreItem storeItem)
        {
            // FIX: Use the correct StoreItem properties that users can toggle
            if (storeItem.IsEquippable) return "equippable";
            if (storeItem.IsWearable) return "wearable";
            if (storeItem.IsUsable) return "usable";
            return "item";
        }

        public static bool IsRaceBanned(ThingDef thingDef)
        {
            if (thingDef?.race == null)
                return false;

            // Ban humanlike races
            if (thingDef.race.Humanlike)
            {
                Logger.Debug($"Banned race detected: {thingDef.defName} (Humanlike)");
                return true;
            }

            // Add other banned race conditions here if needed
            string[] bannedRaces = {
        "Human", "Colonist", "Slave", "Refugee", "Prisoner",
        "Spacer", "Tribal", "Pirate", "Outlander", "Villager"
    };

            if (bannedRaces.Any(race => thingDef.defName.Contains(race) ||
                                       (thingDef.label?.Contains(race) == true)))
            {
                Logger.Debug($"Banned race detected: {thingDef.defName}");
                return true;
            }

            return false;
        }

        public static bool IsRaceBannedByName(string itemName)
        {
            if (string.IsNullOrWhiteSpace(itemName))
                return false;

            // Clean the item name first (using the same logic as GetStoreItemByName)
            string cleanItemName = itemName.Trim();
            cleanItemName = cleanItemName.TrimEnd('(', '[', '{').TrimStart(')', ']', '}').Trim();

            // Try to find if this matches any humanlike race
            var raceDef = RaceUtils.FindRaceByName(cleanItemName);
            if (raceDef != null)
            {
                Logger.Debug($"Banned race detected by name: '{cleanItemName}' -> {raceDef.defName}");
                return true;
            }

            return false;
        }

        public static string FormatCurrencyMessage(int amount, string currencySymbol)
        {
            var settings = CAPChatInteractiveMod.Instance.Settings.GlobalSettings;
            return $"{amount}" + " - " + $"{currencySymbol}";
        }

        // Debug

        // Temporary debug method - add to StoreCommandHelper
        public static void SimpleLockerDebug(Map map)
        {
            if (map == null)
            {
                Logger.Debug("Map is null");
                return;
            }

            // Just list all buildings and see what we find
            Logger.Debug($"=== Simple Debug: All player buildings on map ===");
            int count = 0;
            foreach (var building in map.listerBuildings.allBuildingsColonist)
            {
                count++;
                if (count <= 10) // Only show first 10
                {
                    Logger.Debug($"Building: {building.def.defName} at {building.Position}");
                }
            }
            Logger.Debug($"Total buildings: {count}");
            Logger.Debug($"=== End Debug ===");
        }


    }
}