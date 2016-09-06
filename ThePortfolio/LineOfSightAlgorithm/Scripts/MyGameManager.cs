using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

// Copyright (C) 2016 Emil Almkvist
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

public class MyGameManager : MonoBehaviour {

 
	public int testNr = -3;

	public static MyGameManager itself;
	public PlayerScript player;

	public Transform allWallsParent;
	public CameraEdge cameraEdge;
	public StaticEdgeScript[] edgesOfMap;
	private EdgeHandlerScript[] allWalls;
	public SortedList<float, EdgeInterface> allVisibleEdges = new SortedList<float, EdgeInterface> ();
	public int infiniteLoopGuard = 800;

	
	//List<Vector4> theShape = new List<Vector4>(); //For debugging purposes
	public Bounds cameraBounds;
    

	//Irrelevant for the Line of sight algorithm
	void Awake()
    {
        if (MyGameManager.itself != null)
        {
   
            Destroy(itself.gameObject);
            itself = this;
        }
        else
        {
            itself = this;
        }


    }

	//Irrelevant for the Line of sight algorithm
    void Start()
    {
        edgesOfMap = allWallsParent.GetComponentsInChildren<StaticEdgeScript>();
        allWalls = allWallsParent.GetComponentsInChildren<EdgeHandlerScript>();

    }

	//Irrelevant for the Line of sight algorithm
    void Update()
    {
      //updateLineOfSight (ref theShape); //for testing purposes
    }

