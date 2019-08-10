
public static class Helpers
{
    public static float GetNextLerp(float start, float end, float current, int num_steps)
    {
        float range = end - start;
        // Cap it - if it's before the start, make it start
        if (current < start && start < end)
        {
            return start;
        }
        else if (current > start && start > end)
        {
            return start;
        }
        // If it's after the end, make it end
        else if (current > end && start < end)
        {
            return end;
        }
        else if (current < end && start > end)
        {
            return end;
        }
        // Normal case
        else
        {
            float step = range / num_steps;
            return current + step;
        }
    }
}
