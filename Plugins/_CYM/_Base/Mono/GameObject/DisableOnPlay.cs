using UnityEngine;
namespace CYM
{

    public class DisableOnPlay : MonoBehaviour
    {

        void Awake()
        {
            gameObject.SetActive(false);
        }
    }

}