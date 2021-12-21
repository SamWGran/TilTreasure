using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    //Variables for drawing on screen
    [SerializeField]
    Sprite[] maps;
    [SerializeField]
    GameObject cross;
    private Image map;
    private float screenX, screenY;
    private RectTransform canvasRect;

    // Gyroscope related variables
    Quaternion gyro;
    Quaternion offset;
    Vector2 coords = new Vector2();
    private float angleSpan = 180;

    // Game variables
    private Vector2[] points = new Vector2[3];
    private int currentPoint = -1;
    private AudioSource successAudio;


    void Awake() {
        map = GetComponent<Image>();
        map.sprite = maps[Random.Range(0, maps.Length-1)];
        successAudio = gameObject.GetComponent<AudioSource>();

        canvasRect = cross.transform.parent.gameObject.GetComponent<Canvas>().GetComponent<RectTransform>();
        screenX = canvasRect.rect.width;
        screenY = canvasRect.rect.height;
        StartInstance();
    }

    void Start() {
        Gyroscope phoneGyro = Input.gyro;
        phoneGyro.enabled = true;

        points[0] = new Vector2(0.2f, 0.2f);
        points[1] = new Vector2(0.5f, 0.8f);
        points[2] = new Vector2(0.2f, 0.8f);
    }

    // Used when a  point is found
    private void StartInstance() {
        currentPoint++;
        if (currentPoint >= points.Length) {
            PlayerData.LoadMenu();
        }
        map.sprite = maps[Random.Range(0, maps.Length-1)];
        ProximityPlayer.instance.StartNewPlayer();

    }

     void Update() {
        if (ProximityPlayer.instance.playingGame == false) {
            SetScreenPos(0.5f, 0.5f);
        } else {
            gyro = GyroToUnity(Input.gyro.attitude);
            Vector3 removeZ = gyro.eulerAngles;
            gyro = Quaternion.Euler(removeZ.x, 0, removeZ.y);

            // Stores correct orientations
            if (gyro.eulerAngles.x > 180) {
                coords.y = gyro.eulerAngles.x - 360;
            } else {
                coords.y = gyro.eulerAngles.x;
            }
            if (gyro.eulerAngles.z > 180) {
                coords.x = gyro.eulerAngles.z - 360;
            } else {
                coords.x = gyro.eulerAngles.z;
            }

            //Normalizes angles and sets to span 0-1
            coords *= (1/angleSpan);
            coords += new Vector2(0.5f, 0.5f);
            coords.x = 1.0f - coords.x;

            SetScreenPos(coords.x, coords.y);
            float time = GetDistanceTime(coords);
            if (time < 0.2f) {
                successAudio.Play();
                StartInstance();

            }
            ProximityPlayer.instance.timeBetweenFeedback = GetDistanceTime(coords);
        }
    }

    private float GetDistanceTime(Vector2 coords) {
        Vector2 scaledCoords, scaledPoint, scalar;
        scalar = new Vector2(screenX, screenY);
        scaledCoords = Vector2.Scale(coords, scalar);
        scaledPoint = Vector2.Scale(points[currentPoint], scalar);

        float distance = Vector2.Distance(scaledCoords, scaledPoint);
        distance /= Mathf.Sqrt(screenX*screenX + screenY*screenY);
        distance *= 1.9f; distance += 0.1f;
        return distance;
    }

    private void SetScreenPos(float xPercentage, float yPercentage) {
        float x = screenX * xPercentage;
        float y = screenY * yPercentage;
        Vector3 pos = cross.GetComponent<RectTransform>().anchoredPosition;
        pos.x = x; pos.y = y;
        cross.GetComponent<RectTransform>().anchoredPosition = pos;
    }

    private static Quaternion GyroToUnity(Quaternion q) {
        return new Quaternion(q.x, q.y, q.z, -q.w);
    }
}