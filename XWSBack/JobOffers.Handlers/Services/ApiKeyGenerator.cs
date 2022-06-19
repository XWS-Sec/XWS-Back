using System;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using JobOffers.Model;
using MongoDB.Driver;

namespace JobOffers.Handlers.Services
{
    public class ApiKeyGenerator
    {
        private const string Prefix = "XWS.";
        
        private readonly IMongoCollection<Company> _companyCollection;
        private readonly HashAlgorithm _hashAlgorithm;

        public ApiKeyGenerator(IMongoClient client, HashAlgorithm hashAlgorithm)
        {
            _hashAlgorithm = hashAlgorithm;
            _companyCollection = client.GetDatabase("JobOffers").GetCollection<Company>("JobOffers");
        }

        public async Task<string> GetApiKey()
        {
            var shouldNotStop = true;
            var apiKey = string.Empty;
            do
            {
                var randomGenerator = RandomNumberGenerator.Create();
                var bytes = new byte[128];
                randomGenerator.GetBytes(bytes);
                apiKey = Convert.ToBase64String(bytes);
                apiKey = Prefix + apiKey;

                var hashed = ComputeHash(apiKey);

                var companyWithThatApiKeyCursor = await _companyCollection.FindAsync(x => x.ApiKeyHash == hashed);

                var companyWithThatApiKey = await companyWithThatApiKeyCursor.FirstOrDefaultAsync();

                if (companyWithThatApiKey == null)
                    shouldNotStop = false;
            } while (shouldNotStop);

            return apiKey;
        }

        public string ComputeHash(string apiKey)
        {
            var bytes = _hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(apiKey));
            var stringBuilder = new StringBuilder();
            foreach (var b in bytes)
            {
                stringBuilder.Append(b.ToString("X2"));
            }

            return stringBuilder.ToString();
        }
    }
}