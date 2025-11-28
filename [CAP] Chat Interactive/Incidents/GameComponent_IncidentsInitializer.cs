// GameComponent_IncidentsInitializer.cs
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
// Initializes incident and weather systems when a game is loaded or started
using CAP_ChatInteractive.Commands.ViewerCommands;
using RimWorld;
using Verse;

namespace CAP_ChatInteractive.Incidents
{
    public class GameComponent_IncidentsInitializer : GameComponent
    {
        private bool incidentsInitialized = false;
        private bool weatherInitialized = false;

        public GameComponent_IncidentsInitializer(Game game) { }

        public override void LoadedGame()
        {
            InitializeSystems();
        }

        public override void StartedNewGame()
        {
            InitializeSystems();
        }

        public override void GameComponentTick()
        {
            if ((!incidentsInitialized || !weatherInitialized) && Current.ProgramState == ProgramState.Playing)
            {
                InitializeSystems();
            }
        }

        private void InitializeSystems()
        {
            if (!incidentsInitialized)
            {
                IncidentsManager.InitializeIncidents();
                incidentsInitialized = true;
            }

            if (!weatherInitialized)
            {
                Weather.BuyableWeatherManager.InitializeWeather();
                weatherInitialized = true;
            }
        }
    }
}