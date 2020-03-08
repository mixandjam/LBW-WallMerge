using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProjectorMovement : MonoBehaviour
{
    private Animator anim;

    [Header("Movement Parameters")]
    public float movSpeed = 3;
    public float rotSpeed = 2;
    public float rotationLerp;
    public float distanceToTurn = 1f;

    [Space]

    [Header("Booleans")]
    public bool isActive;
    public bool movementMode;
    public bool rotationMode;
    public bool isGoingRight;
    public bool isMoving;
    private bool activation;

    private Vector3 originPos;
    private Vector3 targetPos;
    private Vector3 debug;

    private int currentIndex, previousIndex, nextIndex;

    [Space]

    [Header("Public References")]
    public WallMerge player;
    private RaySearch search;
    public Transform pivot;
    public Transform lineRef1, lineRef2;
    public ParticleSystem mergeParticle;
    public ParticleSystem exitParticle;

    private Vector3 savedNormal;

    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
    }


    public void SetPosition(Vector3 orig, Vector3 target, float lerp, RaySearch ray, bool nextCornerIsRight, Vector3 normal)
    {
        transform.forward = normal;
        transform.position = Vector3.Lerp(orig, target, lerp);
        search = ray;
        originPos = nextCornerIsRight ? orig : target;
        targetPos = nextCornerIsRight ? target : orig;
        movementMode = true;
    }

    void Debug()
    {
        if (Input.GetKeyDown(KeyCode.R))
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
    }

    private void Update()
    {
        Debug();

        if (Input.GetKeyDown(KeyCode.Space) && isActive)
        {
            transform.parent = null;
            isActive = false;
            movementMode = false;
            rotationMode = false;
            activation = false;
            anim.speed = 1;
            anim.SetFloat("axis", 0);
            anim.SetTrigger("reset");

            player.transform.position = new Vector3(transform.position.x, player.transform.position.y, transform.position.z) - (transform.forward * .5f);

            Vector3 playerFinalPos = player.transform.position + (transform.forward);

            player.Transition(false,playerFinalPos, transform.forward);
        }

        float axis = Input.GetAxis("Horizontal");

        if (movementMode && !rotationMode && isActive)
        {
            //move player between the two corner points
            transform.position = Vector3.MoveTowards(transform.position, axis > 0 ? targetPos : originPos, Mathf.Abs(axis) * Time.deltaTime * movSpeed);

            if (axis != 0 && !activation)
                activation = true;


            if (Vector3.Distance(transform.position, originPos) > (Vector3.Distance(originPos, targetPos) - distanceToTurn) || Vector3.Distance(transform.position, originPos) < distanceToTurn)
            {
                StartRotation(axis > 0);
            }

        }

        if(rotationMode && !movementMode && isActive)
        {
            CornerRoration(axis);
        }

        if (!activation)
            return;

        anim.SetFloat("axis", Input.GetAxisRaw("Horizontal"));

        if (Input.GetAxis("Horizontal") == 0)
            anim.speed = 0;
        else
            anim.speed = 1;
    }

    public void StartRotation(bool right)
    {
        isGoingRight = right;
        movementMode = false;
        savedNormal = transform.forward;

        currentIndex = search.cornerPoints.FindIndex(x => x.position == (right ? targetPos : originPos));


        pivot.position = GetPivotPosition(currentIndex, right);
        pivot.forward = transform.forward;
        transform.parent = pivot;

        rotationLerp = .01f;

        rotationMode = true;
    }

    public void CornerRoration(float axis)
    {

        float n = isGoingRight ? 1 : -1;

        Vector3 normal = isGoingRight ? search.cornerPoints[currentIndex].normal : search.cornerPoints[previousIndex].normal;

        rotationLerp = Mathf.Clamp(rotationLerp + ((axis * n)* Time.deltaTime * rotSpeed), 0, 1);
        pivot.forward = Vector3.Lerp(savedNormal, normal, rotationLerp);

        if (rotationLerp >= 1 || rotationLerp <= 0)
        {
            rotationMode = false;
            bool complete = (rotationLerp >= 1) ? true : false;

            if (isGoingRight)
            {
                originPos = complete ? search.cornerPoints[currentIndex].position : originPos;
                targetPos = complete ? search.cornerPoints[nextIndex].position : targetPos;
            }
            else
            {
                originPos = complete ? search.cornerPoints[previousIndex].position : originPos;
                targetPos = complete ? search.cornerPoints[currentIndex].position : targetPos;
            }

            transform.parent = null;
            movementMode = true;
            rotationLerp = .01f;
        }
    }

    public Vector3 GetPivotPosition(int currentIndex, bool right)
    {
        Vector3 pos = search.cornerPoints[currentIndex].position;

        lineRef1.position = pos;

        if (currentIndex - 1 > -1)
            previousIndex = currentIndex - 1;
        else
            previousIndex = search.cornerPoints.Count - 1;

        if (currentIndex + 1 < search.cornerPoints.Count)
            nextIndex = currentIndex + 1;
        else
            nextIndex = 0;

        bool origin = Vector3.Distance(transform.position, originPos) < Vector3.Distance(transform.position, targetPos);

        lineRef1.position = pos;
        lineRef1.LookAt(right ? search.cornerPoints[nextIndex].position : search.cornerPoints[previousIndex].position);
        lineRef1.localPosition += lineRef1.forward * distanceToTurn;
        lineRef1.forward = origin ? search.cornerPoints[previousIndex].normal : search.cornerPoints[currentIndex].normal;

        lineRef2.position = pos;
        lineRef2.LookAt(right ? search.cornerPoints[previousIndex].position : search.cornerPoints[nextIndex].position);
        lineRef2.localPosition += lineRef2.forward * distanceToTurn;
        lineRef2.forward = savedNormal;

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
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(originPos, .1f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(targetPos, .1f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(lineRef1.position, .1f);
        Gizmos.DrawRay(lineRef1.position, lineRef1.forward * 3);
        Gizmos.DrawRay(lineRef1.position, -lineRef1.forward * 3);
        Gizmos.DrawSphere(lineRef2.position, .1f);
        Gizmos.DrawRay(lineRef2.position, lineRef2.forward * 3);
        Gizmos.DrawRay(lineRef2.position, -lineRef2.forward * 3);
        Gizmos.color = Color.green;
        Vector3 inter;
        LineLineIntersection(out inter, lineRef1.position, lineRef1.forward, lineRef2.position, lineRef2.forward);
        Gizmos.DrawSphere(inter, .1f);
    }
}
