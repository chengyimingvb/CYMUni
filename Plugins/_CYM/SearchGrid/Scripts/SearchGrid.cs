using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

/// <summary>
/// This class is used to manage objects in the scene to allow more effecient object search operations.
/// </summary>
namespace CYM.SearchGrid
{
    public class SearchGrid : MonoBehaviour
    {
        #region Constant Members

        /// <summary>
        /// This is the approximate max number of cells that is allowed in a grid.
        /// </summary>
        public const int mc_nApproxMaxCells = 5000;

        #endregion

        #region Static Members

        /// <summary>
        /// This is the global list that is used to return object queries.  This class
        /// uses a global list for queries to reduce memory usage and GC calls.
        /// </summary>
        private static List<SearchObject> ms_oGlobalQueryList = new List<SearchObject>(100);

        /// <summary>
        /// This is the global instance of the search grid.  Only one grid should be created at a time.
        /// </summary>
        private static SearchGrid ms_oGlobalInstance = null;

        /// <summary>
        /// This gets the global instance of the search grid.  Only one grid should be created at a time.
        /// </summary>
        public static SearchGrid Instance
        {
            get { return ms_oGlobalInstance; }
        }

        #endregion

        #region Defaults Static Class

        /// <summary>
        /// This internal class holds all of the defaults of the SearchGrid class. 
        /// </summary>
        public static class Defaults
        {
            public const float gridSize = 250.0f;
            public const float cellSize = 50.0f;
        }

        #endregion

        #region Public Members

        /// <summary>
        /// This is the desired size of the cells for this search grid.
        /// </summary>
        public float desiredCellSize = 0.1f;

        /// <summary>
        /// This indicates if debug should be enabled.
        /// </summary>
        public bool enableDebug = false;

        /// <summary>
        /// This indicates if the grid debug should be drawn.
        /// </summary>
        public bool drawGridDebug = false;

        /// <summary>
        /// This indicates if the cell debug should be drawn.
        /// </summary>
        public bool drawCellDebug = false;

        /// <summary>
        /// This indicates if the object debug should be drawn.
        /// </summary>
        public bool drawObjectDebug = false;

        /// <summary>
        /// This indicates if the search debug should be drawn.
        /// </summary>
        public bool drawSearchDebug = false;

        /// <summary>
        /// This indicates if stats should be drawn.
        /// </summary>
        public bool drawStatsDebug = false;

        /// <summary>
        /// This is the debug texture to use when drawing the debug frames.  This should just be a plain white texture. 
        /// </summary>
        public Texture debugTexture = null;

        #endregion

        #region Private Members

        /// <summary>
        /// This is the 3d array that contains all of the cells in the search grid.
        /// </summary>
        private SearchCell[,,] m_aoSearchCells;

        /// <summary>
        /// This is the grid width in number of cells.
        /// </summary>
        private int m_nGridWidth;

        /// <summary>
        /// This is the grid height in number of cells.
        /// </summary>
        private int m_nGridHeight;

        /// <summary>
        /// This is the grid depth in number of cells.
        /// </summary>
        private int m_nGridDepth;

        /// <summary>
        /// This is the minimum world point that this search grid contains.
        /// </summary>
        private Vector3 m_oMinWorldPoint;

        /// <summary>
        /// This is the maximum world point that this search grid contains.
        /// </summary>
        private Vector3 m_oMaxWorldPoint;

        /// <summary>
        /// This is the center world point of this search grid.
        /// </summary>
        private Vector3 m_oCenterWorldPoint;

        /// <summary>
        /// This is the size of this search grid in the world.
        /// </summary>
        private Vector3 m_oWorldSize;

        /// <summary>
        /// This is the size of the cells in this search grid.
        /// </summary>
        private float m_fCellSize;

        /// <summary>
        /// This is the list of game objects in this search grid.
        /// </summary>
        private List<SearchObject> m_oObjects = new List<SearchObject>();

        /// <summary>
        /// This is the number of cells in this search grid.
        /// </summary>
        private int m_nNumCells;

        /// <summary>
        /// This is the maximum size of any object currently in this search grid.
        /// </summary>
        private float m_fMaxObjectRadius = 0.0f;

        /// <summary>
        /// This list is used to store searches that were performed while search debug is on.
        /// </summary>
        private List<SearchDebug> m_oDebugSearches = new List<SearchDebug>();

        /// <summary>
        /// This is used to store per frame search statistics while search debug is on.
        /// </summary>
        private SearchStats[] m_aoSearchStats = new SearchStats[90];

        /// <summary>
        /// This is the current index of the search stats array.
        /// </summary>
        private int m_nSearchStatsIndex = 0;

        /// <summary>
        /// This is the amount of time spent updating the search grid this frame.  Only updated when debugging is enabled.
        /// </summary>
        private float m_fTimeSpentUpdating = 0.0f;

        #endregion

        #region Properties

        /// <summary>
        /// This gets 2d array that contains all of the cells in the search grid.
        /// </summary>
        public SearchCell[,,] SearchCells
        {
            get { return m_aoSearchCells; }
        }

        /// <summary>
        /// This gets the grid width in number of cells.
        /// </summary>
        public int GridWidth
        {
            get { return m_nGridWidth; }
        }

        /// <summary>
        /// This gets the grid height in number of cells.
        /// </summary>
        public int GridHeight
        {
            get { return m_nGridHeight; }
        }

        /// <summary>
        /// This gets the grid depth in number of cells
        /// </summary>
        public int GridDepth
        {
            get { return m_nGridDepth; }
        }

