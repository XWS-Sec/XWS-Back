using System;
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
        const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefhijklmnopqrstuvwxyz";
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
            bool shouldNotStop = true;
            string apiKey = string.Empty;
            do
            {
                var random = new Random();
                apiKey = new string(Enumerable.Repeat(Chars, 26)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
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