using UnityEngine;
using UnityEngine.UI;

namespace CYM.UI
{
    [ExecuteInEditMode]
    public class UIScrollbarFixeder : MonoBehaviour
    {

        Scrollbar Scrollbar;
        private void Awake()
        {
            Scrollbar = GetComponent<Scrollbar>();
        }

        private void Update()
        {
            if (Scrollbar != null)
            {
                Scrollbar.size = 0.0f;
            }
        }
    }

}