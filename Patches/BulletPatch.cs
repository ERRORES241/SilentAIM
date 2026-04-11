using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace SilentAim.Patches
{
    /// <summary>
    /// Harmony patch for vp_Bullet.Start() — intercepts hitscan weapons
    /// (Rifle, Revolver) and redirects the bullet's raycast
    /// toward the best target within the configured FOV.
    /// 
    /// How it works:
    /// 1. vp_Bullet is spawned at the muzzle facing camera forward
    /// 2. Start() fires Physics.Raycast(transform.position, transform.forward, Range)
    /// 3. Prefix: We rotate the bullet transform to face the target bone
    /// 4. Original Start() fires the raycast in the redirected direction → hits target
    /// 5. Postfix: Clean up
    /// </summary>
    [HarmonyPatch(typeof(vp_Bullet), nameof(vp_Bullet.Start))]
    public static class BulletPatch
    {

        [HarmonyPrefix]
        public static void Prefix(vp_Bullet __instance)
        {
            try
            {
                if (!Settings.Enabled) return;

                // Check per-weapon toggle
                GunType gunType = __instance.m_GunType;
                switch (gunType)
                {
                    case GunType.Rifle:
                        if (!Settings.EnableForRifle) return;
                        break;
                    case GunType.Revolver:
                        if (!Settings.EnableForRevolver) return;
                        break;
                    default:
                        return; // Camera type or unknown — skip
                }

                // Find best target
                TargetingSystem.InvalidateCache();
                var target = TargetingSystem.GetCurrentTarget();

                if (!target.Found) return;

                Transform bulletTransform = __instance.transform;
                if (bulletTransform == null) return;

                // Calculate direction from bullet position to target bone
                Vector3 bulletPos = bulletTransform.position;
                Vector3 direction = (target.BoneWorldPosition - bulletPos).normalized;

                // Redirect the bullet
                bulletTransform.rotation = Quaternion.LookRotation(direction);

                if (Settings.EnableDebugLogging)
                {
                    MelonLogger.Msg($"[SilentAim] Bullet redirected → {target.AiName} [{target.BoneName}] @ {target.DistanceToTarget:F1}m ({gunType})");
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[SilentAim] BulletPatch.Prefix error: {ex.Message}");
            }
        }

        [HarmonyPostfix]
        public static void Postfix(vp_Bullet __instance)
        {
            // The bullet is a temporary object that gets destroyed after Start().
            // No rotation restore needed — the raycast already fired.
        }
    }
}
