using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace TPSBR
{
    public class KillBox : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private void OnTriggerStay(Collider other)
        {
            Debug.Log(other.gameObject.name);
            other.gameObject.GetComponentInParent<Agent>().AgentInput.InSuicideBox = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log(other.gameObject.name);
            other.gameObject.GetComponentInParent<Agent>().AgentInput.InSuicideBox = true;
        }

        private void OnCollisionStay(Collision collision)
        {
            Debug.Log(collision.gameObject.name);
            collision.gameObject.GetComponentInParent<Agent>().AgentInput.InSuicideBox = true;
        }

        private void OnCollisionEnter(Collision collision)
        {
            Debug.Log(collision.gameObject.name);
            collision.gameObject.GetComponentInParent<Agent>().AgentInput.InSuicideBox = true;
        }
    }
}
