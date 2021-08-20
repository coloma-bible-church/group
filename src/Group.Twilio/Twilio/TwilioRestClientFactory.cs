namespace Group.Twilio.Twilio
{
    using Common.Configuration;
    using global::Twilio.Clients;
    using global::Twilio.Http;
    using Microsoft.Extensions.Configuration;
    using HttpClient = System.Net.Http.HttpClient;

    public class TwilioRestClientFactory
    {
        readonly IConfiguration _configuration;
        readonly HttpClient _httpClient;

        public TwilioRestClientFactory(
            IConfiguration configuration,
            HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public TwilioRestClient Create() =>
            new(
                username: _configuration.GetRequired("TWILIO_USERNAME"),
                password: _configuration.GetRequired("TWILIO_PASSWORD"),
                accountSid: _configuration["TWILIO_ACCOUNT_SID"],
                region: _configuration["TWILIO_REGION"],
                httpClient: new SystemNetHttpClient(_httpClient),
                edge: _configuration["TWILIO_EDGE"]
            );
    }
}