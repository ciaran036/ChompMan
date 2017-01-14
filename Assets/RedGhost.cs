using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Assets;
using UnityEngine;

public class RedGhost : MonoBehaviour
{
    int startPosX = 13;
    int startPosZ = 12;
    private const float Height = 1.25f;

    private bool _moving;
    private Movement _currentMovement;
    private int _currentPositionX;
    private int _currentPositionZ;

    private BehaviourState _behaviourState;
    private int _targetPosX;
    private int _targetPoxZ;

    private const int Speed = 2;

    private Timer timer;

    private Collider collider;
    private Rigidbody rigidBody;

    private AudioSource audioSource;
    public AudioClip ghostSound;

    // Use this for initialization
	void Start ()
	{
	    timer = new Timer(12000) {Enabled = true};
	    timer.Elapsed += new ElapsedEventHandler(ChangeBehaviour);
        _behaviourState = BehaviourState.Scatter;
	    _currentPositionX = startPosX;
	    _currentPositionZ = startPosZ;
        transform.position = GameGrid.Position(startPosX, Height, startPosZ);

        _currentMovement = Movement.Up;
	    _moving = false;

        if (_behaviourState == BehaviourState.Scatter)
        {
            // Outside of game board
            _targetPosX = -5;
            _targetPoxZ = -5;
        }

        // TODO: Below needed?
	    collider = GetComponent<Collider>();
	    collider.enabled = true;

	    tag = "ghost";

	    audioSource = GetComponent<AudioSource>();
	    audioSource.Play();
	    audioSource.loop = true;
	}

    // Update is called once per frame
	void Update () {
		
	}

    void FixedUpdate()
    {
        if (GameGrid.Grid != null)
        {
            if (_currentMovement == Movement.Up)
            {
                int finalMovementPosition = GameGrid.Up(_currentPositionZ);
                if (!_moving || transform.position.z > GameGrid.GetDrawPositionZ(finalMovementPosition))
                {
                    _moving = true;
                    MoveUp();
                }
                else
                {
                    Debug.Log("Up movement completed");
                    _currentPositionZ = finalMovementPosition;
                    GetNextMovement();
                }
            }
            else if (_currentMovement == Movement.Down)
            {
                int finalMovementPosition = GameGrid.Down(_currentPositionZ);
                if (!_moving || transform.position.z < GameGrid.GetDrawPositionZ(finalMovementPosition))
                {
                    _moving = true;
                    MoveDown();
                }
                else
                {
                    Debug.Log("Down movement completed");
                    _currentPositionZ = finalMovementPosition;
                    GetNextMovement();
                }
            }
            else if (_currentMovement == Movement.Left)
            {
                int finalMovementPosition = GameGrid.Left(_currentPositionX);
                if (!_moving || transform.position.x < GameGrid.GetDrawPositionX(finalMovementPosition))
                {
                    _moving = true;
                    MoveLeft();
                }
                else
                {
                    Debug.Log("Left movement completed");
                    _currentPositionX = finalMovementPosition;
                    GetNextMovement();
                }
            }
            else if (_currentMovement == Movement.Right)
            {
                int finalMovementPosition = GameGrid.Right(_currentPositionX);
                if (!_moving || transform.position.x > GameGrid.GetDrawPositionX(finalMovementPosition))
                {
                    _moving = true;
                    MoveRight();
                }
                else
                {
                    Debug.Log("Right movement completed");
                    _currentPositionX = finalMovementPosition;
                    GetNextMovement();
                }
            }
            else if (_currentMovement == Movement.TeleportLeft)
            {
                int finalMovementPosition = GameGrid.Right(_currentPositionX);
                if (!_moving || transform.position.x > GameGrid.GetDrawPositionX(finalMovementPosition))
                {
                    _moving = true;
                    MoveRight();
                }
                else
                {
                    Debug.Log("Movement into RIGHT teleport completed");
                    _currentPositionX = GameGrid.TeleportLeft(_currentPositionX);
                    transform.position = new Vector3(GameGrid.GetDrawPositionX(_currentPositionX), Height, GameGrid.GetDrawPositionZ(_currentPositionZ));
                    GetNextMovement();
                }
            }
            else if (_currentMovement == Movement.TeleportRight)
            {
                int finalMovementPosition = GameGrid.Left(_currentPositionX);
                if (!_moving || transform.position.x < GameGrid.GetDrawPositionX(finalMovementPosition))
                {
                    _moving = true;
                    MoveLeft();
                }
                else
                {
                    Debug.Log("Movement into LEFT teleport completed");
                    _currentPositionX = GameGrid.TeleportRight(_currentPositionX);
                    transform.position = new Vector3(GameGrid.GetDrawPositionX(_currentPositionX), Height, GameGrid.GetDrawPositionZ(_currentPositionZ));
                    GetNextMovement();
                }
            }
        }
    }

