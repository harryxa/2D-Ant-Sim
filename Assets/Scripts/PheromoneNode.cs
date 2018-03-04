﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PheromoneNode : MonoBehaviour 
{

	public float concentration;
	public float defaultConc = 20f;
	private float maxConc = 100f;
	protected float evaporationRate = 5f;
	public float defaultScale = 0.5f;
	//public bool exists = true;
	public int gridX;
	public int gridY;

	void Start () 
	{
		concentration = defaultConc;
	}
	
	void FixedUpdate () 
	{
		if (concentration > 0f) 
		{
			if (concentration > maxConc)
				concentration = maxConc;
			
			concentration -= evaporationRate * Time.deltaTime;
			float scale = (concentration / defaultConc) * defaultScale; 

			transform.localScale = new Vector3(scale,scale,scale);
		} 
		else 
		{
			Destroy(gameObject);
		}
	}

	public void boostConc() 
	{
		concentration += defaultConc;
	}

	public void setXY(int x, int y)
	{
		gridX = x;
		gridY = y;
	}
}
