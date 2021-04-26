using UnityEngine;
using System.Collections;

/// <summary>
/// This script is used to attach to any game object that should be
/// added to the search grid as a searchable object.  If a game object
/// has a script of this type associated with it, it will automatically
/// be added to the grid.
/// </summary>
namespace CYM.SearchGrid
{
    public class SearchObject : MonoBehaviour
    {
        #region Public Member Variables

        /// <summary>
        /// This indicates if the radius of this search object should be updated every frame, or only
        /// when it is first setup and/or added to the search grid.
        /// </summary>
        public bool updateRadiusEveryFrame = false;

        /// <summary>
        /// This indicates if the approx radius for this search object should automatically be calculated.
        /// </summary>
        public bool automaticallyCalculateRadius = true;

        /// <summary>
        /// This is the manual radius of the object to use when searching for it in the search grid.
        /// </summary>
        public float baseObjectRadius = 0.1f;

        /// <summary>
        /// This indicates if the maximum scale of this search object should apply to the base object radius.
        /// 
        /// So if the base object radius is 0.1, and the scale is (2.0, 1.5, 0.5).  Then
        /// the final search object radius would be (0.1 * 2.0) = 0.2.
        /// </summary>
        public bool applyScaleToRadius = false;

        #endregion

        #region Private Member Variables

        /// <summary>
        /// This is the search cell this search object currently belongs to.
        /// </summary>
        private SearchCell m_oSearchCell = null;

        /// <summary>
        /// This was the position of this search object the last time it was updated.
        /// </summary>
        private Vector3 m_oLastPosition = Vector3.zero;

        /// <summary>
        /// This is the approximate radius of this object.
        /// </summary>
        private float m_fRadius = 0.0f;

        /// <summary>
        /// This indicates if this search object needs to try and add itself to a search grid.
        /// </summary>
        private bool m_bAddNeeded = false;

        #endregion

        #region Properties

        /// <summary>
        /// This gets/sets the search cell this search object belongs to.
        /// THIS SHOULD NEVER BE SET DIRECTLY!  ONLY SEARCHCELL WILL SET THIS!
        /// </summary>
        public SearchCell SearchCell
        {
            get { return m_oSearchCell; }
            set { m_oSearchCell = value; }
        }

        /// <summary>
        /// This gets the radius of this object that should be used to search against.
        /// </summary>
        public float Radius
        {
            get { return m_fRadius; }
        }

        #endregion

        #region Init/Deinit Functions

        /// <summary>
        /// This is called when this script is reset
        /// </summary>
        public void Reset()
        {
            CalculateInitialRadius();
        }

        /// <summary>
        /// This calculates the initial base radius
        /// </summary>
        public void CalculateInitialRadius()
        {
            //calcualte radius automatically for default value
            bool oldValue = automaticallyCalculateRadius;
            automaticallyCalculateRadius = true;
            baseObjectRadius = UpdateRadius();
            automaticallyCalculateRadius = oldValue;

            //offset for scale
            float maxScale = transform.localScale.z;
            if (transform.localScale.x >= transform.localScale.y &&
                transform.localScale.x >= transform.localScale.z)
                maxScale = transform.localScale.x;
            else if (transform.localScale.y >= transform.localScale.x &&
                     transform.localScale.y >= transform.localScale.z)
                maxScale = transform.localScale.y;
            baseObjectRadius /= maxScale;

            //update radius
            UpdateRadius();
        }
        /// <summary>
        /// This gets call when this search object is destroyed.
        /// </summary>
        public void OnDestroy()
        {
            //check if this object belongs to a cell
            if (SearchCell != null)
            {
                //remove this object from the cells grid
                SearchCell.Grid.RemoveObjectInternal(this);
            }
        }

        #endregion

        #region Enable/Disable Functions

        /// <summary>
        /// This is called when the script/object is enabled 
        /// </summary>
        public void OnEnable()
        {
            m_bAddNeeded = true;
            UpdateRadius();
        }

        /// <summary>
        /// This is the called when the script/object is disabled.
        /// </summary>
        public void OnDisable()
        {
            //remove this object from the grid.
            if (SearchCell != null &&
                SearchGrid.Instance != null)
            {
                SearchGrid.RemoveObject(this);
            }
        }

        #endregion

        #region Update Functions

        public void Update()
        {
            //try to add if needed
            if (m_bAddNeeded)
            {
                //mark that an add is no longer needed
                m_bAddNeeded = false;

                //check if there is an instance of the grid
                if (SearchCell == null &&
                    SearchGrid.Instance != null)
                {
                    //add this object to the instance
                    SearchGrid.AddObject(this);
                }
                else
                {
                    //no grid was found, warn
                    Debug.LogWarning("No SearchGrid was found, unable to add this SearchObject to a grid!");
                }
            }
        }

        /// <summary>
        /// This function is called by the SearchGrid to update which cell this search object belongs to.
        /// </summary>
        public void UpdateSearchCell()
        {
            //get current position
            Vector3 currentPosition = transform.position;

            //check if its position has changed
            if (currentPosition.x != m_oLastPosition.x ||
                currentPosition.y != m_oLastPosition.y ||
                currentPosition.z != m_oLastPosition.z)
            {
                //update which cell this search object is in
                if (SearchCell != null &&
                    SearchCell.Grid != null)
                {
                    //make sure it is still in this cell
                    if (SearchCell.IsPointInCell(transform.position) == false)
                    {//it is outside the cell
                     //get the cell it is to belong in
                        SearchCell cell = SearchCell.Grid.GetCell(transform.position);

                        //make sure this cell was not returned
                        if (cell != SearchCell)
                        {
                            //remove from this cell and add to new one
                            SearchCell.Objects.Remove(this);
                            cell.Objects.Add(this);
                            SearchCell = cell;
                        }
                    }
                }

                //update last position
                m_oLastPosition = transform.position;
            }
        }

        /// <summary>
        /// This updates the approximate size of this object and returns it.
        /// </summary>
        /// <returns>The approximate size of this object.</returns>
        public float UpdateRadius()
        {
            //automatically calculate radius if needed
            if (automaticallyCalculateRadius)
            {
                //get the current extents
                Vector3 currentExtents = Vector3.zero;
                if (GetComponent<Collider>() != null)
                    currentExtents = GetComponent<Collider>().bounds.extents;
                else if (GetComponent<Renderer>() != null)
                    currentExtents = GetComponent<Renderer>().bounds.extents;

                //get max extents component
                m_fRadius = currentExtents.z;
                if (currentExtents.x >= currentExtents.y &&
                    currentExtents.x >= currentExtents.z)
                    m_fRadius = currentExtents.x;
                else if (currentExtents.y >= currentExtents.x &&
                         currentExtents.y >= currentExtents.z)
                    m_fRadius = currentExtents.y;
            }
            //apply scale if needed
            else if (applyScaleToRadius)
            {
                //get the max scale component
                float maxScale = transform.localScale.z;
                if (transform.localScale.x >= transform.localScale.y &&
                    transform.localScale.x >= transform.localScale.z)
                    maxScale = transform.localScale.x;
                else if (transform.localScale.y >= transform.localScale.x &&
                         transform.localScale.y >= transform.localScale.z)
                    maxScale = transform.localScale.y;

                //apply scale
                m_fRadius = baseObjectRadius * maxScale;
            }
            //set to base radius
            else
            {
                m_fRadius = baseObjectRadius;
            }

            //return radius
            return m_fRadius;
        }

        #endregion
    }
}