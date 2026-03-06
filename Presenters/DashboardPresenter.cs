using MocSaude.Services;

namespace MocSaude.Presenters
{
    public class DashboardPresenter
    {
        private readonly IDashboardView _view;
        private readonly SchemaService _schema;
        private readonly DashboardService _dashboard;

        public DashboardPresenter(
            IDashboardView view,
            SchemaService schema,
            DashboardService dashboard)
        {
            _view = view;
            _schema = schema;
            _dashboard = dashboard;

            _view.OnTableChanged += async (s, e) => await OnTableChangedAsync();
            _view.OnLoadData += async (s, e) => await OnLoadDataAsync();
        }
        public async Task InitializeAsync()
        {
            _view.SetLoading(true);
            try
            {
                var tables = await _schema.GetTablesAsync();
                _view.SetTables(tables);
            }
            catch (Exception ex) { _view.ShowError(ex.Message); }
            finally { _view.SetLoading(false); }
        }
        private async Task OnTableChangedAsync()
        {
            if (_view.SelectedTableName == null) return;
            _view.SetLoading(true);
            try
            {
                var table = await _schema.GetTableWithColumnsAsync(
                    _view.SelectedTableName,
                    _view.SelectedSchemaName ?? "dbo");

                _view.SetColumns(table.Columns);
                _view.SetGroupByOptions(
                    table.Columns.Where(c => !c.IsNumeric).Select(c => c.ColumnName).ToList());
                _view.SetAggregateOptions(
                    table.Columns.Where(c => c.IsNumeric).Select(c => c.ColumnName).ToList());
            }
            catch (Exception ex) { _view.ShowError(ex.Message); }
            finally { _view.SetLoading(false); }
        }
        private async Task OnLoadDataAsync()
        {
            if (_view.SelectedTableName == null || 
                _view.SelectedGroupBy == null || 
                _view.SelectedAggregate == null) return;

            _view.SetLoading(true);

            try
            {
                var table = await _schema.GetTableWithColumnsAsync(
                    _view.SelectedTableName,
                    _view.SelectedSchemaName ?? "dbo");

                var dataset = await _dashboard.LoadAsync(
                    table,
                    _view.SelectedColumns,
                    _view.SelectedGroupBy,
                    _view.SelectedAggregate,
                    _view.SelectedAggFunc ?? "SUM");

                _view.UpdateGrid(dataset.Rows);
                _view.UpdateChart(dataset.ChartData,
                    $"{dataset.AggFunc}({dataset.Aggregate})) por {dataset.GroupBy}");
            }
            catch (Exception ex)
            {
                _view.ShowError(ex.Message);
            }
            finally 
            { 
                _view.SetLoading(false); 
            }
        }
    }
}
