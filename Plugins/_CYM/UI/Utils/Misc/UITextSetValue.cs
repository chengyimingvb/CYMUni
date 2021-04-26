using UnityEngine;
using UnityEngine.UI;

namespace CYM.UI
{
    [RequireComponent(typeof(UnityEngine.UI.Text))]
    public class UITextSetValue : MonoBehaviour
    {

        private UnityEngine.UI.Text m_Text;
        public string floatFormat = "0.00";

        protected void Awake()
        {
            this.m_Text = this.gameObject.GetComponent<UnityEngine.UI.Text>();
        }

        public void SetFloat(float value)
        {
            if (this.m_Text != null)
                this.m_Text.text = value.ToString(floatFormat);
        }
    }
}
