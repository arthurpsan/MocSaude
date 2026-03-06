using Microsoft.Extensions.Configuration;
using MocSaude.Infraestructure;
using MocSaude.Repositories;
using ScottPlot;
using System.Drawing.Drawing2D;

namespace MocSaude.Forms
{
    public class DashboardFixoForm : Form
    {
        // ── repositório ──────────────────────────────────────────────────
        private KpiRepository _kpiRepo = null!;

        // ── filtros ──────────────────────────────────────────────────────
        private ComboBox cboAno  = new();
        private ComboBox cboMes  = new();
        private Button   btnAtualizar = new();
        private Label    lblStatus    = new();
        private ProgressBar progress  = new();

        // ── KPI cards (painel superior) ──────────────────────────────────
        private Panel pnlKpis = new();

        // ── gráficos ─────────────────────────────────────────────────────
        private ScottPlot.WinForms.FormsPlot plotMensal     = new();
        private ScottPlot.WinForms.FormsPlot plotModalidade = new();
        private ScottPlot.WinForms.FormsPlot plotTop5       = new();

        // ── tabela ────────────────────────────────────────────────────────
        private DataGridView dgv = new();

        // ── cores do tema ────────────────────────────────────────────────
        private static readonly Color CInternacao = Color.FromArgb(33, 150, 243);
        private static readonly Color CObito      = Color.FromArgb(244, 67,  54);
        private static readonly Color CDias       = Color.FromArgb(0,  150, 136);
        private static readonly Color CUti        = Color.FromArgb(156, 39, 176);
        private static readonly Color CValor      = Color.FromArgb(255, 152,  0);
        private static readonly Color CSidebar    = Color.FromArgb(28,  37,  46);
        private static readonly Color CBg         = Color.FromArgb(240, 242, 245);

        // ─────────────────────────────────────────────────────────────────
        public DashboardFixoForm()
        {
            InitializarUI();
            WireRepo();
            this.Load += async (s, e) => await CarregarTudoAsync();
        }

        // ── fiação do repositório ─────────────────────────────────────────
        private void WireRepo()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
            var connStr = config.GetConnectionString("SqlServer")!;
            _kpiRepo = new KpiRepository(new DatabaseConnection(connStr));
        }

        // ── carrega dados e atualiza toda a tela ──────────────────────────
        private async Task CarregarTudoAsync()
        {
            SetLoading(true);
            try
            {
                // popula anos disponíveis na primeira carga
                if (cboAno.Items.Count == 0)
                {
                    var anos = await _kpiRepo.GetAnosDisponiveisAsync();
                    cboAno.Items.Add("Todos os anos");
                    foreach (var a in anos) cboAno.Items.Add(a);
                    cboAno.SelectedIndex = 0;
                }

                var ano = cboAno.SelectedItem is int a2 ? (int?)a2 : null;
                var mes = cboMes.SelectedItem is int m  ? (int?)m  : null;

                // carrega em paralelo
                var tSummary    = _kpiRepo.GetSummaryAsync(ano, mes);
                var tTop5       = _kpiRepo.GetTop5DoencasAsync(ano, mes);
                var tMensal     = _kpiRepo.GetInternacoesPorMesAsync(ano);
                var tModalidade = _kpiRepo.GetModalidadeInternacaoAsync(ano, mes);

                await Task.WhenAll(tSummary, tTop5, tMensal, tModalidade);

                AtualizarKpis(tSummary.Result);
                AtualizarGraficoMensal(tMensal.Result);
                AtualizarGraficoModalidade(tModalidade.Result);
                AtualizarGraficoTop5(tTop5.Result);
                AtualizarTabela(tTop5.Result);

                lblStatus.Text = $"Atualizado em {DateTime.Now:HH:mm:ss}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Erro ao carregar dados",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Erro ao carregar.";
            }
            finally { SetLoading(false); }
        }

