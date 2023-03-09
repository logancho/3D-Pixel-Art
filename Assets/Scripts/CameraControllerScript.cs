using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraControllerScript : MonoBehaviour
{
    [SerializeField] UniversalRendererData feature;
    [SerializeField] Camera orthoCamera;

    [SerializeField] float rotation = 30.0f;
    [SerializeField] float rotationUnit = 5.0f;
    PixelizeFeature pixelateFeature;

    private float UnitPerPixel = 0;
    private float UnitPerPixelHorizontal = 0;
    // Start is called before the first frame update
    private float timer = 0;
    [SerializeField] float waitTime = 0.2f;


    private Vector3 forward, right;
    void Start()
    {
        //Initialize our reference to pixelateFeature
        pixelateFeature = (PixelizeFeature)feature.rendererFeatures[1];
        transform.rotation = Quaternion.Euler(rotation, transform.eulerAngles.y, transform.eulerAngles.z);
        //
        forward = new Vector3(0, 0, 1);
        right = new Vector3(1, 0, 0);
    }

    void FixedUpdate()
    {
        
        //UnitPerPixel = 0.2f;
        //Movement
        if (Input.GetAxisRaw("Horizontal") < -0 && timer > waitTime)
        {
            transform.position += right * -UnitPerPixelHorizontal;
            //Debug.Log(right * -UnitPerPixelHorizontal);
            //transform.position += new Vector3(-UnitPerPixelHorizontal, 0, 0);
            timer = 0;
        }
        if (Input.GetAxisRaw("Horizontal") > 0 && timer > waitTime)
        {
            transform.position += right * UnitPerPixelHorizontal;
            //Debug.Log(right * UnitPerPixelHorizontal);
            //transform.position += new Vector3(UnitPerPixelHorizontal, 0, 0);
            timer = 0;
        }
        if (Input.GetAxisRaw("Vertical") < 0 && timer > waitTime)
        {
            transform.position += forward * -UnitPerPixel;
            //transform.position += new Vector3(0, 0, -UnitPerPixel);
            timer = 0;
        }
        if (Input.GetAxisRaw("Vertical") > 0 && timer > waitTime)
        {
            transform.position += forward * UnitPerPixel;
            //transform.position += new Vector3(0, 0, UnitPerPixel);
            timer = 0;
        }

        //Rotation

        if (Input.GetKey(KeyCode.Q) && timer > waitTime)
        {
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + rotationUnit, transform.eulerAngles.z);
            right = Vector3.Normalize(Quaternion.Euler(0, rotationUnit, 0) * right);
            //Debug.Log(right);
            forward = Vector3.Normalize(Quaternion.Euler(0, rotationUnit, 0) * forward);
            timer = 0;
        }
        if (Input.GetKey(KeyCode.E) && timer > waitTime)
        {
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y - rotationUnit, transform.eulerAngles.z);
            right = Vector3.Normalize(Quaternion.Euler(0, -rotationUnit, 0) * right);
            //Debug.Log(right);
            forward = Vector3.Normalize(Quaternion.Euler(0, -rotationUnit, 0) * forward);
            timer = 0;
        }


        //Need to improve snapping!!! - to include rotation!

        //Translate camera along axis by a singular increment of UnitPerPixel
        //if (UnitPerPixel != 0 && UnitPerPixelHorizontal != 0)
        //{
        //    transform.position = GetSnappedPosition(transform.position, 1.0f / UnitPerPixel, 1.0f / UnitPerPixelHorizontal);
        //}
        
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (transform.rotation.eulerAngles.x != rotation)
        {
            transform.rotation = Quaternion.Euler(rotation, transform.eulerAngles.y, transform.eulerAngles.z);
        }
        UpdateUPP();
        //We need to include trigonometry... the angle of the camera!
        //Debug.Log(UnitPerPixel);
    }

    void UpdateUPP()
    {
        int pixelCount = pixelateFeature.m_ScriptablePass.pixelScreenHeight;
        //Debug.Log(pixelCount);
        if (pixelCount != 0)
        {
            float HeightInUnits = orthoCamera.orthographicSize * 2.0f / Mathf.Sin(Mathf.Deg2Rad * rotation);
            //float HeightInUnits = orthoCamera.orthographicSize * 2.0f * Mathf.Sin(Mathf.Deg2Rad * rotation);
            UnitPerPixel = HeightInUnits / ((float)pixelCount);
            UnitPerPixelHorizontal = orthoCamera.orthographicSize * 2.0f / ((float)pixelCount);
        }
    }

    Vector3 GetSnappedPosition(Vector3 position, float snapPPU = 1f, float snapPPUH = 1f)
    {
        float x = Mathf.Round(position.x * snapPPUH) / snapPPUH;
        float z = Mathf.Round(position.z * snapPPU) / snapPPU;
        return new Vector3(x, position.y, z);
    }
}
