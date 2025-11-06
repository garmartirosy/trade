using Microsoft.AspNetCore.Mvc;
using ModelEarth.Models;
using System.Data;
using System.Linq;
using Microsoft.Data.SqlClient;   // SqlServer provider
using Npgsql;                     // Postgres provider
using System.Text.RegularExpressions;
using ModelEarth.Models.Data;
using Dapper;

namespace ModelEarth.Controllers
{
    // Inherit from your BaseController (which contains cookie helpers)
    public class DataController : BaseController
    {
        private readonly ILogger<DataController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;


        public DataController(ILogger<DataController> logger, IWebHostEnvironment env, IConfiguration config)

        {
            _env = env;
            _logger = logger;
            _config = config;

        }

        [HttpGet]
        public IActionResult TradeView()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TradeView(TradeSearch tradeSearch)
        {
            string? connString = _config.GetConnectionString("DefaultConnection");

            const string sql = @"
        select trade_id, year, region1, region2, industry1, industry2, amount
        from public.get_trade(
            @p_year,
            @p_region1,
            @p_region2,
            null,    -- p_industry1
            null,    -- p_industry2
            null,    -- p_amount_min
            null     -- p_amount_max
        );";

            await using var conn = new Npgsql.NpgsqlConnection(connString);
            await conn.OpenAsync();   

            var dp = new Dapper.DynamicParameters();

            dp.Add("p_year", tradeSearch.Years);       // int[] or null
            dp.Add("p_region1", tradeSearch.Region1);  // string[] or null
            dp.Add("p_region2", tradeSearch.Region2);  // string[] or null

            var rows = await conn.QueryAsync<Trade>(
                new CommandDefinition(sql, dp));       

            return View(rows); 
        }

    }
}
