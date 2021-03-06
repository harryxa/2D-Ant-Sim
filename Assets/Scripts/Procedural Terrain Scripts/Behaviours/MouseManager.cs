﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;

public class MouseManager : MonoBehaviour
{

	public GameObject cursor;
	Vector3 lastMousePosition;
    public float cursorClickOffset;
	//scrolling or zooming
	float minFov = 10f;
	float maxFov = 100f;
	float sensitivity = 20f;

	Vector3 dragStart;
	Tile tileSelected;

    private bool pheromone;
    private bool food;
    private bool ant;
    private bool placeTile;
    private bool carryP;


    protected PheromoneGrid pGrid;

    
    // Use this for initialization
    void Start ()
	{
		tileSelected = new Tile (Tile.Type.Smooth_Stone);
        pheromone = false;
        ant = false;
        food = false;
        placeTile = false;
        pGrid = GameObject.FindWithTag("PGrid").GetComponent<PheromoneGrid>();
    }

    // Update is called once per frame
    void Update ()
	{
        Vector3 currMousePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);
        Vector3 currMousePosAnts = currMousePos;
        currMousePos = new Vector3 (currMousePos.x + World.instance.tileGridWidth/2f, currMousePos.y + World.instance.tileGridHeight/2f, currMousePos.z);
        
		//update cursor position
		Tile tileUnderMouse = GetTileAtWorldCoord (currMousePos);
		if (tileUnderMouse != null) {
			cursor.SetActive (true);
			Vector3 cursorPosition = new Vector3 (tileUnderMouse.X + 0.5f, tileUnderMouse.Y + 0.5f, 0);
			cursorPosition = new Vector3 (cursorPosition.x, cursorPosition.y, cursorPosition.z);

			cursor.transform.position = cursorPosition;
		} else {
			cursor.SetActive (false);
		}


        //if over ui bail out

		if (Input.GetMouseButtonDown (0)) {
			dragStart = currMousePos;
		}

        if (!EventSystem.current.IsPointerOverGameObject())
        {
            // left click mouse click
            if (Input.GetMouseButtonUp(0))
            {
                Vector3 placement = new Vector3(currMousePosAnts.x + cursorClickOffset, currMousePosAnts.y + cursorClickOffset, 0);


                if (pheromone == true)
                {
                    pGrid.AddPheromone(placement, PheromoneGrid.PheromoneType.STANDARD, 1.0f);
                }
                else if (food == true)
                {
                    pGrid.addFood(placement);
                }
                else if (ant == true)
                {

                }
                else if (carryP == true)
                {
                    pGrid.AddPheromone(placement, PheromoneGrid.PheromoneType.CARRYING, 3f);
                }




                else if (placeTile == true)
                {
                    Tile previousTile = null;
                    int startX = Mathf.FloorToInt(dragStart.x);
                    int endX = Mathf.FloorToInt(currMousePos.x);

                    int startY = Mathf.FloorToInt(dragStart.y);
                    int endY = Mathf.FloorToInt(currMousePos.y);

                    if (endX < startX)
                    {
                        int temp = endX;
                        endX = startX;
                        startX = temp;
                    }
                    if (endY < startY)
                    {
                        int temp = endY;
                        endY = startY;
                        startY = temp;
                    }

                    for (int x = startX; x <= endX; x++)
                    {
                        for (int y = startY; y <= endY; y++)
                        {
                            Tile t = World.instance.GetTileAt(x, y);

                            if (t != null)
                            {
                                t.type = tileSelected.type;
                                t.wall = tileSelected.wall;
                            }
                        }
                    }


                    for (int x = startX; x <= endX; x++)
                    {
                        for (int y = startY; y <= endY; y++)
                        {
                            Tile t = World.instance.GetTileAt(x, y);

                            if (t != null)
                            {
                                if (previousTile == null)
                                {
                                    previousTile = t;
                                    World.instance.OnTileTypeChange(t.chunkNumber);
                                }

                                if (t.chunkNumber != previousTile.chunkNumber)
                                    World.instance.OnTileTypeChange(t.chunkNumber);

                                previousTile = t;
                            }
                        }
                    }
                    //World.instance.OnMountainChange();
                }
            }


        }
       

		//screen dragging using middle and right click
		if (Input.GetMouseButton (1) || Input.GetMouseButton (2)) {
			Vector3 diff = lastMousePosition - currMousePos;
			Camera.main.transform.Translate (diff);
		}

		//mouse scrolling
		float fov = Camera.main.orthographicSize;
		fov -= Input.GetAxis ("Mouse ScrollWheel") * sensitivity;
		fov = Mathf.Clamp (fov, minFov, maxFov);
		Camera.main.orthographicSize = fov;

		lastMousePosition = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		lastMousePosition = new Vector3 (lastMousePosition.x + World.instance.tileGridWidth/2f, lastMousePosition.y + World.instance.tileGridHeight/2f, lastMousePosition.z);

	}

    Tile GetTileAtWorldCoord (Vector3 coord)
	{
		int x = Mathf.FloorToInt (coord.x);
		int y = Mathf.FloorToInt (coord.y);

		return World.instance.GetTileAt (x, y);
	}



    public void PlacePheromone()
    {
        ant = false;
        pheromone = true;
        food = false;
        placeTile = false;
        carryP = false;
    }

    public void PlaceFood()
    {
        ant = false;
        pheromone = false;
        food = true;
        placeTile = false;
        carryP = false;
    }

    public void PlaceAnt()
    {
        ant = true;
        pheromone = false;
        food = false;
        placeTile = false;
        carryP = false;
    }
    public void PlaceCarryP()
    {
        ant = false;
        pheromone = false;
        food = false;
        placeTile = false;
        carryP = true;
    }





    //BUTTON FUNCTIONALITY
    public void OnStone ()
	{
		tileSelected.type = Tile.Type.Smooth_Stone;
        placeTile = true;
	}

	public void OnGrass ()
	{
		tileSelected.type = Tile.Type.Grass;
        placeTile = true;

    }

    public void OnSand ()
	{
		tileSelected.type = Tile.Type.Sand;
	}

	public void OnShallowWater ()
	{
		tileSelected.type = Tile.Type.Shallow_Water;
	}

	public void OnDeepWater ()
	{
		tileSelected.type = Tile.Type.Deep_Water;
	}

	public void OnDirt ()
	{
		tileSelected.type = Tile.Type.Dirt;
	}

	public void OnMountains ()
	{
		tileSelected.wall = Tile.Wall.Brick;
		tileSelected.type = Tile.Type.Smooth_Stone;
	}

	public void OnRemoveMountains ()
	{
		tileSelected.wall = Tile.Wall.Empty;
	}

}
