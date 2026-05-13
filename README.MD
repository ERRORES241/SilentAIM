# AimbotMod_TLD

[![MelonLoader](https://img.shields.io/badge/MelonLoader-0.7.2-blue.svg)](https://github.com/LavaGang/MelonLoader)
[![ModSettings](https://img.shields.io/badge/Dependency-ModSettings-orange.svg)](https://github.com/zeobviouslyfakeacc/ModSettings)
[![Version](https://img.shields.io/badge/Version-1.2.5-green.svg)](#)

Two independent aim systems for The Long Dark. Silent Aim redirects projectiles to the target bone without moving the camera. Vector Aim physically tracks a target bone with the camera while you're in ADS. Both can run at the same time.

---

### Silent Aim

Projectiles redirect to the nearest target inside your FOV cone. No camera movement, no tells.

- Aim at a specific bone: Head, Neck, Torso, Hips, or Limb
- Optional visibility check — skips targets behind walls/trees
- Per-animal and per-weapon toggles
- FOV circle overlay so you know what's in range

### Vector Aim

Camera smoothly tracks a target bone while you're aiming down sights. Smooth factor controls how fast it catches up — 1 snaps instantly, 20 is cinematic slow.

- Only active in ADS (`right-click`)
- Independent FOV, range, and bone settings from Silent Aim — you can track neck with Vector Aim while Silent Aim redirects to head
- Per-weapon toggles
- Bow not supported. ehhhhh.........

### Weapons

| Weapon | Silent Aim | Vector Aim |
| :--- | :---: | :---: |
| Rifle | ✓ | ✓ |
| Revolver | ✓ | ✓ |
| Bow | ✓ | — |
| Stones | ✓ | ✓ |
| Flare Gun | ✓ | ✓ |

---

### Visuals

All overlays are optional and toggled individually in mod settings.

| Overlay | Description |
| :--- | :--- |
| FOV Circle | Draws a circle on screen showing the exact cone Silent Aim uses to pick targets |
| Target Reticle | Small crosshair drawn directly on the selected target bone in world space |
| Target Line | Line from the center of the screen to the target bone — shows what's being tracked |
| Target Info | Text overlay: animal name, targeted bone, and distance in meters |

Colors and opacity for the FOV circle are configurable via RGB + alpha sliders.

---

### Hotkeys

| Action | Key |
| :--- | :--- |
| Toggle Silent Aim | `F2` (configurable) |
| Toggle Vector Aim | `F4` (configurable) |

Toggling either system via hotkey immediately updates the mod settings menu — child options appear and disappear in real time.

---

### Requirements

- MelonLoader v0.7.2+
- ModSettings
- The Long Dark v2.54+

### Installation

1. Drop `AimbotMod_TLD.dll` into your `Mods` folder.
2. Launch the game. Configure both systems in the mod settings menu.

---

### Screenshots

*(Coming Soon)*

### Demo

*(SOON™)*
