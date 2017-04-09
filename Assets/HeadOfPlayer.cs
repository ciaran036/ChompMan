using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadOfPlayer : MonoBehaviour
{
    private AudioSource audioSource;
    public AudioClip pacmanDeath;
    public AudioClip pelletEatSound;
    public AudioClip portalSound;

    public Collider collider;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        collider = GetComponent<Collider>();
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
            if (collision.gameObject.name.Contains(ObjectNames.PowerPellet))
            {
                Debug.Log("We collided with a super pellet. Initiating Frightened mode.");
                Ghost.ApplyFrightenedModeToAllGhosts();
            }
            Destroy(collision.gameObject);
            audioSource.PlayOneShot(pelletEatSound, 0.3f);
            GhostsController.CheckGhostStartThresholds();
        }
        else if (collision.gameObject.tag == ObjectTags.Ghost)
        {
            var ghost = collision.gameObject.GetComponent<Ghost>();
            if (ghost != null && ghost.BehaviourState != BehaviourState.Frightened && ghost.BehaviourState != BehaviourState.Caught)
            {
                Debug.Log("We got caught by the ghost! TAG: " + collision.gameObject.tag);
                audioSource.PlayOneShot(pacmanDeath);
                //Physics.IgnoreCollision(collider, collision.collider);
            }
            else if (ghost.BehaviourState == BehaviourState.Frightened)
            {
                Debug.Log("We caught the ghost!");
                ghost.ActivateCaughtMode();
            }
            
            // TODO: Implement player death logic!
        }
        else if (collision.gameObject.name == ObjectNames.LeftPortalImage)
        {
            if (transform.position.x > 0)
            {
                Debug.Log("We used the left portal!");
                playPortalSound();
                teleportRight();
            }
        }
        else if (collision.gameObject.name == ObjectNames.RightPortalImage)
        {
            if (transform.position.x < 0)
            {
                Debug.Log("We used the right portal!");
                playPortalSound();
                teleportLeft();
            }
        }
    }

    private void playPortalSound()
    {
        audioSource.PlayOneShot(portalSound);
    }

    private void teleportRight()
    {
        transform.Translate(new Vector3(transform.position.x - 40f, transform.position.y, transform.position.z - 2f));
    }

    private void teleportLeft()
    {
        transform.Translate(new Vector3(transform.position.x + 39f, transform.position.y, transform.position.z - 2f));
    }
}
