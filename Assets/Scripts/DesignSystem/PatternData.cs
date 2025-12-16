using UnityEngine;

[CreateAssetMenu(fileName = "NewPattern", menuName = "Design/Pattern Data")]

[System.Serializable]
public class PatternData
{
    public string patternId;
    public string displayName;
    public Sprite patternSprite;
    public Sprite whiteTextureSprite;
    public Texture2D patternTexture;
}