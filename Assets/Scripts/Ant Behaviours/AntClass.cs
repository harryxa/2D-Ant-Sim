using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class AntClass : MonoBehaviour 
{
	//debug stuff
	public bool setSecrete = true;


	//basic properties of an ant

	private float health;			//the amount of health the ant has
	private float hungerThreshold;	//the threshold below which the ant will be hungry
	private float healthRate;		//rate at which health decreases
	private bool alive = true;		//determines whether the ant is alive or not
	List<int> itemsInView;			//an array of items within view e.g. ants or food
	List<int> pheremonesInRange;	//an array of pheremones within range

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
	public int smellRadius = 10;
	public float smellStrength = 0.6f;

	// Use this for initialization
	void Start () 
	{
		limiti = Random.Range (0, limit);
		transform.rotation = Quaternion.Euler (0f, 0f, Random.Range(0, 360));

		//find the pheromone grid, where pheromone data is stored.
		pGrid = GameObject.FindWithTag("PGrid").GetComponent<PheromoneGrid>();

		worldHeight = pGrid.getWorldHeight () / 2;
		worldWidth = pGrid.getWorldWidth () / 2;

		//set ants initial target vector to its own position, rather than 0,0,0
		targetPosition = new Vector3 (transform.position.x, transform.position.y, 0f);
	}
	
	// Update is called once per frame
	void Update () 
	{
		boundingBox();

		if (setSecrete == true)
			secrete ();
		
		moveAnt ();
	}

	//detects pheremones within range of ant
	private Vector3 SmellDirection()
	{
		//gets the ants location on the grid
		Vector3 antGridPos = pGrid.worldToGrid(transform.position);
		int gridX = Mathf.RoundToInt(antGridPos.x);
		int gridY = Mathf.RoundToInt(antGridPos.y);

		Vector3 smellDirection = new Vector3 (antGridPos.x, antGridPos.y,0f);


		//loops through grid in square shape around ant
		for(int x = gridX - smellRadius; x <= gridX + smellRadius; x++)
		{
			for(int y = gridY - smellRadius; y <= gridY + smellRadius; y++)
			{	
				//creates a vector from the ant to the node on the grid
				Vector3 pherDir = new Vector3 (x, y, 0) - new Vector3 (gridX, gridY,0);	
				Vector3 foodDir = new Vector3 (x, y, 0) - new Vector3 (gridX, gridY,0);	
				Vector3 wrappedPherPos = pGrid.wrapGridCoord (new Vector3 (x, y, 0));

				//limits the search to a radius around the ant, rather than a square
				if (pherDir.magnitude <= smellRadius) 
				{	
					if (pGrid.grid [(int)wrappedPherPos.x, (int)wrappedPherPos.y] != null) 
					{
						PheromoneNode n = pGrid.grid [(int)wrappedPherPos.x, 
													  (int)wrappedPherPos.y].GetComponent<PheromoneNode> ();
						
						Food f = pGrid.grid [(int)wrappedPherPos.x, (int)wrappedPherPos.y].GetComponent<Food> ();
						
						pherDir.Normalize ();
						foodDir.Normalize ();

						pherDir *= n.concentration;

						if (f == null)
							smellDirection += pherDir;

						else if (f != null) 
						{			
							foodDir *= f.concentration;
							smellDirection += foodDir;
						}
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
	void secrete ()
	{
		if (limit >= limiti) {
			pGrid.addPheromone (transform.position);
			limiti = 0;
		} else
			limiti++;		
	}



	//TODO fix movement, trying to sort smeel direction to only get called when ants are at taregt location

	//TODO: ants follow different pheromones depending on the situation
	//moves the ant in the direction its facing
	private void moveAnt()
	{			
		//SetTarget(SmellDirection());
		//SetTarget (RandomTarget());

	
			SetTarget (SmellDirection () + RandomTarget ());

			steer ();
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

	//sets ants target vector
	void SetTarget(Vector3 target)
	{
		if ((transform.position.y < targetPosition.y + distanceRadius && 
			transform.position.y > targetPosition.y - distanceRadius && 
			transform.position.x < targetPosition.x + distanceRadius && 
			transform.position.x > targetPosition.x - distanceRadius) ) 
		{
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
