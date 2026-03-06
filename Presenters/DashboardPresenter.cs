using Microsoft.EntityFrameworkCore.Metadata.Internal;
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

                var todasColunas = table.Columns;

                _view.SetGroupByOptions(table.Columns.ToList());
                _view.SetAggregateOptions(table.Columns.ToList());
            }
            catch (Exception ex) { _view.ShowError(ex.Message); }
            finally { _view.SetLoading(false); }
        }

        private async Task OnLoadDataAsync()
        {
            if (_view.SelectedTableName == null) return;

            _view.SetLoading(true);
            try
            {
                // busca o esquema da tabela atual
                var tableSchema = await _schema.GetTableWithColumnsAsync(
                    _view.SelectedTableName,
                    _view.SelectedSchemaName ?? "dbo");

                // chama o serviço passando o novo campo de filtro global
                var dashboardData = await _dashboard.GetDatasetAsync(
                    tableSchema,
                    _view.SelectedGroupBy,
                    _view.SelectedAggregate,
                    _view.SelectedAggFunc,
                    _view.GlobalFilter);

                // atualiza a interface
                _view.UpdateGrid(dashboardData.TableData);
                _view.UpdateChart(dashboardData.ChartData, $"Análise: {_view.SelectedAggregate} por {_view.SelectedGroupBy}");
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
