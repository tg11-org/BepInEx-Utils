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
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace org.TG11.utils.Helpers
{
    public static class GameObjectUtil
    {
        /// <summary>Enumerates all root GameObjects across all loaded scenes.</summary>
        public static IEnumerable<GameObject> GetAllRootObjects()
        {
            int count = SceneManager.sceneCount;
            for (int i = 0; i < count; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;

                foreach (var go in scene.GetRootGameObjects())
                    yield return go;
            }
        }

        /// <summary>Finds a GameObject by exact name (searches active + inactive).</summary>
        public static GameObject FindByName(string name)
        {
            foreach (var root in GetAllRootObjects())
            {
                var t = FindInChildrenByName(root.transform, name);
                if (t != null) return t.gameObject;
            }
            return null;
        }

        /// <summary>Finds all components of type T, including inactive objects.</summary>
        public static List<T> FindAllComponents<T>() where T : Component
        {
            var results = new List<T>();
            foreach (var root in GetAllRootObjects())
                results.AddRange(root.GetComponentsInChildren<T>(true));
            return results;
        }

        /// <summary>
        /// Tries to find a "singleton-ish" component: first instance of T in loaded scenes.
        /// </summary>
        public static T FindFirstComponent<T>() where T : Component
        {
            foreach (var root in GetAllRootObjects())
            {
                var found = root.GetComponentInChildren<T>(true);
                if (found != null) return found;
            }
            return null;
        }

        /// <summary>Returns a full hierarchy path like "Root/Child/Grandchild".</summary>
        public static string GetPath(Transform t)
        {
            if (t == null) return "<null>";
            var stack = new Stack<string>();
            while (t != null)
            {
                stack.Push(t.name);
                t = t.parent;
            }
            return string.Join("/", stack);
        }

        /// <summary>
        /// Find a transform by walking children, including inactive.
        /// </summary>
        private static Transform FindInChildrenByName(Transform root, string name)
        {
            if (root.name == name) return root;

            for (int i = 0; i < root.childCount; i++)
            {
                var child = root.GetChild(i);
                var found = FindInChildrenByName(child, name);
                if (found != null) return found;
            }
            return null;
        }

        /// <summary>
        /// Debug: list root objects, optionally filtered.
        /// </summary>
        public static List<string> ListRootNames(Func<GameObject, bool> filter = null)
        {
            return GetAllRootObjects()
                .Where(go => filter == null || filter(go))
                .Select(go => go.name)
                .Distinct()
                .OrderBy(n => n)
                .ToList();
        }

        /// <summary>
        /// Debug: find objects whose name contains a substring.
        /// </summary>
        public static List<GameObject> FindByNameContains(string substring, bool ignoreCase = true)
        {
            if (substring == null) substring = "";
            var comp = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            var matches = new List<GameObject>();
            foreach (var root in GetAllRootObjects())
            {
                foreach (var t in root.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name.IndexOf(substring, comp) >= 0)
                        matches.Add(t.gameObject);
                }
            }
            return matches;
        }
    }
}
