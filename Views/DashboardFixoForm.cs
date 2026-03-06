// aliases para resolver ambiguidade entre System.Drawing / System.Windows.Forms e ScottPlot
using SD  = System.Drawing;
using SWF = System.Windows.Forms;
using SPC = ScottPlot;

using Microsoft.Extensions.Configuration;
using MocSaude.Infraestructure;
using MocSaude.Repositories;

namespace MocSaude.Forms
{
    public class DashboardFixoForm : SWF.Form
    {
        private KpiRepository _kpiRepo = null!;

        private SWF.ComboBox    cboAno       = new();
        private SWF.ComboBox    cboMes       = new();
        private SWF.Button      btnAtualizar = new();
        private SWF.Label       lblStatus    = new();
        private SWF.ProgressBar progress     = new();
        private SWF.Panel       pnlKpis      = new();

        private ScottPlot.WinForms.FormsPlot plotMensal     = new();
        private ScottPlot.WinForms.FormsPlot plotModalidade = new();
        private ScottPlot.WinForms.FormsPlot plotTop5       = new();

        private SWF.DataGridView dgv = new();

        private static readonly SD.Color CInternacao = SD.Color.FromArgb(33,  150, 243);
        private static readonly SD.Color CObito      = SD.Color.FromArgb(244,  67,  54);
        private static readonly SD.Color CDias       = SD.Color.FromArgb(0,   150, 136);
        private static readonly SD.Color CUti        = SD.Color.FromArgb(156,  39, 176);
        private static readonly SD.Color CValor      = SD.Color.FromArgb(255, 152,   0);
        private static readonly SD.Color CSidebar    = SD.Color.FromArgb(28,   37,  46);
        private static readonly SD.Color CBg         = SD.Color.FromArgb(240, 242, 245);

        public DashboardFixoForm()
        {
            InitializarUI();
            WireRepo();
            this.Load += async (s, e) => await CarregarTudoAsync();
        }

        private void WireRepo()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();
            var connStr = config.GetConnectionString("SqlServer")!;
            _kpiRepo = new KpiRepository(new DatabaseConnection(connStr));
        }

        private async Task CarregarTudoAsync()
        {
            SetLoading(true);
            try
            {
                if (cboAno.Items.Count == 1) // só "Todos os anos" → ainda nao carregou do banco
                {
                    var anos = await _kpiRepo.GetAnosDisponiveisAsync();
                    foreach (var a in anos) cboAno.Items.Add(a);
                    // mantem "Todos os anos" selecionado
                }

                var ano = cboAno.SelectedItem is int a2 ? (int?)a2 : null;
                var mes = cboMes.SelectedItem is MesItem mi ? (int?)mi.Numero : null;

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
                SWF.MessageBox.Show(ex.Message, "Erro ao carregar dados",
                    SWF.MessageBoxButtons.OK, SWF.MessageBoxIcon.Error);
                lblStatus.Text = "Erro ao carregar.";
            }
            finally { SetLoading(false); }
        }

        private void AtualizarKpis(KpiSummary s)
        {
            if (InvokeRequired) { Invoke(() => AtualizarKpis(s)); return; }
            pnlKpis.Controls.Clear();

            var cards = new[]
            {
                ("Internações",          s.TotalInternacoes.ToString("N0"),        CInternacao),
                ("Óbitos",               s.TotalObitos.ToString("N0"),             CObito),
                ("Média Dias Internado", s.MediaDiasPerm.ToString("N1") + " dias", CDias),
                ("Média Dias UTI",       s.MediaDiasUti.ToString("N1")  + " dias", CUti),
                ("Valor Total Gasto",    "R$ " + s.ValorTotalGasto.ToString("N0"), CValor),
                ("Taxa de Mortalidade",  s.TotalInternacoes > 0
                                               ? ((double)s.TotalObitos / s.TotalInternacoes * 100).ToString("N2") + "%"
                                               : "—",                                 CObito),
            };

            int cardW = (pnlKpis.Width - 20) / cards.Length - 8;
            for (int i = 0; i < cards.Length; i++)
            {
                var (titulo, valor, cor) = cards[i];
                var card = CriarKpiCard(titulo, valor, cor, cardW);
                card.Location = new SD.Point(10 + i * (cardW + 8), 10);
                pnlKpis.Controls.Add(card);
            }
        }

