using UnityEngine;
using UnityEngine.UI;

namespace CYM.UI
{
    public class UIGlobeFillInverter : MonoBehaviour
    {
        [SerializeField] private UnityEngine.UI.Image m_Image;

        public void OnChange(float value)
        {
            if (this.m_Image != null)
            {
                this.m_Image.fillAmount = 1f - value;
            }
        }
    }
}
