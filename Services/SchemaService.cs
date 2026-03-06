using System;
using Dapper;
using MocSaude.Infraestructure;
using MocSaude.Models.Schema;
using MocSaude.Repositories;

namespace MocSaude.Services
{
    public class SchemaService
    {
        private readonly SchemaRepository _repo;
        private List<TableSchema>? _tablesCache;
        private readonly Dictionary<String, TableSchema> _colCache = new();

        public SchemaService(SchemaRepository repo) => _repo = repo;
        
        public async Task<List<TableSchema>> GetTablesAsync()
        {
            if (_tablesCache != null) return _tablesCache;
            _tablesCache = (await _repo.GetTablesAsync()).ToList();
            return _tablesCache;
        }

        public async Task<TableSchema> GetTableWithColumnsAsync(
            String tableName, String schema = "dbo")
        {
            var key = $"{schema}.{tableName}";
            if (_colCache.ContainsKey(key)) return _colCache[key];

            var tables = await GetTablesAsync();
            var table = tables.FirstOrDefault(t =>
                t.TableName == tableName && t.SchemaName == schema)
                ?? new TableSchema { TableName = tableName, SchemaName = schema };

            var cols = await _repo.GetColumnsAsync(tableName, schema);
            table.Columns = cols.ToList();
            _colCache[key] = table;
            return table;
        }

        public void InvalidateCache()
        {
            _tablesCache = null;
            _colCache.Clear();
        }
    }
}
