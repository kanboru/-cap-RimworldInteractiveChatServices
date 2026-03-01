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
                return "RICS.Research.Error".Translate();
            }
        }

        private static string GetCurrentResearchStatus()
        {
            var researchManager = Find.ResearchManager;
            var currentProject = researchManager.GetProject();  // note: in 1.5 this is .CurrentProject

            if (currentProject == null)
            {
                return "RICS.Research.NoArgsCurrent".Translate();
            }

            // Use defensive defaults
            float progress = Math.Max(0f, currentProject.ProgressApparent);
            float cost = Math.Max(1f, currentProject.CostApparent);   // prevent div-by-zero

            // Prevent NaN / div-by-zero / empty formatting
            if (float.IsNaN(progress) || float.IsInfinity(progress)) progress = 0f;
            if (float.IsNaN(cost) || float.IsInfinity(cost)) cost = 0f;
            if (cost <= 0f) cost = 1f;  // avoid div-by-zero in percent

            float percent = (progress / cost) * 100f;

            // Optional: show at least "0" even if values are tiny
            return "RICS.Research.CurrentStatus".Translate(
                currentProject.LabelCap,
                progress > 0.001f ? progress : 0f,
                cost > 0.001f ? cost : 0f,
                percent >= 0f ? percent : 0f
            );
        }

        private static string GetSpecificResearchStatus(string researchName)
        {
            var allResearch = DefDatabase<ResearchProjectDef>.AllDefs;
            var matchingProjects = allResearch.Where(r =>
                r.LabelCap.ToString().ToLower().Contains(researchName.ToLower()) ||
                r.defName.ToLower().Contains(researchName.ToLower())
            ).ToList();

            if (matchingProjects.Count == 0)
            {
                return "RICS.Research.NoMatch".Translate(researchName);
            }

            if (matchingProjects.Count > 1)
            {
                var projectNames = string.Join(", ", matchingProjects.Take(3).Select(p => p.LabelCap));
                string ellipsis = matchingProjects.Count > 3 ? "RICS.Research.MultipleEllipsis".Translate() : "";
                return "RICS.Research.MultipleMatches".Translate(researchName, projectNames, ellipsis);
            }

            var project = matchingProjects[0];
            var researchManager = Find.ResearchManager;

            if (project.IsFinished)
            {
                return "RICS.Research.Completed".Translate(project.LabelCap);
            }

            float progress = researchManager.GetProgress(project);
            float totalCost = project.CostApparent;
            float percent = totalCost > 0 ? (progress / totalCost) * 100 : 0;

            string status = project.CanStartNow
                ? "RICS.Research.StatusAvailable".Translate()
                : "RICS.Research.StatusLocked".Translate();

            return "RICS.Research.SpecificStatus".Translate(
                project.LabelCap,
                progress,
                totalCost,
                percent,
                status
            );
        }

        internal static string HandleStudyCommand(ChatMessageWrapper messageWrapper, string[] args)
        {
            var research = Find.ResearchManager;
            if (research == null)
                return "RICS.Research.NoResearchManager".Translate();

            var projects = research.CurrentAnomalyKnowledgeProjects
                ?.Select(a => a.project)
                .Where(p => p != null && p.knowledgeCategory != null)
                .ToList();

            if (projects == null || projects.Count == 0)
                return "RICS.Research.NoActiveAnomaly".Translate();

            var basic = projects
                .FirstOrDefault(p => p.knowledgeCategory.overflowCategory == null);

            var advanced = projects
                .FirstOrDefault(p => p.knowledgeCategory.overflowCategory != null);

            string bas = basic != null
                ? "RICS.Research.StudyFormat".Translate(
                    basic.LabelCap,
                    basic.ProgressApparent,
                    basic.CostApparent,
                    basic.ProgressPercent)
                : "RICS.Research.StudyNone".Translate();

            string adv = advanced != null
                ? "RICS.Research.StudyFormat".Translate(
                    advanced.LabelCap,
                    advanced.ProgressApparent,
                    advanced.CostApparent,
                    advanced.ProgressPercent)
                : "RICS.Research.StudyNone".Translate();

            return "RICS.Research.StudyStatus".Translate(bas, adv);
        }
    }
}
