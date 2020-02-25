using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTest : MonoBehaviour
{
    public bool active;
    public bool rotation;

    public float movementLerp;
    public float rotationLerp;

    public Vector3 p1;
    public Vector3 p2;
    public int i1, i2;
    public RaySearch search;
    public Transform pivot;
    private Vector3 savedNormal;

    private float distanceToTurn = .85f;

    public void SetPosition(Vector3 c1, Vector3 c2, float lerp, RaySearch ray)
    {
        transform.position = Vector3.Lerp(c1, c2, lerp);
        search = ray;
        p1 = c1;
        p2 = c2;
        movementLerp = lerp;
        active = true;
    }

    private void Update()
    {
        float axis = Input.GetAxis("Horizontal");
        if (active)
        {
            transform.position = Vector3.Lerp(transform.position,Vector3.Lerp(p1, p2, movementLerp), .8f);
            movementLerp = Mathf.Clamp(movementLerp + axis * Time.deltaTime * 2, 0, 1);

            if(movementLerp >= distanceToTurn)
            {
                active = false;
                print("stopR");
                i1 = search.cornerPoints.FindIndex(x => x.position == p2);
                i2 = (i1 == search.cornerPoints.Count - 1) ? 0 : i1 + 1;
                rotation = true;
                //position the pivot
                pivot.forward = transform.forward;
                pivot.position = transform.position;
                pivot.localPosition += new Vector3(0, 0, Vector3.Dot(transform.forward, (search.cornerPoints[i2].normal)));
                //make pivot parent
                transform.parent = pivot;
                //set rotation lerp to 0
                movementLerp = 0;
                rotationLerp = 0;
                //save normal
                savedNormal = transform.forward;

                print(Vector3.Angle((search.cornerPoints[i1].normal), (search.cornerPoints[i2].normal)));

            }
            else if(movementLerp <= 0)
            {
                print("stopL");
            }
        }

        if (rotation)
        {
            rotationLerp = Mathf.Clamp(rotationLerp + axis * Time.deltaTime * 5, 0, 1);
            pivot.forward = Vector3.Lerp(savedNormal, search.cornerPoints[i1].normal, rotationLerp);

            if (rotationLerp >= 1)
            {
                movementLerp = 1 - distanceToTurn;
                transform.parent = null;
                p1 = search.cornerPoints[i1].position;
                p2 = search.cornerPoints[i2].position;
                rotation = false;
                active = true;
            }
        }
    }
}
