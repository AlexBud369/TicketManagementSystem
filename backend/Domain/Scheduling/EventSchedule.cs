namespace Domain.Scheduling;

public static class EventSchedule {
    public static DateTime GetStartUtc(DateTime date, TimeSpan startTime) {
        var day = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        return day.Add(startTime);
    }

    public static DateTime GetEndUtc(DateTime date, TimeSpan startTime, TimeSpan endTime) {
        var start = GetStartUtc(date, startTime);
        var end = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc).Add(endTime);

        if (end <= start)
            end = end.AddDays(1);

        return end;
    }

    public static bool HasStarted(DateTime date, TimeSpan startTime, DateTime utcNow) {
        return GetStartUtc(date, startTime) <= utcNow;
    }
}
