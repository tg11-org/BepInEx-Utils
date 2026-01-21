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
using org.TG11.utils;

namespace org.TG11.utils.Core
{
    public sealed class ServiceRegistry
    {
        private readonly Dictionary<Type, object> _services = new();

        public void Register<T>(T instance) where T : class
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            _services[typeof(T)] = instance;
            TG11_utils.Log.LogInfo("TG11 Utils Registered a service.");
        }

        public bool TryGet<T>(out T service) where T : class
        {
            if (_services.TryGetValue(typeof(T), out var obj) && obj is T typed)
            {
                service = typed;
                return true;
            }
            service = null;
            return false;
        }

        public T Get<T>() where T : class
        {
            if (TryGet<T>(out var s)) return s;
            throw new KeyNotFoundException($"Service not registered: {typeof(T).FullName}");
        }

        public void Remove<T>() where T : class => _services.Remove(typeof(T));
        public void Clear() => _services.Clear();
    }
}
