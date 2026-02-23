using R10.Core.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IMapService
    {
        Task<List<MapDTO>> GetMarkerById(string screen, int id);
    }
}