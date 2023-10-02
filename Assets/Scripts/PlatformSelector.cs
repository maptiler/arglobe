using UnityEngine;


public class PlatformSelector : MonoBehaviour
{
    [SerializeField]
    MonoBehaviour[] hololensScripts, mobileScripts;

    [SerializeField]
    GameObject[] hololensObjects, mobileObjects;

    [SerializeField]
    Camera hololensCamera, mobileCamera;


    private void Awake()
    {
#if WINDOWS_UWP
        for (int i = 0; i < hololensScripts.Length; i++)
        {
            hololensScripts[i].enabled = true;
        }
        for (int i = 0; i < mobileScripts.Length; i++)
        {
            mobileScripts[i].enabled = false;
        }

        for (int i = 0; i < hololensObjects.Length; i++)
        {
            hololensObjects[i].SetActive(true);
        }
        for (int i = 0; i < mobileObjects.Length; i++)
        {
            mobileObjects[i].SetActive(false);
        }
        hololensCamera.enabled = true;
        mobileCamera.enabled = false;
        LevelManagerGlobe.Instance.cameraRef = hololensCamera;
#else
        for (int i = 0; i < hololensScripts.Length; i++)
        {
            hololensScripts[i].enabled = false;
        }
        for (int i = 0; i < mobileScripts.Length; i++)
        {
            mobileScripts[i].enabled = true;
        }

        for (int i = 0; i < hololensObjects.Length; i++)
        {
            hololensObjects[i].SetActive(false);
        }
        for (int i = 0; i < mobileObjects.Length; i++)
        {
            mobileObjects[i].SetActive(true);
        }
        hololensCamera.enabled = false;
        mobileCamera.enabled = true;
        LevelManagerGlobe.Instance.cameraRef = mobileCamera;
#endif
    }
}
