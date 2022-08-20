using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NullTest : MonoBehaviour
{
    private GameObject _gameObject;

    private void OnEnable()
    {
        if (_gameObject == null)
        {
            Debug.Log("null");
        }
    }
}
