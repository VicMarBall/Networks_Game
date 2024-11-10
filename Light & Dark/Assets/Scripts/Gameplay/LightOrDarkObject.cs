using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightOrDarkObject : MonoBehaviour
{
    GameplayEnvironmentMode environmentMode;
    // Start is called before the first frame update
    void Start()
    {
        environmentMode = GameManager.instance.gameplayEnvironmentMode;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
