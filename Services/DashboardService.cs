using MocSaude.Models;
using MocSaude.Models.Schema;
using MocSaude.Repositories;

namespace MocSaude.Services
{
    public class DashboardService
    {
        private readonly DynamicQueryRepository _repo;

        public DashboardService(DynamicQueryRepository repo) => _repo = repo;

        public async Task<DashboardDataset> LoadAsync(
            TableSchema table,
            List<String> selectedColumns,
            String groupByCol,
            String aggregateCol,
            String aggFunc)
        {
            var rowsTask = _repo.QueryTableAsync(table, selectedColumns);
            var chartTask = _repo.QueryAggregatedAsync(table, groupByCol, aggregateCol, aggFunc);

            await Task.WhenAll(rowsTask, chartTask);

            return new DashboardDataset
            {
                Rows = rowsTask.Result,
                ChartData = chartTask.Result.Select(r => new ChartPoint { Label = r.Label, Value = r.Value }).ToList(),
                TableName = table.TableName,
                GroupBy = groupByCol,
                Aggregate = aggregateCol,
                AggFunc = aggFunc
            };
        }
    }
}
