using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PheromoneNode : MonoBehaviour 
{


    protected SpriteRenderer m_spriteRenderer;

	public float defaultConc = 20f;
	private float maxConc = 100f;
	public float evaporationRate = 0.5f;
	public float defaultScale = 0.5f;
	//public bool exists = true;
	public int gridX;
	public int gridY;

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
		carryConcentration += defaultConc;
	}

    public void setXY(int x, int y)
    {
        gridX = x;
        gridY = y;
    }

	public void reducePheromoneConcentration()
	{
		if (pheromoneConcentration > 0f) 
		{
			if (pheromoneConcentration > maxConc)
				pheromoneConcentration = maxConc;

			pheromoneConcentration -= evaporationRate * Time.deltaTime;
			float scale = (pheromoneConcentration / defaultConc) * defaultScale; 

			transform.localScale = new Vector3(scale,scale,scale);
		}
		else if (carryConcentration > 0f) 
		{
			if (carryConcentration > maxConc)
				carryConcentration = maxConc;

			carryConcentration -= evaporationRate * Time.deltaTime;
			float scale = (carryConcentration / defaultConc) * defaultScale; 

			transform.localScale = new Vector3(scale,scale,scale);
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
            if (m_spriteRenderer.color != Color.magenta)
                m_spriteRenderer.color = Color.magenta;
        }
    }

}
