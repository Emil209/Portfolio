using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// Copyright (C) 2016 Emil Almkvist
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//


/// 
///
/// 	HOW TO USE 
/// 	
/// 	Start setup:
///		0: This preparation needs to be done in all scenes using this Line of sight algorithm
/// 	1: Create a new Layer and call it "LineOfSight". Make sure in the "Physics2d" settings that object in this layer can only collide with other LineOfSight objects 
/// 	1: Create an empty GameObject and give it the "CameraEdge" script
/// 	2: Create a new empty GameObject and give it an EdgeCollider2D and a "StaticEdgeScript"
/// 	3: Make sure that the booleans "Is Open" and "Wants To Be Open" is true. Make also sure that the "Mask" is set to "LineOfSight"
/// 	4: Make the object a child to the "CameraEdge" object
/// 	5: Change its layer to be our new "LineOfSight"
/// 	6: Repeat step 2-5 until you have four "StaticEdgeScript" gameObjects
///  	7: Create an empty gameObject and call it "AllLineOfSights". Leave it empty for now
/// 	8: Create a gameObject containing the "GameManager" script. Make sure its "testNr" is set to -3 or less
/// 	9: Make sure that the GameManager's "PlayerScript" is a reference to the player
/// 	10: Make sure that the GameManager's "Camera Edge" is a reference to the "CameraEdge" gameObject
/// 	11: Make sure that the GameManager's "All Walls Parent" is a reference to the "AllLineOfSights" object.
/// 
/// 	
/// 
/// 	Placing out walls:
/// 	
/// 	There are two kind of walls that you can place out: walls that togheter surrounds the player (for example a room - the walls inside the room togheter "surrounds" the player) and walls that obscure vision but do not do not surround the player (for example a pillar inside a room)
/// 	Depending on what kind of wall you are creating line of sight for, step 4 will change slightly. The rest do not change.
///
/// 	1: Create a gameObject and give it the "EdgeScript".
/// 	2: Make the EdgeScript object a child to the "AllLineOfSights" object created in the Start Setup
/// 	3: Create a gameObject and give it the "StaticEdgeScript" and an "EdgeCollider2D"
/// 	4: Change the "StaticEdgeScript" gameObject's layer to be "LineOfSight"
/// 	5: Make the "StaticEdgeScript" gameObject a child to the "EdgeScript"
/// 	6: Make the EdgeCollider2D align with the wall you want to obscure the players vision. Make sure that the EdgeCollider2D does not contain more than 2 points.
/// 		a) If the wall will be part of several walls that togheter surrounds the player, then make sure that the EdgeCollider2D->Points->Element0 points clockwise.
/// 		b) If the wall will at some point of the game NOT be part of several walls that togheter surrounds the player then make sure that the EdgeCollider2D->Points->Element0 points counter clockwise.
/// 	7: Make sure that the booleans "Is Open" and "Wants To Be Open" is true. Make also sure that the "Mask" is set to "LineOfSight"
/// 	8: The variable "Forward Face Is" is an int that can be between 0 and 3. It represents in what direction the edge is "looking" towards. 
/// 		0 means that the edge is looking south.
/// 		1 means that the edge is looking west.
/// 		2 means that the edge is looking north.
/// 		3 means that the edge is looking east.
/// 		Make sure that this value is set as accordingly as possible. 
/// 	9. Repeat step 3-8 until you have reached a full circle of walls. It is no longer important in what direction the EdgeCollider2D->Points->Element0 is pointing in, but make sure that each new "StaticEdgeScript" gameObject is placed in a clockwise/counter clockwise order (depening on what type of wall it is) in the Unity hierachy.
/// 		For example: Given a small room with four walls (AKA walls that surrounds the player), the northen wall may be in the top of the Unity hierarchy, followed by the east wall, followed by the sout wall, and in the bottom of the hierarchy, the west wall. Remember that all walls must be children to the "EdgeScript" GameObject.
/// 	10: Repeat step 1-9 for each object in the world.
/// 
/// 	Placing out doors:
/// 
/// 	1: Give the door in your scene the "DoorScript" script
/// 	1: Find the two "StaticEdgeScript" gameObjects that the door is inbetween (we call these gameObjects "wallX" and "wallY" for easier understanding). In the "doorScript", make sure that the reference "One of the line of sight edges" is a reference to one of the two walls (does not matter which).
/// 	2: Make sure that wallX's "TheSharedDoor" reference is a reference to wallY. Do the same thing for wallY, but to a reference to wallX.
///
/// 	
/// 	FEATURES THAT WILL COME
/// 	1: The algorithm have limited prefab support right now, which makes it impractical to use in larger levels. I will work on allowing better prefab support to enable easier use of the algorithm.
/// 	2: Doors do not "slide" open, they kinda teleports away. This will be addressed.
/// 	
/// 
/// 	CONTACT INFORMATION
/// 	
/// 	If you have any questions or bug reports, please contact me at Emil209@hotmail.com
/// 	
///

public class StaticEdgeScript : MonoBehaviour, EdgeInterface{

	const int INSIDE = 0; // 0000
	const int LEFT = 1;   // 0001
	const int RIGHT = 2;  // 0010
	const int BOTTOM = 4; // 0100
	const int TOP = 8;    // 1000

	[HideInInspector]
	public float key;



	public int forwardFaceIs = -1; //0 för norr, 1 för east, 2 för south, 3 för west

	int forwardFaceWhenClosed = 0; //0 för norr, 1 för east, 2 för south, 3 för west


	public bool isOpen = true;
	public bool wantsToBeOpen = true;

