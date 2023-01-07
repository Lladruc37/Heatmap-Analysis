using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatmapGenerator : MonoBehaviour
{
    //Range of IDs to take from
    [Range(1,140)]
    public int maxIds;
    //Amount of IDs to store
    [Range(1,140)]
    public int totalIdsToStore;

    //DataList
    [HideInInspector] public List<int> numbersChosen = new List<int>();
    public List<HeatmapData> heatmapDatas = new List<HeatmapData>();

    [HideInInspector] public string url = "https://citmalumnes.upc.es/~sergicf4/";
    [HideInInspector] public string sUrl = "GetEvent.php";

    public static Action<int> OnGetEvent;

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

    // Start is called before the first frame update
    void Start()
    {
        while (numbersChosen.Count<totalIdsToStore)
        {
            bool isDupe = false;

            int luckyNumber = UnityEngine.Random.Range(1, maxIds);

            foreach(int number in numbersChosen)
            {
                if (number == luckyNumber) { isDupe = true; }
            }

            if(!isDupe)
            {
                numbersChosen.Add(luckyNumber);
            }
        }

        foreach (int number in numbersChosen)
        {
            OnGetEvent?.Invoke(number);
        }

    }

    // Update is called once per frame
    void Update()
    {
        if(heatmapDatas.Count >= totalIdsToStore)
        {
            canUpdate = true;
            
        }

        if(canUpdate)
        {
            if (updateOnce)
            {
                Debug.Log("printing Info");
                heatmapDatas[1].PrintInfo();
                updateOnce = false;
            }
        }
    }

    private void OnValidate()
    {
        if(totalIdsToStore>maxIds)
        { totalIdsToStore = maxIds; }
    }
}
