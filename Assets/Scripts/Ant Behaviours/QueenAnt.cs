using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueenAnt : AntClass
{

    public List<GameObject> ants;
	public GameObject workerAnt;
	public int antCount;
	private float timer = 0;

    private int activeAnts = 0;
    public int activeScouts = 0;

	// Use this for initialization
	void Start ()
	{
		this.pGrid = GameObject.FindWithTag("PGrid").GetComponent<PheromoneGrid>();

		this.pGrid.addNest (transform.position);
		antSpeed = 0;
		SpawnAnts();
        ants = new List<GameObject>();

        for (int i = 0; i <= ants.Count; i++)
        {
            if(activeAnts <= activeScouts )
            {
                if(ants[i].active == false)
                {

                }
            }
        }
    }
	
	// Update is called once per frame
	void Update ()
	{
        for(int i = 0; i <= ants.Count; i++)
        {

        }
	}

	void SpawnAnts ()
	{
		for(int i = 0; i <= antCount; i++)
        {
            GameObject ant = Instantiate(workerAnt, transform.position, Quaternion.Euler(0, 0, Random.value * 360));
            ants.Add(ant);
            ants[i].SetActive(false);
            
        }
    }

}
