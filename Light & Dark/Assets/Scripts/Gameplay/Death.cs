using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Death : MonoBehaviour
{


    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.tag == "Player")
        {
            other.gameObject.transform.parent.GetComponentInParent<PlayerMovement>().die();
        }

    }
}
