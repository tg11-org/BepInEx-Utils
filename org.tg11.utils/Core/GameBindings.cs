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

namespace org.TG11.utils.Core
{
    public sealed class GameBindings
    {
        private readonly Dictionary<string, Func<string>> _getters = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Func<string, (bool ok, string err)>> _setters = new(StringComparer.OrdinalIgnoreCase);

        public IEnumerable<string> Keys => _getters.Keys;

        public void Register(string name, Func<string> get, Func<string, (bool ok, string err)> set = null)
        {
            _getters[name] = get ?? throw new ArgumentNullException(nameof(get));
            if (set != null) _setters[name] = set;
        }

        public bool TryGet(string name, out string value, out string err)
        {
            err = null;
            value = null;
            if (!_getters.TryGetValue(name, out var g)) { err = "Unknown binding"; return false; }
            try { value = g(); return true; }
            catch (Exception ex) { err = ex.ToString(); return false; }
        }

        public bool TrySet(string name, string raw, out string err)
        {
            err = null;
            if (!_setters.TryGetValue(name, out var s)) { err = "Binding is read-only or unknown"; return false; }
            try
            {
                var (ok, e) = s(raw);
                err = e;
                return ok;
            }
            catch (Exception ex) { err = ex.ToString(); return false; }
        }
    }
}
