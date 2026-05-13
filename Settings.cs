using ModSettings;
using System.Reflection;
using UnityEngine;

namespace AimMod
{
    public enum BoneChoice
    {
        Head,
        Neck,
        Torso,
        Hips,
        Limb
    }

    /// <summary>
    /// Mod settings integrated with ModSettings library.
    /// Extends JsonModSettings for automatic JSON persistence and in-game UI.
    /// </summary>
    public class SilentAimSettings : JsonModSettings
    {
        // ── Silent Aim ──────────────────────────────────────────────

        [Section("Silent Aim")]

        [Name("Enable Silent Aim")]
        [Description("So, you've decided to become a cheater...")]
        public bool Enabled = false;

        [Name("Toggle Hotkey")]
        [Description("Key to toggle silent aim on/off")]
        public KeyCode ToggleKey = KeyCode.F2;

        [Name("Aim FOV (degrees)")]
        [Description("Field of view cone for target detection (Use mouse wheel to change, or ctrl + plus/minus)")]
        [Slider(5f, 180f)]
        public float AimFov = 60f;

        [Name("Max Range (meters)")]
        [Description("Maximum targeting distance")]
        [Slider(10f, 500f)]
        public float MaxRange = 200f;

        [Name("Target Bone")]
        [Description("Which body part to aim at")]
        public BoneChoice SelectedHitPoint = BoneChoice.Head;

        [Name("Enable Visibility Check")]
        [Description("Block silent aim if a target is behind walls or trees")]
        public bool EnableVisibilityCheck = true;

        // ── Weapons ──────────────────────────────────────────────

        [Section("Weapons")]

        [Name("Enable for Rifle")]
        public bool EnableForRifle = false;

        [Name("Enable for Revolver")]
        public bool EnableForRevolver = false;

        [Name("Enable for Bow")]
        public bool EnableForBow = false;

        [Name("Enable for Stone")]
        public bool EnableForStone = false;

        [Name("Enable for Flare Gun")]
        public bool EnableForFlareGun = false;

        // ── Vector Aim ───────────────────────────────────────────

        [Section("Vector Aim")]

        [Name("Enable Vector Aim")]
        [Description("Physically moves the camera toward the target bone while aiming (ADS). Independent from Silent Aim.")]
        public bool VectorAimEnabled = false;

        [Name("Toggle Hotkey")]
        [Description("Key to toggle Vector Aim on/off")]
        public KeyCode VectorAimToggleKey = KeyCode.F4;

        [Name("Smooth Factor")]
        [Description("Camera tracking speed. 1 = instant snap, 20 = very slow / cinematic")]
        [Slider(1f, 20f)]
        public float VectorAimSmoothFactor = 5f;

        [Name("Aim FOV (degrees)")]
        [Description("Field of view cone for Vector Aim target detection")]
        [Slider(5f, 180f)]
        public float VectorAimFov = 60f;

        [Name("Max Range (meters)")]
        [Slider(10f, 500f)]
        public float VectorAimMaxRange = 200f;

        [Name("Target Bone")]
        [Description("Which body part the camera tracks")]
        public BoneChoice VectorAimSelectedHitPoint = BoneChoice.Head;

        [Name("Enable Visibility Check")]
        [Description("Block Vector Aim if target is behind walls or trees")]
        public bool VectorAimEnableVisibilityCheck = true;

        // ── Vector Aim - Weapons ──────────────────────────────────

        [Section("Vector Aim - Weapons")]

        [Name("Enable for Rifle")]
        public bool VectorAimForRifle = false;

        [Name("Enable for Revolver")]
        public bool VectorAimForRevolver = false;

        [Name("Enable for Stone")]
        public bool VectorAimForStone = false;

        [Name("Enable for Flare Gun")]
        public bool VectorAimForFlareGun = false;

        // ── Target Info ───────────────────────────────────────────────

        [Section("Target Filters")]

        [Name("Target Wolves")]
        public bool TargetWolves = true;

        [Name("Target Bears")]
        public bool TargetBears = true;

        [Name("Target Deer/Stag")]
        public bool TargetStags = true;

        [Name("Target Rabbits")]
        public bool TargetRabbits = true;

