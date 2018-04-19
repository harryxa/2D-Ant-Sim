using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

//instantiate all pheromones at start
//array of list for pheromone grid all equals 0 to start
//list contains different pheromone types

//OR

//have an all encompasing pheromone node that contains the conc. of all types

public class AntClass : MonoBehaviour
{

    SpriteRenderer m_SpriteRenderer;

    //debug stuff
    public bool canSecrete = true;

	public enum AntState
	{
        SCOUTING,
		CARRYING,
		GATHERING,
        NESTING,
        COLLECTINGFOOD,
        DEBUG
    };

	public AntState state;
    //basic properties of an ant

	protected bool firstStepsTaken = false;
	protected bool stepped;

	//wander variables for random target
	protected float jitterScale = 0.5f;
	protected float wanderDistance = 1f;
	protected Vector3 targetPosition;
	protected float distanceRadius = 0.1f;
	protected float rotateSpeed = 20f;

	//bounding box floats
	float worldHeight;
	float worldWidth;
	float halfWorldHeight;
	float halfWorldWidth;

	//Vector3 nextPosition;
	public float antSpeed = 20f;

	protected int limit = 5;
	protected int limiti;

	//SMELLING VARIABLES
	private int smellRadius = 6;
	private float smellStrength = 1f;

    //WALL AVOIDANCE
	protected bool leftNext;
	protected int leftTurnsTried = 0;
	protected int rightTurnsTried = 0;
	protected float turningAngle = 0.5f;
	protected float angleMultiplier = 1.1f;

    //FOOD RELATED SUCH AND SUCH
    private Food foodItem;    
    private Vector3 foodPosition;
    public bool smellingFood = false;
    public bool carryingFood;

    //SMELLING
    protected PheromoneGrid pGrid;
    private Vector3 smellDirection;
    private Vector3 pherDir;
    public float carryPheromoneCount;
   
    private NestManager nest;

    public float hunger;
    public bool nesting;


    public float debugcarrycount = 0f;

    void Start ()
	{
        m_SpriteRenderer = GetComponent<SpriteRenderer>();

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

        nest = GameObject.FindWithTag("Nest").GetComponent<NestManager>();

        //state = AntState.DEBUG;
        if(state == AntState.DEBUG)
        {
            antSpeed = 2;
        }

        hunger = Random.Range(50, 100);


        carryingFood = false;
		stepped = false;
	}

	void Update ()
	{
        //secrete pheromone when target destination is reached
        if (canSecrete == true)
        {
			Secrete ();
			canSecrete = false;
		}
		if (firstStepsTaken)
			Move ();
		else
        {			
			Move ();
		}

        ChangeAntColour();
        StateActions();
        smellingFood = false;
        CollectingFood();
        hunger -= 1 * Time.deltaTime;
        Hunger();
        //Debug.DrawLine(transform.position, transform.position + SmellDirection());
    }

    //Sets the target for the ant then moves the ant to said target
    //secrete set to true, ant deposits another pheromone at target pos (set to false in update)
    //if WANDERING, smell for food and randomly wander around the map
    //if CARRYING, follow pheromones and move towards nest
    //if GATHERING smell pheromone and move away from nest
    //if NESTING return to nest and eat, stop scouting if too many scouts
    protected void Move()
    {
        Vector3 target = Vector3.zero;

        if (TargetReached(targetPosition, distanceRadius))
        {
            canSecrete = true;

            switch (state)
            {
                case AntState.SCOUTING:
                    target = SmellDirection() + RandomTarget();
                    target.Normalize();
                    SetTarget(target);
                    break;

                case AntState.CARRYING:
                    target = SmellDirection() + RandomTarget() + NestDirection();
                    target.Normalize();
                    SetTarget(target);
                    //Debug.DrawLine(transform.position, transform.position + SmellDirection());
                    break;

                case AntState.GATHERING:
                    target = SmellDirection() * 1f - NestDirection() * 0.9f + RandomTarget();
                    target.Normalize();
                    SetTarget(target);
                    //Debug.DrawLine(transform.position, transform.position - SmellDirection());
                    break;

                case AntState.NESTING:
                    target = SmellDirection() + RandomTarget() + NestDirection();
                    target.Normalize();
                    SetTarget(target);
                    break;

                case AntState.DEBUG:
                    SetTarget(SmellDirection());
                    break;


            }
            FixTarget(); //....
        }

        //turns the ant to look where theyre going
        Steer();
        float step = antSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
    }

