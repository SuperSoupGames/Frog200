using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TPSBR;
using Unity.VisualScripting;
using UnityEngine;



public class MyGameModeManager : MonoBehaviour
{
    public Material GoodMaterial;
    public Material BadMaterial;

    public List<GameObject> LightSources;
    public List<string> LightSourceNames = new List<string>();

    public GameObject StaticRoot;

    public Gameplay Gameplay;

    // Start is called before the first frame update
    void Start()
    {
        LightSources = GameObject.FindGameObjectsWithTag("LightSource").ToList<GameObject>();
        if(LightSources is null)
        {
            Debug.LogError("NO LIGHT SOURCES FOUND!");
            return;
        }
        foreach (var lightSource in LightSources)
        {
            var parentName = lightSource.transform.parent.name;
            if(parentName != "STATIC")
            {
                var oldParent = lightSource.transform.parent;
                lightSource.transform.parent = StaticRoot.transform;
                lightSource.transform.SetSiblingIndex(oldParent.GetSiblingIndex() + 1);
            }
            var name = lightSource.name;
            LightSourceNames.Add(lightSource.name);
        }


    }

    bool _firstTime = true;
    float _waitForFirstTime = 10f;
    // Update is called once per frame
    void Update()
    {
        if(Gameplay.Context.Runner is not null)
        {
            //Debug.Log("Game time: " + Gameplay.Context.Runner.SimulationRenderTime);
            if (_firstTime && Gameplay.Context.Runner.SimulationRenderTime > _waitForFirstTime)
            {
                _firstTime = false;
                for (int i = 0; i < StaticRoot.transform.childCount; i++)
                {
                    Debug.Log("total kids: " + StaticRoot.transform.childCount);
                    var light = StaticRoot.transform.GetChild(i);
                    if (light.tag == "LightSource")
                    {
                        MakeLightOn(i);
                    } else if (light.tag == "Untagged")
                    {
                        var a = 1;
                    } else
                    {
                        var b = 2;
                    }
                }
            }
        }
    }

    public void MakeLightOn(int index)
    {
        Debug.Log("LIGHT ON: " + index.ToString());
        var light = StaticRoot.transform.GetChild(index);
        var poleScript = light.GetComponent<LightPole>();
        poleScript.OFF.SetActive(false);
        poleScript.ON.SetActive(true);
        poleScript.LightGroup.SetActive(true);
        light.GetComponent<MeshRenderer>().material = GoodMaterial;
    }

    public void MakeLightOff(int index)
    {
        Debug.Log("LIGHT OFF: " + index.ToString());
        var light = StaticRoot.transform.GetChild(index);
        var poleScript = light.GetComponent<LightPole>();
        poleScript.OFF.SetActive(true);
        poleScript.ON.SetActive(false);
        poleScript.LightGroup.SetActive(false);
        light.GetComponent<MeshRenderer>().material = BadMaterial;
    }

}
