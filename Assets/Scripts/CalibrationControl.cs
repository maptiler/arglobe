using UnityEngine;
using UnityEngine.XR.ARFoundation;


public class CalibrationControl : MonoBehaviour
{
    [SerializeField]
    ARSession arSessionRef;
    
    [SerializeField]
    ARPlaneManager arPlaneManagerRef;

    [SerializeField]
    ARPointCloudManager arPointCloudManagerRef;

    [SerializeField]
    LayerMask layerMask;

    [SerializeField]
    GameObject indicatorPrefab, infinitePlanePrefab;


    GameObject currentIndicator, currentInfinitePlane;

    public bool IsPlaced { get; private set; }


    void Update()
    {
        CalibrationRaycast();
    }

    private void OnEnable()
    {
        LevelManagerGlobe.Instance.MapParent.SetActive(false);
        currentIndicator = null;
        currentInfinitePlane = null;
        arPlaneManagerRef.enabled = true;
        arPointCloudManagerRef.enabled = true;
        arPlaneManagerRef.SetTrackablesActive(true);
        arPointCloudManagerRef.SetTrackablesActive(true);
        arSessionRef.Reset();
    }

    private void OnDisable()
    {
        LevelManagerGlobe.Instance.MapParent.SetActive(true);
        Destroy(currentIndicator);
        Destroy(currentInfinitePlane);
        currentIndicator = null;
        currentInfinitePlane = null;
        arPlaneManagerRef.enabled = false;
        arPointCloudManagerRef.enabled = false;
        arPlaneManagerRef.SetTrackablesActive(false);
        arPointCloudManagerRef.SetTrackablesActive(false);
    }

    void CalibrationRaycast()
    {
        Ray ray = LevelManagerGlobe.Instance.cameraRef.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 1000, layerMask))
        {
            Vector3 hitPos = hit.point;

            if (currentInfinitePlane == null)
            {
                currentInfinitePlane = Instantiate(infinitePlanePrefab);
                currentInfinitePlane.transform.position = new Vector3(0, hitPos.y, 0);
                currentInfinitePlane.transform.localScale = new Vector3(50f, 1f, 50f);
                arPlaneManagerRef.enabled = false;
                arPointCloudManagerRef.enabled = false;
                arPlaneManagerRef.SetTrackablesActive(false);
                arPointCloudManagerRef.SetTrackablesActive(false);
            }

            if (currentIndicator == null)
            {
                currentIndicator = Instantiate(indicatorPrefab);
            }

            hitPos.y += 0.001f;
            currentIndicator.transform.position = hitPos;
            currentIndicator.transform.Rotate(Vector3.up * Time.deltaTime * 45);
        }
        else
        {
            if (currentIndicator != null)
            {
                Destroy(currentIndicator);
                currentIndicator = null;
            }
        }
    }

    public bool HasGround()
    {
        //Debug.Log("Has ground " + currentInfinitePlane != null);
        return currentInfinitePlane != null;
    }

    public void ConfirmCalibration()
    {
        LevelManagerGlobe.Instance.MapParent.transform.parent.position =
            new Vector3(
                currentIndicator.transform.position.x,
                LevelManagerGlobe.Instance.cameraRef.transform.position.y,
                currentIndicator.transform.position.z); //currentIndicator.transform.position + (Vector3.up * 1.25f);
        IsPlaced = true;
        enabled = false;
    }
}