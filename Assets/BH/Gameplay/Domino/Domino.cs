using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Domino : MonoBehaviour {

    AudioSource collisionAudio;
    BoxCollider boxCol;
    private Rigidbody rb;
    Renderer rend;
    public AudioClip clip;

    // Use this for initialization
    void Awake () {
        collisionAudio = gameObject.GetComponent<AudioSource>();
        boxCol = gameObject.GetComponent<BoxCollider>(); //Might alter later
        rb = gameObject.GetComponent<Rigidbody>();

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void setAudioClip(AudioClip audio)
    {
        // NEED TO CHANGE LATER Sets audioclip based on whether collision is with another domino or the floor
        //clip = audio;
    }

    void OnCollisionEnter(Collision other)
    {
        Rigidbody otherRB = other.gameObject.GetComponent<Rigidbody>();
        if (otherRB == null)
            otherRB = rb;
        collisionAudio.clip = clip;
        collisionAudio.Play();
    }
}
