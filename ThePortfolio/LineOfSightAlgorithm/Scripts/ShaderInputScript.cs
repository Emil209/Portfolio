using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShaderInputScript : MonoBehaviour {
	// Copyright (C) 2016 Emil Almkvist
	//
	// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
	// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
	// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
	//

	private Vector2 checkVector = Vector2.zero;
	private Material mat;
	private PlayerScript player;
	List<Vector4> theShape = new List<Vector4>();


	void Start(){
		player = MyGameManager.itself.player;
		mat = GetComponent<Renderer>().sharedMaterial;
	}

	//Gather and send the required information to the shader
	void Update() {
			checkVector.x = player.transform.position.x;
			checkVector.y = player.transform.position.y;

			MyGameManager.itself.updateLineOfSight (ref theShape);
			mat.SetVector ("_Player1_Pos", checkVector);
			for (int x = 0; x < theShape.Count; x++) {
				mat.SetVector ("dynamicShape" + x, theShape [x]);
			}
			mat.SetFloat ("_NumberOfTriangles", theShape.Count);
	}
}

