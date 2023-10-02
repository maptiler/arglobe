using UnityEngine;


public class UIController : MonoBehaviour {
	[field: SerializeField]
	public GlobePanelUI GlobePanel { get; private set; }

	[field: SerializeField]
	public InfoPanelUI InfoPanel { get; private set; }

	[field: SerializeField]
	public ChatPanelUI ChatPanel { get; private set; }

	[field: SerializeField]
	public TutorialController TutorialPanel { get; private set; }

	[SerializeField]
	private GameObject globeButton, recalibrateButton, helpButton, infoButton;

	public GameObject GlobeButton { get => globeButton; }


	private void Awake()
	{
		TutorialPanel.SetTutorialStep(0);
		/*if (PlayerPrefs.GetInt("firstRun", 0) == 0)
		{
		}
		else
		{
			SetButtonsVisibility(true);
		}*/
	}

	public void SetButtonsVisibility(bool newVisibility)
    {
		globeButton.SetActive(newVisibility);
		recalibrateButton.SetActive(newVisibility);
		helpButton.SetActive(newVisibility);
		infoButton.SetActive(newVisibility);
	}

	public void CancelTutorialPressed()
	{
		//Vector3 position = mapParent.transform.position;
		LevelManagerGlobe.Instance.replaceFinalStep(Vector3.zero);//position);
	}
}