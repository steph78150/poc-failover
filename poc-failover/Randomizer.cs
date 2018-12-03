using System;

public class Randomizer {
    private readonly Random _random = new Random();

    public TimeSpan GetRandomDelay(TimeSpan maxDelay) { 
        var milliseconds = _random.Next( (int) maxDelay.TotalMilliseconds);
        return TimeSpan.FromMilliseconds(milliseconds);
    }
}
