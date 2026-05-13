using HarmonyLib;
using Il2Cpp;

namespace SilentAim.Patches
{
    /// <summary>
    /// Postfix on vp_FPSCamera.Update().
    /// Runs after the game has processed mouse input and set m_Yaw/m_Pitch,
    /// so our override always wins within the same Update cycle.
    /// The camera's own LateUpdate() then applies our angles to the transform.
    /// </summary>
    [HarmonyPatch(typeof(vp_FPSCamera), "Update")]
    public static class VectorAimCameraPatch
    {
        [HarmonyPostfix]
        public static void Postfix(vp_FPSCamera __instance)
        {
            VectorAimSystem.ApplyToCamera(__instance);
        }
    }
}
