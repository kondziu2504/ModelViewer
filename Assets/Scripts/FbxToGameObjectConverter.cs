using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Autodesk.Fbx;
using EarClipperLib;
using System.Linq;

public static class FbxToGameObjectConverter
{
    private class Triangle
    {
        public Vector3[] points;
        public int[] indices;
        public Vector3[] normals;

        public Triangle(Vector3[] points, int[] indices, Vector3[] normals)
        {
            this.points = points;
            this.indices = indices;
            this.normals = normals;
        }
    }

    public static GameObject Convert(string filename, Material material)
    {
        using (FbxManager fbxManager = FbxManager.Create())
        {
            fbxManager.SetIOSettings(FbxIOSettings.Create(fbxManager, Globals.IOSROOT));

            // Import the scene to make sure file is valid
            using (FbxImporter importer = FbxImporter.Create(fbxManager, "myImporter"))
            {
                bool status = importer.Initialize(filename, -1, fbxManager.GetIOSettings());

                FbxScene scene = FbxScene.Create(fbxManager, "myScene");
                importer.Import(scene);

                return GameObjectFromFbxScene(scene, material);
            }
        }
    }
    
    private static GameObject GameObjectFromFbxScene(FbxScene fbxScene, Material material)
    {
        var rootNode = fbxScene.GetRootNode();
        GameObject model = new GameObject("LoadedModel");
        for (int i = 0; i < rootNode.GetChildCount(); i++)
        {
            GameObject modelPart = ModelPartFromFbxNode(rootNode.GetChild(i), material);
            modelPart.transform.parent = model.transform;
        }

        return model;
    }

    private static GameObject ModelPartFromFbxNode(FbxNode node, Material material)
    {
        GameObject modelPart = new GameObject(node.GetName());
        MeshFilter meshFilter = modelPart.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = modelPart.AddComponent<MeshRenderer>();

        FbxMesh fbxMesh = node.GetMesh();

        Mesh unityMesh = new Mesh();
        List<Vector3> unityVerts = new List<Vector3>();
        List<int> unityTriInds = new List<int>();
        List<Vector3> unityNormals = new List<Vector3>();

        int indexOffset = 0;
        for (int pInd = 0; pInd < fbxMesh.GetPolygonCount(); pInd++)
        {
            Vector3[] polygonPoints = GetPolygonPoints(fbxMesh, pInd);
            Vector3[] polygonNormals = GetPolygonNormals(fbxMesh, pInd);

            Triangle[] triangles = Triangulate(polygonPoints, polygonNormals, indexOffset);

            foreach(Triangle triangle in triangles)
            {
                unityVerts.AddRange(triangle.points);
                unityTriInds.AddRange(triangle.indices);
                unityNormals.AddRange(triangle.normals);
            }

            indexOffset += triangles.Length * 3;
        }

        unityMesh.SetVertices(unityVerts);
        unityMesh.SetTriangles(unityTriInds, 0);
        unityMesh.SetNormals(unityNormals);
        meshFilter.mesh = unityMesh;
        meshRenderer.material = material;

        return modelPart;
    }

    private static Triangle[] Triangulate(Vector3[] polygon, Vector3[] normals, int indexOffset)
    {
        EarClipping earClipping = new EarClipping();
        List<Vector3m> points = polygon.Select((Vector3 input) => new Vector3m(input.x, input.y, input.z)).ToList();
        earClipping.SetPoints(points);
        earClipping.Triangulate();

        Vector3 FromVector3m(Vector3m vector3m) => new Vector3(
                    (float)vector3m.X.GetSignedDouble(),
                    (float)vector3m.Y.GetSignedDouble(),
                    (float)vector3m.Z.GetSignedDouble()
                );

        Triangle[] triangles = new Triangle[earClipping.Result.Count / 3];
        for (int i = 0; i < triangles.Length; i++)
        {
            Vector3[] trianglePoints = new Vector3[]
            {
                FromVector3m(earClipping.Result[i * 3 + 0]),
                FromVector3m(earClipping.Result[i * 3 + 1]),
                FromVector3m(earClipping.Result[i * 3 + 2])
            };
            int[] indices = new int[] {
                indexOffset + i * 3 + 0,
                indexOffset + i * 3 + 1,
                indexOffset + i * 3 + 2
            };
            Vector3[] triangleNormals = new Vector3[]
            {
                normals[points.FindIndex((Vector3m element) => element == earClipping.Result[i * 3 + 0])],
                normals[points.FindIndex((Vector3m element) => element == earClipping.Result[i * 3 + 1])],
                normals[points.FindIndex((Vector3m element) => element == earClipping.Result[i * 3 + 2])]
            };
            triangles[i] = new Triangle(trianglePoints, indices, triangleNormals);
        }

        return triangles.ToArray();
    }


    private static Vector3[] GetPolygonPoints(FbxMesh fbxMesh, int polygonIndex)
    {
        Vector3[] polygonPoints = new Vector3[fbxMesh.GetPolygonSize(polygonIndex)];
        for (int i = 0; i < fbxMesh.GetPolygonSize(polygonIndex); i++)
            polygonPoints[i] = ToUnityVector3(fbxMesh.GetControlPointAt(fbxMesh.GetPolygonVertex(polygonIndex, i)));
        return polygonPoints;
    }

    private static Vector3[] GetPolygonNormals(FbxMesh fbxMesh, int polygonIndex)
    {
        Vector3[] polygonNormals = new Vector3[fbxMesh.GetPolygonSize(polygonIndex)];
        for (int i = 0; i < fbxMesh.GetPolygonSize(polygonIndex); i++)
        {
            FbxVector4 fbxNormal;
            fbxMesh.GetPolygonVertexNormal(polygonIndex, i, out fbxNormal);
            polygonNormals[i] = ToUnityVector3(fbxNormal);
        }
        return polygonNormals;
    }

    private static Vector3 ToUnityVector3(FbxVector4 fbxVector4)
    {
        return new Vector3((float)fbxVector4[0], (float)fbxVector4[1], (float)fbxVector4[2]);
    }
}
