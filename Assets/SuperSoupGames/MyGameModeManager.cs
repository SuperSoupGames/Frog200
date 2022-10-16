using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class MyGameModeManager : MonoBehaviour
{
    public Material GoodMaterial;
    public Material BadMaterial;

    public GameObject[] LightSources;

    // Start is called before the first frame update
    void Start()
    {
        LightSources = GameObject.FindGameObjectsWithTag("LightSource");
        if(LightSources is null)
        {
            Debug.LogError("NO LIGHT SOURCES FOUND!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
