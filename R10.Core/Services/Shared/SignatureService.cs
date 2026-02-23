using R10.Core.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using R10.Core.Entities;
using System.Linq;
using R10.Core.Interfaces.Patent;
using R10.Core.Entities.Patent;
using R10.Core.Identity;
using System.Linq.Expressions;
using System;
using System.Data;
using R10.Core.DTOs;
using Microsoft.EntityFrameworkCore;

namespace R10.Core.Services
{
    public class SignatureService : ISignatureService
    {
        private readonly ISignatureRepository _repository;

        public SignatureService(ISignatureRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<DocReviewDTO>> GetDocs(string systemType, string displayType) {
              return await _repository.GetDocs(systemType,displayType);
        }

        public async Task MarkReviewed(List<DocReviewDTO> updated, string userName)
        {
            await _repository.MarkReviewed(updated, userName);
        }
        
    }
}
