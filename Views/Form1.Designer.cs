namespace MocSaude.Forms
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tableLayoutPanel1 = new TableLayoutPanel();
            cboTables = new ComboBox();
            clbColumns = new CheckedListBox();
            cboGroupBy = new ComboBox();
            cboAggregate = new ComboBox();
            cboAggFunc = new ComboBox();
            btnLoad = new Button();
            dgvData = new DataGridView();
            formsPlot1 = new ScottPlot.WinForms.FormsPlot();
            progressBar = new ProgressBar();
            lblStatus = new Label();
            tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvData).BeginInit();
            SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3333321F));
            tableLayoutPanel1.Controls.Add(cboTables, 0, 0);
            tableLayoutPanel1.Controls.Add(clbColumns, 2, 0);
            tableLayoutPanel1.Controls.Add(cboGroupBy, 0, 1);
            tableLayoutPanel1.Controls.Add(cboAggregate, 0, 2);
            tableLayoutPanel1.Controls.Add(cboAggFunc, 1, 0);
            tableLayoutPanel1.Controls.Add(btnLoad, 1, 1);
            tableLayoutPanel1.Controls.Add(dgvData, 1, 2);
            tableLayoutPanel1.Controls.Add(formsPlot1, 2, 1);
            tableLayoutPanel1.Controls.Add(progressBar, 2, 2);
            tableLayoutPanel1.Controls.Add(lblStatus, 2, 3);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 4;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3333321F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3333359F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3333359F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 110F));
            tableLayoutPanel1.Size = new Size(800, 450);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // cboTables
            // 
            cboTables.FormattingEnabled = true;
            cboTables.Location = new Point(3, 3);
            cboTables.Name = "cboTables";
            cboTables.Size = new Size(121, 23);
            cboTables.TabIndex = 0;
            // 
            // clbColumns
            // 
            clbColumns.Dock = DockStyle.Fill;
            clbColumns.FormattingEnabled = true;
            clbColumns.Location = new Point(535, 3);
            clbColumns.Name = "clbColumns";
            clbColumns.Size = new Size(262, 107);
            clbColumns.TabIndex = 1;
            // 
            // cboGroupBy
            // 
            cboGroupBy.FormattingEnabled = true;
            cboGroupBy.Location = new Point(3, 116);
            cboGroupBy.Name = "cboGroupBy";
            cboGroupBy.Size = new Size(121, 23);
            cboGroupBy.TabIndex = 2;
            // 
            // cboAggregate
            // 
            cboAggregate.FormattingEnabled = true;
            cboAggregate.Location = new Point(3, 229);
            cboAggregate.Name = "cboAggregate";
            cboAggregate.Size = new Size(121, 23);
            cboAggregate.TabIndex = 3;
            // 
            // cboAggFunc
            // 
            cboAggFunc.FormattingEnabled = true;
            cboAggFunc.Items.AddRange(new object[] { "SUM", "COUNT", "AVG", "MAX", "MIN" });
            cboAggFunc.Location = new Point(269, 3);
            cboAggFunc.Name = "cboAggFunc";
            cboAggFunc.Size = new Size(121, 23);
            cboAggFunc.TabIndex = 4;
            // 
            // btnLoad
            // 
            btnLoad.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            btnLoad.Location = new Point(269, 158);
            btnLoad.Name = "btnLoad";
            btnLoad.Size = new Size(260, 23);
            btnLoad.TabIndex = 5;
            btnLoad.Text = "Carregar Dados";
            btnLoad.UseVisualStyleBackColor = true;
            // 
            // dgvData
            // 
            dgvData.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvData.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvData.Dock = DockStyle.Fill;
            dgvData.Location = new Point(269, 229);
            dgvData.Name = "dgvData";
            dgvData.Size = new Size(260, 107);
            dgvData.TabIndex = 6;
            // 
            // formsPlot1
            // 
            formsPlot1.DisplayScale = 1F;
            formsPlot1.Location = new Point(535, 116);
            formsPlot1.Name = "formsPlot1";
            formsPlot1.Size = new Size(150, 107);
            formsPlot1.TabIndex = 7;
            // 
            // progressBar
            // 
            progressBar.Location = new Point(535, 229);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(100, 23);
            progressBar.Style = ProgressBarStyle.Marquee;
            progressBar.TabIndex = 8;
            progressBar.Visible = false;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(535, 339);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(43, 15);
            lblStatus.TabIndex = 9;
            lblStatus.Text = "Pronto";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(tableLayoutPanel1);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvData).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel tableLayoutPanel1;
        private ComboBox cboTables;
        private CheckedListBox clbColumns;
        private ComboBox cboGroupBy;
        private ComboBox cboAggregate;
        private ComboBox cboAggFunc;
        private Button btnLoad;
        private DataGridView dgvData;
        private ScottPlot.WinForms.FormsPlot formsPlot1;
        private ProgressBar progressBar;
        private Label lblStatus;
    }
}