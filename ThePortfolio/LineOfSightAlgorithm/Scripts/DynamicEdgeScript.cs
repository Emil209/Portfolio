using UnityEngine;
using System.Collections;

// Copyright (C) 2016 Emil Almkvist
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

public class DynamicEdgeScript : EdgeInterface {

	public static int numberOf;

	public int myNumber;
	public bool isProjection = false;
	public StaticEdgeScript belongsTo;

	public Vector2 forwardPoint;
	public Vector2 backwardPoint;

	public EdgeInterface forwardEdge;
	public EdgeInterface backwardEdge;

	public bool forwardHit = false;
	public bool backwardHit = false;

	public DynamicEdgeScript(Vector2 forward, Vector2 backward){
		forwardPoint = forward;
		backwardPoint = backward;
		numberOf += 1;
		myNumber = numberOf;
	}

	public void shootProjections(){
		belongsTo.shootProjections ();
	}

	public EdgeInterface splitThisEdge(bool keepForward, EdgeInterface connectedEdge, Vector2 pointOfIntersect){
		return this;
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
		if (belongsTo != null) {
			return "Dynamic " +myNumber + " belongsTo: " + belongsTo.getName ();

		}
		return  "Dynamic " +myNumber +" Isproj " + isProjection +" back " +  backwardEdge.getName();
	}

	public bool isProj(){
		return isProjection;
	}

	public bool getFacedRight(){
		return belongsTo.getFacedRight();
	}

	public EdgeCollider2D getCol(){
		return belongsTo.col;
	}

	public bool isDyn(){
		return true;
	}

	public string getGameName(){
		return belongsTo.gameObject.name;
	}


	public bool isThisCamera(){
		return belongsTo.isThisCamera();
	}

	//Set the forward edge to be the next part representing the static edge
	public void connectToForwardNeighbour(){
		setForwardEdge (belongsTo.splittedEdges [belongsTo.splittedEdges.IndexOf (this)-1]);
	}

	//Set the backward edge to be the next part representing the static edge
	public void connectToBackwardNeighbour(){
		setBackwardEdge (belongsTo.splittedEdges [belongsTo.splittedEdges.IndexOf (this)+1]);
	}

}
