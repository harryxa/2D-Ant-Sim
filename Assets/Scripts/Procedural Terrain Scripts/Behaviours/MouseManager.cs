using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseManager : MonoBehaviour
{

	public GameObject cursor;
	Vector3 lastMousePosition;

	//scrolling or zooming
	float minFov = 10f;
	float maxFov = 100f;
	float sensitivity = 20f;

	Vector3 dragStart;
	Tile tileSelected;

	// Use this for initialization
	void Start ()
	{
		tileSelected = new Tile (Tile.Type.Smooth_Stone);
	}

	// Update is called once per frame
	void Update ()
	{
		Vector3 currMousePos = Camera.main.ScreenToWorldPoint (Input.mousePosition);

		//update cursor position
		Tile tileUnderMouse = GetTileAtWorldCoord (currMousePos);
		if (tileUnderMouse != null) {
			cursor.SetActive (true);
			Vector3 cursorPosition = new Vector3 (tileUnderMouse.X + 0.5f, tileUnderMouse.Y + 0.5f, 0);
			cursor.transform.position = cursorPosition;
		} else {
			cursor.SetActive (false);
		}

		if (Input.GetMouseButtonDown (0)) {
			dragStart = currMousePos;
		}


		// left click mouse click
		if (Input.GetMouseButtonUp (0)) {

			Tile previousTile = null;
			int startX = Mathf.FloorToInt (dragStart.x);
			int endX = Mathf.FloorToInt (currMousePos.x);

			int startY = Mathf.FloorToInt (dragStart.y);
			int endY = Mathf.FloorToInt (currMousePos.y);

			if (endX < startX) {
				int temp = endX;
				endX = startX;
				startX = temp;
			}
			if (endY < startY) {
				int temp = endY;
				endY = startY;
				startY = temp;
			}

			for (int x = startX; x <= endX; x++) {
				for (int y = startY; y <= endY; y++) {
					Tile t = World.instance.GetTileAt (x, y);

					if (t != null) {
						t.type = tileSelected.type;
						t.wall = tileSelected.wall;
					}
				}
			}


			for (int x = startX; x <= endX; x++) {
				for (int y = startY; y <= endY; y++) {
					Tile t = World.instance.GetTileAt (x, y);

					if (t != null) {
						if (previousTile == null) {
							previousTile = t;
							World.instance.OnTileTypeChange (t.chunkNumber);
						}

						if (t.chunkNumber != previousTile.chunkNumber)
							World.instance.OnTileTypeChange (t.chunkNumber);
						

						previousTile = t;
					}
				}
			}
			World.instance.OnMountainChange ();
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
	}



	Tile GetTileAtWorldCoord (Vector3 coord)
	{
		int x = Mathf.FloorToInt (coord.x);
		int y = Mathf.FloorToInt (coord.y);

		return World.instance.GetTileAt (x, y);
	}



	//BUTTON FUNCTIONALITY
	public void OnStone ()
	{
		tileSelected.type = Tile.Type.Smooth_Stone;
	}

	public void OnGrass ()
	{
		tileSelected.type = Tile.Type.Grass;
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
