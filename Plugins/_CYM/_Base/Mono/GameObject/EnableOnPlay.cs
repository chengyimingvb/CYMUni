using UnityEngine;
namespace CYM
{
    public class EnableOnPlay : MonoBehaviour
    {
        void Awake()
        {
            gameObject.SetActive(true);
        }

    }
}
