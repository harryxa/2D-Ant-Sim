using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueenAnt : AntClass
{
    
    public List<GameObject> ants;
	public GameObject workerAnt;
    // was private
	public int pooledAntCount;
    public int activeAnts = 0;
    public float antsInWorld;
    public int colonySize;
    private float scoutRatio = 0.25f;

    public float workerAntSpeed;
    public float timeSinceAntReleased = 0f;
    public float antReleaseFrequency;

    //private float previousCarryPheromoneCount = 0;
    private float pheromonesPerAntPerSecond = 750f;
    private float minimumPheromonesForAntRelease = 50f;
    private NestManager nest;

    private float stockpiledMealsPerAnt = 5f;

    private float birthFoodCost = 0.5f;     //food required to increment colony size
    private float timeSinceAntBorn;
    private float antBirthFreq;
    private float freqScale = 10f; // ant per ten seconds when we have an excess
    private float maxBirthFreq = 2f;


    // Use this for initialization
    void Start ()
	{
		this.pGrid = GameObject.FindWithTag("PGrid").GetComponent<PheromoneGrid>();
        PlaceQueen();
        this.pGrid.addNest(transform.position);
        nest = GameObject.FindWithTag("Nest").GetComponent<NestManager>();
        
        antSpeed = 0;
        ants = new List<GameObject>();

		SpawnAnts();

        state = AntState.GATHERING;
        worldManager = GameObject.FindWithTag("WorldManager").GetComponent<WorldManager>();

    }

    // Update is called once per frame
    void Update ()
	{
        SmellPheromone();
        ManageColonySize();
        ActivateAnts();
        timeSinceAntReleased += Time.deltaTime * worldManager.timeRate;
        timeSinceAntBorn += Time.deltaTime * worldManager.timeRate;
        ReleaseGatherers();
    }


    public void ManageColonySize()
    {
        float stockpileSize = nest.foodStored / (AntClass.foodTakenFromNest * stockpiledMealsPerAnt);

        // If food stored is bigger than colony size incease colony size, but
        // scale by the number of stockpiled meals wanted
        if (stockpileSize > colonySize)
        {

            antBirthFreq = colonySize / stockpileSize; //partway through calculating
            if (antBirthFreq > maxBirthFreq)
                antBirthFreq = maxBirthFreq;

            antBirthFreq *= freqScale; // actual frequency

            if (timeSinceAntBorn > antBirthFreq)
            {
                nest.TakeFood(birthFoodCost);

                timeSinceAntBorn = 0f;
                colonySize++;
            }
        }
    }

    public void ActivateAnts()
    {
        for (int i = 0; i < ants.Count; i++)
        {
            AntClass antComponent = ants[i].GetComponent<AntClass>();
            if (antComponent.state == AntState.DEAD)
            {
                Debug.Log("ant died at: " + transform.position);
                activeAnts--;
                colonySize--;
                ants[i].transform.position = nest.transform.position;
                antComponent.hunger = antComponent.maxHunger;
                antComponent.state = AntState.SCOUTING;

                ants[i].SetActive(false);
            }
            if (activeAnts < antsInWorld)
            {
                if (ants[i].activeSelf == false)
                {
                    ants[i].SetActive(true);
                    activeAnts++;
                }
            }
            else if (activeAnts > antsInWorld)
            {
                if (ants[i].activeSelf == true)
                {
                    if (antComponent.state == AntState.NESTING
                        && antComponent.atNest == true)
                    {
                        antComponent.atNest = false;
                        ants[i].SetActive(false);
                        activeAnts--;

                        //TODO: sort hunger out
                        antComponent.hunger = 100f;
                    }
                   
                }
            }

            antComponent.SetAntSpeed(workerAntSpeed);
        }
    }

    public void ReleaseGatherers()
    {
        //
        antReleaseFrequency = pheromonesPerAntPerSecond / (carryPheromoneCount-minimumPheromonesForAntRelease);

        if (antReleaseFrequency > 0)
        {
            if(timeSinceAntReleased >= antReleaseFrequency)
            {
                // Release an ant
                if (activeAnts < colonySize)
                {
                    antsInWorld++;
                }
                // Reset time conter
                timeSinceAntReleased = 0f;
            }   
        }   
        else if(antsInWorld > colonySize * scoutRatio)
        {
            antsInWorld-= Time.deltaTime * worldManager.timeRate;
        }
        else
        {
            antsInWorld = Mathf.FloorToInt(colonySize * scoutRatio);
        }
        //previousCarryPheromoneCount = carryPheromoneCount;
    }

    private void PlaceQueen()
    {
        transform.position = new Vector3(Random.Range(-WorldManager.worldWidth/2f, WorldManager.worldWidth/2f),
                                             Random.Range(-WorldManager.worldHeight/2f, WorldManager.worldHeight/2f),
                                             0f);

        while (!PositionSafe(transform.position))
            transform.position = new Vector3(Random.Range(-WorldManager.worldWidth/2f, WorldManager.worldWidth/2f),
                                             Random.Range(-WorldManager.worldHeight/2f, WorldManager.worldHeight/2f),
                                             0f);
    }

    private bool PositionSafe(Vector3 worldPosition)
    {
        Vector3 gridPosition = World.instance.worldToTileGrid(worldPosition); //******
        int x = (int)gridPosition.x;
        int y = (int)gridPosition.y;

        return (World.instance.GetTileAt(x, y).type == Tile.Type.Grass);
    }

    //smell for carry pheromones
    private void SmellPheromone()
    {
        //gets the ants location on the grid
        Vector3 antGridPos = pGrid.worldToGrid(transform.position);
        int gridX = (int)antGridPos.x;
        int gridY = (int)antGridPos.y;

        int smellRadius = 7;
        carryPheromoneCount = 0f;
       
        for (int x = gridX - smellRadius; x <= gridX + smellRadius; x++)
        {
            for (int y = gridY - smellRadius; y <= gridY + smellRadius; y++)
            {
                if (x >= 0 && x < WorldManager.worldWidth && y >= 0 && y < WorldManager.worldHeight)
                {
                    if (pGrid.grid[x, y] != null)
                    {
                        if (pGrid.grid[x, y].GetComponent<PheromoneNode>().gatheringConcentration >= 1)
                        {
                            carryPheromoneCount += pGrid.grid[x, y].GetComponent<PheromoneNode>().gatheringConcentration;
                        }
                    }
                }
            }
        }
    }
    

	void SpawnAnts ()
	{
        GameObject parentAntGameObject = new GameObject("Ants");

		for(int i = 0; i < pooledAntCount; i++)
        {
            GameObject ant = Instantiate(workerAnt, transform.position, Quaternion.Euler(0, 0, Random.value * 360));
            ants.Add(ant);
            ants[i].SetActive(false);
            ants[i].transform.SetParent(parentAntGameObject.transform);
        }
    }
}
