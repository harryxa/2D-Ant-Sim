using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class AntClass : MonoBehaviour
{
	//debug stuff
	public bool setSecrete = true;

	public enum AntState {WANDERING, CARRYING, GATHERING};
	public AntState state;
	//basic properties of an ant

	private float health;
	//the amount of health the ant has
	private float hungerThreshold;
	//the threshold below which the ant will be hungry
	private float healthRate;
	//rate at which health decreases
	private bool alive = true;
	//determines whether the ant is alive or not
	List<int> itemsInView;
	//an array of items within view e.g. ants or food
	List<int> pheremonesInRange;
	//an array of pheremones within range

	private bool walked = false;

	//wander variables for random target
	private float jitterScale = 0.5f;
	private float wanderDistance =1.5f;
	private Vector3 targetPosition;
	private float distanceRadius = 0.1f;
	//box collider
	private float rotateSpeed = 5f;


	private PheromoneGrid pGrid;

	//bounding box floats
	float worldHeight;
	float worldWidth;
	float halfWorldHeight;
	float halfWorldWidth;

	//navigation
	private Vector3 nextPosition;
	public float antSpeed = 7f;

	private int limit = 5;
	private int limiti;

	//smelling
	public int smellRadius = 20;
	public float smellStrength = 0.2f;

	private bool leftNext;
	private int leftTurnsTried = 0;
	private int rightTurnsTried = 0;

	private float turningAngle = 5;
	private float angleMultiplier = 1.1f;



	// Use this for initialization
	void Start ()
	{
		limiti = Random.Range (0, limit);
		transform.rotation = Quaternion.Euler (0f, 0f, Random.Range (0, 360));

		//find the pheromone grid, where pheromone data is stored.
		pGrid = GameObject.FindWithTag ("PGrid").GetComponent<PheromoneGrid> ();

		halfWorldHeight = pGrid.getWorldHeight () / 2;
		halfWorldWidth = pGrid.getWorldWidth () / 2;

		worldHeight = pGrid.getWorldHeight ();
		worldWidth = pGrid.getWorldWidth ();

		//set ants initial target vector to its own position, rather than 0,0,0
		targetPosition = new Vector3 (transform.position.x, transform.position.y, 0f);
		ResetTargetCheck ();
		if (Random.value > 0.8f)
			state = AntState.WANDERING;
		else
			state = AntState.CARRYING;
	}

	// Update is called once per frame
	void Update ()
	{
		//boundingBox ();

		if (setSecrete == true)
			secrete ();
		if (walked)
			Move();
		else {
			//First movement as random to prevent pheremone issues
			MoveAntRand ();
			walked = true;
		}
	}

	//detects pheremones within range of ant
	private Vector3 SmellDirection ()
	{
		//gets the ants location on the grid
		Vector3 antGridPos = pGrid.worldToGrid (transform.position);
		int gridX = Mathf.RoundToInt (antGridPos.x);
		int gridY = Mathf.RoundToInt (antGridPos.y);

		Vector3 smellDirection = new Vector3 (antGridPos.x, antGridPos.y, 0f);


		//loops through grid in square shape around ant
		for (int x = gridX - smellRadius; x <= gridX + smellRadius; x++) {
			for (int y = gridY - smellRadius; y <= gridY + smellRadius; y++) {	
				//creates a vector from the ant to the node on the grid
				Vector3 pherPos = new Vector3 (x, y, 0);
				Vector3 pherDir = pherPos - new Vector3 (gridX, gridY, 0);	

				//Vector3 pherPos = pGrid.wrapGridCoord (pherPos);


				//limits the search to a radius around the ant, rather than a square
				if (x >= 0 && x < worldWidth &&
					y >= 0 && y < worldHeight &&
					pherDir.magnitude <= smellRadius) 
				{	
					if (pGrid.grid [x, y] != null) {
						PheromoneNode n = pGrid.grid [x, y].GetComponent<PheromoneNode> ();

						Food f = pGrid.grid [x, y].GetComponent<Food> ();

						if (f != null) {		
							smellDirection = pherDir;
							//smellDirection = pGrid.gridToWorld (smellDirection);
							smellDirection.Normalize ();

							Debug.DrawLine (transform.position, transform.position + smellDirection, Color.yellow);
							state = AntState.CARRYING;
							return smellDirection;
						} // effectively else (as this returns)


						pherDir.Normalize ();
						pherDir *= n.concentration;
						smellDirection += pherDir;
					}	
				}
			}
		}

		//smellDirection = pGrid.gridToWorld (smellDirection);
		smellDirection.Normalize ();
		Debug.DrawLine (transform.position, transform.position + smellDirection, Color.cyan);
		return smellStrength * smellDirection;
	}


	//TODO: various pheremones sevreted deonding on the situation
	//secretes a pheremone where the ant is currently standing
	void secrete ()
	{
		if (limit >= limiti) {
			pGrid.addPheromone (transform.position);
			limiti = 0;
		} else
			limiti++;		
	}

	private void Move()
	{
		switch (state) 
		{
		case AntState.WANDERING:
			SetTarget (RandomTarget ());
			break;
		case AntState.CARRYING:
			SetTarget (SmellDirection ());
			break;
		case AntState.GATHERING:
			SetTarget (SmellDirection () + RandomTarget ());
			break;
		}



		FixTarget ();

		steer ();
		float step = antSpeed * Time.deltaTime;
		transform.position = Vector3.MoveTowards (transform.position, targetPosition, step);
	}

	//TODO fix movement, trying to sort smeel direction to only get called when ants are at taregt location

	//TODO: ants follow different pheromones depending on the situation
	//moves the ant in the direction its facing
	private void MoveAnt ()
	{			
		//SetTarget(SmellDirection());
		//SetTarget (RandomTarget());


		SetTarget (SmellDirection () + RandomTarget ());
		//SetTarget (RandomTarget ());

		//boundingBox ();
		FixTarget ();

		steer ();
		float step = antSpeed * Time.deltaTime;
		transform.position = Vector3.MoveTowards (transform.position, targetPosition, step);	
	}

	private void MoveAntRand ()
	{			
		//SetTarget(SmellDirection());
		//SetTarget (RandomTarget());


		//SetTarget (SmellDirection () + RandomTarget ());
		SetTarget (RandomTarget ());

		//boundingBox ();
		FixTarget ();

		steer ();
		float step = antSpeed * Time.deltaTime;
		transform.position = Vector3.MoveTowards (transform.position, targetPosition, step);	
	}

	private bool CheckTarget()
	{

		if (targetPosition.x < -halfWorldWidth || targetPosition.x >= halfWorldWidth || targetPosition.y < -halfWorldHeight || targetPosition.y >= halfWorldHeight) 
		{
			return false;
		} 
		else if(World.instance.GetTileAt (Mathf.FloorToInt (targetPosition.x + halfWorldWidth), Mathf.FloorToInt (targetPosition.y + halfWorldHeight)).type != Tile.Type.Grass)
		{				
			return false;
		}
		else
			return true;
	}

	private void FixTarget()
	{
		if(!CheckTarget()) {
			//generate new vector for ant as theyve found an unwalkable surface
			Vector3 targetVector = targetPosition - transform.position;

			int turnsTried;

			if (leftNext) {
				turnsTried = leftTurnsTried;
			} else
				turnsTried = rightTurnsTried;

			float angle = turningAngle * Mathf.Pow (angleMultiplier, turnsTried - 1);

			if (!leftNext)
				angle *= -1;

			Quaternion r = Quaternion.Euler (0, 0, angle);	
			targetVector = r * targetVector;
			targetPosition = transform.position + targetVector;

			if (!CheckTarget()) {
				if (leftNext)
					leftTurnsTried++;
				else
					rightTurnsTried++;
				leftNext = !leftNext;
				FixTarget ();
			} else
				ResetTargetCheck ();
		} 
	}

	private void ResetTargetCheck()
	{
		leftTurnsTried = 0;
		rightTurnsTried = 0;

		if (Random.Range (-1, 1f) > 0)
			leftNext = true;
		else
			leftNext = false;
	}


	public Vector3 RandomTarget ()
	{
		Vector3 target = new Vector3 (Random.Range (-1f, 1f), Random.Range (-1f, 1f), 0f);

		//make the Target fit on circle again
		target.Normalize ();
		target *= jitterScale;

		return target;
	}

	//sets ants target vector
	void SetTarget (Vector3 target)
	{
		//sets new target position once old target is reached 
		if ((transform.position.y < targetPosition.y + distanceRadius &&
		    transform.position.y > targetPosition.y - distanceRadius &&
		    transform.position.x < targetPosition.x + distanceRadius &&
		    transform.position.x > targetPosition.x - distanceRadius)) {
			//move the target in front of the character
			targetPosition = transform.position + transform.up * wanderDistance + target;
		}
	}

	//bounds ants within area
	private void boundingBox ()
	{
		if (transform.position.x < -halfWorldWidth) {
			transform.position = new Vector3 (halfWorldWidth, transform.position.y, 0);
			targetPosition = transform.position;
		} else if (transform.position.x > halfWorldWidth) {
			transform.position = new Vector3 (-halfWorldWidth, transform.position.y, 0);
			targetPosition = transform.position;
		} else if (transform.position.y < -halfWorldHeight) {
			transform.position = new Vector3 (transform.position.x, halfWorldHeight, 0);
			targetPosition = transform.position;
		} else if (transform.position.y > halfWorldHeight) {
			transform.position = new Vector3 (transform.position.x, -halfWorldHeight, 0);
			targetPosition = transform.position;
		}
	}

	//changes the direction that the ant is facing
	private void steer ()
	{
		Vector3 direction = targetPosition - transform.position;
		float angle = Mathf.Atan2 (direction.y, direction.x) * Mathf.Rad2Deg;
		transform.rotation = Quaternion.Lerp (transform.rotation, 
			Quaternion.Euler (0f, 0f, angle - 90f), 
			rotateSpeed * Time.deltaTime);	

		Debug.DrawLine (transform.position, targetPosition);
	}


}
