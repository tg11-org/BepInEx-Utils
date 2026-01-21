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

using System.Collections;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;

using org.TG11.utils.Core;
using org.TG11.utils.Helpers;
using org.TG11.utils.API;

namespace org.TG11.utils
{
    [BepInPlugin("org.tg11.utils", "TG11 utils", "0.0.2")]
    public class TG11_utils : BaseUnityPlugin
    {
        private const string HostName = "TG11_UtilsHost";

        private ConfigEntry<bool> _debugEnabled;

        internal static TG11_utils Instance { get; private set; }
        internal static ManualLogSource Log { get; private set; }

        internal ServiceRegistry Services { get; private set; }

        internal CommandRegistry Commands { get; private set; }
        internal ConsoleOverlay Console { get; private set; }
        internal SettingsOverlay Settings { get; private set; }
        internal HotkeyManager Hotkeys { get; private set; }


        private ConfigEntry<KeyboardShortcut> _toggleConsoleKey;
        private ConfigEntry<KeyboardShortcut> _toggleSettingsKey;

        private static org.TG11.utils.Core.TG11Host EnsureHost()
        {
            var existing = GameObject.Find(HostName);
            if (existing != null)
                return existing.GetComponent<org.TG11.utils.Core.TG11Host>() ?? existing.AddComponent<org.TG11.utils.Core.TG11Host>();

            var go = new GameObject(HostName);
            return go.AddComponent<org.TG11.utils.Core.TG11Host>();
        }


        private void Awake()
        {
            Instance = this;
            Log = Logger;
            TG11API.Log = Log;

            Log.LogInfo("TG11 Utils loaded");
            Log.LogInfo($"[TG11] Assembly: {Assembly.GetExecutingAssembly().FullName}");
            Log.LogInfo($"[TG11] Location: {Assembly.GetExecutingAssembly().Location}");

            gameObject.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(gameObject);

            var cvars = new CVarRegistry();
            Services.Register(cvars);
            TG11API.CVars = cvars;

            Services = new ServiceRegistry();
            Services.Register(this);
            TG11API.Services = Services;

            Hotkeys = new HotkeyManager();
            Services.Register(Hotkeys);
            TG11API.Hotkeys = Hotkeys;

            // Config Binds
            _toggleConsoleKey = Config.Bind("Hotkeys", "ToggleConsole", new KeyboardShortcut(KeyCode.BackQuote), "Toggle TG11 console");
            _toggleSettingsKey = Config.Bind("Hotkeys", "ToggleSettings", new KeyboardShortcut(KeyCode.F10), "Toggle TG11 settings");
            _debugEnabled = Config.Bind("Debug", "Enabled", false, "Enable TG11 debug logging/overlays");

            TG11API.SetDebug(_debugEnabled.Value);

            TG11API.CVars.RegisterBool(
                "tg11.debug",
                () => TG11API.DebugEnabled,
                v => TG11API.SetDebug(v),      // or if SetDebug is public; otherwise assign + maybe raise event
                defaultValue: _debugEnabled.Value,
                help: "Enable TG11 debug logs/overlays"
            );


            Commands = new CommandRegistry(Log);
            Console  = new ConsoleOverlay(Commands, Log);
            Settings = new SettingsOverlay(Config, Log);

            Services.Register(Commands);
            Services.Register(Console);
            Services.Register(Settings);

            RegisterBuiltInCommands();

            // Create host AFTER console/settings exist
            var host = EnsureHost();

            host.OnTick = () =>
            {
                if (Hotkeys.Pressed("console.toggle", _toggleConsoleKey.Value, 0.20f, allowWhileTyping: true)) Console.Toggle();
                if (Hotkeys.Pressed("settings.toggle", _toggleSettingsKey.Value, 0.20f, allowWhileTyping: false)) Settings.Toggle();
                Console.Update();
            };

            host.OnDraw = () =>
            {
                Console.OnGUI();
                Settings.OnGUI();
            };

            Log.LogInfo("[TG11] Host created and wired. API Online.");
        }


