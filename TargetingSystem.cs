using Il2Cpp;
using MelonLoader;
using UnityEngine;
using Il2CppInterop.Runtime;

namespace AimMod
{
    /// <summary>
    /// Core targeting system that scans all alive AI entities,
    /// filters by FOV and range, and finds the best bone to aim at.
    /// </summary>
    public static class TargetingSystem
    {
        /// <summary>Result of a targeting scan.</summary>
        public struct TargetResult
        {
            public bool Found;
            public BaseAi Ai;
            public Vector3 BoneWorldPosition;
            public Vector3 DirectionFromCamera;
            public float DistanceToTarget;
            public float AngleFromCenter;
            public string AiName;
            public string BoneName;
        }

        // Cache for Silent Aim
        private static TargetResult _lastResult;
        private static int _lastFrameUpdated = -1;

        // Separate cache for Vector Aim (different FOV/bone/range settings)
        private static TargetResult _lastVectorAimResult;
        private static int _lastVectorAimFrame = -1;

        /// <summary>
        /// Returns the best target for Silent Aim. Cached per frame.
        /// </summary>
        public static TargetResult GetCurrentTarget()
        {
            int frame = Time.frameCount;
            if (frame == _lastFrameUpdated)
                return _lastResult;

            _lastFrameUpdated = frame;
            _lastResult = FindBestTarget(
                Settings.AimFov,
                Settings.MaxRange,
                Settings.CurrentBodyPartEnum,
                Settings.EnableVisibilityCheck);
            return _lastResult;
        }

        /// <summary>
        /// Returns the best target for Vector Aim using its own settings. Cached per frame.
        /// </summary>
        public static TargetResult GetVectorAimTarget()
        {
            int frame = Time.frameCount;
            if (frame == _lastVectorAimFrame)
                return _lastVectorAimResult;

            _lastVectorAimFrame = frame;
            _lastVectorAimResult = FindBestTarget(
                Settings.VectorAimFov,
                Settings.VectorAimMaxRange,
                Settings.VectorAimBodyPartEnum,
                Settings.VectorAimEnableVisibilityCheck);
            return _lastVectorAimResult;
        }

        /// <summary>
        /// Clears both caches, forcing a re-scan next call.
        /// </summary>
        public static void InvalidateCache()
        {
            _lastFrameUpdated = -1;
            _lastVectorAimFrame = -1;
        }

