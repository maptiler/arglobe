using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

public class ChatBubble : MonoBehaviour {

	public delegate void ChatBubbleMoveCompletionHandler ();
	private ChatBubbleMoveCompletionHandler _callback;

	public static GameObject createChatBubble(GameObject canvas, string text) {
		
		GameObject chatBubble = (GameObject)Instantiate(Resources.Load("Prefabs/ChatBubble"));
		chatBubble.transform.SetParent (canvas.transform,false);
		chatBubble.GetComponentInChildren<Text>().text = text;
		Canvas.ForceUpdateCanvases ();
		Vector2 p = new Vector2(-chatBubble.GetComponent<RectTransform>().rect.width, -20f);
		 
		chatBubble.GetComponent<RectTransform> ().anchoredPosition = p;
		return chatBubble;
	}

	public static IEnumerator moveBuble(GameObject chatBubble, List<GameObject> chatBubbles, ChatBubbleMoveCompletionHandler callback) {

		float seconds = 0.2f;
		float elapsedTime = 0f;

		RectTransform rect1 = chatBubble.GetComponent<RectTransform> ();
		GameObject previousBubble = chatBubbles.Count() == 0 ? chatBubble : chatBubbles.Last();
		RectTransform rect = previousBubble.GetComponent<RectTransform> ();

		float height = chatBubbles.Count () == 0 ? 0 : rect.rect.height;

		Vector2 startPosition = new Vector2 (-rect1.rect.width, rect.anchoredPosition.y - height - 5f);
		chatBubble.GetComponent<RectTransform> ().anchoredPosition = startPosition;

		Vector2 endPosition = new Vector2 (20f, startPosition.y);
		Vector2 currentPosition = startPosition;

		while (currentPosition.x != endPosition.x) {

			currentPosition = chatBubble.GetComponent<RectTransform>().anchoredPosition = Vector2.Lerp (startPosition, endPosition, (elapsedTime/seconds));

			elapsedTime += Time.deltaTime;
			yield return new WaitForEndOfFrame ();
		}

		callback.Invoke ();
	}

	public void animateBubble() {


	}
}
