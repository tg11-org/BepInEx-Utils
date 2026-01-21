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
using System.Collections.Generic;
using System.Text;
using BepInEx.Logging;

public sealed class CommandRegistry
{
    private readonly ManualLogSource _log;

    private readonly Dictionary<string, (string help, Action<string[]> handler)> _commands
        = new(StringComparer.OrdinalIgnoreCase);

    public CommandRegistry(ManualLogSource log) => _log = log;

    public void Register(string name, string help, Action<string[]> handler)
        => _commands[name] = (help, handler);

    public bool TryExecute(string input, Action<string> output)
    {
        var tokens = Tokenize(input);
        if (tokens.Count == 0) return false;

        var cmd = tokens[0];
        tokens.RemoveAt(0);

        if (!_commands.TryGetValue(cmd, out var entry))
        {
            output($"Unknown command: {cmd}. Try: help");
            return false;
        }

        try
        {
            entry.handler(tokens.ToArray());
            return true;
        }
        catch (Exception ex)
        {
            _log.LogError(ex);
            output($"Error: {ex.GetType().Name}: {ex.Message}");
            return false;
        }
    }

    public string HelpText()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Commands:");
        foreach (var kv in _commands)
            sb.AppendLine($" - {kv.Key}: {kv.Value.help}");
        return sb.ToString();
    }

    // Very small tokenizer supporting quotes
    private static List<string> Tokenize(string input)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];
            if (c == '"')
            {
                inQuotes = !inQuotes;
                continue;
            }

            if (!inQuotes && char.IsWhiteSpace(c))
            {
                if (sb.Length > 0) { result.Add(sb.ToString()); sb.Clear(); }
                continue;
            }

            sb.Append(c);
        }

        if (sb.Length > 0) result.Add(sb.ToString());
        return result;
    }
}