        // ── KPI cards ─────────────────────────────────────────────────────
        private void AtualizarKpis(KpiSummary s)
        {
            if (InvokeRequired) { Invoke(() => AtualizarKpis(s)); return; }

            pnlKpis.Controls.Clear();

            var cards = new[]
            {
                ("🏥 Internações",           s.TotalInternacoes.ToString("N0"),        CInternacao),
                ("💀 Óbitos",                s.TotalObitos.ToString("N0"),             CObito),
                ("📅 Média Dias Internado",  s.MediaDiasPerm.ToString("N1") + " dias", CDias),
                ("🏨 Média Dias UTI",        s.MediaDiasUti.ToString("N1")  + " dias", CUti),
                ("💰 Valor Total Gasto",     "R$ " + s.ValorTotalGasto.ToString("N0"), CValor),
                ("📊 Taxa de Mortalidade",   s.TotalInternacoes > 0
                                                ? ((double)s.TotalObitos / s.TotalInternacoes * 100).ToString("N2") + "%"
                                                : "—",                                 CObito),
            };

            int cardW = (pnlKpis.Width - 20) / cards.Length - 8;

            for (int i = 0; i < cards.Length; i++)
            {
                var (titulo, valor, cor) = cards[i];
                var card = CriarKpiCard(titulo, valor, cor, cardW);
                card.Location = new Point(10 + i * (cardW + 8), 10);
                pnlKpis.Controls.Add(card);
            }
        }

        private Panel CriarKpiCard(string titulo, string valor, Color cor, int largura)
        {
            var pnl = new Panel
            {
                Size      = new Size(largura, 80),
                BackColor = Color.White,
                Cursor    = Cursors.Default
            };

            // barra colorida no topo
            var barra = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 5,
                BackColor = cor
            };

            var lblTitulo = new Label
            {
                Text      = titulo,
                ForeColor = Color.FromArgb(100, 110, 120),
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                AutoSize  = false,
                Width     = largura - 16,
                Height    = 22,
                Location  = new Point(8, 12),
                TextAlign = ContentAlignment.MiddleLeft
            };

