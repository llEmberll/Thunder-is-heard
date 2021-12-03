using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Movement : MonoBehaviour
{
    private int screenWidth;
    private int screenHeight;

    public float speed;
    public float minX;
    public float maxX;
    public float minZ;
    public float maxZ;
    public bool useCameraMovement;

    // Start is called before the first frame update
    void Start()
    {
        screenWidth = Screen.width;
        screenHeight = Screen.height;
    }

    // Update is called once per frame
    void Update()
    {
        Camera cam = GetComponent<Camera>();

        Vector3 camPos = cam.transform.position;

        float mw = Input.GetAxis("Mouse ScrollWheel");

        float minSize = 1.8f;
        float maxSize = 4.2f;

        if (mw > 0f)
        {
            if (cam.orthographicSize < maxSize)
            {
                cam.orthographicSize += Time.deltaTime * 25f;
            }
            else cam.orthographicSize = maxSize;


        }
        else if (mw < 0f)
        {
            if (cam.orthographicSize > minSize)
            {
                cam.orthographicSize -= Time.deltaTime * 25f;
            }
            else cam.orthographicSize = minSize;
        }
    


        if (Input.mousePosition.x <= 20)
        {
            camPos.x -= Time.deltaTime * speed;
            camPos.z += Time.deltaTime * speed;

        }

        else if (Input.mousePosition.x >= screenWidth - 20)
        {
            camPos.x += Time.deltaTime * speed;
            camPos.z -= Time.deltaTime * speed;
        }

        else if (Input.mousePosition.y <= 20)
        {
            camPos.x -= Time.deltaTime * speed;
            camPos.z -= Time.deltaTime * speed;

        }

        else if (Input.mousePosition.y >= screenHeight - 20)
        {
            camPos.x += Time.deltaTime * speed;
            camPos.z += Time.deltaTime * speed;

        }

        if (useCameraMovement)
        {
            transform.position = new Vector3(Mathf.Clamp(camPos.x, minX, maxX), camPos.y, Mathf.Clamp(camPos.z, minZ, maxZ));
        }
        
    }
}
