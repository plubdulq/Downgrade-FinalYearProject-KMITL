using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class destroyBall : MonoBehaviour
{
     void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Air")
        {
            Destroy(other.gameObject);;
        }
        if (other.tag == "AirHot")
        {
            Destroy(other.gameObject);;
        }
        if (other.tag == "AirCool")
        {
            Destroy(other.gameObject);;
        }
 

    }
}
