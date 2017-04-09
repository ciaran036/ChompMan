using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Assets;

public class Pellets : MonoBehaviour
{
    public static Material pelletMaterial;
    public static int TotalNumberOfPellets;

	public static void Load()
    {
        pelletMaterial = (Material)Resources.Load(Materials.Pickup, typeof(Material));

        float x = GameGrid.XDrawStartPos;
        float z = GameGrid.ZDrawStartPos;

        foreach (var line in GameGrid.Text)
        {
            foreach(char cell in line)
            {
                if(cell == '-' || cell == 'I')
                {
                    TotalNumberOfPellets++;
                    drawPellet(x, z);
                }
                else if (cell == 'P')
                {
                    TotalNumberOfPellets++;
                    drawPowerPellet(x, z);
                }
                x--;
            }
            x = GameGrid.XDrawStartPos;
            z++;
        }
	}

    private static void drawPellet(float x, float z)
    {
        var pellet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        
        pellet.transform.position = new Vector3(x, 1, z);
        pellet.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        
        var renderer = pellet.GetComponent<Renderer>();
        renderer.material = pelletMaterial;

        pellet.tag = ObjectTags.Pellet;
        pellet.name = ObjectNames.Pellet;
    }

    private static void drawPowerPellet(float x, float z)
    {
        var powerPellet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        powerPellet.transform.position = new Vector3(x, 1, z);
        powerPellet.transform.localScale = new Vector3(1f, 1f, 1f);

        var renderer = powerPellet.GetComponent<Renderer>();
        renderer.material = pelletMaterial;

        powerPellet.tag = ObjectTags.Pellet;
        powerPellet.name = ObjectNames.PowerPellet;
    }

    public static int NumberOfPelletsRemaining()
    {
        return GameObject.FindGameObjectsWithTag(ObjectTags.Pellet).Length-1;
    }
}
