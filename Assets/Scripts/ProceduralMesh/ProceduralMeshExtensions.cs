/**
 * File Name: ProceduralMeshExtensions.cs
 * Description: Script that contains the extension functionality for the ProceduralMesh class; in particular, these  
 *                  extension functions allow for various procedural mesh triangulations or shape configurations
 * 
 * Authors: Will Lacey
 * Date Created: January 15, 2022
 * 
 * Additional Comments: 
 *      File Line Length: 120
 **/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ProceduralMeshExtensions
{
    /************************************************************/
    #region Triangle & Quad Functions

    public static void TriangulateTriangle(this ProceduralMesh proceduralMesh, float edgeLength, int subdivisions = 0)
    {
        Vector3 v0 = new Vector3(0, 0, 0);
        Vector3 v1 = new Vector3(0, 0, 1);
        Vector3 v2 = new Vector3(1, 0, 0);
        proceduralMesh.AddTriangle(v0, v1, v2, subdivisions);

        Vector2 uv0 = new Vector2(0, 0);
        Vector2 uv1 = new Vector2(0, 1);
        Vector2 uv2 = new Vector2(1, 0);
        proceduralMesh.AddTriangleUV(uv0, uv1, uv2, subdivisions);

        proceduralMesh.Apply();
    }

    public static void TriangulateQuad(this ProceduralMesh proceduralMesh, float edgeLength, int subdivisions = 0)
    {
        float halfEdgeLength = 0.5f * edgeLength;

        Vector3 v0 = new Vector3(halfEdgeLength, 0, halfEdgeLength);
        Vector3 v1 = new Vector3(halfEdgeLength, 0, -halfEdgeLength);
        Vector3 v2 = new Vector3(-halfEdgeLength, 0, -halfEdgeLength);
        Vector3 v3 = new Vector3(-halfEdgeLength, 0, halfEdgeLength);
        proceduralMesh.AddQuad(v0, v1, v2, v3, subdivisions);

        Vector2 uv0 = new Vector2(0, 0);        // 0,1      1,1
        Vector2 uv1 = new Vector2(1, 0);        //  how textures
        Vector2 uv2 = new Vector2(1, 1);        //  are UV'ed
        Vector2 uv3 = new Vector2(0, 1);        // 0,0      1,0
        proceduralMesh.AddQuadUV(uv0, uv1, uv2, uv3, subdivisions);

        proceduralMesh.Apply(hasMeshCollider: true);
    }

    #endregion

    #region Cube & Tetrahedron Functions

    public static void TriangulateCube(this ProceduralMesh proceduralMesh, float edgeLength)
    {
        AddCubePointsToProceduralMesh(proceduralMesh, edgeLength);
        proceduralMesh.Apply();
    }

    private static void AddCubePointsToProceduralMesh(ProceduralMesh proceduralMesh, float edgeLength, 
        int subdivisions = 0)
    {
        float halfEdgeLength = 0.5f * edgeLength;

        Vector3 v0 = new Vector3(halfEdgeLength, halfEdgeLength, halfEdgeLength);
        Vector3 v1 = new Vector3(halfEdgeLength, halfEdgeLength, -halfEdgeLength);
        Vector3 v2 = new Vector3(halfEdgeLength, -halfEdgeLength, halfEdgeLength);
        Vector3 v3 = new Vector3(halfEdgeLength, -halfEdgeLength, -halfEdgeLength);
        Vector3 v4 = new Vector3(-halfEdgeLength, halfEdgeLength, halfEdgeLength);
        Vector3 v5 = new Vector3(-halfEdgeLength, halfEdgeLength, -halfEdgeLength);
        Vector3 v6 = new Vector3(-halfEdgeLength, -halfEdgeLength, halfEdgeLength);
        Vector3 v7 = new Vector3(-halfEdgeLength, -halfEdgeLength, -halfEdgeLength);

        // sides
        proceduralMesh.AddQuad(v0, v2, v3, v1, subdivisions);
        proceduralMesh.AddQuad(v5, v1, v3, v7, subdivisions);
        proceduralMesh.AddQuad(v0, v4, v6, v2, subdivisions);
        proceduralMesh.AddQuad(v4, v5, v7, v6, subdivisions);
        // bases
        proceduralMesh.AddQuad(v0, v1, v5, v4, subdivisions);
        proceduralMesh.AddQuad(v2, v6, v7, v3, subdivisions);
    }

    public static void TriangulateTetrahedron(this ProceduralMesh proceduralMesh, float edgeLength)
    {
        Vector3 centroid = proceduralMesh.transform.position;
        float height = Mathf.Sqrt(2f/3f) * edgeLength;
        float v2z = Mathf.Sqrt(edgeLength * edgeLength - height * height);
        float halfEdgeLength = 0.5f * edgeLength;
        float v34z = -Mathf.Sqrt(v2z * v2z - halfEdgeLength * halfEdgeLength);

        Vector3 v0 = centroid + new Vector3(0, 0.75f * height, 0);
        Vector3 v1 = centroid + new Vector3(0, -0.25f * height, v2z);
        Vector3 v2 = centroid + new Vector3(-halfEdgeLength, -0.25f * height, v34z);
        Vector3 v3 = centroid + new Vector3(halfEdgeLength, -0.25f * height, v34z);

        proceduralMesh.AddTriangle(v0, v2, v1);
        proceduralMesh.AddTriangle(v0, v1, v3);
        proceduralMesh.AddTriangle(v0, v3, v2);
        proceduralMesh.AddTriangle(v3, v1, v2);
        proceduralMesh.Apply();
    }

    #endregion

    #region Sphere Functions

    public static void TriangulateSphere(this ProceduralMesh proceduralMesh, float radius, int subdivisions = 0)
    {
        AddCubePointsToProceduralMesh(proceduralMesh, 1, subdivisions);

        for (int i = 0; i < proceduralMesh.Vertices.Count; i++)
        {
            proceduralMesh.Vertices[i] = radius * Vector3.Normalize(proceduralMesh.Vertices[i]);
            // proceduralMesh.Vertices[i] = radius * PointOnCubeToPointOnSphere(proceduralMesh.Vertices[i]);
        }

        proceduralMesh.Apply();
    }

    // Thanks to http://mathproofs.blogspot.com/2005/07/mapping-cube-to-sphere.html
    public static Vector3 PointOnCubeToPointOnSphere(Vector3 point)
    {
        float x2 = point.x * point.x;
        float y2 = point.y * point.y;
        float z2 = point.z * point.z;
        float x = point.x * Mathf.Sqrt(1 - (y2 + z2) / 2 + (y2 + z2) / 3);
        float y = point.y * Mathf.Sqrt(1 - (x2 + z2) / 2 + (x2 + z2) / 3);
        float z = point.z * Mathf.Sqrt(1 - (x2 + y2) / 2 + (x2 + y2) / 3);
        return new Vector3(x, y, z);
    }

    #endregion

    #region Temp Functions

    public static void TriangulateQuad(this ProceduralMesh proceduralMesh, 
        float lengthX, float lengthZ, int numberOfQuadsX, int numberOfQuadsZ)
    {
        Vector3 v0 = new Vector3();
        Vector3 v1 = new Vector3();
        Vector3 v2 = new Vector3();
        Vector3 v3 = new Vector3();

        Vector2 uv0 = new Vector2();
        Vector2 uv1 = new Vector2();
        Vector2 uv2 = new Vector2();
        Vector2 uv3 = new Vector2();

        for (int z = 0; z < numberOfQuadsZ; z++)
        {
            v1 = new Vector3(0, 0, z * lengthZ / numberOfQuadsZ);
            v3 = new Vector3(0, 0, (z + 1) * lengthZ / numberOfQuadsZ);

            uv1 = new Vector3(0, z * lengthZ / numberOfQuadsZ);
            uv3 = new Vector3(0, (z + 1) * lengthZ / numberOfQuadsZ);
            
            for (int x = 0; x < numberOfQuadsX; x++)
            {
                v0 = v1;
                v1 = new Vector3((x + 1) * lengthX / numberOfQuadsX, 0, z * lengthZ / numberOfQuadsZ);
                v2 = v3;
                v3 = new Vector3((x + 1) * lengthX / numberOfQuadsX, 0, (z + 1) * lengthZ / numberOfQuadsZ);
                proceduralMesh.AddQuad(v2, v3, v1, v0);

                uv0 = uv1;
                uv1 = new Vector2((x + 1) * lengthX / numberOfQuadsX, z * lengthZ / numberOfQuadsZ);
                uv2 = uv3;
                uv3 = new Vector2((x + 1) * lengthX / numberOfQuadsX, (z + 1) * lengthZ / numberOfQuadsZ);

                proceduralMesh.AddQuadUV(uv2, uv3, uv1, uv0);
            }
        }

        proceduralMesh.Apply();
    }

    #endregion
    /************************************************************/
}
