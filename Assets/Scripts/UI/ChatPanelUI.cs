using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;


public class ChatPanelUI : MonoBehaviour
{
	[SerializeField]
	private CanvasGroup thisCanvasGroup;

	[SerializeField]
	TMP_Text chatText;

	[SerializeField]
	RectTransform thisRectTransform;

	DG.Tweening.Core.TweenerCore<float, float, DG.Tweening.Plugins.Options.FloatOptions> fadeTween;


	public void SetChatText(string newText)
	{
		gameObject.SetActive(true);
		thisCanvasGroup.alpha = 0;
		chatText.SetText(newText);
		StartCoroutine(MoveSequence());
	}

	private IEnumerator MoveSequence()
    {
		yield return null;
		thisCanvasGroup.alpha = 0;
		if (fadeTween != null)
			fadeTween.Kill(true);
		fadeTween = DOTween.To(() => thisCanvasGroup.alpha, x => thisCanvasGroup.alpha = x, 1, 1).OnComplete(() => thisCanvasGroup.alpha = 1);
		Vector2 startPosition = new Vector2(-thisRectTransform.rect.width, -20);
		Vector2 endPosition = new Vector2(20f, startPosition.y);
		thisRectTransform.anchoredPosition = startPosition;
		thisRectTransform.DOAnchorPos(endPosition, 1).SetEase(Ease.OutCubic);
	}

	public void CloseChat()
    {
		gameObject.SetActive(false);
    }
}
