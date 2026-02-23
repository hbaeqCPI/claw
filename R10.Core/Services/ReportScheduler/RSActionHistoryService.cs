using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Core.Services
{
    public class RSActionHistoryService : IRSActionHistoryService
    {
        private readonly IApplicationDbContext _repository;

        public RSActionHistoryService(
            IApplicationDbContext repository
            )
        {
            _repository = repository;
        }


    }
}
