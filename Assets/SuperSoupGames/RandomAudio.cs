using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TPSBR
{
    public class RandomAudio : MonoBehaviour
    {
        public GameObject[] Audios;


        // Start is called before the first frame update
        void Start()
        {
        
        }
        bool awakedOne = false;
        // Update is called once per frame
        void Update()
        {
            if(awakedOne == false)
            {
                int rand = Random.Range(0, Audios.Length - 1);
                Audios[rand].SetActive(true);
                awakedOne = true;
            }
        
        }
    }
}
