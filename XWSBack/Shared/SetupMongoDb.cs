using System;
using MongoDB.Driver;

namespace Shared
{
    public class SetupMongoDb
    {
        public static MongoClient CreateClient<T>(string endpointName, string collectionName)
        {
            var mongoConnectionString =
                Environment.GetEnvironmentVariable($"{endpointName}MongoDb") ?? "mongodb://localhost:27017";
            var client = new MongoClient(mongoConnectionString);
            
            var usersDb = client.GetDatabase(collectionName);
            var collection = usersDb.GetCollection<T>(collectionName);
            if (collection == null)
            {
                usersDb.CreateCollection(collectionName);
            }

            return client;
        }
    }
}