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
    protected WorldManager worldManager;
    private SpriteRenderer m_SpriteRenderer;

    //debug stuff
    public bool canSecrete = true;

	public enum AntState
	{
        SCOUTING,
		CARRYING,
		GATHERING,
        NESTING,
        COLLECTINGFOOD,
        DEAD,
        DEBUG
    };

	public AntState state;
    //basic properties of an ant

	protected bool firstStepsTaken = false;
	protected bool stepped;

	//wander variables for random target
	protected float randomWanderMagnitude = 0.5f;
	protected float wanderDistance = 1f;
	protected Vector3 targetPosition;
	protected float distanceRadius = 0.1f;
	protected float rotateSpeed = 20f;

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
	protected float turningAngle = 1f;
	protected float angleMultiplier = 1.1f;

    //FOOD RELATED SUCH AND SUCH
    private Food foodItem;    
    private Vector3 foodPosition;
    public bool smellingFood = false;
    public bool carryingFood;
    private float carryAmount;
    public static float foodTakenFromNest = 0.25f;

    //SMELLING
    protected PheromoneGrid pGrid;
    private Vector3 smellDirection;
    private Vector3 pherDir;
    protected float carryPheromoneCount;
   
    private NestManager nest;

    public float hunger;
    public float maxHunger = 100f;
    public bool atNest;

    private float nestPullStrength = 0.9f;


   

    private float starvationValue = -50f;

    void Start ()
	{
        worldManager = GameObject.FindWithTag("WorldManager").GetComponent<WorldManager>();
        m_SpriteRenderer = GetComponent<SpriteRenderer>();

        limiti = Random.Range (0, limit);
		transform.rotation = Quaternion.Euler (0f, 0f, Random.Range (0, 360));

		//find the pheromone grid, where pheromone data is stored.
		pGrid = GameObject.FindWithTag ("PGrid").GetComponent<PheromoneGrid> ();

		//set ants initial target vector to its own position, rather than 0,0,0
		targetPosition = new Vector3 (transform.position.x, transform.position.y, 0f);
		ResetTargetCheck ();

        nest = GameObject.FindWithTag("Nest").GetComponent<NestManager>();

        //state = AntState.DEBUG;

        hunger = Random.Range(90, maxHunger);


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
        hunger -= 0.25f * Time.deltaTime * worldManager.timeRate;
        Hunger();
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
                    target = SmellDirection() + RandomDirection() - (NestDirection() * 0.05f);
                    target.Normalize();
                    SetTarget(target);
                    break;

                case AntState.CARRYING:
                                                                                  // bit dodgy doing it here
                    target = SmellDirection() + RandomDirection() + (NestDirection() * nestPullStrength);

                    //target = SmellDirection() + NestDirection();
                    target.Normalize();
                    SetTarget(target);
                    //Debug.DrawLine(transform.position, transform.position + SmellDirection());
                    break;

                case AntState.GATHERING:
                    target = SmellDirection() - NestDirection() * nestPullStrength + RandomDirection();
                    target.Normalize();
                    SetTarget(target);
                    //Debug.DrawLine(transform.position, transform.position - SmellDirection());
                    break;

                case AntState.NESTING:
                    target = SmellDirection() + RandomDirection() + NestDirection();
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
        float step = antSpeed * Time.deltaTime * worldManager.timeRate;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
    }

    

    //detects pheremones within range of ant
    protected Vector3 SmellDirection()
    {
        //gets the ants location on the grid
        Vector3 antGridPos = pGrid.worldToGrid(transform.position);
        int gridX = (int)antGridPos.x;
        int gridY = (int)antGridPos.y;

        //smellDirection = ants position

        //CHANGES HERE

        //smellDirection = new Vector3(antGridPos.x, antGridPos.y, 0f);
        smellDirection = Vector3.zero;
        Vector3 negDir = Vector3.zero;

        carryPheromoneCount = 0f;

        //loops through grid in square shape around ant
        //creates a vector, pherDir, from the ant to the node on the grid
        //limits the search to a radius around the ant, rather than a square. checks its in world
        //if valid grid position
        for (int x = gridX - smellRadius; x <= gridX + smellRadius; x++)
        {
            for (int y = gridY - smellRadius; y <= gridY + smellRadius; y++)
            {                
                pherDir = new Vector3(x - gridX, y - gridY, 0);
                
                if (x >= 0 && x < WorldManager.worldWidth && y >= 0 && y < WorldManager.worldHeight && pherDir.magnitude <= smellRadius)
                {                    
                    if (pGrid.grid[x, y] != null)
                    {
                        ////always smell negative pheromones
                        negDir = pherDir;
                        negDir.Normalize();
                        negDir *= -pGrid.grid[x, y].GetComponent<PheromoneNode>().negativeConcentration;

                        carryPheromoneCount += pGrid.grid[x, y].GetComponent<PheromoneNode>().gatheringConcentration;


                        //if WANDERING smell for food and randomly wander around the map
                        //set state to carry if food found                        
                        if (state == AntState.SCOUTING && !smellingFood)
                        {
                            //if carryPheromones are smelt change to gathering
                            if (carryPheromoneCount > 40)
                            {
                                state = AntState.GATHERING;
                            }
                            SmellForFood(x, y);
                        }
                        //if CARRYING, head home
                        else if (state == AntState.CARRYING)
                        {
                            if (pGrid.grid[x, y].GetComponent<PheromoneNode>().gatheringConcentration >= 1 ||
                                pGrid.grid[x, y].GetComponent<PheromoneNode>().pheromoneConcentration >= 1)
                            {

                                if(carryPheromoneCount < 400)
                                {
                                    pherDir.Normalize();
                                    pherDir *= pGrid.grid[x, y].GetComponent<PheromoneNode>().pheromoneConcentration;
                                    smellDirection += pherDir;
                                }
                                else
                                {
                                    pherDir.Normalize();
                                    pherDir *= pGrid.grid[x, y].GetComponent<PheromoneNode>().pheromoneConcentration
                                        + pGrid.grid[x, y].GetComponent<PheromoneNode>().gatheringConcentration;

                                    smellDirection += pherDir;
                                }
                            }
                        }
                        //if GATHERING smell pheromones, collect food etc
                        else if (state == AntState.GATHERING && !smellingFood)
                        {       
                            if (pGrid.grid[x,y].GetComponent<PheromoneNode>().gatheringConcentration >= 1)
                            {
                                pherDir.Normalize();
                                pherDir *= pGrid.grid[x, y].GetComponent<PheromoneNode>().gatheringConcentration;
                                smellDirection += pherDir;

                                //value to determine 
                            }
                            SmellForFood(x, y);                         
                        }                       
                        else if(state == AntState.NESTING)
                        {
                            pherDir.Normalize();
                            pherDir *= pGrid.grid[x, y].GetComponent<PheromoneNode>().pheromoneConcentration + 
                                pGrid.grid[x, y].GetComponent<PheromoneNode>().gatheringConcentration;

                            smellDirection += pherDir;
                        }
                        else if(state == AntState.DEBUG)
                        {

                        }

                        smellDirection += negDir;

                    }
                }
            }
        }

        smellDirection.Normalize();        

        return smellStrength * smellDirection;
    }



    //Creates a random Vector for ant exploration
    //Used for ant movements
    public Vector3 RandomDirection()
    {
        Vector3 target = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f);

        //make the Target fit on circle again
        target.Normalize();
        target *= randomWanderMagnitude;

        return target;
    }

    //vector pointing towards the nest
    public Vector3 NestDirection()
    {
        Vector3 targetpos = Vector3.zero;

        if (targetpos != pGrid.worldNestPosition)
        {
            targetpos = pGrid.worldNestPosition;
        }

        Vector3 nestDirection = new Vector3(targetpos.x - transform.position.x, targetpos.y - transform.position.y, 0);
        nestDirection.Normalize();

        return nestDirection;
        //Debug.DrawLine(transform.position, transform.position + nestDirection, Color.black);
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
                //smellDirection.Normalize();

                foodItem = pGrid.grid[x, y].GetComponent<Food>();
                foodPosition = foodItem.worldFoodPosition;
                smellingFood = true;
            }
            else
            {
                //smellDirection = Vector3.zero;                
            }
        }
        else if (state == AntState.GATHERING)
        {
            if (pGrid.grid[x, y].GetComponent<Food>() != null)
            {
                smellDirection = pherDir;
                //smellDirection.Normalize();
                foodItem = pGrid.grid[x, y].GetComponent<Food>();
                foodPosition = foodItem.worldFoodPosition;
                smellingFood = true;
            }
        }
    }

    public void CollectingFood()
    {
        if(state == AntState.COLLECTINGFOOD)
        {
            carryAmount = Random.Range(1, 3);
            carryingFood = true;
            foodItem.ReduceFood(carryAmount);
            state = AntState.CARRYING;
            hunger = maxHunger;          
        }
    }

    public void Hunger()
    {
        if(hunger <= 10)
        {
            state = AntState.NESTING;
        }
        if(hunger < starvationValue)
        {
            state = AntState.DEAD;
        }
    }

    public void StateActions()
    {
        Vector3 targetdest = Vector3.zero;

        switch (state)
        {
            case AntState.SCOUTING:

                atNest = false;

                if (smellingFood == true)
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
                    nest.StoreFood(carryAmount);
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
                    
                    atNest = true;
                    if (hunger > 70)
                    {
                        atNest = false;
                        state = AntState.SCOUTING;
                    }
                    else if(nest.foodStored >= foodTakenFromNest)
                    {
                        nest.TakeFood(foodTakenFromNest);

                        hunger = maxHunger;
                    }
                   
                    else //if(nest.foodStored < foodTakenFromNest)
                    {
                        state = AntState.SCOUTING;
                    }
                }
                break;
        }
    }



    //secretes a pheremone where the ant is currently standing
    private void Secrete ()
    {
        // previous +0.5s mean this is now the same as transform.position
        // SCHEDULED FOR REMOVAL
        Vector3 antPosition = new Vector3(transform.position.x, transform.position.y, 0);
        Vector3 tileGridPos = World.instance.worldToTileGrid(antPosition);
        if (World.instance.GetTileAt((int)tileGridPos.x, (int)tileGridPos.y).type != Tile.Type.Grass)
        {
            return;
        }
        else
        {
            if (state == AntState.SCOUTING)
            {
                pGrid.AddPheromone(antPosition, PheromoneGrid.PheromoneType.STANDARD, 1.0f);
            }

            else if (state == AntState.CARRYING)
            {
                float nestDista = Vector3.Distance(transform.position, pGrid.worldNestPosition);
                if (nestDista > 5f)
                {
                    //food of size 100 drops nowmal pheromone amount
                    float pherMuliplier = foodItem.quantity / 100f;
                    if (pherMuliplier < 1)
                        pherMuliplier = 1;

                    pGrid.AddPheromone(antPosition, PheromoneGrid.PheromoneType.CARRYING, pherMuliplier);
                }
                //else add more if close to nest, want to draw ant sou tof the nest
                else
                {
                    pGrid.AddPheromone(antPosition, PheromoneGrid.PheromoneType.CARRYING, 2f);
                }
            }
        }        
	}

    protected bool CheckTarget ()
	{
        Vector3 tgPosition = World.instance.worldToTileGrid(targetPosition);
        Tile tile = World.instance.GetTileAt((int)tgPosition.x, (int)tgPosition.y);

		if (targetPosition.x <= -WorldManager.worldWidth/2f || targetPosition.x >= WorldManager.worldWidth/2f || targetPosition.y <= -WorldManager.worldHeight/2f || targetPosition.y >= WorldManager.worldHeight/2f)
        {
			return false;
		}
        else if (tile == null)
        {
            Debug.Log(tgPosition.x + ", " + tgPosition.y);
            return false;
        }
        else if (tile.type != Tile.Type.Grass && tile.type != Tile.Type.Dirt )
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

            //COULDVE BEEN CAUSE FOR CRASHES??????
            //if(turnsTried > 1000)
                //Debug.Log(turnsTried);

            //float angle = turningAngle * Mathf.Pow (angleMultiplier, turnsTried - 1);
            float angle = turningAngle * turnsTried;

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
			rotateSpeed * Time.deltaTime * worldManager.timeRate);	

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
            if (m_SpriteRenderer.color != Color.cyan)
                m_SpriteRenderer.color = Color.cyan;
        }
        else if (state == AntState.NESTING)
        {
            if (m_SpriteRenderer.color != Color.grey)
                m_SpriteRenderer.color = Color.grey;
        }
        else if (state == AntState.DEBUG)
        {
            if (m_SpriteRenderer.color != Color.black)
                m_SpriteRenderer.color = Color.black;
        }
    }

    public void SetAntSpeed(float _antSpeed)
    {
        antSpeed = _antSpeed;
    }

}
