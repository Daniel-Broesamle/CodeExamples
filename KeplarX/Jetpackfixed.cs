using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using UnityEngine.InputSystem;

public class Jetpackfixed : MonoBehaviour
{
    [Header("Scripts")]
    public static Jetpackfixed instance;
    [SerializeField] Movementfixed Movement;

    [Header("GameObjects")]
    [SerializeField] GameObject WalkGO;

    [Header("Transforms And Vectors")]
    public Transform Rotationstransfer;
    [SerializeField] Transform KameraTF;
    public Vector3 flydirection;
    public Vector3 side;
    public Vector3 up;
    public Vector3 forward;
    public Vector3 lerpingdirection;
    public Vector2 cameradrift;
    public Vector2 stickposition;

    [Header("Rigidbodies")]
    [SerializeField] Rigidbody LucyRB;

    [Header("Floats")]
    float flightspeed = 40;
    public float currentfuel;
    [SerializeField] float tankfillandemptyrate;
    [SerializeField] float tanksize = 102f;
    float yrotation;
    //int targetFrameRate = 60;

    [Header("Bools")]
    public bool flying = false;
    [SerializeField] bool fixedcameramode = true;
    public bool downwardsthrust = false;
    public bool permissiontofly = true;
    public bool takeoff;
    public bool controlling = true;

