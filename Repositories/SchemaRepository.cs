using System;
using Dapper;
using MocSaude.Infraestructure;
using MocSaude.Models.Schema;

namespace MocSaude.Repositories
{
    public class SchemaRepository
    {
        private readonly DatabaseConnection _databaseConnection;
        public SchemaRepository(DatabaseConnection databaseConnection) => _databaseConnection = databaseConnection;

        public async Task<IEnumerable<TableSchema>> GetTablesAsync()
        {
            const string sql = @"
                SELECT 
                    TABLE_SCHEMA AS SchemaName,
                    TABLE_NAME   AS TableName
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_TYPE = 'BASE TABLE'
                ORDER BY TABLE_SCHEMA, TABLE_NAME";

            using var conn = _databaseConnection.CreateConnection();
            var results = await conn.QueryAsync(sql);

            return results.Select(r => new TableSchema
            {
                SchemaName = ((IDictionary<string, object>)r)["SchemaName"]?.ToString() ?? "dbo",
                TableName = ((IDictionary<string, object>)r)["TableName"]?.ToString() ?? ""
            });
        }

        public async Task<IEnumerable<ColumnSchema>> GetColumnsAsync(
            string tableName, string schemaName = "dbo")
        {
            const string sql = @"
                SELECT
                    c.COLUMN_NAME       AS ColumnName,
                    c.DATA_TYPE         AS DataType,
                    c.ORDINAL_POSITION  AS OrdinalPosition,
                    c.CHARACTER_MAXIMUM_LENGTH AS MaxLength,
                    CASE c.IS_NULLABLE WHEN 'YES' THEN 1 ELSE 0 END AS IsNullable,
                    CASE WHEN pk.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END AS IsPrimaryKey
                FROM INFORMATION_SCHEMA.COLUMNS c
                LEFT JOIN (
                    SELECT ku.COLUMN_NAME
                    FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                    JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE ku
                        ON tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME
                       AND tc.TABLE_NAME      = ku.TABLE_NAME
                    WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
                      AND tc.TABLE_NAME      = @TableName
                      AND tc.TABLE_SCHEMA    = @SchemaName
                ) pk ON c.COLUMN_NAME = pk.COLUMN_NAME
                WHERE c.TABLE_NAME   = @TableName
                  AND c.TABLE_SCHEMA = @SchemaName
                ORDER BY c.ORDINAL_POSITION";

            using var conn = _databaseConnection.CreateConnection();
            var results = await conn.QueryAsync(sql, new { TableName = tableName, SchemaName = schemaName });

            return results.Select(r =>
            {
                var d = (IDictionary<string, object>)r;
                return new ColumnSchema
                {
                    ColumnName = d["ColumnName"]?.ToString() ?? "",
                    DataType = d["DataType"]?.ToString() ?? "",
                    OrdinalPosition = Convert.ToInt32(d["OrdinalPosition"] ?? 0),
                    MaxLength = d["MaxLength"] == null ? null : Convert.ToInt32(d["MaxLength"]),
                    IsNullable = Convert.ToInt32(d["IsNullable"] ?? 0) == 1,
                    IsPrimaryKey = Convert.ToInt32(d["IsPrimaryKey"] ?? 0) == 1
                };
            });
        }
    }
}
