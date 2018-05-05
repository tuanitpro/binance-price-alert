using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1.Core
{
    public class Coinmarketcap
    {
        [JsonProperty("total_market_cap_usd")]
        public decimal TotalMarketCapUsd { get; set; }
        [JsonProperty("total_24h_volume_usd")]
        public decimal Total24hVolumeUsd { get; set; }
        [JsonProperty("bitcoin_percentage_of_market_cap")]
        public decimal BitcoinPercentageOfMarketCap { get; set; }
    }
    public class CoinmarketcapService
    {
        public async Task<Coinmarketcap> GetCoinmarketcapGlobal()
        {
            Coinmarketcap dataOutput = new Coinmarketcap();
            var httpClient = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.coinmarketcap.com/v1/global/");
            HttpResponseMessage response = await httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                dataOutput = JsonConvert.DeserializeObject<Coinmarketcap>(content);
            }
            return await Task.FromResult(dataOutput);
        }
    }
}