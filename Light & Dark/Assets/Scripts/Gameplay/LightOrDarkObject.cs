using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightOrDarkObject : MonoBehaviour
{
    [SerializeField] GameplayEnvironmentMode environmentMode;

	[SerializeField] Material lightMaterial;
	[SerializeField] Material darkMaterial;
    // Start is called before the first frame update
    void Start()
    {
        environmentMode = GameManager.instance.gameplayEnvironmentMode;

        if (environmentMode == GameplayEnvironmentMode.LIGHT)
        {
			GetComponent<Renderer>().material = lightMaterial;
		}
        else if (environmentMode == GameplayEnvironmentMode.DARK)
        {
			GetComponent<Renderer>().material = darkMaterial;
		}
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
