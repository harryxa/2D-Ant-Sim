using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodManager : MonoBehaviour {

	private PheromoneGrid pGrid;
	int i = 0;
	// Use this for initialization
	void Start () 
	{
		pGrid = GameObject.FindWithTag("PGrid").GetComponent<PheromoneGrid>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (i == 0) {
			pGrid.addFood (new Vector3 (-30, -30, 0));
			i++;
		}
	}
}
