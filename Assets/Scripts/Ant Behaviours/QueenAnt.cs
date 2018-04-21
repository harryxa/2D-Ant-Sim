﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueenAnt : AntClass
{

    public List<GameObject> ants;
	public GameObject workerAnt;
	public int antCount;

    private int activeAnts = 0;
    public int numberOfScouts;

    public float workerAntSpeed;
    

    // Use this for initialization
    void Start ()
	{

		this.pGrid = GameObject.FindWithTag("PGrid").GetComponent<PheromoneGrid>();

        halfWorldHeight = pGrid.getWorldHeight() / 2;
        halfWorldWidth = pGrid.getWorldWidth() / 2;

        worldHeight = pGrid.getWorldHeight();
        worldWidth = pGrid.getWorldWidth();

        PlaceQueen();
        this.pGrid.addNest(transform.position);
        antSpeed = 0;
        ants = new List<GameObject>();
		SpawnAnts();
        workerAntSpeed = 2f;
        state = AntState.GATHERING;
        worldHeight = pGrid.getWorldHeight();
        worldWidth = pGrid.getWorldWidth();

    }
	
	// Update is called once per frame
	void Update ()
	{
        SmellPheromone();

        for (int i = 0; i <= ants.Count - 1; i++)
        {
            if (activeAnts < numberOfScouts)
            {
                if (ants[i].activeSelf == false)
                {
                    ants[i].SetActive(true);
                    activeAnts++;
                }
            }
            else if (activeAnts > numberOfScouts)
            {
                if (ants[i].activeSelf == true)
                {
                    if(ants[i].GetComponent<AntClass>().state == AntState.NESTING && ants[i].GetComponent<AntClass>().nesting == true)
                    {
                        ants[i].GetComponent<AntClass>().nesting = false;
                        ants[i].SetActive(false);
                        activeAnts--;
                        ants[i].GetComponent<AntClass>().hunger = 100f;
                    }
                }
            }            
            ants[i].GetComponent<AntClass>().SetAntSpeed(workerAntSpeed);
           // ants[i].GetComponent<AntClass>().setSecrete();
        }
    }

    private void PlaceQueen()
    {
        
        while (!PositionSafe(transform.position))
            transform.position = new Vector3(Random.Range(-halfWorldWidth, halfWorldWidth),
                                             Random.Range(-halfWorldHeight, halfWorldHeight),
                                             0f);
    }

    private bool PositionSafe(Vector3 worldPosition)
    {
        Vector3 gridPosition = pGrid.worldToGrid(worldPosition);
        int x = Mathf.FloorToInt(gridPosition.x);
        int y = Mathf.FloorToInt(gridPosition.y);

        return (World.instance.GetTileAt(x, y).type == Tile.Type.Grass);
    }

    //smell for carry pheromones
    private void SmellPheromone()
    {
        //gets the ants location on the grid
        Vector3 antGridPos = pGrid.worldToGrid(transform.position);
        int gridX = Mathf.RoundToInt(antGridPos.x);
        int gridY = Mathf.RoundToInt(antGridPos.y);

        int smellRadius = 7;
        carryPheromoneCount = 0f;
        CarryCount = 0f;
       
        for (int x = gridX - smellRadius; x <= gridX + smellRadius; x++)
        {
            for (int y = gridY - smellRadius; y <= gridY + smellRadius; y++)
            {
                if (x >= 0 && x < worldWidth && y >= 0 && y < worldHeight)
                {
                    if (pGrid.grid[x, y] != null)
                    {
                        if (pGrid.grid[x, y].GetComponent<PheromoneNode>().carryConcentration >= 1)
                        {
                            carryPheromoneCount += pGrid.grid[x, y].GetComponent<PheromoneNode>().carryConcentration;
                        }
                    }
                }
            }
        }
    }

    public void ReleaseGatherers()
    {
        //the stronger the pheromone count the more ants get released
    }

    

	void SpawnAnts ()
	{
		for(int i = 0; i < antCount; i++)
        {
            GameObject ant = Instantiate(workerAnt, transform.position, Quaternion.Euler(0, 0, Random.value * 360));
            ants.Add(ant);
            ants[i].SetActive(false);            
        }
    }
}
