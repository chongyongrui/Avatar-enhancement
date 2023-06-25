using System.Collections.Generic;
using UnityEngine;

public class CarAi : MonoBehaviour
{
    private PrometeoCarController carController;
    [SerializeField] private float forwardAmount = 0f;
    [SerializeField] private float turnAmount = 0f;
    [SerializeField] private string turnTriggerTag;
    [SerializeField] private string targetPositionTag;
    [SerializeField] private float turnSpeed = 1f;
    [SerializeField] private float turnTriggerRange = 2f; // Range to slow down and make the turn
    private Vector3 targetPosition;

    public List<TurnTrigger> turnTriggers = new List<TurnTrigger>();
    private int currentTriggerIndex = 0;
    private bool reachedTarget = false;

    private void Start()
    {
        carController = GetComponent<PrometeoCarController>();

        // Find all objects with the specified turn trigger tag
        GameObject[] turnTriggerObjects = GameObject.FindGameObjectsWithTag(turnTriggerTag);

        foreach (GameObject turnTriggerObject in turnTriggerObjects)
        {
            TurnTrigger turnTrigger = turnTriggerObject.GetComponent<TurnTrigger>();
            if (turnTrigger != null)
            {
                turnTriggers.Add(turnTrigger);
            }
        }

        // Sort the turn triggers based on their assigned indices
        turnTriggers.Sort((a, b) => a.Index.CompareTo(b.Index));

        if (turnTriggers.Count > 0)
        {
            setTarget(turnTriggers[0].transform.position);
        }
        else if (!string.IsNullOrEmpty(targetPositionTag))
        {
            GameObject targetObject = GameObject.FindWithTag(targetPositionTag);
            if (targetObject != null)
            {
                setTarget(targetObject.transform.position);
            }
            else
            {
                Debug.LogError("Target position GameObject with tag " + targetPositionTag + " not found!");
            }
        }
        else
        {
            Debug.LogError("No turn triggers or target position set!");
        }
    }

    private void Update()
    {
        if (!reachedTarget)
        {
            if (currentTriggerIndex >= turnTriggers.Count)
            {
                // Reached the last turn trigger or no turn triggers available, move towards the target position
                Vector3 dirToMove = (targetPosition - transform.position).normalized;
                float dot = Vector3.Dot(transform.forward, dirToMove);

                forwardAmount = 1f;
                turnAmount = Mathf.Clamp(dot * turnSpeed, -1f, 1f);

                // Check if the car has reached the target position
                float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
                if (distanceToTarget <= 1f)
                {
                    forwardAmount = 0f;
                    reachedTarget = true;
                }
            }
            else
            {
                // Move towards the current turn trigger
                Vector3 dirToMove = (turnTriggers[currentTriggerIndex].transform.position - transform.position).normalized;
                float dot = Vector3.Dot(transform.forward, dirToMove);

                if (dot < 0.99f)
                {
                    forwardAmount = 1f;
                    turnAmount = Mathf.Clamp(dot * turnSpeed, -1f, 1f);
                }
                else
                {
                    forwardAmount = 0f;
                    turnAmount = 0f;
                }

                float angleOfDir = Vector3.SignedAngle(transform.forward, dirToMove, Vector3.up);

                if (Mathf.Abs(angleOfDir) <= turnTriggerRange)
                {
                    // Slow down or come to a pause before making the turn
                    forwardAmount = 0.5f;
                }

                // Check if the car has reached the current turn trigger
                float distanceToTrigger = Vector3.Distance(transform.position, turnTriggers[currentTriggerIndex].transform.position);
                if (distanceToTrigger <= 1f)
                {
                    currentTriggerIndex++; // Move to the next turn trigger
                    if (currentTriggerIndex >= turnTriggers.Count)
                    {
                        // Reached the last turn trigger, move towards the target position
                        setTarget(targetPosition);
                    }
                }
            }
        }
        else
        {
            // Car has reached the target position, stop completely
            forwardAmount = 0f;
            turnAmount = 0f;
        }

        // Send the input values to the car controller
        carController.setInputs(forwardAmount, turnAmount);
    }

    public void AddTurnTrigger(TurnTrigger turnTrigger)
    {
        turnTriggers.Add(turnTrigger);

        // Sort the turn triggers based on their assigned indices
        turnTriggers.Sort((a, b) => a.Index.CompareTo(b.Index));
    }

    public void setTarget(Vector3 targetPos)
    {
        this.targetPosition = targetPos;
    }
}
