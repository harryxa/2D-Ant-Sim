using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class AntClass : MonoBehaviour 
{
	//basic properties of an ant

	//species;						//species of ant (not sure if useful)
	public int ID;					//unique identifier
	//colony;						//which colony the ant is a member of
	//nest;							//where the ant was born
	private float health;			//the amount of health the ant has
	private float hungerThreshold;	//the threshold below which the ant will be hungry
	private float healthRate;		//rate at which health decreases
	private bool alive = true;				//determines whether the ant is alive or not
	//public int goal;				//the current goal the ant is trying to complete
	//public Vector3 target;		//the target coord of an item e.g. food
	List<int> itemsInView;			//an array of items within view e.g. ants or food
	List<int> pheremonesInRange;	//an array of pheremones within range
	//prioratisedDirection;			//the direction the ant wants to move in

	//wander variables for random target
	private float jitterScale = 0.5f;
	private float wanderDistance = 2f;
	private Vector3 targetPosition;
	private float distanceRadius = 0.1f;	//box collider
	private float rotateSpeed = 5f;


	private PheromoneGrid pGrid;

	//bounding box floats
	float worldHeight;
	float worldWidth;

	//navigation
	private Vector3 nextPosition;
	public float antSpeed = 7f;

	private int limit = 5;
	private int limiti;

	//smelling
	private int smellRadius = 10;
	private float smellStrength = 0.6f;

	// Use this for initialization
	void Start () 
	{		
		//stuff for the wander behavior for getSteering()
		//float theta = Random.value * 2 * Mathf.PI;
		//create a vector to a target position on the wander circle
		//wanderTarget = new Vector3(wanderRadius * Mathf.Cos(theta), wanderRadius * Mathf.Sin(theta), 0f);
		//target = new Vector3(0,0,0);

		limiti = Random.Range (0, limit);

		transform.rotation = Quaternion.Euler (0f, 0f, Random.Range(0, 360));
		//navigation
		pGrid = GameObject.FindWithTag("PGrid").GetComponent<PheromoneGrid>();

		worldHeight = pGrid.getWorldHeight () / 2;
		worldWidth = pGrid.getWorldWidth () / 2;

	}
	
	// Update is called once per frame
	void Update () 
	{
		boundingBox();
		secrete ();
		moveAnt ();

	}

	//add the ant to the map
	void AddToMap()
	{
		
	}

	//remove the ant from the map
	void RemoveFromMap()
	{
		
	}

	//determine whether the ant is hungry
	void IsHungry()
	{

	}

	//take a single peice of food if the ant is standing near it
	void TakeFood()
	{

	}

	//determines if the ant is ontop of the nest (for dropping off food etc.)
	void AtNest()
	{

	}

	//determines whether the ants can see the nest
	void SeeNest()
	{

	}

	//pick a food item to target
	void FindFood(Vector3 foodPosition)
	{

	}

	//move towards a piece of food and use the item of food
	void GetFood()
	{

	}

	//decide how to use a piece of food e.g. eat or carry
	void UseFood()
	{
		
	}

	//scan visable surroundings for items of interest, populates itemsInView
	void Search()
	{
		
	}

	void OnTriggerEnter(Collider food)
	{
		Debug.Log("Collision detected");
	}

	//detects pheremones within range
	private Vector3 SmellDirection()
	{
		//detect pheromone nodes within a radius
		//calculate mean concentration
		Vector3 gridPos = pGrid.worldToGrid(transform.position);

		int gridX = Mathf.RoundToInt(gridPos.x);
		int gridY = Mathf.RoundToInt(gridPos.y);

		Vector3 smellDirection = new Vector3 (0f,0f,0f);
		for(int x = gridX - smellRadius; x <= gridX + smellRadius; x++)
		{
			for(int y = gridY - smellRadius; y <= gridY + smellRadius; y++)
			{
				
				Vector3 pherDir = new Vector3 (x, y, 0) - new Vector3 (gridX, gridY,0);
				Vector3 wrappedPherPos = pGrid.wrapGridCoord (new Vector3 (x, y, 0));

				if (pherDir.magnitude <= smellRadius) 
				{	
					//Debug.Log (x + ","+ y + "  |  " + pherDir.x + ", " + pherDir.y + "  |  " + (int)wrappedPherPos.x + ", " + (int)wrappedPherPos.y);
					if (pGrid.grid [(int)wrappedPherPos.x, (int)wrappedPherPos.y] != null) {
						PheromoneNode n = pGrid.grid [(int)wrappedPherPos.x, 
													  (int)wrappedPherPos.y].GetComponent<PheromoneNode> ();
						pherDir.Normalize ();
						pherDir *= n.concentration;

						smellDirection += pherDir;
					}
				}
			}
		}

		smellDirection.Normalize ();
		Debug.DrawLine (transform.position, transform.position+smellDirection, Color.cyan);
		return smellStrength * smellDirection;
	}


	//TODO: various pheremones sevreted deonding on the situation
	//secretes a pheremone where the ant is currently standing
	void secrete()
	{
		if (limit >= limiti) {
			pGrid.addPheromone (transform.position);
			limiti = 0;
		} else
			limiti++;
		
	}

	//TODO: ants follow different pheromones depending on the situation
	//moves the ant in the direction its facing
	private void moveAnt()
	{			
		//SetTarget(SmellDirection());
		//SetTarget (RandomTarget());
		SetTarget(SmellDirection() + RandomTarget());
		//SetTarget(new Vector3(0,0,0));
		steer();
		float step = antSpeed * Time.deltaTime;
		transform.position = Vector3.MoveTowards (transform.position, targetPosition, step);
	}


	public Vector3 RandomTarget() 
	{
		Vector3 target = new Vector3 (Random.Range (-1f, 1f) , Random.Range (-1f, 1f), 0f);

		//make the Target fit on circle again
		target.Normalize ();
		target *= jitterScale;

		return target;
	}

	//random target destination for the random ant movement
	void SetTarget(Vector3 target)
	{
		if ((transform.position.y < targetPosition.y + distanceRadius && 
			transform.position.y > targetPosition.y - distanceRadius && 
			transform.position.x < targetPosition.x + distanceRadius && 
			transform.position.x > targetPosition.x - distanceRadius) ) 
		{

//			//make the Target fit on circle again
//			target.Normalize ();
//			target *= jitterScale;

			//move the target in front of the character
			targetPosition = transform.position + transform.up * wanderDistance + target;

		}
	}

	//bounds ants within area
	private void boundingBox()
	{
		

		if (transform.position.x < -worldWidth) 
		{
			transform.position = new Vector3(worldWidth, transform.position.y,0);
			targetPosition = transform.position;
		}
		else if (transform.position.x > worldWidth) 
		{
			transform.position =  new Vector3(-worldWidth, transform.position.y,0);
			targetPosition = transform.position;
		}
		else if (transform.position.y < -worldHeight) 
		{
			transform.position =  new Vector3(transform.position.x, worldHeight,0);
			targetPosition = transform.position;
		}
		else if (transform.position.y > worldHeight) 
		{
			transform.position = new Vector3(transform.position.x, -worldHeight,0);
			targetPosition = transform.position;
		}
	}

	//kill the ant and remove it from the simulation.
	void Die()
	{

	}

	//changes the direction that the ant is facing 
	private void steer ()
	{
		Vector3 direction = targetPosition - transform.position;
		float angle = Mathf.Atan2 (direction.y, direction.x) * Mathf.Rad2Deg;
		transform.rotation = Quaternion.Lerp(transform.rotation, 
							 Quaternion.Euler(0f, 0f, angle - 90f), 
							 rotateSpeed*Time.deltaTime);	

		Debug.DrawLine (transform.position, targetPosition);
	}

}
