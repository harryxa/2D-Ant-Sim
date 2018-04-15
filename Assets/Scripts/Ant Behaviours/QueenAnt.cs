using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueenAnt : AntClass
{

    public List<GameObject> ants;
	public GameObject workerAnt;
	public int antCount;

    private int activeAnts = 0;
    public int activeScouts = 10;
    int antcounter;

    // Use this for initialization
    void Start ()
	{
		this.pGrid = GameObject.FindWithTag("PGrid").GetComponent<PheromoneGrid>();

		this.pGrid.addNest (transform.position);
		antSpeed = 0;
        ants = new List<GameObject>();
		SpawnAnts();


        antcounter = ants.Count;
        Debug.Log(antcounter);
    }
	
	// Update is called once per frame
	void Update ()
	{
        for (int i = 0; i <= antcounter; i++)
        {
            if (activeAnts <= activeScouts)
            {
                if (ants[i].activeSelf == false)
                {
                    ants[i].SetActive(true);
                    activeAnts++;
                }

            }
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
