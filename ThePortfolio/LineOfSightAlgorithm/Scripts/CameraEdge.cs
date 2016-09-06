using UnityEngine;
using System.Collections;

// Copyright (C) 2016 Emil Almkvist
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

public class CameraEdge : MonoBehaviour {
	public StaticEdgeScript[] allEdges;


	void Start(){
		allEdges = GetComponentsInChildren<StaticEdgeScript> ();
		for (int x = 0; x < allEdges.Length - 1; x++) {
			allEdges [x].shareForward = allEdges [x + 1];
			allEdges [x + 1].shareBackward = allEdges [x];
			allEdges [x].isCamera = true;
		}
		allEdges[allEdges.Length - 1].shareForward = allEdges [0];
		allEdges [0].shareBackward = allEdges[allEdges.Length - 1];
		allEdges [allEdges.Length - 1].isCamera = true;
	}

	//Get the cameras viewport as bounds
	public Bounds currentBounds(){
		Bounds temp = new Bounds ();
		temp.SetMinMax (new Vector3(allEdges [3].backwardPoint.x, allEdges [3].backwardPoint.y, 0) , new Vector3(allEdges [0].forwardPoint.x, allEdges [0].forwardPoint.y, 0));
		return temp;
	}

	//Update the edges based on the main camera's viewport
	public void reset(){

		int x1Side = 0;
		int y1Side = 0;
		int x2Side = 0;
		int y2Side = 0;
		//iterate through all the four camera edges, and then update their position
		for (int x = 0; x < allEdges.Length; x++) {
			switch(x){
			case 0:
				x1Side = 0;
				y1Side = 1;
				x2Side = 1;
				y2Side = 1;
				break;
			case 1:
				x1Side = 1;
				y1Side = 1;
				x2Side = 1;
				y2Side = 0;
				break;
			case 2:
				x1Side = 1;
				y1Side = 0;
				x2Side = 0;
				y2Side = 0;
				break;

			case 3:
				x1Side = 0;
				y1Side = 0;
				x2Side = 0;
				y2Side = 1;
				break;
			default:

				break;
			}
			Vector2[] points = allEdges [x].col.points;
			allEdges [x].backwardPoint = Camera.main.ViewportToWorldPoint (new Vector3 (x1Side, y1Side, -10));
			allEdges [x].forwardPoint = Camera.main.ViewportToWorldPoint (new Vector3 (x2Side, y2Side, -10));
			points [0] = allEdges [x].transform.InverseTransformPoint (allEdges [x].forwardPoint);
			points [1] = allEdges [x].transform.InverseTransformPoint (allEdges [x].backwardPoint);
			allEdges [x].col.points = points;
		}

		//itererate through all the camera edges (note that the last edge must be handled outside the for-loop) and make sure they have their standard neighbours
		for (int x = 0; x < allEdges.Length - 1; x++) {

			allEdges [x].setForwardEdge(allEdges [x].shareForward);
			allEdges [x].getForwardEdge().setBackwardEdge(allEdges [x]);
			allEdges [x].splittedEdges.Clear ();
		}
		allEdges [allEdges.Length - 1].setForwardEdge(allEdges [allEdges.Length - 1].shareForward);
		allEdges[allEdges.Length - 1].shareForward.setBackwardEdge(allEdges [allEdges.Length - 1]);
		allEdges [allEdges.Length - 1].splittedEdges.Clear ();

	}

	public StaticEdgeScript getStatic(int nr){
		return allEdges [nr];
	}
}