        [Name("Target Moose")]
        public bool TargetMoose = true;

        [Name("Target Cougars")]
        public bool TargetCougars = true;

        // ── Visual ───────────────────────────────────────────────

        [Section("Visual")]

        [Name("Show FOV Circle")]
        [Description("Render the FOV circle overlay on screen")]
        public bool ShowFovCircle = true;

        [Name("Show Target Info")]
        [Description("Display target name, bone, and distance")]
        public bool ShowTargetInfo = true;

        [Name("Show Target Reticle")]
        [Description("Draw the small crosshair directly on the selected target bone")]
        public bool ShowTargetReticle = true;

        [Name("Show Target Line")]
        [Description("Draw a line from crosshair to target")]
        public bool ShowTargetLine = true;

        [Name("FOV Circle Red")]
        [Slider(0f, 1f)]
        public float FovCircleR = 0f;

        [Name("FOV Circle Green")]
        [Slider(0f, 1f)]
        public float FovCircleG = 1f;

        [Name("FOV Circle Blue")]
        [Slider(0f, 1f)]
        public float FovCircleB = 0.5f;

        [Name("FOV Circle Opacity")]
        [Slider(0.1f, 1f)]
        public float FovCircleA = 0.6f;

        // ── Debug ────────────────────────────────────────────────

        [Section("Debug")]

        [Name("Enable Debug Logging")]
        [Description("Print messages to console when targeting and shooting")]
        public bool EnableDebugLogging = false;

        protected override void OnChange(FieldInfo field, object oldValue, object newValue)
        {
            base.OnChange(field, oldValue, newValue);
            RefreshVisibility();
            RefreshGUI();
        }

        protected override void OnConfirm()
        {
            base.OnConfirm();
            TargetingSystem.InvalidateCache();
        }

        internal void RefreshVisibility()
        {
            // Silent Aim child settings — hide everything except the master toggle
            bool sa = Enabled;
            SetFieldVisible(nameof(ToggleKey),             sa);
            SetFieldVisible(nameof(AimFov),                sa);
            SetFieldVisible(nameof(MaxRange),              sa);
            SetFieldVisible(nameof(SelectedHitPoint),      sa);
            SetFieldVisible(nameof(EnableVisibilityCheck), sa);
            SetFieldVisible(nameof(EnableForRifle),        sa);
            SetFieldVisible(nameof(EnableForRevolver),     sa);
            SetFieldVisible(nameof(EnableForBow),          sa);
            SetFieldVisible(nameof(EnableForStone),        sa);
            SetFieldVisible(nameof(EnableForFlareGun),     sa);

            // Vector Aim child settings — hide everything except the master toggle
            bool va = VectorAimEnabled;
            SetFieldVisible(nameof(VectorAimToggleKey),             va);
            SetFieldVisible(nameof(VectorAimSmoothFactor),          va);
            SetFieldVisible(nameof(VectorAimFov),                   va);
            SetFieldVisible(nameof(VectorAimMaxRange),               va);
            SetFieldVisible(nameof(VectorAimSelectedHitPoint),       va);
            SetFieldVisible(nameof(VectorAimEnableVisibilityCheck),  va);
            SetFieldVisible(nameof(VectorAimForRifle),               va);
            SetFieldVisible(nameof(VectorAimForRevolver),            va);
            SetFieldVisible(nameof(VectorAimForStone),               va);
            SetFieldVisible(nameof(VectorAimForFlareGun),            va);
        }
    }

    /// <summary>
    /// Static accessor for mod settings.
    /// Wraps the SilentAimSettings instance for convenient access throughout the mod.
    /// </summary>
    public static class Settings
    {
        private static SilentAimSettings _settings;

        /// <summary>
        /// Body part display names in priority order (most lethal → least).
        /// </summary>
        public static readonly string[] HitPointNames = { "Head", "Neck", "Torso", "Hips", "Limb" };

        /// <summary>
        /// Maps our display index → BodyPart enum value.
        /// Display: Head(0), Neck(1), Torso(2), Hips(3), Limb(4)
        /// BodyPart enum: head=0, torso=1, hips=2, limb=3, neck=4
        /// </summary>
        public static readonly int[] HitPointToBodyPart = { 0, 4, 1, 2, 3 };

