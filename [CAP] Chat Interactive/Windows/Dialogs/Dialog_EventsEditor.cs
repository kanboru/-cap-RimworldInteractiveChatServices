using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using CAP_ChatInteractive.Incidents;
using System;

namespace CAP_ChatInteractive
{
    public class Dialog_EventsEditor : Window
    {
        private Vector2 scrollPosition = Vector2.zero;
        private Vector2 categoryScrollPosition = Vector2.zero;
        private string searchQuery = "";
        private string lastSearch = "";
        private EventSortMethod sortMethod = EventSortMethod.Name;
        private bool sortAscending = true;
        private string selectedModSource = "All";
        private string selectedCategory = "All";
        private Dictionary<string, int> modSourceCounts = new Dictionary<string, int>();
        private Dictionary<string, int> categoryCounts = new Dictionary<string, int>();
        private List<BuyableIncident> filteredEvents = new List<BuyableIncident>();
        private Dictionary<string, (int baseCost, string karmaType)> originalSettings = new Dictionary<string, (int, string)>();

        public override Vector2 InitialSize => new Vector2(1200f, 700f);

        public Dialog_EventsEditor()
        {
            doCloseButton = true;
            forcePause = true;
            absorbInputAroundWindow = true;

            BuildModSourceCounts();
            BuildCategoryCounts();
            FilterEvents();
            SaveOriginalSettings();
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (searchQuery != lastSearch || filteredEvents.Count == 0)
            {
                FilterEvents();
            }

            Rect headerRect = new Rect(0f, 0f, inRect.width, 65f);
            DrawHeader(headerRect);

            Rect contentRect = new Rect(0f, 70f, inRect.width, inRect.height - 70f - CloseButSize.y);
            DrawContent(contentRect);
        }

        private void DrawHeader(Rect rect)
        {
            Widgets.BeginGroup(rect);

            // Title row
            Text.Font = GameFont.Medium;
            GUI.color = ColorLibrary.Orange;
            Rect titleRect = new Rect(0f, 0f, 200f, 30f);
            Widgets.Label(titleRect, "Events Editor");

            // Draw underline
            Rect underlineRect = new Rect(titleRect.x, titleRect.yMax - 2f, titleRect.width, 2f);
            Widgets.DrawLineHorizontal(underlineRect.x, underlineRect.y, underlineRect.width);

            Text.Font = GameFont.Small;
            GUI.color = Color.white;

            // Second row for controls
            float controlsY = 35f;

            // Search bar with label
            Rect searchLabelRect = new Rect(0f, controlsY, 60f, 30f);
            Widgets.Label(searchLabelRect, "Search:");

            Rect searchRect = new Rect(65f, controlsY, 200f, 30f);
            searchQuery = Widgets.TextField(searchRect, searchQuery);

            // Sort buttons
            Rect sortRect = new Rect(270f, controlsY, 300f, 30f);
            DrawSortButtons(sortRect);

            // Action buttons
            Rect actionsRect = new Rect(575f, controlsY, 400f, 30f);
            DrawActionButtons(actionsRect);

            Widgets.EndGroup();
        }

        private void DrawSortButtons(Rect rect)
        {
            Widgets.BeginGroup(rect);

            float buttonWidth = 90f;
            float spacing = 5f;
            float x = 0f;

            if (Widgets.ButtonText(new Rect(x, 0f, buttonWidth, 30f), "Name"))
            {
                if (sortMethod == EventSortMethod.Name)
                    sortAscending = !sortAscending;
                else
                    sortMethod = EventSortMethod.Name;
                SortEvents();
            }
            x += buttonWidth + spacing;

            if (Widgets.ButtonText(new Rect(x, 0f, buttonWidth, 30f), "Cost"))
            {
                if (sortMethod == EventSortMethod.Cost)
                    sortAscending = !sortAscending;
                else
                    sortMethod = EventSortMethod.Cost;
                SortEvents();
            }
            x += buttonWidth + spacing;

            if (Widgets.ButtonText(new Rect(x, 0f, buttonWidth, 30f), "Karma"))
            {
                if (sortMethod == EventSortMethod.Karma)
                    sortAscending = !sortAscending;
                else
                    sortMethod = EventSortMethod.Karma;
                SortEvents();
            }

            string sortIndicator = sortAscending ? " ↑" : " ↓";
            Rect indicatorRect = new Rect(x + buttonWidth + 10f, 8f, 50f, 20f);
            Widgets.Label(indicatorRect, sortIndicator);

            Widgets.EndGroup();
        }

