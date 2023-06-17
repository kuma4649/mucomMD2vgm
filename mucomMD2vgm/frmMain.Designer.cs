namespace mucomMD2vgm
{
    partial class FrmMain
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            this.tsbToVGZ = new System.Windows.Forms.ToolStripButton();
            this.tsbOnPlay = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbCompile = new System.Windows.Forms.ToolStripButton();
            this.tsbOpen = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tsbWatcher = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tsbLoopEx = new System.Windows.Forms.ToolStripButton();
            this.tslMaxRendering = new System.Windows.Forms.ToolStripLabel();
            this.tstbMaxRendering = new System.Windows.Forms.ToolStripTextBox();
            this.tslSecond = new System.Windows.Forms.ToolStripLabel();
            this.tspbProgress = new System.Windows.Forms.ToolStripProgressBar();
            this.tsslMessage = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.dgvResult = new System.Windows.Forms.DataGridView();
            this.clmPartName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.clmChip = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.clmCounter = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.clmLpos = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.clmSpacer = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.toolStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.toolStripContainer1.BottomToolStripPanel.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvResult)).BeginInit();
            this.SuspendLayout();
            // 
            // tsbToVGZ
            // 
            this.tsbToVGZ.CheckOnClick = true;
            this.tsbToVGZ.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbToVGZ.Image = global::mucomMD2vgm.Properties.Resources.icon5;
            this.tsbToVGZ.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbToVGZ.Name = "tsbToVGZ";
            this.tsbToVGZ.Size = new System.Drawing.Size(23, 22);
            this.tsbToVGZ.Text = "Compress to VGZ";
            // 
            // tsbOnPlay
            // 
            this.tsbOnPlay.CheckOnClick = true;
            this.tsbOnPlay.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbOnPlay.Image = global::mucomMD2vgm.Properties.Resources.icon3;
            this.tsbOnPlay.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbOnPlay.Name = "tsbOnPlay";
            this.tsbOnPlay.Size = new System.Drawing.Size(23, 22);
            this.tsbOnPlay.Text = "Play After Compile";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // tsbCompile
            // 
            this.tsbCompile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbCompile.Image = global::mucomMD2vgm.Properties.Resources.icon2;
            this.tsbCompile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbCompile.Name = "tsbCompile";
            this.tsbCompile.Size = new System.Drawing.Size(23, 22);
            this.tsbCompile.Text = "Compile";
            this.tsbCompile.Click += new System.EventHandler(this.TsbCompile_Click);
            // 
            // tsbOpen
            // 
            this.tsbOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbOpen.Image = global::mucomMD2vgm.Properties.Resources.icon1;
            this.tsbOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbOpen.Name = "tsbOpen";
            this.tsbOpen.Size = new System.Drawing.Size(23, 22);
            this.tsbOpen.Text = "Open and Compile";
            this.tsbOpen.Click += new System.EventHandler(this.TsbOpen_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbOpen,
            this.tsbCompile,
            this.toolStripSeparator1,
            this.tsbOnPlay,
            this.tsbWatcher,
            this.tsbToVGZ,
            this.toolStripSeparator2,
            this.tsbLoopEx,
            this.tslMaxRendering,
            this.tstbMaxRendering,
            this.tslSecond});
            this.toolStrip1.Location = new System.Drawing.Point(3, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(291, 25);
            this.toolStrip1.TabIndex = 0;
            // 
            // tsbWatcher
            // 
            this.tsbWatcher.CheckOnClick = true;
            this.tsbWatcher.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbWatcher.Image = global::mucomMD2vgm.Properties.Resources.icon4;
            this.tsbWatcher.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbWatcher.Name = "tsbWatcher";
            this.tsbWatcher.Size = new System.Drawing.Size(23, 22);
            this.tsbWatcher.Text = "Watch to gwi file";
            this.tsbWatcher.CheckedChanged += new System.EventHandler(this.TsbWatcher_CheckedChanged);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // tsbLoopEx
            // 
            this.tsbLoopEx.CheckOnClick = true;
            this.tsbLoopEx.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbLoopEx.Image = global::mucomMD2vgm.Properties.Resources.icon6;
            this.tsbLoopEx.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbLoopEx.Name = "tsbLoopEx";
            this.tsbLoopEx.Size = new System.Drawing.Size(23, 22);
            this.tsbLoopEx.Text = "LoopEX";
            this.tsbLoopEx.Click += new System.EventHandler(this.TsbLoopEx_Click);
            // 
            // tslMaxRendering
            // 
            this.tslMaxRendering.BackColor = System.Drawing.Color.Transparent;
            this.tslMaxRendering.Enabled = false;
            this.tslMaxRendering.Name = "tslMaxRendering";
            this.tslMaxRendering.Size = new System.Drawing.Size(84, 22);
            this.tslMaxRendering.Text = "Max rendering";
            // 
            // tstbMaxRendering
            // 
            this.tstbMaxRendering.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tstbMaxRendering.Enabled = false;
            this.tstbMaxRendering.Name = "tstbMaxRendering";
            this.tstbMaxRendering.Size = new System.Drawing.Size(40, 25);
            this.tstbMaxRendering.Text = "600";
            this.tstbMaxRendering.TextBoxTextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.tstbMaxRendering.TextChanged += new System.EventHandler(this.TstbMaxRendering_TextChanged);
            // 
            // tslSecond
            // 
            this.tslSecond.BackColor = System.Drawing.Color.Transparent;
            this.tslSecond.Enabled = false;
            this.tslSecond.Name = "tslSecond";
            this.tslSecond.Size = new System.Drawing.Size(12, 22);
            this.tslSecond.Text = "s";
            // 
            // tspbProgress
            // 
            this.tspbProgress.Name = "tspbProgress";
            this.tspbProgress.Size = new System.Drawing.Size(100, 16);
            this.tspbProgress.Visible = false;
            // 
            // tsslMessage
            // 
            this.tsslMessage.Name = "tsslMessage";
            this.tsslMessage.Size = new System.Drawing.Size(0, 17);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsslMessage,
            this.tspbProgress});
            this.statusStrip1.Location = new System.Drawing.Point(0, 0);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(744, 22);
            this.statusStrip1.TabIndex = 0;
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.BottomToolStripPanel
            // 
            this.toolStripContainer1.BottomToolStripPanel.Controls.Add(this.statusStrip1);
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.splitContainer1);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(744, 472);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(744, 519);
            this.toolStripContainer1.TabIndex = 2;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip1);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.dgvResult);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.textBox1);
            this.splitContainer1.Size = new System.Drawing.Size(744, 472);
            this.splitContainer1.SplitterDistance = 329;
            this.splitContainer1.TabIndex = 0;
            // 
            // dgvResult
            // 
            this.dgvResult.AllowUserToAddRows = false;
            this.dgvResult.AllowUserToDeleteRows = false;
            this.dgvResult.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvResult.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvResult.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvResult.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.clmPartName,
            this.clmChip,
            this.clmCounter,
            this.clmLpos,
            this.clmSpacer});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.Navy;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvResult.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvResult.Location = new System.Drawing.Point(0, 0);
            this.dgvResult.Name = "dgvResult";
            this.dgvResult.RowHeadersVisible = false;
            this.dgvResult.RowTemplate.Height = 21;
            this.dgvResult.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvResult.Size = new System.Drawing.Size(329, 472);
            this.dgvResult.TabIndex = 0;
            // 
            // clmPartName
            // 
            this.clmPartName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.clmPartName.HeaderText = "Part";
            this.clmPartName.Name = "clmPartName";
            this.clmPartName.ReadOnly = true;
            this.clmPartName.Width = 56;
            // 
            // clmChip
            // 
            this.clmChip.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.clmChip.HeaderText = "Chip";
            this.clmChip.Name = "clmChip";
            this.clmChip.ReadOnly = true;
            this.clmChip.Width = 56;
            // 
            // clmCounter
            // 
            this.clmCounter.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.clmCounter.HeaderText = "Total Count";
            this.clmCounter.Name = "clmCounter";
            this.clmCounter.ReadOnly = true;
            this.clmCounter.Width = 98;
            // 
            // clmLpos
            // 
            this.clmLpos.HeaderText = "Loop Count";
            this.clmLpos.Name = "clmLpos";
            // 
            // clmSpacer
            // 
            this.clmSpacer.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.clmSpacer.HeaderText = "";
            this.clmSpacer.Name = "clmSpacer";
            this.clmSpacer.ReadOnly = true;
            // 
            // textBox1
            // 
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.Location = new System.Drawing.Point(0, 0);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox1.Size = new System.Drawing.Size(411, 472);
            this.textBox1.TabIndex = 0;
            // 
            // frmMain
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(744, 519);
            this.Controls.Add(this.toolStripContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(320, 240);
            this.Name = "frmMain";
            this.Text = "mucomMD2vgm";
            this.Shown += new System.EventHandler(this.FrmMain_Shown);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.FrmMain_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.FrmMain_DragEnter);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FrmMain_KeyDown);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.toolStripContainer1.BottomToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.BottomToolStripPanel.PerformLayout();
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvResult)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStripButton tsbToVGZ;
        private System.Windows.Forms.ToolStripButton tsbOnPlay;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton tsbCompile;
        private System.Windows.Forms.ToolStripButton tsbOpen;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton tsbWatcher;
        private System.Windows.Forms.ToolStripProgressBar tspbProgress;
        private System.Windows.Forms.ToolStripStatusLabel tsslMessage;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataGridView dgvResult;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ToolStripButton tsbLoopEx;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripLabel tslMaxRendering;
        private System.Windows.Forms.ToolStripTextBox tstbMaxRendering;
        private System.Windows.Forms.ToolStripLabel tslSecond;
        private System.Windows.Forms.DataGridViewTextBoxColumn clmPartName;
        private System.Windows.Forms.DataGridViewTextBoxColumn clmChip;
        private System.Windows.Forms.DataGridViewTextBoxColumn clmCounter;
        private System.Windows.Forms.DataGridViewTextBoxColumn clmLpos;
        private System.Windows.Forms.DataGridViewTextBoxColumn clmSpacer;
    }
}

