using Microsoft.EntityFrameworkCore;
using R10.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using R10.Core.DTOs;

namespace R10.Infrastructure.Data
{

    public class MapRepository : IMapRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public MapRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<MapDTO>> GetMarkerById(string screen, int id)
        {
            var action = GetAction(screen);

            if (action == 0)
                return null;

            var list = await _dbContext.MapDTO.FromSqlInterpolated($"procSysMapMarker @Action={action}, @Id={id}").AsNoTracking().ToListAsync();
            return list;
        }

        private int GetAction(string screen)
        {

            switch (screen)
            {
                case "Application":
                    return 1;

                case "Trademark":
                    return 2;

                case "Invention":
                    return 3;

                default:
                    return 0;
            }

        }
    }
}


