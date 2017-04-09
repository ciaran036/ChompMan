using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Assets;
using UnityEditor;
using UnityEngine;

public abstract class Ghost : MonoBehaviour
{
    #region Variables
    protected const float Height = 1.25f;

    protected int startPosX;
    protected int startPosZ;
    protected bool _moving;
    protected Movement _currentMovement;
    protected int _currentPositionX;
    protected int _currentPositionZ;
    
    public BehaviourState BehaviourState;
    protected int _targetPosX;
    protected int _targetPoxZ;

    public int Speed;

    protected Timer timer;
    
    protected Collider collider;
    protected Rigidbody rigidBody;

    // Audio
    protected AudioSource audioSource;
    public AudioClip ghostSound;

    protected Renderer renderer;
    //protected Material frightenedBlueMaterial;
    //protected Material frightenedWhiteMaterial;
    protected Material material;

    #endregion

    protected void GhostStart()
    {
        renderer = GetComponent<Renderer>();
        material = (Material) Resources.Load(Materials.White, typeof(Material));
        //frightenedBlueMaterial = (Material)Resources.Load(Materials.FrightenedGhost, typeof(Material));
        //frightenedWhiteMaterial = (Material) Resources.Load(Materials.White, typeof(Material));

        collider = GetComponent<Collider>();
        collider.enabled = true;

        tag = ObjectTags.Ghost;

        audioSource = GetComponent<AudioSource>();
        audioSource.Play();
        audioSource.loop = true;

        _moving = false;
    }

