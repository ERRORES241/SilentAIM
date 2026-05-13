using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using System.Collections.Generic;
using UnityEngine;

namespace AimMod.Patches
{
    /// <summary>
    /// Harmony patch for StoneItem.FixedUpdate() — detects thrown stones in flight
    /// and teleports them near the target bone, same approach as BowPatch with arrows.
    /// 
    /// Each stone is only redirected once (tracked by instance ID).
    /// </summary>
    [HarmonyPatch(typeof(StoneItem), nameof(StoneItem.FixedUpdate))]
    public static class StonePatch
    {
        private static readonly HashSet<int> _redirectedStones = new();

        [HarmonyPostfix]
        public static void Postfix(StoneItem __instance)
        {
            try
            {
                if (__instance == null) return;
                if (!__instance.m_Thrown) return;            // Only thrown stones
                if (!Settings.Enabled) return;
                if (!Settings.EnableForStone) return;

                int id = __instance.GetInstanceID();
                if (_redirectedStones.Contains(id)) return;  // Already redirected this stone

                var rb = __instance.m_RigidBody;
                if (rb == null) return;
                if (rb.velocity.sqrMagnitude < 1f) return;   // Not moving

                Camera cam = GameManager.GetMainCamera();
                if (cam == null) return;

                Vector3 camPos = cam.transform.position;

                // Only redirect stones close to the camera (just thrown, within 15m)
                float distToCam = Vector3.Distance(__instance.transform.position, camPos);
                if (distToCam > 15f) return;

                // Find target
                TargetingSystem.InvalidateCache();
                var target = TargetingSystem.GetCurrentTarget();
                if (!target.Found) return;

                // Teleport stone to 0.15m from target bone
                Vector3 directionToTarget = (target.BoneWorldPosition - camPos).normalized;
                Vector3 teleportPos = target.BoneWorldPosition - (directionToTarget * 0.15f);

                __instance.transform.position = teleportPos;
                rb.position = teleportPos;
                rb.velocity = directionToTarget * 40f;

                _redirectedStones.Add(id);

                // Prevent memory leak — clear old entries periodically
                if (_redirectedStones.Count > 100)
                    _redirectedStones.Clear();

                if (Settings.EnableDebugLogging)
                {
                    MelonLogger.Msg($"[SilentAim] Stone redirected → {target.AiName} [{target.BoneName}] @ {target.DistanceToTarget:F1}m");
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[SilentAim] StonePatch.Postfix error: {ex.Message}");
            }
        }
    }
}