        private void OnEnable()  => Log.LogInfo("[TG11] OnEnable()");
        private void Start()     => Log.LogInfo("[TG11] Start()");
        private void OnDisable() => Log.LogInfo("[TG11] OnDisable()");
        private void OnDestroy() => Log.LogInfo("[TG11] OnDestroy()");

        private void RegisterBuiltInCommands()
        {
            Commands.Register("help", "Lists commands", args => Console.Print(Commands.HelpText()));
            Commands.Register("echo", "Echo arguments", args => Console.Print(string.Join(" ", args)));
            Commands.Register("clear", "Clear console output", args => Console.Clear());

            Commands.Register("roots", "List root scene objects", args =>
            {
                var names = GameObjectUtil.ListRootNames();
                Console.Print(string.Join("\n", names));
            });

            Commands.Register("find", "find <substring> (objects by name contains)", args =>
            {
                if (args.Length == 0) { Console.Print("Usage: find <substring>"); return; }
                var hits = GameObjectUtil.FindByNameContains(args[0]);
                Console.Print($"Found {hits.Count}:");
                for (int i = 0; i < Mathf.Min(50, hits.Count); i++)
                    Console.Print(GameObjectUtil.GetPath(hits[i].transform));
                if (hits.Count > 50) Console.Print("(showing first 50)");
            });
            Commands.Register("debug", "debug [on|off|toggle] (view/set debug mode)", args =>
            {
                if (args.Length == 0)
                {
                    Console.Print($"DebugEnabled = {TG11API.DebugEnabled}");
                    return;
                }

                var a = args[0].Trim().ToLowerInvariant();
                bool next;

                if (a == "on" || a == "true" || a == "1") next = true;
                else if (a == "off" || a == "false" || a == "0") next = false;
                else if (a == "toggle") next = !TG11API.DebugEnabled;
                else
                {
                    Console.Print("Usage: debug [on|off|toggle]");
                    return;
                }

                TG11API.SetDebug(next);
                _debugEnabled.Value = next; // persist
                Console.Print($"DebugEnabled = {TG11API.DebugEnabled}");
            });
            Commands.Register("vars", "List console variables", args =>
            {
                foreach (var v in TG11API.CVars.All)
                    Console.Print($"{v.Name} ({v.Type}) = {v.GetAsString()}  [default={v.DefaultAsString}]");
            });

            Commands.Register("get", "get <name> (read variable)", args =>
            {
                if (args.Length < 1) { Console.Print("Usage: get <name>"); return; }
                if (!TG11API.CVars.TryGet(args[0], out var v)) { Console.Print("Unknown var"); return; }
                Console.Print($"{v.Name} = {v.GetAsString()}");
            });

            Commands.Register("set", "set <name> <value> (set variable)", args =>
            {
                if (args.Length < 2) { Console.Print("Usage: set <name> <value>"); return; }

                var name = args[0];
                var value = string.Join(" ", args, 1, args.Length - 1);

                if (!TG11API.CVars.TryGet(name, out var v)) { Console.Print("Unknown var"); return; }

                if (v.TrySetFromString(value, out var err))
                    Console.Print($"{v.Name} = {v.GetAsString()}");
                else
                    Console.Print($"Error: {err}");
            });

            Commands.Register("toggle", "toggle <name> (toggle bool var)", args =>
            {
                if (args.Length < 1) { Console.Print("Usage: toggle <name>"); return; }
                if (!TG11API.CVars.TryGet(args[0], out var v)) { Console.Print("Unknown var"); return; }
                if (v.Type != "bool") { Console.Print("toggle only works on bool vars"); return; }

                var current = v.GetAsString().ToLowerInvariant() == "true";
                v.TrySetFromString(current ? "false" : "true", out _);
                Console.Print($"{v.Name} = {v.GetAsString()}");
            });

            Commands.Register("reset", "reset <name> (reset var to default)", args =>
            {
                if (args.Length < 1) { Console.Print("Usage: reset <name>"); return; }
                if (!TG11API.CVars.TryGet(args[0], out var v)) { Console.Print("Unknown var"); return; }
                v.ResetToDefault();
                Console.Print($"{v.Name} = {v.GetAsString()}  (reset)");
            });

        }
    }
}
