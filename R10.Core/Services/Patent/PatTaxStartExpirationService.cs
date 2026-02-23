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
using R10.Core.DTOs;

namespace R10.Core.Services
{
    public class PatTaxStartExpirationService : IPatTaxStartExpirationService
    {
        private readonly IPatTaxStartExpirationRepository _repository;

        public PatTaxStartExpirationService(IPatTaxStartExpirationRepository repository)
        {
            _repository = repository;
        }

        public async Task<PatCountryLawTaxInfoDTO> ComputeTaxStart(int appId)
        {
            var app = await _repository.GetCountryApplicationToCompute(appId);
            if (app == null)
            {
                return new PatCountryLawTaxInfoDTO() { RequireUserResponse = PatCountryLawTaxInfoDTO.UserResponse.NotNeeded };
            }

            var taxStartInfo = await _repository.ComputeTaxStart(appId);
            taxStartInfo.RequireUserResponse = PatCountryLawTaxInfoDTO.UserResponse.AcceptReject;

            //has calculated expiration date
            if (taxStartInfo.ExpTaxDate.HasValue)
            {
                //has found a Tax Start Date that has missing BasedOn date
                if (taxStartInfo.ExpTaxNoBaseDateCount > 0)
                {
                    taxStartInfo.MessageType = taxStartInfo.ExpTaxDateCount > 1 ? TaxDateMessageType.MultipleNoBaseDate : TaxDateMessageType.SingleNoBaseDate;
                }
                else
                {
                    //multiple Tax Start date found
                    if (taxStartInfo.ExpTaxDateCount > 1)
                    {
                        taxStartInfo.MessageType = TaxDateMessageType.Multiple;
                    }
                    else if (!app.TaxStartDate.HasValue || app.TaxStartDate.Value != taxStartInfo.ExpTaxDate.Value)
                    {
                        taxStartInfo.MessageType = TaxDateMessageType.Single;
                    }
                    else
                    {
                        taxStartInfo.RequireUserResponse = PatCountryLawTaxInfoDTO.UserResponse.NotNeeded;
                    }
                }
            }
            else if (taxStartInfo.ExpTaxNoBaseDateCount > 0)
            {
                //No Tax Start Date can be calculated, and yet there are some possible
                //Tax Start Dates that have missing BasedOn Dates
                taxStartInfo.MessageType = TaxDateMessageType.MissingBasedOnDate;
                taxStartInfo.RequireUserResponse = PatCountryLawTaxInfoDTO.UserResponse.JustOk;
            }
            else
            {
                taxStartInfo.RequireUserResponse = PatCountryLawTaxInfoDTO.UserResponse.NotNeeded;
            }

            return taxStartInfo;
        }

        public async Task<PatCountryLawTaxInfoDTO> ComputeExpiration(int appId)
        {
            var app = await _repository.GetCountryApplicationToCompute(appId);
            if (app == null)
            {
                return new PatCountryLawTaxInfoDTO() { RequireUserResponse = PatCountryLawTaxInfoDTO.UserResponse.NotNeeded };
            }

            if (!app.IssDate.HasValue)
            {
                var canCompute = await _repository.CanComputeExpirationBeforeIssue(app);
                if (canCompute == PatCountryLawTaxInfoDTO.UserResponse.NotNeeded)
                {
                    return new PatCountryLawTaxInfoDTO() { RequireUserResponse = PatCountryLawTaxInfoDTO.UserResponse.NotNeeded };
                }
            }

            var expireInfo = await _repository.ComputeExpiration(appId);
            expireInfo.RequireUserResponse = PatCountryLawTaxInfoDTO.UserResponse.AcceptReject;

            if (expireInfo.ExpTaxDate.HasValue)
            {
                expireInfo.ExpTaxDate = ((DateTime)expireInfo.ExpTaxDate).AddDays(app.PatentTermAdj);

                //multiple expiration date found
                if (expireInfo.ExpTaxDateCount > 1 && expireInfo.ExpTaxNoBaseDateCount == 0)
                {
                    expireInfo.MessageType = TaxDateMessageType.Multiple;
                    //expireInfo.ExpirationDates.ForEach(d=> d= d.AddDays(app.PatentTermAdj));
                    var expirationDates = expireInfo.ExpirationDates.Select(d => d = d.AddDays(app.PatentTermAdj))
                        .OrderBy(d=> d).ToList();
                    expireInfo.ExpirationDates = expirationDates;
                    expireInfo.ExpTaxDate = (DateTime)expirationDates[0];
                }
                else if (!app.ExpDate.HasValue || app.ExpDate.Value != expireInfo.ExpTaxDate.Value)
                {
                    expireInfo.MessageType = app.PatentTermAdj > 0 ? TaxDateMessageType.WithPta : TaxDateMessageType.Single;
                }
                else
                {
                    expireInfo.RequireUserResponse = PatCountryLawTaxInfoDTO.UserResponse.NotNeeded;
                }
               
            }
            else if (expireInfo.ExpTaxNoBaseDateCount > 0)
            {
                //No expiration Date can be calculated, and yet there are some possible
                //Expiration Dates that have missing BasedOn Dates
                expireInfo.MessageType = TaxDateMessageType.MissingBasedOnDate;
                expireInfo.RequireUserResponse = PatCountryLawTaxInfoDTO.UserResponse.JustOk;
            }
            else
            {
                expireInfo.RequireUserResponse = PatCountryLawTaxInfoDTO.UserResponse.NotNeeded;
            }

            return expireInfo;
        }

        public async Task UpdateTaxInfo(TaxInfoUpdateType updateType, int appId, DateTime? taxStartDate,
            DateTime? expireDate, string updatedBy)
        {
            await _repository.UpdateTaxInfo(updateType, appId, taxStartDate, expireDate, updatedBy);
        }
    }
}
