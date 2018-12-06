using System;

public class NetworkRandomizer {
    private readonly Random _random = new Random();

    public static readonly TimeSpan NetworkDelay = TimeSpan.FromMilliseconds(15);

    public TimeSpan GetRandomDelay() { 
        var milliseconds = _random.Next( (int) NetworkDelay.TotalMilliseconds);
        return TimeSpan.FromMilliseconds(milliseconds) + NetworkDelay;
    }
}
