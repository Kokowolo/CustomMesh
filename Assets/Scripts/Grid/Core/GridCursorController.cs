/*
 * File Name: GridCursorController.cs
 * Description: This script is for ...
 * 
 * Author(s): Kokowolo, Will Lacey
 * Date Created: August 24, 2022
 * 
 * Additional Comments:
 *      File Line Length: 120
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using Kokowolo.Utilities;
// using Kokowolo.Lutro;
// using Kokowolo.Lutro.Combat;

namespace Kokowolo.Grid
{
    public class GridCursorController : MonoSingleton<GridCursorController>
    {
        /************************************************************/
        #region Events

        public static event EventHandler<GridCursorControllerEventArgs> OnCoordinatesChanged;

        public class GridCursorControllerEventArgs : EventArgs
        {
            public GridCoordinates previous;
            public GridCoordinates current;
        }

        #endregion
        /************************************************************/
        #region Fields

        private GridCoordinates nextCoordinates = GridCoordinates.Invalid;
        private GridCursorControllerEventArgs eventArgs = new GridCursorControllerEventArgs();

        private GridMapVisualJob visualJob;

        #endregion
        /************************************************************/
        #region Properties

        public GridCoordinates Coordinates { get; private set; } = GridCoordinates.Invalid;

        /// <summary>
        /// Get the Cell on Coordinates, which might be null
        /// </summary>
        public GridCell Cell => GridManager.Map.GetCell(Coordinates);
        
        /// <summary>
        /// Get the Unit on Coordinates, which might be null
        /// </summary>
        // public Unit Unit => Cell.GetUnit();

        #endregion
        /************************************************************/
        #region Functions

        protected override void MonoSingleton_Awake()
        {
            GridManager.OnGridEnabled += Handle_GridManager_OnGridEnabled;
            GridManager.OnGridDisabled += Handle_GridManager_OnGridDisabled;
        }

        protected override void MonoSingleton_OnDestroy()
        {
            GridManager.OnGridEnabled -= Handle_GridManager_OnGridEnabled;
            GridManager.OnGridDisabled -= Handle_GridManager_OnGridDisabled;
        }

        private void LateUpdate() 
        {
            UpdateGridCoordinates();
        }

        private GridCoordinates GetCoordinatesFromScreenPoint()
        {
            // HACK: move this to Grid package
            GridCoordinates coordinates = GridCoordinates.Invalid;
            if (CursorManager.HasValidHitInfo)
            {
                // if (CursorManager.HitInfo.transform.gameObject.IsInLayerMask(LayerManager.UnitLayerMask))
                // {
                //     coordinates = CursorManager.HitInfo.transform.GetComponentInParent<Unit>().Transform.Coordinates;
                // }
                // else
                // {
                    coordinates = GridCoordinates.GetGridCoordinates(CursorManager.HitInfo.point);
                // }
            }   
            return coordinates;
        }

        private void UpdateGridCoordinates()
        {
            nextCoordinates = GetCoordinatesFromScreenPoint();
            // Debug.Log($"GridCursorController {nextCoordinates}");
            if (GridManager.Map.Contains(nextCoordinates) && !GridManager.Map.GetCell(nextCoordinates).IsExplorable) 
            {
                nextCoordinates = GridCoordinates.Invalid;
            }

            if (BaseInputManager.IsMouseOverUI()) 
            {
                nextCoordinates = GridCoordinates.Invalid;
            }
            else
            {
                if (!GridManager.Map.Contains(nextCoordinates) || nextCoordinates == Coordinates) return;

                eventArgs.previous = Coordinates;
                eventArgs.current = nextCoordinates;
                Coordinates = nextCoordinates;
                OnCoordinatesChanged?.Invoke(this, eventArgs);
                UpdateGridMapVisual();
            }        
        }

        private void UpdateGridMapVisual()
        {
            if (visualJob == null) 
            {
                List<GridCoordinates> list = new List<GridCoordinates>() { Coordinates };
                visualJob = GridManager.Visual.CreateVisualJob(
                    GridMapVisualJob.JobType.Singles, 
                    list,
                    priority: 10
                );
            }
            else
            {
                List<GridCoordinates> list = new List<GridCoordinates>() { Coordinates };
                visualJob.Update(list);
            }
        }

        private void Handle_GridManager_OnGridEnabled(object sender, EventArgs e)
        {
            gameObject.SetActive(true);
            // CursorManager.LayerMask = LayerManager.GridCursorLayerMask;
        }

        private void Handle_GridManager_OnGridDisabled(object sender, EventArgs e)
        {
            gameObject.SetActive(false);
        }

        #endregion
        /************************************************************/
    }
}