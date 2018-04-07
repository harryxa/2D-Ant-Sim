using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class AntClass : MonoBehaviour
{

    SpriteRenderer m_SpriteRenderer;

    //debug stuff
    public bool canSecrete = true;

	public enum AntState
	{
        WANDERING,
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
	protected float wanderDistance = 1.5f;
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
	private int smellRadius = 20;
	private float smellStrength = 0.4f;

	protected bool leftNext;
	protected int leftTurnsTried = 0;
	protected int rightTurnsTried = 0;

	protected float turningAngle = 5;
	protected float angleMultiplier = 1.1f;



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
			state = AntState.WANDERING;
		else
			state = AntState.GATHERING;

		stepped = false;
	}

	void Update ()
	{
        //boundingBox ();

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
            
    }

    //Sets the target for the ant then move the ant there
    protected void Move()
    {
        if (TargetReached())
        {
            //secrete set to true, ant deposits another pheromone at target pos
            canSecrete = true;

            switch (state)
            {
                //if wandering smell for food and randomly wander around the map
                case AntState.WANDERING:
                    SetTarget(SmellDirection() + RandomTarget());
                    break;

                //if carrying food follow pheromones
                //TODO: follow food gathering pheromone
                case AntState.CARRYING:
                    SetTarget(SmellDirection());
                    break;

                //if gathering smell pheromone and randomly wander
                case AntState.GATHERING:
                    SetTarget(SmellDirection() + RandomTarget());
                    break;
            }
            FixTarget(); //....
        }

        //turns the ant to look where theyre going
        steer();
        float step = antSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
    }

    //TODO fix ant state changes
    //detects pheremones within range of ant
    protected Vector3 SmellDirection()
    {
        //gets the ants location on the grid
        Vector3 antGridPos = pGrid.worldToGrid(transform.position);
        int gridX = Mathf.RoundToInt(antGridPos.x);
        int gridY = Mathf.RoundToInt(antGridPos.y);

        //smellDirection = ants position
        Vector3 smellDirection = new Vector3(antGridPos.x, antGridPos.y, 0f);

        //loops through grid in square shape around ant
        for (int x = gridX - smellRadius; x <= gridX + smellRadius; x++)
        {
            for (int y = gridY - smellRadius; y <= gridY + smellRadius; y++)
            {
                //creates a vector from the ant to the node on the grid
                Vector3 pherDir = new Vector3(x - gridX, y - gridY, 0);

                //limits the search to a radius around the ant, rather than a square. checks its in world
                if (x >= 0 && x < worldWidth && y >= 0 && y < worldHeight && pherDir.magnitude <= smellRadius)
                {
                    //if valid grid position
                    if (pGrid.grid[x, y] != null)
                    {
                        //if wandering smell for food and randomly wander around the map
                        //set state to carry if food found
                        if (state == AntState.WANDERING)
                        {
                            //if the item in the grid is food
                            //set smellDirection to the food position and normalise the vector
                            if (pGrid.grid[x, y].GetComponent<Food>() != null)
                            {
                                smellDirection = pherDir;
                                smellDirection.Normalize();

                                //set state to carry and smell strength increase (HACKY)
                                if (gridX == x || gridY == y)
                                {
                                    state = AntState.CARRYING;
                                }
                                return smellDirection;
                            }
                            //otherwise wander randomly
                            else
                            {
                                smellDirection = Vector3.zero;
                            }
                        }
                        //if carrying, head home
                        else if(state == AntState.CARRYING)
                        {
                            Vector3 targetpos = pGrid.worldToGrid(pGrid.nestPosition);

                            smellDirection = new Vector3(targetpos.x - gridX, targetpos.y - gridY, 0);

                            pherDir.Normalize();
                            pherDir *= pGrid.grid[x, y].GetComponent<PheromoneNode>().concentration;
                            smellDirection += pherDir;

                            if (gridX == targetpos.x || gridY == targetpos.y)
                            {
                                smellStrength = 0.4f;
                                state = AntState.GATHERING;
                            }

                        }
                        //if gathering smell pheromones
                        //TODO: smell food and collect
                        else if(state == AntState.GATHERING)
                        {
                            
                            pherDir.Normalize();
                            pherDir *= pGrid.grid[x, y].GetComponent<PheromoneNode>().concentration;
                            smellDirection += pherDir;

                        }
                    }             
                }
            }
        }

        //smellDirection = pGrid.gridToWorld (smellDirection);
        smellDirection.Normalize();
        Debug.DrawLine(transform.position, transform.position + smellDirection, Color.black);
        return smellStrength * smellDirection;
    }



	//TODO: various pheremones sevreted deonding on the situation
	//secretes a pheremone where the ant is currently standing
	void secrete ()
	{
        if (state == AntState.WANDERING)
        {
            pGrid.addPheromone(transform.position);
        }

        else if (state == AntState.CARRYING || state == AntState.GATHERING)
        {
            pGrid.addCarryingPheromone(transform.position);
        }
	}

    //old secrete function
    //void secrete()
    //{
    //    //limit = 5
    //    //limiti = random number between 0 and 5
    //    if (limit >= limiti)
    //    {

    //        pGrid.addPheromone(transform.position);


    //        limiti = 0;
    //    }
    //    else
    //        limiti++;
    //}


    //TODO fix movement, trying to sort smeel direction to only get called when ants are at taregt location

    //TODO: ants follow different pheromones depending on the situation
    //moves the ant in the direction its facing
    //	protected void MoveAnt ()
    //	{
    //		//SetTarget(SmellDirection());
    //		//SetTarget (RandomTarget());
    //
    //
    //		SetTarget (SmellDirection () + RandomTarget ());
    //		//SetTarget (RandomTarget ());
    //
    //		//boundingBox ();
    //		FixTarget ();
    //
    //		steer ();
    //		float step = antSpeed * Time.deltaTime;
    //		transform.position = Vector3.MoveTowards (transform.position, targetPosition, step);
    //	}

    //TODO get rid
    //	protected void MoveAntRand ()
    //	{			
    //		//SetTarget(SmellDirection());
    //		//SetTarget (RandomTarget());
    //
    //
    //		//SetTarget (SmellDirection () + RandomTarget ());
    //		TargetReached (RandomTarget ());// TODO oioio ioio
    //
    //		//boundingBox ();
    //		FixTarget ();
    //
    //		steer ();
    //		float step = antSpeed * Time.deltaTime;
    //		transform.position = Vector3.MoveTowards (transform.position, targetPosition, step);	
    //	}

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
	protected bool TargetReached ()
	{
		return 
			transform.position.y < targetPosition.y + distanceRadius &&
		transform.position.y > targetPosition.y - distanceRadius &&
		transform.position.x < targetPosition.x + distanceRadius &&
		transform.position.x > targetPosition.x - distanceRadius;
	}

	protected void SetTarget (Vector3 target)
	{
		targetPosition = transform.position + transform.up * wanderDistance + target;
	}

    //bounds ants within area
    //protected void boundingBox ()
    //{
    //	if (transform.position.x < -halfWorldWidth) {
    //		transform.position = new Vector3 (halfWorldWidth, transform.position.y, 0);
    //		targetPosition = transform.position;
    //	} else if (transform.position.x > halfWorldWidth) {
    //		transform.position = new Vector3 (-halfWorldWidth, transform.position.y, 0);
    //		targetPosition = transform.position;
    //	} else if (transform.position.y < -halfWorldHeight) {
    //		transform.position = new Vector3 (transform.position.x, halfWorldHeight, 0);
    //		targetPosition = transform.position;
    //	} else if (transform.position.y > halfWorldHeight) {
    //		transform.position = new Vector3 (transform.position.x, -halfWorldHeight, 0);
    //		targetPosition = transform.position;
    //	}
    //}

    //changes the direction that the ant is facing

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

        if(state == AntState.WANDERING)
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