	public StaticEdgeScript theSharedDoor;

	[HideInInspector]
	public Vector2 forwardPoint;
	[HideInInspector]
	public Vector2 backwardPoint;

	bool isFacingRight = true;
	[HideInInspector]
	public bool isCamera = false;
	EdgeInterface forwardEdge;
	EdgeInterface backwardEdge;

	public StaticEdgeScript shareForward, shareBackward;

	public List<DynamicEdgeScript> splittedEdges = new List<DynamicEdgeScript> ();

	public LayerMask mask = 1 << 14;

	public EdgeCollider2D col;
	public EdgeHandlerScript manager;

	public Vector2 center = Vector2.zero;
	public float centerDistance = 0.0001f;

	Vector2 backToFor;
	Vector2 forToBack;

	bool forwardIsIn = false;


	void Awake(){
		col = GetComponent<EdgeCollider2D> ();
		manager = GetComponentInParent<EdgeHandlerScript> ();
	}

	void Start(){
		if (forwardFaceIs == 3) {
			forwardFaceWhenClosed = 0;
		} else {
			forwardFaceWhenClosed = 1 + forwardFaceIs;
		}
	}

	//Places the center behind the edge centerDistance units
	public void changeTheCenterPosition(bool closed){
		int current = forwardFaceIs;
		if (closed) {
			current = forwardFaceWhenClosed;
		}
		center = Vector2.Lerp (transform.TransformPoint (col.points [0]), transform.TransformPoint (col.points [1]), 0.5f);
		switch(current){
		case 0:
			center.y = center.y + centerDistance;
			break;
		case 1:
			center.x = center.x + centerDistance;
			break;
		case 2:
			center.y = center.y - centerDistance;
			break;
		case 3:
			center.x = center.x - centerDistance;
			break;
		default:
			throw new Exception("The Variable 'forwardFaceIs' is not set to a value between 0-3.");
		}
	}
		


	//The Cohen Sutherlands Line Clipping algorithm
	//MOST OF THE CREDIT GOES TO ALL THE PEOPLE THAT WORKED ON THE IMPLEMENTATION IN THE WIKIPEDIA ARTCILE https://en.wikipedia.org/wiki/Cohen%E2%80%93Sutherland_algorithm
	//I have only changed the example provided in the article to fit my purposes.
	// forwardClip and backwardClip are the positions for the potential intersections between this edge and one (or two) of the camera edges.
	//forwardSide represents what Camera side the forwardClip collided with, and backwardSide represents what Camera side the backwardClip collided with.
	// If 0, this edge collided with the top side of the Camera. 
	// If 1, this edge collided with the right side of the Camera.
	// If 2, this edge collided with the bottom side of the Camera.
	// If 3, this edge collided with the left side of the Camera.
	// If -1, this edge did not collide with any of the Cameras edges.
	bool cohenSutherlandsLineClipping(out Vector2 forwardClip, out Vector2 backwardClip, out int forwardSide, out int backwardSide, Bounds theBox){
		double x0 = Math.Round (System.Convert.ToDouble (forwardPoint.x), 3);
		double y0 = Math.Round (System.Convert.ToDouble (forwardPoint.y), 3);
		double x1 = Math.Round (System.Convert.ToDouble (backwardPoint.x), 3);
		double y1 = Math.Round (System.Convert.ToDouble (backwardPoint.y), 3);


		// Compute the bit code for a point (x, y) using the clip rectangle
		// bounded diagonally by (theBox.min.x, theBox.min.y), and (theBox.max.x, theBox.max.y)

		// ASSUME THAT theBox.max.x, theBox.min.x, theBox.max.y and theBox.min.y are global constants.



		// Cohen–Sutherland clipping algorithm clips a line from
		// P0 = (x0, y0) to P1 = (x1, y1) against a rectangle with 
		// diagonal from (theBox.min.x, theBox.min.y) to (theBox.max.x, theBox.max.y).
			// compute outcodes for P0, P1, and whatever point lies outside the clip rectangle
		int outcode0 = ComputeOutCode(x0, y0, theBox);
		int outcode1 = ComputeOutCode(x1, y1, theBox);
			bool accept = false;
		forwardSide = -1;
		backwardSide = -1;
			while (true) {
				if (0  == (outcode0 | outcode1)) { // Bitwise OR is 0. Trivially accept and get out of loop
					accept = true;
					break;
			} else if (0 != (outcode0 & outcode1)) { // Bitwise AND is not 0. Trivially reject and get out of loop
					break;
				} else {
					// failed both tests, so calculate the line segment to clip
					// from an outside point to an intersection with clip edge
				double x, y = 0;
				bool wasItForward = false;
				int toSwitch = -1;
					// At least one endpoint is outside the clip rectangle; pick it.
				int outcodeOut = 0;

				//If the Edge needed cutting in the front, then set wasItForward to true
				if (outcode0 != 0) {
					outcodeOut = outcode0;
					wasItForward = true;
				} else {
					outcodeOut = outcode1;
				}

					// Now find the intersection point;
					// use formulas y = y0 + slope * (x - x0), x = x0 + (1 / slope) * (y - y0)
				if (0 != (outcodeOut & TOP)) {           // point is above the clip rectangle
					x = x0 + (x1 - x0) * (theBox.max.y - y0) / (y1 - y0);
					y = theBox.max.y;
					toSwitch = 0;
				} else if (0 != (outcodeOut & BOTTOM)) { // point is below the clip rectangle
					x = x0 + (x1 - x0) * (theBox.min.y - y0) / (y1 - y0);
					y = theBox.min.y;
					toSwitch = 2;
				} else if (0 != (outcodeOut & RIGHT)) {  // point is to the right of clip rectangle
					y = y0 + (y1 - y0) * (theBox.max.x - x0) / (x1 - x0);
					x = theBox.max.x;
					toSwitch = 1;
				} else if (0 != (outcodeOut & LEFT)) {   // point is to the left of clip rectangle
					y = y0 + (y1 - y0) * (theBox.min.x - x0) / (x1 - x0);
					x = theBox.min.x;
					toSwitch = 3;
				} else {
					throw new InvalidOperationException("Cohens Algoritm failed");	
				}

				//If the Edge needed cutting in the front, then update the forwardSide to what side of the Camera the Edge collided with
				if (wasItForward) {
					forwardSide = toSwitch;
				} else {
					backwardSide = toSwitch;
				}
					// Now we move outside point to intersection point to clip
					// and get ready for next pass.
					if (outcodeOut == outcode0) {
						x0 = x;
						y0 = y;
					outcode0 = ComputeOutCode(x0, y0, theBox);
					} else {
						x1 = x;
						y1 = y;
					outcode1 = ComputeOutCode(x1, y1, theBox);
					}
				}
			}
		if (accept) {
			// Following functions are left for implementation by user based on
			// their platform (OpenGL/graphics.h etc.)
			forwardClip = new Vector2 ((float)x0, (float)y0);
			backwardClip = new Vector2 ((float)x1, (float)y1);
			return true;
		} else {
			forwardClip = Vector2.zero;
			backwardClip = Vector2.zero;
			return false;
		}

	}

