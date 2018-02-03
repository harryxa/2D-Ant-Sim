using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : PheromoneNode
{

	private float quantity = 10;
	public float smellFactor = 2;

	void Awake()
	{
		concentration = quantity * smellFactor;
	}

	void FixedUpdate()
	{
		if (quantity == 0) {
			Destroy (gameObject);
		}
	}
}
