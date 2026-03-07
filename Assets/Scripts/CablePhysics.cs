using UnityEngine;
using System.Collections.Generic;

public class CablePhysics : MonoBehaviour
{
    public LaunchHand launchHand;
    public CableManager cablemanager;
    public Rigidbody playerRb;
    public Transform startTransform;
    public Transform endTransform;
    public LineRenderer lineRenderer;
    public LayerMask collisionLayer;

    public int visualSegmentsPerSection = 8;
    public float surfaceOffset = 0.015f;

    private List<Vector3> ropePoints = new List<Vector3>();

    private HashSet<PowerPole> polesTouchedThisFrame = new HashSet<PowerPole>();


    public bool isActive = false;

    public float cableRadius = 0.05f;
    public int radialSegments = 8;

    private Mesh cableMesh;
    private MeshFilter meshFilter;

    public RotateArms aimOverride;

    public bool controlsRightArm = false;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();

        if (meshFilter == null)
            meshFilter = gameObject.AddComponent<MeshFilter>();

        if (GetComponent<MeshRenderer>() == null)
            gameObject.AddComponent<MeshRenderer>();

        cableMesh = new Mesh();
        cableMesh.name = "CableMesh";
        meshFilter.mesh = cableMesh;


        InitializeCable();
    }

    void LateUpdate()
    {


        if (!isActive)
        {
            if (cableMesh.vertexCount > 0)
                cableMesh.Clear();
            return;
        }

        if (aimOverride != null)
        {

            Vector3 targetPoint;

            if (ropePoints.Count > 1)
            {
                targetPoint = ropePoints[1];
            }
            else
            {
                targetPoint = endTransform.position;
            }

            if (controlsRightArm)
                aimOverride.rightHitPoint = targetPoint;
            else
                aimOverride.leftHitPoint = targetPoint;


            if (controlsRightArm)
            {
                Debug.DrawLine(startTransform.position, targetPoint, Color.red);
            }
            else
            {
                Debug.DrawLine(startTransform.position, targetPoint, Color.blue);
            }
        }

        polesTouchedThisFrame.Clear();

        if (ropePoints.Count < 2) return;

        ropePoints[0] = startTransform.position;
        ropePoints[ropePoints.Count - 1] = endTransform.position;

        HandleWrapping();
        HandleUnwrapping();
        DetectPoleContacts();
        RenderRope();
        AdjustScale();
    }

    public void InitializeCable()
    {
        ropePoints.Clear();
        ropePoints.Add(startTransform.position);
        ropePoints.Add(endTransform.position);
    }

    void HandleWrapping()
    {
        float ropeRadius = cableRadius * 2.5f;
        for (int i = 0; i < ropePoints.Count - 1; i++)
        {
            Vector3 from = ropePoints[i];
            Vector3 to = ropePoints[i + 1];

            Vector3 dir = (to - from);
            float dist = dir.magnitude;

            if (dist <= 0.001f)
                continue;

            dir.Normalize();





            if (Physics.SphereCast(from, ropeRadius, dir, out RaycastHit hit, dist, collisionLayer))
            {
                Vector3 wrapPoint = hit.point + hit.normal * surfaceOffset;

                if (Vector3.Distance(wrapPoint, from) > 0.05f &&
                    Vector3.Distance(wrapPoint, to) > 0.05f)
                {


                    ropePoints.Insert(i + 1, wrapPoint);

                    PowerPole pole = hit.collider.GetComponent<PowerPole>();
                    if (pole != null)
                        pole.Startglow();


                    return; 
                }
            }
        }
    }

    void HandleUnwrapping()
    {
        if (ropePoints.Count <= 2)
            return;

        for (int i = 1; i < ropePoints.Count - 1; i++)
        {
            Vector3 prev = ropePoints[i - 1];
            Vector3 next = ropePoints[i + 1];
            float ropeRadius = 0.1f;
            Vector3 dir = (next - prev);
            float dist = dir.magnitude;
            dir.Normalize();
            if (!Physics.Linecast(prev, next, collisionLayer))
            {
                ropePoints.RemoveAt(i);
                break;
            }
        }
    }

    void DetectPoleContacts()
    {
        float ropeRadius = 0.05f;

        for (int i = 0; i < ropePoints.Count - 1; i++)
        {
            Vector3 from = ropePoints[i];
            Vector3 to = ropePoints[i + 1];

            Vector3 dir = (to - from);
            float dist = dir.magnitude;

            if (dist <= 0.001f)
                continue;

            dir.Normalize();

            RaycastHit[] hits = Physics.SphereCastAll(from, ropeRadius, dir, dist, collisionLayer);

            foreach (var hit in hits)
            {
                PowerPole pole = hit.collider.GetComponent<PowerPole>();
                if (pole != null)
                {
                    polesTouchedThisFrame.Add(pole);
                }
            }
        }

        PowerPole[] allPoles = FindObjectsOfType<PowerPole>();

        foreach (PowerPole pole in allPoles)
        {
            if (polesTouchedThisFrame.Contains(pole))
                pole.Startglow();
        }
    }


    void RenderRope()
    {
        List<Vector3> finalPoints = new List<Vector3>();

        for (int i = 0; i < ropePoints.Count - 1; i++)
        {
            Vector3 from = ropePoints[i];
            Vector3 to = ropePoints[i + 1];

            for (int j = 0; j < visualSegmentsPerSection; j++)
            {
                float t = j / (float)visualSegmentsPerSection;
                finalPoints.Add(Vector3.Lerp(from, to, t));
            }
        }

        finalPoints.Add(ropePoints[ropePoints.Count - 1]);

        GenerateTubeMesh(finalPoints);
    }

    public List<Vector3> GetCablePoints()
    {
        return ropePoints;
    }

    public void RemoveLastWrapPoint()
    {
        if (ropePoints.Count > 2)
            ropePoints.RemoveAt(ropePoints.Count - 2);
    }

    void GenerateTubeMesh(List<Vector3> points)
    {
        if (points.Count < 2) return;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        for (int i = 0; i < points.Count; i++)
        {
            Vector3 forward;

            if (i == points.Count - 1)
                forward = points[i] - points[i - 1];
            else
                forward = points[i + 1] - points[i];

            forward.Normalize();

            Vector3 right = Vector3.Cross(forward, Vector3.up);
            if (right == Vector3.zero)
                right = Vector3.Cross(forward, Vector3.forward);

            right.Normalize();
            Vector3 up = Vector3.Cross(forward, right);

            for (int j = 0; j < radialSegments; j++)
            {
                float angle = (j / (float)radialSegments) * Mathf.PI * 2f;
                Vector3 localPoint = transform.InverseTransformPoint(points[i]);

                Vector3 localRight = transform.InverseTransformDirection(right);
                Vector3 localUp = transform.InverseTransformDirection(up);

                Vector3 localOffset =
                    Mathf.Cos(angle) * localRight * cableRadius +
                    Mathf.Sin(angle) * localUp * cableRadius;

                vertices.Add(localPoint + localOffset);
            }
        }

        for (int i = 0; i < points.Count - 1; i++)
        {
            for (int j = 0; j < radialSegments; j++)
            {
                int current = i * radialSegments + j;
                int next = current + radialSegments;

                int nextSegment = i * radialSegments + (j + 1) % radialSegments;
                int nextSegmentNext = nextSegment + radialSegments;

                triangles.Add(current);
                triangles.Add(nextSegment);
                triangles.Add(next);

                triangles.Add(nextSegment);
                triangles.Add(nextSegmentNext);
                triangles.Add(next);


            }
        }

        cableMesh.Clear();
        cableMesh.SetVertices(vertices);
        cableMesh.SetTriangles(triangles, 0);
        cableMesh.RecalculateNormals();
    }

    void AdjustScale()
    {
        Vector3 parentScale = transform.parent ? transform.parent.lossyScale : Vector3.one;

        transform.localScale = new Vector3(
            1f / parentScale.x,
            1f / parentScale.y,
            1f / parentScale.z
        );
    }

    public float GetCableLength()
    {
        float total = 0f;

        for (int i = 0; i < ropePoints.Count - 1; i++)
        {
            total += Vector3.Distance(ropePoints[i], ropePoints[i + 1]);
        }

        return total;
    }

    public void ApplySharedTension(float excess)
    {
        if (launchHand == null || launchHand.CanReturn)
            return;

        if (playerRb == null)
            return;

        Vector3 nextPoint = ropePoints[1];

        Vector3 playerFlat = new Vector3(startTransform.position.x, 0f, startTransform.position.z);
        Vector3 targetFlat = new Vector3(nextPoint.x, 0f, nextPoint.z);

        Vector3 flatDir = (targetFlat - playerFlat);

        if (flatDir.magnitude <= 0.001f)
            return;

        flatDir.Normalize();

        float stiffness = 20;
        float damping = 1f;

        Vector3 force = flatDir * excess * stiffness;
        force.y = 0f;

        Vector3 velocityFlat = Vector3.ProjectOnPlane(playerRb.velocity, Vector3.up);
        Vector3 velocityAlongFlat = Vector3.Project(velocityFlat, flatDir);

        force -= velocityAlongFlat * damping;

        playerRb.AddForce(force, ForceMode.Acceleration);
    }
}