	int ComputeOutCode(double x, double y, Bounds theBox)
	{
		int code;

		code = INSIDE;          // initialised as being inside of clip window

		if (x < theBox.min.x)           // to the left of clip window
			code |= LEFT;
		else if (x > theBox.max.x)      // to the right of clip window
			code |= RIGHT;
		if (y < theBox.min.y)           // below the clip window
			code |= BOTTOM;
		else if (y > theBox.max.y)      // above the clip window
			code |= TOP;

		return code;
	}

	//Prepare the StaticEdge to be evaluated by returning it to its original state
	void reset(){
		splittedEdges.Clear ();
		col.enabled = true;
	}


	//Find if the edge is inside the camera bounds. If the edge collides with one or more of the camera edges, then split both the edge and the camera Edge(s) accordingly. Then make the splitted edges be connected to eachother.
	public bool needsToBeCalculated(Bounds theBox){


		Vector2 inter1;
		Vector2 inter2;
		int side1;
		int side2;

		//check if the edge is inside the Cameras view
		if(!cohenSutherlandsLineClipping(out inter1,out inter2,out  side1,out side2, theBox)){ 
			return false;
		}

		//Return the edge to its original state
		reset ();


		int sidesColl = 0;
		if (side1 != -1) {
			sidesColl += 1;
		}
		if (side2 != -1) {
			sidesColl += 1;
		}
		forwardIsIn = false;

		//how to proceed depends on how many different camera sides the edge collided with
		switch (sidesColl) {
		case 0:
			forwardIsIn = true;
			return true;
		case 1:
			//if the forwardPoint is inside the camera, then split the camera edge and this edge. Then change this edges backward neighbour to the splitted camera edge 
			if (side2 != -1) {
				if (side2 == -1) {
					throw new InvalidOperationException ("Cohen Sutherland's Line Clipping algorithm returned wrong results");
				}

				StaticEdgeScript cameraEdge = MyGameManager.itself.cameraEdge.getStatic (side2);
				forwardCombine (ref cameraEdge, inter2);

				forwardIsIn = true;
			}
			//else, split the camera edge and this edge. Then change this edges forward neighbour to the splitted camera edge 
			else {
				if (side1 == -1) {
					Debug.Log (gameObject.name);
					Debug.Break ();
				}
				StaticEdgeScript cameraEdge = MyGameManager.itself.cameraEdge.getStatic (side1);
				backwardCombine (ref cameraEdge, inter1);
			}
			return true;
		case 2:

			//if this edge collides with two edges of the camera, then split it two times and change both its neighbour
			StaticEdgeScript cameraEdge1 = MyGameManager.itself.cameraEdge.getStatic (side1);
			StaticEdgeScript cameraEdge2 = MyGameManager.itself.cameraEdge.getStatic (side2);

			forwardCombine (ref cameraEdge2, inter2);
			backwardCombine (ref cameraEdge1, inter1);

			return true;
		default:
			throw new InvalidOperationException("edge collided 3 or more times");	
		}
	}
		

