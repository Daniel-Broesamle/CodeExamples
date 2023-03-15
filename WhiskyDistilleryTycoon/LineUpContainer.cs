using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineUpContainer : MonoBehaviour
{
    public static LineUpContainer instance;
    public RaycastHit raycasthit;
    public Camera Cam;
    [SerializeField] public static List<Building> BuildingsToLink = new List<Building>();
    [SerializeField] public List<GameObject> UIPointstoconnect = new List<GameObject>();
    public int RecentID;
    public int IDdesangeklicktenGebaudes;
    public bool NewLineStarted = false;
    public bool Cancelable = false;
    public Line aktuellekette;
    //public LineRenderer[] arrayofinactiveLinerenderers = new LineRenderer[0];
    public List<LineRenderer> arrayofinactiveLinerenderers = new List<LineRenderer>();
    public LineRenderer redlineconnectingmouseandlastaktuellelinemember;
    public LineRenderer greenlineconnectingaktuelleline;
    public LineRenderer aktuellerInaktiverLinerenderer;
    public Vector3[] aktuelleListePositions = new Vector3[0];
    public bool currentlineCompleted;
    public bool giveoptionstocompleteline;
    public GameObject completelistbutton;
    public GameObject deletebuton;
    public GameObject cancellistbutton; //public void Cancelbutton(){linestate starten; aktuellekette leeren;}
    public void Start()
    {
        greenlineconnectingaktuelleline.startWidth = 2f;
        greenlineconnectingaktuelleline.endWidth = 2f;
        redlineconnectingmouseandlastaktuellelinemember.startWidth = 2f;
        redlineconnectingmouseandlastaktuellelinemember.endWidth = 2f;
        aktuellerInaktiverLinerenderer = arrayofinactiveLinerenderers[0];
        aktuellerInaktiverLinerenderer.SetWidth(2.0f, 2.0f);
    }
    private void Awake()
    {
        // standart singleton pattern
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }


    }
    public void Completethelist()
    {
        completelistbutton.SetActive(false);
        cancellistbutton.SetActive(false);
        currentlineCompleted = true;
    }
    public void Cancelbutton()
    {
        //linestate starten; 
        //aktuellekette leeren;
        if (aktuellekette.line.Count > 0)
        {
            aktuellekette.line.Clear();
            StorageManager.instance.linesList.RemoveAt(StorageManager.instance.linesList.Count - 1);
        }
        StateMachine.instance.OnChainFinish = true;
    }
    public void Cancelbutton2()
    {
        aktuellekette.line.Clear();
        StorageManager.instance.linesList.RemoveAt(StorageManager.instance.linesList.Count - 1);
        if (StateMachine.instance.currentState != StateMachine.instance.chainState)
        {
            if (aktuellekette.line.Count > 0)
            {
            }
            StateMachine.instance.OnChainFinish = true;
        }
    }
    public void Cancelall()
    {
        StorageManager.instance.linesList.Clear();
        for (int i = 1; i < arrayofinactiveLinerenderers.Count - 1; i++)
        {
            Destroy(arrayofinactiveLinerenderers[i].gameObject);
        }
        //aktuellerInaktiverLinerenderer.positionCount = 0;
        for (int i = 0; i < aktuellerInaktiverLinerenderer.positionCount - 1; i++)
        {
            aktuellerInaktiverLinerenderer = arrayofinactiveLinerenderers[0];
        }
        aktuellekette.line.Clear();
        for (int i = 0; i < aktuelleListePositions.Length - 1; i++)
        {
            aktuelleListePositions[i] = Vector3.zero;
        }
        StateMachine.instance.onLineToolClick = true;
    }
}
