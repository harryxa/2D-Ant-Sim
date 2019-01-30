using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PheromoneNode : MonoBehaviour 
{
    protected SpriteRenderer m_spriteRenderer;
    private WorldManager worldManager;

	private float defaultConc = 5f;
	private float maxConc = 50;

	//protected float evaporationRate = 0.2f;
    protected float evaporationRate = 0.5f;
    protected float carryEvaporationRate = 0.3f;

    protected float defaultScale = 0.1f;

	//public bool exists = true;
	public int gridX;
	public int gridY;

    //PHEROMONE TYPES
	public float pheromoneConcentration = 0f;
    public float nestConcentration = 0f;
    public float gatheringConcentration = 0f;
    public float negativeConcentration = 0f;



    void Awake()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
    }


    void Start () 
	{
        worldManager = GameObject.FindWithTag("WorldManager").GetComponent<WorldManager>();
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
		gatheringConcentration += defaultConc * multiplier;
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

			pheromoneConcentration -= evaporationRate * Time.deltaTime * worldManager.timeRate;

            if (pheromoneConcentration > gatheringConcentration)
            {
                float scale = (pheromoneConcentration / defaultConc) * defaultScale;
                transform.localScale = new Vector3(scale, scale, scale);
            }
            if (pheromoneConcentration < 0)
                pheromoneConcentration = 0f;
		}
        //CARRY PHEROMONE
		if (gatheringConcentration > 0f) 
		{
			if (gatheringConcentration > maxConc)
				gatheringConcentration = maxConc;

			gatheringConcentration -= carryEvaporationRate * Time.deltaTime * worldManager.timeRate;

            if (pheromoneConcentration < gatheringConcentration)
            {
                float scale = (gatheringConcentration / defaultConc) * defaultScale;
                transform.localScale = new Vector3(scale, scale, scale);
            }
            if (gatheringConcentration < 0)
                gatheringConcentration = 0f;

        }
		if (negativeConcentration > 0)
		{
            transform.localScale = new Vector3(1f, 1f, 1f);
		}
	}

    public void ChangePheromoneColour()
    {
        if (gatheringConcentration > 0)
        {
            if (m_spriteRenderer.color != Color.blue)
                m_spriteRenderer.color = Color.blue;
        }
        else if (pheromoneConcentration > 0)
        {
            if (m_spriteRenderer.color != Color.white)
                m_spriteRenderer.color = Color.white;
        }
        
        else if (negativeConcentration > 0)
        {
            if (m_spriteRenderer.color != Color.clear)
                m_spriteRenderer.color = Color.clear;
        }
    }

}
