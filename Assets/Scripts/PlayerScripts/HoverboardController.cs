using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HoverboardController : MonoBehaviour
{
    [SerializeField] LayerMask floorRaycastLayers;

    [Header("Motion")]
    [SerializeField] float maxSpeed = 25f;
    [SerializeField] float accel = 2f;
    [SerializeField] float decel = 2f;
    [SerializeField] float breakDecelBase = 5f; // starting decel
    [SerializeField] float breakDecelRate = 2;
    float curBreakDecel;
    bool isBreaking;
    float breakTurnDir; // the direction we were turning when we started breaking

    [Header("Turning")]
    [SerializeField] float initTurnSpeed = 2f;
    [SerializeField] float initTurnAccel = 15f;
    [SerializeField] float maxTurnSpeed = 5f;
    [SerializeField] float turnAccel = 2f;
    [SerializeField] float turnDecel = 2f;
    [SerializeField] float boardTurnAmount = 6f;
    [SerializeField] float breakTurnDecel = 4f;
    // turning
    float turnVelocity;
    float totalTurnVelocity; // the current turn velocity + the initial turn speed, used for decelerating during breaking

    [Header("Boosting")]
    [SerializeField] int baseBoostAmount = 1; // the standard amount the player should always have on a cooldown
    [SerializeField] TMP_Text boostsText;
    [SerializeField] float boostCooldown = 3;
    int boostsAvailable = 1;
    int additionalBoosts;
    float boostTimer; // for cooldown

    [Header("Effects")]
    [SerializeField] ParticleSystem boostParticles;

    [Header("Spin Out")]
    [SerializeField] float spinOutMultiplier = 1.65f;
    [SerializeField] float maxSpinOutForce = 145;

    [Header("Jumping")]
    [SerializeField] float gravityValue = 6f;
    [SerializeField] float gravityMultiplier = 2f;
    [SerializeField] float groundedDist = 0.5f;
    [SerializeField] float jumpForce = 10f;
    bool isGrounded = true;
    float curGravity;
    // upwards velocity
    float jumpVelocity;

    [Header("Grinding")]
    [SerializeField] float grindDetectHeight = 3f;
    [SerializeField] float grindDetectWidth = 3f;
    [SerializeField] float grindAngleDiff = 30f;
    [SerializeField] float grindDecel = 15f;
    bool canGrind = false;
    GrindObject currentGrind;
    Collider[] grindCollidersHit;
    Vector3 grindDir;

    BoardGraphics board;

    Rigidbody rb;

    // forward velocity
    float baseVelocity;
    float additionalVelocity;
    float additionalAccel;
    float additionalTarget;
    bool additionalTargetReached;
    Vector3 finalCalculatedVelocity; // after the base, additional, and other forces (like spin out) have been put together

    float yaw;
    float pitch;

    [HideInInspector] public GameObject graphics;
    float boardRoll;
    float curBoardRoll;
    float boardYaw;
    float curBoardYaw;
    Vector3 finalCalculatedRotation;

    bool collisionActive = true;

    // spinning out
    bool isSpinningOut = false;
    float spinOutRecovery = 3f;
    Vector3 spinOutVector = Vector3.zero;
    float spinOutOffset = 1440;
    float curSpinOutOffset = 0;
    float spinOutRate = 0;
    int spinOutDir;

    enum MovementState { STANDARD, GRIND, WALLRIDE };
    MovementState movementState = MovementState.STANDARD;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        board = GetComponentInChildren<BoardGraphics>();
        graphics = transform.GetChild(0).gameObject;

        boostsAvailable = baseBoostAmount;
        boostsText.text = "x" + (boostsAvailable + additionalBoosts);
        boostTimer = boostCooldown;

        curBoardRoll = graphics.transform.localEulerAngles.z;
        curBoardYaw = graphics.transform.localEulerAngles.y;
    }

    // Update is called once per frame
    void Update()
    {
        BoostCooldown();

        switch (movementState)
        {
            case MovementState.STANDARD:
                ProcessStandardMovement();
                CheckForGrind();
                if (canGrind && Input.GetButtonDown("Grind"))
                {
                    // start grind
                    canGrind = false;
                    rb.angularVelocity = Vector3.zero;
                    rb.velocity = grindDir * ((baseVelocity + additionalVelocity) * 0.85f);
                    Vector3 closestPoint = currentGrind.GetComponent<Collider>().ClosestPoint(transform.position);
                    transform.position = new Vector3(closestPoint.x, closestPoint.y + 1.5f, closestPoint.z);
                    movementState = MovementState.GRIND;
                }
                break;
            case MovementState.GRIND:
                if (grindCollidersHit.Length <= 0)
                {
                    currentGrind = null;
                }
                ProcessGrindMovement();
                CheckForGrind();
                break;
        }
    }

    void ProcessStandardMovement()
    {
        #region jumping
        if (!isGrounded) 
        { 
            jumpVelocity -= curGravity * gravityMultiplier * Time.deltaTime;
            curGravity += gravityMultiplier * Time.deltaTime;
        }
        else
        {
            curGravity = gravityValue;
        }

        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.up, out hit, groundedDist + 1, floorRaycastLayers) && jumpVelocity <= 0)
        {
            // hit the ground
            isGrounded = true;
            transform.position = new Vector3(transform.position.x, hit.point.y + groundedDist, transform.position.z);
            jumpVelocity = 0;

            // adjust for surface rotation
            Quaternion targetRot = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            Quaternion slerpedRot = Quaternion.Slerp(transform.rotation, targetRot, 0.1f);
            rb.rotation = slerpedRot;

            // if mid flip when hitting ground
            if (board.State() == BoardGraphics.FlipState.FLIPPING && !board.CheckSuccess())
            {
                jumpVelocity = Mathf.Sqrt(jumpForce / 3 * gravityValue);
                baseVelocity *= 0.3f;
                additionalVelocity *= 0.3f;
            }
        }
        else
        {
            isGrounded = false;
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            jumpVelocity = Mathf.Sqrt(jumpForce * gravityValue);
        }
        #endregion

        #region flipping
        if (!isGrounded)
        {
            if (Mathf.Abs(Input.GetAxis("Flip")) > 0 || Mathf.Abs(Input.GetAxis("FlipMouse")) > 0)
            {
                board.SetFlipActive(true);
                if (Mathf.Abs(Input.GetAxis("Flip")) > 0) { board.SetValue(Input.GetAxis("Flip")); }
                else { board.SetValue(Input.GetAxis("FlipMouse")); }
                
            }
            else
            {
                board.SetFlipActive(false);
                board.SetValue(0);
            }
        }
        else
        {
            board.SetFlipActive(false);
            board.SetValue(0);
        }
        #endregion

        #region velocity and acceleration
        if (Input.GetAxis("Vertical") > 0 && !isBreaking) { baseVelocity += Input.GetAxis("Vertical") * accel * Time.deltaTime; }
        else { baseVelocity -= decel * Time.deltaTime; }


        if (Input.GetButtonDown("Boost") && boostsAvailable + additionalBoosts > 0)
        {
            if (boostsAvailable > 0)
            {
                boostsAvailable--;
            }
            else if (additionalBoosts > 0)
            {
                additionalBoosts--;
            }
            Boost(45, 50);
            boostsText.text = "x" + (boostsAvailable + additionalBoosts);
            boostParticles.Play();
        }

        if (additionalVelocity < additionalTarget && !isBreaking)
        {
            additionalVelocity += additionalAccel * Time.deltaTime;
        }
        else
        {
            // if target velocity reached
            additionalTargetReached = true;
            additionalTarget = 0;
        }

        if (additionalTargetReached)
        {
            additionalVelocity -= (decel) * Time.deltaTime;
        }

        if (isBreaking) 
        { 
            baseVelocity -= curBreakDecel * Time.deltaTime; 
            additionalVelocity -= curBreakDecel * Time.deltaTime;
            curBreakDecel += breakDecelRate * Time.deltaTime;
        }
        else
        {
            curBreakDecel = breakDecelBase;
        }

        baseVelocity = Mathf.Clamp(baseVelocity, 0, maxSpeed);
        additionalVelocity = Mathf.Max(0, additionalVelocity);

        // always push spin out force to zero
        spinOutVector = Vector3.Lerp(spinOutVector, Vector3.zero, spinOutRecovery * Time.deltaTime);

        finalCalculatedVelocity = (baseVelocity + additionalVelocity) * transform.forward + spinOutVector + (Vector3.up * jumpVelocity);

        rb.velocity = finalCalculatedVelocity;
        #endregion

        float verInput = Input.GetAxisRaw("Vertical");
        float horInput = Input.GetAxisRaw("Horizontal");

        #region breaking
        if (Input.GetAxis("Break") < 0 || Input.GetButton("Break"))
        {
            if (!isBreaking)
            {
                // initial break
                isBreaking = true;
                if (Mathf.Abs(rb.angularVelocity.y) > 0) { breakTurnDir = Mathf.Sign(rb.angularVelocity.y); } // which way we were turning 
                turnVelocity *= 0.8f;
            }
        }
        else
        {
            isBreaking = false;
        }

        if (isBreaking)
        {
            turnVelocity = Mathf.MoveTowards(turnVelocity, 0, breakTurnDecel * Time.deltaTime);
        }
        else if (Mathf.Abs(horInput) > 0 && !isBreaking)
        {
            if (turnVelocity < initTurnSpeed) { turnVelocity += initTurnAccel * Time.deltaTime; }
            else { turnVelocity += turnAccel * Time.deltaTime; }
        }
        else
        {
            turnVelocity = Mathf.MoveTowards(turnVelocity, 0, turnDecel * Time.deltaTime);
        }
        #endregion

        // MORESO DRIFTING THAN BREAKING
        //if (Mathf.Abs(horInput) > 0)
        //{
        //    if (turnVelocity < initTurnSpeed) { turnVelocity += initTurnAccel * Time.deltaTime; }
        //    else { turnVelocity += turnAccel * Time.deltaTime; }
        //}
        //if (isBreaking) { turnVelocity -= breakTurnDecel * Time.deltaTime; }
        turnVelocity = Mathf.Clamp(turnVelocity, 0, maxTurnSpeed);

        if (isBreaking) { yaw = turnVelocity * breakTurnDir; }
        else { yaw = turnVelocity * horInput; }

        //if (Input.GetMouseButton(0)) { pitch -= 80 * Time.deltaTime; }
        //else if (Input.GetMouseButton(1)) { pitch += 80 * Time.deltaTime; }
        //else { pitch = Mathf.MoveTowards(pitch, 0, 60 * Time.deltaTime); }
        //pitch = Mathf.Clamp(pitch, -pitchLimit, pitchLimit);

        rb.angularVelocity = yaw * transform.up;
        //rb.rotation = Quaternion.Euler(new Vector3(pitch, rb.rotation.eulerAngles.y, 0));

        if (isBreaking)
        {
            boardRoll = (baseVelocity + additionalVelocity) * -breakTurnDir;
            boardYaw = breakTurnDir * 70;
        }
        else
        {
            boardRoll = -rb.angularVelocity.y * boardTurnAmount;
            boardYaw = 0;
        }

        boardRoll = Mathf.Clamp(boardRoll, -50, 50);
        boardYaw = Mathf.Clamp(boardYaw, -70, 70);

        curBoardRoll = Mathf.LerpAngle(curBoardRoll, boardRoll, 8 * Time.deltaTime);
        // TODO this needs to become independent of transform's angles, as it messes with the spin out
        curBoardYaw = Mathf.LerpAngle(curBoardYaw, boardYaw, 8 * Time.deltaTime);

        if (isSpinningOut)
        {
            if (Mathf.Abs(curSpinOutOffset) < spinOutOffset)
            {
                // spin out rate decreases over time to emulate natural force
                spinOutRate -= 1800 * Time.deltaTime;
                spinOutRate = Mathf.Max(spinOutRate, 360);
                curSpinOutOffset += spinOutRate * spinOutDir * Time.deltaTime;

                if (spinOutRate <= 0) { isSpinningOut = false; } // safe guard for if the rate gets to 0 before reaching target spins
            }
            else
            {
                isSpinningOut = false;
            }
        }
        else
        {
            curSpinOutOffset = Mathf.MoveTowardsAngle(curSpinOutOffset, 0, 60 * Time.deltaTime);
        }

        finalCalculatedRotation = new Vector3(0, curBoardYaw + curSpinOutOffset, curBoardRoll);

        graphics.transform.localEulerAngles = finalCalculatedRotation;
    }

    void ProcessGrindMovement()
    {
        rb.velocity = grindDir * (baseVelocity + additionalVelocity);
        baseVelocity -= grindDecel * Time.deltaTime; additionalVelocity -= grindDecel * Time.deltaTime;
        baseVelocity = Mathf.Clamp(baseVelocity, 0, maxSpeed);
        additionalVelocity = Mathf.Max(0, additionalVelocity);

        // if grind to halt, stop grind
        if (baseVelocity + additionalVelocity <= 0)
        {
            movementState = MovementState.STANDARD;
            currentGrind.HighlightColor(false);
            currentGrind = null;
            baseVelocity = maxSpeed / 4; // a little push forward
            jumpVelocity = Mathf.Sqrt(jumpForce / 2 * gravityValue);
        }

        // if let grind button go or jump, stop grind
        if (Input.GetButtonUp("Grind") || Input.GetButtonDown("Jump"))
        {
            movementState = MovementState.STANDARD;
            currentGrind.HighlightColor(false);
            currentGrind = null;
            jumpVelocity = Mathf.Sqrt(jumpForce / 2 * gravityValue);
        }
    }

    public void Boost(float targetVelocity, float accelRate)
    {
        additionalTarget = targetVelocity;
        additionalAccel = accelRate;
        additionalTargetReached = false;
        ScreenShake.instance.ShakeScreen(0.025f, 1f);
    }

    public void AddBoosts(int num)
    {
        additionalBoosts += num;
        boostsText.text = "x" + (boostsAvailable + additionalBoosts);
    }

    public void BoostCooldown()
    {
        if (boostsAvailable < baseBoostAmount)
        {
            if (boostTimer > 0)
            {
                boostTimer -= Time.deltaTime;
            }
            else
            {
                boostsAvailable++;
                boostsText.text = "x" + (boostsAvailable + additionalBoosts);
                boostTimer = boostCooldown;
            }
        }
    }

    // total forward velocity
    public Vector3 ForwardVelocity()
    {
        return (baseVelocity + additionalVelocity) * transform.forward + spinOutVector;
    }

    // are we currently spinning out?
    public bool IsSpinningOut()
    {
        return isSpinningOut;
    }

    public void SetCollisionActive(bool active)
    {
        collisionActive = active;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!collisionActive || baseVelocity + additionalVelocity == 0) { return; }
        if (other.gameObject.CompareTag("Wall"))
        {
            Vector3 dir = (transform.position - other.transform.position).normalized;
            dir.y = 0;
            dir += -transform.forward; // give a little extra push in the direct opposite forward velocity
            isSpinningOut = true;
            spinOutVector = dir * Mathf.Min(maxSpinOutForce, ((baseVelocity + additionalVelocity) * spinOutMultiplier));

            // spins the player graphics
            spinOutDir = Mathf.RoundToInt(Mathf.Sign(rb.angularVelocity.y));
            spinOutRate = 1440;
            curSpinOutOffset = spinOutRate / 2 * spinOutDir; // inital 'force'
        }
    }

    public bool CheckForGrind()
    {
        grindCollidersHit = Physics.OverlapBox(transform.position - new Vector3(0, grindDetectHeight / 4), new Vector3(grindDetectWidth, grindDetectHeight / 2, 5), transform.rotation, LayerMask.GetMask("Grind"));
        foreach (Collider coll in grindCollidersHit)
        {
            if (coll.TryGetComponent(out currentGrind))
            {
                foreach (Vector3 dir in currentGrind.grindDirections)
                {
                    float angleDiff = Vector3.Angle(dir, transform.forward);
                    if (angleDiff < grindAngleDiff)
                    {
                        canGrind = true;
                        grindDir = dir;
                        currentGrind.HighlightColor(true);
                    }
                }
                return true;
            }
        }
        canGrind = false;
        if (currentGrind != null) { currentGrind.HighlightColor(false); movementState = MovementState.STANDARD; }
        currentGrind = null;
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Debug.DrawRay(transform.position, -transform.up * groundedDist, Color.red);
        Gizmos.DrawWireCube(transform.position - new Vector3(0, grindDetectHeight / 4), new Vector3(grindDetectWidth, grindDetectHeight / 2, 5));
    }
}
