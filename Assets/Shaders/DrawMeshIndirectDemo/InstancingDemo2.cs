using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InstancingDemo2 : MonoBehaviour
{
    public GameObject sourceTerrain;
    private Mesh sourceMesh;
    public Mesh instancedMesh;
    public Material instancedMaterial;

    private ComputeBuffer meshPropertiesBuffer;
    private ComputeBuffer argsBuffer;

    private Bounds bounds;


    // Mesh Properties struct to be read from the GPU.
    // Size() is a convenience funciton which returns the stride of the struct.
    private struct MeshProperties
    {
        //eventually, you should switch the mat4 with 3 float4s so that the matrix calculation happens in the gpu
        public Matrix4x4 mat;
        public Vector4 color;

        public static int Size()
        {
            return
                sizeof(float) * 4 * 4 + // matrix;
                sizeof(float) * 4;      // color;
        }
    }

    private void Setup()
    {
        //Update bounds to be based on source mesh
        //  -   (Boundary surrounding the meshes we will be drawing.  Used for occlusion.)
        //  -   Expand by grass height .. ? (inspired by nedmakesgames grass tutorial)
        bounds = sourceMesh.bounds;
        //bounds.Expand(3);

        //Initialize buffers to be passed to gpu
        InitializeBuffers();
    }

    private void InitializeBuffers()
    {
        int numSourceTriangles = sourceMesh.triangles.Length / 3;
        Debug.Log(numSourceTriangles);
        // Argument buffer used by DrawMeshInstancedIndirect.
        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };

        // Arguments for drawing mesh.
        // 0 == number of triangle indices, 1 == population, others are only relevant if drawing submeshes.
        args[0] = (uint)instancedMesh.GetIndexCount(0);
        args[1] = (uint)numSourceTriangles;
        args[2] = (uint)instancedMesh.GetIndexStart(0);
        args[3] = (uint)instancedMesh.GetBaseVertex(0);
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);

        // Initialize buffer with the given population.
        MeshProperties[] properties = new MeshProperties[numSourceTriangles];

        for (int i = 0; i < numSourceTriangles; i++)
        {
            MeshProperties props = new MeshProperties();
            ////Read triangle number
            Vector3 triangleVert1 = sourceMesh.vertices[sourceMesh.triangles[i*3]];
            Vector3 triangleVert2 = sourceMesh.vertices[sourceMesh.triangles[i*3 + 1]];
            Vector3 triangleVert3 = sourceMesh.vertices[sourceMesh.triangles[i*3 + 2]];
            Vector3 position = triangleVert1 + triangleVert2 + triangleVert3;
            position /= 3.0f;
            //Vector3 position = new Vector3(i * 10.0f, 0, 0);
            position = sourceTerrain.transform.localToWorldMatrix * position;

            Quaternion rotation = Quaternion.identity;
            Vector3 scale = Vector3.one * 4f;

            props.mat = Matrix4x4.TRS(position, rotation, scale);
            props.color = Color.Lerp(Color.red, Color.blue, Random.value);
            properties[i] = props;
        }

        meshPropertiesBuffer = new ComputeBuffer((int)sourceMesh.GetIndexCount(0), MeshProperties.Size());
        meshPropertiesBuffer.SetData(properties);
        instancedMaterial.SetBuffer("_Properties", meshPropertiesBuffer);
    }
        // Start is called before the first frame update
        void Start()
    {
        sourceMesh = sourceTerrain.GetComponent<MeshFilter>().sharedMesh;
        Debug.Log("sourceMesh assigned");
        Setup();
    }

    // Update is called once per frame
    void Update()
    {
        Graphics.DrawMeshInstancedIndirect(instancedMesh, 0, instancedMaterial, bounds, argsBuffer);
    }

    private void OnDisable()
    {
        if (meshPropertiesBuffer != null)
        {
            meshPropertiesBuffer.Release();
        }
        meshPropertiesBuffer = null;

        if (argsBuffer != null)
        {
            argsBuffer.Release();
        }
        argsBuffer = null;
    }
}
