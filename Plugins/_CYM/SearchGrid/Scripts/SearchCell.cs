using UnityEngine;
using System.Collections;
using System.Collections.Generic;



/// <summary>
/// This class is a search cell in a search grid. It is used to
/// contain search objects and keep track of their positions.
/// </summary>
namespace CYM.SearchGrid
{
    public class SearchCell
    {
        #region Private Member Variables

        /// <summary>
        /// This is the minimum world point that this search cell contains.
        /// </summary>
        private Vector3 m_oMinCellPoint;

        /// <summary>
        /// This is the maximum world point that this search cell contains.
        /// </summary>
        private Vector3 m_oMaxCellPoint;

        /// <summary>
        /// This is the center world point this search cell.
        /// </summary>
        private Vector3 m_oCenterCellPoint;

        /// <summary>
        /// This is the array of all adjacent cells. 1,1,1 is THIS CELL.
        /// </summary>
        private SearchCell[,,] m_aoAdjacentCells = new SearchCell[3, 3, 3];

        /// <summary>
        /// This is the list of all objects this cell contains.
        /// </summary>
        private List<SearchObject> m_oObjects = new List<SearchObject>();

        /// <summary>
        /// This is the grid to which this cell belongs.
        /// </summary>
        private SearchGrid m_oGrid;

        /// <summary>
        /// This is the x index of the grid that this cell is positioned at.
        /// </summary>
        private int m_nGridX;

        /// <summary>
        /// This is the y index of the grid that this cell is positioned at.
        /// </summary>
        private int m_nGridY;

        /// <summary>
        /// This is the z index of the grid that this cell is positioned at.
        /// </summary>
        private int m_nGridZ;

        #endregion

        #region Properties

        /// <summary>
        /// This gets the minimum world point that this search cell contains.
        /// </summary>
        public Vector3 MinCellPoint
        {
            get { return m_oMinCellPoint; }
        }

        /// <summary>
        /// This gets the maximum world point that this search cell contains.
        /// </summary>
        public Vector3 MaxCellPoint
        {
            get { return m_oMaxCellPoint; }
        }

        /// <summary>
        /// This gets the center world point of this cell.
        /// </summary>
        public Vector3 CenterCellPoint
        {
            get { return m_oCenterCellPoint; }
        }

        /// <summary>
        /// This gets the array of all adjacent cells.
        /// </summary>
        public SearchCell[,,] AdjacentCells
        {
            get { return m_aoAdjacentCells; }
        }

        /// <summary>
        /// This gets the list of all objects this cell contains.
        /// DO NOT ADD DIRECTLY TO THIS LIST!!!!
        /// </summary>
        public List<SearchObject> Objects
        {
            get { return m_oObjects; }
        }

        /// <summary>
        /// This gets the grid to which this cell belongs.
        /// </summary>
        public SearchGrid Grid
        {
            get { return m_oGrid; }
        }

        /// <summary>
        /// This gets the x index of the grid that this cell is positioned at.
        /// </summary>
        public int GridX
        {
            get { return m_nGridX; }
        }

        /// <summary>
        /// This gets the y index of the grid that this cell is positioned at.
        /// </summary>
        public int GridY
        {
            get { return m_nGridY; }
        }

        /// <summary>
        /// This gets the z index of the grid that this cell is positioned at.
        /// </summary>
        public int GridZ
        {
            get { return m_nGridZ; }
        }

        #endregion

        #region Init/Deinit Functions

        /// <summary>
        /// This is the main constructor for the search cell.
        /// </summary>
        /// <param name="oMinCellPoint">This is the min world point of this cell.</param>
        /// <param name="oMaxCellPoint">This is the max world point of this cell.</param>
        /// <param name="nGridX">This is the x index of the grid that this cell is positioned at.</param>
        /// <param name="nGridY">This is the y index of the grid that this cell is positioned at.</param>
        /// <param name="nGridZ">This is the z index of the grid that this cell is positioned at.</param>
        /// <param name="oGrid">This is the grid this cell belongs to.</param>
        public SearchCell(Vector3 oMinCellPoint, Vector3 oMaxCellPoint, int nGridX, int nGridY, int nGridZ, SearchGrid oGrid)
        {
            //set initial variables
            m_oMinCellPoint = oMinCellPoint;
            m_oMaxCellPoint = oMaxCellPoint;
            m_oCenterCellPoint = (m_oMinCellPoint + m_oMaxCellPoint) * 0.5f;
            m_nGridX = nGridX;
            m_nGridY = nGridY;
            m_nGridZ = nGridZ;

            //clear adjacency
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    for (int z = 0; z < 3; z++)
                    {
                        m_aoAdjacentCells[x, y, z] = null;
                    }
                }
            }
            m_aoAdjacentCells[1, 1, 1] = this;

