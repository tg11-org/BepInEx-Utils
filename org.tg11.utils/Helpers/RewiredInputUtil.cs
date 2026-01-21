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

using BepInEx.Configuration;
using UnityEngine;

namespace org.TG11.utils.Helpers
{
    public static class RewiredInputUtil
    {
        public static bool IsReady()
        {
            try
            {
                return Rewired.ReInput.isReady && Rewired.ReInput.controllers?.Keyboard != null;
            }
            catch { return false; }
        }

        public static bool GetKeyDown(KeyCode key)
        {
            try
            {
                var kb = Rewired.ReInput.controllers.Keyboard;
                return kb != null && kb.GetKeyDown(key);
            }
            catch { return false; }
        }

        public static bool GetKey(KeyCode key)
        {
            try
            {
                var kb = Rewired.ReInput.controllers.Keyboard;
                return kb != null && kb.GetKey(key);
            }
            catch { return false; }
        }

        public static bool ModifiersHeld(KeyboardShortcut shortcut)
        {
            foreach (var mod in shortcut.Modifiers)
                if (!GetKey(mod))
                    return false;
            return true;
        }
    }
}