	//Split this edge into two DynamicEdges - a forward part inside the camera and a backward part outside the camera - based on the intersection coordinate. Then split the cameraEdge into a backward part that will be connected to this edge's new forward part, and another part that is not, based on the intersection coordinate.
	void forwardCombine(ref StaticEdgeScript cameraEdge, Vector3 intersection){
		//If the cameraEdge has been splitted before, we want to split the DynamicEdge of that camera that contains the intersection coordinate.
		int index  = returnRightEdge (cameraEdge, intersection);
		EdgeInterface theEdgeToCut = cameraEdge;
		if (index == -1) {
			index = 0;
		}else{
			theEdgeToCut = cameraEdge.splittedEdges [index];
		}

		//Create the new DynamicEdge that represents the backward part of the cameraEdge to be split, based on the 
		DynamicEdgeScript newEdgeBackward = new DynamicEdgeScript (intersection, theEdgeToCut.getBackwardPoint ());
		newEdgeBackward.belongsTo = cameraEdge;
		//if the cameraEdge that was splitted had a backward neighbour, take that neighbour and make it a backward neighbour to the newly created Camera backward part...
		newEdgeBackward.setBackwardEdge(theEdgeToCut.getBackwardEdge ());
		//..then inform the backward neighbour that it has a new forward neighbour
		if (theEdgeToCut.getBackwardEdge () != null) {
			if (theEdgeToCut.getBackwardEdge ().getForwardEdge () == theEdgeToCut) {
				theEdgeToCut.getBackwardEdge ().setForwardEdge (newEdgeBackward);
			}
		}

		//Now split this edge and connect its forward to the newly made Camera backward part
		splitThisEdgeForward (ref newEdgeBackward, (Vector2)intersection);

		//Now create the forward part of the Camera edge to be split
		DynamicEdgeScript newEdgeForward = new DynamicEdgeScript (theEdgeToCut.getForwardPoint(), intersection);
		newEdgeForward.belongsTo = cameraEdge;
		//set the backward part as the backward neighbour for the forwardPart
		newEdgeForward.setBackwardEdge (newEdgeBackward);
		//if the Camera edge that was splitted had a forward neighbour, take that neighbour and make it a forward neighbour to the newly created Camera forward part...
		newEdgeForward.setForwardEdge(theEdgeToCut.getForwardEdge ());
		//..then inform the forward neighbour that it has a new backward neighbour
		if (theEdgeToCut.getForwardEdge () != null) {
			if (theEdgeToCut.getForwardEdge ().getBackwardEdge () == theEdgeToCut) {
				theEdgeToCut.getForwardEdge ().setBackwardEdge (newEdgeForward);
			}
		}


		//Add the newly created parts to the collection representing all parts of this edge, in a sorted order with the most forward edge first in the collection, and the most backward edge last in the collection
		cameraEdge.splittedEdges [index] = newEdgeForward; //Replaces the edge that was split
		cameraEdge.splittedEdges.Insert (index+1, newEdgeBackward);

		//inform potential nearby parts that they have new neighbours
		if (index != 0) {
			if (cameraEdge.splittedEdges [index - 1].getBackwardEdge () == theEdgeToCut) {
				cameraEdge.splittedEdges [index - 1].setBackwardEdge (newEdgeForward);
			}
		}

		if (index + 2 != cameraEdge.splittedEdges.Count) {
			if (cameraEdge.splittedEdges [index +2].getForwardEdge () == theEdgeToCut) {
				cameraEdge.splittedEdges [index + 2].setForwardEdge (newEdgeBackward);
			}
		}

	}

	//Split this Edge into a forward part and a backward part based on the pointOfIntersect. Then connect the forward part to the connectedEdge
	public void splitThisEdgeForward(ref DynamicEdgeScript connectedEdge, Vector2 pointOfIntersect){
		//If this Edge has been splitted before, we want to split the DynamicEdge that contains the intersection coordinate.
		int index = returnRightEdge (this, pointOfIntersect);
		EdgeInterface theEdgeToCut = this;
		if (index == -1) {
			index = 0;
		}else{
			theEdgeToCut = this.splittedEdges [index];
		}
		//Create the forward part of the edge to be split
		DynamicEdgeScript newEdgeForward = new DynamicEdgeScript (theEdgeToCut.getForwardPoint(), pointOfIntersect);
		newEdgeForward.belongsTo = this;
		//Connect the forward part with the connectedEdge by making the backward neighbour the connectedEdge
		newEdgeForward.setBackwardEdge(connectedEdge);
		//Then update the connectedEdge that the forward part is its new forward neighbour
		connectedEdge.setForwardEdge (newEdgeForward);
		//If the edgeToBeCut had a forward neighbour, take that neighbour and make it a forward neighbour to the newly created forward part...
		newEdgeForward.setForwardEdge(theEdgeToCut.getForwardEdge());
		//..then inform the forward neighbour that it has a new backward neighbour
		if (theEdgeToCut.getForwardEdge() != null) {
			if (theEdgeToCut.getForwardEdge ().getBackwardEdge () == theEdgeToCut) {
				theEdgeToCut.getForwardEdge ().setBackwardEdge (newEdgeForward);
			}
		}

		//Now create the backward part
		DynamicEdgeScript newEdgeBackward = new DynamicEdgeScript (pointOfIntersect, theEdgeToCut.getBackwardPoint ());
		newEdgeBackward.belongsTo = this;
		//set the forward part as the forward neighbour for the backwardpwart
		newEdgeBackward.setForwardEdge (newEdgeForward);
		//If the splitted edge had a backward neighbour, then take that neighbour and make it a backward neighbour to the backward part...
		newEdgeBackward.setBackwardEdge(theEdgeToCut.getBackwardEdge ());
		//...then inform the neighbour that it has a new forward neighbour
		if (theEdgeToCut.getBackwardEdge () != null) {
			if (theEdgeToCut.getBackwardEdge ().getForwardEdge () == theEdgeToCut) {
				theEdgeToCut.getBackwardEdge ().setForwardEdge (newEdgeBackward);
			}
		}

		//Add the newly created parts to the collection representing all parts of this edge, in a sorted order with the most forward edge first in the collection, and the most backward edge last in the collection
		splittedEdges[index] = newEdgeForward; //Replaces the edge that was split
		splittedEdges.Insert(index+1, newEdgeBackward);

		//inform potential nearby parts that they have new neighbours
		if (index != 0) {
			if (splittedEdges [index - 1].getBackwardEdge () == theEdgeToCut) {
				splittedEdges [index - 1].setBackwardEdge (newEdgeForward);
			}
		}

		if (index + 2 != splittedEdges.Count) {
			if (splittedEdges [index +2].getForwardEdge () == theEdgeToCut) {
				splittedEdges [index + 2].setForwardEdge (newEdgeBackward);
			}
		}
	}

