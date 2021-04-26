//------------------------------------------------------------------------------
// UIObjectCreator.cs
// Copyright 2021 2021/2/24 
// Created by CYM on 2021/2/24
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

using CYM;
namespace CYM.UI
{

    public class UIObjectCreator
    {
        #region prop
        const string Background = "UI/Skin/Background.psd";
        const string UISprite = "UI/Skin/UISprite.psd";
        const string UIMask = "UI/Skin/UIMask.psd";
        static Color PanelColor = new Color(1f, 1f, 1f, 0.392f);
        static Color DefaultSelectableColor = new Color(1f, 1f, 1f, 1f);
        static Vector2 ThinElementSize = new Vector2(160f, 20f);
        static Font DefaultFont=>UIConfig.Ins.DefaultFont;
        #endregion

        #region utile
        static T CreateUIRoot<T>(Vector2 size, MenuCommand menuCommand=null) where T:UControl
        {
            var ret = CreateUIRoot(typeof(T).Name, size, menuCommand);
            ret.AddComponent<T>();
            return ret.GetComponent<T>();
        }
        static GameObject CreateUIRoot(string name, Vector2 size, MenuCommand menuCommand=null)
        {
            GameObject child = new GameObject(name);
            child.layer =LayerMask.NameToLayer("UI");
            RectTransform rectTransform = child.AddComponent<RectTransform>();
            rectTransform.sizeDelta = size;
            GameObject parent = menuCommand!=null? menuCommand.context as GameObject:null;
            var canvas = GameObject.FindObjectOfType<Canvas>();
            var activeTrans = Selection.activeTransform;
            if (activeTrans != null)
            {
                child.transform.SetParent(activeTrans.transform, false);
            }
            else if (parent != null)
            {
                child.transform.SetParent(parent.transform, false);
            }
            else if (canvas != null)
            {
                child.transform.SetParent(canvas.transform);
            }
            Selection.activeGameObject = child;
            return child;
        }
        static RectTransform CreateUIObject(string name, GameObject parent)
        {
            GameObject go = new GameObject(name);
            go.AddComponent<RectTransform>();
            SetParentAndAlign(go, parent);
            return go.GetComponent<RectTransform>() ;
        }
        private static void SetParentAndAlign(GameObject child, GameObject parent)
        {
            if (parent == null)
                return;

            child.transform.SetParent(parent.transform, false);
            SetLayerRecursively(child, parent.layer);
        }
        static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            Transform t = go.transform;
            for (int i = 0; i < t.childCount; i++)
                SetLayerRecursively(t.GetChild(i).gameObject, layer);
        }
        static void SetDefaultColorTransitionValues(Selectable slider)
        {
            ColorBlock colors = slider.colors;
            colors.highlightedColor = new Color(0.882f, 0.882f, 0.882f);
            colors.pressedColor = new Color(0.698f, 0.698f, 0.698f);
            colors.disabledColor = new Color(0.521f, 0.521f, 0.521f);
        }
        static void SetTrans(RectTransform rect,Vector2 pos,Vector2 anchorMin,Vector2 anchorMax,Vector2 pivot,Vector2 size)
        {
            rect.anchoredPosition = pos;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.sizeDelta = size;
            rect.pivot = pivot;
        }
        #endregion

        #region add com
        static Image AddImage(GameObject go, Image.Type type, string path)
        {
            Image temp = go.SafeAddComponet<Image>();
            temp.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(path);
            temp.type = type;
            temp.color = PanelColor;
            return temp;
        }
        static RawImage AddRawImage(GameObject go, string path)
        {
            RawImage temp = go.SafeAddComponet<RawImage>();
            temp.texture = AssetDatabase.GetBuiltinExtraResource<Sprite>(path).texture;
            temp.color = PanelColor;
            return temp;
        }
        static Mask AddMask(GameObject go)
        {
            Mask temp = go.SafeAddComponet<Mask>();
            return temp;
        }
        static Text AddText(GameObject go,string name)
        {
            Text text = go.gameObject.SafeAddComponet<Text>();
            text.text = name;
            text.font = DefaultFont;
            return text;
        }
        #endregion

        #region create
        static GameObject CreateScrollbar()
        {
            // Create GOs Hierarchy
            GameObject scrollbarRoot = CreateUIRoot("Scrollbar", ThinElementSize,null);
            GameObject sliderArea = CreateUIObject("Sliding Area", scrollbarRoot).gameObject;
            GameObject handle = CreateUIObject("Handle", sliderArea).gameObject;

            Image bgImage = scrollbarRoot.AddComponent<Image>();
            bgImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(Background);
            bgImage.type = Image.Type.Sliced;
            bgImage.color = DefaultSelectableColor;

            Image handleImage = handle.AddComponent<Image>();
            handleImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(UISprite);
            handleImage.type = Image.Type.Sliced;
            handleImage.color = DefaultSelectableColor;

            RectTransform sliderAreaRect = sliderArea.GetComponent<RectTransform>();
            sliderAreaRect.sizeDelta = new Vector2(-20, -20);
            sliderAreaRect.anchorMin = Vector2.zero;
            sliderAreaRect.anchorMax = Vector2.one;

            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20, 20);

