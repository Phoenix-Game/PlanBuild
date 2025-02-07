﻿using System;
using System.Collections.Generic;
using System.Linq;
using Jotunn.Managers;
using PlanBuild.Plans;
using UnityEngine;
using Logger = Jotunn.Logger;
using Object = UnityEngine.Object;

namespace PlanBuild.Blueprints.Tools
{
    internal class DeleteObjectsComponent : ToolComponentBase
    {
        public override void OnUpdatePlacement(Player self)
        {
            if (!self.m_placementMarkerInstance)
            {
                return;
            }

            EnableSelectionProjector(self, true);

            float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
            if (scrollWheel != 0f)
            {
                if (ZInput.GetButton(Config.ShiftModifierButton.Name))
                {
                    UpdateCameraOffset(scrollWheel);
                }
                else
                {
                    UpdateSelectionRadius(scrollWheel);
                }
                UndoRotation(self, scrollWheel);
            }
        }

        public override void OnPlacePiece(Player self, Piece piece)
        {
            if (!Config.AllowTerrainmodConfig.Value && !SynchronizationManager.Instance.PlayerIsAdmin)
            {
                MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, "$msg_terrain_disabled");
                return;
            }

            int delcnt;
            if (ZInput.GetButton(Config.CtrlModifierButton.Name))
            {
                // Remove Pieces
                delcnt = RemoveObjects(
                    self.m_placementGhost.transform, SelectionRadius,
                    new Type[] { typeof(Piece) },
                    new Type[] { typeof(PlanPiece) });
            }
            else if (ZInput.GetButton(Config.AltModifierButton.Name))
            {
                // Remove All
                delcnt = RemoveObjects(
                    self.m_placementGhost.transform, SelectionRadius, null, new Type[]
                    { typeof(Character), typeof(TerrainModifier), typeof(ZSFX) });
            }
            else
            {
                // Remove Vegetation
                delcnt = RemoveObjects(
                    self.m_placementGhost.transform, SelectionRadius, null, new Type[]
                    { typeof(Character), typeof(TerrainModifier), typeof(ZSFX), typeof(Piece), typeof(ItemDrop)});
            }
            
            if (delcnt > 0)
            {
                MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, 
                    Localization.instance.Localize("$msg_removed_objects", delcnt.ToString()));
            }
        }

        private int RemoveObjects(Transform transform, float radius, Type[] includeTypes, Type[] excludeTypes)
        {
            Logger.LogDebug($"Entered RemoveVegetation {transform.position} / {radius}");

            int delcnt = 0;
            ZNetScene zNetScene = ZNetScene.instance;
            try
            {
                Vector3 startPosition = transform.position;

                if (Location.IsInsideNoBuildLocation(startPosition))
                {
                    return delcnt;
                }

                IEnumerable<GameObject> prefabs = Object.FindObjectsOfType<GameObject>()
                    .Where(obj => Vector3.Distance(startPosition, obj.transform.position) <= radius &&
                                  obj.GetComponent<ZNetView>() &&
                                  //obj.GetComponents<Component>().Select(x => x.GetType()) is Type[] comp &&
                                  (includeTypes == null || includeTypes.All(x => obj.GetComponent(x) != null)) &&
                                  (excludeTypes == null || excludeTypes.All(x => obj.GetComponent(x) == null)));

                foreach (GameObject prefab in prefabs)
                {
                    if (!prefab.TryGetComponent(out ZNetView zNetView))
                    {
                        continue;
                    }

                    zNetView.ClaimOwnership();
                    zNetScene.Destroy(prefab);
                    ++delcnt;
                }
                Jotunn.Logger.LogDebug($"Removed {delcnt} objects");
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"Error while removing objects: {ex}");
            }

            return delcnt;
        }
    }
}