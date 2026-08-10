namespace Task
{
    public class Data
    {
        public string DisplayName { get; private set; }
        public string Description { get; private set; }
        public  DateTime Deadline { get; private set; }
        public string Status { get; set; } = "To Do";

        public Data(string shortName, string description, DateTime deadline)
        {
            DisplayName = shortName;
            Description = description;
            Deadline = deadline;
        }
        public string ToSaveString()
        {
            return $"{Status}|{DisplayName}|{Description}|{Deadline:dd/MM/yyyy}";
        }
    }
}
