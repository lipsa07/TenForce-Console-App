using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using Newtonsoft.Json;
using Test_Taste_Console_Application.Constants;
using Test_Taste_Console_Application.Domain.DataTransferObjects;
using Test_Taste_Console_Application.Domain.DataTransferObjects.JsonObjects;
using Test_Taste_Console_Application.Domain.Objects;
using Test_Taste_Console_Application.Domain.Services.Interfaces;
using Test_Taste_Console_Application.Utilities;

namespace Test_Taste_Console_Application.Domain.Services
{
    /// <inheritdoc />
    public class MoonService : IMoonService
    {
        private readonly HttpClientService _httpClientService;

        private readonly Lazy<IReadOnlyList<Moon>> _moons;
        public MoonService(HttpClientService httpClientService)
        {
            _httpClientService = httpClientService;
            _moons = new Lazy<IReadOnlyList<Moon>>(LoadAllMoons, LazyThreadSafetyMode.PublicationOnly);
        }
        public IEnumerable<Moon> GetAllMoons()
        {
            return _moons.Value;
        }

        //Calling API and storing the result in a collection of Planet objects. The collection is then returned to the caller.
        private IReadOnlyList<Moon> LoadAllMoons()
        {
            var response = _httpClientService.Client
                .GetAsync(UriPath.GetAllMoonsWithMassQueryParameters)
                .Result;

            //If the status code isn't 200-299, then the function returns an empty collection.
            if (!response.IsSuccessStatusCode)
            {
                Logger.Instance.Warn($"{LoggerMessage.GetRequestFailed}{response.StatusCode}");
                return new Collection<Moon>();
            }

            var content = response.Content.ReadAsStringAsync().Result;

            //The JSON converter uses DTO's, that can be found in the DataTransferObjects folder, to deserialize the response content.
            var allMoons = new Collection<Moon>();
            var results = JsonConvert.DeserializeObject<JsonResult<MoonDto>>(content);

            //The JSON converter can return a null object. 
            if (results == null) return new Collection<Moon>();

            foreach(MoonDto moonDto in results.Bodies)
            {
                allMoons.Add(new Moon(moonDto));
            }

            return allMoons;
        }
    }
}
