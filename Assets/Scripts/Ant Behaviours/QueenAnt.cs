using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueenAnt : AntClass
{


	public GameObject workerAnt;
	public int antCount = 0;
	private float timer = 0;

	// Use this for initialization
	void Start ()
	{
		this.pGrid = GameObject.FindWithTag("PGrid").GetComponent<PheromoneGrid>();

		this.pGrid.addNest (transform.position);
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
		if (antCount < 100)
		if (timer > 0.02) {
			Instantiate (workerAnt, transform.position, Quaternion.Euler (0, 0, Random.value * 360));
			timer = 0;
			antCount++;
		
	}
	}

}
