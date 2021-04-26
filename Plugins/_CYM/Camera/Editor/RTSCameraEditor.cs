
using UnityEditor;
using UnityEngine;
using System.Collections;
using CYM;
using Sirenix.OdinInspector.Editor;
using UnityEditor.SceneManagement;
namespace CYM.Cam
{
    [CustomEditor(typeof(RTSCamera))]
    public class RTSCameraEditor : OdinEditor
    {

        private RTSCamera mCam;
        private bool baseSetting;
        private bool boundSetting;
        private bool followSetting;
        private bool controlSetting;

        void Awake()
        {
            mCam = target as RTSCamera;
        }

        public override void OnInspectorGUI()
        {
            baseSetting = EditorGUILayout.Foldout(baseSetting, "Basic");
            if (baseSetting)
            {
                EditorGUILayout.LabelField("Smoothing Settings");
                mCam.movementLerpSpeed = EditorGUILayout.FloatField("  -Movement Lerp Speed", mCam.movementLerpSpeed);
                mCam.rotationLerpSpeed = EditorGUILayout.FloatField("  -Rotation Lerp Speed", mCam.rotationLerpSpeed);
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Scroll Settings");

                CamScrollAnimType tempType = mCam.scrollAnimationType;
                tempType = (CamScrollAnimType)EditorGUILayout.EnumPopup("   -Animation Type", tempType);

                if (tempType != mCam.scrollAnimationType
                    && EditorUtility.DisplayDialog("Replacing Changes", "If you switch to another animation type, your settings in current mode will be replaced or modified.", "Continue", "Cancel"))
                {
                    mCam.scrollAnimationType = tempType;
                }


                switch (mCam.scrollAnimationType)
                {
                    case CamScrollAnimType.Simple:
                        mCam.MinHight = EditorGUILayout.FloatField("    -Min High", mCam.MinHight);
                        mCam.MaxHight = EditorGUILayout.FloatField("    -Max High", mCam.MaxHight);
                        mCam.MinAngle = EditorGUILayout.FloatField("    -Min Angle", mCam.MinAngle);
                        mCam.MaxAngle = EditorGUILayout.FloatField("    -Max Angle", mCam.MaxAngle);
                        break;

                    case CamScrollAnimType.Advanced:
                        mCam.scrollXAngle = EditorGUILayout.CurveField(new GUIContent("    Scroll X Angle", "Scroll X Angle Animation"), mCam.scrollXAngle);
                        mCam.scrollHigh = EditorGUILayout.CurveField(new GUIContent("    Scroll High", "Scroll High Animation"), mCam.scrollHigh);
                        break;
                }
                mCam.ScrollValue = EditorGUILayout.Slider("    -Start Scroll Value", mCam.ScrollValue, 0f, 1f);
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Casting Settings");
                mCam.groundHighTest = EditorGUILayout.Toggle("  -Ground Check", mCam.groundHighTest);
                if (mCam.groundHighTest)
                {
                    mCam.groundMask = PreviewUtil.LayerMaskField("  -Ground Mask", mCam.groundMask);
                }
                mCam.groundHighTestValMax = EditorGUILayout.FloatField("  -Ground Check Value", mCam.groundHighTestValMax);
                EditorGUILayout.Space();
            }

            boundSetting = EditorGUILayout.Foldout(boundSetting, "Bound");
            if (boundSetting)
            {

                mCam.bound.xMin = EditorGUILayout.FloatField("  -Min X", mCam.bound.xMin);
                mCam.bound.xMax = EditorGUILayout.FloatField("  -Max X", mCam.bound.xMax);
                mCam.bound.yMin = EditorGUILayout.FloatField("  -Min Z", mCam.bound.yMin);
                mCam.bound.yMax = EditorGUILayout.FloatField("  -Max Z", mCam.bound.yMax);

                if (GUILayout.Button("Use Suggested Values") && EditorUtility.DisplayDialog("替换你当前的设置", "自动扫描，来设置范围大小.", "Confirm", "Cancel"))
                {
                    Bounds[] discoveredBounds;

                    MeshRenderer[] renderers = Resources.FindObjectsOfTypeAll<MeshRenderer>();
                    discoveredBounds = new Bounds[renderers.Length];

                    EditorUtility.DisplayProgressBar("Calculating...", "Finding objects...", 0);
                    for (int i = 0; i < discoveredBounds.Length; i++)
                        discoveredBounds[i] = renderers[i].bounds;

                    EditorUtility.DisplayProgressBar("Calculating...", "Calculating bounds along X...", 0.25f);
                    float endValues = Mathf.Infinity;
                    for (int i = 0; i < discoveredBounds.Length; i++)
                    {
                        if (endValues > discoveredBounds[i].min.x)
                            endValues = discoveredBounds[i].min.x;
                    }
                    mCam.bound.xMin = endValues;

                    EditorUtility.DisplayProgressBar("Calculating...", "Calculating bounds along X...", 0.5f);
                    endValues = Mathf.NegativeInfinity;
                    for (int i = 0; i < discoveredBounds.Length; i++)
                    {
                        if (endValues < discoveredBounds[i].max.x)
                            endValues = discoveredBounds[i].max.x;
                    }
                    mCam.bound.xMax = endValues;

                    EditorUtility.DisplayProgressBar("Calculating...", "Calculating bounds along Z...", 0.75f);
                    endValues = Mathf.Infinity;
                    for (int i = 0; i < discoveredBounds.Length; i++)
                    {
                        if (endValues > discoveredBounds[i].min.z)
                            endValues = discoveredBounds[i].min.z;
                    }
                    mCam.bound.yMin = endValues;

                    EditorUtility.DisplayProgressBar("Calculating...", "Calculating bounds along Z...", 0.99f);
                    endValues = Mathf.NegativeInfinity;
                    for (int i = 0; i < discoveredBounds.Length; i++)
                    {
                        if (endValues < discoveredBounds[i].max.z)
                            endValues = discoveredBounds[i].max.z;
                    }
                    mCam.bound.yMax = endValues;

                    EditorUtility.ClearProgressBar();
                }

                EditorGUILayout.HelpBox("The white rectangle in scene view will help you configure scene bounds.", MessageType.Info);

                EditorGUILayout.Space();
            }

            followSetting = EditorGUILayout.Foldout(followSetting, "Follow");
            if (followSetting)
            {
                mCam.allowFollow = EditorGUILayout.Toggle("  -Allow Follow", mCam.allowFollow);
                if (mCam.allowFollow)
                {
                    mCam.unlockWhenMove = EditorGUILayout.Toggle("  -Unlock When Move", mCam.unlockWhenMove);
                }
                else
                {
                    EditorGUILayout.HelpBox("Enable Follow to let your camera focus something on center of screen or go to a fixed point.", MessageType.Info);
                }

                EditorGUILayout.Space();
            }

            controlSetting = EditorGUILayout.Foldout(controlSetting, "Control");
            if (controlSetting)
            {
                //Screen Edge
                mCam.mouseScreenEdgeMoveControl = EditorGUILayout.Toggle("  Mouse Screen Edge Movement", mCam.mouseScreenEdgeMoveControl);
                if (mCam.mouseScreenEdgeMoveControl)
                {
                    mCam.desktopMoveSpeed = EditorGUILayout.FloatField("    -Move Speed", mCam.desktopMoveSpeed);
                    mCam.deskScreenEdgeWidth = EditorGUILayout.FloatField("    -Edge Width", mCam.deskScreenEdgeWidth);
                    mCam.desktopDisSEMWhenDrag = EditorGUILayout.Toggle("    -Dis when drag", mCam.desktopDisSEMWhenDrag);
                }
                EditorGUILayout.Space();

                //Drag
                mCam.mouseDragControl = EditorGUILayout.Toggle("  Mouse Drag Control", mCam.mouseDragControl);
                if (mCam.mouseDragControl)
                {
                    mCam.mouseDragButton = System.Convert.ToInt32(EditorGUILayout.EnumPopup("    -Move Button", (MouseButton)mCam.mouseDragButton));
                    mCam.desktopMoveDragSpeed = EditorGUILayout.FloatField("    -Move Speed", mCam.desktopMoveDragSpeed);

                    if (mCam.mouseDragButton == mCam.mouseRotateButton)
                    {
                        EditorGUILayout.HelpBox("Control button overlapping.", MessageType.Warning);
                    }
                }
                EditorGUILayout.Space();

                //Rotation
                mCam.mouseRotateControl = EditorGUILayout.Toggle("  Mouse Rotate Control", mCam.mouseRotateControl);
                if (mCam.mouseRotateControl)
                {
                    mCam.mouseRotateButton = System.Convert.ToInt32(EditorGUILayout.EnumPopup("    -Rotate Button", (MouseButton)mCam.mouseRotateButton));
                    mCam.desktopRotateSpeed = EditorGUILayout.FloatField("    -Rotate Speed", mCam.desktopRotateSpeed);

                    if (mCam.mouseDragButton == mCam.mouseRotateButton)
                    {
                        EditorGUILayout.HelpBox("Control button overlapping.", MessageType.Warning);
                    }
                }
                EditorGUILayout.Space();

                //Scroll
                mCam.mouseScrollControl = EditorGUILayout.Toggle("  Mouse Scroll Control", mCam.mouseScrollControl);
                if (mCam.mouseScrollControl)
                {
                    mCam.desktopScrollSpeed = EditorGUILayout.FloatField("    -Scroll Speed", mCam.desktopScrollSpeed);
                }
                EditorGUILayout.Space();

                //Touch
                mCam.touchScreenEdgeMoveControl = EditorGUILayout.Toggle("  Touch Screen Edge Movement", mCam.touchScreenEdgeMoveControl);
                if (mCam.touchScreenEdgeMoveControl)
                {
                    mCam.touchMoveSpeed = EditorGUILayout.FloatField("    -Move Speed", mCam.touchMoveSpeed);
                    mCam.touchScreenEdgeWidth = EditorGUILayout.FloatField("    -Edge Width", mCam.touchScreenEdgeWidth);
                }
                EditorGUILayout.Space();

                mCam.touchDragControl = EditorGUILayout.Toggle("  Touch Drag Control", mCam.touchDragControl);
                if (mCam.touchDragControl)
                {
                    mCam.touchMoveDragSpeed = EditorGUILayout.FloatField("    -Move Speed", mCam.touchMoveDragSpeed);
                }
                EditorGUILayout.Space();

                mCam.touchRotateControl = EditorGUILayout.Toggle("  Touch Rotate Control", mCam.touchRotateControl);
                if (mCam.touchRotateControl)
                {
                    mCam.touchRotateSpeed = EditorGUILayout.FloatField("    -Rotate Speed", mCam.touchRotateSpeed);
                }
                EditorGUILayout.Space();

                mCam.touchScrollControl = EditorGUILayout.Toggle("  Touch Scroll Control", mCam.touchScrollControl);
                if (mCam.touchScrollControl)
                {
                    mCam.touchScrollSpeed = EditorGUILayout.FloatField("    -Scroll Speed", mCam.touchScrollSpeed); 
                }
                EditorGUILayout.Space();
            }

            if (GUILayout.Button("Save"))
            {
                EditorUtility.SetDirty(mCam);
                AssetDatabase.SaveAssets();
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                EditorSceneManager.SaveOpenScenes();
            }
        }
    }

}