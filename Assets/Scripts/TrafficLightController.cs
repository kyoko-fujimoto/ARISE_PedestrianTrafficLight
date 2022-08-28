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
    
    public Vector3 anchorPosition = new Vector3();
    public Quaternion anchorQuaternion = new Quaternion();

    private List<GameObject> _trafficLights = new List<GameObject>();
    private List<List<List<GameObject>>> _mosesTrafficLights = new List<List<List<GameObject>>>();
    private bool _isActivateTrafficLights = false;
    private bool _isActivateOnce = false;
    private Vector3 _position = new Vector3();
    
    private const int c_trafficLightsWidth = 30;
    private const int c_trafficLightsHeight = 10;

    private int tapCount = 0;
    private bool _isUnfinishedCoroutine = false;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < c_trafficLightsWidth; ++i)
        {
            for (int j = 0; j < c_trafficLightsHeight; ++j)
            {
                GameObject trafficLight = Instantiate(
                    TrafficLightPrefab,
                    new Vector3(0, 0, 0),
                    Quaternion.identity
                );
                
                trafficLight.SetActive(false); 

                _trafficLights.Add(trafficLight);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!Input.GetMouseButton(0) || _isUnfinishedCoroutine)
        {
            return;
        }

        switch(tapCount)
        {
        case 0:
            for (int i = 0; i < c_trafficLightsWidth; ++i)
            {
                for (int j = 0; j < c_trafficLightsHeight; ++j)
                {
                    StartCoroutine( VisibleTrafficLight(new Vector3(i * 0.4f, j * 0.75f, 0), i, j)); 
                }
            }
            break;

        case 1:
            StartCoroutine(HiddenTrafficLights());
            break;

        case 2:
            StartCoroutine(TurnOffLightRed());
            break;
        
        case 3: 
            StartCoroutine(MosesTrafficLights());
            StartCoroutine(MosesTrafficLightsChild());
            break;

        default:
            break; 

        }
        
        _isUnfinishedCoroutine = true;

        ++tapCount;
    }

    IEnumerator TurnOffLightRed()
    {
        yield return new WaitForSeconds(0.1f); 
        
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
        
        _isUnfinishedCoroutine = false;
    }

    // trafficlight を position の位置に waitIndex * 0.5 秒後に生成する
    IEnumerator VisibleTrafficLight(Vector3 position, int waitIndexX, int waitIndexY)
    {
        bool isLeftSide = waitIndexX < (c_trafficLightsWidth / 2);
        int waitX = isLeftSide ? waitIndexX : c_trafficLightsWidth - waitIndexX - 1;
        
        yield return new WaitForSeconds(0.1f * waitIndexY + 0.2f * waitX); 

        GameObject trafficLight = _trafficLights[waitIndexX + c_trafficLightsWidth * waitIndexY];

        trafficLight.transform.position += anchorPosition + position + new Vector3(-12.5f, -1, 18);
        trafficLight.transform.rotation = Quaternion.Euler(0, 180, 0);
        // trafficLight.transform.RotateAround(new Vector3(0, 0, 0), Vector3.up, 0f);
        trafficLight.SetActive(true);

        StartCoroutine(RotateTrafficLight(trafficLight, isLeftSide));
    }

    // trafficLight を回転させる
    IEnumerator RotateTrafficLight(GameObject trafficLight, bool isClockWise)
    {
        const int step = 12;
        
        int stepY = isClockWise ? step : -1 * step;

        for ( int i = 0; i < 180 / step; ++i ) 
        {
            trafficLight.transform.Rotate(new Vector3(0, stepY, 0));
            yield return new WaitForSeconds(0.01f); 
        }

        _isUnfinishedCoroutine = false;
    }

    IEnumerator HiddenTrafficLights()
    {
        const float step = 0.5f;

        for ( int i = 0; i < 16 / step; ++i ) 
        {
            List<GameObject> leftTrafficLights = new List<GameObject>();
            List<GameObject> rightTrafficLights = new List<GameObject>();

            for ( int j = 0; j < c_trafficLightsHeight * c_trafficLightsWidth; ++j )
            {
                var indexWidth = j % c_trafficLightsWidth;

                var trafficLight = _trafficLights[j];

                if ( true
                     && indexWidth >= c_trafficLightsWidth / 2
                     && indexWidth < c_trafficLightsWidth - 5
                     && i % 4 == 0 )
                {
                    var newTrafficLight = Instantiate( trafficLight ) as GameObject;
                    newTrafficLight.transform.localPosition = trafficLight.transform.localPosition;

                    rightTrafficLights.Add(newTrafficLight); 
                }

                if ( true
                     && indexWidth >= 5
                     && indexWidth < c_trafficLightsWidth / 2
                     && i % 4 == 0 )
                {
                    var newTrafficLight = Instantiate( trafficLight ) as GameObject;
                    newTrafficLight.transform.localPosition = trafficLight.transform.localPosition;

                    leftTrafficLights.Add(newTrafficLight); 
                }

                trafficLight.transform.position += new Vector3(0, 0, -step); 
            }
            yield return new WaitForSeconds(0.01f);
                
            List<List<GameObject>> trafficLights = new List<List<GameObject>>();
            trafficLights.Add(leftTrafficLights); 
            trafficLights.Add(rightTrafficLights); 

            _mosesTrafficLights.Add(trafficLights);
        }
        
        _isUnfinishedCoroutine = false;
    }
    
    IEnumerator MosesTrafficLights()
    {
        const float step = 0.2f;

        for ( int i = 0; i < 15; ++i )
        {
            for ( int j = 0; j < c_trafficLightsHeight * c_trafficLightsWidth; ++j )
            {
                bool isLeftSide = (j % c_trafficLightsWidth) < (c_trafficLightsWidth / 2);
                float stepX = isLeftSide ? -1 * step : step;
            
                _trafficLights[j].transform.position += new Vector3(stepX, 0, 0);
            }
            yield return new WaitForSeconds(0.01f); 
        }
        
        _isUnfinishedCoroutine = false;
    }

    IEnumerator MosesTrafficLightsChild()
    {
        const float step = 0.2f;

        _mosesTrafficLights.Reverse();

        foreach ( var trafficLightsList in _mosesTrafficLights )
        {
            var leftTrafficLights = trafficLightsList[0];
            var rightTrafficLights = trafficLightsList[1];
                    
            if ( leftTrafficLights.Count == 0 && rightTrafficLights.Count == 0 )
            {
                continue;
            }

            StartCoroutine(MosesTrafficLightChild( leftTrafficLights, rightTrafficLights ));

            yield return new WaitForSeconds(0.3f); 
        }

        _isUnfinishedCoroutine = false;
    }

    IEnumerator MosesTrafficLightChild( List<GameObject> leftTrafficLights, List<GameObject> rightTrafficLights )
    {
        const float step = 0.2f;
        
        for ( int i = 0; i < 15; ++i )
        {
            if ( leftTrafficLights.Count == 0 && rightTrafficLights.Count == 0 )
            {
                continue;
            }
                    // rightTrafficLights[j].transform.position += new Vector3(0, 0, -step);
                    
            foreach ( var trafficLight in leftTrafficLights )
            {
                trafficLight.transform.position += new Vector3(-step, 0, 0);
            }
            foreach ( var trafficLight in rightTrafficLights )
            {
                trafficLight.transform.position += new Vector3(step, 0, 0);
            }
                
            yield return new WaitForSeconds(0.01f); 
        }
    }
}
