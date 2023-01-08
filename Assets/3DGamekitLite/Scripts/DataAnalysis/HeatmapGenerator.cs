using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
//using System.Numerics;
using UnityEngine;

public class HeatmapGenerator : MonoBehaviour
{
    //Range of IDs to take from
    [HideInInspector]
    public int maxIds;
    //Amount of IDs to store
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

    public class CubeClass
    {
        public GameObject cubePrefab;
        public Vector3 originPosition = new Vector3();
        public int size;
        public int value;


        public void InstantiateCube()
        {
            Instantiate(cubePrefab, originPosition, Quaternion.identity);
        }
    }

    // bools
    bool updateOnce = true;
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
        maxIds = GetNumberEvents();

        if (totalIdsToStore > maxIds)
        {
            Debug.LogError("Error! Too many ids to store. Please choose a lower number.");
        }
        else
        {
            Debug.Log("Downloading data...");
            while (numbersChosen.Count < totalIdsToStore)
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
                }
            }

            foreach (int number in numbersChosen)
            {
                OnGetEvent?.Invoke(number);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (heatmapDatas.Count >= totalIdsToStore)
        {
            canUpdate = true;
            Debug.Log("Data downloaded!");
        }

        if (canUpdate)
        {
            if (updateOnce)
            {
                Debug.Log("Printing Sample Info");
                heatmapDatas[1].PrintInfo();
                updateOnce = false;
            }
        }

        // call the function here with the map that we wnat to show as a int

    }

    private void ShowMovementMap(int id)
    {
        switch (id)
        {
            case 1: // attack Map
                {
                    int cubeSize = 1; // TODO: let it change it manually, add a key to generate map
                    GenerateCubesMap(cubeSize);

                    foreach(CubeClass cube in cubesList)
                    {
                        cube.value = DetermineCubeColor(cube);
                        cube.InstantiateCube();
                    }

                }
                break;
            default:
                break;
        }
    }

    private int DetermineCubeColor(CubeClass cubeP)
    {
        int max = 0; // TODO




        return 0;
    }

    private void GenerateCubesMap(int size) // this fills the cubeList with the correct amount of cubes and sizes/positions. The value of the color will be calculated later
    {

        // The map is 120x80, with the corners being: -33, 40; -33, -40; 94,-40; 94,40 (counter-clockwise)
        // fill an array with all the possible cubes with the given size.
        // Vector3[] map = new Vector3[];
        // return map;

        int mapWidth = 120;
        int mapHeight = 80;

        int totalCubes = 0; //TODO

        // Create an array


        //for(int i = 0; i < totalCubes; i++)
        //{
        //    cubesList[i].originPosition = 0; //TODO
        //}

    }

    private int CalculateNumber(Vector3 pos)
    {
        // this has to calculate a normalized number (bet. 0 - 1) and return it
       
        int min = 0; // default min is always 0
        //int max = CalculateMax; // TODO

        return 1;
    }
}
