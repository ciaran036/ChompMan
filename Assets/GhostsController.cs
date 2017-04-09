using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostsController : MonoBehaviour
{
    public static bool BlueGhostActive;
    public static bool OrangeGhostActive;

    public GhostsController()
    {
        
    }

    // Use this for initialization
    void Start()
    {
        BlueGhostActive = false;
        var blueGhost = GameObject.Find(ObjectNames.BlueGhost);
        blueGhost.SetActive(false);

        OrangeGhostActive = false;
        var orangeGhost = GameObject.Find(ObjectNames.OrangeGhost);
        orangeGhost.SetActive(false);
    }

    public static void CheckGhostStartThresholds()
    {
        var ghostController = GameObject.Find(ObjectNames.GhostController);

        if (!BlueGhostActive)
        {
            var blueGhost = ghostController.transform.FindChild(ObjectNames.BlueGhost).gameObject;
            if ((Pellets.TotalNumberOfPellets - Pellets.NumberOfPelletsRemaining()) >= 30)
            {
                Debug.Log("BLUE GHOST: At least 30 pellets have been eaten, started moving");
                blueGhost.SetActive(true);
                BlueGhostActive = true;
            }
        }


        if (!OrangeGhostActive)
        {
            int numberOfPelletsEaten = Pellets.TotalNumberOfPellets - Pellets.NumberOfPelletsRemaining();
            int percentagePelletsEaten = (int) Math.Round((double) (100 * numberOfPelletsEaten) / Pellets.TotalNumberOfPellets);
            if (percentagePelletsEaten >= OrangeGhost.PercentageOfPelletsEaten)
            {
                var orangeGhost = ghostController.transform.FindChild(ObjectNames.OrangeGhost).gameObject;
                Debug.Log("ORANGE GHOST: Pellets eaten has reached threshold, started moving!");
                orangeGhost.SetActive(true);
                OrangeGhostActive = true;
            }
        }
    }
}
