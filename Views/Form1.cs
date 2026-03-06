using Dapper;
using Microsoft.Extensions;
using Microsoft.Extensions.Configuration;
using MocSaude.Infraestructure;
using MocSaude.Models;
using MocSaude.Models.Schema;
using MocSaude.Presenters;
using MocSaude.Repositories;
using MocSaude.Services;
using System;

namespace MocSaude.Forms
{
    public partial class Form1 : Form, IDashboardView
    {
        private DashboardPresenter _presenter;

        // -- IDashboard view propriedades processadas pelo presenter --

        public String? SelectedTableName
            => (cboTables.SelectedItem as TableSchema)?.TableName;
        public String? SelectedSchemaName
            => (cboTables.SelectedItem as TableSchema)?.SchemaName;
        public String? SelectedGroupBy
            => cboGroupBy.SelectedItem?.ToString();
        public String? SelectedAggregate
            => cboAggregate.SelectedItem?.ToString();
        public String? SelectedAggFunc
            => cboAggFunc.SelectedItem?.ToString() ?? "SUM";
        public List<String> SelectedColumns
            => clbColumns.CheckedItems.Cast<String>().ToList();

        // -- eventos que o Presenter "escuta" --
        public event EventHandler? OnTableChanged;
        public event EventHandler? OnLoadData;

        public Form1()
        {
            InitializeComponent();
            this.Load += async (s, e) => await _presenter.InitializeAsync();
            WirePresenter();

            cboTables.SelectedIndexChanged += cboTables_SelectedIndexChanged;
            btnLoad.Click += btnLoad_Click;
        }

        private void WirePresenter()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
            var connStr = config.GetConnectionString("SqlServer")!;

            var db = new DatabaseConnection(connStr);
            var schemaRepo = new SchemaRepository(db);
            var queryRepo = new DynamicQueryRepository(db);
            var schemaSvc = new SchemaService(schemaRepo);
            var dashboardSvc = new DashboardService(queryRepo);

            _presenter = new DashboardPresenter(this, schemaSvc, dashboardSvc);

        }

        private async void Form1_Load(object sender, EventArgs e)
            => await _presenter.InitializeAsync();

        // usuario selecionou uma tabela no ComboBox
        private void cboTables_SelectedIndexChanged(object sender, EventArgs e)
            => OnTableChanged?.Invoke(this, EventArgs.Empty);

        // usuario clicou no botão "Carregar Dados"
        private void btnLoad_Click(object sender, EventArgs e)
            => OnLoadData?.Invoke(this, EventArgs.Empty);

        // -- IDashboardView: métodos que o Presenter "chama" para atualizar a UI --
        public void SetTables(List<TableSchema> tables)
        {
            if (InvokeRequired) { Invoke(() => SetTables(tables)); return; }
            cboTables.DataSource = null;
            cboTables.DisplayMember = "";
            cboTables.DataSource = tables;
            cboTables.DisplayMember = "FullName";
            if (tables.Any())
                OnTableChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SetColumns(List<ColumnSchema> columns)
        {
            if (InvokeRequired) { Invoke(() => SetColumns(columns)); return; }
            clbColumns.Items.Clear();
            foreach (var column in columns)
                clbColumns.Items.Add(column.ColumnName, true);
        }

        public void SetGroupByOptions(List<String> cols)
        {
            if (InvokeRequired) { Invoke(() => SetGroupByOptions(cols)); return; }
            cboGroupBy.DataSource = null;
            cboGroupBy.DataSource = cols;
        }

        public void SetAggregateOptions(List<String> cols)
        {
            if (InvokeRequired) { Invoke(() => SetAggregateOptions(cols)); return; }
            cboAggregate.DataSource = null;
            cboAggregate.DataSource = cols;
        }

        public void UpdateGrid(List<Dictionary<String, object>> rows)
        {
            if (InvokeRequired) { Invoke(() => UpdateGrid(rows)); return; }
            var dt = new System.Data.DataTable();
            if (rows.Any())
            {
                foreach (var colName in rows[0].Keys)
                    dt.Columns.Add(colName);
                foreach (var row in rows)
                {
                    var dr = dt.NewRow();
                    foreach (var kv in row)
                        dr[kv.Key] = kv.Value ?? DBNull.Value;
                    dt.Rows.Add(dr);
                }
            }
            dgvData.DataSource = dt;
        }

        public void UpdateChart(List<ChartPoint> points, String title)
        {
            if (InvokeRequired) { Invoke(() => UpdateChart(points, title)); return; }
            var plt = formsPlot1.Plot;
            plt.Clear();
            plt.Title(title);
            if (!points.Any()) { formsPlot1.Refresh(); return; }

            var values = points.Select(p => p.Value).ToArray();
            var labels = points.Select(p => p.Label).ToArray();
            plt.Add.Bars(values);
            plt.Axes.Bottom.SetTicks(
                Enumerable.Range(0, labels.Length).Select(i => (Double)i).ToArray(),
                labels);
            formsPlot1.Refresh();
        }

        public void SetLoading(Boolean loading)
        {
            if (InvokeRequired) { Invoke(() => SetLoading(loading)); return; }
            progressBar.Visible = loading;
            btnLoad.Enabled = !loading;
            lblStatus.Text = loading ? "Carregando..." : "Pronto";
        }

        public void ShowError(String message)
        {
            if (InvokeRequired) { Invoke(() => ShowError(message)); return; }
            MessageBox.Show(message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
