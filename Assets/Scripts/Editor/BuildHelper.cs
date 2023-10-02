using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

class BuildHelper : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }

    public void OnPreprocessBuild(BuildReport report)
    {
        PlayerSettings.SplashScreen.showUnityLogo = false;
    }
}