using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace SilentAim
{
    /// <summary>
    /// Physically moves the FPS camera toward the target bone while the player is in ADS.
    /// Called from VectorAimCameraPatch (Postfix on vp_FPSCamera.Update) each frame.
    /// </summary>
    public static class VectorAimSystem
    {
        private static bool _isTracking;

        /// <summary>
        /// Main per-frame entry point. Called by VectorAimCameraPatch after the game
        /// processes mouse input and sets m_Yaw/m_Pitch, so our values win for this frame.
        /// </summary>
        public static void ApplyToCamera(vp_FPSCamera fpsCamera)
        {
            if (!Settings.VectorAimEnabled)
            {
                _isTracking = false;
                return;
            }

            // ADS gate — only run while player is actively aiming
            if (!IsPlayerAiming())
            {
                _isTracking = false;
                return;
            }

            // Per-weapon toggle
            if (!IsCurrentWeaponEnabled())
            {
                _isTracking = false;
                return;
            }

            // Get target using Vector Aim's own FOV/range/bone settings
            var target = TargetingSystem.GetVectorAimTarget();
            if (!target.Found)
            {
                _isTracking = false;
                return;
            }

            Vector3 delta = target.BoneWorldPosition - fpsCamera.transform.position;
            float magnitude = delta.magnitude;
            if (magnitude < 0.5f) return;

            // Project world-space delta into vp_FPSCamera-local space.
            // This gives us the angular error relative to WHERE THE CAMERA IS CURRENTLY POINTING,
            // avoiding all world-space sign and convention ambiguity around m_Pitch.
            Vector3 localDir = fpsCamera.transform.InverseTransformDirection(delta.normalized);

            // Guard: target behind the camera (FOV filter should prevent this, but safety first)
            if (localDir.z <= 0f) return;

            // Yaw: localDir.x > 0 means target is to the right → turn right → add to m_Yaw
            float yawDelta = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;

            // Pitch: localDir.y > 0 means target is above current aim direction → look up.
            // TLD's vp_FPSCamera convention: positive m_Pitch = looking UP (verified empirically),
            // so the correction sign matches localDir.y directly.
            float pitchDelta = Mathf.Atan2(localDir.y, localDir.z) * Mathf.Rad2Deg;

            float smooth = Mathf.Max(1f, Settings.VectorAimSmoothFactor);

            fpsCamera.m_Yaw   += yawDelta   / smooth;
            fpsCamera.m_Pitch += pitchDelta / smooth;

            _isTracking = true;

            if (Settings.EnableDebugLogging)
                MelonLogger.Msg($"[VectorAim] → {target.AiName} [{target.BoneName}] " +
                                $"{target.DistanceToTarget:F1}m | localY:{localDir.y:F3} " +
                                $"yawΔ:{yawDelta:F1} pitchΔ:{pitchDelta:F1} " +
                                $"Yaw:{fpsCamera.m_Yaw:F1} Pitch:{fpsCamera.m_Pitch:F1}");
        }

        private static bool IsPlayerAiming()
        {
            try
            {
                var playerObj = GameManager.GetPlayerObject();
                if (playerObj == null) return false;

                var fpsPlayer = playerObj.GetComponent<vp_FPSPlayer>();
                return fpsPlayer != null && fpsPlayer.m_InZoom;
            }
            catch { return false; }
        }

        private static bool IsCurrentWeaponEnabled()
        {
            try
            {
                PlayerManager pm = GameManager.GetPlayerManagerComponent();
                if (pm == null) return false;

                GearItem item = pm.m_ItemInHands;
                if (item == null) return false;

                // Gun (Rifle / Revolver / Flare Gun all share GunItem)
                var gunItem = item.GetComponent<GunItem>();
                if (gunItem != null)
                {
                    return gunItem.m_GunType switch
                    {
                        GunType.Rifle    => Settings.VectorAimForRifle,
                        GunType.Revolver => Settings.VectorAimForRevolver,
                        _                => Settings.VectorAimForFlareGun  // catch-all: flare gun or other gun types
                    };
                }

                if (item.GetComponent<BowItem>()   != null) return Settings.VectorAimForBow;
                if (item.GetComponent<StoneItem>()  != null) return Settings.VectorAimForStone;
            }
            catch { }
            return false;
        }

        /// <summary>Whether Vector Aim is currently actively tracking a target.</summary>
        public static bool IsTracking => _isTracking;
    }
}
