using System.Collections;
using UnityEngine;


public class TutorialController : MonoBehaviour
{
    [SerializeField]
    private GameObject rotationAnimationObject, zoomAnimationObject;

    [SerializeField]
    GameObject confirmButton, cancelButton;


    int currentStep = 0;
    Vector3 startEulerAngles;
    Vector3 startScale;



    private void Update()
    {
        switch(currentStep)
        {
            case 0:
                CalibrationCheck();
                break;
            case 1:
                //PlacementCheck();
                break;
            case 2:
                SpinCheck();
                break;
            case 3:
                ScaleCheck();
                break;
        }
    }

    public void SetTutorialStep(int newStep = 0)
    {
        enabled = true;
        Handheld.Vibrate();
        currentStep = newStep;
        LevelManagerGlobe.Instance.UIControllerRef.SetButtonsVisibility(false);
        switch (currentStep)
        {
            case 0:
                TutStepZeroCalibrate();
                break;
            case 1:
                TutStepOnePlaceGlobe();
                break;
            case 2:
                TutStepTwoSpin();
                break;
            case 3:
                TutStepThreeScale();
                break;
            default:
                TutStepFourEnd();
                break;
        }
    }

    void CalibrationCheck()
    {
        if(LevelManagerGlobe.Instance.CalibrationScript.HasGround())
        {
            SetTutorialStep(1);
        }
    }

    public void OnCalibrationConfirmed()
    {
        LevelManagerGlobe.Instance.CalibrationScript.ConfirmCalibration();
        confirmButton.SetActive(false);
        cancelButton.SetActive(false);
        if (PlayerPrefs.GetInt("firstRun", 0) == 0)
        {
            SetTutorialStep(2);
        }
        else
        {
            LevelManagerGlobe.Instance.UIControllerRef.SetButtonsVisibility(true);
            LevelManagerGlobe.Instance.UIControllerRef.ChatPanel.CloseChat();
        }
    }

    public void OnCalibrationCanceled()
    {
        LevelManagerGlobe.Instance.CalibrationScript.enabled = false;
        LevelManagerGlobe.Instance.UIControllerRef.SetButtonsVisibility(true);
        LevelManagerGlobe.Instance.UIControllerRef.ChatPanel.CloseChat();
        confirmButton.SetActive(false);
        cancelButton.SetActive(false);
        enabled = false;
    }

    void SpinCheck()
    {
        float rotValue = Mathf.Abs(startEulerAngles.y - LevelManagerGlobe.Instance.MapParent.transform.eulerAngles.y);

        if (rotValue > 180)
        {
            rotationAnimationObject.SetActive(false);
            LevelManagerGlobe.Instance.SpinScript.enabled = false;
            LevelManagerGlobe.Instance.ScaleScript.enabled = true;
            SetTutorialStep(3);
        }
    }

    void ScaleCheck()
    {
        if (Mathf.Abs(startScale.x - LevelManagerGlobe.Instance.MapParent.transform.parent.localScale.x) > 12f)
        {
            zoomAnimationObject.SetActive(false);
            LevelManagerGlobe.Instance.SpinScript.enabled = true;
            LevelManagerGlobe.Instance.ScaleScript.enabled = true;
            SetTutorialStep(4);
        }
    }

    public void TutStepZeroCalibrate()
    {
        LevelManagerGlobe.Instance.UIControllerRef.ChatPanel.SetChatText("First we need to scan your room. Aim your camera at the floor.\n You should see yellow dots floating around.");
        LevelManagerGlobe.Instance.CalibrationScript.enabled = true;
        confirmButton.SetActive(false);
        cancelButton.SetActive(false);
    }

    void TutStepOnePlaceGlobe()
    {
        LevelManagerGlobe.Instance.UIControllerRef.ChatPanel.SetChatText("Good job. You can now place the globe\nby pressing the confirm button. Globe will be\nplaced at the center of the circle indicator.");
        confirmButton.SetActive(true);
        if (LevelManagerGlobe.Instance.CalibrationScript.IsPlaced)
        {
            cancelButton.SetActive(true);
        }
    }

    void TutStepTwoSpin()
    {
        LevelManagerGlobe.Instance.UIControllerRef.ChatPanel.SetChatText("Now lets try spinning the Globe around.\nSwipe your finger on the screen and see what happens.");
        startEulerAngles = LevelManagerGlobe.Instance.MapParent.transform.eulerAngles;
        rotationAnimationObject.SetActive(true);
    }

    void TutStepThreeScale()
    {
        LevelManagerGlobe.Instance.UIControllerRef.ChatPanel.SetChatText("Easy right? Now try to zoom in and out.");
        startScale = LevelManagerGlobe.Instance.MapParent.transform.parent.localScale;
        zoomAnimationObject.SetActive(true);
    }

    void TutStepFourEnd()
    {
        LevelManagerGlobe.Instance.UIControllerRef.ChatPanel.SetChatText("Excellent! This is everything.\n Remember you can replace the globe anytime\nas well as change it's texture.");
        PlayerPrefs.SetInt("firstRun", 1);
        LevelManagerGlobe.Instance.SpinScript.enabled = true;
        LevelManagerGlobe.Instance.ScaleScript.enabled = true;
        StartCoroutine(DelayedTutEnd());
    }

    private IEnumerator DelayedTutEnd()
    {
        rotationAnimationObject.SetActive(false);
        zoomAnimationObject.SetActive(false);
        yield return new WaitForSeconds(5);
        LevelManagerGlobe.Instance.UIControllerRef.SetButtonsVisibility(true);
        LevelManagerGlobe.Instance.UIControllerRef.ChatPanel.CloseChat();
        enabled = false;
    }
}