using UnityEngine;
namespace CYM
{
    public class EffectStar : MonoBehaviour
    {

        public GameObject[] starFX;
        public int ea;
        public int currentEa;
        public float delay;
        public float currentDelay;
        public bool isEnd;
        public int idStar;
        public static EffectStar myStar;

        void Awake()
        {
            myStar = this;
        }

        void Start()
        {

        }
        private void OnEnable()
        {
            Reset();
        }

        void Update()
        {
            if (!isEnd)
            {
                currentDelay -= Time.deltaTime;
                if (currentDelay <= 0)
                {
                    if (currentEa != ea)
                    {
                        currentDelay = delay;
                        starFX[currentEa].SetActive(true);
                        currentEa++;
                    }
                    else
                    {
                        isEnd = true;
                        currentDelay = delay;
                        currentEa = 0;
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                Reset();
            }
        }

        public void Reset()
        {
            for (int i = 0; i < 3; i++)
            {
                starFX[i].SetActive(false);
            }
            currentDelay = delay;
            currentEa = 0;
            isEnd = false;
            for (int i = 0; i < 3; i++)
            {
                starFX[i].SetActive(false);
            }
        }
    }
}