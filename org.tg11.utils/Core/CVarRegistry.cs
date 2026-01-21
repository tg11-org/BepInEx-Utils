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
using System.Globalization;

namespace org.TG11.utils.Core
{
    public sealed class CVarRegistry
    {
        private readonly Dictionary<string, ICVar> _vars = new(StringComparer.OrdinalIgnoreCase);

        public IEnumerable<ICVar> All => _vars.Values;

        public bool TryGet(string name, out ICVar cvar) => _vars.TryGetValue(name, out cvar);

        public void RegisterBool(string name, Func<bool> get, Action<bool> set, bool defaultValue = false, string help = "")
            => _vars[name] = new BoolCVar(name, help, defaultValue, get, set);

        public void RegisterInt(string name, Func<int> get, Action<int> set, int defaultValue = 0, string help = "")
            => _vars[name] = new IntCVar(name, help, defaultValue, get, set);

        public void RegisterFloat(string name, Func<float> get, Action<float> set, float defaultValue = 0f, string help = "")
            => _vars[name] = new FloatCVar(name, help, defaultValue, get, set);

        public void RegisterString(string name, Func<string> get, Action<string> set, string defaultValue = "", string help = "")
            => _vars[name] = new StringCVar(name, help, defaultValue, get, set);

        public interface ICVar
        {
            string Name { get; }
            string Help { get; }
            string Type { get; }
            string DefaultAsString { get; }
            string GetAsString();
            bool TrySetFromString(string raw, out string error);
            void ResetToDefault();
        }

        private abstract class CVarBase<T> : ICVar
        {
            public string Name { get; }
            public string Help { get; }
            public string Type { get; }
            public string DefaultAsString => Format(_defaultValue);

            protected readonly T _defaultValue;
            protected readonly Func<T> _get;
            protected readonly Action<T> _set;

            protected CVarBase(string name, string help, string type, T defaultValue, Func<T> get, Action<T> set)
            {
                Name = name;
                Help = help ?? "";
                Type = type;
                _defaultValue = defaultValue;
                _get = get;
                _set = set;
            }

            public string GetAsString() => Format(_get());

            public void ResetToDefault() => _set(_defaultValue);

            public bool TrySetFromString(string raw, out string error)
            {
                if (TryParse(raw, out var v, out error))
                {
                    _set(v);
                    return true;
                }
                return false;
            }

            protected abstract bool TryParse(string raw, out T value, out string error);
            protected abstract string Format(T value);
        }

        private sealed class BoolCVar : CVarBase<bool>
        {
            public BoolCVar(string name, string help, bool def, Func<bool> get, Action<bool> set)
                : base(name, help, "bool", def, get, set) { }

            protected override bool TryParse(string raw, out bool value, out string error)
            {
                raw = (raw ?? "").Trim().ToLowerInvariant();
                if (raw is "1" or "true" or "on" or "yes") { value = true; error = null; return true; }
                if (raw is "0" or "false" or "off" or "no") { value = false; error = null; return true; }
                value = default;
                error = "Expected: on/off, true/false, 1/0, yes/no";
                return false;
            }

            protected override string Format(bool value) => value ? "true" : "false";
        }

        private sealed class IntCVar : CVarBase<int>
        {
            public IntCVar(string name, string help, int def, Func<int> get, Action<int> set)
                : base(name, help, "int", def, get, set) { }

            protected override bool TryParse(string raw, out int value, out string error)
            {
                if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                { error = null; return true; }
                error = "Expected integer";
                return false;
            }

            protected override string Format(int value) => value.ToString(CultureInfo.InvariantCulture);
        }

        private sealed class FloatCVar : CVarBase<float>
        {
            public FloatCVar(string name, string help, float def, Func<float> get, Action<float> set)
                : base(name, help, "float", def, get, set) { }

            protected override bool TryParse(string raw, out float value, out string error)
            {
                if (float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                { error = null; return true; }
                error = "Expected float";
                return false;
            }

            protected override string Format(float value) => value.ToString(CultureInfo.InvariantCulture);
        }

        private sealed class StringCVar : CVarBase<string>
        {
            public StringCVar(string name, string help, string def, Func<string> get, Action<string> set)
                : base(name, help, "string", def, get, set) { }

            protected override bool TryParse(string raw, out string value, out string error)
            {
                value = raw ?? "";
                error = null;
                return true;
            }

            protected override string Format(string value) => value ?? "";
        }
    }
}
