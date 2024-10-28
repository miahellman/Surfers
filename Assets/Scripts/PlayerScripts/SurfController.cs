using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Tricks;

public class SurfController : MonoBehaviour
{
    [SerializeField] LayerMask floorRaycastLayers;
    [SerializeField] float dotTolerance = 0.5f;

    [Header("Motion")]
    [SerializeField] float maxSpeed = 65f;
    [SerializeField] float accel = 35f;
    [SerializeField] float decel = 12f;
    [SerializeField] float breakDecelBase = 5f; // starting decel
    [SerializeField] float breakDecelRate = 9.35f;
    [SerializeField] float floorRotSpeed = 30;
    float curBreakDecel;
    bool isBreaking;
    float breakTurnDir; // the direction we were turning when we started breaking

    [Header("Turning")]
    [SerializeField] float initTurnSpeed = 1.75f;
    [SerializeField] float initTurnAccel = 14f;
    [SerializeField] float maxTurnSpeed = 3f;
    [SerializeField] float turnAccel = 0.55f;
    [SerializeField] float turnDecel = 4f;
    [SerializeField] float boardTurnAmount = 10f;
    [SerializeField] float breakTurnDecel = 1.3f;
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
    [SerializeField] float gravityValue = 13f;
    [SerializeField] float gravityMultiplier = 3.2f;
    [SerializeField] float groundedDist = 1.2f;
    [SerializeField] float hoverHeight = 1.2f;
    [SerializeField] float jumpForce = 100f;
    ArcCastComponent arcCast;
    bool isGrounded = true;
    float curGravity;
    // upwards velocity
    float jumpVelocity;
    

    [Header("Grinding")]
    [SerializeField] Vector3 grindDetectBox = new Vector3(5.3f, 3f, 10f);
    [SerializeField] float grindAngleDiff = 60f;
    [SerializeField] float grindDecel = 20f;
    [SerializeField] float inputWindow = 0.25f;
    Coroutine grindInputCo;
    bool grindInputActive; // for input window
    bool canGrind = false;
    GrindObject currentGrind;
    Collider[] grindCollidersHit;
    Vector3 grindDir;

    [Header("Collision")]
    [SerializeField] CollisionBox characterBox;
    [SerializeField] CollisionBox frontBox;
    [SerializeField] CollisionBox boardBox;
    [SerializeField] float frontCheckDist = 3;
    // for checking for a collider that can't be scaled in front of us
    Collider validCollider;
    Vector3 lastPosition; // for checking collision
    bool[] colliding = { false, false, false };

    // wallride
    [Header("Wallriding")]
    [SerializeField] float wallRideDist = 2f;
    [SerializeField] float wallDismountForce = 75f;
    RaycastHit curWallRide;
    bool wallLeft;
    bool wallRight;
    bool wallRideInputActive = false;
    RaycastHit leftWallHit;
    RaycastHit rightWallHit;
    Quaternion startRot;
    Coroutine wallrideInputCo;
    Vector3 wallrideDir;

    BoardGraphics board;
    Rigidbody rb;

    // forward velocity
    float baseVelocity;
    float additionalVelocity;
    float additionalAccel;
    float additionalTarget;
    bool additionalTargetReached;
    Vector3 additionalForce; // any additional force
    Vector3 finalCalculatedVelocity; // after the base, additional, and other forces (like spin out) have been put together

    float yaw;

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
        arcCast = GetComponent<ArcCastComponent>();
        graphics = transform.GetChild(0).gameObject;

        //boostsAvailable = baseBoostAmount;
        //boostsText.text = "x" + (boostsAvailable + additionalBoosts);
        //boostTimer = boostCooldown;

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
                CheckCollision();

