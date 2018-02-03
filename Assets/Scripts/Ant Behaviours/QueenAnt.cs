using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueenAnt : AntClass {


	public GameObject workerAnt;
	public int antCount;

	// Use this for initialization
	void Start () 
	{
		antSpeed = 0;
		SpawnAnts (antCount);

	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

	void SpawnAnts(int n)
	{
		for (int i = 0; i < n; i++) {
			Instantiate(workerAnt, transform.position, Quaternion.identity);
		}
	}
}
