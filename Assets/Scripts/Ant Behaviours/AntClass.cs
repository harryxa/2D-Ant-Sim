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
		GATHERING
    };

	public AntState state;
    //basic properties of an ant
    bool carrying = false;

	protected bool firstStepsTaken = false;
	protected bool stepped;

	//wander variables for random target
	protected float jitterScale = 0.5f;
	protected float wanderDistance = 1f;
	protected Vector3 targetPosition;
	protected float distanceRadius = 0.1f;
	//box collider
	protected float rotateSpeed = 20f;


	protected PheromoneGrid pGrid;

	//bounding box floats
	float worldHeight;
	float worldWidth;
	float halfWorldHeight;
	float halfWorldWidth;

	//navigation
	//Vector3 nextPosition;
	public float antSpeed = 20f;

	protected int limit = 5;
	protected int limiti;

	//SMELLING VARIABLES
	private int smellRadius = 8;
	private float smellStrength = 1f;

	protected bool leftNext;
	protected int leftTurnsTried = 0;
	protected int rightTurnsTried = 0;

	protected float turningAngle = 0.5f;
	protected float angleMultiplier = 1.1f;

    private Vector3 smellDirection;
    private Vector3 pherDir;
    private Vector3 foodPosition;
    private bool smellingFood = false;

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

		if (Random.value > 0f)
			state = AntState.SCOUTING;
		else
			state = AntState.GATHERING;

		stepped = false;
	}

	void Update ()
	{
        //secrete pheromone when target destination is reached
        if (canSecrete == true)
        {
			secrete ();
			canSecrete = false;
		}
		if (firstStepsTaken)
			Move ();
		else
        {			
			Move ();
		}

        changeAntColour();
        StateChange();        
    }



    //Sets the target for the ant then move the ant there
    protected void Move()
    {
        if (TargetReached(targetPosition, distanceRadius))
        {
            //secrete set to true, ant deposits another pheromone at target pos
            canSecrete = true;

            switch (state)
            {
                //if wandering smell for food and randomly wander around the map
                case AntState.SCOUTING:
                    SetTarget(SmellDirection() + RandomTarget());
                    break;

                //if carrying food follow pheromones
                //TODO: follow food gathering pheromone
                case AntState.CARRYING:
                    //SetTarget(SmellDirection() + RandomTarget() + NestDirection());

                    SetTarget(SmellDirection() * 1.5f + RandomTarget() + NestDirection() * 0.5f);
                    Debug.DrawLine(transform.position, transform.position - SmellDirection() * 1.5f);

                    break;

                //if gathering smell pheromone and randomly wander
                case AntState.GATHERING:
                  //  SetTarget(SmellDirection() + FoodDirection());
                    SetTarget(SmellDirection() * 1f - NestDirection() * 1f);
                    //SetTarget(SmellDirection() * 1f);
                    //Debug.DrawLine(transform.position, transform.position - SmellDirection());

                    break;
            }
            FixTarget(); //....
        }

        //turns the ant to look where theyre going
        steer();
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
        smellDirection = new Vector3(antGridPos.x, antGridPos.y, 0f);


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
                            //returns a smell direction if food smelt
                            if (pGrid.grid[x, y].GetComponent<PheromoneNode>().carryConcentration > 10)
                            {
                                state = AntState.GATHERING;
                            }
                            SmellForFood(x, y);                           
                            
                        }
                        //if CARRYING, head home
                        else if (state == AntState.CARRYING)
                        {
                            pherDir.Normalize();
                            pherDir *= pGrid.grid[x, y].GetComponent<PheromoneNode>().pheromoneConcentration + pGrid.grid[x, y].GetComponent<PheromoneNode>().carryConcentration;
                            smellDirection += pherDir;
                            //Debug.DrawLine(smellDirection, transform.position);

                        }
                        //if GATHERING smell pheromones
                        //TODO: smell food and collect
                        else if (state == AntState.GATHERING)
                        {       
                            if (pGrid.grid[x,y].GetComponent<PheromoneNode>().carryConcentration > 0.01)
                            {
                                pherDir.Normalize();
                                pherDir *= pGrid.grid[x, y].GetComponent<PheromoneNode>().carryConcentration;
                                smellDirection += pherDir;
                            }
                            SmellForFood(x, y);                           
                        }
                    }             
                }
            }
        }

        

        //smellDirection = pGrid.gridToWorld (smellDirection);
        smellDirection.Normalize();

        return smellStrength * smellDirection;
    }

    public void StateChange()
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
                        state = AntState.CARRYING;
                        smellingFood = false;
                    }
                }
               
                break;

            case AntState.CARRYING:
                targetdest = pGrid.worldNestPosition;
                if (TargetReached(targetdest, 2f))
                {
                    state = AntState.GATHERING;
                }

                break;

            case AntState.GATHERING:

                if (smellingFood == true)
                {
                    if (TargetReached(foodPosition, 2f))
                    {
                        state = AntState.CARRYING;
                        smellingFood = false;
                    }
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
    void secrete ()
    {

        if (World.instance.GetTileAt(Mathf.FloorToInt(targetPosition.x + halfWorldWidth), Mathf.FloorToInt(targetPosition.y + halfWorldHeight)).type != Tile.Type.Grass)
        {
            return;
        }
        else
        {
            if (state == AntState.SCOUTING)
            {
                pGrid.addPheromone(transform.position, 'S', 1.0f);
            }

            else if (state == AntState.CARRYING)
            {
                float nestDista = Vector3.Distance(transform.position, pGrid.worldNestPosition);
                if (nestDista > 5f)
                {
                    pGrid.addPheromone(transform.position, 'C', 2.0f);
                }
                else
                {
                    pGrid.addPheromone(transform.position, 'C', 8f);
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
                foodPosition = pGrid.grid[x, y].GetComponent<Food>().worldFoodPosition;
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
                foodPosition = pGrid.grid[x, y].GetComponent<Food>().worldFoodPosition;
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


	public Vector3 RandomTarget ()
	{
		Vector3 target = new Vector3 (Random.Range (-1f, 1f), Random.Range (-1f, 1f), 0f);

		//make the Target fit on circle again
		target.Normalize ();
		target *= jitterScale;

		return target;
	}

	//sets ants target vector
	protected bool TargetReached (Vector3 targetPos, float distanceRad)
	{
		return 
			transform.position.y < targetPos.y + distanceRad &&
		    transform.position.y > targetPos.y - distanceRad &&
		    transform.position.x < targetPos.x + distanceRad &&
		    transform.position.x > targetPos.x - distanceRad;
	}

	protected void SetTarget (Vector3 target)
	{
		targetPosition = transform.position + transform.up * wanderDistance + target;
        //Debug.DrawLine(transform.position, targetPosition);
    }

    protected void steer ()
	{
		Vector3 direction = targetPosition - transform.position;
		float angle = Mathf.Atan2 (direction.y, direction.x) * Mathf.Rad2Deg;
		transform.rotation = Quaternion.Lerp (transform.rotation, 
			Quaternion.Euler (0f, 0f, angle - 90f), 
			rotateSpeed * Time.deltaTime);	

		//Debug.DrawLine (transform.position, targetPosition);
	}

    private void changeAntColour()
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


}