            //set grids
            m_oGrid = oGrid;
        }

        #endregion

        #region Query Functions

        /// <summary>
        /// This checks whether or not a point is located within this cell.
        /// </summary>
        /// <param name="oWorldPoint">The world point to check.</param>
        /// <returns>Whether or not the point is in the cell.</returns>
        public bool IsPointInCell(Vector3 oWorldPoint)
        {
            if (oWorldPoint.x >= m_oMinCellPoint.x &&
                oWorldPoint.x <= m_oMaxCellPoint.x &&
                oWorldPoint.y >= m_oMinCellPoint.y &&
                oWorldPoint.y <= m_oMaxCellPoint.y &&
                oWorldPoint.z >= m_oMinCellPoint.z &&
                oWorldPoint.z <= m_oMaxCellPoint.z)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// This checks whether or not a point is located within this cell.
        /// </summary>
        /// <param name="fXPos">The world x point to check.</param>
        /// <param name="fYPos">The world y point to check.</param>
        /// <param name="fZPos">The world z point to check.</param>
        /// <returns>Whether or not the point is in the cell.</returns>
        public bool IsPointInCell(float fXPos, float fYPos, float fZPos)
        {
            if (fXPos >= m_oMinCellPoint.x &&
                fXPos <= m_oMaxCellPoint.x &&
                fYPos >= m_oMinCellPoint.y &&
                fYPos <= m_oMaxCellPoint.y &&
                fZPos >= m_oMinCellPoint.z &&
                fZPos <= m_oMaxCellPoint.z)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// This checks if a cell is adjacent to this one.
        /// </summary>
        /// <param name="oCell">The cell to check.</param>
        public bool IsCellAdjacent(SearchCell oCell)
        {
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    for (int z = 0; z < 3; z++)
                    {
                        if (oCell == m_aoAdjacentCells[x, y, z])
                            return true;
                    }
                }
            }
            return false;
        }

        #endregion

        #region Debug Functions

        /// <summary>
        /// This draws the debug for this search cell.
        /// </summary>
        /// <param name="drawCell">This indicates if the cell should be drawn.</param>
        /// <param name="drawObjects">This indicates if the objects should be drawn.</param>
        public void DrawDebug(bool drawCell, bool drawObjects)
        {
            //check if objects should be drawn
            if (drawObjects)
            {
                //loop through all objects and draw lines to them.
                for (int i = 0; i < m_oObjects.Count; i++)
                {
                    Debug.DrawLine(m_oCenterCellPoint, m_oObjects[i].transform.position, Color.blue);
                }
            }

            //check if the cell should be drawn
            if (drawCell)
            {
                //draw width lines
                Debug.DrawLine(new Vector3(m_oMinCellPoint.x, m_oMinCellPoint.y, m_oMinCellPoint.z),
                               new Vector3(m_oMaxCellPoint.x, m_oMinCellPoint.y, m_oMinCellPoint.z),
                               Color.yellow);
                Debug.DrawLine(new Vector3(m_oMinCellPoint.x, m_oMaxCellPoint.y, m_oMinCellPoint.z),
                               new Vector3(m_oMaxCellPoint.x, m_oMaxCellPoint.y, m_oMinCellPoint.z),
                               Color.yellow);
                Debug.DrawLine(new Vector3(m_oMinCellPoint.x, m_oMinCellPoint.y, m_oMaxCellPoint.z),
                               new Vector3(m_oMaxCellPoint.x, m_oMinCellPoint.y, m_oMaxCellPoint.z),
                               Color.yellow);
                Debug.DrawLine(new Vector3(m_oMinCellPoint.x, m_oMaxCellPoint.y, m_oMaxCellPoint.z),
                               new Vector3(m_oMaxCellPoint.x, m_oMaxCellPoint.y, m_oMaxCellPoint.z),
                               Color.yellow);

                //draw height lines
                Debug.DrawLine(new Vector3(m_oMinCellPoint.x, m_oMinCellPoint.y, m_oMinCellPoint.z),
                               new Vector3(m_oMinCellPoint.x, m_oMaxCellPoint.y, m_oMinCellPoint.z),
                               Color.yellow);
                Debug.DrawLine(new Vector3(m_oMaxCellPoint.x, m_oMinCellPoint.y, m_oMinCellPoint.z),
                               new Vector3(m_oMaxCellPoint.x, m_oMaxCellPoint.y, m_oMinCellPoint.z),
                               Color.yellow);
                Debug.DrawLine(new Vector3(m_oMinCellPoint.x, m_oMinCellPoint.y, m_oMaxCellPoint.z),
                               new Vector3(m_oMinCellPoint.x, m_oMaxCellPoint.y, m_oMaxCellPoint.z),
                               Color.yellow);
                Debug.DrawLine(new Vector3(m_oMaxCellPoint.x, m_oMinCellPoint.y, m_oMaxCellPoint.z),
                               new Vector3(m_oMaxCellPoint.x, m_oMaxCellPoint.y, m_oMaxCellPoint.z),
                               Color.yellow);

                //draw depth lines
                Debug.DrawLine(new Vector3(m_oMinCellPoint.x, m_oMinCellPoint.y, m_oMinCellPoint.z),
                               new Vector3(m_oMinCellPoint.x, m_oMinCellPoint.y, m_oMaxCellPoint.z),
                               Color.yellow);
                Debug.DrawLine(new Vector3(m_oMaxCellPoint.x, m_oMinCellPoint.y, m_oMinCellPoint.z),
                               new Vector3(m_oMaxCellPoint.x, m_oMinCellPoint.y, m_oMaxCellPoint.z),
                               Color.yellow);
                Debug.DrawLine(new Vector3(m_oMinCellPoint.x, m_oMaxCellPoint.y, m_oMinCellPoint.z),
                               new Vector3(m_oMinCellPoint.x, m_oMaxCellPoint.y, m_oMaxCellPoint.z),
                               Color.yellow);
                Debug.DrawLine(new Vector3(m_oMaxCellPoint.x, m_oMaxCellPoint.y, m_oMinCellPoint.z),
                               new Vector3(m_oMaxCellPoint.x, m_oMaxCellPoint.y, m_oMaxCellPoint.z),
                               Color.yellow);
            }
        }

        #endregion
    }
}