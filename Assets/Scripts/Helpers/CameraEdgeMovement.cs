using UnityEngine;
using UnityEditor;

public class CameraEdgeMovement : MonoBehaviour
{
    public int Boundary = 5;
    public int speed = 5;

    private int screenWidth;
    private int screenHeight;

    void Start()
    {
        screenWidth = Screen.width;
        screenHeight = Screen.height;
    }

    void Update()
    {
        float sumX = transform.position.x;
        float sumZ = transform.position.z;

        if (Input.mousePosition.x > screenWidth - Boundary)
        {
            sumX += speed * Time.deltaTime;
        }

        if (Input.mousePosition.x < 0 + Boundary)
        {
            sumX -= speed * Time.deltaTime;
        }

        if (Input.mousePosition.y > screenHeight - Boundary)
        {
            sumZ += speed * Time.deltaTime;
        }

        if (Input.mousePosition.y < 0 + Boundary)
        {
            sumZ -= speed * Time.deltaTime;
        }

        transform.position = new Vector3(sumX, transform.position.y, sumZ);
    }

}