	//Split this edge into two DynamicEdges - a backward part inside the camera and a forward part outside the camera - based on the intersection coordinate. Then split the cameraEdge into a forward part that will be connected to this edge's new backward part, and another part that is not, based on the intersection coordinate.
	void backwardCombine(ref StaticEdgeScript cameraEdge, Vector3 intersection){
		//If the cameraEdge has been splitted before, we want to split the DynamicEdge of that camera that contains the intersection coordinate.
		int index = returnRightEdge (cameraEdge, intersection);
		EdgeInterface theEdgeToCut = cameraEdge;
		if (index == -1) {
			index = 0;
		}else{
			theEdgeToCut = cameraEdge.splittedEdges [index];
		}

		//Create the forward part of the edge to be split
		DynamicEdgeScript newEdgeForward = new DynamicEdgeScript (theEdgeToCut.getForwardPoint(), intersection);
		newEdgeForward.belongsTo = cameraEdge;
		//If the theEdgeToCut had a forward neighbour, take that neighbour and make it a forward neighbour to the newly created forward part...
		newEdgeForward.setForwardEdge(theEdgeToCut.getForwardEdge ());
		//...then inform the neighbour that it has a new backward neighbour
		if (theEdgeToCut.getForwardEdge() != null) {
			if (theEdgeToCut.getForwardEdge ().getBackwardEdge () == theEdgeToCut) {
				theEdgeToCut.getForwardEdge ().setBackwardEdge (newEdgeForward);
			}
		}

		//Now split this edge and connect its backward part to the newly made Camera forward part
		splitThisEdgeBackward (ref newEdgeForward, (Vector2)intersection);

		//Now create the backward part
		DynamicEdgeScript newEdgeBackward = new DynamicEdgeScript (intersection, theEdgeToCut.getBackwardPoint ());
		newEdgeBackward.belongsTo = cameraEdge;
		//set the forward part as the forward neighbour for the backwardpwart
		newEdgeBackward.setForwardEdge (newEdgeForward);
		//If the splitted edge had a backward neighbour, then take that neighbour and make it a backward neighbour to the backward part...
		newEdgeBackward.setBackwardEdge(theEdgeToCut.getBackwardEdge());
		//...then inform the neighbour that it has a new forward neighbour
		if (theEdgeToCut.getBackwardEdge () != null) {
			if (theEdgeToCut.getBackwardEdge ().getForwardEdge () == theEdgeToCut) {
				theEdgeToCut.getBackwardEdge ().setForwardEdge (newEdgeBackward);
			}
		}

		//Add the newly created parts to the collection representing all parts of this edge, in a sorted order with the most forward edge first in the collection, and the most backward edge last in the collection
		cameraEdge.splittedEdges[index] = newEdgeForward;//Replaces the edge that was split
		cameraEdge.splittedEdges.Insert(index+1, newEdgeBackward);

		//inform potential nearby parts that they have new neighbours
		if (index != 0) {
			if (cameraEdge.splittedEdges [index - 1].getBackwardEdge () == theEdgeToCut) {
				cameraEdge.splittedEdges [index - 1].setBackwardEdge (newEdgeForward);
			}
		}

		if (index + 2 < cameraEdge.splittedEdges.Count) {
			if (cameraEdge.splittedEdges [index +2].getForwardEdge () == theEdgeToCut) {
				cameraEdge.splittedEdges [index + 2].setForwardEdge (newEdgeBackward);
			}
		}
	}

	//Split this Edge into a forward part and a backward part based on the pointOfIntersect. Then connect the backward part to the connectedEdge
	public void splitThisEdgeBackward(ref DynamicEdgeScript connectedEdge, Vector2 pointOfIntersect){
		//If this Edge has been splitted before, we want to split the DynamicEdge that contains the intersection coordinate.
		int index  = returnRightEdge (this, pointOfIntersect);
		EdgeInterface theEdgeToCut = this;
		if (index == -1) {
			index = 0;
		}else{
			theEdgeToCut = this.splittedEdges [index];
		}

		//Create the backward part of the edge to be split
		DynamicEdgeScript newEdgeBackward = new DynamicEdgeScript (pointOfIntersect, theEdgeToCut.getBackwardPoint());
		newEdgeBackward.belongsTo = this;
		//Connect the backward part with the connectedEdge by making the forward neighbour the connectedEdge
		newEdgeBackward.setForwardEdge(connectedEdge);
		//Then update the connectedEdge that the backward part is its new backward neighbour
		connectedEdge.setBackwardEdge (newEdgeBackward);
		//If the splitted edge had a backward neighbour, then take that neighbour and make it a backward neighbour to the backward part...
		newEdgeBackward.setBackwardEdge(theEdgeToCut.getBackwardEdge());
		//...then inform the neighbour that it has a new forward neighbour
		if (theEdgeToCut.getBackwardEdge() != null) {
			if (theEdgeToCut.getBackwardEdge ().getForwardEdge () == theEdgeToCut) {
				theEdgeToCut.getBackwardEdge ().setForwardEdge (newEdgeBackward);
			}
		}


		//Now create the forward part
		DynamicEdgeScript newEdgeForward = new DynamicEdgeScript (theEdgeToCut.getForwardPoint (), pointOfIntersect);
		newEdgeForward.belongsTo = this;
		//set the backward part as the backward neighbour for the forwardPart
		newEdgeForward.setBackwardEdge (newEdgeBackward);
		//If the splitted edge had a forward neighbour, then take that neighbour and make it a forward neighbour to the forward part...
		newEdgeForward.setForwardEdge(theEdgeToCut.getForwardEdge ());
		//...then inform the neighbour that it has a new backward neighbour
		if (theEdgeToCut.getForwardEdge () != null) {
			if (theEdgeToCut.getForwardEdge ().getBackwardEdge () == theEdgeToCut) {
				theEdgeToCut.getForwardEdge ().setBackwardEdge (newEdgeForward);
			}
			}

		//Add the newly created parts to the collection representing all parts of this edge, in a sorted order with the most forward edge first in the collection, and the most backward edge last in the collection
		splittedEdges[index] = newEdgeForward;//Replaces the edge that was split
		splittedEdges.Insert (index +1, newEdgeBackward);

		//inform potential nearby parts that they have new neighbours
		if (index != 0) {
			if (splittedEdges [index - 1].getBackwardEdge () == theEdgeToCut) {
				splittedEdges [index - 1].setBackwardEdge (newEdgeForward);
			}
		}

		if (index + 2 != splittedEdges.Count) {
			if (splittedEdges [index +2].getForwardEdge () == theEdgeToCut) {
				splittedEdges [index + 2].setForwardEdge (newEdgeBackward);
			}
		}
	
	}


