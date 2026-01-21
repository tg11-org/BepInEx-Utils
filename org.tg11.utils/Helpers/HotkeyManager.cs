// Copyright (C) 2026 TG11
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;

namespace org.TG11.utils.Helpers
{
    /// <summary>
    /// Centralized hotkey edge-trigger helper:
    /// - Rewired preferred when ready, Unity Input fallback
    /// - Optional "ignore while typing"
    /// - Debounce per hotkey ID
    /// </summary>
    public sealed class HotkeyManager
    {
        private readonly Dictionary<string, float> _nextAllowed = new();

        /// <param name="id">Unique ID for this hotkey action (ex: "console.toggle")</param>
        /// <param name="shortcut">BepInEx KeyboardShortcut</param>
        /// <param name="cooldownSeconds">Debounce time</param>
        /// <param name="allowWhileTyping">If true, hotkey still fires even when an IMGUI TextField is focused</param>
        public bool Pressed(string id, KeyboardShortcut shortcut, float cooldownSeconds = 0.20f, bool allowWhileTyping = false)
        {
            // Typing guard (IMGUI console text field, etc.)
            if (!allowWhileTyping && InputUtil.IsTypingInIMGUI())
                return false;

            // Debounce gate
            var now = Time.realtimeSinceStartup;
            if (_nextAllowed.TryGetValue(id, out var next) && now < next)
                return false;

            // Input check
            if (!IsShortcutDown(shortcut))
                return false;

            _nextAllowed[id] = now + Mathf.Max(0f, cooldownSeconds);
            return true;
        }

        /// <summary>Resets debounce for a given id (useful if you rebind keys or want immediate next press).</summary>
        public void Reset(string id) => _nextAllowed.Remove(id);

        public void ResetAll() => _nextAllowed.Clear();

        private static bool IsShortcutDown(KeyboardShortcut shortcut)
        {
            // Prefer Rewired if it exists and is ready
            if (RewiredInputUtil.IsReady())
            {
                if (!RewiredInputUtil.GetKeyDown(shortcut.MainKey))
                    return false;

                return RewiredInputUtil.ModifiersHeld(shortcut);
            }

            // Fallback to Unity legacy input
            return InputUtil.ShortcutDown(shortcut, ignoreWhileTyping: false);
        }
    }
}
