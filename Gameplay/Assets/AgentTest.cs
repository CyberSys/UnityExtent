using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class AgentTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<NavMeshAgent>().SetDestination(new Vector3(0.5f, 0.0f, 10.5f));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
