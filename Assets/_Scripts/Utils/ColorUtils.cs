using System;
using UnityEngine;

//TODO: remove when adding animations
public static class ColorUtils
{
    public static Color HexToColor(string hex)
    {
        try
        {
            var red = Convert.ToInt32(hex.Substring(0, 2), 16);
            var green = Convert.ToInt32(hex.Substring(2, 2), 16);
            var blue = Convert.ToInt32(hex.Substring(4, 2), 16);

            return new Color32((byte)red, (byte)green, (byte)blue, 255);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error converting hex to color: {ex.Message}");
            return Color.white;
        }
    }
}