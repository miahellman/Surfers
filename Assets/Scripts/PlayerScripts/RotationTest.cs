using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class RotationTest : MonoBehaviour
{
    public float moveSpeed = 5f;



    public Rigidbody myRB;

    public LayerMask floorRaycastLayers;


    public float floorRaycastLength = 2;

    public float floorHoverHeight = 1.1f;


    public Vector3 angularOffset = Vector3.zero;


    // Start is called before the first frame update
    void Start()
    {
        myRB = GetComponent<Rigidbody>();


    }

    // Update is called once per frame
    void Update()
    {





    }


    private void FixedUpdate()
    {
        Vector3 moveDir = Vector3.zero;







        RaycastHit hit;
        //Now we raycast to the ground!
        //Raycasts return the surface normal of what we hit

        if (Physics.Raycast(myRB.position, -transform.up, out hit, floorRaycastLength, floorRaycastLayers))
        {


            Debug.Log("We're touching a floor!");

            //Now we need to get the surface normal.

            Vector3 surfaceNorm = hit.normal;

            Quaternion targetRot = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;

            Quaternion slerpedRot = Quaternion.Slerp(transform.rotation, targetRot, 0.1f);



            Vector3 difference = (transform.position - hit.point);


            transform.position = hit.point + difference.normalized * floorHoverHeight;

            myRB.rotation = slerpedRot;







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