    [Header("Others")]
    [SerializeField] Material Blinklichttextur;
    [SerializeField] Animator PlayerAnimator;
    [SerializeField] CinemachineFreeLook WalkFL;
    [SerializeField] CinemachineFreeLook FlyingFL;
    [SerializeField] Light Fuellamp;
    void Start()
    {
        instance = this;
        QualitySettings.vSyncCount = 0;
        LucyRB = GetComponent<Rigidbody>();
        Movement = GetComponent<Movementfixed>();
        currentfuel = 102f;
    }
    private void Update()
    {
        if (flying && currentfuel >= 0 && !Movement.endlag && permissiontofly)
        {
            if (!controlling)
            {
                flydirection = forward;
            }
        }
        if (!flying && Movement.grounded)
        {
            yrotation = 0;
            WalkGO.SetActive(true);
        }
        if (!flying || !permissiontofly)
        {
            PlayerAnimator.SetBool("Fliegt", false);
            Movement.freemoving = true;
        }
        if (flying && currentfuel >= 0&&!Movement.jetsprint)
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0 && flightspeed <= 60)
            {
                flightspeed += Input.GetAxis("Mouse ScrollWheel") * 10;
            }
            if (Input.GetAxis("Mouse ScrollWheel") < 0 && flightspeed >= 10)
            {
                flightspeed += Input.GetAxis("Mouse ScrollWheel") * 10;
            }
            Movement.CharacterchildobjectTF.localRotation = Quaternion.Euler(0, yrotation, 0);
            PlayerAnimator.SetBool("Fliegt", true);
            WalkGO.SetActive(false);
            Movement.freemoving = false;
            //if (fixedcameramode)
            //{
                flydirection = (forward + stickposition.x * -side * 0.5f + stickposition.y * up * 0.5f).normalized;
                if (yrotation > -45 && stickposition.x > 0)
                {
                    yrotation -= 0.1f;
                }
                if (yrotation < 45 && stickposition.x < 0)
                {
                    yrotation += 0.1f;
                }
                if (stickposition.x == 0)
                {
                    yrotation = Mathf.Lerp(yrotation, 0, 0.03f);
                }
            //}
            if (forward.y == 0 && !Movement.jetsprint)
            {
                transform.rotation = Quaternion.LookRotation(Vector3.up - flydirection * 0.01f);
            }
            if (forward.y > 0 && !takeoff && !Movement.jetsprint)
            {
                transform.rotation = Quaternion.LookRotation(up);
            }
            if (forward.y < 0 && !Movement.jetsprint)
            {
                Rotationstransfer.LookAt(Rotationstransfer.position + up);
                transform.eulerAngles = new Vector3(Rotationstransfer.eulerAngles.x, Rotationstransfer.eulerAngles.y, 180);
            }
        }
    }
    IEnumerator FlugstartCoroutine()
    {
        FlugstartKameraAusrichtung();
        takeoff = true;
        forward = Movement.CharacterchildobjectTF.forward + Vector3.up;
        yield return new WaitForSeconds(0.05f);
        takeoff = false;
    }
    public void FlugstartKameraAusrichtung()
    {
        FlyingFL.m_XAxis.Value = transform.eulerAngles.y - 180;
        WalkFL.m_XAxis.Value = transform.eulerAngles.y - 180;
    }
    IEnumerator TimeOutforMayTurn()
    {
        yield return new WaitForSeconds(0.4f);
        Movement.freemoving = true;
    }
    void FixedUpdate()
    {
        FlyingFL.m_YAxis.Value += cameradrift.y * 0.01f;
        FlyingFL.m_XAxis.Value += cameradrift.x * 0.2f;
        if (currentfuel <= 0f && flying)
        {
            FlugstoppinderLuft();
        }
        if (downwardsthrust)
        {
            LucyRB.AddForce(Vector3.down * 160f, ForceMode.Acceleration);
            if (Movement.grounded&&!PlayerAnimator.GetBool("Faceplant") && !PlayerAnimator.GetBool("Backdrop"))
            {
                StartCoroutine("Superheldenlandung");
            }
        }
        if(!takeoff)
        {
            if (flying && currentfuel >= 0)
            {
                if (!Movement.jetsprint)
                {
                    forward = Vector3.LerpUnclamped(forward, new Vector3(transform.position.x - KameraTF.position.x, transform.position.y - KameraTF.position.y + 2, transform.position.z - KameraTF.position.z).normalized, 0.05f); //ist forward.y=0,stimmt die Rotation nicht
                    side = Vector3.Cross(forward, Vector3.up).normalized;
                    up = Vector3.Cross(side, forward).normalized;
                    Fuellamp.color = Color.LerpUnclamped(Fuellamp.color, Color.red, 0.005f);
                    Blinklichttextur.color = Fuellamp.color;
                    currentfuel -= tankfillandemptyrate;
                    LucyRB.AddForce(flydirection * flightspeed, ForceMode.Acceleration);
                }
            }
        }
        else
        {
            forward = new Vector3(transform.position.x - KameraTF.position.x, transform.position.y - KameraTF.position.y + 2, transform.position.z - KameraTF.position.z).normalized; side = Vector3.Cross(forward, Vector3.up).normalized;
            up = Vector3.Cross(side, forward).normalized;
            Fuellamp.color = Color.LerpUnclamped(Fuellamp.color, Color.red, 0.005f);
            Blinklichttextur.color = Fuellamp.color;
            currentfuel -= tankfillandemptyrate;
            LucyRB.AddForce(flydirection * flightspeed, ForceMode.Acceleration);
        }
        if (Movement.freemoving && !Movement.endlag && Movement.spin == 0)
        {
            Vector3 Flugrichtung = new Vector3(Movement.LucyRigidbody.velocity.x + Movement.CharacterchildobjectTF.forward.x, 48, Movement.LucyRigidbody.velocity.z + Movement.CharacterchildobjectTF.forward.z).normalized;
        }
        if (!flying && currentfuel <= tanksize)
        {
            if (Movement.grounded)
            {
                currentfuel += tankfillandemptyrate;
                Fuellamp.color = Color.LerpUnclamped(Fuellamp.color, Color.green, 0.005f); //Tankfuellungsrate/40
                Blinklichttextur.color = Fuellamp.color;
            }
            if (Movement.spinningcondition != 0 && Movement.spinningcondition != 1)
            {
                currentfuel += tankfillandemptyrate*2;
                Fuellamp.color = Color.LerpUnclamped(Fuellamp.color, Color.green, 0.01f); //Tankfuellungsrate/20
            }
        }
        if (flying && Movement.grounded && !takeoff&&!Movement.jetsprint)
        {
            if (!Movement.ducking && !Movement.jetsprint)
            {
                if (transform.eulerAngles.x <= 314)
                {
                    Movement.turningonthespot = false;
                    Movement.endlag = true;
                    flying = false;
                    WalkFL.m_XAxis.Value = FlyingFL.m_XAxis.Value;
                    WalkFL.m_YAxis.Value = FlyingFL.m_YAxis.Value;
                    StartCoroutine("BruchlandungODERFaceplant");
                }
            }
            else
            {
                Movement.Fluglaufvoid();
            }
        }
    }
    public void NAVIGATING(InputAction.CallbackContext context)
    {
        if (!Movement.jetsprint)
        {
            if (!context.canceled)
            {
                stickposition = context.ReadValue<Vector2>().normalized;
            }
            else
            {
                stickposition = Vector2.zero;
            }
            if (!Movement.endlag && flying && !Movement.jetsprint)
            {
                controlling = true;
                cameradrift.x = 2 * stickposition.x;
                cameradrift.y = -stickposition.y;
            }
            if (context.ReadValue<Vector2>() == Vector2.zero)
            {
                controlling = false;
            }
        }
    }
    public void FLIGHT(InputAction.CallbackContext context)
    {
        if (currentfuel > 0.2f && permissiontofly&&!Movement.endlag&&!HookShotModule.instance.aiming)
        {
            if (context.started)
            {
                StartCoroutine("FlugstartCoroutine");
                Movement.spinningcondition = 0;
                Movement.spin = 0;
                if (Movement.grounded)
                {
                    flydirection = new Vector3(0, LucyRB.velocity.y + 1, 0).normalized;
                    FlyingFL.m_YAxis.Value = 0.6f;
                    Movement.grounded = false;
                    LucyRB.AddForce(Vector3.up * 4f, ForceMode.Impulse);
                    Movement.walking = false;
                }
                else
                {
                    flydirection = Movement.CharacterchildobjectTF.up;
                    FlyingFL.m_YAxis.Value = 1 - flydirection.y;
                }
            }
            if (!Movement.endlag && !context.canceled && currentfuel > 5f)
            {
                flying = true;
            }
            if (context.canceled && !Movement.grounded)
            {
                StopCoroutine("FlugstartCoroutine");
                FlugstoppinderLuft();
            }
        }
        if (context.canceled && Movement.jetsprint)
        {
            flying = false;
            Movement.jetsprint = false;
            Movement.duckend = false;
            Movement.ducking = false;
            forward = Vector3.zero;
            side = Vector3.zero;
            up = Vector3.zero;
            if (Movement.Direction == Vector2.zero)
            {
                Movement.walking = false;
            }
            else
            {
                Movement.walking = true;
            }
        }
    }
    void FlugstoppinderLuft()
    {
        Movement.duckend = false;
        Movement.ducking = false;
        stickposition = Vector2.zero;
        flying = false;
        WalkFL.m_XAxis.Value = FlyingFL.m_XAxis.Value;
        WalkFL.m_YAxis.Value = FlyingFL.m_YAxis.Value;
        if (!Movement.grounded)
        {
            Movement.spinningcondition = 1;
            Movement.endlag = false;
        }
        if (forward.y >= 0 && !Movement.grounded)
        {
            Movement.spin = transform.eulerAngles.x;
            Movement.yangle = transform.eulerAngles.y;
            Movement.ytargetangle = transform.eulerAngles.y;
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
            Movement.CharacterchildobjectTF.localRotation = Quaternion.Euler(Movement.spin, 0f, 0f);
        }
        if (forward.y < 0 && !Movement.grounded)
        {
            Movement.spin = transform.eulerAngles.x;
            if (Movement.yangle <= 180)
            {
                Movement.yangle = transform.eulerAngles.y + 180;
                Movement.ytargetangle = Movement.yangle;
            }
            else
            {
                Movement.yangle = transform.eulerAngles.y - 180;
                Movement.ytargetangle = Movement.yangle;
            }
            transform.eulerAngles = new Vector3(0, Movement.yangle, 0);
            Movement.CharacterchildobjectTF.localRotation = Quaternion.Euler(Movement.spin, 0f, 0f);
        }
        LucyRB.AddForce(flydirection * 10f, ForceMode.VelocityChange);
        WalkGO.SetActive(true);
        stickposition = Vector2.zero;
        cameradrift = Vector2.zero;
        forward = Vector3.zero;
        side = Vector3.zero;
        up = Vector3.zero;
        Movement.PlayerAnimator.SetBool("Faceplant", true);
        StartCoroutine("TimeOutforMayTurn");
    }
    public void TRIGGER(InputAction.CallbackContext context)
    {
        if (currentfuel >= 0)
        {
            flightspeed = 40f + context.ReadValue<float>() * 25f;
        }
    }
    public void ABWARTSDUSE(InputAction.CallbackContext context)
    {
        if (!Movement.grounded && !context.canceled)
        {
            downwardsthrust = true;
        }
        if (context.canceled && downwardsthrust)
        {
            downwardsthrust = false;
        }
    }
    IEnumerator BruchlandungODERFaceplant()
    {
        cameradrift = Vector2.zero;
        if (forward.y < 0)
        {
            Movement.yangle = transform.eulerAngles.y + 180;
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y + 180, 0);
        }
        else
        {
            Movement.yangle = transform.eulerAngles.y;
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        }
        stickposition = Vector2.zero;
        forward = Vector3.zero;
        side = Vector3.zero;
        up = Vector3.zero;
        permissiontofly = false;
        Movement.ImpulseSource.GenerateImpulse(0.2f);
        PlayerAnimator.SetBool("Bruchlandung", true);
        yield return new WaitForSeconds(0.1f);
        PlayerAnimator.SetBool("Bruchlandung", false);
        yield return new WaitForSeconds(1.8f);
        Movement.endlag = false;
        permissiontofly = true;
    }
    IEnumerator Superheldenlandung()
    {
        Movement.ImpulseSource.GenerateImpulse(0.5f);
        Movement.endlag = true;
        flying = false;
        PlayerAnimator.SetBool("Superheldenlandung", true);
        if (!Movement.ducking)
        {
            yield return new WaitForSeconds(0.3f);
        }
        PlayerAnimator.SetBool("Superheldenlandung", false);
        downwardsthrust = false;
        yield return new WaitForSeconds(0.6f);
        Movement.endlag = false;
    }
}
