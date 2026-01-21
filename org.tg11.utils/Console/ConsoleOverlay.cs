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
using BepInEx.Logging;
using UnityEngine;

public sealed class ConsoleOverlay
{
    private readonly CommandRegistry _commands;
    private readonly ManualLogSource _log;

    private bool _visible;
    private string _input = "";
    private Vector2 _scroll;

    private readonly List<string> _lines = new();

    public ConsoleOverlay(CommandRegistry commands, ManualLogSource log)
    {
        _commands = commands;
        _log = log;
        Print("TG11 Console ready. Type `help`.");
    }

    public void Toggle() => _visible = !_visible;

    public void Update()
    {
        // Optional: keep input focus behavior here
    }

    public void OnGUI()
    {
        if (!_visible) return;

        const int w = 700;
        const int h = 380;
        var rect = new Rect(20, 20, w, h);

        GUI.Box(rect, "TG11 Terminal");

        GUILayout.BeginArea(new Rect(rect.x + 10, rect.y + 25, rect.width - 20, rect.height - 35));

        _scroll = GUILayout.BeginScrollView(_scroll, GUILayout.ExpandHeight(true));
        foreach (var line in _lines)
            GUILayout.Label(line);
        GUILayout.EndScrollView();

        GUILayout.BeginHorizontal();
        GUI.SetNextControlName("tg11_console_input");
        _input = GUILayout.TextField(_input, GUILayout.ExpandWidth(true));

        if (GUILayout.Button("Send", GUILayout.Width(80)))
            Submit();

        GUILayout.EndHorizontal();

        // Press Enter to submit
        var e = Event.current;
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Return)
        {
            Submit();
            e.Use();
        }

        // Keep focus on input
        GUI.FocusControl("tg11_console_input");

        GUILayout.EndArea();
    }

    private void Submit()
    {
        var text = _input?.Trim();
        if (string.IsNullOrEmpty(text)) return;

        Print($"> {text}");
        _input = "";

        _commands.TryExecute(text, Print);
        _scroll.y = float.MaxValue; // jump to bottom
    }

    public void Print(string msg)
    {
        foreach (var line in msg.Split('\n'))
            _lines.Add(line.TrimEnd('\r'));
        if (_lines.Count > 500) _lines.RemoveRange(0, _lines.Count - 500);
    }

    public void Clear() => _lines.Clear();
}
