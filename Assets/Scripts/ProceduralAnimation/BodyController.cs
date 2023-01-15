using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyController : MonoBehaviour
{
    [SerializeField] private GameObject[] legTargets;
    [SerializeField] private GameObject[] legCubes;
    [SerializeField] private GameObject spider;
    
    [SerializeField] private float moveDistance = 2.5f;
    [SerializeField] private int legMovementSmoothness = 5;
    [SerializeField] private int bodySmoothness = 8;
    [SerializeField] private int velocitySmoothness = 3;
    [SerializeField] private float overStepMultiplier = 1.3f;
    [SerializeField] private int waitTimeBetweenSteps = 2;
    [SerializeField] private float spiderJitterCutOff = 0.1f;
    [SerializeField] private float stepHeight = 0.5f;

    private bool currentLeg = true;
    private Vector3 lastBodyUp;
    [SerializeField] private bool enableBodyRotation = true;
    
    private Vector3[] legPositions;
    private Vector3[] legOriginalPositions;
    private Vector3 velocity;
    private Vector3 lastRecordedVelocity;
    private Vector3 lastSpiderPosition;

    private List<int> oppositeLegIndex = new List<int>();
    private List<int> nextIndexToMove = new List<int>();
    private List<int> indexMoving = new List<int>();

    void Start()
    {
        lastBodyUp = transform.up;
        
        legPositions = new Vector3[legTargets.Length];
        legOriginalPositions = new Vector3[legTargets.Length];
        
        for (int i = 0; i < legPositions.Length; i++)
        {
            Vector3 position = legTargets[i].transform.position;
            legPositions[i] = position;
            legOriginalPositions[i] = position;

            if (currentLeg)
            {
                oppositeLegIndex.Add(i+1);
                currentLeg = false;
            } else if (!currentLeg)
            {
                oppositeLegIndex.Add(i-1);
                currentLeg = true;
            }
        }
        lastSpiderPosition = spider.transform.position;

        RotateBody();        
    }

    void FixedUpdate()
    {
        velocity = spider.transform.position - lastSpiderPosition;
        velocity = velocity + velocitySmoothness * lastRecordedVelocity;
        velocity = velocity / (velocitySmoothness + 1);
        
        MoveLegs();
        RotateBody();

        lastSpiderPosition = spider.transform.position;
        lastRecordedVelocity = velocity;
    }

    private void RotateBody()
    {
        if (!enableBodyRotation) return;

        Vector3 v1 = legTargets[0].transform.position - legTargets[1].transform.position;
        Vector3 v2 = legTargets[2].transform.position - legTargets[3].transform.position;
        Vector3 normal = Vector3.Cross(v1,v2).normalized;
        Vector3 up = Vector3.Lerp(lastBodyUp, normal, 1f/bodySmoothness);
        transform.up = up;
        //transform.rotation = Quaternion.LookRotation(transform.parent.forward, up);
        lastBodyUp = transform.up;
    }

    private void MoveLegs()
    {
        
        for (int i = 0; i < legTargets.Length; i++)
        {
            if (Vector3.Distance(legTargets[i].transform.position, legCubes[i].transform.position) >= moveDistance)
            {
                if (!nextIndexToMove.Contains(i) && !indexMoving.Contains(i)) nextIndexToMove.Add(i);
            } else if (!indexMoving.Contains(i))
            {
                legTargets[i].transform.position = legOriginalPositions[i];
            }
        }
        
        if (nextIndexToMove.Count == 0 || indexMoving.Count != 0) return;

        Vector3 targetPosition = legCubes[nextIndexToMove[0]].transform.position;
        targetPosition = targetPosition + Mathf.Clamp(velocity.magnitude * overStepMultiplier, 0, 2) *
            (legCubes[nextIndexToMove[0]].transform.position - legTargets[nextIndexToMove[0]].transform.position) +
             velocity * overStepMultiplier;
        //Debug.Log(nextIndexToMove[0] + "Step()");
        StartCoroutine(step(nextIndexToMove[0], targetPosition,  false));
    }

    IEnumerator step(int index, Vector3 moveTo, bool isOpposite)
    {
        if (!isOpposite) MoveOppositeLeg(oppositeLegIndex[index]);
        
        if (nextIndexToMove.Contains(index)) nextIndexToMove.Remove(index);
        if (!indexMoving.Contains(index)) indexMoving.Add(index);

        Vector3 startingPosition = legOriginalPositions[index];

        for (int i = 1; i <= legMovementSmoothness; i++)
        {
            legTargets[index].transform.position = Vector3.Lerp(startingPosition,
                moveTo + new Vector3(0, Mathf.Sign(i / (legMovementSmoothness + spiderJitterCutOff) * Mathf.PI) * stepHeight, 0),
                i / legMovementSmoothness);
            yield return new WaitForFixedUpdate();
        }

        legOriginalPositions[index] = moveTo;

        for (int i = 1; i <= waitTimeBetweenSteps; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        
        if (indexMoving.Contains(index)) indexMoving.Remove(index);
    }

    private void MoveOppositeLeg(int index)
    {
        Vector3 targetPosition = legCubes[index].transform.position;
        targetPosition = targetPosition + Mathf.Clamp(velocity.magnitude * overStepMultiplier, 0, 1.5f) *
            (legCubes[index].transform.position - legTargets[index].transform.position) +
             velocity * overStepMultiplier;
        //Debug.Log(index + "MoveOppositeLeg()_Step()");
        StartCoroutine(step(index, targetPosition,  true));
    }

}