	//Checks if the staticEdge have been splitted before, and if it has return the index of the staticEdge's splittedEdges collection that contains the dynamicEdge that contains the pointOfIntersect coordinates
	//If it has not been splitted before, create a placeholder dynamicEdge that the algorithm will replace later in the code, and return -1 to represent that it has not been splitted before
	private int returnRightEdge(StaticEdgeScript staticEdge, Vector3 pointOfIntersect){
		//If it has not been splitted before, create a placeholder dynamicEdge that the algorithm will replace later in the code, and return -1 to represent that it has not been splitted before
		if (staticEdge.splittedEdges.Count == 0) {
			staticEdge.splittedEdges.Add (new DynamicEdgeScript (Vector3.zero, Vector3.zero));
			return -1;
		}
		int index = 0;

		bool done = false;
		float shortestDist = float.MaxValue;
		float contester;
		int x = 0;
		//Iterate through the splittedEdges to get the index of the dynamicEdge which backwardPoint is closest to the pointOfIntersect
		while (!done && x < staticEdge.splittedEdges.Count) {
			contester = Vector2.Distance (staticEdge.splittedEdges [x].backwardPoint, (Vector2)pointOfIntersect);
			if (shortestDist > contester) {
				shortestDist = contester;
				index = x;
				x += 1;
			} else {
				done = true;
			}
		}
			
		//If the index representing an dynamicEdge is the last index of the collection...
		if (index != staticEdge.splittedEdges.Count-1) {
			//... then check if the pointOfIntersection is located in the dynamicEdge part behind the current index dynamicEdge part by determining which point is closest to the pointOfIntersect after
				//moving the backwardPoint of the current index slightly towards the index's forwardPoint, and moving the forwardPoint of the index+1 dynamicEdge towards the backwardPoint with an equally small margin
			float distance1 = Vector2.Distance (Vector2.MoveTowards (staticEdge.splittedEdges [index].backwardPoint, staticEdge.splittedEdges [index].forwardPoint, 0.001f), pointOfIntersect);
			float distance2 = Vector2.Distance (Vector2.MoveTowards (staticEdge.splittedEdges [index + 1].forwardPoint, staticEdge.splittedEdges [index + 1].backwardPoint, 0.001f), pointOfIntersect);

			if ( distance1>= distance2) {
				index += 1;
			}
		}

		//Return the index
		return index;
	}
		


	//return true if the edge is facing toward the player, and sent out the part of the Edge that is inside the camera.
	public bool isFacingPlayer(out EdgeInterface visible){
		Vector2 playerPos = (Vector2)MyGameManager.itself.player.transform.position;

		visible = this;

		//If this edge have been splitted, and its forwardPoint is not inside the camera, then the dynamicEdge that is inside the camera is located in index 1 of the collection
		if (splittedEdges.Count != 0) {
			if (forwardIsIn) {
				visible = splittedEdges [0];
			} else {
				visible = splittedEdges [1];
			}
		}

		return checkFacing (playerPos, visible);
	}


