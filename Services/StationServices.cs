using Microsoft.EntityFrameworkCore;
using System;
using Train_D.Services;
using Train_D.Models;
using Train_D.Data;
using Train_D.DTO;
using AutoMapper;
using Newtonsoft.Json.Linq;

namespace Train_D.Services
{
    public class StationsServices : IStationsServices
    {
        private readonly ApplicationDbContext _context;


        public StationsServices(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<String>> GetAll()
        {
            return await _context.Stations.Select(s => s.StationName).ToListAsync();
        }

        public async Task<Station> GetByName(string stationName)
        {
            return await _context.Stations.SingleOrDefaultAsync(m => m.StationName == stationName);
        }

        public async Task<Station> Add(Station station)
        {
            await _context.AddAsync(station);
            _context.SaveChanges();
            return station;
        }

        public Station Delete(Station station)
        {
            _context.Remove(station);
            _context.SaveChanges();
            return station;
        }

        public Station Update(Station station)
        {
            _context.Update(station);
            _context.SaveChanges();
            return station;
        }

        public bool IsExist(string stationName)
        {
            return _context.Stations.Any(s=>s.StationName == stationName);

        }

        public Dictionary<char, object> GroupedSations(List<string> stations)
        {
            var values = new Dictionary<char, object>();
            var station = stations.GroupBy(s => s[0])
                .OrderBy(c => c.Key);
            foreach (var item in station)
            {
                values.Add(item.Key, item.Select(x => x));
            }
            return values;
        }
    }
}
