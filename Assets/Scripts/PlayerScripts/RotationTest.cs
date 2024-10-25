using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.GraphicsBuffer;

public class RotationTest : MonoBehaviour
{
    public float moveSpeed = 5f;



    public Rigidbody myRB;

    public LayerMask floorRaycastLayers;


    public float floorRaycastLength = 2;

    public float floorHoverHeight = 1.1f;


    public float dotTolerance = 0.5f;

    public float arcAngle = 180;

    public float arcAngularOffset = 90;

    public int arcResolution = 6;





    // Start is called before the first frame update
    void Start()
    {
        myRB = GetComponent<Rigidbody>();


    }

    // Update is called once per frame
    void Update()
    {





    }

    bool ArcCast(Vector3 center, Quaternion rotation, float angle, float radius, int resolution, LayerMask layer, out RaycastHit hit)
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



    private void FixedUpdate()
    {
        Vector3 moveDir = Vector3.zero;







        RaycastHit hit;
        //Now we raycast to the ground!
        //Raycasts return the surface normal of what we hit

        //if (Physics.Raycast(myRB.position, -transform.up, out hit, floorRaycastLength, floorRaycastLayers))
        if (ArcCast(myRB.position, transform.rotation, arcAngle, floorRaycastLength, arcResolution, floorRaycastLayers, out hit))
        {

            float dotProduct = Vector3.Dot(transform.up, hit.normal);

            if (dotProduct > dotTolerance)
            {
                Debug.Log("We're touching a floor!");

                //Now we need to get the surface normal.

                Vector3 surfaceNorm = hit.normal;

                Quaternion targetRot = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;

                Quaternion slerpedRot = Quaternion.Slerp(transform.rotation, targetRot, 0.1f);



                Vector3 difference = (transform.position - hit.point);


                //transform.position = hit.point + difference.normalized * floorHoverHeight;

                myRB.rotation = slerpedRot;



            }











        }
        else
        {
            Debug.Log("0");


        }


        moveDir = (transform.forward * Input.GetAxisRaw("Vertical") + transform.right * Input.GetAxisRaw("Horizontal")).normalized;

        Vector3 newPos = transform.position + moveDir * moveSpeed * Time.fixedDeltaTime;

        transform.position = newPos;



    }

}
