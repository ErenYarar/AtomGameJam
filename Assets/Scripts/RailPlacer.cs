using UnityEngine;
using UnityEngine.Splines;

public class RailBuilder : MonoBehaviour
{
    public SplineContainer mainSpline;
    public GameObject[] railPrefabs;

    private int currentIndex = 0;
    private GameObject ghostInstance;
    public Material ghostMaterial;

    void Start()
    {
        // Oyun başlarken ilk rayı seçili yap
        SpawnGhost();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            CyclePrefab();
        }

        if (Input.GetMouseButtonDown(0) && ghostInstance != null)
        {
            PlaceRail();
            SpawnGhost(); // Ray ekledikten sonra yeni ghost gelsin
        }
    }

    void CyclePrefab()
    {
        currentIndex = (currentIndex + 1) % railPrefabs.Length;
        if (ghostInstance != null) Destroy(ghostInstance);
        SpawnGhost();
    }

    void SpawnGhost()
    {
        ghostInstance = Instantiate(railPrefabs[currentIndex]);

        // Ghost'un materyalini ghostMaterial ile değiştir
        var renderers = ghostInstance.GetComponentsInChildren<MeshRenderer>();
        foreach (var r in renderers)
        {
            r.material = ghostMaterial;
        }

        PositionGhostAtEnd();
    }

    void PositionGhostAtEnd()
    {
        var main = mainSpline.Spline;
        int lastIndex = main.Count - 1;
        BezierKnot lastKnot = main[lastIndex];

        Vector3 lastPosWorld = mainSpline.transform.TransformPoint(lastKnot.Position);

        var ghostSpline = ghostInstance.GetComponent<SplineContainer>().Spline;
        Vector3 firstKnotWorld = ghostInstance.transform.TransformPoint(ghostSpline[0].Position);

        Vector3 delta = lastPosWorld - firstKnotWorld;
        ghostInstance.transform.position += delta;
    }

    void PlaceRail()
    {
        var main = mainSpline.Spline;
        var ghostSpline = ghostInstance.GetComponent<SplineContainer>().Spline;

        int lastIndex = main.Count - 1;
        BezierKnot lastKnot = main[lastIndex];
        Vector3 lastPosWorld = mainSpline.transform.TransformPoint(lastKnot.Position);

        Vector3 firstKnotWorld = ghostInstance.transform.TransformPoint(ghostSpline[0].Position);
        Vector3 delta = lastPosWorld - firstKnotWorld;

        for (int i = 1; i < ghostSpline.Count; i++)
        {
            BezierKnot knot = ghostSpline[i];
            Vector3 knotWorld = ghostInstance.transform.TransformPoint(knot.Position);
            knotWorld += delta;
            knot.Position = mainSpline.transform.InverseTransformPoint(knotWorld);
            main.Add(knot);
        }

        Destroy(ghostInstance);
        ghostInstance = null;
    }
}
