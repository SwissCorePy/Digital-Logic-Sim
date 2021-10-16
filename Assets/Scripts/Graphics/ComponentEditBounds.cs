using UnityEngine;

namespace Graphics
{
[ExecuteInEditMode]
public class ComponentEditBounds : MonoBehaviour
{
    public float thickness = 0.1f;
    public Material material;
    public Transform inputSignalArea;
    public Transform outputSignalArea;

    private Mesh _quadMesh;
    private Matrix4x4[] _trs;

    private void Start()
    {
        if (Application.isPlaying)
        {
            MeshShapeCreator.CreateQuadMesh(ref _quadMesh);
            CreateMatrices();
        }
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            MeshShapeCreator.CreateQuadMesh(ref _quadMesh);
            CreateMatrices();
            UpdateSignalAreaSizeAndPos(inputSignalArea);
            UpdateSignalAreaSizeAndPos(outputSignalArea);
        }

        for (var i = 0; i < 4; i++) UnityEngine.Graphics.DrawMesh(_quadMesh, _trs[i], material, 0);
    }

    private void UpdateSignalAreaSizeAndPos(Transform signalArea)
    {
        var transform1 = transform;
        var position = signalArea.position;

        position = new Vector3(position.x, transform1.position.y, position.z);
        signalArea.position = position;
        signalArea.localScale = new Vector3(signalArea.localScale.x, transform1.localScale.y, 1);
    }

    private void CreateMatrices()
    {
        var transform1 = transform;
        var centre = transform1.position;

        var width = Mathf.Abs(transform1.localScale.x);
        var height = Mathf.Abs(transform.localScale.y);

        Vector3[] edgeCentres =
        {
            centre + Vector3.left * width / 2,
            centre + Vector3.right * width / 2,
            centre + Vector3.up * height / 2,
            centre + Vector3.down * height / 2
        };

        Vector3[] edgeScales =
        {
            new Vector3(thickness, height + thickness, 1),
            new Vector3(thickness, height + thickness, 1),
            new Vector3(width + thickness, thickness, 1),
            new Vector3(width + thickness, thickness, 1)
        };

        _trs = new Matrix4x4[4];
        for (var i = 0; i < 4; i++) _trs[i] = Matrix4x4.TRS(edgeCentres[i], Quaternion.identity, edgeScales[i]);
    }
}
}