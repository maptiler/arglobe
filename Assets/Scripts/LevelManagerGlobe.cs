﻿using System.Collections;
using UnityEngine;


public class LevelManagerGlobe : MonoBehaviour {
	public static LevelManagerGlobe Instance { get; private set; }

	public Camera cameraRef;

	[SerializeField] GameObject mapParent;
	public GameObject MapParent { get => mapParent; }
	[SerializeField] GameObject elevator;
	//[SerializeField] private GameObject changeButton;
	//[SerializeField] private GameObject replaceButton;
	/*[SerializeField] GameObject cancelButton;
	[SerializeField] GameObject infoButton;
	[SerializeField] GameObject tutorialButton;
	[SerializeField] GameObject globePositionIndicator;
	[SerializeField] GameObject canvas;
	[SerializeField] Material UIMaterial;
	[SerializeField] Material infoMaterial;*/
    [SerializeField] MeshRenderer topCap;
    [SerializeField] MeshRenderer bottomCap;

	[field: SerializeField] public UIController UIControllerRef { get; private set; }

	private GlobeTileProviderUpdated currentGlobe;
	public GlobeTileProviderUpdated CurrentGlobe { get => currentGlobe; }
	//private GameObject currentLowResGameObject;

	//[SerializeField] ScanningControl scanScript;
	[field: SerializeField] public CalibrationControl CalibrationScript { get; private set; }
	[field: SerializeField] public SpinControl SpinScript { get; private set; }
	[field: SerializeField] public ScaleControl ScaleScript { get; private set; }


    private void Awake()
    {
		if (Instance == null)
			Instance = this;
		else
			Destroy(this);
	}

	private void OnDestroy()
	{
		Instance = null;
	}

	// Use this for initialization
	void Start () {

		/*UIMaterial.color = new Color (1f, 1f, 1f, 0f);
		infoMaterial.color = new Color (1f, 1f, 1f, 0f);
        topCapMaterial.color = new Color(1f, 1f, 1f, 0f);
        bottomCapMaterial.color = new Color(1f, 1f, 1f, 0f);*/

		//scanScript = gameObject.GetComponent<ScanningControl> ();
		//scanScript.mapParent = mapParent;
		//spinScript = gameObject.GetComponent<SpinControl> ();
		//spinScript.mapParent = mapParent;

		defaultDetectionAndPlacement ();

		//cancelPressed();

		SwitchGlobe("CelestialGlobe1792"); // FOR DEBUGGING (although it might stay in the release)
	}

	private void defaultDetectionAndPlacement() {

		//mapParent.SetActive (false);
		//changeButton.SetActive (false);
		//replaceButton.SetActive (false);
		/*tutorialButton.SetActive (false);
		globePositionIndicator.SetActive (false);
		infoButton.SetActive (false);*/

		StartCoroutine (startFirstScan ());
	}

	IEnumerator startFirstScan() {

		yield return new WaitForSeconds (1);
		//scanScript.enable ();
		ScanningControl.finalStep += replaceFinalStep;
	}

	public IEnumerator MoveOverSeconds (GameObject objectToMove, Vector3 end, float seconds)
	{
		float elapsedTime = 0;
		Vector3 startingPos = objectToMove.transform.position;
		while (elapsedTime < seconds)
		{
			objectToMove.transform.position = Vector3.Lerp(startingPos, end, (elapsedTime / seconds));
			elapsedTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		objectToMove.transform.position = end;
	}

	public void replace() {

		SpinScript.enabled = false;
		//scanScript.enable ();
		ScanningControl.finalStep += replaceFinalStep;

		//mapParent.SetActive (false);
		//changeButton.SetActive (false);
		/*cancelButton.SetActive (true);
		//replaceButton.SetActive (false);
		infoButton.SetActive (false);
		tutorialButton.SetActive (false);
		globePositionIndicator.SetActive (false);*/
	}

	public void replaceFinalStep(Vector3 position)
	{
		ScanningControl.finalStep -= replaceFinalStep;
		//scanScript.disable();

		mapParent.transform.position = position;

		//mapParent.SetActive(true);

		if (currentGlobe == null)
		{
			SwitchGlobe("CelestialGlobe1792");
		}
	}

	public void SwitchGlobe(string name) 
	{
		if (currentGlobe != null) 
		{
			currentGlobe.RemoveGlobe();
			currentGlobe = null;
		}

		GameObject newGlobe = Instantiate(Resources.Load<GameObject>("Globes/" + name), mapParent.transform);
		currentGlobe = newGlobe.GetComponent<GlobeTileProviderUpdated>();

        topCap.material.color = currentGlobe.globeData.topCapColor;
        bottomCap.material.color = currentGlobe.globeData.bottomCapColor;

		currentGlobe.transform.SetParent(elevator.transform, false);
		currentGlobe.transform.localPosition = new Vector3 (0f, 0f, 0f);
	}
}