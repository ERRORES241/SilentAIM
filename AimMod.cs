using MelonLoader;
using UnityEngine;

namespace AimMod
{
    public class AimMod : MelonMod
    {
        public override void OnInitializeMelon()
        {
            Settings.Init();
            MelonLogger.Msg("═══════════════════════════════════════");
            MelonLogger.Msg("  AimbotMod_TLD Loaded!");
            MelonLogger.Msg($"  [SilentAim] Toggle: {Settings.ToggleKey} | FOV: {Settings.AimFov}° | Bone: {Settings.CurrentHitPointName}");
            MelonLogger.Msg($"  [VectorAim] Toggle: {Settings.VectorAimToggleKey} | Smooth: {Settings.VectorAimSmoothFactor} | FOV: {Settings.VectorAimFov}°");
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
            if (Input.GetKeyDown(Settings.ToggleKey))
            {
                Settings.Enabled = !Settings.Enabled;
                MelonLogger.Msg($"[SilentAim] {(Settings.Enabled ? "ENABLED" : "DISABLED")}");
            }

            if (Input.GetKeyDown(Settings.VectorAimToggleKey))
            {
                Settings.VectorAimEnabled = !Settings.VectorAimEnabled;
                MelonLogger.Msg($"[VectorAim] {(Settings.VectorAimEnabled ? "ENABLED" : "DISABLED")}");
            }

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
