﻿using Jotunn.Managers;
using PlanBuild.Blueprints.Marketplace;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace PlanBuild.Blueprints
{
    internal class SelectionTools
    {
        public static void Copy(bool captureVanillaSnapPoints)
        {
            var bp = new Blueprint();
            bp.ID = $"__{BlueprintManager.TemporaryBlueprints.Count + 1:000}";
            bp.Name = bp.ID;
            bp.Category = BlueprintAssets.CategoryClipboard;
            bp.Capture(Selection.Instance, captureVanillaSnapPoints);
            bp.CreatePiece();
            BlueprintManager.TemporaryBlueprints.Add(bp.ID, bp);
            Selection.Instance.Clear();
            bp.CreateThumbnail(flush: false);
            Player.m_localPlayer.UpdateKnownRecipesList();
            Player.m_localPlayer.UpdateAvailablePiecesList();
            int cat = (int)PieceManager.Instance.GetPieceCategory(BlueprintAssets.CategoryClipboard);
            List<Piece> reorder = Player.m_localPlayer.m_buildPieces.m_availablePieces[cat].OrderByDescending(x => x.name).ToList();
            Player.m_localPlayer.m_buildPieces.m_availablePieces[cat] = reorder;
            Player.m_localPlayer.m_buildPieces.m_selectedCategory = (Piece.PieceCategory)cat;
            Player.m_localPlayer.m_buildPieces.SetSelected(new Vector2Int(0, 0));
            Player.m_localPlayer.SetupPlacementGhost();
        }

        public static void Save()
        {
            var bp = new Blueprint();
            var bpname = Selection.Instance.BlueprintInstance?.ID;
            bpname ??= $"blueprint{BlueprintManager.LocalBlueprints.Count + 1:000}";

            if (bp.Capture(Selection.Instance))
            {
                TextInput.instance.m_queuedSign = new BlueprintSaveGUI(bp);
                TextInput.instance.Show(Localization.instance.Localize("$msg_bpcapture_save", bp.GetPieceCount().ToString()), bpname, 50);
            }
            else
            {
                Jotunn.Logger.LogWarning($"Could not capture blueprint {bpname}");
            }
        }

        public static void Delete()
        {
            var toClear = Selection.Instance.ToList();
            Selection.Instance.Clear();
            foreach (var zdoid in toClear)
            {
                var go = ZNetScene.instance.FindInstance(zdoid);
                if (go)
                {
                    ZNetScene.instance.Destroy(go);
                }
            }
        }

        /// <summary>
        ///     Helper class for naming and saving a captured blueprint via GUI
        ///     Implements the Interface <see cref="TextReceiver" />. SetText is called from <see cref="TextInput" /> upon entering
        ///     an name for the blueprint.<br />
        ///     Save the actual blueprint and add it to the list of known blueprints.
        /// </summary>
        internal class BlueprintSaveGUI : TextReceiver
        {
            private Blueprint newbp;

            public BlueprintSaveGUI(Blueprint bp)
            {
                newbp = bp;
            }

            public string GetText()
            {
                return newbp.Name;
            }

            public void SetText(string text)
            {
                if (string.IsNullOrEmpty(text))
                {
                    return;
                }

                string playerName = Player.m_localPlayer.GetPlayerName();
                string fileName = string.Concat(text.Split(Path.GetInvalidFileNameChars()));

                newbp.ID = $"{playerName}_{fileName}".Trim();
                newbp.Name = text;
                newbp.Creator = playerName;
                newbp.FileLocation = Path.Combine(Config.BlueprintSaveDirectoryConfig.Value, newbp.ID + ".blueprint");
                newbp.ThumbnailLocation = newbp.FileLocation.Replace(".blueprint", ".png");

                if (BlueprintManager.LocalBlueprints.TryGetValue(newbp.ID, out var oldbp))
                {
                    oldbp.DestroyBlueprint();
                    BlueprintManager.LocalBlueprints.Remove(newbp.ID);
                }

                if (!newbp.ToFile())
                {
                    return;
                }

                if (!newbp.CreatePiece())
                {
                    return;
                }

                BlueprintManager.LocalBlueprints.Add(newbp.ID, newbp);
                Selection.Instance.Clear();
                newbp.CreateThumbnail();
                BlueprintManager.RegisterKnownBlueprints();
                BlueprintGUI.ReloadBlueprints(BlueprintLocation.Local);
            }
        }
    }
}
