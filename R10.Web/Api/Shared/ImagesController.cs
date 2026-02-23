using Microsoft.AspNetCore.Mvc;
using R9.Core.Entities.DMS;
using R9.Core.Entities.Patent;
using R9.Core.Interfaces.Shared;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using R9.Web.Filters;

namespace R9.Web.Api.Shared
{
    [Route("api/[controller]")]
    [ApiController]
    [ServiceFilter(typeof(ExceptionFilter))]
    public class ImagesController : ControllerBase
    {
        private readonly xxIImagesService _imagesService;

        public ImagesController(xxIImagesService imagesService)
        {
            _imagesService = imagesService;
        }


        //GET api/images/invention
        [Route("Invention")]
        public async Task<IActionResult> GetAllInventionImages()
        {
            var images = await _imagesService.GetAllInventionImagesAsync();
            return Ok(images);
        }

        //GET api/images/invention/87
        [Route("Invention/{id}")]
        public async Task<IActionResult> GetInventionImageById(int id)
        {
            var images = await _imagesService.GetInventionImageByIdAsync(id);
            if (images == null)
            {
                return NotFound();
            }
            return Ok(images);
        }

        //GET api/images/invention/parentId/20021
        [Route("Invention/ParentId/{id}")]
        public async Task<IActionResult> GetInventionImagesByParentId(int id)
        {
            var images = await _imagesService.GetInventionImagesByParentIdAsync(id);
            if (images.Count == 0)
            {
                return NotFound();
            }
            return Ok(images);
        }

        //GET api/images/invention/parentId/20021/Count
        [Route("Invention/ParentId/{id}/Count")]
        public async Task<IActionResult> GetInventionImagesByParentIdCount(int id)
        {
            var images = await _imagesService.GetInventionImagesByParentIdAsync(id);
            return Ok(images.Count);
        }


        //GET api/images/disclosure
        [Route("Disclosure")]
        public async Task<IActionResult> GetAllDisclosureImages()
        {
            var images = await _imagesService.GetAllDisclosureImagesAsync();
            return Ok(images);
        }

        //GET api/images/disclosure/58
        [Route("Disclosure/{id}")]
        public async Task<IActionResult> GetDisclosureImageById(int id)
        {
            var images = await _imagesService.GetDisclosureImageByIdAsync(id);
            if (images == null)
            {
                return NotFound();
            }
            return Ok(images);
        }

        //GET api/images/disclosure/parentId/2
        [Route("Disclosure/ParentId/{id}")]
        public async Task<IActionResult> GetDisclosureImagesByParentId(int id)
        {
            var images = await _imagesService.GetDisclosureImagesByParentIdAsync(id);
            if (images.Count == 0)
            {
                return NotFound();
            }
            return Ok(images);
        }

        //GET api/images/disclosure/parentId/2/Count
        [Route("Disclosure/ParentId/{id}/Count")]
        public async Task<IActionResult> GetDisclosureImagesByParentIdCount(int id)
        {
            var images = await _imagesService.GetDisclosureImagesByParentIdAsync(id);
            return Ok(images.Count);
        }
    }
}