using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameObject pointerPrefab;
    private GameObject pointer;
    private GameObject gridComponant;
    private NavMeshAgent navMeshAgent;
    private GridGenerator gridGenerator;

    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        pointer = Instantiate(pointerPrefab, gameObject.transform.position, Quaternion.identity);
        gridComponant = GameObject.FindGameObjectWithTag("Generator");
        gridGenerator = gridComponant.GetComponent<GridGenerator>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;

            if (Physics.Raycast(ray, out rayHit))
            {
                //Debug.Log("Mouse Position: " + Input.mousePosition);
                navMeshAgent.SetDestination(rayHit.point);
                pointer.transform.position = rayHit.point;
                gridGenerator.endPosition = rayHit.point;
                //Debug.Log("End Point: " + gridGenerator.endPosition);
            } 
        }
    }
}
