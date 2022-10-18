using AppleStockChecker.Apple;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace AppleStockChecker.Notifier
{
    internal class NotifierProfile
    {
        private Notifier _notifier;
        private Configuration _config;
        private Configuration.Profile _profileConfig;
        private int _index;
        private int _checkMins;

        private HttpClient _httpClient;
        private Dictionary<string, string> _query;

        public NotifierProfile(Notifier notif, Configuration config, int index)
        {
            _notifier = notif;
            _config = config;
            _profileConfig = config.profiles[index];
            _index = index;
            _checkMins = _profileConfig.checkMins;

            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromMilliseconds(config.queryTimeoutMs);

            _query = new Dictionary<string, string>();
            foreach (var keyval in config.fixedParams)
                _query.TryAdd(keyval.Key, keyval.Value);
            int i = 0;
            foreach (Configuration.Model model in config.models)
            {
                if (!model.active)
                    continue;
                _query.TryAdd($"mts.{i}", "regular");
                _query.TryAdd($"parts.{i}", model.part);
                ++i;
            }
            _query.TryAdd("location", _profileConfig.postalCode.ToString());
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            await Notifier.Delay(5000 * _index, cancellationToken);
            while (!cancellationToken.IsCancellationRequested && _profileConfig.active)
            {
                await _notifier.BeginQuery(cancellationToken);
                Console.WriteLine($"={new string('-', 31)} CHECKING {_profileConfig.name} {new string('-', 31)}=");
                await Query(cancellationToken);
                _notifier.EndQuery();

                await Notifier.Delay(Math.Max(_checkMins, 1) * 60000, cancellationToken);
                _checkMins = _profileConfig.checkMins;
            }
        }

        private HttpRequestMessage GetRequest()
        {
            string queryUri = QueryHelpers.AddQueryString(_config.fulfillmentApi, _query);
            var request = new HttpRequestMessage(HttpMethod.Get, queryUri);
            request.Headers.Add("accept", "application/json");
            foreach (var keyval in _config.headers)
                request.Headers.Add(keyval.Key, keyval.Value);
            return request;
        }

        private async Task Query(CancellationToken cancellationToken)
        {
            try
            {
                for (int i = 0; i < 1 + _config.retryAttempts; ++i)
                {
                    HttpResponseMessage response = await _httpClient.SendAsync(GetRequest());
                    if (response.IsSuccessStatusCode)
                    {
                        await HandleResponse(response);
                        break;
                    }

                    if (i == _config.retryAttempts || response.StatusCode != HttpStatusCode.ServiceUnavailable)
                    {
                        await NotifyError("Query failed", $"Got HTTP status {response.StatusCode} after {i} retries");
                        break;
                    }

                    await Notifier.Delay(_config.retryWaitMs, cancellationToken);
                }
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                await NotifyError("Query failed", "HTTP request timed out");
            }
            catch (Exception ex)
            {
                await NotifyError("Query failed", $"Got exception {ex.Message}");
                Console.WriteLine(ex.Message);
            }
        }

        private async Task HandleResponse(HttpResponseMessage response)
        {
            using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
            string responseBody = await reader.ReadToEndAsync();

            try
            {
                JObject @object = JObject.Parse(responseBody);
                JToken? status = @object.SelectToken("head.status");
                int statusCode;
                if (status != null && (statusCode = status.ToObject<int>()) != 200)
                {
                    await NotifyError("Bad status", $"Api replied with status {statusCode}");
                    return;
                }

                string error = "";
                var availability = FindAvailability(@object, ref error);
                if (availability != null)
                    await NotifyAvailability(availability);
                else 
                    await NotifyError("No availability", $"Could not find availability. {error}");
            }
            catch (JsonException)
            {
                await NotifyError("Invalid response", "Received non-JSON data");
            }
        }

        private Dictionary<string, Notification>? FindAvailability(JObject @object, ref string error)
        {
            JToken? storeToken = @object.SelectToken("body.content.pickupMessage.stores");
            JToken[] stores;
            if (storeToken == null || (stores = storeToken.ToArray()) == null)
            {
                error = $"No stores for zip {_profileConfig.postalCode}.";
                return null;
            }

            var results = new Dictionary<string, Notification>();
            foreach (JToken store in stores)
            {
                Store? storeInfo = store.ToObject<Store>();
                JToken? partsAvailability = store.SelectToken("partsAvailability");
                if (storeInfo == null || partsAvailability == null)
                    continue;

                if (!string.IsNullOrEmpty(_profileConfig.preferredStore) && storeInfo.Value.storeName != _profileConfig.preferredStore)
                    continue;
                if (_profileConfig.maxStoreDistance > 0 && storeInfo.Value.storedistance > _profileConfig.maxStoreDistance)
                    continue;

                string storeName = storeInfo.Value.storeName;
                results.TryAdd(storeName, new Notification { StoreInfo = storeInfo.Value });

                var partsTokens = partsAvailability.ToArray();
                foreach (JToken partToken in partsTokens)
                {
                    PartAvailability? part = partToken.Children().First().ToObject<PartAvailability>();
                    if (part == null || part.Value.pickupDisplay == "unavailable")
                        continue;
                    string model = _notifier.LookupModel(part.Value.partNumber);
                    results[storeName].Available.Add(new Notification.Availability { Part = part.Value, Model = model });
                }
            }

            return results;
        }

        private async Task NotifyAvailability(Dictionary<string, Notification> notifications)
        {
            int inStock = 0;
            foreach (Notification notification in notifications.Values)
            {
                var availableModels = notification.Available;
                for (int i = 0; i < availableModels.Count; ++i, ++inStock)
                    availableModels[i].Message = $"{availableModels[i].Model} - IN STOCK";
                await _notifier.Notify(notification);
            }

            if (inStock > 0)
                _checkMins = Math.Max(_profileConfig.suppressInStockMins, _profileConfig.checkMins);
        }

        private async Task NotifyError(string title, string message)
            => await _notifier.NotifyError($"{_profileConfig.name} - {title}", message);
    }
}