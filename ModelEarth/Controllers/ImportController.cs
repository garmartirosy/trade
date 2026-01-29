using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Npgsql;
using Microsoft.Data.SqlClient;
using Dapper;


namespace ModelEarth.Controllers
{
    public class ImportController : BaseController
    {
        private readonly IConfiguration _config;

        public ImportController(IConfiguration config) 
        {
            _config = config;
        }


        // Get the list of tables in the database
        // Choose which table to upload data to
        // 
        [HttpGet]
        public async Task<IActionResult> Upload(string? ConnectionString)
        {

            if (ConnectionString == null)
            {
                ConnectionString = _config.GetConnectionString("DefaultConnection");
            }

            using var conn = new NpgsqlConnection(ConnectionString);
            var sql = "SELECT table_name FROM information_schema.tables where table_schema = 'public'";

            IEnumerable<string> Tables = await conn.QueryAsync<string>(sql);

            return View(Tables);

        }

        [HttpPost]
        public async Task<IActionResult> Upload(string tableName, IFormFile file, string? ConnectionString)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                return BadRequest("No table selected.");
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            ConnectionString ??= _config.GetConnectionString("DefaultConnection");

            await using var conn = new NpgsqlConnection(ConnectionString);
            await conn.OpenAsync();

            // ✅ Security: make sure tableName is a real table in public schema
            var tableExists = await conn.ExecuteScalarAsync<int>(
                @"SELECT COUNT(*) 
          FROM information_schema.tables 
          WHERE table_schema = 'public' AND table_name = @tableName",
                new { tableName });

            if (tableExists == 0)
                return BadRequest("Invalid table.");

            var copyCommand = $@"
        COPY public.""{tableName}""
        FROM STDIN (FORMAT csv, HEADER true)";

            await using var writer = await conn.BeginTextImportAsync(copyCommand);

            await using (var fileStream = file.OpenReadStream())
            using (var reader = new StreamReader(fileStream))
            {
                char[] buffer = new char[8192];
                int read;
                while ((read = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await writer.WriteAsync(buffer, 0, read);
                }
            }

            // finish COPY
            await writer.DisposeAsync();

            return Ok("CSV imported successfully.");
        }



        // [HttpPost]
        //
        // public async Task<IActionResult> Upload(string str)
        // {
        //     var connectionString = _config.GetConnectionString("DefaultConnection");
        //
        //     string sql = "insert into FormTextTest values ('" +str+ "');";
        //
        //     var conn = new NpgsqlConnection(connectionString);
        //
        //     conn.Execute(sql);
        //
        //     return View();
        //
        //
        //     // ImportController/Upload
        //     
        // }


        public IActionResult Upload1()
        {
            return View();
        }



    }
}