    //detects pheremones within range of ant
    protected Vector3 SmellDirection()
    {
        //gets the ants location on the grid
        Vector3 antGridPos = pGrid.worldToGrid(transform.position);
        int gridX = Mathf.RoundToInt(antGridPos.x);
        int gridY = Mathf.RoundToInt(antGridPos.y);

        //smellDirection = ants position

        //CHANGES HERE

        //smellDirection = new Vector3(antGridPos.x, antGridPos.y, 0f);
        smellDirection = Vector3.zero;

        carryPheromoneCount = 0f;
        debugcarrycount = 0f;

        //loops through grid in square shape around ant
        //creates a vector, pherDir, from the ant to the node on the grid
        //limits the search to a radius around the ant, rather than a square. checks its in world
        //if valid grid position
        for (int x = gridX - smellRadius; x <= gridX + smellRadius; x++)
        {
            for (int y = gridY - smellRadius; y <= gridY + smellRadius; y++)
            {                
                pherDir = new Vector3(x - gridX, y - gridY, 0);
                
                if (x >= 0 && x < worldWidth && y >= 0 && y < worldHeight && pherDir.magnitude <= smellRadius)
                {                    
                    if (pGrid.grid[x, y] != null)
                    {
                        //if WANDERING smell for food and randomly wander around the map
                        //set state to carry if food found                        
                        if (state == AntState.SCOUTING)
                        {
                            //if carryPheromones are smelt change to gathering
                            if (pGrid.grid[x, y].GetComponent<PheromoneNode>().carryConcentration > 10)
                            {
                                state = AntState.GATHERING;
                            }
                            SmellForFood(x, y);                           
                            
                        }
                        //if CARRYING, head home
                        else if (state == AntState.CARRYING)
                        {
                            if (pGrid.grid[x, y].GetComponent<PheromoneNode>().carryConcentration >= 1 || pGrid.grid[x, y].GetComponent<PheromoneNode>().pheromoneConcentration >= 1)
                            {
                                debugcarrycount += pGrid.grid[x, y].GetComponent<PheromoneNode>().carryConcentration;

                                if(debugcarrycount < 200)
                                {
                                    pherDir.Normalize();
                                    pherDir *= pGrid.grid[x, y].GetComponent<PheromoneNode>().pheromoneConcentration;
                                    smellDirection += pherDir;
                                }
                                else
                                {
                                    pherDir.Normalize();
                                    pherDir *= pGrid.grid[x, y].GetComponent<PheromoneNode>().pheromoneConcentration + pGrid.grid[x, y].GetComponent<PheromoneNode>().carryConcentration;
                                    smellDirection += pherDir;
                                }
                            }
                        }
                        //if GATHERING smell pheromones, collect food etc
                        else if (state == AntState.GATHERING)
                        {       
                            if (pGrid.grid[x,y].GetComponent<PheromoneNode>().carryConcentration >= 1)
                            {
                                pherDir.Normalize();
                                pherDir *= pGrid.grid[x, y].GetComponent<PheromoneNode>().carryConcentration;
                                smellDirection += pherDir;
                                carryPheromoneCount += pGrid.grid[x, y].GetComponent<PheromoneNode>().carryConcentration;
                            }
                            SmellForFood(x, y);                         
                        }                       
                        else if(state == AntState.NESTING)
                        {
                            pherDir.Normalize();
                            pherDir *= pGrid.grid[x, y].GetComponent<PheromoneNode>().pheromoneConcentration + pGrid.grid[x, y].GetComponent<PheromoneNode>().carryConcentration;
                            smellDirection += pherDir;
                        }
                        else if(state == AntState.DEBUG)
                        {
                            pherDir.Normalize();
                            pherDir *= pGrid.grid[x, y].GetComponent<PheromoneNode>().pheromoneConcentration;
                            smellDirection += pherDir;
                        }
                    }             
                }
            }
        }
       
        smellDirection.Normalize();        

        return smellStrength * smellDirection;
    }

