using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NestManager : MonoBehaviour
{
    public float foodStored;

	// Use this for initialization
	void Start ()
    {
        foodStored = 50f;
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    public void TakeFood(float amount)
    {
        foodStored -= amount;
    }

    public void StoreFood(float amount)
    {
        foodStored += amount;
    }



}
