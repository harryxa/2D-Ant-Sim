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
    public int antcounter;

    public float workerAntSpeed;

    // Use this for initialization
    void Start ()
	{
		this.pGrid = GameObject.FindWithTag("PGrid").GetComponent<PheromoneGrid>();

		this.pGrid.addNest (transform.position);
		antSpeed = 0;
        ants = new List<GameObject>();
		SpawnAnts();
        workerAntSpeed = 2f;

        antcounter = ants.Count;
        Debug.Log(antcounter);
    }
	
	// Update is called once per frame
	void Update ()
	{
        for (int i = 0; i <= antcounter - 1; i++)
        {
            if (activeAnts <= activeScouts)
            {
                if (ants[i].activeSelf == false)
                {
                    ants[i].SetActive(true);
                    activeAnts++;
                    //ants[i].GetComponent<AntClass>().SetAntSpeed(1f);
                }

            }

            ants[i].GetComponent<AntClass>().SetAntSpeed(workerAntSpeed);
            //else if (activeAnts >= activeScouts)
            //{
            //    if(ants[i].activeSelf == true)
            //    {
            //        ants[i].SetActive(false);
            //        activeAnts--;
            //    }

            //}
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