    private void ChangeBehaviour(object source, ElapsedEventArgs e)
    {
        switch (_behaviourState)
        {
            case BehaviourState.Scatter:
                Debug.Log("CHASE MODE ACTIVATED");
                _behaviourState = BehaviourState.Chase;
                timer.Stop();
                timer.Interval = 36000;
                timer.Start();
                break;
            case BehaviourState.Chase:
                Debug.Log("SCATTER MODE ACTIVATED");
                _behaviourState = BehaviourState.Scatter;
                timer.Stop();
                timer.Interval = 12000;
                timer.Start();
                break;
        }
    }

    private void GetNextMovement()
    {
        var possibleMoves = GameGrid.GetPossibleMoves(_currentPositionX, _currentPositionZ);

        if (GameGrid.Grid[_currentPositionX, _currentPositionZ] == GridPiece.Intersection)
        {
            _currentMovement = GetNewDirection(possibleMoves);
        }
        else if (GameGrid.Grid[_currentPositionX, _currentPositionZ] == GridPiece.LeftPortal)
        {
            _currentMovement = Movement.TeleportRight;
            _moving = false;
        }
        else if (GameGrid.Grid[_currentPositionX, _currentPositionZ] == GridPiece.RightPortal)
        {
            _currentMovement = Movement.TeleportLeft;
            _moving = false;
        }
        else
        {
            _currentMovement = ContinueMovement(possibleMoves);
        }
    }

    private void MoveRight()
    {
        transform.Translate(new Vector3(-Speed, 0, 0) * Time.deltaTime);
    }

    private void MoveLeft()
    {
        transform.Translate(new Vector3(Speed, 0, 0) * Time.deltaTime);
    }

    private void MoveDown()
    {
        transform.Translate(new Vector3(0, -Speed, 0) * Time.deltaTime);
    }

    private void MoveUp()
    {
        transform.Translate(new Vector3(0, Speed, 0) * Time.deltaTime);
    }

    private Movement GetNewDirection(List<Movement> possibleMoves)
    {
        if (_behaviourState == BehaviourState.Scatter)
        {
            var targetPosition = scatterTarget();
            return calculateBestDirection(targetPosition, possibleMoves);
        }
        else if (_behaviourState == BehaviourState.Chase)
        {
            var player = GameObject.FindGameObjectWithTag("MainCamera");
            if (player != null)
            {
                return calculateBestDirection(player.transform.position, possibleMoves);
            }
        }

        // TODO: Replace placeholder
        return Movement.Down;
    }

    private Vector3 scatterTarget()
    {
        var targetDrawPositionX = GameGrid.GetDrawPositionX(_targetPosX);
        var targetDrawPositionZ = GameGrid.GetDrawPositionZ(_targetPoxZ);
        var targetPosition = new Vector3(targetDrawPositionX, Height, targetDrawPositionZ);
        return targetPosition;
    }

    private Movement calculateBestDirection(Vector3 targetPosition, List<Movement> possibleMoves)
    {
        var potentialMovements = new List<PotentialMovement>();

        foreach (var possibleMove in possibleMoves)
        {
            var potentialMovement = new PotentialMovement {Movement = possibleMove};
            var position = new Vector3();
            switch (possibleMove)
            {
                case Movement.Left:
                    position = new Vector3(Left(transform.position.x), transform.position.y);
                    break;
                case Movement.Right:
                    position = new Vector3(Right(transform.position.x), transform.position.y);
                    break;
                case Movement.Up:
                    position = new Vector3(transform.position.x, Up(transform.position.y));
                    break;
                case Movement.Down:
                    position = new Vector3(transform.position.x, Down(transform.position.y));
                    break;
            }

            potentialMovement.Distance = Vector3.Distance(targetPosition, position);
            potentialMovements.Add(potentialMovement);
        }

        _moving = false;

        return potentialMovements
            .OrderBy(p => p.Distance)
            .First(m => m.Movement != GameGrid.GetOppositeMovement(_currentMovement)).Movement;
    }

    /// <summary>
    /// Continue moving ghost in same direction except if grid forces change in direction. 
    /// Ghost cannot move back in the direction they came from. 
    /// </summary>
    /// <param name="possibleMoves"></param>
    /// <returns></returns>
    private Movement ContinueMovement(List<Movement> possibleMoves)
    {
        _moving = false;
        return possibleMoves.First(m => m != GameGrid.GetOppositeMovement(_currentMovement));
    }

    public static float Up(float z)
    {
        return z - 1;
    }

    public static float Down(float z)
    {
        return z + 1;
    }

    public static float Left(float x)
    {
        return x + 1;
    }

    public static float Right(float x)
    {
        return x - 1;
    }
}

public class PotentialMovement
{
    public Movement Movement;
    public float Distance;
}

public enum BehaviourState
{
    Chase = 0,
    Scatter = 1,
    Frightened = 2
}


