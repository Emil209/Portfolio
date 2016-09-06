using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Copyright (C) 2016 Emil Almkvist
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

public class EdgeHandlerScript : MonoBehaviour {

	public StaticEdgeScript[] allEdges;// = new List<StaticEdgeScript> ();


	//Makes sure all walls do not have any holes
	void Start(){
		allEdges = GetComponentsInChildren<StaticEdgeScript> ();

		//The first Edge is special, so initiate it individually
		initiateFirstEdge ();
		//Then iterate through all the edges (note though that the last edge must be handled outside the for-loop) and connect it to the next edge in the array
		for (int x = 0; x < allEdges.Length - 1; x++) {
			connectEdges (allEdges [x], allEdges [x + 1]);
		}

		connectEdges (allEdges [allEdges.Length - 1], allEdges [0]);

		//Update all the center position in accordance the updated edgeCollider2ds
		foreach (StaticEdgeScript edg in allEdges) {
			edg.changeTheCenterPosition (false);
		}
	}

	//Update the first Edge's forwardPoint and backwardPoint coordinates
	private void initiateFirstEdge(){
		Vector2[] points = allEdges[0].col.points;
		allEdges[0].forwardPoint = allEdges[0].gameObject.transform.TransformPoint(points [0]);
		allEdges[0].backwardPoint = allEdges[0].gameObject.transform.TransformPoint(points [1]);
	}

	//Connect edge1 forward with edge2. Also update edge2's EdgeCollider2d
	public void connectEdges(StaticEdgeScript edge1, StaticEdgeScript edge2){
		//Vector2[] points1 = edge1.col.points;
		Vector2[] points = edge2.col.points;
		//First check what point of edge2's EdgeCollider2D that is closest to edge1's forwardPoint, then make that point the backwardPoint for edge2
		float dist1 = Vector2.Distance (edge1.forwardPoint, edge2.gameObject.transform.TransformPoint(points [0]));
		float dist2 = Vector2.Distance (edge1.forwardPoint, edge2.gameObject.transform.TransformPoint(points [1]));
		int index1;
		int index2;
		if (dist1 < dist2) {
			index1 = 0;
			index2 = 1;
		} else {
			index1 = 1;
			index2 = 0;
		}

		//Tell edge1 that edge2 is its forward neighbour
		edge1.setForwardEdge(edge2);
		//Tell edge2 that edge1 is its backward neighbour
		edge2.setBackwardEdge(edge1);
		//Update edge2's backwardPoint and EdgeCollider2d about the new coordinates for the backwardPoint
		edge2.backwardPoint = edge1.forwardPoint;
		points [index1] = edge2.gameObject.transform.InverseTransformPoint(edge1.forwardPoint);
		edge2.col.points = points;
		//update the edge2's forwardPoint coordinates
		edge2.forwardPoint = edge2.gameObject.transform.TransformPoint(points [index2]);

		//Inform edge1 that it is sharing its forwardPoint with edge2
		edge1.shareForward = edge2;
		//Inform edge2 that it is sharing its backwardPoint with edge1
		edge2.shareBackward = edge1;
	}

	public void reset(){
		
		//Close/open all doors that want to be closed/opened
		for (int x = 0; x < allEdges.Length; x++) {
			allEdges [x].openAndCloseDoor ();
		}

		//itererate through all the edges (note that the last edge must be handled outside the for-loop) and make sure they have their standard neighbours
		for (int x = 0; x < allEdges.Length - 1; x++) {
			
			allEdges [x].setForwardEdge(allEdges [x].shareForward);
			allEdges [x].getForwardEdge().setBackwardEdge(allEdges [x]);
		}
		allEdges [allEdges.Length - 1].setForwardEdge(allEdges [allEdges.Length - 1].shareForward);
		allEdges[allEdges.Length - 1].shareForward.setBackwardEdge(allEdges [allEdges.Length - 1]);


		DynamicEdgeScript.numberOf = 0;
	}
}
