using Il2Cpp;
using UnityEngine;

namespace SilentAim
{
    /// <summary>
    /// Renders the 2D FOV circle overlay and target indicators using Unity's GL system.
    /// Called from MelonMod.OnGUI().
    /// </summary>
    public static class FovOverlay
    {
        private static Material _glMaterial;

        private static Material GetGLMaterial()
        {
            if (_glMaterial == null)
            {
                _glMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
                _glMaterial.hideFlags = HideFlags.HideAndDontSave;
                _glMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                _glMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                _glMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                _glMaterial.SetInt("_ZWrite", 0);
            }
            return _glMaterial;
        }

        /// <summary>
        /// Main render method — call from OnGUI.
        /// Draws FOV circle, target line, and target info.
        /// </summary>
        public static void Draw()
        {
            if (!Settings.Enabled) return;

            Vector2 center = new Vector2(Screen.width / 2f, Screen.height / 2f);

            // Draw FOV circle
            if (Settings.ShowFovCircle)
            {
                float radius = TargetingSystem.GetFovCircleRadiusPixels();
                DrawCircle(center, radius, Settings.FovCircleColor, 2f);
            }

            // Get current target
            var target = TargetingSystem.GetCurrentTarget();

            if (target.Found)
            {
                Camera cam = GameManager.GetMainCamera();
                if (cam != null)
                {
                    // Project target bone position to screen
                    Vector3 screenPos = cam.WorldToScreenPoint(target.BoneWorldPosition);

                    // WorldToScreenPoint Y is bottom-left (0,0), GUI is top-left (0,0)
                    Vector3 glPos = screenPos;
                    Vector3 guiPos = screenPos;
                    guiPos.y = Screen.height - guiPos.y;

                    // Only draw if target is in front of camera
                    if (screenPos.z > 0)
                    {
                        // Draw line from center to target (uses GL, bottom-left origin)
                        if (Settings.ShowTargetLine)
                        {
                            DrawLine(center, new Vector2(glPos.x, glPos.y),
                                     new Color(1f, 0.3f, 0.3f, 0.7f), 1.5f);
                        }

                        // Draw target reticle (small crosshair at target, uses GL)
                        if (Settings.ShowTargetReticle)
                        {
                            DrawTargetReticle(new Vector2(glPos.x, glPos.y),
                                             new Color(1f, 0.2f, 0.2f, 0.9f));
                        }

                        // Draw target info text (uses GUI, top-left origin)
                        if (Settings.ShowTargetInfo)
                        {
                            DrawTargetInfo(target, new Vector2(guiPos.x, guiPos.y));
                        }
                    }
                }
            }

            // Draw status text at top of screen
            DrawStatusHud(target);
        }

        /// <summary>
        /// Draws a circle using line segments with proper pixel-space GL matrix.
        /// </summary>
        private static void DrawCircle(Vector2 center, float radius, Color color, float thickness)
        {
            int segments = Mathf.Clamp((int)(radius * 0.7f), 32, 128); // Smooth scaling segments
            float angleStep = 360f / segments;

            var mat = GetGLMaterial();
            mat.SetPass(0);

            GL.PushMatrix();
            GL.LoadPixelMatrix(); // Set up correct pixel coordinate space

            GL.Begin(GL.TRIANGLE_STRIP);
            GL.Color(color);

            float innerRadius = radius - thickness / 2f;
            float outerRadius = radius + thickness / 2f;

            for (int i = 0; i <= segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                float cos = Mathf.Cos(angle);
                float sin = Mathf.Sin(angle);

                float xInner = center.x + cos * innerRadius;
                float yInner = center.y + sin * innerRadius;
                
                float xOuter = center.x + cos * outerRadius;
                float yOuter = center.y + sin * outerRadius;

                // Alternate between inner and outer ring to form a continuous triangle strip
                GL.Vertex3(xInner, yInner, 0);
                GL.Vertex3(xOuter, yOuter, 0);
            }

            GL.End();
            GL.PopMatrix();
        }