        /// <summary>
        /// Scans all alive AI in the scene and finds the best target
        /// within the given FOV cone, range, and targeting parameters.
        /// </summary>
        private static TargetResult FindBestTarget(float aimFov, float maxRange, int desiredBodyPart, bool visibilityCheck)
        {
            var result = new TargetResult { Found = false };

            Camera cam = GameManager.GetMainCamera();
            if (cam == null) return result;

            Transform camTransform = cam.transform;
            Vector3 camPos = camTransform.position;
            Vector3 camForward = camTransform.forward;
            float cameraFov = cam.fieldOfView;
            float maxFovAngle = aimFov / 2f;

            // Access all alive AI from the manager
            var aiList = BaseAiManager.m_BaseAis;
            if (aiList == null) return result;

            float bestAngle = float.MaxValue;

            for (int i = 0; i < aiList.Count; i++)
            {
                BaseAi ai;
                try
                {
                    ai = aiList[i];
                    if (ai == null) continue;
                    if (ai.m_CurrentMode == AiMode.Dead || ai.m_CurrentMode == AiMode.Disabled)
                        continue;

                    // Filter by animal type
                    switch (ai.m_AiSubType)
                    {
                        case AiSubType.Wolf: if (!Settings.TargetWolves) continue; break;
                        case AiSubType.Bear: if (!Settings.TargetBears) continue; break;
                        case AiSubType.Stag: if (!Settings.TargetStags) continue; break;
                        case AiSubType.Rabbit: if (!Settings.TargetRabbits) continue; break;
                        case AiSubType.Moose: if (!Settings.TargetMoose) continue; break;
                        case AiSubType.Cougar: if (!Settings.TargetCougars) continue; break;
                        default: break;
                    }
                }
                catch
                {
                    continue;
                }

                // Try to get the desired bone position
                Vector3 bonePos;
                string boneName;
                if (!TryGetBonePosition(ai, desiredBodyPart, out bonePos, out boneName))
                {
                    // Fallback: try head, then any available bone
                    if (!TryGetBonePosition(ai, 0, out bonePos, out boneName))
                    {
                        // Use AI center as last resort
                        try
                        {
                            bonePos = ai.transform.position + Vector3.up * 0.5f;
                            boneName = "Center";
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }

                // Distance check
                Vector3 dirToTarget = bonePos - camPos;
                float distance = dirToTarget.magnitude;
                if (distance > maxRange || distance < 1f) continue;

                // Angle check (FOV cone)
                float angle = Vector3.Angle(camForward, dirToTarget.normalized);
                if (angle > maxFovAngle) continue;

                // Visibility Check (Line of Sight)
                if (visibilityCheck)
                {
                    int layerMask = (1 << vp_Layer.Default) | 
                                    (1 << vp_Layer.Ground) | 
                                    (1 << vp_Layer.TerrainObject) | 
                                    (1 << vp_Layer.Buildings) | 
                                    (1 << vp_Layer.InteractiveProp);

                    if (Physics.Linecast(camPos, bonePos, out RaycastHit hit, layerMask))
                    {
                        // If the ray hits environment before the target (with a 0.75m tolerance to avoid clipping issues)
                        if (Vector3.Distance(camPos, hit.point) < distance - 0.75f)
                        {
                            continue;
                        }
                    }
                }

                // Pick the closest to crosshair (smallest angle)
                if (angle < bestAngle)
                {
                    bestAngle = angle;
                    result.Found = true;
                    result.Ai = ai;
                    result.BoneWorldPosition = bonePos;
                    result.DirectionFromCamera = dirToTarget.normalized;
                    result.DistanceToTarget = distance;
                    result.AngleFromCenter = angle;
                    result.BoneName = boneName;

                    try
                    {
                        result.AiName = ai.m_AiSubType.ToString();
                    }
                    catch
                    {
                        result.AiName = "Unknown";
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Finds the world position of a specific body part on an AI entity
        /// by scanning its LocalizedDamage child components.
        /// </summary>
        private static bool TryGetBonePosition(BaseAi ai, int bodyPartEnum, out Vector3 position, out string boneName)
        {
            position = Vector3.zero;
            boneName = "";

            try
            {
                var localizedDamageComponents = ai.gameObject.GetComponentsInChildren<LocalizedDamage>(true);
                if (localizedDamageComponents == null || localizedDamageComponents.Length == 0)
                    return false;

                for (int i = 0; i < localizedDamageComponents.Length; i++)
                {
                    var ld = localizedDamageComponents[i];
                    if (ld == null) continue;

                    if ((int)ld.m_BodyPart == bodyPartEnum)
                    {
                        position = ld.transform.position;
                        boneName = ld.m_BodyPart.ToString();
                        return true;
                    }
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Warning($"[SilentAim] Error getting bone position: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Returns all bone positions for an AI entity (for potential ESP features).
        /// </summary>
        public static System.Collections.Generic.Dictionary<string, Vector3> GetAllBonePositions(BaseAi ai)
        {
            var bones = new System.Collections.Generic.Dictionary<string, Vector3>();

            try
            {
                var localizedDamageComponents = ai.gameObject.GetComponentsInChildren<LocalizedDamage>(true);
                if (localizedDamageComponents == null) return bones;

                for (int i = 0; i < localizedDamageComponents.Length; i++)
                {
                    var ld = localizedDamageComponents[i];
                    if (ld == null) continue;

                    string partName = ld.m_BodyPart.ToString();
                    if (!bones.ContainsKey(partName))
                    {
                        bones[partName] = ld.transform.position;
                    }
                }
            }
            catch { }

            return bones;
        }

        /// <summary>
        /// Calculates the screen-space FOV circle radius in pixels for Silent Aim.
        /// </summary>
        public static float GetFovCircleRadiusPixels() =>
            GetFovCircleRadiusPixels(Settings.AimFov);

        /// <summary>
        /// Calculates the screen-space FOV circle radius in pixels for a given aim FOV.
        /// radius = screenHalfHeight * tan(aimHalfFov) / tan(cameraHalfFov)
        /// </summary>
        public static float GetFovCircleRadiusPixels(float aimFovDegrees)
        {
            Camera cam = GameManager.GetMainCamera();
            if (cam == null) return 100f;

            float screenHalfHeight = Screen.height / 2f;
            float tanAim = Mathf.Tan(aimFovDegrees / 2f * Mathf.Deg2Rad);
            float tanCam = Mathf.Tan(cam.fieldOfView / 2f * Mathf.Deg2Rad);

            if (tanCam <= 0f) return 100f;

            return screenHalfHeight * tanAim / tanCam;
        }
    }
}
