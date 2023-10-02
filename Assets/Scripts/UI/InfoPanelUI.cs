using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class InfoPanelUI : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup thisCanvasGroup;

	[SerializeField]
	private TMP_Text authorText, dateText, titleText;

	[SerializeField]
	private Image infoImage;


	string currentUrl;
	DG.Tweening.Core.TweenerCore<float, float, DG.Tweening.Plugins.Options.FloatOptions> fadeTween;


	public void ShowInfoPanel()
	{
		SetInfoPanel();
		thisCanvasGroup.gameObject.SetActive(true);
		thisCanvasGroup.interactable = true;
		thisCanvasGroup.blocksRaycasts = true;
		thisCanvasGroup.alpha = 0;
		fadeTween.Kill();
		fadeTween = DOTween.To(() => thisCanvasGroup.alpha, x => thisCanvasGroup.alpha = x, 1, 1).OnComplete(() => thisCanvasGroup.alpha = 1);
	}

	public void CloseInfoPanel()
	{
		thisCanvasGroup.interactable = false;
		thisCanvasGroup.blocksRaycasts = false;
		fadeTween.Kill();
		fadeTween = DOTween.To(() => thisCanvasGroup.alpha, x => thisCanvasGroup.alpha = x, 0, 1).OnComplete(() => thisCanvasGroup.gameObject.SetActive(false));
	}

	public void SetInfoPanel()
	{
		infoImage.sprite = LevelManagerGlobe.Instance.CurrentGlobe.globeData.globeSprite;
		authorText.text = LevelManagerGlobe.Instance.CurrentGlobe.globeData.authorText;
		dateText.text = LevelManagerGlobe.Instance.CurrentGlobe.globeData.dateText;
		titleText.text = LevelManagerGlobe.Instance.CurrentGlobe.globeData.titleText;
		currentUrl = LevelManagerGlobe.Instance.CurrentGlobe.globeData.url;
	}

	public void OpenLink()
	{
		Application.OpenURL(currentUrl);
	}
}