        /// <summary>
        /// This gets the minimum world point that this search grid contains.
        /// </summary>
        public Vector3 MinWorldPoint
        {
            get { return m_oMinWorldPoint; }
        }

        /// <summary>
        /// This gets the maximum world point that this search grid contains.
        /// </summary>
        public Vector3 MaxWorldPoint
        {
            get { return m_oMaxWorldPoint; }
        }

        /// <summary>
        /// This gets the center world point of this search grid.
        /// </summary>
        public Vector3 CenterWorldPoint
        {
            get { return m_oCenterWorldPoint; }
        }

        /// <summary>
        /// This gets the size of this grid in the world.
        /// </summary>
        public Vector3 WorldSize
        {
            get { return m_oWorldSize; }
        }

        /// <summary>
        /// This gets the size of the cells in this search grid.
        /// </summary>
        public float CellSize
        {
            get { return m_fCellSize; }
        }

        /// <summary>
        /// This gets the number of cells in this search grid.
        /// </summary>
        public int NumCells
        {
            get { return m_nNumCells; }
        }

        /// <summary>
        /// This gets the list of game objects in this search grid.
        /// </summary>
        public ReadOnlyCollection<SearchObject> Objects
        {
            get { return m_oObjects.AsReadOnly(); }
        }

        #endregion

        #region Init/Deinit Functions

        /// <summary>
        /// This is called with this script needs to be reset.
        /// </summary>
        public void Reset()
        {
            if (transform != null)
            {
                transform.localScale = new Vector3(Defaults.gridSize, Defaults.gridSize, Defaults.gridSize);
            }
            desiredCellSize = Defaults.cellSize;
            debugTexture = null;
        }

        /// <summary>
        /// This is called when this script is started.
        /// </summary>
        public void Start()
        {
            //set this as the global instance
            if (ms_oGlobalInstance != null &&
                ms_oGlobalInstance != this)
            {
                Debug.LogWarning("An existing SearchGrid is already created and will be overriden.  Only one SearchGrid should be created at a time.");
            }
            ms_oGlobalInstance = this;

            //reset all debug stats
            for (int i = 0; i < m_aoSearchStats.Length; i++)
            {
                m_aoSearchStats[i].Reset();
            }

            //try to load 1x1white texture
            if (debugTexture == null)
                debugTexture = (Texture)Resources.Load("1x1White");

            //set the initial grid size.
            Vector3 desiredGridSize = transform.localScale;

            //validate grid size
            desiredGridSize.x = Validate(desiredGridSize.x, 0.01f, float.PositiveInfinity, 1.0f, this, "gridSize.x", true);
            desiredGridSize.y = Validate(desiredGridSize.y, 0.01f, float.PositiveInfinity, 1.0f, this, "gridSize.y", true);
            desiredGridSize.z = Validate(desiredGridSize.z, 0.01f, float.PositiveInfinity, 1.0f, this, "gridSize.z", true);

            //validate cell size
            float gridArea = transform.localScale.x * transform.localScale.y * transform.localScale.z;
            float minCellSize = Mathf.Pow(gridArea / (float)SearchGrid.mc_nApproxMaxCells, 1.0f / 3.0f);
            float maxCellSize = Mathf.Min(transform.localScale.x, transform.localScale.y, transform.localScale.z);
            if (minCellSize > maxCellSize)
                minCellSize = maxCellSize;
            desiredCellSize = Validate(desiredCellSize, minCellSize, maxCellSize, Mathf.Lerp(minCellSize, maxCellSize, 0.25f), this, "cellSize", true);

            //setup grid
            Setup(transform.position - desiredGridSize * 0.5f, transform.position + desiredGridSize * 0.5f, desiredCellSize);
        }