	//Fill the triangleFan with the positions rerpesenting the triangles corners
	public void updateLineOfSight(ref List<Vector4> triangleFan){
		//reset all values
		reset(ref triangleFan);


		//Get all edges that are inside the Camera bounds
		List<StaticEdgeScript> isVisible = new List<StaticEdgeScript> ();
		for (int z = 0; z < edgesOfMap.Length; z++) {

			if (edgesOfMap [z].needsToBeCalculated (cameraBounds)) {
				isVisible.Add(edgesOfMap[z]);
			}
		}


		//Iterate through all edges inside the camera bounds and put the edges that are facing the player in a sortedList based on the distance from the player
		for (int z = 0; z < isVisible.Count; z++) {
			bool shouldBeAdded = true;
			EdgeInterface edgeInside;
			shouldBeAdded = isVisible [z].isFacingPlayer (out edgeInside);

			if (shouldBeAdded) {
				isVisible [z].key = NearestPointOnFiniteLine ((Vector3)isVisible [z].backwardPoint, isVisible [z].forwardPoint, player.transform.position);
				while (allVisibleEdges.ContainsKey (isVisible [z].key)) {
					isVisible [z].key += 0.001f;
				}
				allVisibleEdges.Add (isVisible [z].key, edgeInside);
			}

		}

		//Shoot projections from the edges that are facing the player and connect the edges to these projections
		for (int y = 0; y < allVisibleEdges.Count; y++) {
			allVisibleEdges.Values[y].shootProjections ();
		}

		//add a cameraEdge in case there are no other edges inside the camera
		allVisibleEdges.Add(float.MaxValue, cameraEdge.getStatic(0));

		//get the edge that is closest to the player
		EdgeInterface first= allVisibleEdges.Values [0];
		EdgeInterface tempEdge = first;



		bool done = false;
		bool broke = false;
		int iterations = 0;
		List<EdgeInterface> alreadyCalculated = new List<EdgeInterface> ();
		alreadyCalculated.Add (tempEdge);

		//Go clockwise forward through the connected edges starting with the edge closest towards the player and collect the corners of the edges that togheter make up the trianglefan. Stop when encountering an already calculated edge or if something went wrong.
		while (!done && iterations < infiniteLoopGuard) {

			if (iterations == testNr || testNr == -1 || testNr == -2) { //only for debugging purposes
				Debug.DrawLine (tempEdge.getBackwardPoint (), tempEdge.getForwardPoint (), Color.magenta, 0);
				if(testNr != -1){
					Debug.Log ("name " + tempEdge.getName ());
				}
			}

			if (!tempEdge.isProj ()) { //if the edge is a projection, there is no need to add its corners to the trianglefan
				triangleFan.Add (new Vector4 (tempEdge.getBackwardPoint ().x, tempEdge.getBackwardPoint ().y, tempEdge.getForwardPoint ().x, tempEdge.getForwardPoint ().y));
			}

			if (tempEdge.getForwardEdge () == null) { //Something went wrong. If you encounter these debug messages, please make sure you followed the instructions layed out in the "StaticEdgeScript"
													//If you are certain that you have, please manually move the characters position to the position writen out in the "Position" message. 
													//If the messages keeps coming up from that position, please send your scene to Emil209@hotmail.com so I can fix it
				Debug.Log ("iteration " + iterations);
				Debug.Log ("ForwadEdge null: " + tempEdge.getName ());
				Debug.Log ("Point lost: " + tempEdge.getForwardPoint () + " and " + tempEdge.getBackwardPoint ());
					Debug.Log ("Position " + MyGameManager.itself.player.transform.position.x + " y " + MyGameManager.itself.player.transform.position.y);
				broke = true;
				//Debug.Break (); //Used for debugging purposes
				break;

			}
			alreadyCalculated.Add (tempEdge);
			tempEdge = tempEdge.getForwardEdge ();
			iterations += 1;
			if (alreadyCalculated.Contains(tempEdge)) {
				done = true;
			}
			if (iterations > infiniteLoopGuard-1) { //it is good practice in Unity to always have a infiniteLoopGuard preventing infiniteLoops. If you encounter these messages, please increase the infiniteLoopGuard number
														//If these messages still appear, please send your scene to Emil209@hotmail.com so I can fix it
				Debug.Log ("Position " + MyGameManager.itself.player.transform.position.x + " y " + MyGameManager.itself.player.transform.position.y);

				Debug.Log ("Point loop: " + tempEdge.getName () + " forward " + tempEdge.getForwardEdge ().getName () + " forward.forawrd " + tempEdge.getForwardEdge ().getForwardEdge ().getName ());
			}
		}


		//If something broke, but you cannot reproduce the bug (or you don't want to send the scene to me so I can fix it) and you still want to use the algorithm in the scene, this loop will make an attempt to minimize the damge done by the bug by iterating counter clockwise instead of clockwise. This will most likely make the bug not noticeable for players other then perhaps a framedrop
		if (broke) {
			tempEdge = first.getBackwardEdge ();

			if (!alreadyCalculated.Contains (tempEdge)) {
				alreadyCalculated.Add (tempEdge);
				while (!done && iterations < infiniteLoopGuard) {

					if (!tempEdge.isProj ()) {

						triangleFan.Add (new Vector4 (tempEdge.getBackwardPoint ().x, tempEdge.getBackwardPoint ().y, tempEdge.getForwardPoint ().x, tempEdge.getForwardPoint ().y));
					}

					if (tempEdge.getBackwardEdge () == null) {

						Debug.Log ("iteration " + iterations);
						Debug.Log ("ForwadEdge null: " + tempEdge.getName ());
						Debug.Log ("Point lost: " + tempEdge.getForwardPoint () + " and " + tempEdge.getBackwardPoint ());
						Debug.Log ("Position " + MyGameManager.itself.player.transform.position.x + " y " + MyGameManager.itself.player.transform.position.y);
						broke = true;
						//Debug.Break ();
						break;
					}
					alreadyCalculated.Add (tempEdge);
					tempEdge = tempEdge.getBackwardEdge ();
					iterations += 1;

					if (alreadyCalculated.Contains (tempEdge)) {
						done = true;
					}
					if (iterations > infiniteLoopGuard-1) {
						Debug.Log ("Position " + MyGameManager.itself.player.transform.position.x + " y " + MyGameManager.itself.player.transform.position.y);

						Debug.Log ("Point loop: " + tempEdge.getName () + " forward " + tempEdge.getForwardEdge ().getName () + " forward.forawrd " + tempEdge.getForwardEdge ().getForwardEdge ().getName ());
					}
				}
			}
		}
	}

	//Reset all collections and values to a defualt state
	void reset(ref List<Vector4> result){
		result.Clear ();
		allVisibleEdges.Clear ();
		cameraEdge.reset ();
		cameraBounds = cameraEdge.currentBounds ();
		for (int x = 0; x < allWalls.Length; x++) {
			allWalls [x].reset ();
		}
	}

	//find the distance between a point and the closest point on a finite line 
	//CREDIT GOES TO: lordofduct on Unity's forum
	float NearestPointOnFiniteLine(Vector3 start, Vector3 end, Vector3 pnt)
	{
		var line = (end - start);
		var len = line.magnitude;
		line.Normalize();

		var v = pnt - start;
		var d = Vector3.Dot(v, line);
		d = Mathf.Clamp(d, 0f, len);
		return (pnt - (start + line * d)).magnitude;
	}
}
