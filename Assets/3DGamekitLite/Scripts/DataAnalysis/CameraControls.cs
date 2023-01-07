using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CameraControls : MonoBehaviour
{

    // horizontal rotation speed
    public float horizontalSpeed = 1f;
    // vertical rotation speed
    public float verticalSpeed = 1f;
    private float xRotation = 0.0f;
    private float yRotation = 0.0f;

    //Camera
    public Camera cam;

    //UI Text
    public TMPro.TMP_Text text;

    //Movement speed
    private float movementSpeed = 10;

    void Update()
    {
        //Rotation
        float mouseX = Input.GetAxis("Mouse X") * horizontalSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * verticalSpeed;
        yRotation += mouseX;
        xRotation -= mouseY;
        cam.transform.eulerAngles = new Vector3(xRotation, yRotation, 0.0f);

        //Movement
        if (Input.GetKey(KeyCode.LeftShift))
        {
            movementSpeed = 20;
        }
        else { movementSpeed = 10; }

        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(new Vector3(movementSpeed * Time.deltaTime, 0, 0));
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.Translate(new Vector3(-movementSpeed * Time.deltaTime, 0, 0));
        }
        if (Input.GetKey(KeyCode.E))
        {
            transform.Translate(new Vector3(0, -movementSpeed * Time.deltaTime, 0));
        }
        if (Input.GetKey(KeyCode.Q))
        {
            transform.Translate(new Vector3(0, movementSpeed * Time.deltaTime, 0));
        }
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(new Vector3(0, 0, movementSpeed * Time.deltaTime));
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(new Vector3(0, 0, -movementSpeed * Time.deltaTime));
        }

        //Data Controls     
        if (Input.GetKey(KeyCode.Alpha1))
        { text.text = "Movement heatmap selected."; }
        if (Input.GetKey(KeyCode.Alpha2))
        { text.text = "Attack heatmap selected."; }
        if (Input.GetKey(KeyCode.Alpha3))
        { text.text = "Jump heatmap selected."; }
        if (Input.GetKey(KeyCode.Alpha4))
        { text.text = "Enemy hits heatmap selected."; }
        if (Input.GetKey(KeyCode.Alpha5))
        { text.text = "Enemy kills heatmap selected."; }
        if (Input.GetKey(KeyCode.Alpha6))
        { text.text = "Damage taken heatmap selected."; }
        if (Input.GetKey(KeyCode.Alpha7))
        { text.text = "Death heatmap selected."; }
    }
}
