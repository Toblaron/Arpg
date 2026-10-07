// TrajectoryHelper.cs – launch math for lobbed projectiles.
using Godot;

public static class TrajectoryHelper
{
    /// <summary>
    /// Initial velocity to land on <paramref name="target"/> after <paramref name="flightTime"/> seconds
    /// under <paramref name="gravity"/> (pixels/s², pulling +Y).
    /// </summary>
    public static Vector2 CalculateLaunch(Vector2 start, Vector2 target, float flightTime, float gravity)
    {
        flightTime = Mathf.Max(0.05f, flightTime);
        Vector2 d = target - start;
        return new Vector2(d.X / flightTime, d.Y / flightTime - 0.5f * gravity * flightTime);
    }
}
