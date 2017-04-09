using System.Timers;
using Assets;
using UnityEngine;

public class BlueGhost : Ghost {

    const int StartPosX = 12;
    const int StartPosZ = 12;

    private const int TargetPosX = 20;
    private const int TargetPosZ = 20;

    private bool timerStarted;

    public const float ChaseModeTargetDeviation = 2;
    public const int BlueSpeed = 2;

    void Start()
    {
        startPosX = StartPosX;
        startPosZ = StartPosZ;
        Speed = BlueSpeed;
        _currentPositionX = startPosX;
        _currentPositionZ = startPosZ;
        transform.position = GameGrid.Position(startPosX, Height, startPosZ);

        _currentMovement = Movement.Right;

        if (BehaviourState == BehaviourState.Scatter)
        {
            // Outside of game board
            _targetPosX = TargetPosX;
            _targetPoxZ = TargetPosZ;
        }
        
        GhostStart();
    }

    // Update is called once per frame
    void Update()
    {
        if (BehaviourState == BehaviourState.Frightened)
        {
            BlueAndWhiteFlashing("Blue");
        }
    }

    void FixedUpdate()
    {
        startTimer();
        MovementLogic();
    }

    private void startTimer()
    {
        if (timerStarted == false)
        {
            ActivateScatterMode();
            timerStarted = true;
        }
    }

    /// <summary>
    /// Inky's target location is calculated by looking ahead to tile 2 steps ahead of player's current
    /// direction of travel, and then from that location to the location to Blinky (Red Ghost) we select
    /// the tile that is twice the distance from the red ghost to the targeted location
    /// TODO: Improve
    /// </summary>
    /// <returns></returns>
    protected override Vector3 CalculateTargetLocation(GameObject player)
    {
        var playerTarget = TargetLocation(player, ChaseModeTargetDeviation);
        var redGhost = GameObject.Find(ObjectNames.RedGhost);
        var distance = Vector3.Distance(playerTarget, redGhost.transform.position) * 2;
        return TargetLocation(player, distance);
    }

    protected override string GetMaterialName()
    {
        return "Blue Ghost";
    }

    protected override void ResetSpeed()
    {
        Speed = BlueSpeed;
    }
}
