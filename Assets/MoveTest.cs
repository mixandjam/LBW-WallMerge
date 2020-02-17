using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveTest : MonoBehaviour
{

    public RaySearch raySearch;
    public int index;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position,raySearch.meshPoints[index].position,.5f);
        transform.forward = Vector3.Lerp(transform.forward, raySearch.meshPoints[index].normal,.1f);

        if (Input.GetKey(KeyCode.RightArrow))
        {
            index++;
            if (index > raySearch.meshPoints.Count - 1)
                index = 0;
        }
    }
}
