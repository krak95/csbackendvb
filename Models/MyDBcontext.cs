using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace webAPIreact.Models
{
    public class MyDBcontext : DbContext
    {

        public MyDBcontext(DbContextOptions<MyDBcontext> options) : base(options)
        {
        }

        public DbSet<Production> ProductionResults { get; set; }
        public DbSet<FetchProjectsFromProduction> FetchProjectsFromProductionsResults { get; set; }
        public DbSet<Projects> ProjectsResults { get; set; }
        public DbSet<So> SoResults { get; set; }
        public DbSet<Equipments> EquipsResults { get; set; }
        public DbSet<Users> UsersRegister { get; set; }
        public DbSet<Login> LoginResults { get; set; } 
        public DbSet<Issues> IssuesResults { get; set; }
        public DbSet<ItemIssues> ItemIssuesResults { get; set; }
        public DbSet<UsersFetch> UsersFetchResults{ get; set; }
        public DbSet<Roles> RolesResults { get; set; }
        public DbSet<Jobs> JobsResults { get; set; }
        public DbSet<WorkWeeks> WorkWeeksResults { get; set; }
        public DbSet<WorkWeeksNR> WorkWeeksNRResults { get; set; }

}

    //MYSQL TABLES
    [Table("Login")]
    public class Login
    {
        [Key]
        public int Id_login { get; set; }
        public string? Username { get; set; } 
        public string? Token { get; set; }
        public string? Fullname { get; set; }
        public string? LogDate { get; set; }
        public string? Role { get; set; }
        public int? Admin { get; set; }
    }

    [Table("Roles")]
    public class Roles
    {
        [Key]
        public int Id_roles { get; set; }
        public string? Rolename  { get; set; }
    }

    [Table("Production")]
        public class Production
        {
            [Key]
            public int Id_prod { get; set; }
            public string? Project { get; set; }
            public string? So { get; set; }
            public string? Equipment { get; set; }
            public string? CodeA { get; set; }
            public string? CodeB { get; set; }
            public string? CodePR { get; set; }
            public string? CodeDR { get; set; }
            public string? CodePS { get; set; }
            public string? Type0 { get; set; }
            public string? Type1 { get; set; }
            public string? Type2 { get; set; }
            public string? Type3 { get; set; }
            public string? Type4 { get; set; }
            public string? ReadyPQA { get; set; }
            public string? Tester { get; set; }
            public string? StartDate { get; set; }
            public string? EndDate { get; set; }
            public string? Status { get; set; }
        public string? Ww_number  { get; set; }
        public string? HipotModel { get; set; }
        public string? HipotValue { get; set; }
        public string? HipotMultimeterModel { get; set; }
        public string? Comment { get; set; }
        public string? ChecklistStatus { get; set; }
        public string? TraceabilityStatus { get; set; }

        public string? DeploymentStatus { get; set; }
    }
    
    public class FetchProjectsFromProduction
    {
        [Key]
        public string? Project { get; set; }
        public string? Ww_number { get; set; }
    }

    [Table("Users")]
    public class Users
    {
        [Key]
        public int Idusers { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Fullname { get; set; }
        public int? Admin { get; set; }
        public string? Role { get; set; }
    }
    public class UsersFetch
    {
        [Key]
        public int Idusers { get; set; }
        public string? Fullname { get; set; }
        public int? Admin { get; set; }
        public string? Role { get; set; }
    }

    [Table("Projects")]
    public class Projects
    {
    [Key]
    public int Id_proj { get; set; }
    public string? Project { get; set; }
    public string? Country { get; set; }
    public string? Proj_manager { get; set; }
    public string? Client_name { get; set; }
    }

    [Table("So")]
    public class So
    {
        [Key]
        public int IdSO { get; set; }
        public string? SOref { get; set; }
        public string? Project { get; set; }
    }

    [Table("Equipments")]
    public class Equipments
    {
        [Key]
        public int Idequipments { get; set; }
        public string? EquipName { get; set; }
    }

    [Table("Issues")]
    public class Issues
    {
        [Key]
        public int Id_issues { get; set; }
        public string? Ref_issue { get; set; }
        public string? Description_issue { get; set; }
        public string? Level_issue  { get; set; }   
    }

    [Table("item_issues")]
    public class ItemIssues
    {
        [Key]
        public int Iditem_issues { get; set; }
        public int? Id_issue { get; set; }
        public int? Id_item { get; set; }
        public string? Comment { get;set; }
        public string? Issue_status { get; set; }  
        public string? Ref_issue { get; set; }
        public string? Description_issue { get;set; }
        public string? Level_issue { get; set; }
        public string? Action { get; set; } 

    }

    [Table("workweeks")]
    public class WorkWeeks
    {
        [Key]
        public int Idworkweeks { get; set; }
        public string? Ww_number { get; set; }
        public string? Equipment {  get; set; } 
        public string? Quantity_need { get; set; }
        public string? Project {  get; set; }   
    }

    public class WorkWeeksNR
    {
        [Key]
 
        public string? Ww_number { get; set; }
    }

    public class WorkWeeksProject
    {
        [Key]
        public string? Project { get; set; }
        public string? Ww_number { get; set; }
    }
    //JOBS//

    [Table("jobs")]
    public class Jobs
    {
        [Key]
        public int Idjobs { get; set; }
        public string? Equipment{ get; set; }
        public string? Model { get; set; }
        public string? QuantityDone { get; set; }
        public string? QuantityNeed { get; set; }
        public string? JobNumber { get; set; }
        public string? JobProject { get; set; }
        public int? JobDone { get; set; }    
    }

}


// JWT TOKEN
public static class JwtTokenHelper
{
    public static string GenerateToken(string username, string key)
    {
        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var keyBytes = Encoding.UTF8.GetBytes(key);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.Name, username)
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(keyBytes),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}