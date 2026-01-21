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
using System.Linq;
using System.Reflection;
using System.Text;

namespace org.TG11.utils.Helpers
{
    public static class ReflectionUtil
    {
        private const BindingFlags AllInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags AllStatic   = BindingFlags.Static   | BindingFlags.Public | BindingFlags.NonPublic;

        public static FieldInfo GetField(Type type, string name)
            => type.GetField(name, AllInstance | AllStatic);

        public static PropertyInfo GetProperty(Type type, string name)
            => type.GetProperty(name, AllInstance | AllStatic);

        public static MethodInfo GetMethod(Type type, string name, params Type[] argTypes)
        {
            var methods = type.GetMethods(AllInstance | AllStatic).Where(m => m.Name == name);
            if (argTypes == null || argTypes.Length == 0)
                return methods.FirstOrDefault();

            foreach (var m in methods)
            {
                var ps = m.GetParameters();
                if (ps.Length != argTypes.Length) continue;
                bool ok = true;
                for (int i = 0; i < ps.Length; i++)
                {
                    if (ps[i].ParameterType != argTypes[i]) { ok = false; break; }
                }
                if (ok) return m;
            }
            return null;
        }

        public static T GetFieldValue<T>(object instance, string fieldName)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            var f = GetField(instance.GetType(), fieldName)
                    ?? throw new MissingFieldException(instance.GetType().FullName, fieldName);
            return (T)f.GetValue(instance);
        }

        public static void SetFieldValue(object instance, string fieldName, object value)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            var f = GetField(instance.GetType(), fieldName)
                    ?? throw new MissingFieldException(instance.GetType().FullName, fieldName);
            f.SetValue(instance, value);
        }

        public static T GetPropertyValue<T>(object instance, string propName)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            var p = GetProperty(instance.GetType(), propName)
                    ?? throw new MissingMemberException(instance.GetType().FullName, propName);
            return (T)p.GetValue(instance, null);
        }

        public static void SetPropertyValue(object instance, string propName, object value)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            var p = GetProperty(instance.GetType(), propName)
                    ?? throw new MissingMemberException(instance.GetType().FullName, propName);
            p.SetValue(instance, value, null);
        }

        public static object CallMethod(object instance, string methodName, params object[] args)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            args ??= Array.Empty<object>();

            var argTypes = args.Select(a => a?.GetType() ?? typeof(object)).ToArray();
            var m = GetMethod(instance.GetType(), methodName, argTypes) ?? GetMethod(instance.GetType(), methodName);

            if (m == null)
                throw new MissingMethodException(instance.GetType().FullName, methodName);

            return m.Invoke(instance, args);
        }

        /// <summary>
        /// Debug helper: dump public+private fields and readable properties.
        /// </summary>
        public static string DumpObject(object instance, int maxStringLen = 200)
        {
            if (instance == null) return "<null>";

            var t = instance.GetType();
            var sb = new StringBuilder();
            sb.AppendLine($"{t.FullName}");

            foreach (var f in t.GetFields(AllInstance))
            {
                object v;
                try { v = f.GetValue(instance); }
                catch (Exception ex) { v = $"<error: {ex.GetType().Name}>"; }

                sb.AppendLine($"  [F] {f.FieldType.Name} {f.Name} = {FormatValue(v, maxStringLen)}");
            }

            foreach (var p in t.GetProperties(AllInstance))
            {
                if (p.GetIndexParameters().Length != 0) continue; // skip indexers
                if (!p.CanRead) continue;

                object v;
                try { v = p.GetValue(instance, null); }
                catch { continue; } // many Unity props throw if not valid context

                sb.AppendLine($"  [P] {p.PropertyType.Name} {p.Name} = {FormatValue(v, maxStringLen)}");
            }

            return sb.ToString();
        }

        private static string FormatValue(object v, int maxLen)
        {
            if (v == null) return "<null>";
            var s = v.ToString() ?? "<null>";
            if (s.Length > maxLen) s = s.Substring(0, maxLen) + "...";
            return s;
        }
    }
}
