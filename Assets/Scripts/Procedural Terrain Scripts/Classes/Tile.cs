using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//tile class represents all the tiles in the  world
public class Tile
{
	public bool changed = false;

	public enum Type
	{
		Dirt,
		Grass,
		Smooth_Stone,
		Deep_Water,
		Shallow_Water,
		Sand,
		Void
	}
	public Type type;

	public enum Wall
	{
		Brick,
		Empty

	}
	public Wall wall;

	//constructor to create and assign tile
	public Tile (Type type, Wall wall = Wall.Empty)
	{
		this.wall = wall;
		this.type = type;
	}

	//Tiles are told which mesh they belong to
	public int chunkNumber;
	public int ChunkNumber {
		get {
			return chunkNumber;
		}
		set {
			chunkNumber = value;
		}
	}

	public int mountainChunkNumber;
	public int MountainChunkNumber {
		get {
			return mountainChunkNumber;
		}
		set {
			mountainChunkNumber = value;
		}
	}

	//Coords for the tile
	int x;
	public int X {
		get {
			return x;
		}
		set {
			x = value;
		}
	}

	int y;
	public int Y {
		get {
			return y;
		}
		set {
			y = value;
		}
	}
}
