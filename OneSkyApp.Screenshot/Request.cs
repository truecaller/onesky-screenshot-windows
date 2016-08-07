using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;

namespace OneSkyApp.Screenshot
{
    static class Request
    {
        public static async Task UploadAsync(string apiKey, string apiSecret, string projectId, string data)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add(HttpRequestHeader.ContentType.ToString(), "application/json");

                var uri = BuildUri(apiKey, apiSecret, projectId);

                using (var stringContent = new StringContent(data, Encoding.UTF8))
                    using (var response = await httpClient.PostAsync(uri, stringContent))
                        response.EnsureSuccessStatusCode();
            }
        }

        static Uri BuildUri(string apiKey, string apiSecret, string projectId)
        {
            var unixTimestamp = (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            var md5Hash = ComputeMd5(unixTimestamp + apiSecret);

            var url = $"https://platform.api.onesky.io/1/projects/{projectId}/screenshots?api_key={apiKey}&dev_hash={md5Hash}&timestamp={unixTimestamp}";
            return new Uri(url, UriKind.Absolute);
        }

        static string ComputeMd5(string data)
        {
            var algorithm = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            var buffer = CryptographicBuffer.ConvertStringToBinary(data, BinaryStringEncoding.Utf8);
            var hashed = algorithm.HashData(buffer);

            return CryptographicBuffer.EncodeToHexString(hashed);
        }
    }
}
