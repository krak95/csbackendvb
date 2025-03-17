using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using webAPIreact.Models;

namespace webAPIreact.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MyController : ControllerBase
    {
        private readonly MyDBcontext _context;
            public MyController(MyDBcontext context)
        {
            _context = context;
        }
        [HttpGet("getUsers")]
        public async Task<ActionResult<IEnumerable<Users>>> GetMyProduction()
        {
            return await _context.Users
                                 .FromSqlRaw("CALL fetchUsers") // Call the stored procedure
                                 .ToListAsync();
        }

        [HttpGet("getUser")]
        public async Task<ActionResult<IEnumerable<Users>>>GetMyUsers()
        {
            return await _context.Users.ToListAsync();
        }
        [HttpPost]
        public async Task<ActionResult<Production>>PostMyEntity(Production myProd)
        {
            _context.Production.Add(myProd);
            await
            _context.SaveChangesAsync();
            return CreatedAtAction("getProduction", new {id_prod=myProd.Id_prod}, myProd);
        }

    }
}