        private SWF.Panel CriarKpiCard(string titulo, string valor, SD.Color cor, int largura)
        {
            var pnl = new SWF.Panel
            {
                Size      = new SD.Size(largura, 80),
                BackColor = SD.Color.White,
                Cursor    = SWF.Cursors.Default
            };

            var barra = new SWF.Panel
            {
                Dock      = SWF.DockStyle.Top,
                Height    = 5,
                BackColor = cor
            };

            var lblTitulo = new SWF.Label
            {
                Text      = titulo,
                ForeColor = SD.Color.FromArgb(100, 110, 120),
                Font      = new SD.Font("Segoe UI", 8.5f, SD.FontStyle.Regular),
                AutoSize  = false,
                Width     = largura - 16,
                Height    = 22,
                Location  = new SD.Point(8, 12),
                TextAlign = SD.ContentAlignment.MiddleLeft
            };

            var lblValor = new SWF.Label
            {
                Text      = valor,
                ForeColor = cor,
                Font      = new SD.Font("Segoe UI", 14f, SD.FontStyle.Bold),
                AutoSize  = false,
                Width     = largura - 16,
                Height    = 34,
                Location  = new SD.Point(8, 34),
                TextAlign = SD.ContentAlignment.MiddleLeft
            };

            pnl.Paint += (s, e) =>
            {
                using var pen = new SD.Pen(SD.Color.FromArgb(20, 0, 0, 0), 1);
                e.Graphics.DrawRectangle(pen, 0, 0, pnl.Width - 1, pnl.Height - 1);
            };

            pnl.Controls.AddRange(new SWF.Control[] { barra, lblTitulo, lblValor });
            return pnl;
        }

        private void AtualizarGraficoMensal(List<(string Label, long Contagem)> dados)
        {
            if (InvokeRequired) { Invoke(() => AtualizarGraficoMensal(dados)); return; }

            var plt = plotMensal.Plot;
            plt.Clear();
            plt.Title("Internações por Mês");
            plt.FigureBackground.Color = new SPC.Color(255, 255, 255);
            plt.DataBackground.Color   = new SPC.Color(250, 250, 250);

            if (!dados.Any()) { plotMensal.Refresh(); return; }

            var bars = plt.Add.Bars(dados.Select(d => (double)d.Contagem).ToArray());
            bars.Color = new SPC.Color(CInternacao.R, CInternacao.G, CInternacao.B);

            plt.Axes.Bottom.SetTicks(
                Enumerable.Range(0, dados.Count).Select(i => (double)i).ToArray(),
                dados.Select(d => d.Label).ToArray());
            plt.Axes.Bottom.TickLabelStyle.FontSize = 10;
            plt.Axes.Left.Label.Text = "Internações";
            plt.Axes.AutoScale();
            plotMensal.Refresh();
        }

        private void AtualizarGraficoModalidade(List<(string Label, long Contagem)> dados)
        {
            if (InvokeRequired) { Invoke(() => AtualizarGraficoModalidade(dados)); return; }

            var plt = plotModalidade.Plot;
            plt.Clear();
            plt.Title("Modalidade de Internação");
            plt.FigureBackground.Color = new SPC.Color(255, 255, 255);
            plt.DataBackground.Color   = new SPC.Color(250, 250, 250);

            if (!dados.Any()) { plotModalidade.Refresh(); return; }

            var palette = new SPC.Color[]
            {
                new(CInternacao.R, CInternacao.G, CInternacao.B),
                new(CObito.R,      CObito.G,      CObito.B),
                new(CDias.R,       CDias.G,        CDias.B),
                new(CUti.R,        CUti.G,         CUti.B),
                new(CValor.R,      CValor.G,       CValor.B),
            };

            var slices = dados.Select((d, i) => new SPC.PieSlice
            {
                Value      = d.Contagem,
                Label      = d.Label,
                FillColor  = palette[i % palette.Length],
                LabelStyle = { FontSize = 10 }
            }).ToList();

            var pie = plt.Add.Pie(slices);
            pie.ExplodeFraction    = 0.05;
            pie.SliceLabelDistance = 1.4;
            plt.ShowLegend(SPC.Edge.Right);
            plt.Axes.AutoScale();
            plotModalidade.Refresh();
        }