	//Return true if the edge is facing towards the player
	private bool checkFacing(Vector2 playerPos, EdgeInterface me){
		//Cast a ray from a point slightly behind the edgeCollider2d towards the player
		RaycastHit2D ray = Physics2D.Linecast (center, playerPos, mask);
		bool ready = false;
		List<StaticEdgeScript> edges2 = new List<StaticEdgeScript> ();
		int infiniteLoopProt2 = 0;
		while (!ready&& infiniteLoopProt2 < 100) {
			//If the ray hit nothing, then the edge is not facing towards the player
			if (ray.collider == null) {
				foreach (StaticEdgeScript ed in edges2) {
					ed.col.enabled = true;
				}
				notFacingForward (me);
				isFacingRight = false;

				return false;
			} else {
				//If the ray hit this edge, then the edge is facing towards the player
				if (ray.collider.gameObject == this.gameObject) {
					foreach (StaticEdgeScript ed in edges2) {
						ed.col.enabled = true;
					}
	
					isFacingRight = true;
					return true;
		
				} else {
					//If the distance from the center and the point of collision is less than the distance in which we have moved the center behind the edgeCollider2d, then temporary disable the StaticEdge hit, and cast the ray again
					if (Vector2.Distance (ray.point, center) < centerDistance) {
						StaticEdgeScript tempStats = ray.collider.GetComponent<StaticEdgeScript> ();
						tempStats.col.enabled = false;
						edges2.Add (tempStats);
						ray = Physics2D.Linecast (center, playerPos, mask);
						infiniteLoopProt2 += 1;
					} else {
						//If the ray collided with the object it share its forwarPoint coordinates with...
						if (ray.collider.gameObject == shareForward.gameObject) {
							//...then check if the distance from the point of collision towards the shared forwardPoint is less than the distance we have moved the center behind the edgeCollider2d.
							//If that is the case, return true, else return false
							if (Vector2.Distance (ray.point, forwardPoint) < centerDistance) {

								foreach (StaticEdgeScript ed in edges2) {
									ed.col.enabled = true;
								}
								isFacingRight = true;
								return true;

							} else {
								foreach (StaticEdgeScript ed in edges2) {
									ed.col.enabled = true;
								}
								notFacingForward (me);

								isFacingRight = false;
								return false;
							}
						}
						//If the ray collided with the object it share its backwardPoint coordinates with...
						else if (ray.collider.gameObject == shareBackward.gameObject) {
							//...then check if the distance from the point of collision towards the shared backwardPoint is less than the distance we have moved the center behind the edgeCollider2d.
							//If that is the case, return true, else return false
							if (Vector2.Distance (ray.point, backwardPoint) < 0.001f) {

								foreach (StaticEdgeScript ed in edges2) {
									ed.col.enabled = true;
								}
								isFacingRight = true;
								return true;
							} else {
								foreach (StaticEdgeScript ed in edges2) {
									ed.col.enabled = true;
								}
								notFacingForward (me);

								isFacingRight = false;
								return false;
							}
						}
						//If the ray collided with another staticEdge that do not fill the requirements stated above, then return false
						else {
							foreach (StaticEdgeScript ed in edges2) {
								ed.col.enabled = true;
							}
							notFacingForward (me);

							isFacingRight = false;
							return false;
						}
					}
				}
			}
		}
		if (infiniteLoopProt2 > 70) {
			throw new Exception ("InfiniteLoop at checkingFacing");
		}
		return true;
	}

	//If the edge is not facing towards the player, then take it out of the algorithm entirely
	void notFacingForward(EdgeInterface me){
		if (me.getForwardEdge() != null) {

			//parts representing the Camera are not allowed to have no neighbours, so give it a new neighbour if that would be the case
			if (me.getForwardEdge ().isThisCamera ()) {
				DynamicEdgeScript cameraDyn = (DynamicEdgeScript)me.getForwardEdge();
				cameraDyn.connectToBackwardNeighbour ();
			} else {
				me.getForwardEdge ().setBackwardEdge (null);
			}
			me.setForwardEdge (null);
		}
			
		if (me.getBackwardEdge() != null) {

			//parts representing the Camera are not allowed to have no neighbours, so give it a new neighbour if that would be the case
			if (me.getBackwardEdge ().isThisCamera ()) {
				DynamicEdgeScript cameraDyn = (DynamicEdgeScript)me.getBackwardEdge();
				cameraDyn.connectToForwardNeighbour ();
			} else {
				me.getBackwardEdge ().setForwardEdge (null);
			}
			me.setBackwardEdge(null);
		}
		col.enabled = false;
	}

	//Check if any of the edge's neighbours have been taken out of the algorithm. If so, then create new neighbours by shooting a projectiong based on the players position
	public void shootProjections(){
		shootForward ();

		shootBackward ();
	}

	//Check if the edge's forwardPoint lacks a neighbour, and if that is the case, create a new neighbour by shooting a projection based on the players position
	void shootForward(){
		//If the edge has been splitted, then use the forward-most part
		EdgeInterface me = this;
		if (splittedEdges.Count > 0) {
			me = splittedEdges [0];
		}

		if (me.getForwardEdge() == null){
			//Disable itself temporary to make sure the upcoming raycast does not hit itself
			col.enabled = false;
			//Shoot a ray in the direction opposite of the players position
			RaycastHit2D ray = Physics2D.Raycast (me.getForwardPoint(), me.getForwardPoint() - (Vector2)MyGameManager.itself.player.transform.position, Mathf.Infinity, mask);
			col.enabled = true;
			if (ray.collider != null) {
				StaticEdgeScript tempStatic = ray.collider.gameObject.GetComponent<StaticEdgeScript> ();
				//Create a new dynamicEdge representing the "bridge" between this edge and the edge the ray hit
				DynamicEdgeScript tempEdge1 = new DynamicEdgeScript (ray.point, me.getForwardPoint ()); 
				//Since this dynamicEdge does not represent an staticEdge, we inform that it is a projection
				tempEdge1.isProjection = true;
				//Set the projection as this edge's new forward neighbour
				me.setForwardEdge (tempEdge1);
				//Set this edge as the projection's new backward neighbour
				tempEdge1.backwardEdge = me;
				//Split the staticEdge that the ray hit, and connect its newly created forward part with the projection
				tempStatic.splitThisEdgeForward (ref tempEdge1, ray.point);
			} else {
				Debug.Log ("Raycast did not hit anything at ForwardPoint " + gameObject.name);
				throw new InvalidOperationException();
			}
		}

	}

