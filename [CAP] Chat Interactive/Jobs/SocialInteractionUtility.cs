// SocialInteractionUtility.cs

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
using RimWorld;
using System.Linq;
using Verse;
using Verse.AI;

namespace CAP_ChatInteractive.Utilities
{
    public static class SocialInteractionUtility
    {
        public static bool TryStartSocialVisit(Pawn initiator, Pawn target, InteractionDef interaction, out string failReason)
        {
            failReason = null;

            if (!CanPawnsInteract(initiator, target))
            {
                failReason = $"{initiator.Name} cannot interact with {target.Name}";
                return false;
            }

            if (!CanTargetInteractNow(target))
            {
                failReason = GetUnavailableReason(target);
                return false;
            }

            Job socialJob = JobMaker.MakeJob(JobDefOf_CAP.CAP_SocialVisit, target);
            socialJob.interaction = interaction;
            initiator.jobs.StartJob(socialJob, JobCondition.InterruptForced);

            return true;
        }

        private static bool CanPawnsInteract(Pawn initiator, Pawn target)
        {
            return initiator != null && target != null &&
                   !initiator.Dead && !target.Dead &&
                   initiator.Spawned && target.Spawned &&
                   !initiator.Downed && !target.Downed;
        }

        private static bool CanTargetInteractNow(Pawn target)
        {
            if (target.jobs.curDriver != null && target.jobs.curDriver.asleep) return false;
            if (target.CurJob != null && target.CurJob.def == JobDefOf.LayDown) return false;
            if (target.InMentalState) return false;
            if (target.Drafted) return false; // Maybe allow interactions with drafted pawns?

            return true;
        }

        private static string GetUnavailableReason(Pawn target)
        {
            if (target.jobs.curDriver != null && target.jobs.curDriver.asleep)
                return $"{target.Name} is sleeping";
            if (target.CurJob != null && target.CurJob.def == JobDefOf.LayDown)
                return $"{target.Name} is resting";
            if (target.InMentalState)
                return $"{target.Name} is in a mental break";
            if (target.Drafted)
                return $"{target.Name} is drafted";

            return $"{target.Name} is unavailable";
        }
    }
}