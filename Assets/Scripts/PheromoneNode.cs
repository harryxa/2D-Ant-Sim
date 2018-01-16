using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PheromoneNode : MonoBehaviour 
{

	public float concentration = 10f;
	private float evaporationRate = 0.01f;
	public bool exists;

	// Use this for initialization
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (concentration > 0) {
			exists = true;
			concentration -= evaporationRate * Time.deltaTime;
			//Debug.Log (concentration);
		} else
			exists = false;
	}
}
