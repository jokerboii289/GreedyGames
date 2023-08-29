using System.Collections.Generic;

[System.Serializable]
public class UITemplate
{
    public string templateName;
    public List<UIObject> objects;
}

[System.Serializable]
public class UIObject
{
    public string type;
    public UIProperties properties;
    public UIPosition position;
    public UISize size;
    public List<UIObject> children;
}

[System.Serializable]
public class UIProperties
{
    public string text;
    public int value;
    public string renderMode;
    public string name;
    // Define properties specific to UI elements (text, image, color, etc.)
}

[System.Serializable]
public class UIPosition
{
    public float x;
    public float y;
}

[System.Serializable]
public class UISize
{
    public float width;
    public float height;
}
