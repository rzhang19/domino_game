using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class launchDomino : MonoBehavior {
	/*
	 ********************* Assumptions **********************
	 * 1. This entire script currently assumes that the start and end heights are the same. Future versions of the script
	 *		may involve height difference calculations (would also need a way to see if we have reached ground).
	 *
	 ****************** Member variables ********************
	 * IMPORTANT NOTE: all angles are in RADIANS
	 *
	 * heightAngle: angle of the height with respect to ground. 0 is level, PI / 2 is completely upwards - MUST BE BETWEEN 0 and PI / 2
	 * velocity: speed vector of the "power" that the domino will be launched with - MUST BE POSITIVE
	 * direction: angle with respect to 2D direction - "rotation" of object, similar to how dominoes are rotated. "0" is "north" - MUST BE BETWEEN 0 and 2PI
	 *
	 * launched: whether domino is launched (explained in "Stages" section)
	 *
	 ********************** Stages **************************
	 * 1. waiting stage - pad has not been activated, is waiting for response
	 *		launched and reachedGround are both false
	 * 2. launched stage - pad has been activated, domino is flying through the air, has not yet hit ground yet
	 * 3. finished stage - domino has hit the ground after being launched, is possibly bouncing around
	 */

	// Enable user to customize these values "in game" as well
	public float heightAngle = Mathf.PI / 4;
	public float velocity = 100;
	public float direction = 0;
	
	private const float GRAVITY = 9.8;
	
	private bool launched = false;
	private bool reachedGround = false;
	
	private float currVerticalVelocity;
	private float currHorizontalVelocity;
	
	private float initialVerticalVelocity;
	private float initialHorizontalVelocity;
	
	private float height = 0;
	private float time = 0;

	// Use this for initialization
	void Start() {
		initialHorizontalVelocity = Mathf.cos(velocity);
		initialVerticalVelocity = Mathf.sin(velocity);
		
		CalculateTotalTime();
	}
	
	// Update is called once per frame
	// TODO: implement
	void Update() {
		if (launched) {
			if (reachedGround) {
				
			}
			
			else {
				
			}
		}
	}
	
	/*
	 * Using physics kinematics, estimate the total time the domino spends in the air, given the angle and 
	 */
	private void CalculateTotalTime() {
		height = initialVerticalVelocity * initialVerticalVelocity / (2 * GRAVITY);
		time = 2 * initialVerticalVelocity / GRAVITY;
	}
	
	// Helper functions to enable user to set height angle, velocity, and direction while the user is "in game"
	void UpdateAngle(float newHeightAngle) {
		heightAngle = newHeightAngle;
	}
	
	void UpdateVelocity(float newVelocity) {
		velocity = newVelocity;
	}
}