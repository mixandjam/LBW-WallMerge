using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MoveTest : MonoBehaviour
{

    [Header("Movement Parameters")]
    public float movSpeed = 3;
    public float rotSpeed = 2;

    public bool active;
    public bool rotation;
    public bool isGoingRight;

    public float rotationLerp;

    public Vector3 p1;
    public Vector3 p2;
    public int i1, i2,i3,i0;
    public RaySearch search;
    public Transform pivot;
    public Transform lineRef1, lineRef2;

    private Vector3 savedNormal;

    private float distanceToTurn = 1f;

    public void SetPosition(Vector3 pos1, Vector3 pos2, float lerp, RaySearch ray, bool nextCornerIsRight, Vector3 normal)
    {
        transform.forward = normal;
        transform.position = Vector3.Lerp(pos1, pos2, lerp);
        search = ray;
        p1 = nextCornerIsRight ? pos2 : pos1;
        p2 = nextCornerIsRight ? pos1 : pos2;
        active = true;
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
        }

        float axis = Input.GetAxis("Horizontal");

        if (active)
        {

            transform.position = Vector3.MoveTowards(transform.position, p2, axis * Time.deltaTime * movSpeed);

            if(Vector3.Distance(transform.position, p1) > (Vector3.Distance(p1,p2) - distanceToTurn))
            {
                StartRotation(true);
            }
            else if(Vector3.Distance(transform.position, p1) < distanceToTurn)
            {
                print("left");
                StartRotation(false);
            }

        }

        if (rotation)
        {
            float n = isGoingRight ? 1 : -1;
            Vector3 normal = isGoingRight ? search.cornerPoints[i1].normal : search.cornerPoints[i3].normal;

            rotationLerp = Mathf.Clamp(rotationLerp + ((axis*n) * Time.deltaTime * rotSpeed), 0, 1);
            pivot.forward = Vector3.Lerp(savedNormal, normal, rotationLerp);

            if (rotationLerp >= 1 || rotationLerp <= 0)
            {
                bool complete = (rotationLerp >= 1) ? true : false;


                p1 = complete ? search.cornerPoints[complete ? i1 : i0].position : p1;
                p2 = complete ? search.cornerPoints[complete ? i2 : i1].position : p2;

                transform.parent = null;
                rotation = false;
                active = true;
                rotationLerp = .01f;
            }
        }
    }

    public void StartRotation(bool right)
    {
        isGoingRight = right;
        active = false;

        savedNormal = transform.forward;

        i1 = search.cornerPoints.FindIndex(x => x.position == p2);
        i0 = right ? 0 : ((i1 == 0) ? search.cornerPoints.Count - 1 : i1 - 1);
        i2 = right ? ((i1 == search.cornerPoints.Count - 1) ? 0 : i1 + 1) : ((i1 == 0) ? search.cornerPoints.Count-1 : i1 - 1);
        i3 = right ? 0 : ((i2 == 0) ? search.cornerPoints.Count - 1 : i2 - 1); 

        pivot.position = GetPivotPosition(right);
        transform.parent = pivot;
        rotationLerp = .01f;
        rotation = true;
    }

    public Vector3 GetPivotPosition(bool right)
    {
        lineRef1.position = right ? p2 : p1;
        lineRef1.LookAt(right ? p1 : p2);
        lineRef1.localPosition += lineRef1.forward * distanceToTurn;
        lineRef1.forward = savedNormal;

        lineRef2.position = right ? p2 : p1;
        lineRef2.LookAt(search.cornerPoints[right ? i2 : i3].position);
        lineRef2.localPosition += lineRef2.forward * distanceToTurn;
        lineRef2.forward = search.cornerPoints[right ? i1 : i3].normal;

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
        Gizmos.DrawSphere(p1, .2f);
        Gizmos.DrawSphere(p2, .2f);

    }
}