	//Check if the edge's backwardPoint lacks a neighbour, and if that is the case, create a new neighbour by shooting a projection based on the players position
	void shootBackward(){
		//If the edge has been splitted, then use the backward-most part
		EdgeInterface me = this;
		if (splittedEdges.Count > 0) {
			me = splittedEdges[splittedEdges.Count-1];
		}

		if (me.getBackwardEdge() == null){
			//Disable itself temporary to make sure the upcoming raycast does not hit itself
			col.enabled = false;
			//Shoot a ray in the direction opposite of the players position
			RaycastHit2D ray = Physics2D.Raycast (me.getBackwardPoint(), me.getBackwardPoint() - (Vector2)MyGameManager.itself.player.transform.position, Mathf.Infinity, mask);
			col.enabled = true;
			if(ray.collider != null){
				StaticEdgeScript tempStatic = ray.collider.gameObject.GetComponent<StaticEdgeScript> ();
				//Create a new dynamicEdge representing the "bridge" between this edge and the edge the ray hit
				DynamicEdgeScript tempEdge1 = new DynamicEdgeScript (me.getBackwardPoint(), ray.point); 
				//Since this dynamicEdge does not represent an staticEdge, we inform that it is a projection
				tempEdge1.isProjection = true;
				//Set the projection as this edge's new backward neighbour
				me.setBackwardEdge(tempEdge1);
				//Set this edge as the projection's new forward neighbour
				tempEdge1.forwardEdge = me;
				//Split the staticEdge that the ray hit, and connect its newly created backward part with the projection
				tempStatic.splitThisEdgeBackward (ref tempEdge1, ray.point);
			}else {
				Debug.Log ("Ray was null at Forward " + gameObject.name);
				throw new InvalidOperationException();
			}
		}

	}


	//Open or close the door
	public void openAndCloseDoor(){
		//If the door wants to be in a state that it currently isn't...
		if (wantsToBeOpen != isOpen) {
			Vector2[] points = col.points;

			//... then find what point of the EdgeCollider2D is the forwardPoint, then change that point to the new position of the closed/opened door
			float dist1 = Vector2.Distance (shareForward.backwardPoint, gameObject.transform.TransformPoint (points [0]));
			float dist2 = Vector2.Distance (shareForward.backwardPoint, gameObject.transform.TransformPoint (points [1]));

			forwardPoint = theSharedDoor.shareForward.backwardPoint;
			if (dist1 < dist2) {
				points [0] = gameObject.transform.InverseTransformPoint (forwardPoint);
			} else {
				points [1] = gameObject.transform.InverseTransformPoint (forwardPoint);
			}


			//Do the same process for the opposite wall
			Vector2[] points2 = theSharedDoor.col.points;

			float dist11 = Vector2.Distance (theSharedDoor.shareForward.backwardPoint, theSharedDoor.gameObject.transform.TransformPoint (points2 [0]));
			float dist22 = Vector2.Distance (theSharedDoor.shareForward.backwardPoint, theSharedDoor.gameObject.transform.TransformPoint (points2 [1]));

			theSharedDoor.forwardPoint = shareForward.backwardPoint;
			if (dist11 < dist22) {
				points2 [0] = theSharedDoor.gameObject.transform.InverseTransformPoint (theSharedDoor.forwardPoint);
			} else {
				points2 [1] = theSharedDoor.gameObject.transform.InverseTransformPoint (theSharedDoor.forwardPoint);
			}



			//Temporary save the edge in which this edge previously shared the forwardPoint with
			StaticEdgeScript tempStatic = shareForward;
			//Update what edge this edge is sharing the forwardPoint with now
			shareForward = theSharedDoor.shareForward;
			//Inform the edge this edge is sharing the forwardPoint with that it is now sharing the point with this
			shareForward.shareBackward = this;

			//Do the same process for the opposite wall
			theSharedDoor.shareForward = tempStatic;
			theSharedDoor.shareForward.shareBackward = theSharedDoor;

			//Update the points for both walls
			col.points = points;
			theSharedDoor.col.points = points2;

			//Change the states of the walls
			isOpen = wantsToBeOpen;
			theSharedDoor.isOpen = wantsToBeOpen;

			//Update the center position of the walls
			changeTheCenterPosition (!wantsToBeOpen);
			theSharedDoor.changeTheCenterPosition (!wantsToBeOpen);
		}
	}

	//The door informs the walls that it is now changing states
	public void prepareOpenDoor(bool open){
		wantsToBeOpen = open;
		theSharedDoor.wantsToBeOpen = open;
	}
	public string getGameName(){
		return gameObject.name;
	}

	public void setForwardEdge (EdgeInterface edge){
		forwardEdge = edge;
	}
	public void setBackwardEdge (EdgeInterface edge){
		backwardEdge = edge;
	}
	public Vector2 getBackwardPoint(){
		return backwardPoint;
	}
	public Vector2 getForwardPoint(){
		return forwardPoint;
	}
	public EdgeInterface getForwardEdge(){
		return forwardEdge;
	}

	public EdgeInterface getBackwardEdge(){
		return backwardEdge;
	}

	public string getName(){
		return "static " + gameObject.name;
	}

	public bool isProj(){
		return false;
	}

	public bool getFacedRight(){
		return isFacingRight;
	}


	public EdgeCollider2D getCol(){

		return col;

	}

	public bool isDyn(){
		return false;
	}
		

	public bool isThisCamera(){
		return isCamera;
	}
}