            Scrollbar scrollbar = scrollbarRoot.AddComponent<Scrollbar>();
            scrollbar.handleRect = handleRect;
            scrollbar.targetGraphic = handleImage;
            SetDefaultColorTransitionValues(scrollbar);

            return scrollbarRoot;
        }
        #endregion

        #region Menu
        [MenuItem("Tools/UI/UDupplicate", false)]
        static void CreateUDupplicate(MenuCommand menu)
        {
            var root = CreateUIRoot<UDupplicate>(new Vector2(200, 200), menu);
        }
        [MenuItem("Tools/UI/UText", false)]
        static void CreateUText(MenuCommand menu)
        {
            var root = CreateUIRoot<UText>(new Vector2(200, 200), menu);
            var text = AddText(root.GO,"New Text");
            root.Name = text;
        }
        [MenuItem("Tools/UI/UImage", false)]
        static void CreateUImage(MenuCommand menu)
        {
            var root = CreateUIRoot<UImage>(new Vector2(200, 200), menu);
            var image = AddImage(root.GO, Image.Type.Simple,UISprite);
            root.Icon = image;
        }
        [MenuItem("Tools/UI/URawImage", false)]
        static void CreateURawImage(MenuCommand menu)
        {
            var root = CreateUIRoot<URawImage>(new Vector2(200, 200), menu);
            var image = AddRawImage(root.GO, UISprite);
            root.Image = image;
        }
        [MenuItem("Tools/UI/UIconText", false)]
        static void UIconText(MenuCommand menu)
        {
            var root = CreateUIRoot<UText>(new Vector2(318, 68), menu);
            RectTransform iconRect = CreateUIObject("icon", root.GO);
            RectTransform textRect = CreateUIObject("text", root.GO);
            SetTrans(iconRect, new Vector2(0.0f,0.0f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(68, 68));
            SetTrans(textRect, new Vector2(193f, 0.0f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(250, 68));
            var icon = AddImage(iconRect.gameObject, Image.Type.Simple, UISprite);
            var text = AddText(textRect.gameObject, "New Text");
            root.Icon = icon;
            root.Name = text;
        }
        [MenuItem("Tools/UI/UBgText", false)]
        static void UBgText(MenuCommand menu)
        {
            var root = CreateUIRoot<UText>(new Vector2(318, 68), menu);
            RectTransform textRect = CreateUIObject("text", root.GO);
            SetTrans(textRect, new Vector2(193f, 0.0f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(250, 68));
            var bg = AddImage(root.gameObject, Image.Type.Simple, UISprite);
            var text = AddText(textRect.gameObject, "New Text");
            root.Bg = bg;
            root.Name = text;
        }
        [MenuItem("Tools/UI/UButton", false)]
        static void UButton(MenuCommand menu)
        {
            var root = CreateUIRoot<UButton>(new Vector2(200, 200), menu);
            RectTransform textRect = CreateUIObject("text", root.GO);
            var bg = AddImage(root.GO, Image.Type.Simple, UISprite);
            var text = AddText(textRect.gameObject, "New Text");
            root.Bg = bg;
            root.Text = text;
        }
        [MenuItem("Tools/UI/UIconButton", false)]
        static void UIconButton(MenuCommand menu)
        {
            var root = CreateUIRoot<UButton>(new Vector2(200, 200), menu);
            RectTransform iconRect = CreateUIObject("icon", root.GO);
            RectTransform textRect = CreateUIObject("text", root.GO);
            var bg = AddImage(root.GO, Image.Type.Simple,UISprite);
            var icon = AddImage(iconRect.gameObject, Image.Type.Simple,UISprite);
            var text = AddText(textRect.gameObject,"New Text");
            root.Bg = bg;
            root.Icon = icon;
            root.Text = text;
        }
        [MenuItem("Tools/UI/UCheckBox", false)]
        static void UCheckBox(MenuCommand menu)
        {
            var root = CreateUIRoot<UCheckBox>(new Vector2(200, 200), menu);
            RectTransform activeIconRect = CreateUIObject("activeIcon", root.GO);
            RectTransform textRect = CreateUIObject("text", root.GO);
            var bg = AddImage(root.GO, Image.Type.Simple, UISprite);
            var activeIcon = AddImage(activeIconRect.gameObject, Image.Type.Simple, UISprite);
            var text = AddText(textRect.gameObject, "New Text");
            root.Bg = bg;
            root.ActiveIcon = activeIcon;
            root.Text = text;
        }
        [MenuItem("Tools/UI/UScroll_NoBar", false)]
        static void UScroll_NoBar(MenuCommand menu)
        {
            UScroll root = CreateUIRoot<UScroll>(new Vector2(200, 200), menu);
            RectTransform placeholder = CreateUIObject("placeholder", root.GO);
            RectTransform emptyDesc = CreateUIObject("EmptyDesc", root.GO);
            SetTrans(placeholder, new Vector2(0.0f, 0.0f), Vector2.up, Vector2.one, new Vector2(0.5f, 1f), new Vector2(0, 300));
            SetTrans(emptyDesc, new Vector2(0.0f, 0.0f), Vector2.up, Vector2.one, new Vector2(0.5f, 1f), new Vector2(0, 100));
            root.Content = placeholder;
            var emptyDescText = emptyDesc.gameObject.AddComponent<Text>();
            emptyDescText.text = "Empty Desc";
            emptyDescText.font = DefaultFont;
            emptyDescText.alignment = TextAnchor.UpperCenter;
            root.EmptyDesc = emptyDescText;
            AddImage(root.GO, Image.Type.Sliced,UIMask);
            AddMask(root.GO);

        }
        //protected static void InternalAddScrollView<T>(MenuCommand menuCommand) where T : ScrollView
        //{
        //    GameObject root = CreateUIElementRoot(typeof(T).Name, new Vector2(200, 200));

        //    GameObject viewport = CreateUIObject("Viewport", root);
        //    GameObject content = CreateUIObject("Content", viewport);

        //    GameObject parent = menuCommand.context as GameObject;
        //    if (parent != null)
        //    {
        //        root.transform.SetParent(parent.transform, false);
        //    }
        //    Selection.activeGameObject = root;



        //    GameObject hScrollbar = CreateScrollbar();
        //    hScrollbar.name = "Scrollbar Horizontal";
        //    hScrollbar.transform.SetParent(root.transform, false);
        //    RectTransform hScrollbarRT = hScrollbar.GetComponent<RectTransform>();
        //    hScrollbarRT.anchorMin = Vector2.zero;
        //    hScrollbarRT.anchorMax = Vector2.right;
        //    hScrollbarRT.pivot = Vector2.zero;
        //    hScrollbarRT.sizeDelta = new Vector2(0, hScrollbarRT.sizeDelta.y);

        //    GameObject vScrollbar = CreateScrollbar();
        //    vScrollbar.name = "Scrollbar Vertical";
        //    vScrollbar.transform.SetParent(root.transform, false);
        //    vScrollbar.GetComponent<Scrollbar>().SetDirection(Scrollbar.Direction.BottomToTop, true);
        //    RectTransform vScrollbarRT = vScrollbar.GetComponent<RectTransform>();
        //    vScrollbarRT.anchorMin = Vector2.right;
        //    vScrollbarRT.anchorMax = Vector2.one;
        //    vScrollbarRT.pivot = Vector2.one;
        //    vScrollbarRT.sizeDelta = new Vector2(vScrollbarRT.sizeDelta.x, 0);



        //    RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        //    viewportRect.anchorMin = Vector2.zero;
        //    viewportRect.anchorMax = Vector2.one;
        //    viewportRect.sizeDelta = Vector2.zero;
        //    viewportRect.pivot = Vector2.up;

        //    RectTransform contentRect = content.GetComponent<RectTransform>();
        //    contentRect.anchorMin = Vector2.up;
        //    contentRect.anchorMax = Vector2.one;
        //    contentRect.sizeDelta = new Vector2(0, 300);
        //    contentRect.pivot = Vector2.up;

        //    ScrollView scrollRect = root.AddComponent<T>();
        //    scrollRect.content = contentRect;
        //    scrollRect.viewport = viewportRect;
        //    scrollRect.horizontalScrollbar = hScrollbar.GetComponent<Scrollbar>();
        //    scrollRect.verticalScrollbar = vScrollbar.GetComponent<Scrollbar>();
        //    scrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
        //    scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
        //    scrollRect.horizontalScrollbarSpacing = -3;
        //    scrollRect.verticalScrollbarSpacing = -3;

        //    Image rootImage = root.AddComponent<Image>();
        //    rootImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(bgPath);
        //    rootImage.type = Image.Type.Sliced;
        //    rootImage.color = panelColor;

        //    Mask viewportMask = viewport.AddComponent<Mask>();
        //    viewportMask.showMaskGraphic = false;

        //    Image viewportImage = viewport.AddComponent<Image>();
        //    viewportImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(maskPath);
        //    viewportImage.type = Image.Type.Sliced;
        //}
        #endregion
    }
}