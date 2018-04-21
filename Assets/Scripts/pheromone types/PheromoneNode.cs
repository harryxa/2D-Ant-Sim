using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PheromoneNode : MonoBehaviour 
{
    protected SpriteRenderer m_spriteRenderer;

	private float defaultConc = 5f;
	private float maxConc = 100f;

	protected float evaporationRate = 0.2f;
	protected float defaultScale = 0.1f;

	//public bool exists = true;
	public int gridX;
	public int gridY;

    //PHEROMONE TYPES
	public float pheromoneConcentration = 0f;
    public float nestConcentration = 0f;
    public float carryConcentration = 0f;
    public float negativeConcentration = 0f;



    void Awake()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
    }


    void Start () 
	{


    }

    void Update () 
	{
		ReducePheromoneConcentration ();
        ChangePheromoneColour();
    }

	public void BoostStandardConc(float multiplier) 
	{
		pheromoneConcentration += defaultConc * multiplier;
	}

    public void BoostNegativeConc(float multiplier)
    {
        negativeConcentration += defaultConc * multiplier;
    }

    public void BoostCarryConc(float multiplier) 
	{
		carryConcentration += defaultConc * multiplier;
	}

    public void SetXY(int x, int y)
    {
        gridX = x;
        gridY = y;
    }

    //reduces all pheromone concentrations
	public void ReducePheromoneConcentration()
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
		else if (carryConcentration > 0f) 
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
		else if (negativeConcentration > 0)
		{
            transform.localScale = new Vector3(1f, 1f, 1f);
		}
	}

    public void ChangePheromoneColour()
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
        else if (negativeConcentration > 1)
        {
            if (m_spriteRenderer.color != Color.black)
                m_spriteRenderer.color = Color.black;
        }
    }

}
