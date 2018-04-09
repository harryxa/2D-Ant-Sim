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

	public float concentration;
	public float foodConcentration;
	public float nestConcentration;
	public float carryConcentration;



	void Start () 
	{
		concentration = defaultConc;
		foodConcentration = defaultConc;
		nestConcentration = defaultConc;
		carryConcentration = defaultConc;


        m_spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void FixedUpdate () 
	{
		reducePheromoneConcentration ();    
	}

	public void boostStandardConc() 
	{
		concentration += defaultConc;
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
		if (concentration > 0f) 
		{
			if (concentration > maxConc)
				concentration = maxConc;

			concentration -= evaporationRate * Time.deltaTime;
			float scale = (concentration / defaultConc) * defaultScale; 

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
}
