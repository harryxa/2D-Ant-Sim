using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PheromoneGrid : MonoBehaviour 
{

	private float worldWidth = 100;
	private float worldHeight = 100;
	private int gridHeight = 100;
	private int gridWidth = 100;
	private int IDCounter = 0;

	public Transform[,] grid;
	public Transform pheromone;

	// Use this for initialization
	void Start () 
	{
		//grid can range from 0 to 10, so + 1
		grid = new Transform[gridWidth + 1, gridHeight + 1];

	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

	public void addPheromone(Vector3 worldPos)
	{
		Vector3 gridPos = worldToGrid (worldPos);
		//TODO: add concentration to current node rather than recreate (when a node exists here already)
		gridPos.x = Mathf.RoundToInt(gridPos.x);
		gridPos.y = Mathf.RoundToInt(gridPos.y);
		int x = (int)gridPos.x;
		int y = (int)gridPos.y;

		if(grid[x, y] != null)
			grid[x,y].GetComponent<PheromoneNode>().boostConc();
		else {
			grid [x, y] = Instantiate(pheromone, gridToWorld(gridPos), Quaternion.identity);
			PheromoneNode node = grid [x, y].GetComponent<PheromoneNode> ();
			node.setXY (x, y);
			node.setGrid (this);
			node.setID (IDCounter++);
		}
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

	// need to change AntClass's bounding box to this eventually
//	public Vector3 wrapWorldCoord(Vector3 worldPos)
//	{
//
//	}

	public float getWorldWidth()
	{		
		return worldWidth;
	}

	public float getWorldHeight()
	{
		return worldHeight;
	}

}
