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
                Settings.RefreshUI();
                MelonLogger.Msg($"[SilentAim] {(Settings.Enabled ? "ENABLED" : "DISABLED")}");
            }

            if (Input.GetKeyDown(Settings.VectorAimToggleKey))
            {
                Settings.VectorAimEnabled = !Settings.VectorAimEnabled;
                Settings.RefreshUI();
                MelonLogger.Msg($"[VectorAim] {(Settings.VectorAimEnabled ? "ENABLED" : "DISABLED")}");
            }
        }
    }
}
