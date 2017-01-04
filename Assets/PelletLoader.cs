using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class PelletLoader : MonoBehaviour
{
    public Material pelletMaterial;

    const float XStartPos = 12.5f;
    const float ZStartPos = -12.5f;

	// Use this for initialization
	void Start ()
    {
        pelletMaterial = (Material)Resources.Load("Materials\\Pickup", typeof(Material));

        Debug.Log(Directory.GetCurrentDirectory());
        var gameGridText = File.ReadAllLines("Assets\\GameGrid.txt");

        float x = XStartPos;
        float z = ZStartPos;

        foreach (var line in gameGridText)
        {
            foreach(char cell in line)
            {
                if(cell == '-')
                {
                    drawPellet(x, z);
                }
                x--;
            }
            x = XStartPos;

            z++;
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void drawPellet(float x, float z)
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
