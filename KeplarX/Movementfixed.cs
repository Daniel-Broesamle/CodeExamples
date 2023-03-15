using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.UIElements;
using UnityEngine.UI;

public class Movementfixed : MonoBehaviour
{
    [Header("Scripts")]
    public static Movementfixed instance;
    public HookShotModule Hookshotmodule;
    public Jetpackfixed Jetpack;
    public Pausenmenu Pausenmenu;
    public DataContainer Data;

    [Header("GameObjects")]
    [SerializeField] GameObject Armbehindcamera;

    [Header("Transforms And Vectors")]
    public Transform MainCameraTF;
    public Transform HookShotTF;
    public Transform CharacterchildobjectTF;
    public Transform AimingmodeLookDirectionTF;
    public Vector3 forward;
    public Vector3 backward;
    public Vector3 side;
    public Vector3 otherside;
    public Vector3 WalkDirection;
    public Vector3 LerpingWalkDirection;
    public Vector2 Direction;


    [Header("Rigidbodies")]
    public Rigidbody LucyRigidbody;


    [Header("Floats And Ints")]
    public float walkingspeed;
    public float dasdusenspeed;
    [SerializeField] float turnsmoothvelocity;
    public float smoothingtime = 0.2f;
    public float yangle;
    public float ytargetangle;
    //public float ZRotation;
    public float spin = 0;
    public float speedadjustment;
    public float duckfactor;
    public float lerpspeed;
    float sidewaylerp;
    float forwardlerp;
    [SerializeField] float railprogression;
    [SerializeField] float railprogressiongrowth = 0.003f;
    public int spinningcondition;



    [Header("Bools")]
    [SerializeField] public bool grounded = true;
    public bool walking = false;
    public bool freemoving = true;
    public bool permissiontojump = true;
    public bool dasduseactivated;
    public bool endlag = false;
    public bool turningonthespot = false;
    public bool ducking;
    public bool Menuopened;
    public bool grabbing;
    public bool permissiontograb;
    public bool jetsprint;
    public bool attacking;
    public bool duckend;
    public bool victory;
    bool braking = false;
    bool dasdusebuttonpressed = false;
    bool dasduseinuse = false;
    bool sprinting;
    bool zooming;

    [Header("Other")]
    public Animator PlayerAnimator;
    public Animator Animatorforthearmbehindthecamera;
    public CinemachineFreeLook WalkFL;
    public CinemachineImpulseSource ImpulseSource;
    public Collider Grabbox;
    public Collider NormalHardCapsule;
    public Collider NormalSoftCapsule;
    public Collider SmallHardCircle;
    public Collider Hitboxfaust;
    public Collider Hitboxfuss;
    public RaycastHit walljump;
    [SerializeField] Sprite Rubin;                                      //DEMO only
    [SerializeField] List<GameObject> bilder = new List<GameObject>();  //DEMO only
    [SerializeField] List<GameObject> InventoryItem = new List<GameObject>();
    [SerializeField] List<Transform> InventoryItemRightRail = new List<Transform>();
    [SerializeField] List<Transform> InventoryItemLeftRail = new List<Transform>();
    [SerializeField] List<int> InventoryItemPositions = new List<int>();

