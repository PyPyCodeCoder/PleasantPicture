using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ImageService.Models;

namespace ImageService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SavedImagesController : ControllerBase
    {
        private readonly ImageServiceContext _context;

        public SavedImagesController(ImageServiceContext context)
        {
            _context = context;
        }

        // GET: api/SavedImages
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SavedImage>>> GetSavedImages()
        {
            return await _context.SavedImages
                .Include(si => si.Image)
                .Include(si => si.User)
                .ToListAsync();
        }

        // GET: api/SavedImages/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SavedImage>> GetSavedImage(int id)
        {
            var savedImage = await _context.SavedImages
                .Include(si => si.Image)
                .Include(si => si.User)
                .FirstOrDefaultAsync(si => si.Id == id);

            if (savedImage == null)
            {
                return NotFound();
            }

            return savedImage;
        }

        // PUT: api/SavedImages/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSavedImage(int id, SavedImage savedImage)
        {
            if (id != savedImage.Id)
            {
                return BadRequest();
            }

            _context.Entry(savedImage).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SavedImageExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/SavedImages
        [HttpPost]
        public async Task<ActionResult<SavedImage>> PostSavedImage(SavedImage savedImage)
        {
            var image = await _context.Images.FindAsync(savedImage.ImageId);
            var user = await _context.Users.FindAsync(savedImage.UserId);

            if (image == null || user == null)
            {
                return BadRequest("Invalid ImageId or UserId");
            }

            savedImage.Image = image;
            savedImage.User = user;
            
            _context.SavedImages.Add(savedImage);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSavedImage", new { id = savedImage.Id }, savedImage);
        }

        // DELETE: api/SavedImages/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSavedImage(int id)
        {
            var savedImage = await _context.SavedImages.FindAsync(id);
            if (savedImage == null)
            {
                return NotFound();
            }

            _context.SavedImages.Remove(savedImage);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SavedImageExists(int id)
        {
            return _context.SavedImages.Any(e => e.Id == id);
        }
    }
}
