using UnityEngine;

[System.AttributeUsage(System.AttributeTargets.Field)]
public class MinMaxSliderAttribute : PropertyAttribute
{
    public float FloatMin;
    public float FloatMax;

    public int IntMin;
    public int IntMax;

    public MinMaxSliderAttribute(float min, float max)
    {
        this.FloatMin = min;
        this.FloatMax = max;
    }

    public MinMaxSliderAttribute(int min, int max)
    {
        this.IntMin = min;
        this.IntMax = max;
    }
}