using BeDudeApi.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MongoDB.Driver.Core.Operations.ElementNameValidators;

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

            foreach (var statusName in new List<string>() {"confirmed", "recovered", "deaths"})
            {
                var apiUrl = "https://api.covid19api.com/dayone/country/israel/status/" + statusName;
                var streamTask = client.GetStreamAsync(apiUrl);
                var statuses = await JsonSerializer.DeserializeAsync<List<CoronaStatus>>(await streamTask);
                var existingStatuses = _coronaStatuses.Find(_ => true).ToList();
                if (existingStatuses.Count == 0)
                {
                    _coronaStatuses.InsertMany(statuses);
                }
                else
                {
                    foreach (var status in statuses)
                    {
                        var insert = true;
                        foreach (var existingStatus in existingStatuses)
                        {
                            if (status.Date == existingStatus.Date && status.Status == existingStatus.Status)
                            {
                                insert = false;
                            }
                        }

                        if (insert) _coronaStatuses.InsertOne(status);
                    }
                }
            }
        }
    }
}