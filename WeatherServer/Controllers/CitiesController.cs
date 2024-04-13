using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CountryModel;
using WeatherServer.DTO;
using Microsoft.Identity.Client;
using Microsoft.AspNetCore.Authorization;

namespace WeatherServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CitiesController(CountriesSourceContext context) : ControllerBase
    {

        // GET: api/Cities
        [HttpGet]
        public async Task<ActionResult<IEnumerable<City>>> GetCities()
        {
            return await context.Cities.ToListAsync();
        }

        // GET: api/Cities/get-population
        [Authorize]
        [HttpGet("get-population")]
        public async Task<ActionResult<IEnumerable<CountryPopulation>>> GetPopulation()
        {
            IQueryable<CountryPopulation> x = from c in context.Countries
                    select new CountryPopulation
                    {
                        Name = c.Name,
                        CountryId = c.CountryId,
                        Population = c.Cities.Sum(city => city.Population)
                    };

            return await x.ToListAsync();
        }

        // GET: api/Cities/get-population2
        [HttpGet("get-population2")]
        public async Task<ActionResult<IEnumerable<CountryPopulation>>> GetPopulation2()
        {
            IQueryable<CountryPopulation> x = context.Countries.Select(c => new CountryPopulation
                                              {
                                                  Name = c.Name,
                                                  CountryId = c.CountryId,
                                                  Population = c.Cities.Sum(city => city.Population)
                                              });

            return await x.ToListAsync();
        }

    }
}
