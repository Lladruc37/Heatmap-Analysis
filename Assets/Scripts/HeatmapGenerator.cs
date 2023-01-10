using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
//using System.Numerics;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
using static Gamekit3D.EffectStateMachineBehavior;

public class HeatmapGenerator : MonoBehaviour
{
    //Range of IDs to take from
    [HideInInspector]
    public int maxIds;
    //Amount of IDs to store
    public bool getAllEvents;
    public int totalIdsToStore;

    // Lists needed
    [HideInInspector] public List<int> numbersChosen = new List<int>();
    public List<HeatmapData> heatmapDatas = new List<HeatmapData>();

    public List<CubeClass> cubesList = new List<CubeClass>();

    [HideInInspector] public string url = "https://citmalumnes.upc.es/~sergicf4/";
    [HideInInspector] public string sUrl = "GetEvent.php";
    [HideInInspector] public string nUrl = "GetNumberEvents.php";

    public static Action<int> OnGetEvent;

    public GameObject cubePrefab;

    // Map size

    int mapWidth = 190;
    int mapLength = 140;

    Dictionary<eventType, int> maxEvents = new Dictionary<eventType, int>();

    public int granularity = 50;

    // To change the color palette of the cubes

    public Color cubeColorA = Color.green;
    public Color cubeColorB = Color.red;


    // Text UI
    public TMPro.TMP_Text text;

    public class CubeClass
    {
        public GameObject classCubePrefab;
        public Vector3 originPosition = new Vector3();
        public int size;
        public Vector3 colorValues;
        public Color color;
        public float value;
        public Dictionary <eventType, int> nEvents = new Dictionary<eventType, int>();

        public GameObject instance = null;

        public CubeClass()
        {  
            nEvents.Add(eventType.movement, 0);
            nEvents.Add(eventType.attack, 0);
            nEvents.Add(eventType.jump, 0);
            nEvents.Add(eventType.hitEnemy,0);
            nEvents.Add(eventType.killEnemy,0);
            nEvents.Add(eventType.recieveDamage,0);
            nEvents.Add(eventType.death,0);
        }
        public void InstantiateCube()
        {
            instance = Instantiate(classCubePrefab, originPosition, Quaternion.identity);
            instance.transform.localScale = new Vector3(size, size, size);
        }

        public void DestroyCube()
        {
            if (instance != null)
            {
                Destroy(instance);
                instance = null;
            }
        }
    }

    // bools
    bool updatePartOne = true;
    bool updatePartTwo = false;
    bool canUpdate = false;


    private void OnEnable()
    {
        OnGetEvent += GetEvent;
    }
    private void OnDisable()
    {
        OnGetEvent -= GetEvent;
    }

    public void GetEvent(int id)
    {
        StartCoroutine(PHP2Event(id));
    }

    IEnumerator PHP2Event(int u)
    {
        string dataUrl = url + sUrl + "?eventId=" + u;
        //Debug.Log(dataUrl);
        WWW www = new WWW(dataUrl);

        yield return www;

        if (www.error == null)
        {
            //Debug.Log(www.text);
            string[] tmp = www.text.Split('>');

            HeatmapData tempHeatMap = new HeatmapData();
            tempHeatMap.dateTime = DateTime.Parse(tmp[0]);
            int tempInt = int.Parse(tmp[1]);
            tempHeatMap.type = (eventType)tempInt;
            tempHeatMap.playerId = uint.Parse(tmp[2]);
            tempHeatMap.sessionId = uint.Parse(tmp[3]);

            float x = float.Parse(tmp[4]);
            float y = float.Parse(tmp[5]);
            float z = float.Parse(tmp[6]);

            tempHeatMap.position.x = x;
            tempHeatMap.position.y = y;
            tempHeatMap.position.z = z;

            heatmapDatas.Add(tempHeatMap);
        }
        else
        {
            Debug.LogError("Error: " + www.error);
        }
    }