            var lblValor = new Label
            {
                Text      = valor,
                ForeColor = cor,
                Font      = new Font("Segoe UI", 14f, FontStyle.Bold),
                AutoSize  = false,
                Width     = largura - 16,
                Height    = 34,
                Location  = new Point(8, 34),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // sombra leve via Paint
            pnl.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(20, 0, 0, 0), 1);
                e.Graphics.DrawRectangle(pen, 0, 0, pnl.Width - 1, pnl.Height - 1);
            };

            pnl.Controls.AddRange(new Control[] { barra, lblTitulo, lblValor });
            return pnl;
        }

        // ── gráfico de barras: internações por mês ────────────────────────
        private void AtualizarGraficoMensal(List<(string Label, long Contagem)> dados)
        {
            if (InvokeRequired) { Invoke(() => AtualizarGraficoMensal(dados)); return; }

            var plt = plotMensal.Plot;
            plt.Clear();
            plt.Title("Internações por Mês");
            plt.Style.Background(figure: Color.White, data: Color.FromArgb(250, 250, 250));

            if (!dados.Any()) { plotMensal.Refresh(); return; }

            var values = dados.Select(d => (double)d.Contagem).ToArray();
            var bars   = plt.Add.Bars(values);
            bars.Color = new ScottPlot.Color(CInternacao.R, CInternacao.G, CInternacao.B);

            plt.Axes.Bottom.SetTicks(
                Enumerable.Range(0, dados.Count).Select(i => (double)i).ToArray(),
                dados.Select(d => d.Label).ToArray());

            plt.Axes.Bottom.TickLabelStyle.FontSize = 10;
            plt.Axes.Left.Label.Text  = "Internações";
            plt.Axes.AutoScale();
            plotMensal.Refresh();
        }

        // ── gráfico de pizza: modalidade ──────────────────────────────────
        private void AtualizarGraficoModalidade(List<(string Label, long Contagem)> dados)
        {
            if (InvokeRequired) { Invoke(() => AtualizarGraficoModalidade(dados)); return; }

            var plt = plotModalidade.Plot;
            plt.Clear();
            plt.Title("Modalidade de Internação");
            plt.Style.Background(figure: Color.White, data: Color.FromArgb(250, 250, 250));

            if (!dados.Any()) { plotModalidade.Refresh(); return; }

            var slices = dados.Select((d, i) =>
            {
                var cores = new[] {
                    new ScottPlot.Color(CInternacao.R, CInternacao.G, CInternacao.B),
                    new ScottPlot.Color(CObito.R,      CObito.G,      CObito.B),
                    new ScottPlot.Color(CDias.R,       CDias.G,       CDias.B),
                    new ScottPlot.Color(CUti.R,        CUti.G,        CUti.B),
                    new ScottPlot.Color(CValor.R,      CValor.G,      CValor.B),
                };
                return new ScottPlot.PieSlice
                {
                    Value       = d.Contagem,
                    Label       = d.Label,
                    FillColor   = cores[i % cores.Length],
                    LabelStyle  = { FontSize = 10 }
                };
            }).ToList();

            var pie = plt.Add.Pie(slices);
            pie.ExplodeFraction   = 0.05;
            pie.SliceLabelDistance = 1.4;
            plt.ShowLegend(Edge.Right);
            plt.Axes.AutoScale();
            plotModalidade.Refresh();
        }

        // ── gráfico de barras horizontal: top 5 doenças ───────────────────
        private void AtualizarGraficoTop5(List<(string Label, long Contagem)> dados)
        {
            if (InvokeRequired) { Invoke(() => AtualizarGraficoTop5(dados)); return; }

            var plt = plotTop5.Plot;
            plt.Clear();
            plt.Title("Top 5 Diagnósticos (CID)");
            plt.Style.Background(figure: Color.White, data: Color.FromArgb(250, 250, 250));

            if (!dados.Any()) { plotTop5.Refresh(); return; }

            var values = dados.Select(d => (double)d.Contagem).ToArray();
            var bars   = plt.Add.Bars(values);
            bars.Color = new ScottPlot.Color(CUti.R, CUti.G, CUti.B);

            plt.Axes.Bottom.SetTicks(
                Enumerable.Range(0, dados.Count).Select(i => (double)i).ToArray(),
                dados.Select(d => d.Label).ToArray());

            plt.Axes.Bottom.TickLabelStyle.FontSize = 10;
            plt.Axes.Left.Label.Text = "Casos";
            plt.Axes.AutoScale();
            plotTop5.Refresh();
        }

        // ── tabela inteligente ────────────────────────────────────────────
        private void AtualizarTabela(List<(string Label, long Contagem)> top5)
        {
            if (InvokeRequired) { Invoke(() => AtualizarTabela(top5)); return; }

            var dt = new System.Data.DataTable();
            dt.Columns.Add("Posição",    typeof(string));
            dt.Columns.Add("CID",        typeof(string));
            dt.Columns.Add("Casos",      typeof(long));
            dt.Columns.Add("% do Total", typeof(string));

            long total = top5.Sum(x => x.Contagem);
            int pos = 1;
            foreach (var (label, contagem) in top5)
            {
                double pct = total > 0 ? (double)contagem / total * 100 : 0;
                dt.Rows.Add(
                    pos == 1 ? "🥇 1º" : pos == 2 ? "🥈 2º" : pos == 3 ? "🥉 3º" : $"  {pos}º",
                    label,
                    contagem,
                    pct.ToString("N1") + "%"
                );
                pos++;
            }

            dgv.DataSource = dt;

            // formatação das colunas
            dgv.Columns["Posição"].Width    = 70;
            dgv.Columns["CID"].Width        = 90;
            dgv.Columns["Casos"].Width      = 100;
            dgv.Columns["% do Total"].Width = 90;

            dgv.Columns["Casos"].DefaultCellStyle.Format    = "N0";
            dgv.Columns["Casos"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgv.Columns["% do Total"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }

        // ── loading ───────────────────────────────────────────────────────
        private void SetLoading(bool loading)
        {
            if (InvokeRequired) { Invoke(() => SetLoading(loading)); return; }
            progress.Visible      = loading;
            btnAtualizar.Enabled  = !loading;
            lblStatus.Text        = loading ? "⏳ Carregando dados..." : lblStatus.Text;
        }

        // ── construção da UI ──────────────────────────────────────────────
        private void InitializarUI()
        {
            this.Text          = "MocSaúde — Painel de Indicadores";
            this.BackColor     = CBg;
            this.Font          = new Font("Segoe UI", 9.5f);
            this.WindowState   = FormWindowState.Maximized;
            this.MinimumSize   = new Size(1100, 700);

            // ── sidebar ──────────────────────────────────────────────────
            var sidebar = new Panel
            {
                Dock      = DockStyle.Left,
                Width     = 240,
                BackColor = CSidebar,
                Padding   = new Padding(16)
            };

            var lblTitulo = new Label
            {
                Text      = "🏥 MocSaúde",
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 15f, FontStyle.Bold),
                AutoSize  = false,
                Width     = 208,
                Height    = 40,
                Location  = new Point(16, 20)
            };

            var lblSubtitulo = new Label
            {
                Text      = "Painel de Indicadores de Saúde",
                ForeColor = Color.FromArgb(140, 160, 180),
                Font      = new Font("Segoe UI", 8.5f),
                AutoSize  = false,
                Width     = 208,
                Height    = 30,
                Location  = new Point(16, 60)
            };

            var sep1 = new Panel
            {
                BackColor = Color.FromArgb(50, 60, 70),
                Location  = new Point(16, 98),
                Size      = new Size(208, 1)
            };

            // filtro Ano
            var lblAno = MakeSideLabel("FILTRO: ANO", 110);
            cboAno = MakeSideCombo(136);
            cboAno.Items.Add("Todos os anos");
            cboAno.SelectedIndex = 0;

            // filtro Mês
            var lblMes = MakeSideLabel("FILTRO: MÊS", 190);
            cboMes = MakeSideCombo(216);
            cboMes.Items.Add("Todos os meses");
            var meses = new[] { "Janeiro","Fevereiro","Março","Abril","Maio","Junho",
                                "Julho","Agosto","Setembro","Outubro","Novembro","Dezembro" };
            for (int i = 0; i < meses.Length; i++)
                cboMes.Items.Add(new MesItem { Numero = i + 1, Nome = meses[i] });
            cboMes.SelectedIndex = 0;

            // botão atualizar
            btnAtualizar = new Button
            {
                Text      = "▶  Atualizar Painel",
                Location  = new Point(16, 272),
                Size      = new Size(208, 42),
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            btnAtualizar.FlatAppearance.BorderSize = 0;
            btnAtualizar.Click += async (s, e) => await CarregarTudoAsync();

            var sep2 = new Panel
            {
                BackColor = Color.FromArgb(50, 60, 70),
                Location  = new Point(16, 330),
                Size      = new Size(208, 1)
            };

            lblStatus = new Label
            {
                Text      = "Aguardando...",
                ForeColor = Color.FromArgb(120, 140, 160),
                Font      = new Font("Segoe UI", 8f),
                AutoSize  = false,
                Width     = 208,
                Height    = 40,
                Location  = new Point(16, 340),
                TextAlign = ContentAlignment.TopLeft
            };

            progress = new ProgressBar
            {
                Style    = ProgressBarStyle.Marquee,
                Location = new Point(16, 385),
                Size     = new Size(208, 8),
                Visible  = false
            };

            sidebar.Controls.AddRange(new Control[]
            {
                lblTitulo, lblSubtitulo, sep1,
                lblAno, cboAno, lblMes, cboMes,
                btnAtualizar, sep2, lblStatus, progress
            });

            // ── área principal ────────────────────────────────────────────
            var main = new Panel { Dock = DockStyle.Fill, Padding = new Padding(16) };

            // faixa de KPIs
            pnlKpis = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 100,
                BackColor = Color.Transparent
            };

            // área de conteúdo abaixo dos KPIs
            var content = new Panel { Dock = DockStyle.Fill };

            // linha superior: gráfico mensal (esq) + gráfico modalidade (dir)
            var rowTop = new Panel { Dock = DockStyle.Top, Height = 280 };

            // wrapper para deixar o splitter funcionar bem
            plotMensal.Dock = DockStyle.Fill;
            var pnlMensal = WrapChart(plotMensal, "📈 Internações por Mês");
            pnlMensal.Dock   = DockStyle.Fill;

            plotModalidade.Dock = DockStyle.Fill;
            var pnlModal = WrapChart(plotModalidade, "🍕 Modalidade de Internação");
            pnlModal.Dock    = DockStyle.Right;
            pnlModal.Width   = 380;

            rowTop.Controls.Add(pnlMensal);
            rowTop.Controls.Add(pnlModal);

            // linha inferior: top5 gráfico (esq) + tabela top5 (dir)
            var rowBot = new Panel { Dock = DockStyle.Fill };

            plotTop5.Dock = DockStyle.Fill;
            var pnlTop5 = WrapChart(plotTop5, "🦠 Top 5 Diagnósticos");
            pnlTop5.Dock  = DockStyle.Fill;

            dgv.Dock  = DockStyle.Right;
            dgv.Width = 400;
            EstilizarDgv();
            var pnlDgv = WrapDgv(dgv, "📋 Detalhamento Top 5");
            pnlDgv.Dock  = DockStyle.Right;
            pnlDgv.Width = 400;

            rowBot.Controls.Add(pnlTop5);
            rowBot.Controls.Add(pnlDgv);

            content.Controls.Add(rowBot);
            content.Controls.Add(rowTop);

            main.Controls.Add(content);
            main.Controls.Add(pnlKpis);

            this.Controls.Add(main);
            this.Controls.Add(sidebar);
        }

        // ── helpers de UI ─────────────────────────────────────────────────
        private Label MakeSideLabel(string text, int y) => new Label
        {
            Text      = text,
            ForeColor = Color.FromArgb(140, 160, 180),
            Font      = new Font("Segoe UI", 8f, FontStyle.Bold),
            AutoSize  = false,
            Width     = 208,
            Height    = 20,
            Location  = new Point(16, y)
        };

        private ComboBox MakeSideCombo(int y)
        {
            var c = new ComboBox
            {
                Location      = new Point(16, y),
                Size          = new Size(208, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle     = FlatStyle.Flat,
                BackColor     = Color.FromArgb(40, 52, 60),
                ForeColor     = Color.White
            };
            return c;
        }

        private Panel WrapChart(Control chart, string titulo)
        {
            var pnl = new Panel
            {
                BackColor = Color.White,
                Padding   = new Padding(0, 32, 0, 0),
                Margin    = new Padding(4)
            };
            var lbl = new Label
            {
                Text      = titulo,
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 60, 80),
                AutoSize  = true,
                Location  = new Point(12, 6)
            };
            chart.Dock = DockStyle.Fill;
            pnl.Controls.Add(chart);
            pnl.Controls.Add(lbl);
            return pnl;
        }

        private Panel WrapDgv(DataGridView grid, string titulo)
        {
            var pnl = new Panel
            {
                BackColor = Color.White,
                Padding   = new Padding(0, 32, 0, 0)
            };
            var lbl = new Label
            {
                Text      = titulo,
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(50, 60, 80),
                AutoSize  = true,
                Location  = new Point(12, 6)
            };
            grid.Dock = DockStyle.Fill;
            pnl.Controls.Add(grid);
            pnl.Controls.Add(lbl);
            return pnl;
        }

        private void EstilizarDgv()
        {
            dgv.BackgroundColor     = Color.White;
            dgv.BorderStyle         = BorderStyle.None;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgv.RowHeadersVisible   = false;
            dgv.ReadOnly            = true;
            dgv.SelectionMode       = DataGridViewSelectionMode.FullRowSelect;
            dgv.EnableHeadersVisualStyles = false;

            dgv.ColumnHeadersDefaultCellStyle.BackColor  = Color.FromArgb(240, 242, 245);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor  = Color.FromArgb(50, 60, 80);
            dgv.ColumnHeadersDefaultCellStyle.Font       = new Font("Segoe UI", 9f, FontStyle.Bold);
            dgv.DefaultCellStyle.SelectionBackColor      = Color.FromArgb(200, 230, 255);
            dgv.DefaultCellStyle.SelectionForeColor      = Color.FromArgb(30, 30, 30);
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgv.CellBorderStyle  = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.GridColor        = Color.FromArgb(225, 228, 232);
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9f);
            dgv.RowTemplate.Height    = 28;
        }

        // ── helper para mês (exibe nome mas passa número) ─────────────────
        private class MesItem
        {
            public int    Numero { get; set; }
            public string Nome   { get; set; } = "";
            public override string ToString() => Nome;
        }
    }
}
