using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PheromoneNode : MonoBehaviour 
{
    protected SpriteRenderer m_spriteRenderer;

	private float defaultConc = 1f;
	private float maxConc = 100f;

	protected float evaporationRate = 0.001f;
	public float defaultScale = 0.01f;

	//public bool exists = true;
	public int gridX;
	public int gridY;

    //PHEROMONE TYPES
	public float pheromoneConcentration = 0f;
	public float foodConcentration = 0f;
	public float nestConcentration = 0f;
	public float carryConcentration = 0f;


    void Awake()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
    }


    void Start () 
	{
		
    }

    void FixedUpdate () 
	{
		reducePheromoneConcentration ();
        changePheromoneColour();
    }

	public void boostStandardConc() 
	{
		pheromoneConcentration += defaultConc;
	}

	public void boostCarryConc() 
	{
		carryConcentration += defaultConc * 2;
	}

    public void setXY(int x, int y)
    {
        gridX = x;
        gridY = y;
    }

    //reduces all pheromone concentrations
	public void reducePheromoneConcentration()
	{
        //STANDARD PHEROMONE
		if (pheromoneConcentration > 0f) 
		{
			if (pheromoneConcentration > maxConc)
				pheromoneConcentration = maxConc;

			pheromoneConcentration -= evaporationRate * Time.deltaTime;

            if (pheromoneConcentration > carryConcentration)
            {
                float scale = (pheromoneConcentration / defaultConc) * defaultScale;
                transform.localScale = new Vector3(scale, scale, scale);
            }
		}
        //CARRY PHEROMONE
		if (carryConcentration > 0f) 
		{
			if (carryConcentration > maxConc)
				carryConcentration = maxConc;

			carryConcentration -= evaporationRate * Time.deltaTime;

            if (pheromoneConcentration < carryConcentration)
            {
                float scale = (carryConcentration / defaultConc) * defaultScale;
                transform.localScale = new Vector3(scale, scale, scale);
            }
		}
		else
		{
			//Destroy(gameObject);
		}
	}

    public void changePheromoneColour()
    {
        if(pheromoneConcentration > carryConcentration)
        {
            if (m_spriteRenderer.color != Color.white)
                m_spriteRenderer.color = Color.white;
        }
        else if (carryConcentration > pheromoneConcentration)
        {
            if (m_spriteRenderer.color != Color.blue)
                m_spriteRenderer.color = Color.blue;
        }
    }

}
