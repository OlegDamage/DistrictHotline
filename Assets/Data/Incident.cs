public class Incident
{
    public string Id { get; }
    public string Title { get; }
    public int BaseSeverity { get; }

    public Incident(string id, string title, int severity)
    {
        Id = id;
        Title = title;
        BaseSeverity = severity;
    }
}
