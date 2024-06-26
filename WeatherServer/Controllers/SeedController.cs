﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CountryModel;
using CsvHelper.Configuration;
using System.Globalization;
using CsvHelper;
using WeatherServer.Data;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Identity;

namespace WeatherServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeedController(CountriesSourceContext db, IHostEnvironment environment, UserManager<WorldCityUser> userManager) : ControllerBase
    {
        private readonly string _pathName = Path.Combine(environment.ContentRootPath, "Data/worldcities.csv");

            [HttpPost("user")]
        public async Task<ActionResult> SeedUser()
        {
            (string name, string email) = ("user1", "comp584@csun.edu");
            WorldCityUser user = new()
            {
                UserName = name,
                Email = email,
                SecurityStamp = Guid.NewGuid().ToString()
            };
            if (await userManager.FindByNameAsync(name) is not null)
            {
                user.UserName = "user2";
            }
            _ = await userManager.CreateAsync(user, "P@ssw0rd!")
                ?? throw new InvalidOperationException();
            user.EmailConfirmed = true;
            user.LockoutEnabled = false;
            await db.SaveChangesAsync();

            return Ok();
        }

            [HttpPost("city")]
        public async Task<ActionResult<Country>> SeedCity()
        {
            Dictionary<string, Country> countries = await db.Countries//.AsNoTracking()
            .ToDictionaryAsync(c => c.Name);

            CsvConfiguration config = new(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                HeaderValidated = null
            };
            int cityCount = 0;
            using (StreamReader reader = new("C:\\Users\\nmh13452\\Downloads\\worldcities.csv"))
            using (CsvReader csv = new(reader, config))
            {
                IEnumerable<WorldCitiesCsv>? records = csv.GetRecords<WorldCitiesCsv>();
                foreach (WorldCitiesCsv record in records)
                {
                    if (!countries.TryGetValue(record.country, out Country? value))
                    {
                        Console.WriteLine($"Not found country for {record.city}");
                        return NotFound(record);
                    }

                    if (!record.population.HasValue || string.IsNullOrEmpty(record.city_ascii))
                    {
                        Console.WriteLine($"Skipping {record.city}");
                        continue;
                    }
                    City city = new()
                    {
                        Name = record.city,
                        Latitude = record.lat,
                        Longtitude = record.lng,
                        Population = (int)record.population.Value,
                        CountryId = value.CountryId,
                    };
                    db.Cities.Add(city);
                    cityCount++;
                }
                await db.SaveChangesAsync();
            }
            return new JsonResult(cityCount);
        }

        [HttpPost("country")]
        public async Task<ActionResult<Country>> SeedCountry()
        {
            // create a lookup dictionary containing all the countries already existing 
            // into the Database (it will be empty on first run).
            Dictionary<string, Country> countriesByName = db.Countries
                .AsNoTracking().ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

            CsvConfiguration config = new(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                HeaderValidated = null
            };

            using StreamReader reader = new("C:\\Users\\nmh13452\\Downloads\\worldcities.csv");
            using CsvReader csv = new(reader, config);

            List<WorldCitiesCsv> records = csv.GetRecords<WorldCitiesCsv>().ToList();
            foreach (WorldCitiesCsv record in records)
            {
                if (countriesByName.ContainsKey(record.country))
                {
                    continue;
                }

                Country country = new()
                {
                    Name = record.country,
                    Iso2 = record.iso2,
                    Iso3 = record.iso3
                };
                await db.Countries.AddAsync(country);
                countriesByName.Add(record.country, country);
            }

            await db.SaveChangesAsync();

            return new JsonResult(countriesByName.Count);
        }

    }
}
