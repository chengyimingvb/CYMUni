using UnityEngine;
namespace CYM.Utile
{
    public class KeepUnderCamera : MonoBehaviour
    {
        void LateUpdate()
        {
            if (Application.isPlaying)
            {
                Transform camTrans = BaseGlobal.MainCamera.transform;
                Vector3 cp = camTrans.position;
                cp.y = transform.position.y;
                if (transform.position != cp) transform.position = cp;
            }
        }
    }
}
