using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
//using System.Numerics;
using UnityEngine;
using static Gamekit3D.EffectStateMachineBehavior;

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
    public List<GameObject> cubesInstantiated = new List<GameObject>();

    [HideInInspector] public string url = "https://citmalumnes.upc.es/~sergicf4/";
    [HideInInspector] public string sUrl = "GetEvent.php";
    [HideInInspector] public string nUrl = "GetNumberEvents.php";

    public static Action<int> OnGetEvent;

    public GameObject cubePrefab;

    // Map size

    int mapWidth = 190;
    int mapLength = 140;
    int mapHeigth = 20;

    public int cubeSize = 1; // TODO: let it change it manually, add a key to generate map

    // Text UI
    public TMPro.TMP_Text text;

    public class CubeClass
    {
        public GameObject classCubePrefab;
        public Vector3 originPosition = new Vector3();
        public int size;
        public Vector3 colorValues;
        public bool isActive;


        public GameObject InstantiateCube()
        {
            return Instantiate(classCubePrefab, originPosition, Quaternion.identity);

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
            //Data Controls     
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                text.text = "Movement heatmap selected.";
                ShowMap(eventType.movement);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                text.text = "Attack heatmap selected.";
                ShowMap(eventType.attack);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                text.text = "Jump heatmap selected.";
                ShowMap(eventType.jump);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                text.text = "Enemy hits heatmap selected.";
                ShowMap(eventType.hitEnemy);
            }
            if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                text.text = "Enemy kills heatmap selected.";
                ShowMap(eventType.killEnemy);
            }
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                text.text = "Damage taken heatmap selected.";
                ShowMap(eventType.recieveDamage);
            }
            if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                text.text = "Death heatmap selected.";
                ShowMap(eventType.death);
            }

            //if (updateOnce)
            //{
            //    Debug.Log("Printing Sample Info");
            //    heatmapDatas[1].PrintInfo();
            //    updateOnce = false;
            //}
        }
    }

    private void ShowMap(eventType type)
    {
        DeleteInstantiatedCubes();
        switch (type)
        {
            case eventType.movement: // attack Map
                {
                    Debug.Log("Generating map");
                    GenerateCubesMap(cubeSize);

                    foreach (CubeClass cube in cubesList)
                    {
                        cube.colorValues = DetermineCubeColor(cube);
                        // TODO bool isActive
                        GameObject temp = cube.InstantiateCube();
                        temp.transform.localScale = new Vector3(cube.size, cube.size, cube.size);
                        temp.GetComponent<Renderer>().material.color = new Color(cube.colorValues.x, cube.colorValues.y, cube.colorValues.z, 0.25f);
                        cubesInstantiated.Add(temp);
                    }
                }
                break;
            default:
                break;
        }
    }

    private Vector3 DetermineCubeColor(CubeClass cubeP)
    {
        int max = 0; // TODO


        return Vector3.zero;
    }

    private void GenerateCubesMap(int size) // this fills the cubeList with the correct amount of cubes and sizes/positions. The value of the color will be calculated later
    {

        // The map is 120x80, with the corners being: -33, 40; -33, -40; 94,-40; 94,40 (counter-clockwise)
        // fill an array with all the possible cubes with the given size.

        int topLeftX = -60 - (size / 2);
        int topLeftZ = 70 - (size / 2);

        int bottomLevel = -3;
        int topLevel = 9;

        int totalSizeX = mapWidth / size;
        int totalSizeZ = mapLength / size;
        int totalSizeY = (- bottomLevel + topLevel) / size;

        for (int k = 0; k <= totalSizeY; k++)
        {
            for (int i = 0; i < totalSizeX; i++)
            {
                for (int j = 0; j < totalSizeZ; j++)
                {
                    CubeClass temp = new CubeClass();
                    temp.classCubePrefab = cubePrefab;
                    temp.originPosition.Set(topLeftX + (size * i), bottomLevel + (size*k), topLeftZ - (size * j));
                    temp.size = size;
                    cubesList.Add(temp);

                }
            }
        }
    }

    private void DeleteInstantiatedCubes()
    {
        if (cubesInstantiated.Count > 0)
        {
            foreach (GameObject toDestroy in cubesInstantiated)
            {
                Destroy(toDestroy);
            }
            cubesInstantiated.Clear();
            cubesList.Clear();
        }
    }
}
