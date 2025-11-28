// CAPChatInteractive_GameComponent.cs
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
// A game component that handles periodic tasks such as awarding coins to active viewers and managing storyteller ticks.
// Uses an efficient tick system to minimize performance impact.
// Storyteller tick logic can be expanded as needed.
using RimWorld;
using Verse;

namespace CAP_ChatInteractive
{
    public class CAPChatInteractive_GameComponent : GameComponent
    {
        private int tickCounter = 0;
        private const int TICKS_PER_REWARD = 120 * 60; // 2 minutes in ticks (60 ticks/sec * 120 sec)

        public CAPChatInteractive_GameComponent(Game game)
        {
            // Ensure lootbox component exists when this game component is created
            if (game != null && game.components != null)
            {
                var existingLootboxComponent = game.GetComponent<LootBoxComponent>();
                if (existingLootboxComponent == null)
                {
                    game.components.Add(new LootBoxComponent(game));
                    Logger.Debug("LootBoxComponent created by GameComponent");
                }
            }
        }

        public override void GameComponentTick()
        {
            tickCounter++;
            if (tickCounter >= TICKS_PER_REWARD)
            {
                tickCounter = 0;
                Viewers.AwardActiveViewersCoins();

                // Debug logging to verify it's working
                Logger.Debug("2-minute coin reward tick executed - awarded coins to active viewers");
            }
        }
    }
}