                if (CheckForWall())
                {
                    // TODO also do input window
                    if (wallRideInputActive)
                    {
                        // start wallride

                        Vector3 attachPoint;
                        // TODO if wall on left and right, need to choose which one is closer, OR just dont design level like that...
                        if (wallLeft) 
                        { 
                            curWallRide = leftWallHit;
                            //Vector3 attachPoint = leftWallHit.point + leftWallHit.normal * 1;
                            //transform.position = attachPoint;
                            //wallrideDir = Quaternion.AngleAxis(-90, Vector3.up) * leftWallHit.normal;
                            attachPoint = curWallRide.collider.ClosestPointOnBounds(transform.position) + transform.right;
                        }
                        else
                        { 
                            curWallRide = rightWallHit;
                            //Vector3 attachPoint = rightWallHit.point + rightWallHit.normal * 1;
                            //transform.position = attachPoint;
                            //wallrideDir = Quaternion.AngleAxis(90, Vector3.up) * rightWallHit.normal;
                            attachPoint = curWallRide.collider.ClosestPointOnBounds(transform.position) + -transform.right;
                        }

                        //rb.rotation = Quaternion.FromToRotation(transform.up, curWallRide.normal) * transform.rotation;
                        //transform.position = new Vector3(transform.position.x, curWallRide.point.y + hoverHeight, transform.position.z);
                        transform.position = attachPoint;
                        graphics.transform.localEulerAngles = Vector3.zero;
                        startRot = transform.rotation;
                        rb.angularVelocity = Vector3.zero;

                        ScoreManager.instance.StartTrick(TrickManager.Wallride);

                        movementState = MovementState.WALLRIDE;
                    }
                }

                if (Input.GetButtonDown("Wallride"))
                {
                    wallrideInputCo = StartCoroutine(WallrideInputWindow());
                }
                if (Input.GetButtonUp("Wallride"))
                {
                    StopCoroutine(wallrideInputCo);
                    wallRideInputActive = false;
                }

                if (Input.GetButtonDown("Grind"))
                {
                    // start input window timer
                    grindInputCo = StartCoroutine(GrindInputWindow());
                }
                if (Input.GetButtonUp("Grind"))
                {
                    StopCoroutine(grindInputCo);
                    grindInputActive = false;
                }

