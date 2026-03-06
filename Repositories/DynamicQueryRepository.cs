using System;
using System.Text;
using System.Text.RegularExpressions;
using Dapper;
using MocSaude.Infraestructure;
using MocSaude.Models.Schema;

namespace MocSaude.Repositories
{
    public class DynamicQueryRepository
    {
        private readonly DatabaseConnection _db;
        public DynamicQueryRepository(DatabaseConnection db)
            => _db = db;

        // valida nomes de tabelas e colunas para evitar SQL Injection
        public static void Validate(String name)
        {
            if (!Regex.IsMatch(name, @"^[\w\s]+$"))
                throw new ArgumentException($"Invalid name: {name}");
        }

        // retorna linhas como lista de dicionários (coluna, valor)
        public async Task<List<Dictionary<string, object>>> QueryTableAsync(
            TableSchema table,
            List<string>? selectedColumns = null,
            int topN = 200)
        {
            var schemaName = table.SchemaName ?? "dbo";
            var tableName = table.TableName ?? "";

            Validate(schemaName);
            Validate(tableName);

            var cols = selectedColumns?.Any() == true
                ? string.Join(", ", selectedColumns.Select(c => { Validate(c); return $"[{c}]"; }))
                : "*";

            var sql = $"SELECT TOP {topN} {cols} FROM [{schemaName}].[{tableName}]";

            using var conn = _db.CreateConnection();
            var rows = await conn.QueryAsync(sql);

            return rows
                .Select(r => ((IDictionary<string, object>)r)
                    .ToDictionary(k => k.Key, k => k.Value))
                .ToList();
        }

        public async Task<List<(string Label, double Value)>> QueryAggregatedAsync(
            TableSchema table,
            string groupByCol,
            string aggregateCol,
            string aggFunc = "SUM")
        {
            var schemaName = table.SchemaName ?? "dbo";
            var tableName = table.TableName ?? "";

            Validate(schemaName);
            Validate(tableName);
            Validate(groupByCol);
            Validate(aggregateCol);

            var allowed = new[] { "SUM", "COUNT", "AVG", "MAX", "MIN" };
            if (!allowed.Contains(aggFunc.ToUpper()))
                throw new ArgumentException($"Funcao invalida: {aggFunc}");

            var sql = $@"
                SELECT TOP 50
                    [{groupByCol}]                        AS Label,
                    {aggFunc.ToUpper()}([{aggregateCol}]) AS Value
                FROM [{schemaName}].[{tableName}]
                WHERE [{groupByCol}] IS NOT NULL
                GROUP BY [{groupByCol}]
                ORDER BY Value DESC";

            using var conn = _db.CreateConnection();
            var rows = await conn.QueryAsync(sql);

            return rows.Select(r => {
                var d = (IDictionary<string, object>)r;
                string label = d["Label"]?.ToString() ?? "";
                double value = Convert.ToDouble(d["Value"] ?? 0);
                return (Label: label, Value: value);
            }).ToList();
        }


    }
}