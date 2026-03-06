using MocSaude.Models;
using MocSaude.Models.Schema;
using MocSaude.Presenters;

namespace MocSaude.Presenters
{
    public interface IDashboardView
    {
        String? SelectedTableName { get; }
        String? SelectedSchemaName { get; }
        String? SelectedGroupBy { get; }
        String? SelectedAggregate { get; }
        String? SelectedAggFunc { get; }
        List<String> SelectedColumns { get; }
        string? GlobalFilter { get; }

        event EventHandler OnTableChanged;
        event EventHandler OnLoadData;

        void SetTables(List<TableSchema> tables);
        void SetColumns(List<ColumnSchema> columns);
        void SetGroupByOptions(List<ColumnSchema> cols);
        void SetAggregateOptions(List<ColumnSchema> cols);
        void UpdateGrid(List<Dictionary<String, object>> rows);
        void UpdateChart(List<ChartPoint> points, String title);
        void SetLoading(Boolean loading);
        void ShowError(String message);
    }
}
