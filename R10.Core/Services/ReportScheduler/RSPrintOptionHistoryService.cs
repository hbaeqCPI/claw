using R10.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace R10.Core.Services
{
    public class RSPrintOptionHistoryService : IRSPrintOptionHistoryService
    {
        private readonly IApplicationDbContext _repository;

        public RSPrintOptionHistoryService(
            IApplicationDbContext repository
            )
        {
            _repository = repository;
        }
    }
}