        // ── Accessors ────────────────────────────────────────────

        public static bool Enabled
        {
            get => _settings.Enabled;
            set => _settings.Enabled = value;
        }

        public static float AimFov
        {
            get => _settings.AimFov;
            set => _settings.AimFov = value;
        }

        public static float MaxRange => _settings.MaxRange;
        public static bool ShowFovCircle => _settings.ShowFovCircle;
        public static bool ShowTargetInfo => _settings.ShowTargetInfo;
        public static bool ShowTargetLine => _settings.ShowTargetLine;
        public static bool EnableForRifle => _settings.EnableForRifle;
        public static bool EnableForRevolver => _settings.EnableForRevolver;
        public static bool EnableForBow => _settings.EnableForBow;
        public static bool EnableForStone => _settings.EnableForStone;
        public static bool EnableForFlareGun => _settings.EnableForFlareGun;

        public static Color FovCircleColor =>
            new(_settings.FovCircleR, _settings.FovCircleG, _settings.FovCircleB, _settings.FovCircleA);

        public static int SelectedHitPointIndex
        {
            get => (int)_settings.SelectedHitPoint;
            set => _settings.SelectedHitPoint = (BoneChoice)value;
        }

        public static string CurrentHitPointName =>
            SelectedHitPointIndex >= 0 && SelectedHitPointIndex < HitPointNames.Length
                ? HitPointNames[SelectedHitPointIndex]
                : "Head";

        public static int CurrentBodyPartEnum =>
            SelectedHitPointIndex >= 0 && SelectedHitPointIndex < HitPointToBodyPart.Length
                ? HitPointToBodyPart[SelectedHitPointIndex]
                : 0;

        // ── Target Filters ───────────────────────────────────────

        public static bool TargetWolves => _settings.TargetWolves;
        public static bool TargetBears => _settings.TargetBears;
        public static bool TargetStags => _settings.TargetStags;
        public static bool TargetRabbits => _settings.TargetRabbits;
        public static bool TargetMoose => _settings.TargetMoose;
        public static bool TargetCougars => _settings.TargetCougars;

        public static bool EnableDebugLogging => _settings.EnableDebugLogging;
        public static bool ShowTargetReticle => _settings.ShowTargetReticle;
        public static bool EnableVisibilityCheck => _settings.EnableVisibilityCheck;

        // ── Vector Aim ───────────────────────────────────────────

        public static bool VectorAimEnabled
        {
            get => _settings.VectorAimEnabled;
            set => _settings.VectorAimEnabled = value;
        }

        public static KeyCode VectorAimToggleKey => _settings.VectorAimToggleKey;
        public static float VectorAimSmoothFactor => _settings.VectorAimSmoothFactor;
        public static float VectorAimFov => _settings.VectorAimFov;
        public static float VectorAimMaxRange => _settings.VectorAimMaxRange;
        public static bool VectorAimEnableVisibilityCheck => _settings.VectorAimEnableVisibilityCheck;
        public static bool VectorAimForRifle => _settings.VectorAimForRifle;
        public static bool VectorAimForRevolver => _settings.VectorAimForRevolver;
        public static bool VectorAimForStone => _settings.VectorAimForStone;
        public static bool VectorAimForFlareGun => _settings.VectorAimForFlareGun;

        public static int VectorAimSelectedHitPointIndex => (int)_settings.VectorAimSelectedHitPoint;

        public static int VectorAimBodyPartEnum =>
            VectorAimSelectedHitPointIndex < HitPointToBodyPart.Length
                ? HitPointToBodyPart[VectorAimSelectedHitPointIndex]
                : 0;

        // ── Hotkeys ──────────────────────────────────────────────

        public static KeyCode ToggleKey => _settings.ToggleKey;
        // ── Methods ──────────────────────────────────────────────

        public static void CycleHitPoint()
        {
            SelectedHitPointIndex = (SelectedHitPointIndex + 1) % HitPointNames.Length;
            _settings.RefreshGUI();
        }

        public static void Init()
        {
            _settings = new SilentAimSettings();
            _settings.AddToModSettings("Aimbot Settings");
            _settings.RefreshVisibility();
            _settings.RefreshGUI();
        }
    }
}
