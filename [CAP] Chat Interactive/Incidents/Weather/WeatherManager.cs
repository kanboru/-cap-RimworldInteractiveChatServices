// WeatherManager.cs
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
// Manages buyable weather types, including loading, saving, and validation.
using LudeonTK;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace CAP_ChatInteractive.Incidents.Weather
{
    public static class BuyableWeatherManager
    {
        public static Dictionary<string, BuyableWeather> AllBuyableWeather { get; private set; } = new Dictionary<string, BuyableWeather>();
        private static bool isInitialized = false;
        private static readonly object lockObject = new object();

//        private static List<TemperatureVariant> temperatureVariants = new List<TemperatureVariant>
//{
//    new TemperatureVariant { BaseWeatherDefName = "RainyThunderstorm", ColdVariantDefName = "SnowyThunderStorm", ThresholdTemperature = 0f },
//    new TemperatureVariant { BaseWeatherDefName = "Rain", ColdVariantDefName = "SnowHard", ThresholdTemperature = 2f }
//};
        public static void InitializeWeather()
        {
            if (isInitialized) return;

            lock (lockObject)
            {
                if (isInitialized) return;


                if (!LoadWeatherFromJson())
                {
                    CreateDefaultWeather();
                    SaveWeatherToJson();
                }
                else
                {
                    ValidateAndUpdateWeather();
                    SaveWeatherToJson();
                }

                isInitialized = true;
                Logger.Message($"[CAP] Buyable Weather System initialized with {AllBuyableWeather.Count} weather types");
            }
        }
        // Loads weather data from JSON file, handling corruption if necessary
        private static bool LoadWeatherFromJson()
        {
            string jsonContent = JsonFileManager.LoadFile("Weather.json");
            if (string.IsNullOrEmpty(jsonContent))
                return false;

            try
            {
                var loadedWeather = JsonFileManager.DeserializeWeather(jsonContent);

                if (loadedWeather == null || loadedWeather.Count == 0)
                {
                    Logger.Error("Weather.json exists but contains no valid data - corrupted");
                    HandleWeatherCorruption("File contains no valid data", jsonContent);
                    return false;
                }

                // ... load into memory ...
                return true;
            }
            catch (Newtonsoft.Json.JsonException jsonEx)
            {
                Logger.Error($"JSON CORRUPTION in Weather.json: {jsonEx.Message}");
                HandleWeatherCorruption($"JSON parsing error: {jsonEx.Message}", jsonContent);
                return false;
            }
            catch (System.Exception e)
            {
                Logger.Error($"Error loading weather JSON: {e.Message}");
                return false;
            }
        }
        // Handles weather data corruption by backing up the corrupted file, notifying the player, and logging details
        private static void HandleWeatherCorruption(string errorDetails, string corruptedJson)
        {
            // Backup corrupted file for debugging
            if (!string.IsNullOrWhiteSpace(corruptedJson))
            {
                try
                {
                    string backupPath = JsonFileManager.GetBackupPath("Weather.json");
                    System.IO.File.WriteAllText(backupPath, corruptedJson);
                    Logger.Debug($"Backed up corrupted Incidents.json to: {backupPath}");
                }
                catch (System.Exception ex)
                {
                    Logger.Error($"Failed to backup corrupted Incidents.json: {ex.Message}");
                }
            }

            // Show in-game notification
            if (Current.ProgramState == ProgramState.Playing)
            {
                string message = "Chat Interactive: Incidents configuration was corrupted.\n" +
                                "Rebuilt with default incidents. Custom settings have been lost.\n" +
                                "Check logs for details.";

                Messages.Message(message, MessageTypeDefOf.NegativeEvent);
            }

            // Log the corrupted content (first 500 chars for debugging)
            if (corruptedJson != null && corruptedJson.Length > 0)
            {
                string preview = corruptedJson.Length > 500 ?
                    corruptedJson.Substring(0, 500) + "..." :
                    corruptedJson;
                Logger.Debug($"Corrupted Incidents JSON preview: {preview}");
            }
        }

        private static void CreateDefaultWeather()
        {
            AllBuyableWeather.Clear();

            var allWeatherDefs = DefDatabase<WeatherDef>.AllDefs.ToList();

            int weatherCreated = 0;
            foreach (var weatherDef in allWeatherDefs)
            {
                try
                {
                    if (!IsWeatherSuitableForStore(weatherDef))
                        continue;

                    string key = GetWeatherKey(weatherDef);
                    if (!AllBuyableWeather.ContainsKey(key))
                    {
                        var buyableWeather = new BuyableWeather(weatherDef);
                        AllBuyableWeather[key] = buyableWeather;
                        weatherCreated++;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Error creating buyable weather for {weatherDef.defName}: {ex.Message}");
                }
            }
        }

        private static bool IsWeatherSuitableForStore(WeatherDef weatherDef)
        {
            string defName = weatherDef.defName.ToLower();

            // Explicitly exclude problematic weather types
            if (defName.Contains("orbit") ||
                defName.Contains("underground") ||
                defName.Contains("undercave") ||
                defName.Contains("unnatural") ||
                defName.Contains("stage") ||
                defName.Contains("metalhell") ||
                // defName.Contains("bloodrain") ||
                defName.Contains("deathpall") ||
                defName.Contains("graypall"))
                return false;

            // Also exclude abstract/base weather definitions
            //if (weatherDef.abstract)  return false;

    // Include everything else that's not excluded
    return true;
        }

        private static string GetWeatherKey(WeatherDef weatherDef)
        {
            return weatherDef.defName;
        }

        private static void ValidateAndUpdateWeather()
        {
            // Similar to incidents validation
            var allWeatherDefs = DefDatabase<WeatherDef>.AllDefs;

            foreach (var weatherDef in allWeatherDefs)
            {
                if (!IsWeatherSuitableForStore(weatherDef))
                    continue;

                string key = GetWeatherKey(weatherDef);
                if (!AllBuyableWeather.ContainsKey(key))
                {
                    var buyableWeather = new BuyableWeather(weatherDef);
                    AllBuyableWeather[key] = buyableWeather;
                }
            }

            var keysToRemove = new List<string>();
            foreach (var kvp in AllBuyableWeather)
            {
                var weatherDef = DefDatabase<WeatherDef>.GetNamedSilentFail(kvp.Key);
                if (weatherDef == null || !IsWeatherSuitableForStore(weatherDef))
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                AllBuyableWeather.Remove(key);
            }
        }

        public static void SaveWeatherToJson()
        {
            LongEventHandler.QueueLongEvent(() =>
            {
                lock (lockObject)
                {
                    try
                    {
                        string jsonContent = JsonFileManager.SerializeWeather(AllBuyableWeather);
                        JsonFileManager.SaveFile("Weather.json", jsonContent);
                    }
                    catch (System.Exception e)
                    {
                        Logger.Error($"Error saving weather JSON: {e.Message}");
                    }
                }
            }, null, false, null, showExtraUIInfo: false, forceHideUI: true);
        }



        [DebugAction("CAP", "Reload Weather", allowedGameStates = AllowedGameStates.Playing)]
        public static void DebugReloadWeather()
        {
            isInitialized = false;
            InitializeWeather();
        }
    }
}