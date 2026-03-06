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
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MocSaude.Forms
{
    public partial class Form1 : Form, IDashboardView
    {
        private DashboardPresenter _presenter;
        private TextBox txtFiltroGlobal;

        // -- IDashboard view propriedades processadas pelo presenter --
        public String? SelectedTableName
            => (cboTables.SelectedItem as TableSchema)?.TableName;
        public String? SelectedSchemaName
            => (cboTables.SelectedItem as TableSchema)?.SchemaName;
        public String? SelectedGroupBy
            => (cboGroupBy.SelectedItem as ColumnSchema)?.ColumnName
               ?? cboGroupBy.SelectedValue?.ToString();
        public String? SelectedAggregate
            => (cboAggregate.SelectedItem as ColumnSchema)?.ColumnName
               ?? cboAggregate.SelectedValue?.ToString();
        public String? GlobalFilter => string.IsNullOrWhiteSpace(txtFiltroGlobal?.Text) ? null : txtFiltroGlobal.Text;

        public String? SelectedAggFunc
        {
            get
            {
                var sel = cboAggFunc.SelectedItem?.ToString();
                return sel switch
                {
                    "Soma Total" => "SUM",
                    "Média" => "AVG",
                    "Contagem (Total de Casos)" => "COUNT",
                    "Valor Máximo" => "MAX",
                    "Valor Mínimo" => "MIN",
                    _ => "COUNT" // padrao mais "tranquilo" para saude
                };
            }
        }

        public List<String> SelectedColumns
            => clbColumns.CheckedItems.Cast<ColumnSchema>().Select(c => c.ColumnName).ToList();

        // -- eventos que o Presenter "escuta" --
        public event EventHandler? OnTableChanged;
        public event EventHandler? OnLoadData;

        public Form1()
        {
            InitializeComponent();
            AplicarDesign();

            WirePresenter(); // deve ser chamado ANTES de registrar eventos que usam _presenter

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

        private void cboTables_SelectedIndexChanged(object sender, EventArgs e)
            => OnTableChanged?.Invoke(this, EventArgs.Empty);

        private void btnLoad_Click(object sender, EventArgs e)
            => OnLoadData?.Invoke(this, EventArgs.Empty);

        public void SetTables(List<TableSchema> tables)
        {
            if (InvokeRequired) { Invoke(() => SetTables(tables)); return; }
            cboTables.DataSource = null;
            cboTables.DisplayMember = "";
            cboTables.DataSource = tables;
            cboTables.DisplayMember = "DisplayName";
            if (tables.Any())
                OnTableChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SetColumns(List<ColumnSchema> columns)
        {
            if (InvokeRequired) { Invoke(() => SetColumns(columns)); return; }

            clbColumns.Items.Clear();
            foreach (var col in columns)
                clbColumns.Items.Add(col, true); // adiciona já marcado
        }

        public void SetGroupByOptions(List<ColumnSchema> cols)
        {
            if (InvokeRequired) { Invoke(() => SetGroupByOptions(cols)); return; }
            cboGroupBy.DataSource = null;
            cboGroupBy.DataSource = cols;
            cboGroupBy.DisplayMember = "DisplayName";
            cboGroupBy.ValueMember = "ColumnName";
        }

        public void SetAggregateOptions(List<ColumnSchema> cols)
        {
            if (InvokeRequired) { Invoke(() => SetAggregateOptions(cols)); return; }
            cboAggregate.DataSource = null;
            cboAggregate.DataSource = cols;
            cboAggregate.DisplayMember = "DisplayName";
            cboAggregate.ValueMember = "ColumnName";
        }

        public void UpdateGrid(List<Dictionary<String, object>> rows)
        {
            if (InvokeRequired) { Invoke(() => UpdateGrid(rows)); return; }

            var dt = new System.Data.DataTable();
            if (rows.Any())
            {
                foreach (var colName in rows[0].Keys)
                {
                    var firstNonNull = rows.Select(r => r[colName]).FirstOrDefault(v => v != null);
                    if (firstNonNull is double || firstNonNull is float || firstNonNull is decimal)
                        dt.Columns.Add(colName, typeof(double));
                    else if (firstNonNull is int || firstNonNull is long)
                        dt.Columns.Add(colName, typeof(int));
                    else
                        dt.Columns.Add(colName, typeof(string));
                }

                foreach (var row in rows)
                {
                    var dr = dt.NewRow();
                    foreach (var kv in row)
                        dr[kv.Key] = kv.Value ?? DBNull.Value;
                    dt.Rows.Add(dr);
                }
            }

            dgvData.DataSource = dt;

            foreach (DataGridViewColumn col in dgvData.Columns)
            {
                if (col.ValueType == typeof(double) || col.ValueType == typeof(decimal))
                {
                    col.DefaultCellStyle.Format = "N2";
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
                else if (col.ValueType == typeof(int) || col.ValueType == typeof(long))
                {
                    col.DefaultCellStyle.Format = "N0";
                    col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                }
            }
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
            var myBars = plt.Add.Bars(values);

            plt.Axes.Bottom.SetTicks(
                Enumerable.Range(0, labels.Length).Select(i => (Double)i).ToArray(),
                labels);

            plt.Axes.Bottom.TickLabelStyle.Rotation = 45;
            plt.Axes.Bottom.TickLabelStyle.Alignment = ScottPlot.Alignment.MiddleLeft;

            plt.Axes.AutoScale();

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

        private void AplicarDesign()
        {
            // limpa o layout antigo se ainda existir
            if (tableLayoutPanel1 != null)
            {
                tableLayoutPanel1.Controls.Clear();
                this.Controls.Remove(tableLayoutPanel1);
            }

            // configura a janela principal
            this.Text = "MocSaúde - Dashboard Analítico";
            this.BackColor = Color.FromArgb(244, 246, 249);
            this.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular);
            this.WindowState = FormWindowState.Maximized;

            // cria o menu lateral escuro
            Panel pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 320,
                BackColor = Color.FromArgb(34, 45, 50),
                Padding = new Padding(20)
            };

            int currentY = 20;

            // funçao auxiliar para adicionar itens no menu
            void AddAoMenuLateral(string titulo, Control ctrl, int altura = 30)
            {
                ctrl.Dock = DockStyle.None;
                ctrl.Anchor = AnchorStyles.Top | AnchorStyles.Left;

                if (!string.IsNullOrEmpty(titulo))
                {
                    Label lbl = new Label
                    {
                        Text = titulo,
                        ForeColor = Color.FromArgb(180, 190, 200),
                        AutoSize = true,
                        Location = new Point(20, currentY),
                        Font = new Font("Segoe UI", 9F, FontStyle.Bold)
                    };
                    pnlSidebar.Controls.Add(lbl);
                    currentY += 22;
                }

                ctrl.Location = new Point(20, currentY);
                ctrl.Width = 280;
                if (altura > 0) ctrl.Height = altura;

                if (ctrl is ComboBox cb) { cb.FlatStyle = FlatStyle.Flat; cb.BackColor = Color.White; }
                if (ctrl is CheckedListBox clb) { clb.BorderStyle = BorderStyle.None; }

                pnlSidebar.Controls.Add(ctrl);
                currentY += ctrl.Height + 15;
            }

            // "traduz" as operações matemáticas
            cboAggFunc.Items.Clear();
            cboAggFunc.Items.AddRange(new object[] {
                "Contagem (Total de Casos)",
                "Soma Total",
                "Média",
                "Valor Máximo",
                "Valor Mínimo"
            });

            if (cboAggFunc.Items.Count > 0) cboAggFunc.SelectedIndex = 0;

            // adiciona os controles ao menu
            AddAoMenuLateral("1. CONJUNTO DE DADOS", cboTables);
            AddAoMenuLateral("2. DIMENSÃO (Ex: Mês, Especialidade, CID)", cboGroupBy);
            AddAoMenuLateral("3. MÉTRICA (Ex: Dias UTI, Óbitos)", cboAggregate);
            AddAoMenuLateral("4. TIPO DE ANÁLISE", cboAggFunc);
            AddAoMenuLateral("5. DETALHAMENTO DA TABELA", clbColumns, 150);

            txtFiltroGlobal = new TextBox { Width = 280, Height = 30 };
            AddAoMenuLateral("FILTRO AVANÇADO (Ex: ANO_CMPT = 2023)", txtFiltroGlobal, 30);

            // estiliza o botão principal
            btnLoad.Dock = DockStyle.None;
            btnLoad.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            btnLoad.FlatStyle = FlatStyle.Flat;
            btnLoad.BackColor = Color.FromArgb(0, 150, 136);
            btnLoad.ForeColor = Color.White;
            btnLoad.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnLoad.Cursor = Cursors.Hand;
            btnLoad.FlatAppearance.BorderSize = 0;
            btnLoad.Text = "Gerar Indicadores";
            AddAoMenuLateral("", btnLoad, 45);

            // configura indicadores de status
            lblStatus.Dock = DockStyle.None;
            lblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            lblStatus.ForeColor = Color.White;
            lblStatus.AutoSize = true;
            AddAoMenuLateral("STATUS DO SISTEMA:", lblStatus);

            progressBar.Dock = DockStyle.None;
            progressBar.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            AddAoMenuLateral("", progressBar, 10);

            // cria a área principal dividida
            SplitContainer splitMain = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 400,
                BackColor = Color.FromArgb(220, 220, 220)
            };

            // estiliza a tabela
            dgvData.Dock = DockStyle.None;
            dgvData.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            dgvData.BackgroundColor = Color.White;
            dgvData.BorderStyle = BorderStyle.None;
            dgvData.EnableHeadersVisualStyles = false;
            dgvData.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(235, 235, 235);
            dgvData.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            dgvData.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 248);
            dgvData.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvData.GridColor = Color.FromArgb(230, 230, 230);
            dgvData.Dock = DockStyle.Fill;

            // configura o gráfico
            formsPlot1.Dock = DockStyle.None;
            formsPlot1.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            formsPlot1.Dock = DockStyle.Fill;
            formsPlot1.Margin = new Padding(10);

            // monta a interface final
            splitMain.Panel1.Controls.Add(formsPlot1);
            splitMain.Panel2.Controls.Add(dgvData);

            this.Controls.Add(splitMain);
            this.Controls.Add(pnlSidebar);
        }
    }
}