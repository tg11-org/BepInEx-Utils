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
using UnityEngine;
using org.TG11.utils;
using org.TG11.utils.API;

namespace org.TG11.utils.Core
{
    public sealed class TG11Host : MonoBehaviour
    {
        public System.Action OnTick;
        public System.Action OnDraw;

        private float _heartbeatInterval = 1f;

        private void Awake()
        {
            // Make it harder for aggressive scene systems to wipe us
            gameObject.hideFlags = HideFlags.HideAndDontSave;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            TG11API.Log?.LogInfo("[TG11Host] Start()");
            StartCoroutine(Heartbeat());
        }

        private void Update()
        {
            OnTick?.Invoke();
        }

        public void Wire(System.Action onTick, System.Action onDraw)
        {
            OnTick = onTick;
            OnDraw = onDraw;
        }

        private void OnGUI()
        {
            OnDraw?.Invoke();
        }

        private IEnumerator Heartbeat()
        {
            int n = 0;
            while (true)
            {
                n++;
                if (TG11API.DebugEnabled)
                    TG11API.Log?.LogInfo($"[TG11Host] heartbeat #{n} t={Time.realtimeSinceStartup:0.00}");
                
                yield return new WaitForSecondsRealtime(_heartbeatInterval);
            }
        }

        private void OnDestroy()
        {
            TG11API.Log?.LogInfo("[TG11Host] OnDestroy()");
        }
    }
}
