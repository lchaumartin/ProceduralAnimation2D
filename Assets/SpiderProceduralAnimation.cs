using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderProceduralAnimation : MonoBehaviour
{
    public Transform[] legTargets;
    public float stepSize = 0.15f;
    public int smoothness = 8;
    public float stepHeight = 0.15f;
    public bool bodyOrientation = true;

    public float raycastRange = 1.5f;
    private Vector2[] defaultLegPositions;
    private Vector2[] lastLegPositions;
    private Vector2 lastBodyUp;
    private bool[] legMoving;
    private int nbLegs;
    
    private Vector2 velocity;
    private Vector2 lastVelocity;
    private Vector2 lastBodyPos;

    private float velocityMultiplier = 7f;

    Vector2[] MatchToSurfaceFromAbove(Vector2 point, float halfRange, Vector2 up)
    {
        Vector2[] res = new Vector2[2];
        res[1] = Vector3.zero;
        RaycastHit2D hit;
        hit = Physics2D.Raycast(point + halfRange * up / 2f, -up, 2f * halfRange);
        Debug.DrawRay(point + halfRange * up / 2f, - up * 2f * halfRange, Color.red, smoothness * Time.deltaTime);
        if (hit.collider)
        {
            res[0] = hit.point;
            res[1] = hit.normal;
        }
        else
        {
            res[0] = point;
        }
        return res;
    }
    
    void Start()
    {
        lastBodyUp = transform.up;

        nbLegs = legTargets.Length;
        defaultLegPositions = new Vector2[nbLegs];
        lastLegPositions = new Vector2[nbLegs];
        legMoving = new bool[nbLegs];
        for (int i = 0; i < nbLegs; ++i)
        {
            defaultLegPositions[i] = legTargets[i].localPosition;
            lastLegPositions[i] = legTargets[i].position;
            legMoving[i] = false;
        }
        lastBodyPos = transform.position;
    }

    IEnumerator PerformStep(int index, Vector3 targetPoint)
    {
        Vector3 startPos = lastLegPositions[index];
        for(int i = 1; i <= smoothness; ++i)
        {
            legTargets[index].position = Vector3.Lerp(startPos, targetPoint, i / (float)(smoothness + 1f));
            legTargets[index].position += transform.up * Mathf.Sin(i / (float)(smoothness + 1f) * Mathf.PI) * stepHeight;
            yield return new WaitForFixedUpdate();
        }
        legTargets[index].position = targetPoint;
        lastLegPositions[index] = legTargets[index].position;
        legMoving[0] = false;
    }


    void FixedUpdate()
    {
        velocity = (Vector2)transform.position - lastBodyPos;
        velocity = (velocity + smoothness * lastVelocity) / (smoothness + 1f);

        if (velocity.magnitude < 0.000025f)
            velocity = lastVelocity;
        else
            lastVelocity = velocity;
        
        
        Vector2[] desiredPositions = new Vector2[nbLegs];
        int indexToMove = -1;
        float maxDistance = stepSize;
        for (int i = 0; i < nbLegs; ++i)
        {
            desiredPositions[i] = transform.TransformPoint(defaultLegPositions[i]);

            float distance = Vector3.ProjectOnPlane(desiredPositions[i] + velocity * velocityMultiplier - lastLegPositions[i], transform.up).magnitude;
            if (distance > maxDistance)
            {
                maxDistance = distance;
                indexToMove = i;
            }
        }
        for (int i = 0; i < nbLegs; ++i)
            if (i != indexToMove)
                legTargets[i].position = lastLegPositions[i];

        if (indexToMove != -1 && !legMoving[0])
        {
            Vector2 targetPoint = desiredPositions[indexToMove] + Mathf.Clamp(velocity.magnitude * velocityMultiplier, 0.0f, 1.5f) * (desiredPositions[indexToMove] - 
                (Vector2)legTargets[indexToMove].position) + velocity * velocityMultiplier;

            Vector2[] positionAndNormalFwd = MatchToSurfaceFromAbove(targetPoint + velocity * velocityMultiplier, raycastRange, 
                ((Vector2)transform.parent.up - velocity * 10).normalized);

            Vector2[] positionAndNormalBwd = MatchToSurfaceFromAbove(targetPoint + velocity * velocityMultiplier, raycastRange*(1f + velocity.magnitude), 
                ((Vector2)transform.parent.up + velocity * 10).normalized);
            
            legMoving[0] = true;
            
            if (positionAndNormalFwd[1] == Vector2.zero)
            {
                StartCoroutine(PerformStep(indexToMove, positionAndNormalBwd[0]));
            }
            else
            {
                StartCoroutine(PerformStep(indexToMove, positionAndNormalFwd[0]));
            }
        }

        lastBodyPos = transform.position;
        if (nbLegs > 1 && bodyOrientation)
        {
            Vector2 v1 = (legTargets[1].position - legTargets[0].position).normalized;
            
            Vector3 v2 = Vector3.back;
            Vector3 normal = Vector3.Cross(v1, v2).normalized;
            Vector3 up = Vector3.Lerp(lastBodyUp, normal, 1f / (float)(smoothness + 1));
            transform.up = up;
            transform.rotation = Quaternion.LookRotation(transform.parent.forward, up);
            lastBodyUp = transform.up;
        }
    }

    private void OnDrawGizmosSelected()
    {
        for (int i = 0; i < nbLegs; ++i)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(legTargets[i].position, 0.05f);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.TransformPoint(defaultLegPositions[i]), stepSize);
        }
    }
}
