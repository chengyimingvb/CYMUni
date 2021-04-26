using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;
namespace CYM.UI
{
    [AddComponentMenu("UI/Control/UImage")]
    [HideMonoScript]
    public class URawImage : UPresenter<UImageData>
    {
        #region 组建
        [FoldoutGroup("Inspector")]
        public RawImage Image;
        #endregion

        #region life
        public override void Refresh()
        {
            base.Refresh();
            if (Data.Icon != null && Image != null)
            {
                Sprite temp = Data.Icon.Invoke();
                if (temp == null)
                    return;
                Image.texture = temp.texture;
            }
        }
        public void Refresh(string icon)
        {
            if (!icon.IsInv())
                Image.texture = icon.GetIcon().texture;
        }
        public void Refresh(Sprite icon)
        {
            Image.texture = icon.texture;
        }
        #endregion

    }
}