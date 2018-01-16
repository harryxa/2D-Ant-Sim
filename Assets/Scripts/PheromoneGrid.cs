using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PheromoneGrid : MonoBehaviour 
{

	private int worldWidth = 100;
	private int worldHeight = 100;
	private int gridHeight = 10;
	private int gridWidth = 10;

	public PheromoneNode[,] grid;

	// Use this for initialization
	void Start () 
	{
		grid = new PheromoneNode[gridWidth, gridHeight];
		addPheromone (new Vector3(Random.value*100, Random.value*100,0f));
		addPheromone (new Vector3(Random.value*100, Random.value*100,0f));
		addPheromone (new Vector3(Random.value*100, Random.value*100,0f));
		addPheromone (new Vector3(Random.value*100, Random.value*100,0f));
		addPheromone (new Vector3(Random.value*100, Random.value*100,0f));
		addPheromone (new Vector3(Random.value*100, Random.value*100,0f));
		//TODO: Work out how to pass the grid position to the pheremonenode (so that it shows up in debugger!!)

	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void addPheromone(Vector3 worldPos) {
		Vector3 gridPos = worldToGrid (worldPos);
		//TODO: add concentration to current node rather than recreate (when a node exists here already)
		grid[Mathf.RoundToInt(gridPos.x), 
			 Mathf.RoundToInt(gridPos.y)] = gameObject.AddComponent<PheromoneNode> ();
	}

	public Vector3 gridToWorld(Vector3 gridPos) {
		return new Vector3 (gridPos.x * (worldWidth / gridWidth), gridPos.y * (worldHeight / gridHeight), 0f);
	}

	public Vector3 worldToGrid(Vector3 worldPos) {
		return new Vector3 (worldPos.x / (worldWidth / gridWidth), worldPos.y / (worldHeight / gridHeight), 0f);
	}

}
