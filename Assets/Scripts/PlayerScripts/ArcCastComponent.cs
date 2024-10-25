using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcCastComponent : MonoBehaviour
{
    public float arcAngle = 180;

    public float arcAngularOffset = 90;

    public int arcResolution = 6;

    public bool ArcCast(Vector3 center, Quaternion rotation, float angle, float radius, int resolution, LayerMask layer, out RaycastHit hit)
    {
        //rotation *= Quaternion.Euler(-angle/2,0,0);
        rotation *= Quaternion.Euler(-angle + arcAngularOffset, 0, 0);

        for (int i = 0; i < resolution; i++)
        {

            Vector3 A = center + rotation * Vector3.forward * radius;
            rotation *= Quaternion.Euler(angle / resolution, 0, 0);
            Vector3 B = center + rotation * Vector3.forward * radius;
            Vector3 AB = B - A;

            Debug.DrawRay(A, AB);

            if (Physics.Raycast(A, AB, out hit, AB.magnitude * 1.001f, layer))
            {
                return true;
            }

        }


        hit = new RaycastHit();
        return false;
    }


}
