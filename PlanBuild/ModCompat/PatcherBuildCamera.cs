﻿using HarmonyLib;
using PlanBuild.Blueprints;

namespace PlanBuild.ModCompat
{
    class PatcherBuildCamera
    {   
        [HarmonyPatch(typeof(Valheim_Build_Camera.Valheim_Build_Camera), "IsTool")]
        [HarmonyPrefix]
        static bool ValheimBuildCamera_IsTool_Prefix(ItemDrop.ItemData itemData, ref bool __result)
        {
            if (itemData?.m_shared.m_name == BlueprintRunePrefab.BlueprintRuneItemName)
            {
                __result = true;
                return false;
            }
            return true;
        }

        internal static void UpdateCamera(On.GameCamera.orig_UpdateCamera orig, GameCamera self, float dt)
        {
            BlueprintManager.Instance.updateCamera = !Valheim_Build_Camera.Valheim_Build_Camera.InBuildMode();
            orig(self, dt);
            BlueprintManager.Instance.updateCamera = true;
        }
    }
}