        /// <summary>
        /// This sets up this search grid.
        /// </summary>
        /// <param name="oMinWorldPoint">The minumum world point this grid is to contain.</param>
        /// <param name="oMaxWorldPoint">The maximum world point this grid is to contain.</param>
        /// <param name="fCellSize">This is the size of the cells in this search grid.</param>
        public void Setup(Vector3 oMinWorldPoint, Vector3 oMaxWorldPoint, float fCellSize)
        {
            m_oMinWorldPoint = oMinWorldPoint;
            m_oMaxWorldPoint = oMaxWorldPoint;
            m_fCellSize = fCellSize;

            //check if x size needs to be updated
            float fXSizeRemainer = (m_oMaxWorldPoint.x - m_oMinWorldPoint.x) % m_fCellSize;
            if (fXSizeRemainer != 0.0f)
            {
                m_oMinWorldPoint.x -= (m_fCellSize - fXSizeRemainer) * 0.5f;
                m_oMaxWorldPoint.x += (m_fCellSize - fXSizeRemainer) * 0.5f;
            }

            //check if y size needs to be updated
            float fYSizeRemainer = (m_oMaxWorldPoint.y - m_oMinWorldPoint.y) % m_fCellSize;
            if (fYSizeRemainer != 0.0f)
            {
                m_oMinWorldPoint.y -= (m_fCellSize - fYSizeRemainer) * 0.5f;
                m_oMaxWorldPoint.y += (m_fCellSize - fYSizeRemainer) * 0.5f;
            }

            //check if y size needs to be updated
            float fZSizeRemainer = (m_oMaxWorldPoint.z - m_oMinWorldPoint.z) % m_fCellSize;
            if (fZSizeRemainer != 0.0f)
            {
                m_oMinWorldPoint.z -= (m_fCellSize - fZSizeRemainer) * 0.5f;
                m_oMaxWorldPoint.z += (m_fCellSize - fZSizeRemainer) * 0.5f;
            }

            //set center world point
            m_oCenterWorldPoint = (m_oMinWorldPoint + m_oMaxWorldPoint) * 0.5f;

            //set world size
            m_oWorldSize = m_oMaxWorldPoint - m_oMinWorldPoint;

            //get grid size values
            float fWorldWidth = m_oMaxWorldPoint.x - m_oMinWorldPoint.x;
            float fWorldHeight = m_oMaxWorldPoint.y - m_oMinWorldPoint.y;
            float fWorldDepth = m_oMaxWorldPoint.z - m_oMinWorldPoint.z;
            m_nGridWidth = (int)Mathf.Round(fWorldWidth / m_fCellSize);
            m_nGridHeight = (int)Mathf.Round(fWorldHeight / m_fCellSize);
            m_nGridDepth = (int)Mathf.Round(fWorldDepth / m_fCellSize);

            //create cell arrays and cells
            m_aoSearchCells = new SearchCell[m_nGridWidth, m_nGridHeight, m_nGridDepth];
            m_nNumCells = m_nGridWidth * m_nGridHeight * m_nGridDepth;
            if (m_nNumCells > mc_nApproxMaxCells * 2)
            {
                Debug.LogWarning("Too many cells in this SearchGrid, not initializing the object!");
                return;
            }
            for (int x = 0; x < m_nGridWidth; x++)
            {
                for (int y = 0; y < m_nGridHeight; y++)
                {
                    for (int z = 0; z < m_nGridDepth; z++)
                    {
                        Vector3 oCellMinPoint = new Vector3(m_oMinWorldPoint.x + x * m_fCellSize,
                                                            m_oMinWorldPoint.y + y * m_fCellSize,
                                                            m_oMinWorldPoint.z + z * m_fCellSize);
                        Vector3 oCellMaxPoint = new Vector3(oCellMinPoint.x + m_fCellSize,
                                                            oCellMinPoint.y + m_fCellSize,
                                                            oCellMinPoint.z + m_fCellSize);
                        m_aoSearchCells[x, y, z] = new SearchCell(oCellMinPoint, oCellMaxPoint, x, y, z, this);
                    }
                }
            }

            //setup cell adjacency
            for (int x = 0; x < m_nGridWidth; x++)
            {
                for (int y = 0; y < m_nGridHeight; y++)
                {
                    for (int z = 0; z < m_nGridDepth; z++)
                    {
                        //get the target cell
                        SearchCell cell = m_aoSearchCells[x, y, z];

                        //loop through all adjacent tiles
                        for (int xd = -1; xd <= 1; xd++)
                        {
                            for (int yd = -1; yd <= 1; yd++)
                            {
                                for (int zd = -1; zd <= 1; zd++)
                                {
                                    if (xd != 0 && yd != 0 && zd != 0)
                                    {
                                        int adjX = x + xd;
                                        int adjY = y + yd;
                                        int adjZ = z + zd;

                                        //make sure it is valid
                                        if (adjX >= 0 && adjX < m_nGridWidth &&
                                            adjY >= 0 && adjY < m_nGridHeight &&
                                            adjZ >= 0 && adjZ < m_nGridDepth)
                                        {
                                            cell.AdjacentCells[xd + 1, yd + 1, zd + 1] = m_aoSearchCells[adjX, adjY, adjZ];
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            /*
            Debug.Log("SearchGrid Created...\n" +
                      "\tGrid Center: " + m_oCenterWorldPoint.ToString() + "\n" +
                      "\t  Grid Size: " + (m_oMaxWorldPoint - m_oMinWorldPoint).ToString("0.00000") + "\n" +
                      "\t  Cell Size: " + m_fCellSize.ToString() + "\n" +
                      "\t Cell Count: " + m_nNumCells + " (" + m_nGridWidth + "x" + m_nGridHeight + "x" + m_nGridDepth + ")");
             */
        }

        /// <summary>
        /// This is called when this script is destroyed.
        /// </summary>
        public void OnDestroy()
        {
            if (ms_oGlobalInstance != this)
            {
                Debug.LogWarning("The current global SearchGrid is NOT this SearchGrid.  This indicates that there are more than one grid object!  Please make sure there is only one created!");
            }
            else
            {
                ms_oGlobalInstance = null;
            }
        }

        #endregion

        #region Search Debug

        /// <summary>
        /// This class is used to capture a single search requested when search debug is on.
        /// </summary>
        public class SearchDebug
        {
            /// <summary>
            /// This is the position of the search.
            /// </summary>
            public Vector3 position = Vector3.zero;

            /// <summary>
            /// This is the radius of the search.
            /// </summary>
            public float radius = 1.0f;

            /// <summary>
            /// This is the number of cells that were searched.
            /// </summary>
            public int numCells = 0;

            /// <summary>
            /// This is the position of the all the objects that were checked.
            /// </summary>
            public List<Vector3> objectPositions = new List<Vector3>();

            /// <summary>
            /// This is all of the results of the objects that were checked.  True indicates in the search, false is out of it.
            /// </summary>
            public List<bool> objectResults = new List<bool>();

            /// <summary>
            /// This is the amount of time this search took.  (in seconds)
            /// </summary>
            public float timeSpent = 0.0f;

            /// <summary>
            /// This draws the debug for this search debug.
            /// </summary>
            public void DrawDebug()
            {
                //draw objects
                for (int i = 0; i < objectPositions.Count; i++)
                {
                    Gizmos.color = objectResults[i] ? Color.green : Color.red;
                    Gizmos.DrawLine(position, objectPositions[i]);
                }

                //draw radius
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(position, radius);
            }
        }

        /// <summary>
        /// This holds all of the search stats for all of the searchss in a single frame.
        /// </summary>
        public struct SearchStats
        {
            /// <summary>
            /// This is the number of searchs that occured.
            /// </summary>
            public int numSearches;

            /// <summary>
            /// This is the number of cells that were searched.
            /// </summary>
            public int numCellsSearched;

            /// <summary>
            /// This is the number of containment checks.
            /// </summary>
            public int numContainmentChecks;

            /// <summary>
            /// This is the number of valid object that were returned.
            /// </summary>
            public int numValidObjects;

            /// <summary>
            /// This is the delta time for the frame.
            /// </summary>
            public float deltaTime;

            /// <summary>
            /// This is the amount of time spent searching.
            /// </summary>
            public float timeSpentSearching;

            /// <summary>
            /// This is the amount of time spent updating.
            /// </summary>
            public float timeSpentUpdating;

            /// <summary>
            /// This resets this search stats.
            /// </summary>
            public void Reset()
            {
                numSearches =
                    numCellsSearched =
                    numContainmentChecks =
                    numValidObjects = 0;
                deltaTime = 0.0f;
                timeSpentSearching = 0.0f;
                timeSpentUpdating = 0.0f;
            }
        }

        #endregion

        #region Accessor Functions

        /// <summary>
        /// This gets the grid x index for an x world position.
        /// </summary>
        /// <param name="fXPos">The x world position.</param>
        /// <returns>The grid x index.</returns>
        private int GetGridX(float fXPos)
        {
            //get the grid x index
            int nGridX = 0;
            if (fXPos <= m_oMinWorldPoint.x)
                nGridX = 0;
            else if (fXPos >= m_oMaxWorldPoint.x)
                nGridX = m_nGridWidth - 1;
            else
                nGridX = (int)((fXPos - m_oMinWorldPoint.x) / m_fCellSize);

            //return the grid x index
            return nGridX;
        }

        /// <summary>
        /// This gets the grid y index for an y world position.
        /// </summary>
        /// <param name="fYPos">The y world position.</param>
        /// <returns>The grid y index.</returns>
        private int GetGridY(float fYPos)
        {
            //get the grid y index
            int nGridY = 0;
            if (fYPos <= m_oMinWorldPoint.y)
                nGridY = 0;
            else if (fYPos >= m_oMaxWorldPoint.y)
                nGridY = m_nGridWidth - 1;
            else
                nGridY = (int)((fYPos - m_oMinWorldPoint.y) / m_fCellSize);

            //return the grid y index
            return nGridY;
        }

        /// <summary>
        /// This gets the grid z index for an z world position.
        /// </summary>
        /// <param name="fZPos">The z world position.</param>
        /// <returns>The grid z index.</returns>
        private int GetGridZ(float fZPos)
        {
            //get the grid z index
            int nGridZ = 0;
            if (fZPos <= m_oMinWorldPoint.z)
                nGridZ = 0;
            else if (fZPos >= m_oMaxWorldPoint.z)
                nGridZ = m_nGridWidth - 1;
            else
                nGridZ = (int)((fZPos - m_oMinWorldPoint.z) / m_fCellSize);

            //return the grid z index
            return nGridZ;
        }

        /// <summary>
        /// This function gets the cell that corresponds with the specified world point.
        /// </summary>
        /// <param name="oWorldPoint">The world point to get the cell for.</param>
        /// <returns>The cell for the world point.</returns>
        public SearchCell GetCell(Vector3 oWorldPoint)
        {
            return GetCell(oWorldPoint.x, oWorldPoint.y, oWorldPoint.z);
        }

        /// <summary>
        /// This function gets the cell that corresponds with the specified world point.
        /// </summary>
        /// <param name="fXPos">The world x position to get the cell for.</param>
        /// <param name="fYPos">The world y position to get the cell for.</param>
        /// <returns>The cell for the world point.</returns>
        public SearchCell GetCell(float fXPos, float fYPos, float fZPos)
        {
            //get the grid position of the cells
            int nGridX = GetGridX(fXPos);
            int nGridY = GetGridY(fYPos);
            int nGridZ = GetGridZ(fZPos);

            //make sure x and y are in bounds of the array
            if (nGridX < 0)
                nGridX = 0;
            if (nGridX > GridWidth - 1)
                nGridX = GridWidth - 1;
            if (nGridY < 0)
                nGridY = 0;
            if (nGridY > GridHeight - 1)
                nGridY = GridHeight - 1;
            if (nGridZ < 0)
                nGridZ = 0;
            if (nGridZ > GridDepth - 1)
                nGridZ = GridDepth - 1;

            //return the cell
            return m_aoSearchCells[nGridX, nGridY, nGridZ];
        }

        #endregion

        #region Add/Remove Object Functions

        /// <summary>
        /// This adds an object to this search grid.
        /// </summary>
        /// <param name="oObject">This is the object to add.</param>
        public void AddObjectInternal(SearchObject oObject)
        {
            //make sure the search cell is null
            if (oObject.SearchCell == null)
            {
                //get the cell
                SearchCell oCell = GetCell(oObject.transform.position);

                //make sure a cell we received
                if (oCell != null)
                {
                    //add this object to the cell.
                    oCell.Objects.Add(oObject);
                    m_oObjects.Add(oObject);
                    oObject.SearchCell = oCell;
                }
            }
            else
            {//this object already belongs to a cell, display warning.
                Debug.LogWarning("Attempting to add a search object that already belongs to a cell, ignoring!");
            }
        }

        /// <summary>
        /// This function adds an object to the current static instance of the search grid.
        /// </summary>
        /// <param name="oObject">The object to add.</param>
        public static void AddObject(SearchObject oObject)
        {
            //make sure there is in instance to add to
            if (Instance != null)
                Instance.AddObjectInternal(oObject);
            else
                Debug.LogWarning("Attempting to call a static SearchGrid function, AddObject, but there is no active SearchGrid.");
        }

        /// <summary>
        /// This removes an object from the search grid.
        /// </summary>
        /// <param name="oObject">The object to remove.</param>
        public void RemoveObjectInternal(SearchObject oObject)
        {
            //remove the object
            m_oObjects.Remove(oObject);
            if (oObject.SearchCell != null)
            {//remove the object from its search cell.
                oObject.SearchCell.Objects.Remove(oObject);
                oObject.SearchCell = null;
            }
        }

        /// <summary>
        /// This removes an object from the static instance of the search grid.
        /// </summary>
        /// <param name="oObject"></param>
        public static void RemoveObject(SearchObject oObject)
        {
            //make sure there is an instance, and remove the object.
            if (Instance != null)
                Instance.RemoveObjectInternal(oObject);
            else
                Debug.LogWarning("Attempting to call a static SearchGrid function, RemoveObject, but there is no active SearchGrid.");
        }

        #endregion

        #region Update Functions

        /// <summary>
        /// This updates all the cells in this search grid.
        /// </summary>
        public void Update()
        {
            //start updating time spent
            if (enableDebug)
                m_fTimeSpentUpdating = Time.realtimeSinceStartup;

            //reset max object size
            m_fMaxObjectRadius = 0.0f;

            //update all objects
            for (int i = 0; i < m_oObjects.Count; i++)
            {
                //update search cell
                m_oObjects[i].UpdateSearchCell();

                //update radius
                float objectRadius = 0.0f;
                if (m_oObjects[i].updateRadiusEveryFrame)
                    objectRadius = m_oObjects[i].UpdateRadius();
                else
                    objectRadius = m_oObjects[i].Radius;

                //check if this is the max object radius
                if (objectRadius > m_fMaxObjectRadius)
                    m_fMaxObjectRadius = objectRadius;
            }

            //finish updating time spent
            if (enableDebug)
                m_fTimeSpentUpdating = Time.realtimeSinceStartup - m_fTimeSpentUpdating;

            //darw debug
            DrawDebug();
        }

        #endregion

        #region Search Functions

        /// <summary>
        /// This is the internal search function for this search grid that will search for objects around a point
        /// in a radius, and return a list of all objects in that search area.
        /// </summary>
        /// <param name="position">The position to search at.</param>
        /// <param name="radius">The radius to search within.</param>
        /// <returns>The read only collection of objects that were within that search area.</returns>
        public ReadOnlyCollection<SearchObject> SearchInternal(Vector3 position, float radius)
        {
            return SearchInternal<SearchObject>(position, radius);
        }

        /// <summary>
        /// This is the templated internal search function for this search grid that will search for objects around a point
        /// in a radius, and return a list of all objects in that search area.  It will only return objects that have components
        /// of the specified type.
        /// </summary>
        /// <param name="position">The position to search at.</param>
        /// <param name="radius">The radius to search within.</param>
        /// <returns>The read only collection of objects that were within that search area.</returns>
        public ReadOnlyCollection<SearchObject> SearchInternal<T>(Vector3 position, float radius)
        {
            //clear global query list
            ms_oGlobalQueryList.Clear();

            //get expanded radius and the squared radius
            float expandedRadius = radius + m_fMaxObjectRadius;

            //get start and end grid positions
            int gridXStart = GetGridX(position.x - expandedRadius);
            int gridXEnd = GetGridX(position.x + expandedRadius);
            int gridYStart = GetGridY(position.y - expandedRadius);
            int gridYEnd = GetGridY(position.y + expandedRadius);
            int gridZStart = GetGridZ(position.z - expandedRadius);
            int gridZEnd = GetGridZ(position.z + expandedRadius);

            //start a new search debug entry if search debug should currently be drawn.
            if (enableDebug)
            {
                m_oDebugSearches.Add(new SearchDebug());
                m_oDebugSearches[m_oDebugSearches.Count - 1].position = position;
                m_oDebugSearches[m_oDebugSearches.Count - 1].radius = radius;
                m_oDebugSearches[m_oDebugSearches.Count - 1].timeSpent = Time.realtimeSinceStartup;
            }

            //loop through all cells and test against objects for containment in the search area
            for (int gridX = gridXStart; gridX <= gridXEnd; gridX++)
            {
                for (int gridY = gridYStart; gridY <= gridYEnd; gridY++)
                {
                    for (int gridZ = gridZStart; gridZ <= gridZEnd; gridZ++)
                    {
                        //make sure x and y are in bounds of the array
                        if (gridX < 0 ||
                            gridX > GridWidth - 1 ||
                            gridY < 0 ||
                            gridY > GridHeight - 1 ||
                            gridZ < 0 ||
                            gridZ > GridDepth - 1)
                        {
                            continue;
                        }

                        if (enableDebug)
                        {
                            m_oDebugSearches[m_oDebugSearches.Count - 1].numCells++;
                        }

                        //add all game objects to list
                        SearchCell cell = m_aoSearchCells[gridX, gridY, gridZ];
                        for (int i = 0; i < cell.Objects.Count; i++)
                        {
                            //check if the template type passed in is NOT SearchObject, and this game object has that component in it
                            bool typeOk = true;
                            if (typeof(T) != typeof(SearchObject) &&
                                cell.Objects[i].GetComponent(typeof(T)) == null)
                            {
                                //this does NOT have that type, REJECT this object
                                typeOk = false;
                            }

                            //make sure the type is okay
                            if (typeOk)
                            {
                                //get search radius sq
                                float searchRadiusSq = radius + cell.Objects[i].Radius;
                                searchRadiusSq *= searchRadiusSq;

                                //check if this is a valid object and is in the search range.
                                bool valid = false;
                                if (cell.Objects[i].transform != null &&
                                   (cell.Objects[i].transform.position - position).sqrMagnitude <= searchRadiusSq)
                                    valid = true;

                                //add to the list if the object was in the search range
                                if (valid)
                                    ms_oGlobalQueryList.Add(cell.Objects[i]);

                                //add debug if needed
                                if (enableDebug)
                                {
                                    m_oDebugSearches[m_oDebugSearches.Count - 1].objectPositions.Add(cell.Objects[i].transform.position);
                                    m_oDebugSearches[m_oDebugSearches.Count - 1].objectResults.Add(valid);
                                }
                            }
                        }
                    }
                }
            }

            //update time spent
            if (enableDebug)
            {
                m_oDebugSearches[m_oDebugSearches.Count - 1].timeSpent = Time.realtimeSinceStartup - m_oDebugSearches[m_oDebugSearches.Count - 1].timeSpent;
            }

            //return list
            return ms_oGlobalQueryList.AsReadOnly();
        }

        /// <summary>
        /// This is the static search function for this search grid that will search for objects around a point
        /// in a radius, and return a list of all objects in that search area.
        /// </summary>
        /// <param name="position">The position to search at.</param>
        /// <param name="radius">The radius to search within.</param>
        /// <returns>The read only collection of objects that were within that search area.</returns>
        public static ReadOnlyCollection<SearchObject> Search(Vector3 position, float radius)
        {
            return Search<SearchObject>(position, radius);
        }

        /// <summary>
        /// This is the templated static search function for this search grid that will search for objects around a point
        /// in a radius, and return a list of all objects in that search area.  It will only return objects that have components
        /// of the specified type.
        /// </summary>
        /// <param name="position">The position to search at.</param>
        /// <param name="radius">The radius to search within.</param>
        /// <returns>The read only collection of objects that were within that search area.</returns>
        public static ReadOnlyCollection<SearchObject> Search<T>(Vector3 position, float radius)
        {
            if (Instance != null)
                return Instance.SearchInternal<T>(position, radius);
            else
            {
                Debug.LogWarning("Attempting to call a static SearchGrid function, Search, but there is no active SearchGrid.");
                return null;
            }
        }

        #endregion

        #region Debug Functions

        /// <summary>
        /// This function draws the debug for this search grid.
        /// </summary>
        public void DrawDebug()
        {
            //check if the grid debug should be drawn.
            if (enableDebug && drawGridDebug)
            {
                //draw width lines
                for (int y = 0; y <= m_nGridHeight; y++)
                {
                    for (int z = 0; z <= m_nGridDepth; z++)
                    {
                        Debug.DrawLine(m_oMinWorldPoint +
                                            new Vector3(0.0f,
                                                        m_fCellSize * (float)y,
                                                        m_fCellSize * (float)z),
                                       m_oMinWorldPoint +
                                            new Vector3(m_oWorldSize.x,
                                                        m_fCellSize * (float)y,
                                                        m_fCellSize * (float)z),
                                       new Color(0.5f, 0.5f, 0.5f, 0.5f));

                    }
                }

                //draw height lines
                for (int x = 0; x <= m_nGridWidth; x++)
                {
                    for (int z = 0; z <= m_nGridDepth; z++)
                    {
                        Debug.DrawLine(m_oMinWorldPoint +
                                            new Vector3(m_fCellSize * (float)x,
                                                        0.0f,
                                                        m_fCellSize * (float)z),
                                       m_oMinWorldPoint +
                                            new Vector3(m_fCellSize * (float)x,
                                                        m_oWorldSize.y,
                                                        m_fCellSize * (float)z),
                                       new Color(0.5f, 0.5f, 0.5f, 0.5f));

                    }
                }

                //draw depth lines
                for (int x = 0; x <= m_nGridWidth; x++)
                {
                    for (int y = 0; y <= m_nGridHeight; y++)
                    {
                        Debug.DrawLine(m_oMinWorldPoint +
                                            new Vector3(m_fCellSize * (float)x,
                                                        m_fCellSize * (float)y,
                                                        0.0f),
                                       m_oMinWorldPoint +
                                            new Vector3(m_fCellSize * (float)x,
                                                        m_fCellSize * (float)y,
                                                        m_oWorldSize.z),
                                       new Color(0.5f, 0.5f, 0.5f, 0.5f));

                    }
                }
            }

            //check if the cell debug should be drawn
            if (enableDebug && drawCellDebug)
            {
                for (int x = 0; x < m_nGridWidth; x++)
                {
                    for (int y = 0; y < m_nGridHeight; y++)
                    {
                        for (int z = 0; z < m_nGridDepth; z++)
                        {
                            if (m_aoSearchCells[x, y, z].Objects.Count > 0)
                            {
                                m_aoSearchCells[x, y, z].DrawDebug(true, true);
                            }
                        }
                    }
                }
            }

            //check if the search debug should be drawn.
            if (enableDebug)
            {
                //aggregate the search stats for this frame.
                m_nSearchStatsIndex++;
                if (m_nSearchStatsIndex >= m_aoSearchStats.Length)
                    m_nSearchStatsIndex = 0;
                m_aoSearchStats[m_nSearchStatsIndex].Reset();
                m_aoSearchStats[m_nSearchStatsIndex].deltaTime = Time.deltaTime;
                m_aoSearchStats[m_nSearchStatsIndex].timeSpentUpdating = m_fTimeSpentUpdating;
                for (int i = 0; i < m_oDebugSearches.Count; i++)
                {
                    m_aoSearchStats[m_nSearchStatsIndex].numSearches++;
                    m_aoSearchStats[m_nSearchStatsIndex].numCellsSearched += m_oDebugSearches[i].numCells;
                    m_aoSearchStats[m_nSearchStatsIndex].numContainmentChecks += m_oDebugSearches[i].objectPositions.Count;
                    m_aoSearchStats[m_nSearchStatsIndex].timeSpentSearching += m_oDebugSearches[i].timeSpent;
                    for (int j = 0; j < m_oDebugSearches[i].objectResults.Count; j++)
                    {
                        if (m_oDebugSearches[i].objectResults[j])
                            m_aoSearchStats[m_nSearchStatsIndex].numValidObjects++;
                    }
                }

                //clear the list of searches that happen this frame
                m_oDebugSearches.Clear();
            }
        }

        #endregion

        #region On Event Functions

        /// <summary>
        /// This is called when the gui for this script needs to be drawn.
        /// </summary>
        public void OnGUI()
        {
            if (enableDebug && drawStatsDebug)
            {
                //aggregate all current search stats
                SearchStats aggregate = new SearchStats();
                aggregate.Reset();
                for (int i = 0; i < m_aoSearchStats.Length; i++)
                {
                    aggregate.deltaTime += m_aoSearchStats[i].deltaTime;
                    aggregate.timeSpentSearching += m_aoSearchStats[i].timeSpentSearching;
                    aggregate.timeSpentUpdating += m_aoSearchStats[i].timeSpentUpdating;
                    aggregate.numCellsSearched += m_aoSearchStats[i].numCellsSearched;
                    aggregate.numContainmentChecks += m_aoSearchStats[i].numContainmentChecks;
                    aggregate.numSearches += m_aoSearchStats[i].numSearches;
                    aggregate.numValidObjects += m_aoSearchStats[i].numValidObjects;
                }

                //calculate serach stats
                float searchesPerSecond = (float)aggregate.numSearches / aggregate.deltaTime;
                float timeSpentSearchingPerSecond = (float)aggregate.timeSpentSearching / aggregate.deltaTime;
                float timeSpentUpdatingPerSecond = (float)aggregate.timeSpentUpdating / aggregate.deltaTime;
                float cellsSearchPerSecond = (float)aggregate.numCellsSearched / aggregate.deltaTime;
                float containmentChecksPerSecond = (float)aggregate.numContainmentChecks / aggregate.deltaTime;
                float validObjectsPerSecond = (float)aggregate.numValidObjects / aggregate.deltaTime;
                float rejectedObjectsPerSecond = (float)(aggregate.numContainmentChecks - aggregate.numValidObjects) / aggregate.deltaTime;

                //start gui frame
                int startX = 10;
                int startY = 20;
                int labelHeight = 20;
                int padding = 7;
                int frameWidth = 200;
                int frameHeight = 292;
                Rect frameRect = new Rect(startX - padding, startY - padding, frameWidth, frameHeight);
                GUI.BeginGroup(frameRect);
                GUI.color = new Color(0, 0, 0, 0.5f);
                GUI.DrawTexture(new Rect(0, 0, frameRect.width, frameRect.height), debugTexture);

                //draw labels
                GUI.color = Color.white;
                GUI.Label(new Rect(padding + 38, padding + labelHeight * 0, 200, labelHeight + 3), "Search Grid Stats");
                GUI.Label(new Rect(padding + 61, padding + labelHeight * 1, 200, labelHeight + 3), "(Count)");
                GUI.Label(new Rect(padding, padding + labelHeight * 2, 200, labelHeight + 3), "Cells: " + NumCells.ToString());
                GUI.Label(new Rect(padding, padding + labelHeight * 3, 200, labelHeight + 3), "Objects: " + Objects.Count.ToString());
                GUI.Label(new Rect(padding + 48, padding + labelHeight * 4, 200, labelHeight + 3), "(Per Second)");
                GUI.Label(new Rect(padding, padding + labelHeight * 5, 200, labelHeight + 3), "Searches: " + searchesPerSecond.ToString("0.00"));
                GUI.Label(new Rect(padding, padding + labelHeight * 6, 200, labelHeight + 3), "Cells Searched: " + cellsSearchPerSecond.ToString("0.00"));
                GUI.Label(new Rect(padding, padding + labelHeight * 7, 200, labelHeight + 3), "Containment Checks: " + containmentChecksPerSecond.ToString("0.00"));
                GUI.Label(new Rect(padding, padding + labelHeight * 8, 200, labelHeight + 3), "Valid Objects: " + validObjectsPerSecond.ToString("0.00"));
                GUI.Label(new Rect(padding, padding + labelHeight * 9, 200, labelHeight + 3), "Rectected Objects: " + rejectedObjectsPerSecond.ToString("0.00"));
                GUI.Label(new Rect(padding + 15, padding + labelHeight * 10, 200, labelHeight + 3), "(Milliseconds Per Second)");
                GUI.Label(new Rect(padding, padding + labelHeight * 11, 200, labelHeight + 3), "Update Time: " + (timeSpentUpdatingPerSecond * 1000.0f).ToString("0.000000"));
                GUI.Label(new Rect(padding, padding + labelHeight * 12, 200, labelHeight + 3), "Search Time: " + (timeSpentSearchingPerSecond * 1000.0f).ToString("0.000000"));
                GUI.Label(new Rect(padding, padding + labelHeight * 13, 200, labelHeight + 3), "Total Time: " + ((timeSpentUpdatingPerSecond + timeSpentSearchingPerSecond) * 1000.0f).ToString("0.000000"));

                //draw the frame outline and end the gui group
                GUI.color = Color.white;
                GUI.DrawTexture(new Rect(0, 0, frameWidth, 1), debugTexture);
                GUI.DrawTexture(new Rect(0, frameHeight - 1, frameWidth, 1), debugTexture);
                GUI.DrawTexture(new Rect(0, 0, 1, frameHeight), debugTexture);
                GUI.DrawTexture(new Rect(frameWidth - 1, 0, 1, frameHeight), debugTexture);
                GUI.EndGroup();
            }
        }

        /// <summary>
        /// This is called when the gizmos for this script need to be drawn.
        /// </summary>
        public void OnDrawGizmos()
        {
            //check if search debug should be drawn
            if (enableDebug && drawSearchDebug)
            {
                //draw the debug of all current debug searches.
                for (int i = 0; i < m_oDebugSearches.Count; i++)
                {
                    m_oDebugSearches[i].DrawDebug();
                }
            }

            //check if object debug should be drawn
            if (enableDebug && drawObjectDebug)
            {
                for (int i = 0; i < m_oObjects.Count; i++)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireSphere(m_oObjects[i].transform.position, m_oObjects[i].Radius);
                }
            }
        }

        #endregion

        #region Validation Functions

        /// <summary>
        /// This functon attempts to validiate a value. 
        /// </summary>
        /// <param name="currentValue">
        /// The current value to validate.
        /// </param>
        /// <param name="minValue">
        /// The min valid value.
        /// </param>
        /// <param name="maxValue">
        /// The max valid value.
        /// </param>
        /// <param name="defaultValue">
        /// The default value to use if the current value is invalid.
        /// </param>
        /// <param name="obj">
        /// The object whose data is being validated.
        /// </param>
        /// <param name="fieldName">
        /// The name of the field being validated.
        /// </param>
        /// <param name="error">
        /// Whether or not this is an error or warning if invalid.
        /// </param>
        /// <returns>
        /// The new, validated value.
        /// </returns>
        public static int Validate(int currentValue, int minValue, int maxValue, int defaultValue, Object obj, string fieldName, bool error)
        {
            //check if the current value is out of the valid range
            if (currentValue < minValue || currentValue > maxValue)
            {
                //construct the log message
                string message = obj.ToString() +
                                 " has its data \"" +
                                 fieldName +
                                 "\" out of its valid data range of " +
                                 minValue +
                                 " to " +
                                 maxValue +
                                 ".  Defaulting to " +
                                 defaultValue +
                                 ".";

                //display the log message
                if (error)
                    Debug.LogError(message, obj);
                else
                    Debug.LogWarning(message, obj);

                //current value is invalid, return default value
                return defaultValue;
            }

            //current value is valid, return current value
            return currentValue;
        }

        /// <summary>
        /// This functon attempts to validiate a value. 
        /// </summary>
        /// <param name="currentValue">
        /// The current value to validate.
        /// </param>
        /// <param name="minValue">
        /// The min valid value.
        /// </param>
        /// <param name="maxValue">
        /// The max valid value.
        /// </param>
        /// <param name="defaultValue">
        /// The default value to use if the current value is invalid.
        /// </param>
        /// <param name="obj">
        /// The object whose data is being validated.
        /// </param>
        /// <param name="fieldName">
        /// The name of the field being validated.
        /// </param>
        /// <param name="error">
        /// Whether or not this is an error or warning if invalid.
        /// </param>
        /// <returns>
        /// The new, validated value.
        /// </returns>
        public static float Validate(float currentValue, float minValue, float maxValue, float defaultValue, Object obj, string fieldName, bool error)
        {
            //check if the current value is out of the valid range
            if (currentValue < minValue || currentValue > maxValue)
            {
                //construct the log message
                string message = obj.ToString() +
                                 " has its data \"" +
                                 fieldName +
                                 "\" out of its valid data range of " +
                                 minValue +
                                 " to " +
                                 maxValue +
                                 ".  Defaulting to " +
                                 defaultValue +
                                 ".";

                //display the log message
                if (error)
                    Debug.LogError(message, obj);
                else
                    Debug.LogWarning(message, obj);

                //current value is invalid, return default value
                return defaultValue;
            }

            //current value is valid, return current value
            return currentValue;
        }

        #endregion
    }
}