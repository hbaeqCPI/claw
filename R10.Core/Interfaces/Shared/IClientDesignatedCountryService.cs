using R10.Core.Entities;
using R10.Core.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R10.Core.Interfaces
{
    public interface IClientDesignatedCountryService : IChildEntityService<Client, ClientDesignatedCountry>
    {
        List<string> ValidSystems { get; }

        Task<bool> Update(object key, int parentId, string systemType, string userName,
            IEnumerable<ClientDesignatedCountry> updated,
            IEnumerable<ClientDesignatedCountry> added,
            IEnumerable<ClientDesignatedCountry> deleted);
    }
}