        private void AtualizarGraficoTop5(List<(string Label, long Contagem)> dados)
        {
            if (InvokeRequired) { Invoke(() => AtualizarGraficoTop5(dados)); return; }

            var plt = plotTop5.Plot;
            plt.Clear();
            plt.Title("Top 5 Diagnósticos (CID)");
            plt.FigureBackground.Color = new SPC.Color(255, 255, 255);
            plt.DataBackground.Color   = new SPC.Color(250, 250, 250);

            if (!dados.Any()) { plotTop5.Refresh(); return; }

            var bars = plt.Add.Bars(dados.Select(d => (double)d.Contagem).ToArray());
            bars.Color = new SPC.Color(CUti.R, CUti.G, CUti.B);

            plt.Axes.Bottom.SetTicks(
                Enumerable.Range(0, dados.Count).Select(i => (double)i).ToArray(),
                dados.Select(d => d.Label).ToArray());
            plt.Axes.Bottom.TickLabelStyle.FontSize = 10;
            plt.Axes.Left.Label.Text = "Casos";
            plt.Axes.AutoScale();
            plotTop5.Refresh();
        }

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
                    pos == 1 ? "1º" : pos == 2 ? "2º" : pos == 3 ? "3º" : $"  {pos}º",
                    label, contagem, pct.ToString("N1") + "%");
                pos++;
            }

            dgv.DataSource = dt;
            dgv.Columns["Posição"].Width    = 70;
            dgv.Columns["CID"].Width        = 90;
            dgv.Columns["Casos"].Width      = 100;
            dgv.Columns["% do Total"].Width = 90;
            dgv.Columns["Casos"].DefaultCellStyle.Format    = "N0";
            dgv.Columns["Casos"].DefaultCellStyle.Alignment = SWF.DataGridViewContentAlignment.MiddleRight;
            dgv.Columns["% do Total"].DefaultCellStyle.Alignment = SWF.DataGridViewContentAlignment.MiddleRight;
        }

        private void SetLoading(bool loading)
        {
            if (InvokeRequired) { Invoke(() => SetLoading(loading)); return; }
            progress.Visible     = loading;
            btnAtualizar.Enabled = !loading;
            if (loading) lblStatus.Text = "Carregando dados...";
        }

        private void InitializarUI()
        {
            this.Text        = "MocSaúde — Painel de Indicadores";
            this.BackColor   = CBg;
            this.Font        = new SD.Font("Segoe UI", 9.5f);
            this.WindowState = SWF.FormWindowState.Maximized;
            this.MinimumSize = new SD.Size(1100, 700);

            // sidebar
            var sidebar = new SWF.Panel
            {
                Dock      = SWF.DockStyle.Left,
                Width     = 240,
                BackColor = CSidebar,
                Padding   = new SWF.Padding(16)
            };

            var lblTitulo = new SWF.Label
            {
                Text      = "MocSaúde",
                ForeColor = SD.Color.White,
                Font      = new SD.Font("Segoe UI", 15f, SD.FontStyle.Bold),
                AutoSize  = false, Width = 208, Height = 40,
                Location  = new SD.Point(16, 20)
            };

            var lblSubtitulo = new SWF.Label
            {
                Text      = "Painel de Indicadores de Saúde",
                ForeColor = SD.Color.FromArgb(140, 160, 180),
                Font      = new SD.Font("Segoe UI", 8.5f),
                AutoSize  = false, Width = 208, Height = 30,
                Location  = new SD.Point(16, 60)
            };

            var sep1 = new SWF.Panel
            {
                BackColor = SD.Color.FromArgb(50, 60, 70),
                Location  = new SD.Point(16, 98),
                Size      = new SD.Size(208, 1)
            };

            var lblAno = MakeSideLabel("FILTRO: ANO", 110);
            cboAno = MakeSideCombo(136);
            cboAno.Items.Add("Todos os anos");
            cboAno.SelectedIndex = 0;

            var lblMes = MakeSideLabel("FILTRO: MÊS", 190);
            cboMes = MakeSideCombo(216);
            cboMes.Items.Add("Todos os meses");
            var meses = new[] {
                "Janeiro","Fevereiro","Março","Abril","Maio","Junho",
                "Julho","Agosto","Setembro","Outubro","Novembro","Dezembro"
            };
            for (int i = 0; i < meses.Length; i++)
                cboMes.Items.Add(new MesItem { Numero = i + 1, Nome = meses[i] });
            cboMes.SelectedIndex = 0;

            btnAtualizar = new SWF.Button
            {
                Text      = "Atualizar Painel",
                Location  = new SD.Point(16, 272),
                Size      = new SD.Size(208, 42),
                BackColor = SD.Color.FromArgb(0, 150, 136),
                ForeColor = SD.Color.White,
                FlatStyle = SWF.FlatStyle.Flat,
                Font      = new SD.Font("Segoe UI", 10f, SD.FontStyle.Bold),
                Cursor    = SWF.Cursors.Hand
            };
            btnAtualizar.FlatAppearance.BorderSize = 0;
            btnAtualizar.Click += async (s, e) => await CarregarTudoAsync();

            var sep2 = new SWF.Panel
            {
                BackColor = SD.Color.FromArgb(50, 60, 70),
                Location  = new SD.Point(16, 330),
                Size      = new SD.Size(208, 1)
            };

            lblStatus = new SWF.Label
            {
                Text      = "Aguardando...",
                ForeColor = SD.Color.FromArgb(120, 140, 160),
                Font      = new SD.Font("Segoe UI", 8f),
                AutoSize  = false, Width = 208, Height = 40,
                Location  = new SD.Point(16, 340),
                TextAlign = SD.ContentAlignment.TopLeft
            };

            progress = new SWF.ProgressBar
            {
                Style    = SWF.ProgressBarStyle.Marquee,
                Location = new SD.Point(16, 385),
                Size     = new SD.Size(208, 8),
                Visible  = false
            };

            sidebar.Controls.AddRange(new SWF.Control[]
            {
                lblTitulo, lblSubtitulo, sep1,
                lblAno, cboAno, lblMes, cboMes,
                btnAtualizar, sep2, lblStatus, progress
            });

            // area principal
            var main = new SWF.Panel { Dock = SWF.DockStyle.Fill, Padding = new SWF.Padding(16) };

            pnlKpis = new SWF.Panel
            {
                Dock      = SWF.DockStyle.Top,
                Height    = 100,
                BackColor = SD.Color.Transparent
            };

            var content = new SWF.Panel { Dock = SWF.DockStyle.Fill };

            var rowTop = new SWF.Panel { Dock = SWF.DockStyle.Top, Height = 280 };
            var pnlMensal = WrapChart(plotMensal, "Internações por Mês");
            pnlMensal.Dock = SWF.DockStyle.Fill;
            var pnlModal = WrapChart(plotModalidade, "Modalidade de Internação");
            pnlModal.Dock  = SWF.DockStyle.Right;
            pnlModal.Width = 380;
            rowTop.Controls.Add(pnlMensal);
            rowTop.Controls.Add(pnlModal);

            var rowBot = new SWF.Panel { Dock = SWF.DockStyle.Fill };
            var pnlTop5 = WrapChart(plotTop5, "Top 5 Diagnósticos");
            pnlTop5.Dock = SWF.DockStyle.Fill;
            EstilizarDgv();
            var pnlDgv = WrapDgv(dgv, "Detalhamento Top 5");
            pnlDgv.Dock  = SWF.DockStyle.Right;
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

        private SWF.Label MakeSideLabel(string text, int y) => new SWF.Label
        {
            Text      = text,
            ForeColor = SD.Color.FromArgb(140, 160, 180),
            Font      = new SD.Font("Segoe UI", 8f, SD.FontStyle.Bold),
            AutoSize  = false, Width = 208, Height = 20,
            Location  = new SD.Point(16, y)
        };

        private SWF.ComboBox MakeSideCombo(int y) => new SWF.ComboBox
        {
            Location      = new SD.Point(16, y),
            Size          = new SD.Size(208, 28),
            DropDownStyle = SWF.ComboBoxStyle.DropDownList,
            FlatStyle     = SWF.FlatStyle.Flat,
            BackColor     = SD.Color.FromArgb(40, 52, 60),
            ForeColor     = SD.Color.White
        };

        private SWF.Panel WrapChart(SWF.Control chart, string titulo)
        {
            var pnl = new SWF.Panel
            {
                BackColor = SD.Color.White,
                Padding   = new SWF.Padding(0, 32, 0, 0),
                Margin    = new SWF.Padding(4)
            };
            var lbl = new SWF.Label
            {
                Text      = titulo,
                Font      = new SD.Font("Segoe UI", 9.5f, SD.FontStyle.Bold),
                ForeColor = SD.Color.FromArgb(50, 60, 80),
                AutoSize  = true,
                Location  = new SD.Point(12, 6)
            };
            chart.Dock = SWF.DockStyle.Fill;
            pnl.Controls.Add(chart);
            pnl.Controls.Add(lbl);
            return pnl;
        }

        private SWF.Panel WrapDgv(SWF.DataGridView grid, string titulo)
        {
            var pnl = new SWF.Panel
            {
                BackColor = SD.Color.White,
                Padding   = new SWF.Padding(0, 32, 0, 0)
            };
            var lbl = new SWF.Label
            {
                Text      = titulo,
                Font      = new SD.Font("Segoe UI", 9.5f, SD.FontStyle.Bold),
                ForeColor = SD.Color.FromArgb(50, 60, 80),
                AutoSize  = true,
                Location  = new SD.Point(12, 6)
            };
            grid.Dock = SWF.DockStyle.Fill;
            pnl.Controls.Add(grid);
            pnl.Controls.Add(lbl);
            return pnl;
        }

        private void EstilizarDgv()
        {
            dgv.BackgroundColor     = SD.Color.White;
            dgv.BorderStyle         = SWF.BorderStyle.None;
            dgv.AutoSizeColumnsMode = SWF.DataGridViewAutoSizeColumnsMode.Fill;
            dgv.ColumnHeadersHeightSizeMode = SWF.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgv.RowHeadersVisible   = false;
            dgv.ReadOnly            = true;
            dgv.SelectionMode       = SWF.DataGridViewSelectionMode.FullRowSelect;
            dgv.EnableHeadersVisualStyles = false;

            dgv.ColumnHeadersDefaultCellStyle.BackColor   = SD.Color.FromArgb(240, 242, 245);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor   = SD.Color.FromArgb(50, 60, 80);
            dgv.ColumnHeadersDefaultCellStyle.Font        = new SD.Font("Segoe UI", 9f, SD.FontStyle.Bold);
            dgv.DefaultCellStyle.SelectionBackColor       = SD.Color.FromArgb(200, 230, 255);
            dgv.DefaultCellStyle.SelectionForeColor       = SD.Color.FromArgb(30, 30, 30);
            dgv.AlternatingRowsDefaultCellStyle.BackColor = SD.Color.FromArgb(248, 250, 252);
            dgv.CellBorderStyle   = SWF.DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.GridColor         = SD.Color.FromArgb(225, 228, 232);
            dgv.DefaultCellStyle.Font  = new SD.Font("Segoe UI", 9f);
            dgv.RowTemplate.Height     = 28;
        }

        private class MesItem
        {
            public int    Numero { get; set; }
            public string Nome   { get; set; } = "";
            public override string ToString() => Nome;
        }
    }
}
