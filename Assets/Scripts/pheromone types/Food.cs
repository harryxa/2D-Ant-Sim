using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : PheromoneNode
{

	private float quantity = 100f;
    public Vector3 worldFoodPosition;

	private float startingConcentration;

	void Start()
	{
        pheromoneConcentration = quantity;
		defaultScale = 0.05f;
	}

	void Update()
	{
        //float scale = (pheromoneConcentration / startingConcentration) * defaultScale;
        float scale = quantity * defaultScale;
        transform.localScale = new Vector3(scale, scale, scale);
        carryConcentration = 0;

        if (quantity == 0)
        {
            Destroy(gameObject);
        }
    }
}
