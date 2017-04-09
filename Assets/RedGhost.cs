using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Assets;
using UnityEngine;

public class RedGhost : Ghost
{
    const int StartPosX = 13;
    const int StartPosZ = 11;

    private const int TargetPosX = 20;
    private const int TargetPosZ = -20;

    private const int RedSpeed = 2;

    void Start ()
	{
	    startPosX = StartPosX;
	    startPosZ = StartPosZ;
	    Speed = RedSpeed;
	    ActivateScatterMode();
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
	void Update ()
    {
        if (BehaviourState == BehaviourState.Frightened)
        {
            BlueAndWhiteFlashing("Red");
        }
        else
        {
            LoadNormalSkin();
        }
    }

    void FixedUpdate()
    {
        MovementLogic();
    }

    /// <summary>
    /// Target the player's exact location
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    protected override Vector3 CalculateTargetLocation(GameObject player)
    {
        return player.transform.position;
    }

    protected override string GetMaterialName()
    {
        return "Red Ghost";
    }

    protected override void ResetSpeed()
    {
        Speed = RedSpeed;
    }
}




