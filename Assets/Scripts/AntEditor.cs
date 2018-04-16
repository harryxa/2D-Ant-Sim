using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntEditor : MonoBehaviour
{
    private List<GameObject> ants;
    private QueenAnt queenAnt;

    public float antSpeed;

    // Use this for initialization
    void Start()
    {
        queenAnt = GameObject.FindWithTag("Queen").GetComponent<QueenAnt>();
        


        for (int i = 0; i <= queenAnt.antCount; i++)
        {
            ants.Add(queenAnt.ants[i]);
        }

    }

    // Update is called once per frame
    void Update()
    {
        //for (int i = 0; i <= queenAnt.antcounter - 1; i++)
        //{
        //    ants[i].GetComponent<AntClass>().SetAntSpeed(1f);
        //}
    }
}
