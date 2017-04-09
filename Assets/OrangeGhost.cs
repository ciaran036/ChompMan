using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Timers;
using Assets;
using UnityEngine;

public class OrangeGhost : Ghost {

    const int StartPosX = 14;
    const int StartPosZ = 12;

    private const int TargetPosX = -20;
    private const int TargetPosZ = 20;

    private bool timerStarted;

    public const int PercentageOfPelletsEaten = 75;
    public const int OrangeSpeed = 2;

    void Start()
    {
        startPosX = StartPosX;
        startPosZ = StartPosZ;
        Speed = OrangeSpeed;
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
            BlueAndWhiteFlashing("Orange");
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
    /// If Clyde is further than eight tiles away, targeting is same as Blinky's (Red Ghost),
    /// otherwise target tile is same tile as the fixed one in Scatter mode. 
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    protected override Vector3 CalculateTargetLocation(GameObject player)
    {
        var distance = Vector3.Distance(player.transform.position, transform.position);
        return distance > 8 ? player.transform.position : GameGrid.Position(startPosX, Height, startPosZ);
    }

    protected override string GetMaterialName()
    {
        return "Orange Ghost";
    }

    protected override void ResetSpeed()
    {
        Speed = OrangeSpeed;
    }
}
