using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PheromoneNode : MonoBehaviour 
{

	public float concentration;
	private float defaultConc = 10f;
	private float maxConc = 100f;
	private float evaporationRate = 5f;
	private float defaultScale = 1f;
	//public bool exists = true;
	public int gridX;
	public int gridY;
	private PheromoneGrid pGrid;
	public int ID;

	//private Transform pheromone;

	void Start () 
	{
		concentration = defaultConc;
	}
	
	// Update is called once per frame
	void FixedUpdate () 
	{
		if (concentration > 0f) {
			if (concentration > maxConc)
				concentration = maxConc;
			
			concentration -= evaporationRate * Time.deltaTime;
			float scale = (concentration / defaultConc) * defaultScale; 

			transform.localScale = new Vector3(scale,scale,scale);

		} else {
			Destroy(gameObject);
		}
	}

	public void boostConc() {
		concentration += defaultConc;
	}

	public void setXY(int x, int y)
	{
		gridX = x;
		gridY = y;
	}

	public void setGrid(PheromoneGrid g)
	{
		pGrid = g;
	}

	public void setID(int id) {
		ID = id;
	}

}
