using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace _CAP__Chat_Interactive.Windows.Dialogs
{
    // Add this class to the same file or a new one
    // Update Dialog_EventSetCustomCooldown.cs
    using System;
    using UnityEngine;
    using Verse;

    namespace _CAP__Chat_Interactive.Windows.Dialogs
    {
        // Update Dialog_EventSetCustomCooldown.cs - fix height
        public class Dialog_EventSetCustomCooldown : Window
        {
            private int cooldownDays = 5;
            private string buffer = "5";
            private Action<int, bool> onConfirm;
            private bool applyToFilteredOnly = false;
            private string filterDescription = "";

            // Increased height from 150f to 200f to avoid overlap
            public override Vector2 InitialSize => new Vector2(350f, 300f);

            public Dialog_EventSetCustomCooldown(Action<int, bool> onConfirm, string filterDescription = "")
            {
                this.onConfirm = onConfirm;
                this.filterDescription = filterDescription;
                doCloseButton = true;
                doCloseX = true;
                forcePause = true;
                absorbInputAroundWindow = true;
                closeOnClickedOutside = true;
            }

            public override void DoWindowContents(Rect inRect)
            {
                Text.Font = GameFont.Medium;
                Widgets.Label(new Rect(0f, 10f, inRect.width, 30f), "Set Custom Cooldown");
                Text.Font = GameFont.Small;

                // Cooldown input
                Widgets.Label(new Rect(0f, 45f, inRect.width, 30f), "Cooldown Days (0-1000):");

                Rect inputRect = new Rect(0f, 75f, 100f, 30f);
                Widgets.TextFieldNumeric(inputRect, ref cooldownDays, ref buffer, 0f, 1000f);

                // Apply to filter toggle
                Rect toggleRect = new Rect(0f, 110f, inRect.width, 30f);

                if (!string.IsNullOrEmpty(filterDescription))
                {
                    Widgets.CheckboxLabeled(toggleRect,
                        $"Apply to filtered events only ({filterDescription})",
                        ref applyToFilteredOnly);

                    // Show info about what this means
                    Rect infoRect = new Rect(0f, 140f, inRect.width, 40f);
                    string infoText = applyToFilteredOnly ?
                        "Will only affect events matching current filters" :
                        "Will affect ALL events, regardless of filters";
                    GUI.color = applyToFilteredOnly ? Color.yellow : Color.white;
                    Widgets.Label(infoRect, infoText);
                    GUI.color = Color.white;
                }
                else
                {
                    Widgets.CheckboxLabeled(toggleRect,
                        "Apply to filtered events only",
                        ref applyToFilteredOnly);

                    Rect warningRect = new Rect(0f, 140f, inRect.width, 40f);
                    GUI.color = Color.yellow;
                    Widgets.Label(warningRect, "No active filter - will apply to all events");
                    GUI.color = Color.white;
                }

                // Note: Removed the help text at the bottom since it might still overlap
                // The important info is already in the UI
            }

            public override void PostClose()
            {
                base.PostClose();
                onConfirm?.Invoke(cooldownDays, applyToFilteredOnly);
            }
        }
    }
}
