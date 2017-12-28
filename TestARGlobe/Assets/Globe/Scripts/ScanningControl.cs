using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.iOS;
using UnityEngine.EventSystems;
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
	[HideInInspector] public GameObject mapParent;

	private GameObject stretchedPlane;
	private  UnityARGeneratePlane planeScript;
	private PointCloudParticleExample particleScript;
	private bool stageTwo = false;
	private GameObject currentPlane;

	private GameObject chatBubble;
	private List<GameObject> chatBubbles = new List<GameObject>();

	public delegate void ScanFinalStep(Vector3 position);
	public static event ScanFinalStep finalStep;

	// Use this for initialization
	void Start () {
		
	}

	// Update is called once per frame
	void Update () {

		Ray ray = Camera.main.ViewportPointToRay (new Vector3(0.5f,0.5f,0f));

		RaycastHit hit;

		if (Physics.Raycast (ray, out hit, 1000, mask)) {

			if (currentIndicator == null) {

				currentPlane = hit.collider.gameObject;

				if (stretchedPlane == null) {
					next ();
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

	public void next() {

		Handheld.Vibrate ();
		stageTwo = true;
		particleScript.disable ();
		planeScript.disable ();

		Vector3 position = currentPlane.transform.position;

		stretchedPlane = Instantiate (planePrefab);

		stretchedPlane.transform.position = position;
		stretchedPlane.transform.localScale = new Vector3 (50f, 1f, 50f);
		nextButtonObject.SetActive (false);
		confirmButton.SetActive (true);

		Destroy(chatBubbles.Last ());
		chatBubbles.Clear ();

		GameObject bubble = ChatBubble.createChatBubble (canvas, "Good job. You can now place the globe\nby pressing the confirm button. Globe will be\nplaced at the center of the circle indicator.");

		StartCoroutine(ChatBubble.moveBuble (bubble, chatBubbles, () => {
			
		}));

		chatBubbles.Add (bubble);
	}

	public void confirm() {

		if (finalStep != null) {
			finalStep (currentIndicator.transform.position);
		}
	}

	public void enable() {

		if (planeScript == null) {
			planeScript = gameObject.GetComponent<UnityARGeneratePlane> ();
			particleScript = gameObject.GetComponent<PointCloudParticleExample> ();
		}

		particleScript.enable ();
		planeScript.enable ();

		GameObject bubble = ChatBubble.createChatBubble (canvas, "First we need to scan your room. Aim your camera at the floor.\n You should see yellow dots floating around.");

		StartCoroutine(ChatBubble.moveBuble (bubble, chatBubbles, () => {
			
		}));

		chatBubbles.Add (bubble);

		//targetImage.SetActive (true);
		//screenTextPanel.SetActive (true);

		enabled = true;
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

		Destroy(chatBubbles.Last ());
		chatBubbles.Clear ();

		planeScript.disable ();
		particleScript.disable ();
		nextButtonObject.SetActive (false);
		confirmButton.SetActive (false);

		if (currentIndicator != null) {
			Destroy (currentIndicator);
			currentIndicator = null;
		}
	}
}