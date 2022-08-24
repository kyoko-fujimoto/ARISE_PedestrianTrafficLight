using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficLightController : MonoBehaviour
{
    public GameObject TrafficLightPrefab;
    public Material LightRedOn;
    public Material LightRedOff;
    public Material LightGreenOn;
    public Material LightGreenOff;

    private List<GameObject> _trafficLights = new List<GameObject>();
    private bool _isActivateTrafficLights = false;
    private bool _isActivateOnce = false;
    private Vector3 _position = new Vector3();
    
    private const int c_trafficLightsWidth = 30;
    private const int c_trafficLightsHeight = 10;

    public Vector3 anchorPosition = new Vector3();

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < c_trafficLightsWidth; ++i)
        {
            for (int j = 0; j < c_trafficLightsHeight; ++j)
            {
                StartCoroutine( VisibleTrafficLight(anchorPosition + new Vector3(i * 0.4f, j * 0.75f, 10), i, j)); 
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(TurnOffLightRed());
    }

    IEnumerator TurnOffLightRed()
    {
        yield return new WaitForSeconds(20); 
        
        GameObject[] lightRedObjects = GameObject.FindGameObjectsWithTag("LightRed");
        foreach (var lightRedObject in lightRedObjects)
        {
            lightRedObject.GetComponent<MeshRenderer>().material = LightRedOff;
        }
        
        GameObject[] lightGreenObjects = GameObject.FindGameObjectsWithTag("LightGreen");
        foreach (var lightGreenObject in lightGreenObjects)
        {
            lightGreenObject.GetComponent<MeshRenderer>().material = LightGreenOn;
        }
    }

    // trafficlight を position の位置に waitIndex * 0.5 秒後に生成する
    IEnumerator VisibleTrafficLight(Vector3 position, int waitIndexX, int waitIndexY)
    {
        bool isLeftSide = waitIndexX < (c_trafficLightsWidth / 2);
        int waitX = isLeftSide ? waitIndexX : c_trafficLightsWidth - waitIndexX - 1;
        
        yield return new WaitForSeconds(0.4f * waitIndexY + 0.6f * waitX); 
            
        GameObject trafficLight = Instantiate(
            TrafficLightPrefab,
            _position + position,
            Quaternion.identity
        );

        _trafficLights.Add(trafficLight);

        StartCoroutine(RotateTrafficLight(trafficLight, isLeftSide));
    }

    // trafficLight を回転させる
    IEnumerator RotateTrafficLight(GameObject trafficLight, bool isClockWise)
    {
        const int step = 6;
        
        int stepY = isClockWise ? step : -1 * step;

        for (int i = 0; i < 360 / step; ++i) 
        {
            trafficLight.transform.Rotate(new Vector3(0, stepY, 0));
            yield return new WaitForSeconds(0.01f); 
        }
    }
}