        /// <summary>
        /// Draws a line between two screen points using GL.
        /// </summary>
        private static void DrawLine(Vector2 from, Vector2 to, Color color, float thickness)
        {
            var mat = GetGLMaterial();
            mat.SetPass(0);

            GL.PushMatrix();
            GL.LoadPixelMatrix();

            GL.Begin(GL.LINES);
            GL.Color(color);

            GL.Vertex3(from.x, from.y, 0);
            GL.Vertex3(to.x, to.y, 0);

            for (float t = 1; t <= thickness; t++)
            {
                GL.Vertex3(from.x, from.y + t, 0);
                GL.Vertex3(to.x, to.y + t, 0);
            }

            GL.End();
            GL.PopMatrix();
        }

        /// <summary>
        /// Draws a small crosshair reticle at target screen position.
        /// </summary>
        private static void DrawTargetReticle(Vector2 pos, Color color)
        {
            float size = 8f;

            var mat = GetGLMaterial();
            mat.SetPass(0);

            GL.PushMatrix();
            GL.LoadPixelMatrix();

            GL.Begin(GL.LINES);
            GL.Color(color);

            // Horizontal line
            GL.Vertex3(pos.x - size, pos.y, 0);
            GL.Vertex3(pos.x + size, pos.y, 0);

            // Vertical line
            GL.Vertex3(pos.x, pos.y - size, 0);
            GL.Vertex3(pos.x, pos.y + size, 0);

            // Diamond shape
            GL.Vertex3(pos.x - size, pos.y, 0);
            GL.Vertex3(pos.x, pos.y - size, 0);

            GL.Vertex3(pos.x, pos.y - size, 0);
            GL.Vertex3(pos.x + size, pos.y, 0);

            GL.Vertex3(pos.x + size, pos.y, 0);
            GL.Vertex3(pos.x, pos.y + size, 0);

            GL.Vertex3(pos.x, pos.y + size, 0);
            GL.Vertex3(pos.x - size, pos.y, 0);

            GL.End();
            GL.PopMatrix();
        }

        /// <summary>
        /// Draws target information near the target reticle.
        /// </summary>
        private static void DrawTargetInfo(TargetingSystem.TargetResult target, Vector2 screenPos)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            style.normal.textColor = new Color(1f, 0.9f, 0.3f, 1f);

            // Shadow
            GUIStyle shadowStyle = new GUIStyle(style);
            shadowStyle.normal.textColor = new Color(0, 0, 0, 0.8f);

            string info = $"{target.AiName} [{target.BoneName}] {target.DistanceToTarget:F1}m";

            Rect shadowRect = new Rect(screenPos.x + 16, screenPos.y - 11, 300, 22);
            Rect textRect = new Rect(screenPos.x + 15, screenPos.y - 12, 300, 22);

            GUI.Label(shadowRect, info, shadowStyle);
            GUI.Label(textRect, info, style);
        }

        /// <summary>
        /// Draws the status HUD at the top-center of the screen.
        /// Shows enabled state, selected bone, and FOV.
        /// </summary>
        private static void DrawStatusHud(TargetingSystem.TargetResult target)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                richText = true
            };

            GUIStyle shadowStyle = new GUIStyle(style);
            shadowStyle.normal.textColor = new Color(0, 0, 0, 0.7f);

            string richHud = Settings.Enabled
                ? $"<color=#66FF66>SilentAim [ON]</color> | Bone: <color=#FFD700>{Settings.CurrentHitPointName}</color> | FOV: {Settings.AimFov:F0}°"
                : $"<color=#FF6666>SilentAim [OFF]</color>";

            if (target.Found && Settings.Enabled)
            {
                richHud += $" | <color=#FF9933>{target.AiName}</color>";
            }

            float hudWidth = 600;
            Rect shadowRect = new Rect(Screen.width / 2f - hudWidth / 2f + 1, 11, hudWidth, 25);
            Rect textRect = new Rect(Screen.width / 2f - hudWidth / 2f, 10, hudWidth, 25);

            GUI.Label(shadowRect, richHud, shadowStyle);
            GUI.Label(textRect, richHud, style);
        }
    }
}
