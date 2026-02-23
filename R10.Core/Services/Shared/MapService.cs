using R10.Core.DTOs;
using R10.Core.Interfaces;

namespace R10.Core.Services.Shared
{
    public class MapService : IMapService
    {

        private readonly IMapRepository _repository;

        public MapService(IMapRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<MapDTO>> GetMarkerById(string screen, int id)
        {
            return await _repository.GetMarkerById(screen, id);
        }

    }


}
