using Microsoft.AspNetCore.Mvc;
using R10.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.Views.Shared.Components.RecordStamps
{
    public class RecordStamps : ViewComponent
    {
        public RecordStamps()
        {
        }

        public IViewComponentResult Invoke(string createdBy, DateTime? dateCreated, string updatedBy, DateTime? lastUpdate, byte[] tStamp = null)
        {
            var model = new RecordStampsEntity
            {
                CreatedBy = createdBy,
                DateCreated = dateCreated,
                UpdatedBy = updatedBy,
                LastUpdate = lastUpdate,
                tStamp = tStamp != null ? Convert.ToBase64String(tStamp) : null
            };
            return View(model);
        }

        //public IViewComponentResult Invoke(dynamic baseEntity)
        //{
        //    const string CPIDATEFORMAT = "dd-MMM-yyyy hh:mm tt";
        //    var model = new RecordStampsEntity
        //    {
        //        CreatedBy = baseEntity.CreatedBy,
        //        UpdatedBy = baseEntity.UpdatedBy,
        //        DateCreated = baseEntity.DateCreated?.ToString(CPIDATEFORMAT),
        //        LastUpdate = baseEntity.LastUpdate?.ToString(CPIDATEFORMAT)
        //    };
        //    return View(model);
        //}
    }

    public class RecordStampsEntity
    {
        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        [Display(Name = "Updated By")]
        public string? UpdatedBy { get; set; }

        [Display(Name = "Date Created")]
        public DateTime? DateCreated { get; set; }

        [Display(Name = "Last Update")]
        public DateTime? LastUpdate { get; set; }

        public string? tStamp { get; set; }
    }

}
