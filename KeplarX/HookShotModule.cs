using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class HookShotModule : MonoBehaviour
{
    [Header("Scripts")]
    public Movementfixed Movement;
    public static HookShotModule instance;
    public Greifhaken1 HookShotScript;
    public List<InteractionwithHookshot> wrapped = new List<InteractionwithHookshot>();
    public List<InteractionwithHookshot> touchedobjects = new List<InteractionwithHookshot>();


    [Header("GameObjects")]
    public GameObject HookShotGO;
    public GameObject walkingcameraGO;
    public GameObject flyingcameraGO;
    public GameObject Fadenkreuz;
    public GameObject shootingcameraGO;


    [Header("Rigidbodies")]
    public Rigidbody HookShotRB;
    public Rigidbody PlayerRigidbody;


    [Header("Bools")]
    public bool hookedToDraggable = false;
    public bool hookedToReachable = false;
    public bool reaching = false;
    public bool aiming = false;
    public bool permissionToReach = true;
    public bool hasHit = false;
    public bool hasShot = false;


    [Header("Vectors and Transforms")]
    [SerializeField] private Transform PlayerTransform;
    public List<Vector3> eckpunkte = new List<Vector3>();
    [SerializeField] Transform wrist;
    public Vector3 dragdirection;
    public Vector3 Hookshotdirection;
    public Vector3 AktuellsterEckpunkt;
    public Vector3 VorletzterEckpunkt;


    [Header("Floats")]
    [SerializeField] private float HookShotSpeed = 10000;
    float cumulativewrappedropelength;
    float currentropelength = 200f;
    float maxropelength = 200f;


    [Header("Other")]
    [SerializeField] private LayerMask aimColliderLayerMask = new LayerMask();
    public RaycastHit raycasthit;
    public RaycastHit edgecheck;
    public LineRenderer linerenderer;
    public CinemachineFreeLook walkingcameraFL;
    public CinemachineFreeLook flyingcameraFL;
    public CinemachineFreeLook aimingcameraFL;
    private void Start()
    {
        instance = this;
        linerenderer.startWidth = 0.1f;
        linerenderer.endWidth = 0.1f;
        PlayerTransform = GetComponent<Transform>();
        PlayerRigidbody = GetComponent<Rigidbody>();
        Movement = GetComponent<Movementfixed>();
        HookShotScript = HookShotGO.GetComponent<Greifhaken1>();
    }
    void Update()
    {
        if (HookShotScript.HookedToReachable || hasHit)
        {
            RopeTouch();
            UnravelMethod();
        }
        if (aiming && !Jetpackfixed.instance.flying)
        {
            Movement.yangle = PlayerTransform.eulerAngles.y;
            if (!reaching)
            {
                PlayerTransform.rotation = Quaternion.Euler(PlayerTransform.eulerAngles.x, aimingcameraFL.m_XAxis.Value + 180, PlayerTransform.eulerAngles.z);
            }
            else
            {
                aimingcameraFL.m_XAxis.Value = PlayerTransform.eulerAngles.y + 180;
            }
        }
        else
        {
            aimingcameraFL.m_XAxis.Value = walkingcameraFL.m_XAxis.Value;
        }
    }
    void RopeTouch()
    {
        linerenderer.SetPosition(linerenderer.positionCount - 1, wrist.position);
        float DISTANZLucybisaktuellsterEckpunkt = Vector3.Distance(PlayerTransform.position, AktuellsterEckpunkt);
        if (Physics.Raycast(wrist.position, AktuellsterEckpunkt - wrist.position, out RaycastHit Edgecheck, DISTANZLucybisaktuellsterEckpunkt))
        {
            float DISTANZEdgecheckbisaktuellsterEckpunkt = Vector3.Distance(Edgecheck.point, AktuellsterEckpunkt);
            if (Edgecheck.collider.tag == "Boden" && DISTANZEdgecheckbisaktuellsterEckpunkt > 0.3f)
            {
                Vector3 NeuerEckpunkt = Edgecheck.point + Edgecheck.normal;
                eckpunkte.Add(NeuerEckpunkt);
                AktuellsterEckpunkt = Edgecheck.point;
                VorletzterEckpunkt = eckpunkte[eckpunkte.Count - 2];
                cumulativewrappedropelength += Vector3.Distance(AktuellsterEckpunkt, VorletzterEckpunkt);
                if (Edgecheck.collider.GetComponent<InteractionwithHookshot>() != null)
                {
                    Edgecheck.collider.SendMessage("Interaction", eckpunkte.Count - 1);
                }
                LineChange();
            }
            if (Edgecheck.collider.tag == "Eckig" && DISTANZEdgecheckbisaktuellsterEckpunkt > 0.3f)
            {
                Vector3 NeuerEckpunkt = Edgecheck.point;
                eckpunkte.Add(NeuerEckpunkt);
                AktuellsterEckpunkt = Edgecheck.point;
                VorletzterEckpunkt = eckpunkte[eckpunkte.Count - 2];
                cumulativewrappedropelength += Vector3.Distance(AktuellsterEckpunkt, VorletzterEckpunkt);
                if (Edgecheck.collider.GetComponent<InteractionwithHookshot>() != null)
                {
                    Edgecheck.collider.SendMessage("Interaction", eckpunkte.Count - 1);
                }
                LineChange();
            }
        }
    }
    public void LineChange()
    {
        linerenderer.positionCount = eckpunkte.Count + 1;
        for (int i = 0; i < eckpunkte.Count; i++)
        {
            linerenderer.SetPosition(i, eckpunkte[i]);
        }
        linerenderer.SetPosition(linerenderer.positionCount - 1, wrist.position);
    }
    public void Trefferlandung()
    {
        linerenderer.enabled = true;
        linerenderer.positionCount = 2;
        linerenderer.SetPosition(0, HookShotGO.transform.position);
        AktuellsterEckpunkt = HookShotGO.transform.position;
        eckpunkte.Add(HookShotGO.transform.position);
        LineChange();
    }
    public void Einziehen()
    {
        HookShotRB.constraints = RigidbodyConstraints.None;
        if (eckpunkte.Count > 1)
        {
            HookShotRB.AddForce(eckpunkte[1] - HookShotRB.transform.position * 1f);
        }
        else
        {
            HookShotRB.AddForce(transform.position - HookShotRB.transform.position * 1f);
        }
        linerenderer.SetPosition(0, HookShotRB.transform.position);
    }
    public void Shot()
    {
        if (!hasShot && !hasHit && !Movement.endlag && !Movement.ducking)
        {
            cumulativewrappedropelength = 0;
            Vector2 ScreenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Ray ray = Camera.main.ScreenPointToRay(ScreenCenterPoint);
            if (Physics.Raycast(ray, out RaycastHit raycasthit, maxropelength, aimColliderLayerMask))
            {
                HookShotGO.transform.position = PlayerTransform.position + 2 * Movement.forward;
                Hookshotdirection = (raycasthit.point - HookShotGO.transform.position).normalized;
                hasShot = true;
                HookShotRB.AddForce(Hookshotdirection * HookShotSpeed);
                StartCoroutine("Shootcoroutine");
            }
        }
    }
    public void Aim(InputAction.CallbackContext context)
    {
        if (!Movement.ducking && !Movement.endlag)
        {
            if (!hasHit)
            {
                Fadenkreuz.SetActive(true);
                if (!Movement.Jetpack.flying && !Movement.endlag)
                {
                    aiming = true;
                }
                else { aiming = false; }
            }
            else
            {
                if (HookShotScript.HookedToReachable || hasHit)
                {
                    if (!context.canceled && !Movement.endlag && permissionToReach)
                    {
                        reaching = true;
                    }
                    else
                    {
                        reaching = false;
                        permissionToReach = true;
                    }
                }
                else { reaching = false; permissionToReach = false; Fadenkreuz.SetActive(!true); }
            }
        }
        if (context.canceled) { aiming = false; Fadenkreuz.SetActive(!true); }
    }
    private void LateUpdate()
    {
        if (aiming)
        {
            shootingcameraGO.SetActive(true);
            walkingcameraFL.m_YAxis.Value = aimingcameraFL.m_YAxis.Value;
        }
        else
        {
            shootingcameraGO.SetActive(false);
            aimingcameraFL.m_YAxis.Value = walkingcameraFL.m_YAxis.Value;
        }
    }
    private void FixedUpdate()
    {
        if (HookShotScript.HookedToReachable || hasHit)
        {
            dragdirection = (AktuellsterEckpunkt - PlayerTransform.position).normalized;
            if (reaching)
            {
                PlayerRigidbody.AddForce(dragdirection * Time.deltaTime * 10000f);
                Movement.smoothingtime = 0.2f;
            }
            else
            {
                Movement.smoothingtime = 0.15f;
                permissionToReach = true;
            }
        }
        if (hasHit)
        {
            foreach (InteractionwithHookshot iwh in wrapped)
            {
                if (PlayerRigidbody.velocity.magnitude > 15)
                {
                    iwh.Ruck();
                }
            }
            if (hasHit)
            {
                currentropelength = maxropelength - Vector3.Distance(PlayerTransform.position, AktuellsterEckpunkt) - cumulativewrappedropelength;
            }
            if (currentropelength < 0f)
            {
                PlayerRigidbody.AddForce(dragdirection * 100f);
                foreach (InteractionwithHookshot iwh in wrapped)
                {
                    if (PlayerRigidbody.velocity.magnitude > 15)
                    {
                        iwh.Ruck();
                    }
                }
            }

        }
    }
    public void UnravelMethod()
    {
        if (eckpunkte.Count > 2)
        {
            if (!Physics.Raycast(transform.position, VorletzterEckpunkt - transform.position, out RaycastHit Unravel, Vector3.Distance(transform.position, VorletzterEckpunkt)))
            {
                foreach (InteractionwithHookshot iwh in touchedobjects)
                {
                    if (iwh.indexes.Contains(eckpunkte.Count / 2))
                    {
                        iwh.indexes.Remove(iwh.indexes[iwh.indexes.Count - 1]);
                        iwh.amountofropeconnections -= 1;
                    }
                }
                cumulativewrappedropelength -= Vector3.Distance(AktuellsterEckpunkt, VorletzterEckpunkt);
                AktuellsterEckpunkt = VorletzterEckpunkt;
                eckpunkte.Remove(eckpunkte[eckpunkte.Count - 1]);
                if (eckpunkte[eckpunkte.Count - 2] != null)
                {
                    VorletzterEckpunkt = eckpunkte[eckpunkte.Count - 2];
                }
                eckpunkte[eckpunkte.Count - 1] = AktuellsterEckpunkt;
                LineChange();
            }
        }
        if (eckpunkte.Count == 2)
        {
            if (!Physics.Raycast(wrist.position, VorletzterEckpunkt - wrist.position, out RaycastHit Unravel, Vector3.Distance(wrist.position, VorletzterEckpunkt) * 0.9f))
            {
                if (!Physics.Raycast(wrist.position, VorletzterEckpunkt + (VorletzterEckpunkt - AktuellsterEckpunkt) / 2 - wrist.position, out RaycastHit Unravel2, Vector3.Distance(wrist.position, VorletzterEckpunkt) * 0.9f))
                {
                    foreach (InteractionwithHookshot iwh in touchedobjects)
                    {
                        if (iwh.indexes.Contains(eckpunkte.Count / 2))
                        {
                            iwh.indexes.Remove(iwh.indexes[iwh.indexes.Count - 1]);
                            iwh.amountofropeconnections -= 1;
                        }
                    }
                    cumulativewrappedropelength -= Vector3.Distance(eckpunkte[1], eckpunkte[0]);
                    AktuellsterEckpunkt = eckpunkte[0];
                    eckpunkte.Remove(eckpunkte[1]);
                    LineChange();
                }
            }
        }
    }
    IEnumerator Shootcoroutine()
    {
        Movement.PlayerAnimator.SetFloat("ShootingStage", 1);
        Movement.walking = false;
        Movement.endlag = true;
        yield return new WaitForSeconds(0.4f);
        Movement.PlayerAnimator.SetFloat("ShootingStage", 2);
        yield return new WaitForSeconds(0.4f);
        Movement.PlayerAnimator.SetFloat("ShootingStage", 0);
        Movement.endlag = false;
    }
}