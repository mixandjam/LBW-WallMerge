using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MoveTest : MonoBehaviour
{

    [Header("Movement Parameters")]
    public float movSpeed = 3;
    public float rotSpeed = 2;
    public float rotationLerp;
    public float distanceToTurn = 1f;

    [Space]

    [Header("Booleans")]
    public bool isMoving;
    public bool isRotating;
    public bool isGoingRight;
    public bool isNextCornerRight;

    public Vector3 originPos;
    public Vector3 targetPos;
    public Vector3 debug;

    private int currentIndex, previousIndex, nextIndex, extraIndex;

    public RaySearch search;
    public Transform pivot;
    public Transform lineRef1, lineRef2;

    private Vector3 savedNormal;

    public void SetPosition(Vector3 orig, Vector3 target, float lerp, RaySearch ray, bool nextCornerIsRight, Vector3 normal)
    {
        transform.forward = normal;
        transform.position = Vector3.Lerp(orig, target, lerp);
        search = ray;
        originPos = orig;
        targetPos = target;
        isMoving = true;
        isNextCornerRight = nextCornerIsRight;
    }

    void Debug()
    {
        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
    }

    private void Update()
    {
        Debug();

        float axis = Input.GetAxis("Horizontal");

        if (isMoving && !isRotating)
        {
            //move player between the two corner points
            transform.position = Vector3.MoveTowards(transform.position, isNextCornerRight ? targetPos : originPos, axis * Time.deltaTime * movSpeed);

            if(Vector3.Distance(transform.position, originPos) > (Vector3.Distance(originPos,targetPos) - distanceToTurn))
            {
                StartRotation(true);
            }
            else if(Vector3.Distance(transform.position, originPos) < distanceToTurn)
            {
                StartRotation(false);
            }

        }

        if (isRotating && !isMoving)
        {
            CornerRoration(axis);
        }
    }

    public void StartRotation(bool right)
    {
        isGoingRight = right;
        isMoving = false;
        savedNormal = transform.forward;

        currentIndex = search.cornerPoints.FindIndex(x => x.position == targetPos);

        previousIndex = currentIndex + (right ? -1 : 1);
        if (previousIndex > search.cornerPoints.Count)
            previousIndex = 0;
        if (previousIndex < 0)
            previousIndex = search.cornerPoints.Count-1;

        nextIndex = currentIndex + (right ? 1 : -1);
        if (nextIndex > search.cornerPoints.Count)
            nextIndex = 0;
        if (nextIndex < 0)
            nextIndex = search.cornerPoints.Count-1;

        extraIndex = nextIndex + (right ? 1 : -1);
        if (extraIndex > search.cornerPoints.Count)
            extraIndex = 0;
        if (extraIndex < 0)
            extraIndex = search.cornerPoints.Count - 1;

        pivot.position = GetPivotPosition(right);
        transform.parent = pivot;
        rotationLerp = .01f;
        isRotating = true;
    }

    public void CornerRoration(float axis)
    {
        float n = isGoingRight ? 1 : -1;
        Vector3 normal = isGoingRight ? search.cornerPoints[currentIndex].normal : search.cornerPoints[extraIndex].normal;

        rotationLerp = Mathf.Clamp(rotationLerp + ((axis * n) * Time.deltaTime * rotSpeed), 0, 1);
        pivot.forward = Vector3.Lerp(savedNormal, normal, rotationLerp);

        if (rotationLerp >= 1 || rotationLerp <= 0)
        {
            bool complete = (rotationLerp >= 1) ? true : false;
            print(complete);

            originPos = complete ? search.cornerPoints[complete ? currentIndex : previousIndex].position : originPos;
            targetPos = complete ? search.cornerPoints[complete ? nextIndex : currentIndex].position : targetPos;

            transform.parent = null;
            isRotating = false;
            isMoving = true;
            rotationLerp = .01f;
        }
    }

    public Vector3 GetPivotPosition(bool right)
    {
        lineRef1.position = right ? targetPos : originPos;
        lineRef1.LookAt(right ? originPos : targetPos);
        lineRef1.localPosition += lineRef1.forward * distanceToTurn;
        lineRef1.forward = savedNormal;

        lineRef2.position = right ? targetPos : originPos;
        lineRef2.LookAt(search.cornerPoints[nextIndex].position);
        lineRef2.localPosition += lineRef2.forward * distanceToTurn;
        lineRef2.forward = search.cornerPoints[right ? currentIndex : extraIndex].normal;

        pivot.forward = savedNormal;

        Vector3 intersection;
        LineLineIntersection(out intersection, lineRef1.position, lineRef1.forward, lineRef2.position, lineRef2.forward);

        return intersection;
    }

    public static bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
    {

        Vector3 lineVec3 = linePoint2 - linePoint1;
        Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
        Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

        float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

        //is coplanar, and not parrallel
        if (Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f)
        {
            float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
            intersection = linePoint1 + (lineVec1 * s);
            return true;
        }
        else
        {
            intersection = Vector3.zero;
            return false;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(originPos, .5f);
        Gizmos.DrawSphere(targetPos, .5f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(debug, .5f);
    }
}