    public void CollectingFood()
    {
        if(state == AntState.COLLECTINGFOOD)
        {
            carryingFood = true;
            foodItem.reduceFoodAmount();
            state = AntState.CARRYING;            
        }
    }

    public void Hunger()
    {
        if(hunger <= 10)
        {
            state = AntState.NESTING;
        }
    }

    public void StateActions()
    {
        Vector3 targetdest = Vector3.zero;

        switch (state)
        {
            case AntState.SCOUTING:

                if(smellingFood == true)
                {
                    //set state to carry
                    if (TargetReached(foodPosition, 2f))
                    {
                        state = AntState.COLLECTINGFOOD;
                        smellingFood = false;
                    }
                }
               
                break;

            case AntState.CARRYING:
                targetdest = pGrid.worldNestPosition;
                if (TargetReached(targetdest, 2f))
                {
                    state = AntState.GATHERING;
                    nest.StoreFood();
                    carryingFood = false;
                }

                break;

            case AntState.GATHERING:

                if (smellingFood == true)
                {
                    if (TargetReached(foodPosition, 2f))
                    {
                        
                        state = AntState.COLLECTINGFOOD;
                        smellingFood = false;
                    }
                }
                
                else if(carryPheromoneCount < 10)
                {
                    state = AntState.SCOUTING;
                }
                break;

            case AntState.NESTING:
                targetdest = pGrid.worldNestPosition;
                if (TargetReached(targetdest, 2f))
                {
                    nesting = true;                    
                }
                break;
        }
    }

    public Vector3 NestDirection()
    {
        Vector3 targetpos = Vector3.zero;

        if(targetpos != pGrid.worldNestPosition)
        {
            targetpos = pGrid.worldNestPosition;
        }

        Vector3 nestDirection = new Vector3(targetpos.x - transform.position.x, targetpos.y - transform.position.y, 0);
        nestDirection.Normalize();

        return nestDirection;
        //Debug.DrawLine(transform.position, transform.position + nestDirection, Color.black);
    }

    //secretes a pheremone where the ant is currently standing
    private void Secrete ()
    {
        Vector3 antPosition = new Vector3(transform.position.x - 0.5f, transform.position.y - 0.5f, 0);

        if (World.instance.GetTileAt(Mathf.FloorToInt(targetPosition.x + halfWorldWidth), Mathf.FloorToInt(targetPosition.y + halfWorldHeight)).type != Tile.Type.Grass)
        {
            return;
        }
        else
        {
            if (state == AntState.SCOUTING)
            {
                pGrid.addPheromone(antPosition, PheromoneGrid.PheromoneType.STANDARD, 1.0f);
            }

            else if (state == AntState.CARRYING)
            {
                float nestDista = Vector3.Distance(transform.position, pGrid.worldNestPosition);
                if (nestDista > 5f)
                {
                    pGrid.addPheromone(antPosition, PheromoneGrid.PheromoneType.CARRYING, 2.0f);
                }
                //else add more if close to nest, want to draw ant sou tof the nest
                else
                {
                    pGrid.addPheromone(antPosition, PheromoneGrid.PheromoneType.CARRYING, 8f);
                }
            }
        }        
	}

    //if food within search radius a vector to the food is returned
    //else, the vector equals zero
    public void SmellForFood(int x, int y)
    {
        if (state == AntState.SCOUTING)
        {
            if (pGrid.grid[x, y].GetComponent<Food>() != null)
            {
                smellDirection = pherDir;
                smellDirection.Normalize();

                foodItem = pGrid.grid[x, y].GetComponent<Food>();
                foodPosition = foodItem.worldFoodPosition;
                smellingFood = true;
            }
            else
            {
                smellDirection = Vector3.zero;                
            }
        }
        else if (state == AntState.GATHERING)
        {
            if (pGrid.grid[x, y].GetComponent<Food>() != null)
            {
                smellDirection = pherDir;
                smellDirection.Normalize();
                foodItem = pGrid.grid[x, y].GetComponent<Food>();
                foodPosition = foodItem.worldFoodPosition;
                smellingFood = true;
            }           
        }
    }
    
