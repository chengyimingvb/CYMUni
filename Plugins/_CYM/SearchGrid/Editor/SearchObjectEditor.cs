using UnityEngine;
using System.Collections;
using UnityEditor;

/// <summary>
/// This is the editor for the search object
/// </summary>
namespace CYM.SearchGrid
{
    [CustomEditor(typeof(SearchObject))]
    public class SearchObjectEditor : Editor
    {
        /// <summary>
        /// This is called when the inspector gui should be run.
        /// </summary>
        public override void OnInspectorGUI()
        {
            GUI.changed = false;

            //get the object
            SearchObject obj = target as SearchObject;
            if (obj)
            {
                //show button to calculate the radius
                bool calculateRadius = GUILayout.Button("Calculate Base Radius");

                //display toggled for updating every frame and automatically calculating radius
                EditorGUIUtility.LookLikeInspector();
                obj.updateRadiusEveryFrame = EditorGUILayout.Toggle("\tUpdate Radius Every Frame", obj.updateRadiusEveryFrame);
                bool wasAutoCalc = obj.automaticallyCalculateRadius;
                obj.automaticallyCalculateRadius = EditorGUILayout.Toggle("\tAutomatically Calculate Radius", obj.automaticallyCalculateRadius);
                if (wasAutoCalc != obj.automaticallyCalculateRadius)
                    calculateRadius = true;

                //calculate radius if needed
                if (calculateRadius)
                    obj.CalculateInitialRadius();

                //show button for applying scale to radius
                bool updateRadius = false;
                if (!obj.automaticallyCalculateRadius)
                {
                    bool wasApplyScale = obj.applyScaleToRadius;
                    obj.applyScaleToRadius = EditorGUILayout.Toggle("\tApply Scale to Radius", obj.applyScaleToRadius);
                    if (wasApplyScale != obj.applyScaleToRadius)
                        updateRadius = true;
                }

                //display base radius
                if (obj.automaticallyCalculateRadius)
                {
                    EditorGUILayout.LabelField("\t\tBase Radius (Locked)", obj.baseObjectRadius.ToString());
                }
                else
                {
                    float oldBaseRadius = obj.baseObjectRadius;
                    obj.baseObjectRadius = Mathf.Abs(EditorGUILayout.FloatField("\tBase Radius", obj.baseObjectRadius));
                    if (oldBaseRadius != obj.baseObjectRadius)
                        updateRadius = true;
                }

                //update radius if needed
                if (updateRadius)
                    obj.UpdateRadius();

                //display final radius
                EditorGUILayout.LabelField("\tFinal Radius (Locked)", obj.Radius.ToString("0.000"));
            }

            if (GUI.changed)
                EditorUtility.SetDirty(target);
        }
    }
}