    private void Start()
    {
        instance = this;
    }
    public void Update()
    {
        if (Data.Inventory.Count > 0)
        {
            Itemstravellingthroughthetubes();
        }
        if (freemoving && !Menuopened)
        {
            if (!Jetpack.flying && spinningcondition != 0)
            {
                if (spin > 360) { spin -= 360; }
                if (spin < 0) { spin += 360; }
                CharacterchildobjectTF.localRotation = Quaternion.Euler(spin, 0, 0);
                PlayerAnimator.SetFloat("LucyEuler", spin);
                if (spin > 0)
                {
                    if (spin < 60 || spin > 290)
                    {
                        PlayerAnimator.SetBool("Backdrop", false);
                        PlayerAnimator.SetBool("Faceplant", false);
                    }
                    if (spin >= 60 && spin < 185)
                    {
                        PlayerAnimator.SetBool("Backdrop", true);
                        PlayerAnimator.SetBool("Faceplant", false);
                    }
                    if (spin >= 185 && spin <= 290)
                    {
                        PlayerAnimator.SetBool("Faceplant", true);
                        PlayerAnimator.SetBool("Backdrop", false);
                    }
                }
            }
            else
            {
                CharacterchildobjectTF.rotation = transform.rotation;
                spin = 0;
            }
            if (walking && grounded && !endlag)
            {
                PlayerAnimator.SetBool("Walking", true);
            }
            if (!walking || !grounded || endlag)
            {
                PlayerAnimator.SetBool("Walking", false);
            }
            if (!braking || !grounded)
            {
                LucyRigidbody.drag = 1.0f;
            }
        }
    }
    void Itemstravellingthroughthetubes()
    {
        railprogression += railprogressiongrowth;
        for (int i = 0; i < Data.Inventory.Count; i++)
        {
            InventoryItem[i].GetComponent<MeshFilter>().mesh = Data.Inventory[i].mesh;
            InventoryItem[i].GetComponent<MeshRenderer>().material = Data.Inventory[i].material;
            InventoryItem[i].transform.rotation = Quaternion.Euler(0, railprogression * 720, 0);
            if (i % 2 == 0)
            {
                InventoryItem[i].transform.position = Vector3.LerpUnclamped(InventoryItemRightRail[InventoryItemPositions[i]].position, InventoryItemRightRail[InventoryItemPositions[i] + 1].position, railprogression);
            }
            else
            {
                InventoryItem[i].transform.position = Vector3.LerpUnclamped(InventoryItemLeftRail[InventoryItemPositions[i]].position, InventoryItemLeftRail[InventoryItemPositions[i] + 1].position, railprogression);
            }
            if (railprogression >= 1)
            {
                if (InventoryItemPositions[i] >= 2)
                {
                    InventoryItemPositions[i] = 0;
                }
                else
                {
                    InventoryItemPositions[i] += 1;
                }
            }
        }
        if (railprogression >= 1) { railprogression = 0; }
    }
    void SoftCapsuleOn()
    {
        NormalSoftCapsule.enabled = true;
    }
    public void FixedUpdate()
    {
        if (!attacking)
        {
            if (duckend && !dasduseinuse)
            {
                ducking = true;
                if (grounded)
                {
                    walking = false;
                }
                PlayerAnimator.SetBool("Ducking", true);
                duckfactor = 2;
            }
            else
            {
                ducking = false;
                PlayerAnimator.SetBool("Ducking", false);
                duckfactor = 1;
            }
        }
        if (!grounded)
        {
            permissiontojump = false;
            PlayerAnimator.SetBool("Grounded", false);
        }
        else
        {
            permissiontograb = true;
            PlayerAnimator.SetBool("Grounded", true);
            PlayerAnimator.SetBool("Jump", false);

            if (sprinting || jetsprint || dasduseinuse)
            {
                if ((LucyRigidbody.velocity.x * LucyRigidbody.velocity.z > 100f || LucyRigidbody.velocity.x * LucyRigidbody.velocity.z < -100f))
                {
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position, -transform.forward, out hit, 1.5f) && hit.normal.y < 0.5f && hit.normal.y > -0.5f)
                    {
                        endlag = true;
                        ImpulseSource.GenerateImpulse(0.1f);
                        PlayerAnimator.SetBool("Uff", true);
                        LucyRigidbody.velocity = -LucyRigidbody.velocity / 3;
                        StopCoroutine("Dashduse");
                        dasduseinuse = false;
                        StartCoroutine("Wandauaaua");
                    }
                }
            }
        }
        if (!(ducking && grounded) && (Direction != Vector2.zero && !endlag) || jetsprint && !dasduseinuse && !PlayerAnimator.GetBool("Bruchlandung"))
        {
            walking = true;
        }
        else
        {
            walking = false;
            sprinting = false;
            PlayerAnimator.SetBool("Sprint", false);
            walkingspeed = 60f;
        }
        if ((PlayerAnimator.GetBool("Faceplant") || PlayerAnimator.GetBool("Backdrop")) && grounded && !jetsprint && !Jetpack.flying)
        {
            permissiontograb = false;
            ImpulseSource.GenerateImpulse(0.5f);
            StartCoroutine("EndlagderBruchlandung");
        }
        if (!attacking && !dasduseinuse)
        {
            if (spinningcondition == 0) { spin = 0; }
            if (spinningcondition == 1)
            {//LAUFENUNDDUCKEN
                spin -= 2.3f * duckfactor;
            }
            if (spinningcondition == -1)
            {//R?CKW?RTSSALTO
                spin += 10f * duckfactor;
            }
            if (spinningcondition == 4)
            {
                spin += 1f * duckfactor;
            }
            if (spinningcondition == 5)
            {
                spin -= 2f * duckfactor;
            }
            if (spinningcondition == 6)
            {
                spin += 4.6f * duckfactor;
            }
            if (spinningcondition == 7)
            {
                spin -= 10f * duckfactor;
            }
        }
        else
        {
            if (Hitboxfuss.enabled)
            {
                if (Physics.Raycast(transform.position - CharacterchildobjectTF.forward * 0.3f, -CharacterchildobjectTF.up, out walljump, 5f) && (walljump.normal.y < 0.5f && walljump.normal.y > -0.5f) && (walljump.collider.tag is "Boden" || walljump.collider.tag is "Eckig"))
                {
                    attacking = false;
                    LucyRigidbody.AddForce(new Vector3(walljump.normal.x, 1, walljump.normal.z) * 1000f);
                    yangle = transform.eulerAngles.y + 180;
                    if (yangle >= 360) { yangle -= 360; }
                    transform.eulerAngles = new Vector3(0f, yangle, 0f);
                    spin = -spin;
                    spinningcondition = 7;
                    StopCoroutine("Schlag");
                    SoftCapsuleOn();
                    endlag = false;
                    PlayerAnimator.SetBool("Punch", false);
                    Hitboxfuss.enabled = false;
                }
                Debug.DrawRay(transform.position + CharacterchildobjectTF.forward * 0.3f, -CharacterchildobjectTF.up, Color.green, 5f);
            }
        }
        Vectorcalculation();
        if (!Jetpack.flying && !dasduseinuse || jetsprint)
        {
            LucyRigidbody.AddForce(Vector3.down * 20, ForceMode.Acceleration);
        }
        if (zooming)
        {
            WalkFL.m_Lens.FieldOfView *= 0.99f;
        }
        if (!zooming && WalkFL.m_Lens.FieldOfView <= 79f) //Hier Playerprefswert-1
        {
            WalkFL.m_Lens.FieldOfView *= 1.04f;
        }
        if (dasduseactivated)
        {
            LucyRigidbody.AddForce(-transform.forward * 1.5f * dasdusenspeed, ForceMode.Impulse);
            dasduseactivated = false;
        }
    }
    public void Screen(InputAction.CallbackContext context)
    {
        if (!endlag && !ducking && grounded && context.performed)
        {
            Pausenmenu.Invoke("ScreenMenu", 0.5f);
        }
    }
    public void GRABBING(InputAction.CallbackContext context)
    {
        if (!endlag && !Jetpack.flying && !context.canceled)
        {
            if (!grounded)
            {
                permissiontograb = false;
            }
            StartCoroutine("Grab");
        }
    }
    public void DUCKING(InputAction.CallbackContext context)
    {
        if (!Jetpack.flying)
        {
            if (!context.canceled)
            {
                duckend = true;
            }
            else
            {
                duckend = false;
            }
        }
        else
        {
            if (context.started)
            {
                duckend = !duckend;
            }
        }
    }
    public void TURNING(InputAction.CallbackContext context)
    {
        if (!jetsprint && grounded)
        {
            lerpspeed = 0.5f;
        }
        else
        {
            if (!grounded)
            {
                lerpspeed = 0.1f;
            }
            else
            {
                lerpspeed = 0.01f;
            }
        }
        Direction = (context.ReadValue<Vector2>()).normalized;

        if (turningonthespot)
        {
            Direction = (context.ReadValue<Vector2>()).normalized;
        }
        if (context.canceled)
        {
            if (!jetsprint)
            {
                LerpingWalkDirection = new Vector3(0, 0, 0);
            }
            Direction = new Vector2(0, 0);
            walking = false;
        }
    }
    void MinispielGewonnen()
    {
        victory = true;
        Pausenmenu.PauseGame();
    }
    public void SPRINT(InputAction.CallbackContext context)
    {
        if (grounded && !Jetpack.flying)
        {
            sprinting = true;
            PlayerAnimator.SetBool("Sprint", true);
            walkingspeed = 80f;
        }
    }
    public void PUNCH(InputAction.CallbackContext context)
    {
        if (!(ducking && grounded) && !endlag && !context.canceled && !(spin >= 140 && spin < 240))
        {
            StartCoroutine("Schlag");
        }
    }
    public void Vectorcalculation()
    {
        if (!Hookshotmodule.aiming && !Hookshotmodule.reaching)
        {
            forward = new Vector3(transform.position.x - MainCameraTF.position.x, 0f, transform.position.z - MainCameraTF.position.z).normalized;
            backward = new Vector3(MainCameraTF.position.x - transform.position.x, 0f, MainCameraTF.position.z - transform.position.z).normalized;
        }
        else
        {
            forward = new Vector3(AimingmodeLookDirectionTF.position.x - MainCameraTF.position.x, 0f, AimingmodeLookDirectionTF.position.z - MainCameraTF.position.z).normalized;
            backward = new Vector3(AimingmodeLookDirectionTF.position.x - transform.position.x, 0f, AimingmodeLookDirectionTF.position.z - transform.position.z).normalized;
        }
        side = Vector3.Cross(Vector3.up, forward).normalized;
        otherside = Vector3.Cross(Vector3.up, backward).normalized;
        if (turningonthespot && !walking && !Jetpack.flying)
        {
            if (Direction != Vector2.zero)
            {
                forwardlerp = Mathf.Lerp(forwardlerp, Direction.y, 0.1f);
                sidewaylerp = Mathf.Lerp(sidewaylerp, Direction.x, 0.1f);
                WalkDirection = (forwardlerp * forward + sidewaylerp * side).normalized;
                yangle = Mathf.SmoothDampAngle(transform.eulerAngles.y, ytargetangle, ref turnsmoothvelocity, 0.03f);
                transform.rotation = Quaternion.Euler(0f, yangle, 0f);
                ytargetangle = Mathf.Atan2(-WalkDirection.x, -WalkDirection.z) * Mathf.Rad2Deg;
            }
        }
        if (jetsprint)
        {
            if (Direction != Vector2.zero)
            {
                WalkDirection = (Direction.y * forward + Direction.x * side);
                if (Direction != Vector2.zero)
                {
                    LerpingWalkDirection = new Vector3(LerpingWalkDirection.x, 0f, LerpingWalkDirection.z);
                    LerpingWalkDirection = Vector3.LerpUnclamped(LerpingWalkDirection, WalkDirection, 0.05f).normalized;
                }
            }
            LucyRigidbody.AddForce(LerpingWalkDirection * 120f * speedadjustment, ForceMode.Acceleration);
            ytargetangle = Mathf.Atan2(-LerpingWalkDirection.x, -LerpingWalkDirection.z) * Mathf.Rad2Deg;
            yangle = Mathf.SmoothDampAngle(transform.eulerAngles.y, ytargetangle, ref turnsmoothvelocity, 0.001f);
            transform.rotation = Quaternion.Euler(0f, yangle, 0f);
            CharacterchildobjectTF.localRotation = Quaternion.Euler(0f, 0f, 0f);
        }
        if (!endlag)
        {
            if (Hookshotmodule.reaching)
            {
                PlayerAnimator.SetBool("Reaching", true);
                ytargetangle = Mathf.Atan2(-Hookshotmodule.dragdirection.x, -Hookshotmodule.dragdirection.z) * Mathf.Rad2Deg;
                yangle = Mathf.SmoothDampAngle(transform.eulerAngles.y, ytargetangle, ref turnsmoothvelocity, smoothingtime / 2);
                transform.rotation = Quaternion.Euler(0f, yangle, 0f);
            }
            else
            {
                PlayerAnimator.SetBool("Reaching", false);
            }
            if (walking && !endlag && !Jetpack.flying)
            {
                WalkDirection = (Direction.y * forward + Direction.x * side);
                LerpingWalkDirection = Vector3.LerpUnclamped(LerpingWalkDirection, WalkDirection, lerpspeed).normalized;
                LucyRigidbody.AddForce(LerpingWalkDirection * walkingspeed * speedadjustment, ForceMode.Acceleration);
                ytargetangle = Mathf.Atan2(-LerpingWalkDirection.x, -LerpingWalkDirection.z) * Mathf.Rad2Deg;
                if (grounded)
                {
                    yangle = Mathf.SmoothDampAngle(transform.eulerAngles.y, ytargetangle, ref turnsmoothvelocity, 0.03f);
                }
                else
                {
                    yangle = Mathf.SmoothDampAngle(transform.eulerAngles.y, ytargetangle, ref turnsmoothvelocity, 0.01f);
                }
                transform.rotation = Quaternion.Euler(0f, yangle, 0f);
            }
        }
    }
    public void Fluglaufvoid()
    {
        duckend = false;
        lerpspeed = 0.005f;
        turningonthespot = false;
        WalkDirection = Jetpack.forward;
        LerpingWalkDirection = Jetpack.flydirection;
        Jetpack.flydirection = Vector3.zero;
        Jetpack.cameradrift = Vector2.zero;
        Jetpack.stickposition = Vector2.zero;
        Jetpack.forward = Vector3.zero;
        Jetpack.side = Vector3.zero;
        Jetpack.up = Vector3.zero;
        jetsprint = true;
        yangle = transform.eulerAngles.y;
        transform.eulerAngles = new Vector3(0, yangle, 0);
    }
    public void JUMP()
    {
        if (permissiontojump && !endlag && !Hookshotmodule.aiming && !dasduseinuse)
        {
            if (jetsprint)
            {
                duckend = false;
                jetsprint = false;
                Jetpack.FlugstartKameraAusrichtung();
            }
            StartCoroutine("Jumping");
            //if (!Bremst)
            //{
                if (ducking && !jetsprint)
                {
                    spinningcondition = 6;
                    LucyRigidbody.velocity = new Vector3(0, 30f, 0);
                }
                else
                {
                    LucyRigidbody.velocity = new Vector3(LucyRigidbody.velocity.x, 20f, LucyRigidbody.velocity.z);
                }
            //}
            //else
            //{
            //    StopCoroutine("Bremse");
            //    Bremst = false;
            //    Spinning = -1;
            //    LucyRigidbody.velocity = new Vector3(LucyRigidbody.velocity.x * -1.5f, 25f, LucyRigidbody.velocity.z * -1.5f);
            //}
            grounded = false;
        }
    }
    public void Collision(Collider collision)
    {
        if (collision.tag == "Item2")
        {
            collision.tag = "Item3";
            PlayerAnimator.SetBool("DeepGrab", true);
            Data.Inventory.Add(collision.GetComponent<Item>().ItemSo);
            bilder[Data.Inventory.Count - 1].GetComponent<UnityEngine.UI.Image>().sprite = Rubin;
            Destroy(collision.gameObject);
            if (Data.Inventory.Count >= 6)
            {
                MinispielGewonnen();
            }
        }
    }
    public void DASDUSE(InputAction.CallbackContext context)
    {
        if (!endlag && !Jetpack.flying && (spin >= 330 || spin <= 30) && !dasduseinuse && !context.canceled)
        {
            walking = false;
            PlayerAnimator.SetBool("Walking", false);
            PlayerAnimator.SetBool("Dasduseinitiated", true);
            endlag = true;
            dasduseinuse = true;
            turningonthespot = true;
            StartCoroutine("Dashduse");
        }
        if (context.performed)
        {
            dasdusebuttonpressed = true;
        }
        if (context.canceled)
        {
            dasdusebuttonpressed = false;
        }
    }
    IEnumerator Dashduse()
    {
        yield return new WaitForSeconds(0.1f);
        if (dasdusebuttonpressed && dasdusenspeed < 30f)
        {
            zooming = true;
            yield return new WaitForSeconds(0.05f);
            dasdusenspeed += 3f;
            StartCoroutine("Dashduse");
        }
        else
        {
            zooming = false;
            PlayerAnimator.SetBool("Dasduseinitiated", false);
            PlayerAnimator.SetBool("Dasdusefire", true);
            LucyRigidbody.AddForce(-transform.forward * 1.5f * dasdusenspeed, ForceMode.Impulse);
            turningonthespot = false;
            yield return new WaitForSeconds(0.75f);
            if (grounded)
            {
                LucyRigidbody.velocity = new Vector3(LucyRigidbody.velocity.x / 3, LucyRigidbody.velocity.y, LucyRigidbody.velocity.z / 3);
            }
            yield return new WaitForSeconds(0.25f);
            dasduseinuse = false;
            PlayerAnimator.SetBool("Dasdusefire", false);
            dasdusenspeed = 21f;
            endlag = false;
        }
    }
    IEnumerator Grab()
    {
        PlayerAnimator.SetBool("Reaching", false);
        Hookshotmodule.reaching = false;
        Hookshotmodule.permissionToReach = false;
        Grabbox.enabled = true;
        turningonthespot = false;
        PlayerAnimator.SetBool("Grabbing", true);
        endlag = true;
        grabbing = true;
        yield return new WaitForSeconds(0.2f);
        grabbing = false;
        PlayerAnimator.SetBool("Grabbing", false);
        yield return new WaitForSeconds(0.45f);
        Grabbox.enabled = false;
        yield return new WaitForSeconds(0.3f);
        endlag = false;
        PlayerAnimator.SetBool("DeepGrab", false);
    }
    IEnumerator Jumping()
    {
        PlayerAnimator.SetBool("Jump", true);
        yield return new WaitForSeconds(0.1f);
        PlayerAnimator.SetBool("Jump", false);
    }
    IEnumerator ArmRunter()
    {
        yield return new WaitForSeconds(0.3f);
        Armbehindcamera.SetActive(false);
    }
    IEnumerator EndlagderBruchlandung()
    {
        LucyRigidbody.velocity = Vector3.zero;
        endlag = true;
        PlayerAnimator.SetBool("Ducking", false);
        PlayerAnimator.SetFloat("LucyRotation", 0);
        PlayerAnimator.SetFloat("LucyEuler", 0);
        if (!grabbing)
        {
            yield return new WaitForSeconds(0.1f);
            PlayerAnimator.SetBool("Backdrop", false);
            PlayerAnimator.SetBool("Faceplant", false);
            yield return new WaitForSeconds(1f);
            endlag = false;
        }
        else
        {
            yield return new WaitForSeconds(0.1f);
            PlayerAnimator.SetBool("Backdrop", false);
            PlayerAnimator.SetBool("Faceplant", false);
            transform.rotation = Quaternion.Euler(0f, yangle, 0f);
            yield return new WaitForSeconds(0.5f);
            endlag = false;
        }
    }
    IEnumerator Wandauaaua()
    {
        endlag = true;
        PlayerAnimator.SetBool("Ducking", false);
        PlayerAnimator.SetBool("Walking", false);
        PlayerAnimator.SetFloat("LucyRotation", 0);
        PlayerAnimator.SetFloat("LucyEuler", 0);
        jetsprint = false;
        yield return new WaitForSeconds(0.1f);
        yield return new WaitForSeconds(2.5f);
        endlag = false;
        PlayerAnimator.SetBool("Uff", false);
    }
    IEnumerator Schlag()
    {
        endlag = true;
        attacking = true;
        PlayerAnimator.SetBool("Punch", true);
        if (!grounded)
        {
            if (spin >= 140 && spin < 240)
            { PlayerAnimator.SetBool("Airpunch", false); PlayerAnimator.SetBool("Dropkick", false); PlayerAnimator.SetBool("Dunk", false); }
            if (spin < 140 || spin >= 310)
            {
                if (spin >= 45 && spin < 140 || (Physics.Raycast(transform.position, -CharacterchildobjectTF.forward, out walljump, 5f) && walljump.collider.tag is "Wand"))
                {
                    if (!((Physics.Raycast(transform.position, -CharacterchildobjectTF.forward, out walljump, 5f) && walljump.collider.tag is "Wand")))
                    {
                        PlayerAnimator.SetBool("Dropkick", true);
                        PlayerAnimator.SetBool("Airpunch", false);
                        PlayerAnimator.SetBool("Dunk", false);
                        yield return new WaitForSeconds(0.2f);
                        NormalSoftCapsule.enabled = false;
                        Hitboxfuss.enabled = true;
                        yield return new WaitForSeconds(0.1f);
                        PlayerAnimator.SetBool("Punch", false);
                        yield return new WaitForSeconds(0.1f);
                        Hitboxfuss.enabled = false;
                        NormalSoftCapsule.enabled = true;
                        yield return new WaitForSeconds(0.1f);
                        attacking = false;
                        endlag = false;
                    }
                    else
                    {
                        Time.timeScale = 0f;
                        PlayerAnimator.SetBool("Dropkick", true);
                        PlayerAnimator.SetBool("Airpunch", false);
                        PlayerAnimator.SetBool("Dunk", false);
                        yield return new WaitForSeconds(0.2f);
                        Hitboxfuss.enabled = true;
                        attacking = false;
                        LucyRigidbody.AddForce(new Vector3(walljump.normal.x, 1, walljump.normal.z) * 1000f);
                        yangle = transform.eulerAngles.y + 180;
                        if (yangle >= 360) { yangle -= 360; }
                        transform.eulerAngles = new Vector3(transform.eulerAngles.x, yangle, transform.eulerAngles.z);
                        spin = -35;
                        spinningcondition = 7;
                        endlag = false;
                        PlayerAnimator.SetBool("Punch", false);
                        Hitboxfuss.enabled = false;
                        StopCoroutine("Schlag");
                    }
                }
                else
                {
                    PlayerAnimator.SetBool("Dunk", true);
                    PlayerAnimator.SetBool("Airpunch", false);
                    PlayerAnimator.SetBool("Dropkick", false);
                    yield return new WaitForSeconds(0.2f);
                    Hitboxfaust.enabled = true;
                    yield return new WaitForSeconds(0.1f);
                    PlayerAnimator.SetBool("Punch", false);
                    yield return new WaitForSeconds(0.1f);
                    Hitboxfaust.enabled = false;
                    yield return new WaitForSeconds(0.1f);
                    attacking = false;
                    endlag = false;
                }
            }
            if (spin >= 220 && spin < 310)
            {
                PlayerAnimator.SetBool("Airpunch", true);
                PlayerAnimator.SetBool("Dropkick", false);
                PlayerAnimator.SetBool("Dunk", false);
                yield return new WaitForSeconds(0.2f);
                Hitboxfaust.enabled = true;
                yield return new WaitForSeconds(0.1f);
                PlayerAnimator.SetBool("Punch", false);
                yield return new WaitForSeconds(0.1f);
                Hitboxfaust.enabled = false;
                yield return new WaitForSeconds(0.1f);
                attacking = false;
                endlag = false;
            }
        }
        else
        {
            yield return new WaitForSeconds(0.2f);
            Hitboxfaust.enabled = true;
            yield return new WaitForSeconds(0.2f);
            PlayerAnimator.SetBool("Punch", false);
            yield return new WaitForSeconds(0.2f);
            Hitboxfaust.enabled = false;
            yield return new WaitForSeconds(0.2f);
            Hitboxfuss.enabled = false;
            yield return new WaitForSeconds(0.1f);
            attacking = false;
            endlag = false;
        }
    }
    IEnumerator Bremse()
    {
        braking = true;
        spinningcondition = 4;
        LucyRigidbody.drag = 4.0f;
        yield return new WaitForSeconds(0.3f);
        spinningcondition = 5;
        yield return new WaitForSeconds(0.15f);
        braking = false;
        spinningcondition = 0;
        spin = 0;
    }
    public void OnTriggerEnter(Collider collision)
    {
        if (collision.tag is "Player.HookShot" && grabbing)
        {
            PlayerAnimator.SetBool("DeepGrab", true);
            collision.GetComponent<Greifhaken1>().Collect();
        }
        if (collision.tag == "Boden" || collision.tag == "Eckig")
        {
            if (NormalSoftCapsule.enabled)
            {
                permissiontojump = true;
                spinningcondition = 0;
                grounded = true;
            }
        }
    }
    public void OnTriggerStay(Collider collision)
    {
        if (collision.tag == "Boden")
        {
            if (collision.tag == "Boden" || collision.tag == "Eckig")
            {
                if (NormalSoftCapsule.enabled)
                {
                    transform.rotation = Quaternion.Euler(0f, yangle, 0f);
                    grounded = true;
                }
            }
        }
    }
    public void OnTriggerExit(Collider collision)
    {
        if (collision.tag == "Boden")
        {
            grounded = false;
        }
    }
}