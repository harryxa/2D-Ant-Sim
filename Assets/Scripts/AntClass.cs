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
	//public Vector3 target;			//the target coord of an item e.g. food
	List<int> itemsInView;			//an array of items within view e.g. ants or food
	List<int> pheremonesInRange;	//an array of pheremones within range
	//prioratisedDirection;			//the direction the ant wants to move in

	//wander variables for getSteering()
	private float jitterScale = 1f;
	private float wanderDistance = 2f;
	private Vector3 wanderTarget;
	private Vector3 targetPosition;
	private float distanceRadius = 0.1f;	//box collider
	private float rotateSpeed = 5f;

	private PheromoneGrid grid;

	//navigation
	private Vector3 nextPosition;
	private float antSpeed = 5f;

	private int limit = 5;
	private int limiti;

	// Use this for initialization
	void Start () 
	{		
		//stuff for the wander behavior for getSteering()
		//float theta = Random.value * 2 * Mathf.PI;
		//create a vector to a target position on the wander circle
		//wanderTarget = new Vector3(wanderRadius * Mathf.Cos(theta), wanderRadius * Mathf.Sin(theta), 0f);
		wanderTarget = new Vector3(0,0,0);

		limiti = Random.Range (0, limit);

		transform.rotation = Quaternion.Euler (0f, 0f, Random.Range(0, 360));
		//navigation
		grid = GameObject.FindWithTag("PGrid").GetComponent<PheromoneGrid>();
	

	}
	
	// Update is called once per frame
	void Update () 
	{
		boundingBox();
		secrete ();
		steer ();
		setDestination ();
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
	void FindFoodTarget()
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

	//detects pheremones within range, populates pheremonesInRange
	void Smell()
	{

	}

	//secretes a pheremone where the ant is currently standing
	void secrete()
	{
		if (limit >= limiti) {
			grid.addPheromone (transform.position);
			limiti = 0;
		} else
			limiti++;
		
	}

	//decides how the ant moves e.g. follow a pheremone or wander randomly
	void Wander()
	{
		
	}

	//moves the ant in the direction its facing
	private void setDestination()
	{						
		nextPosition = steer();

		float step = antSpeed * Time.deltaTime;
		transform.position = Vector3.MoveTowards (transform.position, nextPosition, step);
	}

	private void boundingBox()
	{
		if (transform.position.x < -50) 
		{
			transform.position = new Vector3(49, transform.position.y,0);
			targetPosition = transform.position;
		}
		else if (transform.position.x > 50) 
		{
			transform.position =  new Vector3(-49, transform.position.y,0);
			targetPosition = transform.position;
		}
		else if (transform.position.y < -50) 
		{
			transform.position =  new Vector3(transform.position.x, 49,0);
			targetPosition = transform.position;
		}
		else if (transform.position.y > 50) 
		{
			transform.position = new Vector3(transform.position.x, -49,0);
			targetPosition = transform.position;
		}
	}

//	void faceDirection()
//	{
//		Quaternion rot = new Quaternion ();
//		rot.SetFromToRotation (transform.position, nextPosition);
//		transform.rotation = rot;
//	}

	//kill the ant and remove it from the simulation.
	void Die()
	{

	}


	public Vector3 steer ()
	{
		if ((transform.position.y < targetPosition.y + distanceRadius && 
			transform.position.y > targetPosition.y - distanceRadius && 
			transform.position.x < targetPosition.x + distanceRadius && 
			transform.position.x > targetPosition.x - distanceRadius) ) {//|| targetPosition.y == 0f && targetPosition.x == 0f 


			//add a small random vector to the target's position
			wanderTarget = new Vector3 (Random.Range (-1f, 1f) , Random.Range (-1f, 1f), 0f);

			//make the wanderTarget fit on the wander circle again
			wanderTarget.Normalize ();
			wanderTarget *= jitterScale;

			//move the target in front of the character
			targetPosition = transform.position + transform.up * wanderDistance + wanderTarget;

		}
			Vector3 direction = targetPosition - transform.position;
			float angle = Mathf.Atan2 (direction.y, direction.x) * Mathf.Rad2Deg;
			transform.rotation = Quaternion.Lerp(transform.rotation, 
			Quaternion.Euler(0f, 0f, angle - 90f), 
			rotateSpeed*Time.deltaTime);
		

		Debug.DrawLine (transform.position, targetPosition);

		return targetPosition;
	}

}