    int GetNumberEvents()
    {
        string dataUrl = url + nUrl;
        Debug.Log(dataUrl);
        WWW www = new WWW(dataUrl);

        while (!www.isDone) { }

        if (www.error == null)
        {
            Debug.Log(www.text);
            return int.Parse(www.text);
        }
        else
        {
            Debug.LogError("Error: " + www.error);
        }

        Debug.Log("Session has ended...");
        return 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        canUpdate = false;
        maxIds = GetNumberEvents();

        maxEvents.Add(eventType.movement, 0);
        maxEvents.Add(eventType.attack, 0);
        maxEvents.Add(eventType.jump, 0);
        maxEvents.Add(eventType.hitEnemy, 0);
        maxEvents.Add(eventType.killEnemy, 0);
        maxEvents.Add(eventType.recieveDamage, 0);
        maxEvents.Add(eventType.death, 0);

        //if (getAllEvents)
        //{
        //    totalIdsToStore = maxIds;
        //}

        if (totalIdsToStore > maxIds)
        {
            Debug.LogError("Error! Too many ids to store. Please choose a lower number.");
            text.text = "Error! Too many ids to store. Please choose a lower number.";
        }
        else
        {
            Debug.Log("Getting events...");
            text.text = "Downloading data...";
            //if (getAllEvents)
            //{
            //    for (int i = 0; i != totalIdsToStore; i++)
            //    {
            //        OnGetEvent?.Invoke(i);
            //    }
            //}
            //else
            //{
            while (numbersChosen.Count != totalIdsToStore)
            {
                bool isDupe = false;

                int luckyNumber = UnityEngine.Random.Range(1, maxIds);

                foreach (int number in numbersChosen)
                {
                    if (number == luckyNumber) { isDupe = true; }
                }

                if (!isDupe)
                {
                    numbersChosen.Add(luckyNumber);
                    OnGetEvent?.Invoke(luckyNumber);
                }
            }
            Debug.Log("Total count: " + numbersChosen.Count);
            //}

        }
    }