    protected bool CheckTarget ()
	{

		if (targetPosition.x < -halfWorldWidth || targetPosition.x >= halfWorldWidth || targetPosition.y < -halfWorldHeight || targetPosition.y >= halfWorldHeight)
        {
			return false;
		}
        else if (World.instance.GetTileAt (Mathf.FloorToInt (targetPosition.x + halfWorldWidth), Mathf.FloorToInt (targetPosition.y + halfWorldHeight)).type != Tile.Type.Grass)
        {				
			return false;
		}
        else
			return true;
	}

    //if an unwalkable surface has been reached, turn the ant untill they can move
    protected void FixTarget ()
	{
		if (!CheckTarget ())
        {
			//generate new vector for ant as theyve found an unwalkable surface
			Vector3 targetVector = targetPosition - transform.position;

			int turnsTried;

			if (leftNext)
            {
				turnsTried = leftTurnsTried;
			}
            else
				turnsTried = rightTurnsTried;

			float angle = turningAngle * Mathf.Pow (angleMultiplier, turnsTried - 1);

			if (!leftNext)
				angle *= -1;

			Quaternion r = Quaternion.Euler (0, 0, angle);	
			targetVector = r * targetVector;
			targetPosition = transform.position + targetVector;

			if (!CheckTarget ())
            {
				if (leftNext)
					leftTurnsTried++;
				else
					rightTurnsTried++;
				leftNext = !leftNext;
				FixTarget ();
			}
            else
				ResetTargetCheck ();
		} 
	}

	protected void ResetTargetCheck ()
	{
		leftTurnsTried = 0;
		rightTurnsTried = 0;

		if (Random.Range (-1, 1f) > 0)
			leftNext = true;
		else
			leftNext = false;
	}

    //Creates a random Vector for ant exploration
    //Used for ant movements
	public Vector3 RandomTarget ()
	{
		Vector3 target = new Vector3 (Random.Range (-1f, 1f), Random.Range (-1f, 1f), 0f);

		//make the Target fit on circle again
		target.Normalize ();
		target *= jitterScale;

		return target;
	}

	//Determines whether the ant has reached its target
	protected bool TargetReached (Vector3 targetPos, float distanceRad)
	{
		return 
			transform.position.y < targetPos.y + distanceRad &&
		    transform.position.y > targetPos.y - distanceRad &&
		    transform.position.x < targetPos.x + distanceRad &&
		    transform.position.x > targetPos.x - distanceRad;
	}

    //Sets the target for the ant to move to, taking in various vectors.
    //Such as smell direction, random directions and nest related vectors
	protected void SetTarget (Vector3 target)
	{
		targetPosition = transform.position + transform.up * wanderDistance + target;
    }

    protected void Steer ()
	{
		Vector3 direction = targetPosition - transform.position;
		float angle = Mathf.Atan2 (direction.y, direction.x) * Mathf.Rad2Deg;
		transform.rotation = Quaternion.Lerp (transform.rotation, 
			Quaternion.Euler (0f, 0f, angle - 90f), 
			rotateSpeed * Time.deltaTime);	

		//Debug.DrawLine (transform.position, targetPosition);
	}

    private void ChangeAntColour()
    {

        if(state == AntState.SCOUTING)
        {
            if(m_SpriteRenderer.color != Color.red)
                m_SpriteRenderer.color = Color.red;
        }
        else if(state == AntState.CARRYING)
        {
            if (m_SpriteRenderer.color != Color.yellow)
                m_SpriteRenderer.color = Color.yellow;
        }
        else if(state == AntState.GATHERING)
        {
            if (m_SpriteRenderer.color != Color.magenta)
                m_SpriteRenderer.color = Color.magenta;
        }
    }

    public void SetAntSpeed(float _antSpeed)
    {
        antSpeed = _antSpeed;
    }


}