                if (canGrind && grindInputActive)
                {
                    // start grind
                    canGrind = false;
                    rb.angularVelocity = Vector3.zero;
                    rb.velocity = grindDir * ((baseVelocity + additionalVelocity) * 0.85f);
                    Vector3 closestPoint = currentGrind.GetComponent<Collider>().ClosestPoint(transform.position);
                    transform.position = new Vector3(closestPoint.x, closestPoint.y + 1.5f, closestPoint.z);
                    movementState = MovementState.GRIND;
                    ScoreManager.instance.StartTrick(TrickManager.Grind);
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
            case MovementState.WALLRIDE:
                ProcessWallrideMovement();
                break;
        }
    }

    IEnumerator GrindInputWindow()
    {
        grindInputActive = true;
        yield return new WaitForSeconds(inputWindow);
        grindInputActive = false;
    }

    IEnumerator WallrideInputWindow()
    {
        wallRideInputActive = true;
        yield return new WaitForSeconds(inputWindow);
        wallRideInputActive = false;
    }

    void ProcessStandardMovement()
    {
        #region jumping and grounded
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
        #region arccast
        //if (arcCast.ArcCast(transform.position, transform.rotation, arcCast.arcAngle, groundedDist, arcCast.arcResolution, floorRaycastLayers, out hit) && jumpVelocity <= 0)
        //{
        //    float dotProduct = Vector3.Dot(transform.up, hit.normal);

        //    //And then this is where we just filter out any surfaces that are too steep.
        //    if (dotProduct > dotTolerance)
        //    {
        //        RaycastHit straightDownHit;
        //        if (Physics.Raycast(transform.position, -transform.up, out straightDownHit, groundedDist + 10, floorRaycastLayers) && jumpVelocity <= 0)
        //        {
        //            transform.position = new Vector3(transform.position.x, straightDownHit.point.y + hoverHeight, transform.position.z);
        //        }

        //        // hit the ground
        //        isGrounded = true;
        //        jumpVelocity = 0;

        //        // adjust for surface rotation
        //        Quaternion targetRot = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
        //        Quaternion slerpedRot = Quaternion.Slerp(transform.rotation, targetRot, floorRotSpeed * Time.deltaTime);
        //        rb.rotation = slerpedRot;
        //    }

        //    // if mid flip when hitting ground
        //    if (board.State() == BoardGraphics.FlipState.FLIPPING && !board.CheckSuccess())
        //    {
        //        jumpVelocity = Mathf.Sqrt(jumpForce / 3 * gravityValue);
        //        baseVelocity *= 0.3f;
        //        additionalVelocity *= 0.3f;
        //    }

        //}
        #endregion
        if (Physics.Raycast(transform.position, -transform.up, out hit, groundedDist, floorRaycastLayers) && jumpVelocity <= 0)
        {
            float dotProduct = Vector3.Dot(transform.up, hit.normal);

            RaycastHit secondHit;
            if (Physics.Raycast(rb.position + transform.up, -transform.up, out secondHit, groundedDist + 1, floorRaycastLayers))
            {
                if (dotProduct > dotTolerance)
                {
                    Vector3 difference = (rb.position - secondHit.point);


                    rb.position = secondHit.point + difference.normalized * hoverHeight;
                    isGrounded = true;
                    jumpVelocity = 0;

                    // adjust for surface rotation
                    Quaternion targetRot = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
                    Quaternion slerpedRot = Quaternion.Slerp(transform.rotation, targetRot, floorRotSpeed * Time.deltaTime);
                    rb.rotation = slerpedRot;

                    // if mid flip when hitting ground
                    if (board.State() == BoardGraphics.FlipState.FLIPPING && !board.CheckSuccess())
                    {
                        jumpVelocity = Mathf.Sqrt(jumpForce / 3 * gravityValue);
                        baseVelocity *= 0.3f;
                        additionalVelocity *= 0.3f;
                    }

                }
            }
            //    //And then this is where we just filter out any surfaces that are too steep.
            //if (dotProduct > dotTolerance)
            //{
            //    // hit the ground
            //    transform.position = new Vector3(transform.position.x, hit.point.y + hoverHeight, transform.position.z);
            //    isGrounded = true;
            //    jumpVelocity = 0;

            //    // adjust for surface rotation
            //    Quaternion targetRot = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
            //    Quaternion slerpedRot = Quaternion.Slerp(transform.rotation, targetRot, floorRotSpeed * Time.deltaTime);
            //    rb.rotation = slerpedRot;
            //}
        }
        else
        {
            isGrounded = false;
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            jumpVelocity = Mathf.Sqrt(jumpForce * gravityValue);
            isGrounded = false;
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
        additionalForce = Vector3.Lerp(additionalForce, Vector3.zero, 5 * Time.deltaTime);

        finalCalculatedVelocity = (baseVelocity + additionalVelocity) * transform.forward + spinOutVector + additionalForce + (Vector3.up * jumpVelocity);

        rb.velocity = finalCalculatedVelocity;
        #endregion

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
            ScoreManager.instance.StopTrick();
        }

        // if let grind button go or jump, stop grind
        if (Input.GetButtonUp("Grind") || Input.GetButtonDown("Jump"))
        {
            movementState = MovementState.STANDARD;
            currentGrind.HighlightColor(false);
            currentGrind = null;
            jumpVelocity = Mathf.Sqrt(jumpForce / 2 * gravityValue);
            ScoreManager.instance.StopTrick();
        }
    }

    void ProcessWallrideMovement()
    {
        rb.velocity = transform.forward * (baseVelocity + additionalVelocity);

        Quaternion initTargetRot = Quaternion.FromToRotation(transform.up, curWallRide.normal) * transform.rotation;
        Quaternion initSlerpedRot = Quaternion.Slerp(transform.rotation, initTargetRot, floorRotSpeed * 2f * Time.deltaTime);
        float rotDist = Vector3.Distance(initTargetRot.eulerAngles, initSlerpedRot.eulerAngles); // gets initial rotation, facing wall

        if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0 && !isBreaking)
        {
            if (turnVelocity < initTurnSpeed) { turnVelocity += initTurnAccel * Time.deltaTime; }
            else { turnVelocity += turnAccel * Time.deltaTime; }
        }
        else
        {
            turnVelocity = Mathf.MoveTowards(turnVelocity, 0, turnDecel * Time.deltaTime);
        }

        turnVelocity = Mathf.Clamp(turnVelocity, 0, maxTurnSpeed / 2);
        yaw = turnVelocity * Input.GetAxis("Horizontal");
        rb.angularVelocity = yaw * transform.up;

        RaycastHit hit;
        if (rotDist > 10)
        {
            rb.rotation = initSlerpedRot;
            //transform.position =  + (transform.up * hoverHeight);
        }
        else if (Physics.Raycast(transform.position, -transform.up, out hit, groundedDist + 3, LayerMask.GetMask("Wallride")))
        {
            if (hit.collider == curWallRide.collider)
            {
                Quaternion targetRot = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
                Quaternion slerpedRot = Quaternion.Slerp(transform.rotation, targetRot, floorRotSpeed * Time.deltaTime);
                rb.rotation = slerpedRot;
                //transform.position = transform.position + (transform.up * hoverHeight);
                print("wall");
            }
        }
        else
        {
            // stop wallride
            print("stop wallride");
            rb.angularVelocity = Vector3.zero;
            rb.rotation = startRot;
            wallRideInputActive = false;
            transform.position += transform.forward * 3;
            movementState = MovementState.STANDARD;
            ScoreManager.instance.StopTrick();
            //rb.rotation = Quaternion.Slerp(transform.rotation, startRot, floorRotSpeed * Time.deltaTime);
        }


        // jump off wall
        if (Input.GetButtonUp("Wallride"))
        {
            additionalForce = new Vector3(transform.up.x, 0, transform.up.z) * wallDismountForce;
            rb.rotation = Quaternion.Euler(new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, 0)); ;
            wallRideInputActive = false;
            movementState = MovementState.STANDARD;
            ScoreManager.instance.StopTrick();
        }

        RaycastHit groundHit;
        // too close to ground/edge of wall
        Vector3 groundDir;
        if (wallLeft) { groundDir = transform.right; }
        else { groundDir = -transform.right; }
        if (Physics.Raycast(transform.position, groundDir, out groundHit, 1, LayerMask.GetMask("Ground")))
        {
            print("too close to ground");
            Vector3 horizontalPush = new Vector3(transform.up.x, 0, transform.up.z) * 0.5f;
            transform.position = new Vector3(transform.position.x, groundHit.point.y + hoverHeight, transform.position.z) + horizontalPush;
            transform.rotation = Quaternion.FromToRotation(transform.up, groundHit.normal) * transform.rotation;
            movementState = MovementState.STANDARD;
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

    public bool CheckForGrind()
    {
        grindCollidersHit = Physics.OverlapBox(transform.position - new Vector3(0, grindDetectBox.y / 4), new Vector3(grindDetectBox.x / 2, grindDetectBox.y / 2, grindDetectBox.z / 2), transform.rotation, LayerMask.GetMask("Grind"));
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
        if (currentGrind != null) { currentGrind.HighlightColor(false); movementState = MovementState.STANDARD; ScoreManager.instance.StopTrick(); }
        currentGrind = null;
        return false;
    }

    public bool CheckForWall()
    {
        wallLeft = Physics.Raycast(transform.position, -transform.right, out leftWallHit, wallRideDist, LayerMask.GetMask("Wallride"));
        wallRight = Physics.Raycast(transform.position, transform.right, out rightWallHit, wallRideDist, LayerMask.GetMask("Wallride"));

        if (wallLeft) 
        { 
            leftWallHit.collider.GetComponent<WallrideObject>().HighlightColor(true);
            return true;
            //if (leftWallHit.collider.GetComponent<WallrideObject>().CheckValidNormal(leftWallHit.normal)) { return true; }
            //else { return false; } 
        }
        else if (wallRight) 
        { 
            rightWallHit.collider.GetComponent<WallrideObject>().HighlightColor(true);
            return true;
            //if (rightWallHit.collider.GetComponent<WallrideObject>().CheckValidNormal(rightWallHit.normal)) { return true; }
            //else { return false; }
        }
        else 
        { 
            return false; 
        }
    }

    void CheckCollision()
    {
        RaycastHit objectInFront;
        if (Physics.SphereCast(transform.position - transform.forward, 1.5f, transform.forward, out objectInFront, frontCheckDist))
        {
            float dotProduct = Vector3.Dot(transform.up, objectInFront.normal);
            if (dotProduct < dotTolerance)
            {
                // if able to go up, don't collide
                validCollider = objectInFront.collider;
            }
        }
        else
        {
            validCollider = null;
        }

        // front
        //if (!collisionActive || baseVelocity + additionalVelocity == 0) { return; }
        Collider[] wallsHitFront = Physics.OverlapBox(transform.position + frontBox.RelativeBoxPosition(transform), frontBox.boxSize / 2, transform.rotation);
        foreach (Collider coll in wallsHitFront)
        {
            CollideObject collided;
            if (coll.TryGetComponent<CollideObject>(out collided))
            {
                if (coll == validCollider)
                {
                    Vector3 collPoint = coll.ClosestPointOnBounds(transform.position);
                    Vector3 dir = (lastPosition - collPoint).normalized;
                    dir += -transform.forward; // give a little extra push in the direct opposite forward velocity
                    isSpinningOut = true;
                    spinOutVector = dir * Mathf.Min(maxSpinOutForce, ((baseVelocity + additionalVelocity) * spinOutMultiplier));

                    // spins the player graphics
                    spinOutDir = Mathf.RoundToInt(Mathf.Sign(rb.angularVelocity.y));
                    spinOutRate = 1440;
                    curSpinOutOffset = spinOutRate / 2 * spinOutDir; // initial 'force'
                }
            }
        }

        // character
        Collider[] wallsHitCharacter = Physics.OverlapBox(transform.position + characterBox.RelativeBoxPosition(transform), characterBox.boxSize / 2, transform.rotation);
        foreach (Collider coll in wallsHitCharacter)
        {
            CollideObject collided;
            if (coll.TryGetComponent<CollideObject>(out collided))
            {
                Vector3 collPoint = coll.ClosestPointOnBounds(transform.position);
                Vector3 dir = (lastPosition - collPoint).normalized;
                //dir += -transform.forward;
                transform.position = lastPosition;
                isSpinningOut = true;
                spinOutVector = dir * Mathf.Min(maxSpinOutForce, ((baseVelocity + additionalVelocity) * spinOutMultiplier * 5));

                // spins the player graphics
                spinOutDir = Mathf.RoundToInt(Mathf.Sign(rb.angularVelocity.y));
                spinOutRate = 1440;
                curSpinOutOffset = spinOutRate / 2 * spinOutDir; // initial 'force'
            }
        }

        // board
        Collider[] wallsHitBoard = Physics.OverlapBox(transform.position + boardBox.RelativeBoxPosition(transform), boardBox.boxSize / 2, transform.rotation);
        foreach (Collider coll in wallsHitBoard)
        {
            CollideObject collided;
            if (coll.TryGetComponent<CollideObject>(out collided))
            {
                Vector3 collPoint = coll.ClosestPointOnBounds(transform.position);
                Vector3 dir = (lastPosition - collPoint).normalized;
                //dir += -transform.forward;
                transform.position = lastPosition;
                isSpinningOut = true;
                spinOutVector = dir * Mathf.Min(maxSpinOutForce, ((baseVelocity + additionalVelocity) * spinOutMultiplier * 5));

                // spins the player graphics
                spinOutDir = Mathf.RoundToInt(Mathf.Sign(rb.angularVelocity.y));
                spinOutRate = 1440;
                curSpinOutOffset = spinOutRate / 2 * spinOutDir; // initial 'force'
            }
        }
        lastPosition = transform.position;
    }

    #region PROPERTY GETTERS
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

    public bool IsGrounded()
    {
        return isGrounded;
    }
    #endregion

    private void OnDrawGizmosSelected()
    {
        // ground raycast
        Debug.DrawRay(transform.position, -transform.up * groundedDist, Color.red);
        // wall raycasts
        Debug.DrawRay(transform.position, transform.right * wallRideDist, Color.red);
        Debug.DrawRay(transform.position, -transform.right * wallRideDist, Color.red);
        // forward collision spherecast
        Gizmos.DrawWireSphere(transform.position + (transform.forward * frontCheckDist), 1.5f);
        //Gizmos.DrawWireSphere(transform.position - transform.forward, 1.5f);
        

        Gizmos.color = Color.green;
        // grind detect box
        Gizmos.matrix = Matrix4x4.TRS(transform.position - new Vector3(0, grindDetectBox.y / 2), transform.rotation, grindDetectBox);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        // character collision box
        Gizmos.matrix = Matrix4x4.TRS(transform.position + characterBox.RelativeBoxPosition(transform), transform.rotation, characterBox.boxSize);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        // front collision box
        //Gizmos.matrix = Matrix4x4.TRS(transform.position + transform.forward * 2, transform.rotation, frontBox);
        Gizmos.matrix = Matrix4x4.TRS(transform.position + frontBox.RelativeBoxPosition(transform), transform.rotation, frontBox.boxSize);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        // board collision box
        Gizmos.matrix = Matrix4x4.TRS(transform.position + boardBox.RelativeBoxPosition(transform), transform.rotation, boardBox.boxSize);
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }


}
