using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StyleDefinition 
{
    public string name = "Default";
    public Color mainColor = new Color(0.84f, 0.84f, 0.84f, 1.0f);
    public TitleBarStyle title = new TitleBarStyle();
    public MenuBarStyle menu = new MenuBarStyle();
    public ButtonStyle button = new ButtonStyle();
}
