namespace BeDudeApi.Models
{
    public class CoronaStatusDatabaseSettings : ICoronaStatusDatabaseSettings
    {
        public string CoronaStatusCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public interface ICoronaStatusDatabaseSettings
    {
        string CoronaStatusCollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}