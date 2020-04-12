using BeDudeApi.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace BeDudeApi.Services
{
    public class CoronaStatusService
    {
        private readonly IMongoCollection<CoronaStatus> _coronaStatuses;

        private readonly ILogger<CoronaStatusService> _logger;


        public CoronaStatusService(ICoronaStatusDatabaseSettings settings, ILogger<CoronaStatusService> logger)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            database.GetCollection<CoronaStatus>(settings.CoronaStatusCollectionName);

            _coronaStatuses = database.GetCollection<CoronaStatus>(settings.CoronaStatusCollectionName);
            _logger = logger;

        }

        public List<CoronaStatus> Get()
        {
            List<CoronaStatus> coronaData = _coronaStatuses.Find(coronaStatus => true).ToList();
            return coronaData;
        }

        public List<CoronaSatausResponse> GetCoronaChartData()
        {
            List<CoronaSatausResponse> coronaStatuChartData = new List<CoronaSatausResponse>();
            foreach (var coronaStatus in Get())
            {
                var date = coronaStatus.Date.Substring(0, coronaStatus.Date.IndexOf("T"));
                coronaStatuChartData.Add(new CoronaSatausResponse(date, coronaStatus.Confirmed, "configmed"));
                coronaStatuChartData.Add(new CoronaSatausResponse(date, coronaStatus.Active, "active"));
                coronaStatuChartData.Add(new CoronaSatausResponse(date, coronaStatus.Deaths, "deaths"));
                coronaStatuChartData.Add(new CoronaSatausResponse(date, coronaStatus.Recovered, "recovered"));
            }

            return coronaStatuChartData;

        }



        public CoronaStatus Get(string id) =>
            _coronaStatuses.Find<CoronaStatus>(book => book.Id == id).FirstOrDefault();

        public CoronaStatus Create(CoronaStatus status)
        {
            _coronaStatuses.InsertOne(status);
            return status;
        }

        public async void LoadData()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var streamTask = client.GetStreamAsync("https://api.covid19api.com/live/country/israel/status/confirmed");
            var statuses = await JsonSerializer.DeserializeAsync<List<CoronaStatus>>(await streamTask);
            var existingCoronaStataus = Get();
            _logger.LogInformation("CoronsaStatuses count: " + existingCoronaStataus.Count);
            if (existingCoronaStataus.Count == 0)
            {
                _logger.LogInformation("Empty statuses list, gonna import everything");
                foreach (var status in statuses)
                {
                    var res = Create(status);
                    _logger.LogInformation("Saving new CoronaStatus: " + res.Date);

                }
            }
            else
            {
                foreach (var status in statuses)
                {
                    var insertNewStatus = true;
                    foreach (var existingStatus in existingCoronaStataus)
                    {
                        if (status.Date == existingStatus.Date)
                        {
                            insertNewStatus = false;
                        }
                    }
                    if (insertNewStatus)
                    {
                        Create(status);
                        _logger.LogInformation("New CoronsaStatus has been added");
                    }
                }
            }
        }
    }
}