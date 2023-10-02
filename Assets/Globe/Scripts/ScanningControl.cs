using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;


public class ScanningControl : MonoBehaviour {

	[SerializeField] private GameObject indicatorPrefab;
	[SerializeField] private GameObject nextButtonObject;
	[SerializeField] private GameObject confirmButton;
	[SerializeField] private GameObject planePrefab;
	[SerializeField] private GraphicRaycaster GR;
	[SerializeField] private GameObject canvas;

	public LayerMask mask;

	[HideInInspector] public GameObject currentIndicator;
	//[HideInInspector] public GameObject mapParent;

	private GameObject stretchedPlane;
	private bool stageTwo = false;
	private GameObject currentPlane;

	private List<GameObject> chatBubbles = new List<GameObject>();

	public delegate void ScanFinalStep(Vector3 position);
	public static event ScanFinalStep finalStep;



	// Update is called once per frame
	void Update () {

		Ray ray = LevelManagerGlobe.Instance.cameraRef.ViewportPointToRay (new Vector3(0.5f,0.5f,0f));

		RaycastHit hit;

		if (Physics.Raycast (ray, out hit, 1000, mask)) {

			if (currentIndicator == null) {

				currentPlane = hit.collider.gameObject;

				if (stretchedPlane == null) {
					//next ();
					Debug.LogWarning("TODO: NEXT?");
				}

				currentIndicator = Instantiate (indicatorPrefab);
				nextButtonObject.SetActive (!stageTwo);
				confirmButton.SetActive (stageTwo);
			}
				
			Vector3 position = hit.point;
			position.y += 0.001f;
			currentIndicator.transform.position = position;
			//currentIndicator.transform.rotation = Quaternion.Euler(0f, Camera.main.transform.rotation.eulerAngles.y, 0f);
			currentIndicator.transform.Rotate (Vector3.up * Time.deltaTime*45);

		} else {
			if (currentIndicator != null) {
				nextButtonObject.SetActive (false);
				confirmButton.SetActive (false);
				Destroy (currentIndicator);
				currentIndicator = null;
			}
		}
	}

	public void next()
	{
		Vector3 position = currentPlane.transform.position;

		stretchedPlane = Instantiate(planePrefab);

		stretchedPlane.transform.position = position;
		stretchedPlane.transform.localScale = new Vector3(50f, 1f, 50f);
		nextButtonObject.SetActive(false);
		confirmButton.SetActive(true);
	}

	public void confirm() {

		if (finalStep != null) {
			finalStep (currentIndicator.transform.position);
		}
	}

	IEnumerator newBubble() {

		yield return new WaitForSeconds (2);
	}

	public void disable() {
		enabled = false;

		stageTwo = false;
		nextButtonObject.GetComponentInChildren<Text>().text = "Next";

		if (stretchedPlane != null) {
			Destroy (stretchedPlane);
			stretchedPlane = null;
		}

		//Destroy(chatBubbles.Last ());
		chatBubbles.Clear ();

		nextButtonObject.SetActive (false);
		confirmButton.SetActive (false);

		if (currentIndicator != null) {
			Destroy (currentIndicator);
			currentIndicator = null;
		}
	}
}