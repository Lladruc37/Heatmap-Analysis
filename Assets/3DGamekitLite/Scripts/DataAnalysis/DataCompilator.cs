using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum eventType
{
    _null = -1,
    position,
    hit,
    death,
    kill
}
public class HeatmapData
{
    DateTime dateTime;
    eventType type;
    uint playerId; //Random number (0 - 9)
    uint sessionId; //Return from primary key when starting
    Vector3 position;

    public HeatmapData(DateTime dateTime, eventType type, uint playerId, uint sessionId, Vector3 position)
    {
        this.dateTime = dateTime;
        this.type = type;
        this.playerId = playerId;
        this.sessionId = sessionId;
        this.position = position;
    }
    public string GetData()
    {
        string data = "dateTime=" + dateTime.ToString("yyyy-MM-dd HH:mm:ss") 
            + "&type=" + (int)type 
            + "&playerId=" + playerId 
            + "&sessionId=" + sessionId 
            + "&positionX=" + position.x
            + "&positionY=" + position.y
            + "&positionZ=" + position.z;
        return data;
    }
}

public class SessionClass : MonoBehaviour
{
    public DateTime dateTime;
    public uint id;

    public SessionClass(DateTime dateTime, uint id)
    {
        this.dateTime = dateTime;
        this.id = id;
    }

    public string GetData()
    {
        string data = "dateTime=" + dateTime.ToString("yyyy-MM-dd HH:mm:ss") + "&id=" + id;
        return data;
    }
}

public class DataCompilator : MonoBehaviour
{
    uint playerId = 0;
    uint currentSession = 0;

    public string url = "https://citmalumnes.upc.es/~sergicf4/";
    public string sUrl = "AddSessionGameplay.php";
    public string fUrl = "FinishSessionGameplay.php";
    public string eUrl = "AddEvent.php";

    public static Action<DateTime, eventType, uint, uint, Vector3> OnNewEvent;
    public static Action<DateTime> OnNewSession;
    public static Action<DateTime> OnEndSession;

    private void OnEnable()
    {
        OnNewEvent += NewEvent;
        OnNewSession += NewSession;
        OnEndSession += EndSession;
    }
    // Start is called before the first frame update
    void Start()
    {
        playerId = (uint)UnityEngine.Random.Range(0, 9);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void NewEvent(DateTime dateTime, eventType type, uint playerId, uint sessionId, Vector3 position)
    {
        HeatmapData hmBuffer = new HeatmapData(dateTime, type, playerId, sessionId, position);
        StartCoroutine(Event2PHP(hmBuffer));
    }
    IEnumerator Event2PHP(HeatmapData d)
    {
        string dataUrl = url + eUrl + "?" + d.GetData();
        Debug.Log(dataUrl);
        WWW www = new WWW(dataUrl);

        yield return www;

        if (www.error == null)
        {
            Debug.Log(www.text);
            uint eventId = uint.Parse(www.text);
        }
        else
        {
            Debug.Log(www.error);
        }
    }

    private void NewSession(DateTime dateTime)
    {
        SessionClass sBuffer = new SessionClass(dateTime, playerId);
        StartCoroutine(SessionStart2PHP(sBuffer));
    }

    IEnumerator SessionStart2PHP(SessionClass s)
    {
        string dataUrl = url + sUrl + "?" + s.GetData();
        Debug.Log(dataUrl);
        WWW www = new WWW(dataUrl);

        yield return www;

        if (www.error == null)
        {
            Debug.Log(www.text);
            currentSession = uint.Parse(www.text);
        }
        else
        {
            Debug.Log(www.error);
        }
    }

    private void EndSession(DateTime dateTime)
    {
        SessionClass sBuffer = new SessionClass(dateTime, playerId);
        StartCoroutine(SessionEnd2PHP(sBuffer));
    }

    IEnumerator SessionEnd2PHP(SessionClass s)
    {
        string dataUrl = url + fUrl + "?" + s.GetData();
        Debug.Log(dataUrl);
        WWW www = new WWW(dataUrl);

        yield return www;

        if (www.error == null)
        {
            Debug.Log(www.text);
            currentSession = uint.Parse(www.text);
        }
        else
        {
            Debug.Log(www.error);
        }
    }

}
