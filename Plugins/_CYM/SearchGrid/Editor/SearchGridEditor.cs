using UnityEngine;
using System.Collections;
using UnityEditor;

/// <summary>
/// This is the editor for the search grid
/// </summary>
namespace CYM.SearchGrid
{
    [CustomEditor(typeof(SearchGrid))]
    public class SearchGridEditor : Editor
    {
        /// <summary>
        /// This is the last scale of the search grid editor.
        /// </summary>
        private Vector3 m_oLastScale = Vector3.one;

        /// <summary>
        /// This is the last position of the search grid editor.
        /// </summary>
        private Vector3 m_oLastPosition = Vector3.one;

        /// <summary>
        /// This is called when the inspector gui should be run.
        /// </summary>
        public override void OnInspectorGUI()
        {
            GUI.changed = false;

            //get the grid
            SearchGrid grid = target as SearchGrid;
            if (grid)
            {
                //calc size limits and display slider
                float gridArea = grid.transform.localScale.x * grid.transform.localScale.y * grid.transform.localScale.z;
                float minCellSize = Mathf.Pow(gridArea / (float)SearchGrid.mc_nApproxMaxCells, 1.0f / 3.0f);
                float maxCellSize = Mathf.Min(grid.transform.localScale.x, grid.transform.localScale.y, grid.transform.localScale.z);
                float newCellSize = EditorGUILayout.Slider("Cell Size", grid.desiredCellSize, minCellSize, maxCellSize);
                if (newCellSize != grid.desiredCellSize ||
                    SearchGrid.Instance != grid ||
                    GUI.changed ||
                    m_oLastPosition != grid.transform.position ||
                    m_oLastScale != grid.transform.localScale)
                {
                    grid.desiredCellSize = newCellSize;
                    grid.Start();
                    EditorUtility.SetDirty(target);
                }

                //display stats
                EditorGUILayout.LabelField("Grid Center:", grid.CenterWorldPoint.ToString("0.000"));
                EditorGUILayout.LabelField("Grid Size:", grid.WorldSize.ToString("0.000"));
                EditorGUILayout.LabelField("Cell Count:", grid.NumCells + " (" + grid.GridWidth + "x" + grid.GridHeight + "x" + grid.GridDepth + ")");
                EditorGUILayout.Separator();

                //display debug check boxes
                grid.enableDebug = EditorGUILayout.BeginToggleGroup("Enable Debug", grid.enableDebug);
                grid.drawGridDebug = EditorGUILayout.Toggle("Draw Grid", grid.drawGridDebug);
                grid.drawCellDebug = EditorGUILayout.Toggle("Draw Cells", grid.drawCellDebug);
                grid.drawObjectDebug = EditorGUILayout.Toggle("Draw Objects", grid.drawObjectDebug);
                grid.drawSearchDebug = EditorGUILayout.Toggle("Draw Searches", grid.drawSearchDebug);
                grid.drawStatsDebug = EditorGUILayout.Toggle("Draw Stats", grid.drawStatsDebug);
                EditorGUILayout.EndToggleGroup();

                m_oLastPosition = grid.transform.position;
                m_oLastScale = grid.transform.localScale;
            }

            if (GUI.changed)
                EditorUtility.SetDirty(target);
        }

        public void OnSceneGUI()
        {
            SearchGrid grid = target as SearchGrid;
            if (grid == null)
                return;

            //draw width lines
            for (int y = 0; y <= grid.GridHeight; y++)
            {
                for (int z = 0; z <= grid.GridDepth; z++)
                {
                    Handles.DrawLine(grid.MinWorldPoint +
                                        new Vector3(0.0f,
                                                    grid.CellSize * (float)y,
                                                    grid.CellSize * (float)z),
                                   grid.MinWorldPoint +
                                        new Vector3(grid.WorldSize.x,
                                                    grid.CellSize * (float)y,
                                                    grid.CellSize * (float)z));

                }
            }

            //draw height lines
            for (int x = 0; x <= grid.GridWidth; x++)
            {
                for (int z = 0; z <= grid.GridDepth; z++)
                {
                    Handles.DrawLine(grid.MinWorldPoint +
                                        new Vector3(grid.CellSize * (float)x,
                                                    0.0f,
                                                    grid.CellSize * (float)z),
                                   grid.MinWorldPoint +
                                        new Vector3(grid.CellSize * (float)x,
                                                    grid.WorldSize.y,
                                                    grid.CellSize * (float)z));

                }
            }

            //draw depth lines
            for (int x = 0; x <= grid.GridWidth; x++)
            {
                for (int y = 0; y <= grid.GridHeight; y++)
                {
                    Handles.DrawLine(grid.MinWorldPoint +
                                        new Vector3(grid.CellSize * (float)x,
                                                    grid.CellSize * (float)y,
                                                    0.0f),
                                   grid.MinWorldPoint +
                                        new Vector3(grid.CellSize * (float)x,
                                                    grid.CellSize * (float)y,
                                                    grid.WorldSize.z));

                }
            }
        }
    }
}