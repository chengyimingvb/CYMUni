using UnityEngine;
using UnityEngine.UI;

namespace CYM.UI
{
    public class UIStatsAdd : MonoBehaviour
    {

        [SerializeField] private UnityEngine.UI.Text m_ValueText;

        public void OnButtonPress()
        {
            if (this.m_ValueText == null)
                return;

            this.m_ValueText.text = (int.Parse(this.m_ValueText.text) + 1).ToString();
        }
    }
}
