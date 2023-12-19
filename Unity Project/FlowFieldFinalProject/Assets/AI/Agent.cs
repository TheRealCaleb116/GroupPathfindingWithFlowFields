using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class Agent : MonoBehaviour
{
    public float speed = 5.0f;
    public float maxSpeed = 50.0f;
    public float maxSteeringForce = 10.0f;
    public float maxRotationSpeed = 10.0f;
    public Vector3 velocity = Vector3.zero;
    public float slowingRadius = 5.0f;
    public float agentGoalDist = 1.0f;
    public float damp = 0.85f;
    public Vector3 currentDirection;


    public Vector3 front = Vector3.forward;

    public Vector3 targetDirection = Vector3.forward;

    public float distFromGround = 1.0f;

    private Rigidbody RB;
    private Terrain terrain;

    // Start is called before the first frame update
    void Awake()
    {
        GameManager.registerAgent(this);

        RB = GetComponent<Rigidbody>();
        terrain = GameObject.Find("Terrain").GetComponent<Terrain>();

        //Roatet and set up current direction
        currentDirection = transform.TransformVector(front);
        RB.MoveRotation(Quaternion.FromToRotation(front, currentDirection));
        //targetDirection = currentDirection;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        //Get distance to goal
        Vector3 goalPos3D = GameManager.GetCurrentGoal();
        float distanceToGoal = Vector2.Distance(new Vector2(transform.position.x,transform.position.z), new Vector2(goalPos3D.x, goalPos3D.z));
        //Debug.Log("dist to goal: " + distanceToGoal);

        //Calculate new position and rotation
        Vector3 newPosition = RB.position + velocity * Time.fixedDeltaTime;

        //Set position above ground
        newPosition.y = terrain.SampleHeight(newPosition) + distFromGround;

        Quaternion newRotation = Quaternion.FromToRotation(front, velocity.normalized);

        //Apply movement
        RB.Move(newPosition, newRotation);

        //Update the current direction
        currentDirection = transform.TransformVector(front);

       
        //Get Updated Target Direction
        Vector2 targ2d = PathfindingGrid.GetDirectionAtPosition(RB.position);
        targetDirection = new Vector3(targ2d.x, 0.0f, targ2d.y).normalized;

        //No desire to move in a direction
        if (targetDirection == Vector3.zero) { return; }

        //Clamp the change in direction
        Vector3 clampedTargetDirection = Vector3.RotateTowards(currentDirection, targetDirection, maxRotationSpeed * Mathf.Deg2Rad, 0.0f);
        Vector3 targetVelocity = clampedTargetDirection.normalized * maxSpeed;
        //Debug.DrawRay(transform.position, targetVelocity, Color.blue);


        //Clamp target velocity for arrival
        if (distanceToGoal < slowingRadius)
        {
            targetVelocity = targetVelocity.normalized * maxSpeed * (distanceToGoal / slowingRadius);
        }

        if (distanceToGoal < agentGoalDist)
        {
            //Debug.Log("In closing dist");
            targetVelocity = Vector3.zero;
        }

        Vector3 steering = targetVelocity - velocity;
        steering = steering.normalized * Mathf.Min(steering.magnitude, maxSteeringForce);
        steering = steering / RB.mass;


        //Debug.DrawRay(transform.position, steering, Color.yellow);

        //Debug.DrawRay(transform.position, velocity, Color.red);

        //Update velocity/direciton
        velocity += steering * Time.fixedDeltaTime * speed;
        
        //Cap Speed
        if (velocity.magnitude > maxSpeed)
        {
            velocity = velocity.normalized * maxSpeed;
        }

        if (targetVelocity == Vector3.zero)
        {
            velocity *= damp;
        }


        RB.velocity = velocity;


       


    }


}
