// ColorHelper.cs
// Copyright (c) Captolamia. All rights reserved.
// Licensed under the AGPLv3 License. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace CAP_ChatInteractive.Helpers
{
    internal static class ColorHelper
    {
        private static readonly Dictionary<string, Color> ColorDictionary = new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase)
        {
            {"red", Color.red},
            {"green", Color.green},
            {"blue", Color.blue},
            {"yellow", Color.yellow},
            {"orange", new Color(1f, 0.5f, 0f)},
            {"purple", new Color(0.5f, 0f, 0.5f)},
            {"pink", new Color(1f, 0.75f, 0.8f)},
            {"brown", new Color(0.65f, 0.16f, 0.16f)},
            {"black", Color.black},
            {"white", Color.white},
            {"gray", Color.gray},
            {"cyan", Color.cyan},
            {"magenta", Color.magenta},
            {"silver", new Color(0.75f, 0.75f, 0.75f)},
            {"gold", new Color(1f, 0.84f, 0f)},
            {"maroon", new Color(0.5f, 0f, 0f)},
            {"navy", new Color(0f, 0f, 0.5f)},
            {"teal", new Color(0f, 0.5f, 0.5f)},
            {"lime", new Color(0f, 1f, 0f)},
            {"olive", new Color(0.5f, 0.5f, 0f)},
        };

        public static Color? ParseColor(string colorInput)
        {
            if (string.IsNullOrEmpty(colorInput))
                return null;

            // Remove # if present for hex parsing
            string cleanInput = colorInput.Trim().TrimStart('#');

            // Check named colors first
            if (ColorDictionary.TryGetValue(cleanInput, out Color namedColor))
            {
                return namedColor;
            }

            // Try parsing as hex color
            if (ColorUtility.TryParseHtmlString("#" + cleanInput, out Color hexColor))
            {
                return hexColor;
            }

            return null;
        }

        public static Dictionary<string, Color> GetColorDictionary()
        {
            return ColorDictionary;
        }
    }


}