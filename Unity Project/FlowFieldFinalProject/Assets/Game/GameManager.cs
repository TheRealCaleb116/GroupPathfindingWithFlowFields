using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    //Singleton pattern 
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }

    public static List<Agent> agents = new List<Agent>();

    public static bool SeekTowardsGoal = false;
    public static bool AvoidObstacles = false;

    public static Vector3 GetCurrentGoal()
    {
        return _instance.goal.transform.position;
    }


    public static void registerAgent(Agent a)
    {
        agents.Add(a);
    }

    public GameObject goal;
    public GameObject agentPrefab;
    public GameObject agentsContainer;



    //Gameobject Instance Methods
    public void Awake()
    {
        //Singleton pattern checks
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }


    }

    public void Start()
    {
    }

    private void Update()
    {
        
        //Right click for new boid
        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log(hit.transform.name);
                GameObject b = Instantiate(agentPrefab, agentsContainer.transform, false);
                b.GetComponent<Rigidbody>().position = (new Vector3(hit.point.x, 0.0f, hit.point.z));

            }

        }

        //Left Click for goal pos
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                //Debug.Log(hit.transform.name);
                goal.transform.position = hit.point;
                PathfindingGrid.GenFlowField(GetCurrentGoal());
            }
        }
        

    }



}
