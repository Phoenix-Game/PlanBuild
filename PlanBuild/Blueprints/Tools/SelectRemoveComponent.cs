﻿using UnityEngine;

namespace PlanBuild.Blueprints.Tools
{
    internal class SelectRemoveComponent : SelectionToolComponentBase
    {
        public override void OnUpdatePlacement(Player self)
        {
            if (!self.m_placementMarkerInstance)
            {
                return;
            }

            bool cameraModifier = ZInput.GetButton(Config.ShiftModifierButton.Name);
            bool radiusModifier = ZInput.GetButton(Config.CtrlModifierButton.Name);
            bool connectedModifier = ZInput.GetButton(Config.AltModifierButton.Name);

            if (radiusModifier && !connectedModifier)
            {
                EnableSelectionProjector(self);
                //BlueprintManager.HighlightPiecesInRadius(self.m_placementMarkerInstance.transform.position, SelectionRadius, Color.red);
            }
            else
            {
                DisableSelectionProjector();
            }

            float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
            if (scrollWheel != 0)
            {
                if (cameraModifier)
                {
                    UpdateCameraOffset(scrollWheel);
                }
                else if (radiusModifier && !connectedModifier)
                {
                    UpdateSelectionRadius(scrollWheel);
                }
                UndoRotation(self, scrollWheel);
            }
            
            if (ZInput.GetButtonDown(Config.ToggleButton.Name))
            {
                Player.m_localPlayer.m_buildPieces.LeftPiece();
                Player.m_localPlayer.SetupPlacementGhost();
            }
        }

        public override void OnPlacePiece(Player self, Piece piece)
        {
            bool cameraModifier = ZInput.GetButton(Config.ShiftModifierButton.Name);
            bool radiusModifier = ZInput.GetButton(Config.CtrlModifierButton.Name);
            bool connectedModifier = ZInput.GetButton(Config.AltModifierButton.Name);

            if (cameraModifier)
            {
                Selection.Instance.Clear();
            }
            else if (radiusModifier)
            {
                Selection.Instance.RemovePiecesInRadius(transform.position, SelectionRadius);
            }
            else if (BlueprintManager.LastHoveredPiece &&
                     BlueprintManager.CanCapture(BlueprintManager.LastHoveredPiece))
            {
                if (connectedModifier)
                {
                    Selection.Instance.RemoveGrowFromPiece(BlueprintManager.LastHoveredPiece);
                }
                else
                {
                    Selection.Instance.RemovePiece(BlueprintManager.LastHoveredPiece);
                }
            }
            UpdateDescription();
        }
    }
}