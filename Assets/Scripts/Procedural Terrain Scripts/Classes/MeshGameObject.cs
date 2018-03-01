using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGameObject : MonoBehaviour 
{
	public int x;

	public int X {
		get {
			return x;
		}
		set {
			x = value;
		}
	}

	public int y;

	public int Y {
		get {
			return y;
		}
		set {
			y = value;
		}
	}

	public int width;

	public int Width {
		get {
			return width;
		}
		set {
			width = value;
		}
	}

	public int height;

	public int Height {
		get {
			return height;
		}
		set {
			height = value;
		}
	}


}
