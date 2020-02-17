using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RaySearch : MonoBehaviour
{

    public float stepSize = 0.1f;
    public float offsetMargin = 0.01f;
    public int checkCountMax = 100;
    public List<MeshPoint> meshPoints = new List<MeshPoint>();
   // public List<Vector3> pathPoints = new List<Vector3>();

    List<Vector3[]> debugTangentCheck = new List<Vector3[]>();
    List<Vector3[]> debugNegativeCheck = new List<Vector3[]>();
    List<Vector3[]> debugBehindCheck = new List<Vector3[]>();


    void OnDrawGizmos()
    {
        Handles.color = Color.white;
        //Handles.DrawAAPolyLine(points.ToArray());
        DrawLinePairs(debugTangentCheck, Color.red);
        DrawLinePairs(debugNegativeCheck, Color.blue);
        DrawLinePairs(debugBehindCheck, Color.cyan);
    }

    void DrawLinePairs(List<Vector3[]> list, Color color)
    {
        Handles.color = color;
        foreach (Vector3[] pair in list)
        {
            Handles.DrawAAPolyLine(pair[0], pair[1]);
        }
    }

    void FindNext(Vector3 pt, Vector3 normal)
    {
        MeshPoint mp = new MeshPoint(); mp.position = pt; mp.normal = normal;
        meshPoints.Add(mp);
        Vector3 tangent = Vector3.Cross(normal, Vector3.up);
        Vector3 offsetPt = pt + normal * offsetMargin;
        Vector3 tangentCheckPoint = offsetPt + tangent * stepSize;
        Vector3 negativeCheckPoint = tangentCheckPoint - normal * (offsetMargin * 2);
        Vector3 behindCheckPoint = negativeCheckPoint - tangent * (stepSize * 0.75f);

        // find positive turn or flat surface
        bool foundThing = false;
        RaycastHit hit;

        // Check positive turn
        if (Physics.Raycast(offsetPt, tangent, out hit, stepSize))
        {
            debugTangentCheck.Add(new[] { offsetPt, hit.point });
            foundThing = true;
        }
        else
        {
            debugTangentCheck.Add(new[] { offsetPt, tangentCheckPoint });
            // check flat or slight negative turn
            if (Physics.Raycast(tangentCheckPoint, -normal, out hit, offsetMargin * 2))
            {
                debugNegativeCheck.Add(new[] { tangentCheckPoint, hit.point });
                foundThing = true;
            }
            else
            {
                debugNegativeCheck.Add(new[] { tangentCheckPoint, negativeCheckPoint });
                // check negative turn
                if (Physics.Raycast(negativeCheckPoint, -tangent, out hit, stepSize * 2))
                {
                    foundThing = true;
                    debugBehindCheck.Add(new[] { negativeCheckPoint, hit.point });
                }
                else
                {
                    debugBehindCheck.Add(new[] { negativeCheckPoint, behindCheckPoint });
                }
            }
        }

        if (foundThing && meshPoints.Count < checkCountMax)
            FindNext(hit.point, hit.normal);

    }

    [ContextMenu("Do points")]
    void DoPoints()
    {
        //pathPoints.Clear();
        meshPoints.Clear();
        debugTangentCheck.Clear();
        debugNegativeCheck.Clear();
        debugBehindCheck.Clear();
        if (Physics.Raycast(transform.position, Vector3.forward, out RaycastHit hit))
            FindNext(hit.point, hit.normal);
    }

}

[System.Serializable]
public struct MeshPoint
{
    public Vector3 position;
    public Vector3 normal;
}