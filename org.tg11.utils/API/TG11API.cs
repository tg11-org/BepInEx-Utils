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

using System;
using BepInEx.Logging;
using org.TG11.utils.Core;
using org.TG11.utils.Helpers;

namespace org.TG11.utils.API
{
    /// <summary>
    /// Public, stable API surface for other mods.
    /// Keep this small and backwards-compatible.
    /// </summary>
    public static class TG11API
    {
        public static ManualLogSource Log { get; internal set; }
        public static HotkeyManager Hotkeys { get; internal set; }
        public static ServiceRegistry Services { get; internal set; }
        public static CVarRegistry CVars { get; internal set; }

        // Public debug flag
        public static bool DebugEnabled { get; set; } = false;

        // Optional: notify listeners when debug changes
        public static event Action<bool> DebugChanged;

        public static void SetDebug(bool enabled)
        {
            if (DebugEnabled == enabled) return;
            DebugEnabled = enabled;
            DebugChanged?.Invoke(enabled);
        }

        public static bool IsReady => Log != null && Hotkeys != null && Services != null;
    }
}
