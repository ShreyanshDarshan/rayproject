using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float horizontalSensitivity = 1.0f;
    public float verticalSensitivity = 1.0f;
    public float flySensitivity = 1.0f;
    public float movementSpeed = 0.01f;
    public Transform playerbody;
    float mouseX;
    float mouseY;
    float xRotation;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        mouseX = Input.GetAxis("Mouse X") * horizontalSensitivity * Time.deltaTime;
        mouseY = Input.GetAxis("Mouse Y") * verticalSensitivity * Time.deltaTime;

        //xRotation -= Mathf.Lerp(xRotation, mousey);
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerbody.Rotate(Vector3.up * mouseX);

        playerbody.Translate(playerbody.InverseTransformDirection(transform.forward) * Input.GetAxis("Vertical") * movementSpeed * Time.deltaTime);
        playerbody.Translate(playerbody.InverseTransformDirection(playerbody.right) * Input.GetAxis("Horizontal") * movementSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.Space))
        {
            playerbody.Translate(playerbody.up * flySensitivity * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            playerbody.Translate(-playerbody.up * flySensitivity * Time.deltaTime);
        }
    }
}
