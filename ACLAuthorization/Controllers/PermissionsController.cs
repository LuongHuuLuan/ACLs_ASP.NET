using ACLAuthorization.Helper;
using ACLAuthorization.Models;
using ACLAuthorization.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ACLAuthorization.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "ACLs")]
    public class PermissionsController : ControllerBase
    {
        private readonly Context _context;
        private readonly ILogger<PermissionsController> _logger;

        public PermissionsController(ILogger<PermissionsController> logger, Context context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: api/Status
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PermissionResponse>>> GetPermisstions()
        {
            var permisstions = await _context.permissions.ToListAsync();
            var response = permisstions.Select(permisstion => new PermissionResponse(permisstion)).ToList();
            return response;
        }

        // GET: api/Status/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PermissionResponse>> GetPermisstions(int id)
        {
            var permisstion = await _context.permissions.FindAsync(id);

            if (permisstion == null)
            {
                return NotFound();
            }

            return new PermissionResponse((permisstion));
        }

        // PUT: api/Status/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<ActionResult<PermissionResponse>> PutPermission(int id, PermissionCreate createModel)
        {
            var permission = createModel.convertToModel();
            permission.Id = id;

            _context.Entry(permission).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new PermissionResponse(permission));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Exists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // POST: api/permission
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<PermissionResponse>> PostPermission(PermissionCreate model)
        {
            try
            {
                if (_context.permissions.Any(permission => permission.Resource == model.Resource && permission.Method == MethodConverter.ConvertToMethod(model.Method)))
                {
                    return BadRequest("permission with method is exist!");
                }
                else
                {
                    var permisstion = model.convertToModel();
                    _context.permissions.Add(permisstion);
                    await _context.SaveChangesAsync();

                    return CreatedAtAction(nameof(GetPermisstions), new { id = permisstion.Id }, new PermissionResponse(permisstion));
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }

        // DELETE: api/Status/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePermission(int id)
        {
            var permission = await _context.permissions.FindAsync(id);
            if (permission == null)
            {
                return NotFound();
            }

            _context.permissions.Remove(permission);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool Exists(int id)
        {
            return _context.permissions.Any(e => e.Id == id);
        }
    }
}
