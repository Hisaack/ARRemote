﻿using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DrawmarkModel
{
    public Color color;
    public List<Vector2> points;
}

[Serializable]
public class PlaceCustomShapes
{
    public Vector2 points;
    public Pose Pose;
}

public enum CameraLayer
{
    DEFAULT = 0,
    TransparentFX = 1,
    IGNORE_RAYCAST = 2,
    WATER = 4,
    UI = 5
}
