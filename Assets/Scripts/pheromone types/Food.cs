using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : PheromoneNode
{

	public float quantity = 100f;
    public Vector3 worldFoodPosition;

	private float startingConcentration;

	void Start()
	{
        pheromoneConcentration = quantity;
		defaultScale = 0.04f;
	}

	void Update()
	{
        //float scale = (pheromoneConcentration / startingConcentration) * defaultScale;
        float scale = quantity * defaultScale;
        transform.localScale = new Vector3(scale, scale, scale);

        if (quantity <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void reduceFoodAmount()
    {
        //STANDARD PHEROMONE
        if (pheromoneConcentration > 0f)
        {
            quantity -= 1;           
        }

        else
        {
            Destroy(gameObject);
        }
    }
}
