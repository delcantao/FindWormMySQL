using MySql.Data.MySqlClient;

MySqlConnection conn;
const string fileResult = @"C:\Temp\Leandro_DBS.txt";
var save_in_file = new List<string>();
var i = 0;
var dbs = new List<string>()
{
    "gracac23_wp750",
    "gracac23_wp895",
    "gracac23_wpnutri",
    "gracac23_principal",
    "gracac23_wpfirst"
};
 
foreach (var db in dbs)
{
    if (db.Equals(string.Empty)) continue;

    var conn_string = new MySqlConnectionStringBuilder
    {
        Server = "50.116.112.145",
        UserID = "gracac23_LF",
        Password = Environment.GetEnvironmentVariable("MySQLPW"),
        Database = $"{db}"

    };
    conn = new MySqlConnection(conn_string.ToString());
    conn.Open();


    var tables = GetTableList();

    foreach (var table in tables)
    {
        Console.WriteLine($"{db} - {table} - {i++}");
        var fields = GetFields(table);
        save_in_file.AddRange(from field in fields let find = TryFindScriptInSpecificField(table, field) select db + (char)9 + table + (char)9 + field + (char)9 + find);
    }
}
File.WriteAllLines(fileResult, save_in_file);

IEnumerable<string> GetTableList()
{
    var tables = new List<string>();
    using var cmd = new MySqlCommand("SHOW TABLES", conn);
    var result = cmd.ExecuteReader();
    while (result.Read())
    {
        if (!result.IsDBNull(0))
            tables.Add(result[0].ToString() ?? "");
    }
    result.Close();
    return tables;
}

IEnumerable<string> GetFields(string tableName)
{
    var fields = new List<string>();
    using var cmd = new MySqlCommand($"describe {tableName}", conn);
    var result = cmd.ExecuteReader();
    while (result.Read())
    {
        if (IsText(result["Type"].ToString() ?? ""))
            fields.Add(result["Field"].ToString() ?? "");
    }
    result.Close();

    return fields;
}
bool TryFindScriptInSpecificField(string table, string field)
{
    var command = $"select true from `{table}` where `{field}` like '%classicpartnership%' limit 1;";
    var fields = new List<string>();
    using var cmd = new MySqlCommand(command, conn);
    var ors = cmd.ExecuteReader();
    var result = ors.HasRows;

    ors.Close();
    return result; 
}


bool IsText(string fieldType)
{
    fieldType = fieldType.ToUpper();

    return fieldType.Contains("CHAR") || fieldType.Contains("VARCHAR") || fieldType.Contains("BINARY") ||
           fieldType.Contains("VARBINARY") || fieldType.Contains("BLOB") || fieldType.Contains("TEXT") ||
           fieldType.Contains("ENUM") || fieldType.Contains("SET");
}