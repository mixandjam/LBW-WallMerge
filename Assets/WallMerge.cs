using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using DG.Tweening;
using Cinemachine;

public class WallMerge : MonoBehaviour
{
    private Animator playerAnimator;
    private MovementInput playerMovement;
    private Vector3 closestCorner;
    private Vector3 nextCorner;
    private Vector3 previousCorner;
    private Vector3 chosenCorner;

    [Header("Parameters")]
    public float transitionTime = .8f;

    [Space]

    [Header("Public References")]
    public ProjectorMovement decalMovement;

    [Space]

    [Header("Cameras")]
    public GameObject gameCam;
    public GameObject wallCam;

    [Space]

    [Header("Post Processing")]
    public Volume dofVolume;
    public Volume zoomVolume;
    CinemachineBrain brain;

    private void Start()
    {
        playerAnimator = GetComponent<Animator>();
        playerMovement = GetComponent<MovementInput>();
        brain = Camera.main.GetComponent<CinemachineBrain>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Physics.Raycast(transform.position + (Vector3.up * .1f), transform.forward, out RaycastHit hit, 1))
            {
                if(hit.transform.GetComponentInChildren<RaySearch>() != null)
                {
                    //store raycasted object's RaySearch component
                    RaySearch search = hit.transform.GetComponentInChildren<RaySearch>();

                    //create a new list of all the corner positions
                    List<Vector3> cornerPoints = new List<Vector3>();

                    for (int i = 0; i < search.cornerPoints.Count; i++)
                        cornerPoints.Add(search.cornerPoints[i].position);

                    //find the closest corner position and index
                    closestCorner = GetClosestPoint(cornerPoints.ToArray(), hit.point);
                    int index = search.cornerPoints.FindIndex(x => x.position == closestCorner);

                    //determine the adjacent corners
                    nextCorner = (index < search.cornerPoints.Count - 1) ? search.cornerPoints[index + 1].position : search.cornerPoints[0].position;
                    previousCorner = (index > 0) ? search.cornerPoints[index - 1].position : search.cornerPoints[search.cornerPoints.Count - 1].position;

                    //choose a corner to be the target
                    chosenCorner = Vector3.Dot((closestCorner - hit.point), (nextCorner - hit.point)) > 0 ? previousCorner : nextCorner;
                    bool nextCornerIsRight = isRightSide(-hit.normal, chosenCorner - closestCorner, Vector3.up);

                    //find the distance from the origin point
                    float distance = Vector3.Distance(closestCorner, chosenCorner);
                    float playerDis = Vector3.Distance(chosenCorner, hit.point);

                    //quick fix so that we don't allow the player to start in a corner;
                    if (playerDis > (distance - decalMovement.distanceToTurn))
                        playerDis = distance - decalMovement.distanceToTurn;
                    if(playerDis < decalMovement.distanceToTurn)
                        playerDis = decalMovement.distanceToTurn;

                    //find it's normalized position in the distance of the origin and target
                    float positionLerp = Mathf.Abs(distance - playerDis) / ((distance + playerDis) / 2);

                    //start the MovementScript
                    decalMovement.SetPosition(closestCorner, chosenCorner, positionLerp, search, nextCornerIsRight, hit.normal);

                    //transition logic
                    Transition(true, Vector3.Lerp(closestCorner, chosenCorner, positionLerp), hit.normal);
                }
            }
        }
    }

    public void Transition(bool state, Vector3 point, Vector3 normal)
    {

        transform.forward = -normal;

        playerMovement.enabled = false;
        playerAnimator.SetTrigger("turn");

        Sequence s = DOTween.Sequence();
        s.AppendInterval(.2f);
        s.AppendCallback(() => gameCam.SetActive(!state));
        s.AppendCallback(() => wallCam.SetActive(state));
        s.Append(transform.GetChild(1).DOMove(point - new Vector3(0, .9f, 0), .7f).SetEase(Ease.InBack));
        s.Join(transform.GetChild(1).DOScaleZ(.01f, .7f).SetDelay(.2f));
        s.AppendCallback(() => decalMovement.gameObject.SetActive(state));
        s.AppendCallback(() => decalMovement.GetComponentInChildren<ParticleSystem>().Play());
        //decalMovement.gameObject.SetActive(state);
        //wallCam.SetActive(state);
        //gameCam.SetActive(!state);
        //gameObject.SetActive(!state);

        //float transition = state ? transitionTime : .2f;
        //brain.m_DefaultBlend.m_Time = state ? transitionTime : .2f;

        //Field of View
        //float dof = state ? 1 : 0;
        //DOVirtual.Float(dofVolume.weight, dof, transition, DofPostVolume);
        //if(state)
        //DOVirtual.Float(zoomVolume.weight, 1, .25f, ZoomVolume).OnComplete(()=> DOVirtual.Float(zoomVolume.weight, 0, .4f, ZoomVolume).SetDelay(.1f));
    }

    Vector3 GetClosestPoint(Vector3[] points, Vector3 currentPoint)
    {
        Vector3 pMin = Vector3.zero;
        float minDist = Mathf.Infinity;

        foreach (Vector3 p in points)
        {
            float dist = Vector3.Distance(p, currentPoint);
            if (dist < minDist)
            {
                pMin = p;
                minDist = dist;
            }
        }
        return pMin;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawRay(transform.position + (Vector3.up*.1f), transform.forward);
        Gizmos.DrawSphere(closestCorner, .2f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(previousCorner, .2f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(nextCorner, .2f);

    }

    //https://forum.unity.com/threads/left-right-test-function.31420/
    public bool isRightSide(Vector3 fwd, Vector3 targetDir, Vector3 up)
    {
        Vector3 right = Vector3.Cross(up.normalized, fwd.normalized);        // right vector
        float dir = Vector3.Dot(right, targetDir.normalized);
        return dir > 0f;
    }

    public void DofPostVolume(float x)
    {
        dofVolume.weight = x;
    }

    public void ZoomVolume(float x)
    {
        zoomVolume.weight = x;
    }

}
