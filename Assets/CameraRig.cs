using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRig : MonoBehaviour
{
    private AudioSource audioSource;
    //public AudioClip backgroundMusic;
    public AudioClip pacmanDeath;
    public AudioClip pelletEatSound;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        //audioSource.Play();
        //audioSource.loop = true;
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collision detected with HMD.");
        Debug.Log("Collision with object name: " + collision.gameObject.name);
        Debug.Log("Collision with object tag: " + collision.gameObject.tag);
        if (collision.gameObject.tag == ObjectTags.Pellet)
        {
            Destroy(collision.gameObject);
            audioSource.PlayOneShot(pelletEatSound, 0.3f);
        }
        else if (collision.gameObject.tag == ObjectTags.Ghost)
        {
            Debug.Log("We hit the fucking ghost! TAG: " + collision.gameObject.tag);
            audioSource.PlayOneShot(pacmanDeath);
        }
    }
}