    protected void MovementLogic()
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
                    _currentPositionX = GameGrid.TeleportRight(_currentPositionX);
                    transform.position = new Vector3(GameGrid.GetDrawPositionX(_currentPositionX), Height, GameGrid.GetDrawPositionZ(_currentPositionZ));
                    GetNextMovement();
                }
            }
        }
    }

    /// <summary>
    /// Determine the next movement
    /// </summary>
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
        else if (GameGrid.Grid[_currentPositionX, +_currentPositionZ] == GridPiece.GhostHome)
        {
            _moving = false;
            _currentMovement = Movement.Up;
            ResetSpeed();
            ActivateScatterMode();
        }
        else
        {
            if (BehaviourState != BehaviourState.Caught)
            {
                _currentMovement = ContinueMovement(possibleMoves);
            }
            else
            {
                _currentMovement = GameGrid.NextToBase(_currentPositionX, _currentPositionZ) 
                    ? Movement.Down 
                    : ContinueMovement(possibleMoves);
            }
        }
    }

    /// <summary>
    /// Each ghost has their own unique behaviour when chasing the player
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    protected abstract Vector3 CalculateTargetLocation(GameObject player);

    protected abstract void ResetSpeed();

    /// <summary>
    /// Get a new direction
    /// </summary>
    /// <param name="possibleMoves">All possible moves a ghost can make</param>
    /// <returns>The new direction</returns>
    private Movement GetNewDirection(IList<Movement> possibleMoves)
    {
        // In Scatter mode, ghosts will attempt to move towards a location outside of the grid
        if (BehaviourState == BehaviourState.Scatter)
        {
            var targetPosition = ScatterTarget();
            return calculateBestDirection(targetPosition, possibleMoves);
        }
        // In Chase mode, ghosts will make an attempt to chase the player, according to their personality
        else if (BehaviourState == BehaviourState.Chase)
        {
            var player = GameObject.FindGameObjectWithTag("MainCamera");
            if (player != null)
            {
                var targetPosition = CalculateTargetLocation(player);
                return calculateBestDirection(targetPosition, possibleMoves);
            }
        }
        // In Frightened mode, ghosts will turn in a random direction
        else if (BehaviourState == BehaviourState.Frightened)
        {
            return RandomDirection(possibleMoves);
        }
        // In Caught mode ghosts will attempt to return to the base
        else if (BehaviourState == BehaviourState.Caught)
        {
            var targetPosition = new Vector3(GameGrid.GetDrawPositionX(12), Height, GameGrid.GetDrawPositionZ(12));
            return calculateBestDirection(targetPosition, possibleMoves);
        }

        return Movement.Down;
    }

    /// <summary>
    /// Based on direction that the player is moving, choose the position X steps ahead
    /// TODO: Needs modified - below code will only consider upwards/downwards direction
    /// </summary>
    /// <param name="player">Main CameraRig</param>
    /// <param name="targetDeviation">Number of tiles ahead of the player's forward direction</param>
    /// <returns></returns>
    public static Vector3 TargetLocation(GameObject player, float targetDeviation)
    {
        var targetPosition = new Vector3(0, 0, 0);
        var forwardVector = player.transform.forward;
        if (forwardVector.z < 0)
        {
            Debug.Log("Player is facing upwards");
            targetPosition = new Vector3(
                player.transform.position.x,
                player.transform.position.y,
                player.transform.position.z + targetDeviation);
        }
        else if (forwardVector.z > 0)
        {
            Debug.Log("Player is facing downwards");
            targetPosition = new Vector3(
                player.transform.position.x,
                player.transform.position.y,
                player.transform.position.z - targetDeviation);
        }
        else if (forwardVector.x > 0)
        {
            Debug.Log("Player is facing to the left");
            targetPosition = new Vector3(
                player.transform.position.x + targetDeviation,
                player.transform.position.y,
                player.transform.position.z);
        }
        else if (forwardVector.x < 0)
        {
            Debug.Log("Player is facing to the right");
            targetPosition = new Vector3(
                player.transform.position.x - targetDeviation,
                player.transform.position.y,
                player.transform.position.z);
        }
        return targetPosition;
    }

    private static Movement RandomDirection(IList<Movement> possibleMoves)
    {
        var random = new System.Random();
        var randomNumber = random.Next(0, possibleMoves.Count);
        return possibleMoves[randomNumber];
    }

    /// <summary>
    /// Calculate the target position for Scatter behaviour mode
    /// </summary>
    /// <returns></returns>
    private Vector3 ScatterTarget()
    {
        return getTargetPosition(_targetPosX, _targetPoxZ);
    }

    /// <summary>
    /// Calculate the target position when in Caught behaviour mode
    /// </summary>
    /// <returns></returns>
    private Vector3 CaughtTarget()
    {
        return getTargetPosition(startPosX, startPosZ);
    }

    private Vector3 getTargetPosition(int x, int z)
    {
        var targetDrawPositionX = GameGrid.GetDrawPositionX(x);
        var targetDrawPositionZ = GameGrid.GetDrawPositionZ(z);
        return new Vector3(targetDrawPositionX, Height, targetDrawPositionZ);
    }

    private Movement calculateBestDirection(Vector3 targetPosition, IEnumerable<Movement> possibleMoves)
    {
        var potentialMovements = new List<PotentialMovement>();

        foreach (var possibleMove in possibleMoves)
        {
            var potentialMovement = new PotentialMovement { Movement = possibleMove };
            var position = new Vector3();
            switch (possibleMove)
            {
                case Movement.Left:
                    position = new Vector3(Left(transform.position.x), Height, transform.position.z);
                    break;
                case Movement.Right:
                    position = new Vector3(Right(transform.position.x), Height, transform.position.z);
                    break;
                case Movement.Up:
                    position = new Vector3(transform.position.x, Height, Up(transform.position.z));
                    break;
                case Movement.Down:
                    position = new Vector3(transform.position.x, Height, Down(transform.position.z));
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
    private Movement ContinueMovement(IEnumerable<Movement> possibleMoves)
    {
        _moving = false;
        return possibleMoves.First(m => m != GameGrid.GetOppositeMovement(_currentMovement));
    }

    protected void ChangeBehaviour(object source, ElapsedEventArgs e)
    {
        switch (BehaviourState)
        {
            case BehaviourState.Scatter:
                activateChaseMode();
                break;
            case BehaviourState.Chase:
                ActivateScatterMode();
                break;
            case BehaviourState.Frightened:
                // TODO: Behaviour should go back to previous mode when frightened mode is over
                Debug.Log("Ghosts are finished with being frightened");
                ActivateScatterMode();
                break;
        }
    }

    public void ActivateFrightenedMode()
    {
        BehaviourState = BehaviourState.Frightened;
        setTimer(25000);
        Debug.Log("Enabled frightened mode");
    }

    public void ActivateCaughtMode()
    {
        Debug.Log("CAUGHT MODE ACTIVATED");
        BehaviourState = BehaviourState.Caught;
        Speed *= 3;
        timer.Stop();
    }

    private void activateChaseMode()
    {
        Debug.Log("CHASE MODE ACTIVATED");
        BehaviourState = BehaviourState.Chase;
        setTimer(36000);
    }

    protected void ActivateScatterMode()
    {
        if(timer == null) timer = new Timer { Enabled = true };

        Debug.Log("SCATTER MODE ACTIVATED");
        BehaviourState = BehaviourState.Scatter;
        setTimer(12000);
    }

    private void setTimer(int interval)
    {
        timer.Stop();
        timer.Interval = interval;
        timer.Elapsed += ChangeBehaviour;
        timer.Start();
    }

    public static void ApplyFrightenedModeToAllGhosts()
    {
        var objects = GameObject.FindGameObjectsWithTag(ObjectTags.Ghost);
        foreach (var o in objects)
        {
            var ghost = (Ghost) o.GetComponent(typeof(Ghost));
            ghost.ActivateFrightenedMode();
        }
    }

    protected static void BlueAndWhiteFlashing(string ghost)
    {
        var mesh0 = GameObject.Find(ghost + "Mesh0");
        var mesh1 = GameObject.Find(ghost + "Mesh1");
        var renderer0 = mesh0.GetComponent<Renderer>();
        var renderer1 = mesh1.GetComponent<Renderer>();
        renderer0.material.color = renderer0.material.color = Color.Lerp(Color.blue, Color.white, Mathf.Abs(Mathf.Sin(Time.time * 2)));
        renderer1.material.color = renderer1.material.color = Color.Lerp(Color.blue, Color.white, Mathf.Abs(Mathf.Sin(Time.time * 2)));
        //if (BehaviourState == BehaviourState.Frightened)
        //{
        //    var meshes = GameObject.
        //    renderer.material.color = Color.Lerp(Color.blue, Color.white, Mathf.Abs(Mathf.Sin(Time.time * 2)));
        //}
    }

    //protected IEnumerator BlueAndWhiteFlashing()
    //{
    //    Debug.Log("Frightened skin");
    //    //renderer.materials = renderer.material.name == Materials.FrightenedGhost 
    //    //    ? new[] { frightenedWhiteMaterial } 
    //    //    : new[] { frightenedBlueMaterial };
    //    //renderer.material.color = Color.Lerp(Color.blue, Color.white, Mathf.Abs(Mathf.Sin(Time.time * 2)));
    //    //yield return new WaitForSeconds(0.25f);
    //}

    protected void LoadNormalSkin()
    {
        renderer.materials = new[] { material };
    }

    protected abstract string GetMaterialName();

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
    Frightened = 2,
    Caught = 3
}