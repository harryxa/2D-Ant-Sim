using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueenAnt : AntClass
{


	public GameObject workerAnt;
	public int antCount;
	private float timer = 0;
	// Use this for initialization
	void Start ()
	{
		antSpeed = 0;
		SpawnAnts (antCount);

	}
	
	// Update is called once per frame
	void Update ()
	{
		SpawnAnts (antCount);
		timer = timer + Time.deltaTime;
	}

	void SpawnAnts (int n)
	{
		if (timer > 0.5 ) {
			Instantiate (workerAnt, transform.position, Quaternion.Euler (0, 0, Random.value * 360));
			timer = 0;
		}
	}

}
