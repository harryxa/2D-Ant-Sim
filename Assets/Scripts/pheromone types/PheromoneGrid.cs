using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PheromoneGrid : MonoBehaviour 
{
    


	private float worldWidth = 100;
	private float worldHeight = 100;
	private int gridHeight = 100;
	private int gridWidth = 100;


	public Transform[,] grid;
	public Transform pheromone;
	public Transform genericFood;
	public Transform nest;

    public Vector3 worldNestPosition;

    

    public enum PheromoneType
    {
        STANDARD,
        NEGATIVE,
        CARRYING
    }

    public PheromoneType pType;

    // Use this for initialization
    void Start () 
	{

        //grid can range from 0 to 10, so + 1
        grid = new Transform[gridWidth + 1, gridHeight + 1];

        Debug.Log(World.instance.GetTileAt(0, 0).type);

        InitialisePheromones();

    }

    private void Awake()
    {

    }

    // Update is called once per frame
    void Update () 
	{

    }

    private void InitialisePheromones()
    {
        for (int x = 0; x < gridWidth + 1; x++)
        {
            for (int y = 0; y < gridHeight + 1; y++)
            {
                //TODO hacky grid displacement
                //grid[x, y] = Instantiate(pheromone, gridToWorld(new Vector3(x + 0.5f, y + 0.5f,0)), Quaternion.identity);

                AddPheromone(gridToWorld(new Vector3(x, y, 0)), PheromoneType.STANDARD, 0f);

                //this may brake when world and grid made properly
                if (World.instance.GetTileAt(x, y).type != Tile.Type.Grass)
                {
                    BoostConcentration(PheromoneType.NEGATIVE, x, y, 100f);
                }
                //Debug.Log(World.instance.GetTileAt(x, y).type);
            }
        }
    }

    public void AddPheromone(Vector3 worldPos, PheromoneType _pType, float concentrationMultiplier)
	{
		Vector3 gridPos = worldToGrid (worldPos);
		gridPos.x = Mathf.RoundToInt(gridPos.x);
		gridPos.y = Mathf.RoundToInt(gridPos.y);

        

        int x = (int)gridPos.x;
		int y = (int)gridPos.y;

        gridPos.x += 0.5f;
        gridPos.y += 0.5f;

        if (grid[x, y] == null)
        {
            if (true || World.instance.GetTileAt(x, y).type == Tile.Type.Grass || World.instance.GetTileAt(x, y) == null)
            {
                //instantiate pheromone
                grid[x, y] = Instantiate(pheromone, gridToWorld(gridPos), Quaternion.identity);

                PheromoneNode node = grid[x, y].GetComponent<PheromoneNode>();
                node.SetXY(x, y);
                BoostConcentration(_pType, x, y, concentrationMultiplier); 
            }                       
        }

        //boost concentration of existing pheromone // ### Is instantiating an object too slow? ###
		else
		{
            BoostConcentration(_pType, x, y, concentrationMultiplier);
        }
    }

    //boost varying types of pheromone nodes
    private void BoostConcentration(PheromoneType _pType, int x, int y, float multiplier)
    {
        if (_pType == PheromoneType.STANDARD)
            grid[x, y].GetComponent<PheromoneNode>().BoostStandardConc(multiplier);

        else if (_pType == PheromoneType.CARRYING)
            grid[x, y].GetComponent<PheromoneNode>().BoostCarryConc(multiplier);

        else if (_pType == PheromoneType.NEGATIVE)
        {
            grid[x, y].GetComponent<PheromoneNode>().BoostNegativeConc(multiplier);
        }
    }

    public void addFood(Vector3 worldPos)
	{		
		Vector3 gridPos = worldToGrid (worldPos);
		gridPos.x = Mathf.RoundToInt(gridPos.x);
		gridPos.y = Mathf.RoundToInt(gridPos.y);
		int x = (int)gridPos.x;
		int y = (int)gridPos.y;

		grid [x, y] = Instantiate (genericFood, gridToWorld(gridPos), Quaternion.identity);        

        Food node = grid [x, y].GetComponent<Food> ();
		node.SetXY (x, y);
        node.worldFoodPosition = worldPos;
    }

	public void addNest(Vector3 worldPos)
	{
		Vector3 gridPos = worldToGrid (worldPos);
		gridPos.x = Mathf.RoundToInt(gridPos.x);
		gridPos.y = Mathf.RoundToInt(gridPos.y);
		int x = (int)gridPos.x;
		int y = (int)gridPos.y;

		//Debug.Log (x + ", yoohooo " + y);

		grid [x, y] = Instantiate (nest, gridToWorld(gridPos), Quaternion.identity);

        worldNestPosition = gridToWorld(gridPos);

        NestPheromone node = grid [x, y].GetComponent<NestPheromone> ();
		node.SetXY (x, y);
	}

	public Vector3 gridToWorld(Vector3 gridPos) 
	{
		return new Vector3 (gridPos.x * (worldWidth / (float)gridWidth) - worldWidth/2f, 
			gridPos.y * (worldHeight / (float)gridHeight) - worldHeight/2f, 0f);
	}

	public  Vector3 worldToGrid(Vector3 worldPos)
	{
		return new Vector3 ((worldPos.x + worldWidth/2)   /   (worldWidth / (float)gridWidth),
			(worldPos.y + worldHeight/2)  /   (worldHeight / (float)gridHeight), 0f);
	}

	public Vector3 wrapGridCoord(Vector3 gridPos)
	{
		while (gridPos.x < 0)
			gridPos.x += gridWidth;
		while (gridPos.y < 0)
			gridPos.y += gridHeight;
		while (gridPos.x > gridWidth)
			gridPos.x -= gridWidth;
		while (gridPos.y > gridHeight)
			gridPos.y -= gridHeight;

		return gridPos;
	}


	public float getWorldWidth()
	{		
		return worldWidth;
	}

	public float getWorldHeight()
	{
		return worldHeight;
	}




    
}
