using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : PheromoneNode
{

	public float quantity;
    public Vector3 worldFoodPosition;


	void Start()
	{
        quantity = Random.Range(10, 300);
        pheromoneConcentration = quantity;
		defaultScale = 0.02f;

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

    public void ReduceFood(float amount)
    {
        //STANDARD PHEROMONE
        if (pheromoneConcentration > 0)
        {
            quantity -= amount;           
        }

    }
}
