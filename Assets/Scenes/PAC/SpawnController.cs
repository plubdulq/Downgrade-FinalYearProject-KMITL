using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnController : MonoBehaviour
{
   public RandomSpawner[] randomSpawner;
    public void reStart()
    {
        foreach (var air in randomSpawner)
            {
            air.GetComponent<RandomSpawner>().ReStart();      
           }
    }

 
}
