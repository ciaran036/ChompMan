using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Assets;

public class Pellets : MonoBehaviour
{
    public static Material pelletMaterial;

	public static void Load()
    {
        pelletMaterial = (Material)Resources.Load("Materials\\Pickup", typeof(Material));

        float x = GameGrid.XDrawStartPos;
        float z = GameGrid.ZDrawStartPos;

        foreach (var line in GameGrid.Text)
        {
            foreach(char cell in line)
            {
                if(cell == '-' || cell == 'I' || cell == 'L' || cell == 'R')
                {
                    drawPellet(x, z);
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

        pellet.tag = "pellet";
        pellet.name = "pellet";
        Instantiate(pellet);
    }
}
