using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTest : MonoBehaviour
{
    public bool active;
    public float lerpAmount;
    public Vector3 p1;
    public Vector3 p2;
    public RaySearch search;

    public void SetPosition(Vector3 c1, Vector3 c2, float lerp, RaySearch ray)
    {
        transform.position = Vector3.Lerp(c1, c2, lerp);
        search = ray;
        p1 = c1;
        p2 = c2;
        lerpAmount = lerp;
        active = true;
    }

    private void Update()
    {
        float x = Input.GetAxis("Horizontal");
        if (active)
        {
            transform.position = Vector3.Lerp(p1, p2, lerpAmount);
            lerpAmount = Mathf.Clamp(lerpAmount + x * Time.deltaTime * 5, 0, 1);

            if(lerpAmount >= 1)
            {
                print("stop");
            }
            else if(lerpAmount <= 0)
            {
                print("stop");
            }
        }
    }
}
