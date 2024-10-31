using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowPoint : MonoBehaviour
{
    // this script should be attached to the parent of the camera
    // this object will follow the pivot point on the player, smoothed

    [SerializeField] public Transform pointToRotate; // for rotating the camera AROUND the player
    [SerializeField] public Transform pointToFollow; // for following the player
    Vector3 offset;

    // Start is called before the first frame update
    void Start()
    {
        offset = transform.position - pointToFollow.position;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //float h = 4 * Input.GetAxis("Mouse X");
        //pointToRotate.Rotate(0, h, 0);

        transform.position = Vector3.Lerp(transform.position, pointToFollow.position + offset, 0.25f);
        float rotX = Mathf.LerpAngle(transform.eulerAngles.x, pointToFollow.eulerAngles.x, 0.065f);
        float rotY = Mathf.LerpAngle(transform.eulerAngles.y, pointToFollow.eulerAngles.y, 0.065f);
        //transform.rotation = Quaternion.Euler(new Vector3(rotX, rotY, 0));
        Quaternion r = Quaternion.Euler(new Vector3(pointToFollow.eulerAngles.x, pointToFollow.eulerAngles.y, 0));
        transform.rotation = Quaternion.Slerp(transform.rotation, r, 0.065f);
    }

    void Update()
    {
        //float clampedY = Mathf.Clamp(pointToRotate.localEulerAngles.y, -10, 10);
        //pointToRotate.eulerAngles = new Vector3(pointToRotate.localEulerAngles.x, clampedY, pointToRotate.localEulerAngles.z);
    }

    public float DistFromTarget()
    {
        return Vector3.Distance(transform.position, pointToFollow.position + offset);
    }
}
