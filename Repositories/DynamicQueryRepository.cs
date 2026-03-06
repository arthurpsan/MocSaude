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
            if (!Regex.IsMatch(name, @"^[\p{L}\p{N}\s_]+$"))
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
            var rows = await conn.QueryAsync(sql, commandTimeout: 300);

            return rows
                .Select(r => ((IDictionary<string, object>)r)
                    .ToDictionary(k => k.Key, k => k.Value))
                .ToList();
        }

        public async Task<List<(String Label, Double Value)>> QueryAggregatedAsync(
            TableSchema table,
            String groupByCol,
            String aggregateCol,
            String aggFunc = "SUM",
            String? filter = null)
        {
            var schemaName = table.SchemaName ?? "dbo";
            var tableName = table.TableName ?? "";

            Validate(schemaName);
            Validate(tableName);
            Validate(groupByCol);
            Validate(aggregateCol);

            var allowed = new[] { "SUM", "COUNT", "AVG", "MAX", "MIN" };
            String func = aggFunc.ToUpper();

            if (!allowed.Contains(func))
                throw new ArgumentException($"Função inválida: {aggFunc}");

            var colSchema = table.Columns?.FirstOrDefault(c => c.ColumnName == aggregateCol);
            Boolean isNumeric = colSchema != null && colSchema.IsNumeric;

            if (!isNumeric && (func == "SUM" || func == "AVG"))
            {
                throw new InvalidOperationException($"Não é possível aplicar a função {func} na coluna '{aggregateCol}'. Escolha COUNT, MAX ou MIN para datas e textos.");
            }

            String aggregationExpression;
            if (func == "COUNT")
            {
                aggregationExpression = $"COUNT([{aggregateCol}])";
            }
            else if (func == "MAX" || func == "MIN")
            {
                aggregationExpression = $"{func}([{aggregateCol}])";
            }
            else
            {
                aggregationExpression = $"{func}(CAST([{aggregateCol}] AS FLOAT))";
            }

            string whereClause = !string.IsNullOrEmpty(filter)
                ? $"WHERE [{groupByCol}] IS NOT NULL AND ({filter})"
                : $"WHERE [{groupByCol}] IS NOT NULL";

            var sql = $@"
                SELECT TOP 50
                    [{groupByCol}] AS Label,
                    {aggregationExpression} AS Value
                FROM [{schemaName}].[{tableName}]
                {whereClause}
                GROUP BY [{groupByCol}]
                ORDER BY Value DESC";

            using var conn = _db.CreateConnection();
            var rows = await conn.QueryAsync(sql, commandTimeout: 300);

            return rows.Select(r =>
            {
                var d = (IDictionary<String, object>)r;
                String label = d["Label"]?.ToString() ?? "Não Informado";

                Double value = 0;
                var rawValue = d["Value"];

                if (rawValue != null && rawValue != DBNull.Value)
                {
                    if (rawValue is DateTime dt)
                        value = dt.ToOADate();
                    else
                        Double.TryParse(rawValue.ToString(), out value);
                }

                return (Label: label, Value: value);
            }).ToList();
        }
    }
}