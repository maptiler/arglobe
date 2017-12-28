using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class StartScript : MonoBehaviour {

	[DllImport ("__Internal")]
	private static extern int GetCameraPermission ();

	[DllImport ("__Internal")]
	private static extern void RequestCameraPermission(string gameObject, string callback);

	[DllImport ("__Internal")]
	private static extern void OpenSettings();

	[DllImport ("__Internal")]
	private static extern void DisplaySettingsWarning();

	public delegate void PermissionCallbackHandler ();
	private PermissionCallbackHandler _callback;

	[SerializeField] GameObject text;
	[SerializeField] GameObject startButton;
	[SerializeField] GameObject canvas;

	//private List<GameObject> chatBubbles = new List<GameObject>();

	// Use this for initialization
	void Start () {

		//StartCoroutine(firstChatBubble ());
		start();
	}

	IEnumerator firstChatBubble() {
		yield return new WaitForSecondsRealtime (0.75f);

//		GameObject chatBubble = ChatBubble.createChatBubble (canvas, "Welcome to ARGlobe. We are ready whenever you are.");
//		StartCoroutine (ChatBubble.moveBuble(chatBubble, chatBubbles, () => {
//			startButton.SetActive (true);
//		}));
//		chatBubbles.Add (chatBubble);

		start ();
	}

	private void start() {

		int status = GetCameraPermission ();

		if (status == 0) {
			
			RequestCameraPermission (gameObject.name, "permissionCallback");

		} else if (status == 1) {

			//DisplaySettingsWarning ();

			text.SetActive (true);
			startButton.SetActive (true);

//			GameObject chatBubble = ChatBubble.createChatBubble (canvas, "Sadly, we need permission to use camera,\nso we can see where to place the globe.");
//			StartCoroutine (ChatBubble.moveBuble(chatBubble, chatBubbles, () => {
//				startButton.SetActive(false);
//
//				StartCoroutine(settingsBubble());
//			}));
//			chatBubbles.Add (chatBubble);

		} else {
			SceneManager.LoadSceneAsync ("ARGlobeScene");
		}
	}

	public void goToSettings() {
		OpenSettings ();
	}

//	private IEnumerator settingsBubble() {
//		yield return new WaitForSecondsRealtime(1.25f);
//		GameObject permissionBubble = ChatBubble.createChatBubble (canvas, "You can set camera permission in settings under ARGlobe app.\n Want us to take you there?");
//		StartCoroutine(ChatBubble.moveBuble(permissionBubble, chatBubbles, () => {
//			startButton.GetComponentInChildren<Text>().text = "Let's go";
//			//shouldOpenSettings = true;
//			startButton.SetActive(true);
//
//		}));
//		chatBubbles.Add (permissionBubble);
//	}

	public void permissionCallback(string lmao) {
		start ();
	}
}
