using System;
using System.IO;
using Npgsql;

Console.WriteLine("🔧 Running database migrations...\n");

var connString = "Host=localhost;Database=exiobase;Username=postgres;Password=password;Port=5432";

try
{
    await using var conn = new NpgsqlConnection(connString);
    await conn.OpenAsync();
    Console.WriteLine("✅ Connected to database: exiobase\n");

    // Read and execute migration files
    var scriptFiles = new[]
    {
        @"..\IndustryDB\DB Scripts\Postgres\001_CreateTradeTable.sql",
        @"..\IndustryDB\DB Scripts\Postgres\002_CreateAdditionalTables.sql"
    };

    foreach (var scriptFile in scriptFiles)
    {
        if (!File.Exists(scriptFile))
        {
            Console.WriteLine($"❌ Script not found: {scriptFile}");
            continue;
        }

        Console.WriteLine($"📄 Executing: {Path.GetFileName(scriptFile)}");

        var sql = await File.ReadAllTextAsync(scriptFile);

        await using var cmd = new NpgsqlCommand(sql, conn);
        await cmd.ExecuteNonQueryAsync();

        Console.WriteLine($"   ✅ Completed\n");
    }

    // Verify tables were created
    Console.WriteLine("📊 Verifying tables...");
    var verifySQL = @"
        SELECT tablename
        FROM pg_tables
        WHERE schemaname = 'public'
        ORDER BY tablename;
    ";

    await using var verifyCmd = new NpgsqlCommand(verifySQL, conn);
    await using var reader = await verifyCmd.ExecuteReaderAsync();

    Console.WriteLine("\nCreated tables:");
    while (await reader.ReadAsync())
    {
        Console.WriteLine($"  ✓ {reader.GetString(0)}");
    }

    Console.WriteLine("\n✅ All migrations completed successfully!");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
    Environment.Exit(1);
}
