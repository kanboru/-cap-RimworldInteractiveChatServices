// ViewerCommands.cs
// Copyright (c) Captolamia. All rights reserved.
// Licensed under the AGPLv3 License. See LICENSE file in the project root for full license information.
//
// Commands that viewers can use to interact with the game
using System;
using System.Linq;
using Verse;

namespace CAP_ChatInteractive.Commands.ViewerCommands
{
    internal class ResearchCommandHandler
    {
        internal static string HandleResearchCommand(ChatMessageWrapper messageWrapper, string[] args)
        {
            try
            {
                // If no arguments, show current research
                if (args.Length == 0)
                {
                    return GetCurrentResearchStatus();
                }

                // If arguments provided, search for specific research project
                string researchName = string.Join(" ", args).Trim();
                return GetSpecificResearchStatus(researchName);
            }
            catch (Exception ex)
            {
                Log.Error($"Error in research command: {ex}");
                return "An error occurred while processing the research command.";
            }
        }

        private static string GetCurrentResearchStatus()
        {
            var researchManager = Find.ResearchManager;
            var currentProject = researchManager.GetProject();

            if (currentProject == null)
            {
                return "No research project is currently selected.";
            }

            float progressPercent = researchManager.GetProgress(currentProject);
            float totalCost = currentProject.CostApparent;
            float remainingCost = totalCost - progressPercent;

            return $"Current research: {currentProject.LabelCap} - {progressPercent:F0}/{totalCost:F0} ({progressPercent / totalCost * 100:F1}% complete)";
        }

        private static string GetSpecificResearchStatus(string researchName)
        {
            // Find research project by name
            var allResearch = DefDatabase<ResearchProjectDef>.AllDefs;
            var matchingProjects = allResearch.Where(r =>
                r.LabelCap.ToString().ToLower().Contains(researchName.ToLower()) ||
                r.defName.ToLower().Contains(researchName.ToLower())
            ).ToList();

            if (matchingProjects.Count == 0)
            {
                return $"No research project found matching '{researchName}'. Use !research without arguments to see current project.";
            }

            if (matchingProjects.Count > 1)
            {
                var projectNames = string.Join(", ", matchingProjects.Take(3).Select(p => p.LabelCap));
                return $"Multiple projects match '{researchName}': {projectNames}" + (matchingProjects.Count > 3 ? "..." : "");
            }

            var project = matchingProjects[0];
            var researchManager = Find.ResearchManager;

            if (project.IsFinished)
            {
                return $"{project.LabelCap} - COMPLETED";
            }

            float progress = researchManager.GetProgress(project);
            float totalCost = project.CostApparent;
            float progressPercent = totalCost > 0 ? (progress / totalCost) * 100 : 0;

            string status = project.CanStartNow ? "Available" : "Locked (prerequisites missing)";

            return $"{project.LabelCap} - {progress:F0}/{totalCost:F0} ({progressPercent:F1}% complete) - {status}";
        }
    }
}
