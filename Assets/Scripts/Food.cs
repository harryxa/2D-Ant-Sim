using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : PheromoneNode
{

	private float quantity = 100000f;
	public float smellFactor = 2f;

	private float startingConcentration;

	void Start()
	{		
		concentration = quantity * smellFactor;
		startingConcentration = concentration;
		defaultScale = 2f;
	}

	void FixedUpdate()
	{


		float scale = (concentration / startingConcentration) * defaultScale;
		transform.localScale = new Vector3 (scale, scale, scale);


		if (quantity == 0) {
			Destroy (gameObject);
		}
	}
}
