using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Autodesk.Fbx;
using EarClipperLib;
using System.Linq;
using System.Runtime.InteropServices;
using System;
using System.IO;

public static class FbxToGameObjectConverter
{
    private class Triangle
    {
        public Vector3[] points;
        public int[] indices;
        public Vector3[] normals;
        public Vector2[] uvs;

        public Triangle(Vector3[] points, int[] indices, Vector3[] normals, Vector2[] uvs)
        {
            this.points = points;
            this.indices = indices;
            this.normals = normals;
            this.uvs = uvs;
        }
    }

    public static GameObject Convert(string filename, Material material, string texturesSearchDirectory)
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

                return GameObjectFromFbxScene(scene, material, texturesSearchDirectory);
            }
        }
    }
    
    private static GameObject GameObjectFromFbxScene(FbxScene fbxScene, Material material, string texturesSearchDirectory)
    {
        var rootNode = fbxScene.GetRootNode();
        GameObject model = ModelPartFromFbxNode(rootNode, material, texturesSearchDirectory, isRoot: true);

        return model;
    }

    private static GameObject ModelPartFromFbxNode(FbxNode fbxNode, Material defaultMaterial, string texturesSearchDirectory, bool isRoot = false)
    {
        FbxMesh fbxMesh = fbxNode.GetMesh();

        if (fbxMesh == null && fbxNode.GetChildCount() == 0)
            return null;

        GameObject modelPart = new GameObject(fbxNode.GetName());

        if (!isRoot)
        {
            var fbxPosition = fbxNode.LclTranslation.GetFbxDouble3();
            modelPart.transform.localPosition = new Vector3((float)fbxPosition.X, (float)fbxPosition.Y, (float)fbxPosition.Z);
        }
        var fbxRotation = fbxNode.LclRotation.GetFbxDouble3();
        modelPart.transform.rotation = Quaternion.Euler(new Vector3((float)fbxRotation.X, (float)fbxRotation.Y, (float)fbxRotation.Z));
        var fbxScale = fbxNode.LclScaling.GetFbxDouble3();
        modelPart.transform.localScale = new Vector3((float)fbxScale.X, (float)fbxScale.Y, (float)fbxScale.Z);

        for (int i = 0; i < fbxNode.GetChildCount(); i++)
        {
            GameObject childPart = ModelPartFromFbxNode(fbxNode.GetChild(i), defaultMaterial, texturesSearchDirectory);
            if(childPart != null)
                childPart.transform.parent = modelPart.transform;
        }

        if (fbxMesh != null)
        {
            MeshFilter meshFilter = modelPart.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = modelPart.AddComponent<MeshRenderer>();
            try
            {
                meshFilter.mesh = GetUnityMesh(fbxNode);
                // C# version of Fbx SDK doesn't allow for reading submeshes info and multiple materials,
                // so we are forced to use only one material for entire mesh
                meshRenderer.material = GetUnityMaterial(fbxNode, defaultMaterial, texturesSearchDirectory);
            }
            catch { }

        }

        return modelPart;
    }

    private static Mesh GetUnityMesh(FbxNode node)
    {
        FbxMesh fbxMesh = node.GetMesh();

        Mesh unityMesh = new Mesh();
        List<Vector3> unityVerts = new List<Vector3>();
        List<int> unityTriInds = new List<int>();
        List<Vector3> unityNormals = new List<Vector3>();
        List<Vector2> unityUvs = new List<Vector2>();

        int polyOffset = 0;
        int indexOffset = 0;
        for (int pInd = 0; pInd < fbxMesh.GetPolygonCount(); pInd++)
        {
            Vector3[] polygonPoints = GetPolygonPoints(fbxMesh, pInd);
            Vector3[] polygonNormals = GetPolygonNormals(fbxMesh, pInd);
            Vector2[] polygonUvs = GetPolygonUvs(fbxMesh, pInd, polyOffset);

            Triangle[] triangles = Triangulate(polygonPoints, polygonNormals, polygonUvs, indexOffset);

            foreach (Triangle triangle in triangles)
            {
                unityVerts.AddRange(triangle.points);
                unityTriInds.AddRange(triangle.indices);
                unityNormals.AddRange(triangle.normals);
                if(triangle.uvs != null)
                    unityUvs.AddRange(triangle.uvs);
            }

            indexOffset += triangles.Length * 3;
            polyOffset += fbxMesh.GetPolygonSize(pInd);
        }

        unityMesh.SetVertices(unityVerts);
        unityMesh.SetTriangles(unityTriInds, 0);
        unityMesh.SetNormals(unityNormals);
        unityMesh.SetUVs(0, unityUvs);

        return unityMesh;
    }

    private static Material GetUnityMaterial(FbxNode node, Material defaultMaterial, string texturesSearchDirectory)
    {
        Material unityMaterial = new Material(defaultMaterial);
        FbxSurfacePhong fbxMaterial = node.GetMaterial(0) == null ? null : CastTo<FbxSurfacePhong>(node.GetMaterial(0), true);

        if (fbxMaterial == null)
            return unityMaterial;

        MapFbxMatToUnityMat(fbxMaterial, unityMaterial, texturesSearchDirectory);

        return unityMaterial;
    }

    private static void MapFbxMatToUnityMat(FbxSurfacePhong fbxMaterial, Material unityMaterial, string texturesSearchDirectory)
    {
        var diffuseProp = fbxMaterial.FindProperty("DiffuseColor");
        var specularProp = fbxMaterial.FindProperty("SpecularColor");
        if (diffuseProp.IsValid() && diffuseProp.GetPropertyDataType().ToEnum() == EFbxType.eFbxDouble3)
        {
            unityMaterial.color = ToUnityColor(diffuseProp.GetFbxColor());
            unityMaterial.mainTexture = GetTextureFromProperty(diffuseProp, texturesSearchDirectory);
        }
        else if (specularProp.IsValid() && specularProp.GetPropertyDataType().ToEnum() == EFbxType.eFbxDouble3)
        {
            unityMaterial.color = ToUnityColor(specularProp.GetFbxColor());
            unityMaterial.mainTexture = GetTextureFromProperty(specularProp, texturesSearchDirectory);
        }
        var reflectionProp = fbxMaterial.FindProperty("ReflectionFactor");
        if (reflectionProp.IsValid() && fbxMaterial.Reflection.GetPropertyDataType().ToEnum() == EFbxType.eFbxFloat)
            unityMaterial.SetFloat("_Metallic", reflectionProp.GetFloat());
    }

    private static void PrintProperties_debug(FbxObject fbxObject)
    {
        List<string> debugString = new List<string>();
        FbxProperty matProp = fbxObject.GetFirstProperty();
        while (matProp.IsValid())
        {
            debugString.Add(matProp.GetName() + ": " + matProp.GetSrcObjectCount());
            matProp = fbxObject.GetNextProperty(matProp);
        }
        Debug.Log(string.Join(", ", debugString));
    }

    private static Texture2D GetTextureFromProperty(FbxProperty property, string texturesSearchDirectory, bool linear = false)
    {
        if (property.GetSrcObjectCount() > 0)
        {
            FbxFileTexture fileTexture = CastTo<FbxFileTexture>(property.GetSrcObject(0), false);

            string texturePath = fileTexture.GetFileName();
            string textureFilename = Path.GetFileName(texturePath);

            Texture2D TextureFromFile(string filename)
            {
                byte[] textureData = File.ReadAllBytes(filename);
                Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, true, linear);
                texture.LoadImage(textureData);
                return texture;
            }
            
            if (File.Exists(texturePath))
                return TextureFromFile(texturePath);
            else
            {
                if (!Directory.Exists(texturesSearchDirectory))
                    Directory.CreateDirectory(texturesSearchDirectory);
                var pngFiles = Directory.GetFiles(texturesSearchDirectory, "*.png", SearchOption.AllDirectories);
                var jpgFiles = Directory.GetFiles(texturesSearchDirectory, "*.jpg", SearchOption.AllDirectories);
                var files = pngFiles.Concat(jpgFiles).ToArray();
                var foundFiles = files.Where((string file) => Path.GetFileName(file) == textureFilename).ToArray();
                if (foundFiles.Count() > 0)
                    return TextureFromFile(foundFiles.ElementAt(0));
            }
        }

        return null;
    }

    private static Triangle[] Triangulate(Vector3[] polygon, Vector3[] normals, Vector2[] uvs, int indexOffset)
    {
        Triangle[] triangles;

        if (polygon.Length == 3)
            triangles = new Triangle[] { new Triangle(polygon, new int[] { indexOffset + 0, indexOffset + 1,  indexOffset + 2 }, normals, uvs) };
        else
        {
            List<Vector3m> points = polygon.Select((Vector3 input) => new Vector3m(input.x, input.y, input.z)).ToList();

            Vector3 FromVector3m(Vector3m vector3m) => new Vector3(
                (float)vector3m.X.GetSignedDouble(),
                (float)vector3m.Y.GetSignedDouble(),
                (float)vector3m.Z.GetSignedDouble()
            );

            EarClipping earClipping = new EarClipping();
            earClipping.SetPoints(points);
            earClipping.Triangulate();
            triangles = new Triangle[earClipping.Result.Count / 3];

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
                Vector2[] triangleUvs = uvs == null ? null : new Vector2[]
                {
                    uvs[points.FindIndex((Vector3m element) => element == earClipping.Result[i * 3 + 0])],
                    uvs[points.FindIndex((Vector3m element) => element == earClipping.Result[i * 3 + 1])],
                    uvs[points.FindIndex((Vector3m element) => element == earClipping.Result[i * 3 + 2])]
                };

                triangles[i] = new Triangle(trianglePoints, indices, triangleNormals, triangleUvs);
            }
        }

        return triangles;
    }

    private static Vector2[] GetPolygonUvs(FbxMesh fbxMesh, int pInd, int polyOffset)
    {
        var uvElement = fbxMesh.GetLayer(0).GetUVs(FbxLayerElement.EType.eTextureDiffuse);
        if (uvElement == null)
            return null;
        Vector2[] polygonUvs = new Vector2[fbxMesh.GetPolygonSize(pInd)];
        for (int i = 0; i < polygonUvs.Length; i++)
        {
            bool byCtrlPoint = uvElement.GetMappingMode() == FbxLayerElement.EMappingMode.eByControlPoint;
            int polyVert = fbxMesh.GetPolygonVertex(pInd, i);
            int uvInd = byCtrlPoint ? polyVert : polyOffset + i;
            var fbxVector2 = uvElement.GetDirectArray().GetAt(uvElement.GetIndexArray().GetAt(uvInd));
            polygonUvs[i] = new Vector2((float)fbxVector2.X, (float)fbxVector2.Y);
        }
        return polygonUvs;
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

    private static Color ToUnityColor(FbxColor fbxColor)
    {
        return new Color((float)fbxColor.mRed, (float)fbxColor.mGreen, (float)fbxColor.mBlue, (float)fbxColor.mAlpha);
    }

    private static T CastTo<T>(FbxObject from, bool cMemoryOwn)
    {
        System.Reflection.MethodInfo CPtrGetter = typeof(FbxEmitter).GetMethod("getCPtr", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        return (T) System.Activator.CreateInstance
        (
            typeof(T),
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
            null,
            new object[] { ((HandleRef) CPtrGetter.Invoke(null, new object[] { from })).Handle, cMemoryOwn },
            null
        );
    }

}
