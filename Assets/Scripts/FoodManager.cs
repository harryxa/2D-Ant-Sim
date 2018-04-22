using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodManager : MonoBehaviour {

	private PheromoneGrid pGrid;
    private WorldManager worldManager;

    private float timeSinceFoodDrop;
    private float foodDropFrequency = 180f;



    // Use this for initialization
    void Start () 
	{
        worldManager = GameObject.FindWithTag("WorldManager").GetComponent<WorldManager>();
        pGrid = GameObject.FindWithTag("PGrid").GetComponent<PheromoneGrid>();
        //worldManager = GameObject.FindWithTag("WorldManager").GetComponent<WorldManager>();
        pGrid.addFood(FoodPosition());



    }

    // Update is called once per frame
    void Update () 
	{
        timeSinceFoodDrop += Time.deltaTime * worldManager.timeRate;

        if(timeSinceFoodDrop > foodDropFrequency)
        {
            pGrid.addFood(FoodPosition());
            timeSinceFoodDrop = 0f;
        }


    }

    private Vector3 FoodPosition()
    {
       Vector3 foodPlacement = new Vector3(Random.Range(-WorldManager.worldWidth / 2f, WorldManager.worldWidth / 2f),
                                             Random.Range(-WorldManager.worldHeight / 2f, WorldManager.worldHeight / 2f),
                                             0f);

        while (!PositionSafe(foodPlacement))
            foodPlacement = new Vector3(Random.Range(-WorldManager.worldWidth / 2f, WorldManager.worldWidth / 2f),
                                             Random.Range(-WorldManager.worldHeight / 2f, WorldManager.worldHeight / 2f),
                                             0f);

        return foodPlacement;

    }

    private bool PositionSafe(Vector3 worldPosition)
    {
        Vector3 gridPosition = World.instance.worldToTileGrid(worldPosition); //******
        int x = (int)gridPosition.x;
        int y = (int)gridPosition.y;

        return (World.instance.GetTileAt(x, y).type == Tile.Type.Grass);
    }
}
