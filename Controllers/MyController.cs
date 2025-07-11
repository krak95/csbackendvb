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

//LOGIN//


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

                var hashResult = _passwordHasher.VerifyHashedPassword(user, results[0].Password, user.Password);
                if (hashResult == PasswordVerificationResult.Success)
                {
                    return Ok(results[0]);
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
            catch (Exception ex)
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
                var results = await _context.Database.ExecuteSqlRawAsync("CALL updateLogin({0},{1},{2},{3},{4})",
                    log.Username,
                    token,
                    log.LogDate,
                    log.Role,
                    log.Admin
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
                     "CALL newLogin({0},{1},{2},{3},{4},{5})",
                     user.Username,
                     token,
                     user.Fullname,
                     user.LogDate,
                     user.Role,
                     user.Admin
                     );
                return Ok(token);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error" + ex });
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

//USERS//

        [HttpPost("fetchUsers")]
        public async Task<IActionResult> FetchUsers([FromBody] UsersFetch user)
        {
            //Console.WriteLine($"{equips.EquipName}");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);  // Return validation errors
            }

            try
            {
                var results = await _context.UsersFetchResults.FromSqlRaw(
                    "CALL fetchUsers({0})", user.Fullname).ToListAsync();
                return Ok(results); // Return success message
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error: " + ex.Message }); // Return detailed error
            }
        }
        [HttpPost("fetchRoles")]
        public async Task<IActionResult> FetchRoles([FromBody] Roles role)
        {
            //Console.WriteLine($"{equips.EquipName}");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);  // Return validation errors
            }

            try
            {
                var results = await _context.RolesResults.FromSqlRaw(
                    "CALL fetchRoles({0})", role.Rolename).ToListAsync();
                return Ok(results); // Return success message
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error: " + ex.Message }); // Return detailed error
            }
        }

//ADMIN//
        [HttpPost("assignRoles")]
        public async Task<IActionResult> AssignRoles([FromBody] Users user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                _ = await _context.Database.ExecuteSqlRawAsync(
                     "CALL assignRoles({0},{1})",
                     user.Fullname,
                     user.Role
                     );
                return Ok(new { message = "Role updated inserted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error" + ex.Message });
            }
        }

//EQUIPMENTS//

        [HttpPost("newEquipments")]
        public async Task<IActionResult> NewEquipments([FromBody] Equipments equip)
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
        public async Task<IActionResult> FetchEquipments([FromBody] Equipments equips)
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
        [HttpPost("delEquip")]
        public async Task<IActionResult> DeleteEquip([FromBody] Equipments equip)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _context.Database.ExecuteSqlRawAsync(
                    "CALL delEquip({0})",
                    equip.Idequipments
                    );

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error" + ex.Message });
            }
        }

//PROJECTS//
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
                    "CALL fetchProjects({0},{1},{2},{3})",
                    proj.Project,
                    proj.Country,
                    proj.Proj_manager,
                    proj.Client_name
                    ).ToListAsync();
                return Ok(results); // Return success message
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error: " + ex.Message }); // Return detailed error
            }
        }

        [HttpPost("deleteProject")]
        public async Task<IActionResult> DeleteProject([FromBody] Projects proj)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _context.Database.ExecuteSqlRawAsync(
                    "CALL deleteProject({0})",
                    proj.Id_proj
                    );

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error" + ex.Message });
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


//SO//
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

        [HttpPost("deleteSO")]
        public async Task<IActionResult> DeleteSO([FromBody] So so)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _context.Database.ExecuteSqlRawAsync(
                    "CALL delSO({0})",
                    so.IdSO
                    );

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error" + ex.Message });
            }
        }


//PRODUCTION//
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
                    "CALL checkProduction({0})",
                    prod.Id_prod).ToListAsync();
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
                    "CALL updateStatus({0},{1},{2})",
                    prod1.Id_prod,
                    prod1.Status,
                    prod1.EndDate);
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
        public async Task<IActionResult> DeleteProduction([FromBody] Production prod)
        {
            Console.WriteLine($"Deleting production with ID: {prod.Id_prod},{prod.Tester}"); // or use logging
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);  // Return validation errors
            }
            try
            {
                var result = await _context.Database.ExecuteSqlRawAsync(
                    "CALL delProd({0},{1})",
                    prod.Id_prod,
                    prod.Tester);

                
                return Ok(result); // Return success message
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error: " + ex.Message }); // Return detailed error
            }
        }

       

       
//ISSUES//
        [HttpPost("delIssues")]
        public async Task<IActionResult> DeleteEquip([FromBody] Issues issue)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _context.Database.ExecuteSqlRawAsync(
                    "CALL delIssues({0})",
                    issue.Id_issues
                    );

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error" + ex.Message });
            }
        }
       
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
                    "CALL fetchIssues({0},{1},{2})",
                    issue.Ref_issue,
                    issue.Description_issue,
                    issue.Level_issue
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
                     itemissue.Comment,
                     itemissue.Issue_status
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

        [HttpPost("updateItemIssues")]
        public async Task<IActionResult> UpdateItemIssue([FromBody] ItemIssues ii)
        {
            Console.WriteLine($"ID_ITEM:{ii.Id_item}");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _context.Database.ExecuteSqlRawAsync(
                    "CALL updateItemIssues({0},{1},{2})",
                    ii.Iditem_issues,
                    ii.Issue_status,
                    ii.Action
                    );

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error" + ex.Message });
            }
        }

        [HttpPost("deleteItemIssues")]
        public async Task<IActionResult> DeleteItemIssues([FromBody] ItemIssues ii)
        {
            Console.WriteLine($"ID_ITEM:{ii.Id_item}");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _context.Database.ExecuteSqlRawAsync(
                    "CALL deleteItemIssues({0})",
                    ii.Iditem_issues
                    );

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error" + ex.Message });
            }
        }

        

    }
}