    // Update is called once per frame
    void Update()
    {
        if (heatmapDatas.Count >= totalIdsToStore)
        {
            if (!canUpdate)
			{
                if (updatePartTwo && !updatePartOne)
                {
                    text.text = "Grid created! Generating map...";
                    Debug.Log("Grid created! Generating map...");
                    CreateMap();
                    updatePartTwo = false;
                }

                if (updatePartOne && !updatePartTwo)
                {
                    updatePartOne = false;
                    text.text = "Data downloaded! Creating grid...";
                    Debug.Log("Data downloaded! Creating grid...");
                    GenerateCubesGrid(granularity);
                    updatePartTwo = true;
                }

                if (!updatePartOne && !updatePartTwo)
                {
                    text.text = "Press 1 - 9 to select heatmap!";
                    Debug.Log("Maps generated!");
                    canUpdate = true;
                }
            }
        }

        if (canUpdate)
        {
            //Data Controls     
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                canUpdate = false;
                ShowMap(eventType.movement);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                canUpdate = false;
                ShowMap(eventType.attack);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                canUpdate = false;
                ShowMap(eventType.jump);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                canUpdate = false;
                ShowMap(eventType.hitEnemy);
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                canUpdate = false;
                ShowMap(eventType.killEnemy);
            }
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                canUpdate = false;
                ShowMap(eventType.recieveDamage);
            }
            if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                canUpdate = false;
                ShowMap(eventType.death);
            }
        }
    }

    private void ShowMap(eventType type)
    {
        text.text = "Creating map: " + GetNameEvent(type);
        DeleteInstantiatedCubes();

        foreach (CubeClass cube in cubesList)
        {
            if (cube.nEvents[type] > 0) // Only instantiate cubes with meaningful information
            {
                cube.InstantiateCube();
                cube.value = (float)cube.nEvents[type] / (float)maxEvents[type];
                cube.color = Color.Lerp(cubeColorA, cubeColorB, cube.value);
                cube.color.a = 0.75f;
                cube.instance.GetComponent<Renderer>().material.color = cube.color;
            }
        }

        text.text = "Heatmap selected: " + GetNameEvent(type);
        canUpdate = true;
    }

    private string GetNameEvent(eventType type)
    {
        switch (type)
        {
            case eventType.movement:
                return "Movement";
            case eventType.attack:
                return "Attack";
            case eventType.jump:
                return "Jump";
            case eventType.hitEnemy:
                return "HitEnemy";
            case eventType.killEnemy:
                return "KillEnemy";
            case eventType.recieveDamage:
                return "RecieveDamage";
            case eventType.death:
                return "Death";
            default:
                Debug.Log("This should never happen...");
                return "null";
        }
    }

    private int GetEventsInCube(CubeClass cube, eventType type)
    {
        int events = 0;

        foreach (HeatmapData data in heatmapDatas)
        {
            if (data.type == type)
            {
                if (cube.instance != null)
                {
                    if (cube.instance.GetComponent<Collider>().bounds.Contains(data.position))
                    {
                        events++;
                    }
                }
            }
        }

        return events;
    }

    private void GenerateCubesGrid(int size) // this fills the cubeList with the correct amount of cubes and sizes/positions. The value of the color will be calculated later
    {

        // The map is 120x80, with the corners being: -33, 40; -33, -40; 94,-40; 94,40 (counter-clockwise)
        // fill an array with all the possible cubes with the given size.

        int topLeftX = -60 - (size / 2);
        int topLeftZ = 70 - (size / 2);

        int bottomLevel = -3;
        int topLevel = 9;

        int totalSizeX = mapWidth / size;
        int totalSizeZ = mapLength / size;
        int totalSizeY = (-bottomLevel + topLevel) / size;
        if (totalSizeY <= 0) totalSizeY = 1; // So at least we have one cube?

        for (int k = 0; k <= totalSizeY; k++)
        {
            for (int i = 0; i < totalSizeX; i++)
            {
                for (int j = 0; j < totalSizeZ; j++)
                {
                    CubeClass temp = new CubeClass();
                    temp.classCubePrefab = cubePrefab;
                    temp.originPosition.Set(topLeftX + (size * i), bottomLevel + (size * k), topLeftZ - (size * j));
                    temp.size = size;
                    temp.InstantiateCube();
                    cubesList.Add(temp);
                }
            }
        }
    }

    private void CreateMap()
    {
        List<CubeClass> newList = new List<CubeClass>();
        foreach (CubeClass cube in cubesList)
        {
            Dictionary<eventType, int> tempDictionary = new Dictionary<eventType, int>();

            foreach (KeyValuePair<eventType, int> type in cube.nEvents)
            {
                int events = GetEventsInCube(cube, type.Key);

                if (events > 0)
                {
                    if (!newList.Contains(cube))
                    {
                        newList.Add(cube);
                    }
                }

                if (events > maxEvents[type.Key])
                {
                    maxEvents[type.Key] = events;
                }

                tempDictionary.Add(type.Key, events);
            }

            cube.nEvents = tempDictionary;
        }

        DeleteInstantiatedCubes();
        cubesList.Clear();
        cubesList = newList;

        Debug.Log("Data finalized: Count List: " + cubesList.Count);
        foreach (CubeClass cube in cubesList)
        {
            List<int> counts = new List<int>();
            foreach (KeyValuePair<eventType, int> type in cube.nEvents)
            {
                counts.Add(type.Value);
            }

            if (!counts.Any(p => p != 0))
			{
                Debug.Log(counts[0] + " - " + counts[1] + " - " + counts[2] + " - " + counts[3] + " - " + counts[4] + " - " + counts[5] + " - " + counts[6]);
			}
        }
    }

    private void DeleteInstantiatedCubes()
    {
        foreach (CubeClass cube in cubesList)
        {
            cube.DestroyCube();
        }
    }
}
