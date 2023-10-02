using DG.Tweening;
using UnityEngine;


public class GlobePanelUI : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup thisCanvasGroup;

	DG.Tweening.Core.TweenerCore<float, float, DG.Tweening.Plugins.Options.FloatOptions> fadeTween;


	public void ShowGlobes()
	{
		//replaceButton.SetActive (false);
		//infoButton.SetActive(false);
		//tutorialButton.SetActive(false);
		LevelManagerGlobe.Instance.UIControllerRef.SetButtonsVisibility(false);
		LevelManagerGlobe.Instance.UIControllerRef.GlobeButton.SetActive(true);

		//sideMenu = (GameObject)Instantiate(Resources.Load("Prefabs/MenuBackground"));
		//sideMenu.transform.SetParent(canvas2D.transform, false);
		//sideMenu.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);

		thisCanvasGroup.gameObject.SetActive(true);
		thisCanvasGroup.interactable = true;
		thisCanvasGroup.blocksRaycasts = true;
		thisCanvasGroup.alpha = 0;
		fadeTween.Kill();
		fadeTween = DOTween.To(() => thisCanvasGroup.alpha, x => thisCanvasGroup.alpha = x, 1, 1).OnComplete(() => thisCanvasGroup.alpha = 1);
	}

	public void CloseGlobes()
	{
		LevelManagerGlobe.Instance.UIControllerRef.SetButtonsVisibility(true);
		thisCanvasGroup.interactable = false;
		thisCanvasGroup.blocksRaycasts = false;
		fadeTween.Kill();
		fadeTween = DOTween.To(() => thisCanvasGroup.alpha, x => thisCanvasGroup.alpha = x, 0, 1).OnComplete(() => thisCanvasGroup.gameObject.SetActive(false));
	}

	public void SwitchGlobe(string name)
	{
		LevelManagerGlobe.Instance.SwitchGlobe(name);
		CloseGlobes();
	}
}
