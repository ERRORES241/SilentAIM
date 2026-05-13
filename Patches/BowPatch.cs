using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace AimMod.Patches
{
    /// <summary>
    /// Harmony patch for BowItem.ShootArrow() — intercepts bow shots
    /// and redirects the spawned arrow's Rigidbody velocity toward the best target.
    /// 
    /// Previous approach (rotating camera in Prefix) didn't work because
    /// the arrow direction is determined from a cached value or internal
    /// transform, not the camera's current rotation at ShootArrow time.
    /// 
    /// New approach:
    /// 1. Prefix: Find target and cache it
    /// 2. Original ShootArrow() spawns the arrow normally
    /// 3. Postfix: Find the freshly spawned arrow near the camera
    ///    and redirect its Rigidbody velocity + transform rotation toward target
    /// </summary>
    [HarmonyPatch(typeof(BowItem), nameof(BowItem.ShootArrow))]
    public static class BowPatch
    {
        private static bool _shouldRedirect = false;
        private static Vector3 _targetPosition;
        private static string _targetName;
        private static string _boneName;
        private static float _targetDist;

        [HarmonyPrefix]
        public static void Prefix(BowItem __instance)
        {
            _shouldRedirect = false;

            try
            {
                if (!Settings.Enabled) return;
                if (!Settings.EnableForBow) return;

                // Find best target
                TargetingSystem.InvalidateCache();
                var target = TargetingSystem.GetCurrentTarget();

                if (!target.Found) return;

                // Cache target info for the Postfix
                _shouldRedirect = true;
                _targetPosition = target.BoneWorldPosition;
                _targetName = target.AiName;
                _boneName = target.BoneName;
                _targetDist = target.DistanceToTarget;
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[SilentAim] BowPatch.Prefix error: {ex.Message}");
            }
        }

        [HarmonyPostfix]
        public static void Postfix(BowItem __instance)
        {
            if (!_shouldRedirect) return;
            _shouldRedirect = false;

            try
            {
                Camera cam = GameManager.GetMainCamera();
                if (cam == null) return;

                Vector3 camPos = cam.transform.position;

                // Find the arrow that was just spawned by searching for ArrowItem
                // objects near the camera position (the arrow just left the bow)
                var arrows = Object.FindObjectsOfType<ArrowItem>();
                if (arrows == null || arrows.Length == 0) return;

                ArrowItem spawnedArrow = null;
                float closestDist = float.MaxValue;

                for (int i = 0; i < arrows.Length; i++)
                {
                    var arrow = arrows[i];
                    if (arrow == null) continue;

                    try
                    {
                        // Check for Rigidbody — only in-flight arrows have one
                        var rb = arrow.GetComponent<Rigidbody>();
                        if (rb == null) continue;

                        float dist = Vector3.Distance(arrow.transform.position, camPos);
                        if (dist < closestDist && dist < 10f) // Within 10m of camera (just spawned)
                        {
                            closestDist = dist;
                            spawnedArrow = arrow;
                        }
                    }
                    catch { continue; }
                }

                if (spawnedArrow == null) return;

                var arrowRb = spawnedArrow.GetComponent<Rigidbody>();
                if (arrowRb == null) return;

                // Get current arrow speed
                float speed = arrowRb.velocity.magnitude;
                if (speed < 1f) speed = 50f; // Fallback speed if somehow zero

                // Calculate direction toward target bone from camera
                Vector3 directionToTarget = (_targetPosition - camPos).normalized;

                // Teleport arrow very close to the target bone (0.1m away) to ensure an instant hit
                Vector3 teleportPos = _targetPosition - (directionToTarget * 0.1f);

                // Redirect arrow velocity and rotation
                spawnedArrow.transform.position = teleportPos;
                spawnedArrow.transform.rotation = Quaternion.LookRotation(directionToTarget);
                
                arrowRb.position = teleportPos;
                arrowRb.rotation = spawnedArrow.transform.rotation;
                arrowRb.velocity = directionToTarget * 50f; // High speed to immediately trigger collision

                if (Settings.EnableDebugLogging)
                {
                    MelonLogger.Msg($"[SilentAim] Arrow redirected → {_targetName} [{_boneName}] @ {_targetDist:F1}m (speed:{speed:F1})");
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[SilentAim] BowPatch.Postfix error: {ex.Message}");
            }
        }
    }
}
