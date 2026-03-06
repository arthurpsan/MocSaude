using MocSaude.Models;
using MocSaude.Models.Schema;
using MocSaude.Repositories;

namespace MocSaude.Services
{
    public class DashboardService
    {
        private readonly DynamicQueryRepository _repo;
        public DashboardService(DynamicQueryRepository repo) => _repo = repo;

        public async Task<DashboardDataset> GetDatasetAsync(
                    TableSchema table,
                    string? groupByCol,
                    string? aggregateCol,
                    string? aggFunc,
                    string? filter = null)
        {
            var tableData = await _repo.QueryTableAsync(table, null, 200);

            var dataset = new DashboardDataset
            {
                TableData = tableData
            };

            // só executa agregação se os campos obrigatórios estiverem preenchidos
            if (!string.IsNullOrWhiteSpace(groupByCol) && !string.IsNullOrWhiteSpace(aggregateCol))
            {
                var chartData = await _repo.QueryAggregatedAsync(
                    table,
                    groupByCol,
                    aggregateCol,
                    aggFunc ?? "COUNT",
                    filter);

                dataset.ChartData = chartData.Select(c => new ChartPoint { Label = c.Label, Value = c.Value }).ToList();
            }

            return dataset;
        }
    }
}
