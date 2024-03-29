﻿using ACLAuthorization.Helper;
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
    public class RolesController : ControllerBase
    {
        private readonly Context _context;
        private readonly ILogger<RolesController> _logger;

        public RolesController(ILogger<RolesController> logger, Context context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: api/role
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoleResponse>>> GetRoles()
        {
            var roles = await _context.roles.Include(role => role.Permissions).ToListAsync();
            var response = roles.Select(permisstion => new RoleResponse(permisstion)).ToList();
            return response;
        }

        // GET: api/role/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RoleResponse>> GetRoles(int id)
        {
            var role = await _context.roles.Include(role => role.Permissions).Where(role => role.Id == id).FirstOrDefaultAsync();

            if (role == null)
            {
                return NotFound();
            }

            return new RoleResponse(role);
        }

        // PUT: api/role/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<ActionResult<RoleResponse>> PutRole(int id, RoleCreate createModel)
        {
            var role = createModel.convertToModel(_context);
            role.Id = id;

            _context.Entry(role).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new RoleResponse(role));
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

        // POST: api/role
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<RoleResponse>> PostRole(RoleCreate model)
        {
            try
            {
                if (_context.roles.Any(role => role.Name == model.Name))
                {
                    return BadRequest("Role with name is exist!");
                }
                else
                {
                    var role = model.convertToModel(_context);
                    _context.roles.Add(role);
                    await _context.SaveChangesAsync();

                    return CreatedAtAction(nameof(GetRoles), new { id = role.Id }, new RoleResponse(role));
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

        }

        // add permission
        [HttpPost("Permissions/{id}/Add")]
        public async Task<ActionResult<RoleResponse>> AddPermission(int id, [FromBody] int[] permissionIds)
        {
            var role = await _context.roles.Include(role => role.Permissions).Where(role => role.Id == id).FirstOrDefaultAsync();
            var existingPermissionIds = role.Permissions.Select(p => p.Id).ToList();
            if (role == null)
            {
                return NotFound($"Role with ID {id} not found");
            }

            foreach (var permissionId in permissionIds)
            {

                if (existingPermissionIds.Contains(permissionId))
                {
                    return BadRequest($"Permission with ID {permissionId} exist in role {role.Id}");
                }
                else
                {
                    var permission = await _context.permissions.FindAsync(permissionId);
                    if (permission == null)
                    {
                        return NotFound($"Permission with ID {permissionId} not found");
                    }
                    role.Permissions.Add(permission);
                }
            }
            await _context.SaveChangesAsync();

            return Ok(new RoleResponse(role));
        }

        // remove permission
        [HttpPost("Permissions/{id}/Remove")]
        public async Task<ActionResult<RoleResponse>> RemovePermission(int id, [FromBody] int[] permissionIds)
        {
            var role = await _context.roles.Include(role => role.Permissions).Where(role => role.Id == id).FirstOrDefaultAsync();
            if (role == null)
            {
                return NotFound($"Role with ID {id} not found");
            }

            var existingPermissionIds = role.Permissions.Select(p => p.Id).ToList();
            if (role == null)
            {
                return NotFound();
            }

            foreach (var permissionId in permissionIds)
            {

                if (!existingPermissionIds.Contains(permissionId))
                {
                    return BadRequest($"Permission with ID {permissionId} does not exist in role {role.Id}");
                }
                else
                {
                    var permission = await _context.permissions.FindAsync(permissionId);
                    if (permission == null)
                    {
                        return NotFound($"Permission with ID {permissionId} not found");
                    }
                    role.Permissions.Remove(permission);
                }
            }
            await _context.SaveChangesAsync();

            return Ok(new RoleResponse(role));
        }

        // DELETE: api/role/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await _context.roles.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            _context.roles.Remove(role);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool Exists(int id)
        {
            return _context.roles.Any(e => e.Id == id);
        }
    }
}
