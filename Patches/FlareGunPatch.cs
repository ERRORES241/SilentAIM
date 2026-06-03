using HarmonyLib;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace AimMod.Patches
{
    /// <summary>
    /// Harmony patch for FlareGunRoundItem.SpawnAndFire() — intercepts flare gun shots
    /// and teleports the spawned flare round near the target bone for an instant hit.
    /// 
    /// SpawnAndFire is a static method that creates the round GameObject and calls Fire().
    /// We patch with Postfix to grab the returned GameObject, find its Rigidbody,
    /// and redirect it to the target.
    /// </summary>
    [HarmonyPatch(typeof(FlareGunRoundItem), nameof(FlareGunRoundItem.SpawnAndFire))]
    public static class FlareGunPatch
    {
        [HarmonyPostfix]
        public static void Postfix(GameObject __result)
        {
            try
            {
                if (!Settings.Enabled) return;
                if (!Settings.EnableForFlareGun) return;
                if (__result == null) return;

                TargetingSystem.InvalidateCache();
                var target = TargetingSystem.GetCurrentTarget();

                if (!target.Found) return;

                Camera cam = GameManager.GetMainCamera();
                if (cam == null) return;

                // Get the FlareGunRoundItem component and its rigidbody
                var round = __result.GetComponent<FlareGunRoundItem>();
                if (round == null) return;

                var rb = round.m_Rigidbody;
                if (rb == null)
                {
                    rb = __result.GetComponent<Rigidbody>();
                    if (rb == null) return;
                }

                Vector3 camPos = cam.transform.position;
                Vector3 directionToTarget = (target.BoneWorldPosition - camPos).normalized;

                // Teleport flare round close to the target bone (0.19m away)
                Vector3 teleportPos = target.BoneWorldPosition - (directionToTarget * 0.19f);

                __result.transform.position = teleportPos;
                __result.transform.rotation = Quaternion.LookRotation(directionToTarget);
                rb.position = teleportPos;
                rb.rotation = __result.transform.rotation;
                rb.velocity = directionToTarget * 50f;

                if (Settings.EnableDebugLogging)
                {
                    MelonLogger.Msg($"[SilentAim] Flare redirected → {target.AiName} [{target.BoneName}] @ {target.DistanceToTarget:F1}m");
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[SilentAim] FlareGunPatch.Postfix error: {ex.Message}");
            }
        }
    }
}
