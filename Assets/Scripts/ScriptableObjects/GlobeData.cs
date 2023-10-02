using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Globe Data", menuName = "Three Dragons/Globe Data")]
public class GlobeData : ScriptableObject
{
    public Sprite globeSprite;
    public string globeName;
    public string authorText;
    public string dateText;
    public string titleText;
    public string url;
    public Color topCapColor;
    public Color bottomCapColor;
}
