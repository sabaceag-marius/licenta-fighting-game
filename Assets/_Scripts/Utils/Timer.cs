using UnityEngine;

public class Timer
{
    private float endTime;

    public Timer(float amountSeconds)
    {
        endTime = Time.time + amountSeconds;
    }

    public Timer(int amountFrames)
    {
        // Check for better ways to calculate this
        endTime = Time.time + (float) amountFrames / 60;
    }
    
    public bool IsDone()
    {
        return Time.time > endTime;
    }

    public float RemainingTime()
    {
        return IsDone() ? 0 : endTime - Time.time;
    }
}