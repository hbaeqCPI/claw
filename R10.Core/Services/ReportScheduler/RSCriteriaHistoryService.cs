using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Core.Services
{
    public class RSCriteriaHistoryService : IRSCriteriaHistoryService
    {
        private readonly IApplicationDbContext _repository;

        public RSCriteriaHistoryService(
            IApplicationDbContext repository
            )
        {
            _repository = repository;
        }
    }
}