        private void DrawActionButtons(Rect rect)
        {
            Widgets.BeginGroup(rect);

            float buttonWidth = 100f;
            float spacing = 5f;
            float x = 0f;

            if (Widgets.ButtonText(new Rect(x, 0f, buttonWidth, 30f), "Reset Prices"))
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(
                    "Reset all event prices to default? This cannot be undone.",
                    () => ResetAllPrices()
                ));
            }
            x += buttonWidth + spacing;

            // Enable by Mod Source
            if (Widgets.ButtonText(new Rect(x, 0f, buttonWidth, 30f), "Enable →"))
            {
                ShowEnableByModSourceMenu();
            }
            x += buttonWidth + spacing;

            // Disable by Mod Source
            if (Widgets.ButtonText(new Rect(x, 0f, buttonWidth, 30f), "Disable →"))
            {
                ShowDisableByModSourceMenu();
            }

            Widgets.EndGroup();
        }

        private void DrawContent(Rect rect)
        {
            // Split into categories (left) and events (right)
            float categoriesWidth = 200f;
            float eventsWidth = rect.width - categoriesWidth - 10f;

            Rect categoriesRect = new Rect(rect.x + 5f, rect.y, categoriesWidth - 10f, rect.height);
            Rect eventsRect = new Rect(rect.x + categoriesWidth + 5f, rect.y, eventsWidth - 10f, rect.height);

            DrawCategoriesList(categoriesRect);
            DrawEventsList(eventsRect);
        }

        private void DrawCategoriesList(Rect rect)
        {
            Widgets.DrawMenuSection(rect);

            Rect headerRect = new Rect(rect.x, rect.y, rect.width, 30f);
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(headerRect, "Categories");
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;

            Rect listRect = new Rect(rect.x + 5f, rect.y + 35f, rect.width - 10f, rect.height - 35f);
            Rect viewRect = new Rect(0f, 0f, listRect.width - 20f, categoryCounts.Count * 30f);

            Widgets.BeginScrollView(listRect, ref categoryScrollPosition, viewRect);
            {
                float y = 0f;
                foreach (var category in categoryCounts.OrderByDescending(kvp => kvp.Value))
                {
                    Rect categoryButtonRect = new Rect(0f, y, listRect.xMax - 21f, 28f);

                    if (selectedCategory == category.Key)
                    {
                        Widgets.DrawHighlightSelected(categoryButtonRect);
                    }
                    else if (Mouse.IsOver(categoryButtonRect))
                    {
                        Widgets.DrawHighlight(categoryButtonRect);
                    }

                    string displayName = category.Key == "All" ? "All" : GetDisplayCategoryName(category.Key);
                    string label = $"{displayName} ({category.Value})";

                    Text.Anchor = TextAnchor.MiddleLeft;
                    if (Widgets.ButtonText(categoryButtonRect, label))
                    {
                        selectedCategory = category.Key;
                        FilterEvents();
                    }
                    Text.Anchor = TextAnchor.UpperLeft;

                    y += 30f;
                }
            }
            Widgets.EndScrollView();
        }

        private void DrawEventsList(Rect rect)
        {
            Widgets.DrawMenuSection(rect);

            Rect headerRect = new Rect(rect.x, rect.y, rect.width, 30f);
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.UpperCenter;
            string headerText = $"Events ({filteredEvents.Count})";
            if (selectedCategory != "All")
                headerText += $" - {GetDisplayCategoryName(selectedCategory)}";
            Widgets.Label(headerRect, headerText);
            Text.Anchor = TextAnchor.UpperLeft;
            Text.Font = GameFont.Small;

            Rect listRect = new Rect(rect.x, rect.y + 35f, rect.width, rect.height - 35f);
            float rowHeight = 90f; // Slightly taller for events

            int firstVisibleIndex = Mathf.FloorToInt(scrollPosition.y / rowHeight);
            int lastVisibleIndex = Mathf.CeilToInt((scrollPosition.y + listRect.height) / rowHeight);
            firstVisibleIndex = Mathf.Clamp(firstVisibleIndex, 0, filteredEvents.Count - 1);
            lastVisibleIndex = Mathf.Clamp(lastVisibleIndex, 0, filteredEvents.Count - 1);

            Rect viewRect = new Rect(0f, 0f, listRect.width - 20f, filteredEvents.Count * rowHeight);

            Widgets.BeginScrollView(listRect, ref scrollPosition, viewRect);
            {
                float y = firstVisibleIndex * rowHeight;
                for (int i = firstVisibleIndex; i <= lastVisibleIndex; i++)
                {
                    Rect eventRect = new Rect(0f, y, viewRect.width, rowHeight - 2f);
                    if (i % 2 == 1)
                    {
                        Widgets.DrawLightHighlight(eventRect);
                    }

                    DrawEventRow(eventRect, filteredEvents[i]);
                    y += rowHeight;
                }
            }
            Widgets.EndScrollView();
        }

        private void DrawEventRow(Rect rect, BuyableIncident incident)
        {
            Widgets.BeginGroup(rect);

            try
            {
                // Left section: Name and description
                Rect infoRect = new Rect(5f, 5f, rect.width - 400f, 80f);
                DrawEventInfo(infoRect, incident);

                // Middle section: Enable toggle and event type
                Rect toggleRect = new Rect(rect.width - 390f, 20f, 150f, 50f);
                DrawEventToggle(toggleRect, incident);

                // Right section: Cost and Karma controls
                Rect controlsRect = new Rect(rect.width - 230f, 10f, 225f, 70f);
                DrawEventControls(controlsRect, incident);
            }
            finally
            {
                Widgets.EndGroup();
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = Color.white;
            }
        }

        private void DrawEventInfo(Rect rect, BuyableIncident incident)
        {
            Widgets.BeginGroup(rect);

            // Event name
            Rect nameRect = new Rect(0f, 0f, rect.width, 28f);
            Text.Font = GameFont.Medium;

            string displayLabel = incident.Label;
            if (!string.IsNullOrEmpty(displayLabel))
            {
                displayLabel = char.ToUpper(displayLabel[0]) + (displayLabel.Length > 1 ? displayLabel.Substring(1) : "");
            }

            Widgets.Label(nameRect, displayLabel);
            Text.Font = GameFont.Small;

            // Description
            Rect descRect = new Rect(0f, 28f, rect.width, 40f);
            string description = incident.Description ?? "No description available";
            if (description.Length > 160)
            {
                description = description.Substring(0, 157) + "...";
            }
            Widgets.Label(descRect, description);

            // Mod source and category
            Rect metaRect = new Rect(0f, 68f, rect.width, 15f);
            string metaInfo = $"{GetDisplayModName(incident.ModSource)} - {GetDisplayCategoryName(incident.CategoryName)}";
            GUI.color = Color.gray;
            Widgets.Label(metaRect, metaInfo);
            GUI.color = Color.white;

            Widgets.EndGroup();
        }

        private void DrawEventToggle(Rect rect, BuyableIncident incident)
        {
            Widgets.BeginGroup(rect);

            Rect toggleRect = new Rect(0f, 0f, rect.width, 30f);
            bool enabledCurrent = incident.Enabled;
            Widgets.CheckboxLabeled(toggleRect, "Enabled", ref enabledCurrent);
            if (enabledCurrent != incident.Enabled)
            {
                incident.Enabled = enabledCurrent;
                IncidentsManager.SaveIncidentsToJson();
            }

            // Event type indicator
            Rect typeRect = new Rect(0f, 35f, rect.width, 20f);
            string typeInfo = $"Type: {incident.KarmaType}";
            GUI.color = GetKarmaTypeColor(incident.KarmaType);
            Widgets.Label(typeRect, typeInfo);
            GUI.color = Color.white;

            Widgets.EndGroup();
        }

        private void DrawEventControls(Rect rect, BuyableIncident incident)
        {
            Widgets.BeginGroup(rect);

            float controlHeight = 25f;
            float spacing = 5f;
            float y = 0f;

            // Cost control
            Rect costRect = new Rect(0f, y, rect.width, controlHeight);
            DrawCostControl(costRect, incident);
            y += controlHeight + spacing;

            // Karma type control
            Rect karmaRect = new Rect(0f, y, rect.width, controlHeight);
            DrawKarmaControl(karmaRect, incident);

            Widgets.EndGroup();
        }

        private void DrawCostControl(Rect rect, BuyableIncident incident)
        {
            Widgets.BeginGroup(rect);

            // Label
            Rect labelRect = new Rect(0f, 0f, 60f, 25f);
            Widgets.Label(labelRect, "Cost:");

            // Cost input
            Rect inputRect = new Rect(65f, 0f, 80f, 25f);
            int costBuffer = incident.BaseCost;
            string stringBuffer = costBuffer.ToString();
            Widgets.TextFieldNumeric(inputRect, ref costBuffer, ref stringBuffer, 0, 1000000);

            if (costBuffer != incident.BaseCost)
            {
                incident.BaseCost = costBuffer;
                IncidentsManager.SaveIncidentsToJson();
            }

            // Reset button
            Rect resetRect = new Rect(150f, 0f, 60f, 25f);
            if (Widgets.ButtonText(resetRect, "Reset"))
            {
                incident.BaseCost = CalculateDefaultCost(incident);
                IncidentsManager.SaveIncidentsToJson();
            }

            Widgets.EndGroup();
        }

        private void DrawKarmaControl(Rect rect, BuyableIncident incident)
        {
            Widgets.BeginGroup(rect);

            // Label for Karma
            Rect labelRect = new Rect(0f, 0f, 60f, 25f);
            Widgets.Label(labelRect, "Karma:");

            // Karma dropdown
            Rect dropdownRect = new Rect(65f, 0f, 100f, 25f);
            if (Widgets.ButtonText(dropdownRect, incident.KarmaType))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>
                {
                    new FloatMenuOption("Good", () => UpdateKarmaType(incident, "Good")),
                    new FloatMenuOption("Bad", () => UpdateKarmaType(incident, "Bad")),
                    new FloatMenuOption("Neutral", () => UpdateKarmaType(incident, "Neutral"))
                };

                Find.WindowStack.Add(new FloatMenu(options));
            }

            Widgets.EndGroup();
        }

        private void UpdateKarmaType(BuyableIncident incident, string karmaType)
        {
            incident.KarmaType = karmaType;
            IncidentsManager.SaveIncidentsToJson();
        }

        private Color GetKarmaTypeColor(string karmaType)
        {
            return karmaType?.ToLower() switch
            {
                "good" => Color.green,
                "bad" => Color.red,
                _ => Color.yellow
            };
        }

        // Bulk operations (similar to weather editor)
        private void ShowEnableByModSourceMenu()
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();

            options.Add(new FloatMenuOption("All Mods", EnableAllEvents));

            foreach (var modSource in modSourceCounts.Keys.Where(k => k != "All").OrderBy(k => k))
            {
                string displayName = GetDisplayModName(modSource);
                options.Add(new FloatMenuOption(displayName, () => EnableEventsByModSource(modSource)));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        private void ShowDisableByModSourceMenu()
        {
            List<FloatMenuOption> options = new List<FloatMenuOption>();

            options.Add(new FloatMenuOption("All Mods", DisableAllEvents));

            foreach (var modSource in modSourceCounts.Keys.Where(k => k != "All").OrderBy(k => k))
            {
                string displayName = GetDisplayModName(modSource);
                options.Add(new FloatMenuOption(displayName, () => DisableEventsByModSource(modSource)));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }

        private void EnableEventsByModSource(string modSource)
        {
            int count = 0;
            foreach (var incident in IncidentsManager.AllBuyableIncidents.Values)
            {
                if (GetDisplayModName(incident.ModSource) == modSource)
                {
                    incident.Enabled = true;
                    count++;
                }
            }

            IncidentsManager.SaveIncidentsToJson();
            FilterEvents();
            Messages.Message($"Enabled {count} events from {GetDisplayModName(modSource)}", MessageTypeDefOf.TaskCompletion);
        }

        private void DisableEventsByModSource(string modSource)
        {
            int count = 0;
            foreach (var incident in IncidentsManager.AllBuyableIncidents.Values)
            {
                if (GetDisplayModName(incident.ModSource) == modSource)
                {
                    incident.Enabled = false;
                    count++;
                }
            }

            IncidentsManager.SaveIncidentsToJson();
            FilterEvents();
            Messages.Message($"Disabled {count} events from {GetDisplayModName(modSource)}", MessageTypeDefOf.TaskCompletion);
        }

        private void EnableAllEvents()
        {
            foreach (var incident in IncidentsManager.AllBuyableIncidents.Values)
            {
                incident.Enabled = true;
            }
            IncidentsManager.SaveIncidentsToJson();
            FilterEvents();
        }

        private void DisableAllEvents()
        {
            foreach (var incident in IncidentsManager.AllBuyableIncidents.Values)
            {
                incident.Enabled = false;
            }
            IncidentsManager.SaveIncidentsToJson();
            FilterEvents();
        }

        private int CalculateDefaultCost(BuyableIncident incident)
        {
            // Use the same logic as in BuyableIncident.SetDefaultPricing
            var incidentDef = DefDatabase<IncidentDef>.GetNamedSilentFail(incident.DefName);
            if (incidentDef != null)
            {
                var tempIncident = new BuyableIncident(incidentDef);
                return tempIncident.BaseCost;
            }
            return 500; // Fallback
        }

        private void BuildModSourceCounts()
        {
            modSourceCounts.Clear();
            modSourceCounts["All"] = IncidentsManager.AllBuyableIncidents.Count;

            foreach (var incident in IncidentsManager.AllBuyableIncidents.Values)
            {
                string displayModSource = GetDisplayModName(incident.ModSource);
                if (modSourceCounts.ContainsKey(displayModSource))
                    modSourceCounts[displayModSource]++;
                else
                    modSourceCounts[displayModSource] = 1;
            }
        }

        private void BuildCategoryCounts()
        {
            categoryCounts.Clear();
            categoryCounts["All"] = IncidentsManager.AllBuyableIncidents.Count;

            foreach (var incident in IncidentsManager.AllBuyableIncidents.Values)
            {
                string displayCategory = GetDisplayCategoryName(incident.CategoryName);
                if (categoryCounts.ContainsKey(displayCategory))
                    categoryCounts[displayCategory]++;
                else
                    categoryCounts[displayCategory] = 1;
            }
        }

        private string GetDisplayModName(string modSource)
        {
            if (modSource == "Core") return "RimWorld";
            if (modSource.Contains(".")) return modSource.Split('.')[0];
            return modSource;
        }

        private string GetDisplayCategoryName(string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName)) return "Uncategorized";
            return categoryName;
        }

        private void FilterEvents()
        {
            lastSearch = searchQuery;
            filteredEvents.Clear();

            var allEvents = IncidentsManager.AllBuyableIncidents.Values.AsEnumerable();

            if (selectedCategory != "All")
            {
                allEvents = allEvents.Where(incident => GetDisplayCategoryName(incident.CategoryName) == selectedCategory);
            }

            if (!string.IsNullOrEmpty(searchQuery))
            {
                string searchLower = searchQuery.ToLower();
                allEvents = allEvents.Where(incident =>
                    incident.Label.ToLower().Contains(searchLower) ||
                    incident.Description.ToLower().Contains(searchLower) ||
                    incident.DefName.ToLower().Contains(searchLower) ||
                    incident.ModSource.ToLower().Contains(searchLower) ||
                    incident.CategoryName.ToLower().Contains(searchLower)
                );
            }

            filteredEvents = allEvents.ToList();
            SortEvents();
        }

        private void SortEvents()
        {
            switch (sortMethod)
            {
                case EventSortMethod.Name:
                    filteredEvents = sortAscending ?
                        filteredEvents.OrderBy(incident => incident.Label).ToList() :
                        filteredEvents.OrderByDescending(incident => incident.Label).ToList();
                    break;
                case EventSortMethod.Cost:
                    filteredEvents = sortAscending ?
                        filteredEvents.OrderBy(incident => incident.BaseCost).ToList() :
                        filteredEvents.OrderByDescending(incident => incident.BaseCost).ToList();
                    break;
                case EventSortMethod.Karma:
                    filteredEvents = sortAscending ?
                        filteredEvents.OrderBy(incident => incident.KarmaType).ThenBy(incident => incident.Label).ToList() :
                        filteredEvents.OrderByDescending(incident => incident.KarmaType).ThenBy(incident => incident.Label).ToList();
                    break;
            }
        }

        private void SaveOriginalSettings()
        {
            originalSettings.Clear();
            foreach (var incident in IncidentsManager.AllBuyableIncidents.Values)
            {
                originalSettings[incident.DefName] = (incident.BaseCost, incident.KarmaType);
            }
        }

        private void ResetAllPrices()
        {
            foreach (var incident in IncidentsManager.AllBuyableIncidents.Values)
            {
                incident.BaseCost = CalculateDefaultCost(incident);
            }
            IncidentsManager.SaveIncidentsToJson();
            FilterEvents();
        }

        public override void PostClose()
        {
            IncidentsManager.SaveIncidentsToJson();
            base.PostClose();
        }
    }

    public enum EventSortMethod
    {
        Name,
        Cost,
        Karma
    }
}