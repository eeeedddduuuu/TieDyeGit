using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PatternLineData
{
    public string patternId;
    public Texture2D lineTexture;
}

[System.Serializable]
public class DesignSessionData
{
    public CanvasDesignData designData;
    public Vector2 canvasSize;
    public Vector2 canvasPosition;
}