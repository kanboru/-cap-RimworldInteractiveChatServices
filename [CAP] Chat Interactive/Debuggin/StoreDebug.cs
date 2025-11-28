// StoreDebug.cs
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
// Debugging utilities for the in-game store system
using RimWorld;
using System.Linq;
using Verse;

namespace CAP_ChatInteractive
{
    [StaticConstructorOnStartup]
    public static class StoreDebug
    {
        static StoreDebug()
        {
            // Enable for debugging, disable for release
            // RunStoreDebugTests();
        }

        public static void RunStoreDebugTests()
        {
            Logger.Debug("=== STORE DEBUG TESTS ===");

            // Test store initialization
            Logger.Debug($"Store items count: {Store.StoreInventory.AllStoreItems.Count}");

            // Test category breakdown
            var categories = Store.StoreInventory.AllStoreItems.Values
                .GroupBy(item => item.Category)
                .OrderByDescending(g => g.Count());

            Logger.Debug("Store categories:");
            foreach (var category in categories)
            {
                Logger.Debug($"  {category.Key}: {category.Count()} items");
            }

            // Test specific item lookup
            var testItem = Store.StoreInventory.GetStoreItem("MealSimple");
            if (testItem != null)
            {
                Logger.Debug($"Test item - MealSimple: Price={testItem.BasePrice}, Category={testItem.Category}");
            }

            // Test enabled items
            var enabledItems = Store.StoreInventory.GetEnabledItems();
            Logger.Debug($"Enabled items: {enabledItems.Count()}");

            Logger.Debug("=== END STORE DEBUG TESTS ===");
        }
    }
}