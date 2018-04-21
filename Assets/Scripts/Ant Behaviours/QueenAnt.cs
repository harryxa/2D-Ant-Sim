using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueenAnt : AntClass
{

    public List<GameObject> ants;
	public GameObject workerAnt;
    // was private
	public int pooledAntCount = 2000;

    private int activeAnts = 0;
    public int numberOfScouts;
    public int colonySize;

    public float workerAntSpeed;
    public float timeSinceAntReleased = 0f;
    public float antReleaseRate;

    // Use this for initialization
    void Start ()
	{

		this.pGrid = GameObject.FindWithTag("PGrid").GetComponent<PheromoneGrid>();



        PlaceQueen();

        this.pGrid.addNest(transform.position);
        antSpeed = 0;
        ants = new List<GameObject>();

		SpawnAnts();

        workerAntSpeed = 2f;
        state = AntState.GATHERING;


    }
	
	// Update is called once per frame
	void Update ()
	{
        SmellPheromone();

        // TODO: Sort out this mess!
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

        timeSinceAntReleased += Time.deltaTime;
        ReleaseGatherers();
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
        CarryCount = 0f;
       
        for (int x = gridX - smellRadius; x <= gridX + smellRadius; x++)
        {
            for (int y = gridY - smellRadius; y <= gridY + smellRadius; y++)
            {
                if (x >= 0 && x < WorldManager.worldWidth && y >= 0 && y < WorldManager.worldHeight)
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
        antReleaseRate = 100f / carryPheromoneCount;

        //TODO: add colony size ie. if (numscouts < colsize) then allowed to release more

        //only release at all if can smell at least e.g. 10 pheromones
        if(antReleaseRate > 1f) {
            if(timeSinceAntReleased >= antReleaseRate)
            {
                // Release an ant
                if (activeAnts < pooledAntCount)
                    //??
                    

                // Reset time conter
                timeSinceAntReleased = 0f;
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
