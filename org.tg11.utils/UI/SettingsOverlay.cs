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
using BepInEx.Logging;
using UnityEngine;

public sealed class SettingsOverlay
{
    private readonly ConfigFile _config;
    private readonly ManualLogSource _log;
    private bool _visible;

    public SettingsOverlay(ConfigFile config, ManualLogSource log)
    {
        _config = config;
        _log = log;
    }

    public void Toggle() => _visible = !_visible;

    public void OnGUI()
    {
        if (!_visible) return;

        var rect = new Rect(20, 420, 700, 220);
        GUI.Box(rect, "TG11 Settings (quick view)");

        GUILayout.BeginArea(new Rect(rect.x + 10, rect.y + 25, rect.width - 20, rect.height - 35));
        GUILayout.Label("Use ConfigurationManager for full settings UI.");
        GUILayout.Space(8);

        // Example: show some values you care about
        GUILayout.Label($"Config file: {_config.ConfigFilePath}");

        GUILayout.EndArea();
    }
}
