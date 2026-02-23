using Kendo.Mvc.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Validation.AspNetCore;
using R10.Core.DTOs;
using R10.Core.Helpers;
using R10.Core.Interfaces;
using R10.Web.ApiEmail.Models;
using R10.Web.Interfaces;
using R10.Web.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace R10.Web.ApiEmail.Shared
{
    [Route("/emailapi/[controller]")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme, Policy = SharedAuthorizationPolicy.CanAccessSystem)]
    [EnableCors("EmailAddInCORSPolicy")]
    [ApiController]
    public class SharedController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly IDocumentViewModelService _docViewModelService;
        private readonly IDocumentService _docService;

        public SharedController(IDocumentViewModelService docViewModelService, IDocumentService docService)
        {
            _docService = docService;
            _docViewModelService = docViewModelService;
        }


        [HttpGet("getsystem")]
        public async Task<ActionResult<KeyTextDTO[]>> GetSystem()
        {
            var systemList = await _docViewModelService.GetSystemList(User.GetSystems());
            var results = TransformToArray(systemList);
            return results;
        }

        [HttpGet("getsystemshortname")]
        public async Task<ActionResult<KeyTextDTO[]>> GetSystemShortName()
        {
            var systemList = await _docViewModelService.GetSystemShortNameList(User.GetSystems());
            var results = TransformToArray(systemList);
            return results;
        }

        [HttpGet("getscreen/{systemtype}")]
        public async Task<ActionResult<KeyTextDTO[]>> GetScreen(string systemType)
        {
            var screenList = await _docViewModelService.GetScreenList(systemType);
            var results = TransformToArray(screenList);
            return results;
        }

        [HttpGet("checkconnection")]
        public ActionResult<string> CheckConnection ()
        {
            // use by Outlook add-in to check if connection/token is still valid
            return "success";
        }

        private KeyTextDTO[] TransformToArray(List<LookupDTO> list)
        {
            List<KeyTextDTO> keyTextDTO = new List<KeyTextDTO>();
            list.Each(l => keyTextDTO.Add(new KeyTextDTO { key = l.Value, text = l.Text }));

            return keyTextDTO.ToArray();
        }

        [HttpGet("getDocumentTree")]
        public async Task<ActionResult> GetDocumentTree(string systemType, string screenCode, string dataKey, int dataKeyValue)
        {
            var subTree = await _docService.GetDocumentTreeEmailApi(systemType, screenCode, dataKey, dataKeyValue, null);
            var childFolders = subTree.Where(t => t.parentId == 0).Select(t => new DocTreeEmailApiDTO { id = t.id, text = t.text, hasChildren = t.hasChildren, items = new List<DocTreeEmailApiDTO>() }).ToList();
            var resume = true;
            var folderList = childFolders.ToList();
            while (resume)
            {
                var currentChildFolders = childFolders.ToList();
                childFolders.Clear();
                foreach (var folder in currentChildFolders)
                {
                    if (folder.hasChildren)
                    {
                        var subTree2 = await _docService.GetDocumentTreeEmailApi(systemType, screenCode, dataKey, dataKeyValue, folder.id.ToString());
                        var childChildFolders = subTree2.Where(t => t.parentId == 0).Select(t => new DocTreeEmailApiDTO { id = t.id, text = t.text, hasChildren = t.hasChildren, parentId = folder.id, items = new List<DocTreeEmailApiDTO>() }).ToList();
                        childFolders.AddRange(childChildFolders);
                        if (childChildFolders.Count > 0)
                        {
                            folderList.AddRange(childChildFolders);
                        }
                    }
                }
                if (childFolders.Count == 0)
                {
                    resume = false;
                }
            }

            var folderTreeBase = folderList.Where(fl => fl.parentId == 0).ToList();
            getChildren(ref folderTreeBase, folderTreeBase.ToList(), folderList);

            var returnArray = subTree.Where(t => t.parentId == -1).Select(t => new {id = t.id, text = t.text, hasChildren = t.hasChildren, items = folderTreeBase }).ToArray();
            return Json(returnArray);
        }

        private List<DocTreeEmailApiDTO> getChildren(ref List<DocTreeEmailApiDTO> treeBase, List<DocTreeEmailApiDTO> currentTree, List<DocTreeEmailApiDTO> folderList)
        {
            foreach (var folder in currentTree)
            {
                if (folder.hasChildren)
                {
                    treeBase.Find(b => b.id == folder.id).items.AddRange(folderList.Where(fl => fl.parentId == folder.id));
                    var items = treeBase.Find(b => b.id == folder.id).items;
                    folderList.RemoveAll(fl => fl.parentId == folder.id);
                    getChildren(ref items, treeBase.Find(b => b.id == folder.id).items, folderList);
                }
            }
            return treeBase;
        }

    }
}
