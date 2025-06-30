using Azure.Core;
using Azure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System;
using System.Data;
using webAPIreact.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static webAPIreact.Models.MyDBcontext;

namespace webAPIreact.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MyController : ControllerBase
    {

        private readonly IPasswordHasher<Users> _passwordHasher;
        //private readonly string _key = "this_is_my_very_long_secret_key_for_jwt_token";
        private readonly MyDBcontext _context;
        public MyController(MyDBcontext context, IPasswordHasher<Users> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout([FromBody] Login log)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                _ = await _context.Database.ExecuteSqlRawAsync(
                     "CALL logout({0})",
                     log.Username
                     );
                return Ok(new { message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error" + ex.Message });
            }
        }

        [HttpPost("Register")]
        public async Task<IActionResult> NewUsers([FromBody] Users user)
        {
            var hashedPw = _passwordHasher.HashPassword(user, user.Password);
            //Console.WriteLine($"{user.Fullname},{user.Admin},{user.Role},{user.Username},{hashedPw}");

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                _ = await _context.Database.ExecuteSqlRawAsync(
                     "CALL registerUser({0},{1},{2},{3},{4})",
                     user.Fullname,
                     user.Admin,
                     user.Role,
                     user.Username,
                     hashedPw
                     );
                return Ok(new { message = "User inserted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error" + ex.Message });
            }
        }

        [HttpPost("checkCreds")]
        public async Task<IActionResult> CheckCreds([FromBody] Users user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);  // Return validation errors
            }
            try
            {
                var results = await _context.UsersRegister.FromSqlRaw("CALL checkCreds({0},{1})",
                    user.Username,
                    user.Password
                    ).ToListAsync();

                //SqlDataReader reader = ExecuteReader();
                //while (reader.Read())
                //{
                //    for (int i = 0; i < count; i++)
                //    {
                //        Console.WriteLine(reader.GetValue(i));
                //    }
                //}

                var hashResult = _passwordHasher.VerifyHashedPassword(user, results[0].Password,user.Password);
                if (hashResult == PasswordVerificationResult.Success)
                {
                    return Ok(results[0].Fullname);
                }
                else
                {
                    return BadRequest(new { message = "Wrong credentials!" }); // Return detailed error
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error: " + ex.Message }); // Return detailed error
            }
        }

        [HttpPost("checkLogin")]
        public async Task<IActionResult> CheckLogin([FromBody] Login log)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);  // Return validation errors
            }
            try
            {
            var results = await _context.LoginResults.FromSqlRaw("CALL fetchLogin({0},{1})",
                log.Username,
                log.Token
                ).ToListAsync();
                 //Console.WriteLine($"{log.Username} {log.Token}");  
                return Ok(results); 
            }
            catch(Exception ex)
            {
                return BadRequest(new { message = "Error: " + ex.Message }); // Return detailed error
            }
        }

        [HttpPost("refreshLog")]
        public async Task<IActionResult> RefreshLog([FromBody] Login log)
        {
            var jwtKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWUsImlhdCI6MTUxNjIzOTAyMn0.KMUFsIDTnFmyG3nMiGM6H9FNFUROf3wh7SmqJp-QV30"; // Should be stored securely

            var token = JwtTokenHelper.GenerateToken(log.Username, jwtKey);
            //Console.WriteLine($"update: {log.Username}");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);  // Return validation errors
            }
            try
            {
                var results = await _context.Database.ExecuteSqlRawAsync("CALL updateLogin({0},{1},{2})",
                    log.Username,
                    token,
                    log.LogDate
                    );
                //Console.WriteLine($"{log.Username} {token}");
                return Ok(token);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error: " + ex.Message }); // Return detailed error
            }
        }

        [HttpPost("newLogin")]
        public async Task<IActionResult> NewLogin([FromBody] Login user)
        {
            var jwtKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWUsImlhdCI6MTUxNjIzOTAyMn0.KMUFsIDTnFmyG3nMiGM6H9FNFUROf3wh7SmqJp-QV30"; // Should be stored securely

            var token = JwtTokenHelper.GenerateToken(user.Username, jwtKey);
            //Console.WriteLine($"{user.LogDate}");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                _ = await _context.Database.ExecuteSqlRawAsync(
                     "CALL newLogin({0},{1},{2},{3})",
                     user.Username,
                     token,
                     user.Fullname,
                     user.LogDate
                     );
                return Ok(token);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error" + ex });
            }
        }
        [HttpPost("newEquipments")]
        public async Task<IActionResult> FetchEquipments([FromBody] Equipments equip)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);  // Return validation errors
            }
            try
            {
                _ = await _context.Database.ExecuteSqlRawAsync(
                    "CALL newEquip({0})",
                    equip.EquipName);

                return Ok(new { message = "item fetched" }); // ✅ Return success message
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error: " + ex.Message }); // Return detailed error
            }
        }

        [HttpPost("fetchEquipments")]
        public async Task<IActionResult> FetchProjects([FromBody] Equipments equips)
        {
            //Console.WriteLine($"{equips.EquipName}");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);  // Return validation errors
            }
            try
            {
                var results = await _context.EquipsResults.FromSqlRaw(
                    "CALL fetchEquipments({0})",equips.EquipName).ToListAsync();
                return Ok(results); // Return success message
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error: " + ex.Message }); // Return detailed error
            }
        }

        [HttpPost("fetchProjects")]
        public async Task<IActionResult> FetchProjects([FromBody] Projects proj)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);  // Return validation errors
            }
            try
            {
                var results = await _context.ProjectsResults.FromSqlRaw(
                    "CALL fetchProjects({0})",
                    proj.Project).ToListAsync();
                return Ok(results); // Return success message
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error: " + ex.Message }); // Return detailed error
            }
        }

        [HttpPost("fetchSO")]
        public async Task<IActionResult> FetchSO([FromBody] So so1)
        {
            //Console.WriteLine($"{so1.Project} {so1.SOref}");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);  // Return validation errors
            }
            try
            {
                var results = await _context.SoResults.FromSqlRaw(
                    "CALL fetchSO({0},{1})",
                    so1.Project,
                    so1.SOref).ToListAsync();
                //Console.WriteLine(results.ToArray());
                return Ok(results); // Return success message
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error: " + ex.Message }); // Return detailed error
            }
        }        

        [HttpPost("fetchProduction")]
        public async Task<IActionResult> FetchProduction([FromBody] Production prod)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);  // Return validation errors
            }
            try
            {
                var results = await _context.ProductionResults.FromSqlRaw(
                    "CALL fetchProduction({0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14})",
                    prod.Project,
                    prod.So,
                    prod.Equipment,
                    prod.CodeA,
                    prod.CodeB,
                    prod.CodePR,
                    prod.CodeDR,
                    prod.CodePS,
                    prod.Type0,
                    prod.Type1,
                    prod.Type2,
                    prod.Type3,
                    prod.Type4,
                    prod.Tester,
                    prod.Status).ToListAsync();
                return Ok(results); // ✅ Return success message
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error: " + ex.Message }); // Return detailed error
            }
        }

        [HttpPost("checkProduction")]
        public async Task<IActionResult> CheckProduction([FromBody] Production prod)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);  // Return validation errors
            }
            try
            {
                var results = await _context.ProductionResults.FromSqlRaw(
                    "CALL checkProduction({0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12})",
                    prod.Project,
                    prod.So,
                    prod.Equipment,
                    prod.CodeA,
                    prod.CodeB,
                    prod.CodePR,
                    prod.CodeDR,
                    prod.CodePS,
                    prod.Type0,
                    prod.Type1,
                    prod.Type2,
                    prod.Type3,
                    prod.Type4).ToListAsync();
                return Ok(results); // ✅ Return success message
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error: " + ex.Message }); // Return detailed error
            }
        }

        [HttpPost("updateStatus")]
        public async Task<IActionResult> UpdateStatus([FromBody] Production prod1)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);  // Return validation errors
            }
            try
            {
                _ = await _context.Database.ExecuteSqlRawAsync(
                    "CALL updateStatus({0},{1})",
                    prod1.Id_prod,
                    prod1.Status);
                return Ok(new { message = "Production inserted successfully" }); // Return success message
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error: " + ex.Message }); // Return detailed error
            }
        }

        [HttpPost("newProduction")]
        public async Task<IActionResult> NewProduction([FromBody] Production prod1)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);  // Return validation errors
            }
            try
            {
                _ = await _context.Database.ExecuteSqlRawAsync(
                    "CALL newItem({0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15})",
                    prod1.Project,
                    prod1.So,
                    prod1.Equipment,
                    prod1.CodeA,
                    prod1.CodeB,
                    prod1.CodePR,
                    prod1.CodeDR,
                    prod1.CodePS,
                    prod1.Type0,
                    prod1.Type1,
                    prod1.Type2,
                    prod1.Type3,
                    prod1.Type4,
                    prod1.Tester,
                    prod1.StartDate,
                    prod1.EndDate);
                return Ok(new { message = "Production inserted successfully" }); // Return success message
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error: " + ex.Message }); // Return detailed error
            }
        }

        [HttpPost("delProd")]
        public async Task<IActionResult> DeleteProduction([FromBody] int id_prod)
        {
            //Console.WriteLine($"Deleting production with ID: {id_prod}"); // or use logging
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);  // Return validation errors
            }
            try
            {
                _ = await _context.Database.ExecuteSqlRawAsync(
                    "CALL delProd({0})",
                    id_prod);
                return Ok(new { message = "Production deleted successfully" }); // Return success message
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error: " + ex.Message }); // Return detailed error
            }
        }

        [HttpPost("newProjects1")]
        public async Task<IActionResult> NewProjectC([FromBody] Projects proj)
        {

            if (!ModelState.IsValid)
            {
                //Console.WriteLine(ModelState);
                return BadRequest(ModelState);  // Return validation errors
            }
            try
            {
                //Console.WriteLine($"Proj:{proj}");
                _ = await _context.Database.ExecuteSqlRawAsync(
                    "CALL newProject({0},{1},{2},{3})",
                    proj.Project,
                    proj.Country,
                    proj.Proj_manager,
                    proj.Client_name
                    );
                return Ok(new { message = "Project inserted successfully" }); // Return success message
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error: " + ex.Message }); // Return detailed error
            }
        }

        [HttpPost("newSO")]
        public async Task<IActionResult> NewSO([FromBody] So so1)
        {
            //Console.WriteLine($"{so1.SOref} , {so1.Project}");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                _ = await _context.Database.ExecuteSqlRawAsync(
                     "CALL newSO({0},{1})",
                     so1.SOref,
                     so1.Project
                     );
                return Ok(new { message = "SO inserted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error" + ex.Message });
            }

        }

        //ISSUES//
        [HttpPost("newIssue")]
        public async Task<IActionResult> NewIssue([FromBody] Issues issue)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                _ = await _context.Database.ExecuteSqlRawAsync(
                     "CALL newIssue({0},{1},{2})",
                     issue.Ref_issue,
                     issue.Description_issue,
                     issue.Level_issue
                     );
                return Ok(new { message = "Issue inserted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error" + ex.Message });
            }
        }

        [HttpPost("fetchIssues")]
        public async Task<IActionResult> FetchIssues([FromBody] Issues issue)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _context.IssuesResults.FromSqlRaw(
                    "CALL fetchIssues({0},{1})",
                    issue.Ref_issue,
                    issue.Description_issue
                    ).ToListAsync();

                return Ok(result);
            }
            catch(Exception ex)
            {
                return BadRequest(new { message = "Error" + ex.Message });
            }
        }

        [HttpPost("addItemIssue")]
        public async Task<IActionResult> NewItemIssue([FromBody] ItemIssues itemissue)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                _ = await _context.Database.ExecuteSqlRawAsync(
                     "CALL addItemIssue({0},{1},{2})",
                     itemissue.Id_issue,
                     itemissue.Id_item,
                     itemissue.Comment
                     );
                return Ok(new { message = "Item Issue assigned successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error" + ex.Message });
            }
        }

        [HttpPost("fetchItemIssues")]
        public async Task<IActionResult> FetchItemIssues([FromBody] ItemIssues ii)
        {
            Console.WriteLine($"ID_ITEM:{ii.Id_item}");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _context.ItemIssuesResults.FromSqlRaw(
                    "CALL fetchItemIssues({0})",
                    ii.Id_item
                    ).ToListAsync();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error" + ex.Message });
            }
        }


    }
}