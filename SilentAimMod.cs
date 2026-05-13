using MelonLoader;
using UnityEngine;

namespace SilentAim
{
    /// <summary>
    /// Main MelonLoader mod entry point for the Silent Aim mod.
    /// Handles initialization, hotkeys, and GUI rendering.
    /// </summary>
    public class SilentAimMod : MelonMod
    {
        public override void OnInitializeMelon()
        {
            Settings.Init();
            MelonLogger.Msg("═══════════════════════════════════════");
            MelonLogger.Msg("  SilentAimPlusVector Loaded!");
            MelonLogger.Msg($"  [SilentAim] Toggle: {Settings.ToggleKey} | FOV: {Settings.AimFov}° | Bone: {Settings.CurrentHitPointName}");
            MelonLogger.Msg($"  [VectorAim] Smooth: {Settings.VectorAimSmoothFactor} | FOV: {Settings.VectorAimFov}° | Bone: {Settings.VectorAimSelectedHitPointIndex}");
            MelonLogger.Msg("═══════════════════════════════════════");
        }

        public override void OnUpdate()
        {
            HandleHotkeys();
        }

        public override void OnGUI()
        {
            FovOverlay.Draw();
        }

        private void HandleHotkeys()
        {
            // Toggle silent aim on/off
            if (Input.GetKeyDown(Settings.ToggleKey))
            {
                Settings.Enabled = !Settings.Enabled;
                MelonLogger.Msg($"[SilentAim] {(Settings.Enabled ? "ENABLED" : "DISABLED")}");
            }

            // Cycle through body parts
            if (Input.GetKeyDown(Settings.CycleBoneKey))
            {
                Settings.CycleHitPoint();
                MelonLogger.Msg($"[SilentAim] Hit Point: {Settings.CurrentHitPointName}");
            }

            // Adjust FOV with Ctrl+Scroll or Ctrl+Plus/Minus
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                float scroll = Input.GetAxis("Mouse ScrollWheel");
                if (scroll != 0f)
                {
                    Settings.AimFov = Mathf.Clamp(Settings.AimFov + scroll * 50f, 5f, 180f);
                    MelonLogger.Msg($"[SilentAim] FOV: {Settings.AimFov:F0}°");
                }

                if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus))
                {
                    Settings.AimFov = Mathf.Clamp(Settings.AimFov + 5f, 5f, 180f);
                    MelonLogger.Msg($"[SilentAim] FOV: {Settings.AimFov:F0}°");
                }

                if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
                {
                    Settings.AimFov = Mathf.Clamp(Settings.AimFov - 5f, 5f, 180f);
                    MelonLogger.Msg($"[SilentAim] FOV: {Settings.AimFov:F0}°");
                }
            }
        }
    }
}
