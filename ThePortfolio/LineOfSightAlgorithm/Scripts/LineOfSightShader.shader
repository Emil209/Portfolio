Shader "Custom/LineOfSight" {

// Copyright (C) 2016 Emil Almkvist
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

Properties {
}

SubShader {
    Pass {
        ZWrite On
        ZTest Always
        ColorMask 0
    }pass{
    LOD 200

    Cull Back
    CGPROGRAM
    #pragma vertex vert alpha:blend
    #pragma fragment frag
	#include "UnityCG.cginc"


    float4     _Player1_Pos;
    float4     dynamicShape[100];
	uniform int _NumberOfTriangles;

    struct Triangle {
        float3 Acor;
        float3 Bcor;
        float3 Ccor;
    };

    struct Input {
        float4 location : TEXCOORD1; // : SV_POSITION;
        float4 pos : SV_POSITION;
    };


	bool isInsideTriangle2(float3 Acor, float3 Bcor, float3 Ccor, float3 thePoint);

    Input vert(appdata_full vertexData){
    	Input o;
        o.pos = mul(UNITY_MATRIX_MVP, vertexData.vertex);
        o.location =  mul(_Object2World, vertexData.vertex);




        return o;
    }

    float4 frag(Input IN) : COLOR {
     float3 thePoint = float3(IN.location.x, IN.location.y, 0);
     int index = 0;
     //Iterate through all the triangles and discard the pixel if it is inside one of them
       while(index < _NumberOfTriangles){
			float3 Bcor;
			Bcor.x = dynamicShape[index].x;
			Bcor.y = dynamicShape[index].y;
			Bcor.z = 0;
			float3 Ccor;
			Ccor.x = dynamicShape[index].z;
			Ccor.y = dynamicShape[index].w;
			Bcor.z = 0;
	        if(isInsideTriangle2(_Player1_Pos, Bcor, Ccor, thePoint)){
	        	discard;
				break;
	        }
	        index++;
	    }

	   return float4(0.0, 0.0, 0.0, 1.0);
    }


	bool isInsideTriangle2(float3 Acor, float3 Bcor, float3 Ccor, float3 thePoint) {
		float s = Acor.y * Ccor.x - Acor.x * Ccor.y + (Ccor.y - Acor.y) * thePoint.x + (Acor.x - Ccor.x) * thePoint.y;
		float t = Acor.x * Bcor.y - Acor.y * Bcor.x + (Acor.y - Bcor.y) * thePoint.x + (Bcor.x - Acor.x) * thePoint.y;

		if ((s < 0) != (t < 0))
			return false;

		float A = -Bcor.y * Ccor.x + Acor.y * (Ccor.x - Bcor.x) + Acor.x * (Bcor.y - Ccor.y) + Bcor.x * Ccor.y;
		if (A < 0.0)
		{
			s = -s;
			t = -t;
			A = -A;
		}
		if (s > 0 && t > 0 && (s + t) <= A) {
			return true;
		}
		return false;
	}

    ENDCG
    }
}

Fallback "Transparent/VertexLit"
}
