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
            tsbToVGZ = new System.Windows.Forms.ToolStripButton();
            tsbOnPlay = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            tsbCompile = new System.Windows.Forms.ToolStripButton();
            tsbOpen = new System.Windows.Forms.ToolStripButton();
            toolStrip1 = new System.Windows.Forms.ToolStrip();
            tsbWatcher = new System.Windows.Forms.ToolStripButton();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            tsbLoopEx = new System.Windows.Forms.ToolStripButton();
            tslMaxRendering = new System.Windows.Forms.ToolStripLabel();
            tstbMaxRendering = new System.Windows.Forms.ToolStripTextBox();
            tslSecond = new System.Windows.Forms.ToolStripLabel();
            tspbProgress = new System.Windows.Forms.ToolStripProgressBar();
            tsslMessage = new System.Windows.Forms.ToolStripStatusLabel();
            statusStrip1 = new System.Windows.Forms.StatusStrip();
            toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            dgvResult = new System.Windows.Forms.DataGridView();
            clmPartName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            clmChip = new System.Windows.Forms.DataGridViewTextBoxColumn();
            clmCounter = new System.Windows.Forms.DataGridViewTextBoxColumn();
            clmLpos = new System.Windows.Forms.DataGridViewTextBoxColumn();
            clmSpacer = new System.Windows.Forms.DataGridViewTextBoxColumn();
            textBox1 = new System.Windows.Forms.TextBox();
            toolStrip1.SuspendLayout();
            statusStrip1.SuspendLayout();
            toolStripContainer1.BottomToolStripPanel.SuspendLayout();
            toolStripContainer1.ContentPanel.SuspendLayout();
            toolStripContainer1.TopToolStripPanel.SuspendLayout();
            toolStripContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvResult).BeginInit();
            SuspendLayout();
            // 
            // tsbToVGZ
            // 
            tsbToVGZ.CheckOnClick = true;
            tsbToVGZ.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbToVGZ.Image = Properties.Resources.icon5;
            tsbToVGZ.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbToVGZ.Name = "tsbToVGZ";
            tsbToVGZ.Size = new System.Drawing.Size(23, 22);
            tsbToVGZ.Text = "Compress to VGZ";
            // 
            // tsbOnPlay
            // 
            tsbOnPlay.CheckOnClick = true;
            tsbOnPlay.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbOnPlay.Image = Properties.Resources.icon3;
            tsbOnPlay.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbOnPlay.Name = "tsbOnPlay";
            tsbOnPlay.Size = new System.Drawing.Size(23, 22);
            tsbOnPlay.Text = "Play After Compile";
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // tsbCompile
            // 
            tsbCompile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbCompile.Image = Properties.Resources.icon2;
            tsbCompile.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbCompile.Name = "tsbCompile";
            tsbCompile.Size = new System.Drawing.Size(23, 22);
            tsbCompile.Text = "Compile";
            tsbCompile.Click += TsbCompile_Click;
            // 
            // tsbOpen
            // 
            tsbOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbOpen.Image = Properties.Resources.icon1;
            tsbOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbOpen.Name = "tsbOpen";
            tsbOpen.Size = new System.Drawing.Size(23, 22);
            tsbOpen.Text = "Open and Compile";
            tsbOpen.Click += TsbOpen_Click;
            // 
            // toolStrip1
            // 
            toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsbOpen, tsbCompile, toolStripSeparator1, tsbOnPlay, tsbWatcher, tsbToVGZ, toolStripSeparator2, tsbLoopEx, tslMaxRendering, tstbMaxRendering, tslSecond });
            toolStrip1.Location = new System.Drawing.Point(3, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new System.Drawing.Size(322, 25);
            toolStrip1.TabIndex = 0;
            // 
            // tsbWatcher
            // 
            tsbWatcher.CheckOnClick = true;
            tsbWatcher.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbWatcher.Image = Properties.Resources.icon4;
            tsbWatcher.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbWatcher.Name = "tsbWatcher";
            tsbWatcher.Size = new System.Drawing.Size(23, 22);
            tsbWatcher.Text = "Watch to gwi file";
            tsbWatcher.CheckedChanged += TsbWatcher_CheckedChanged;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // tsbLoopEx
            // 
            tsbLoopEx.CheckOnClick = true;
            tsbLoopEx.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            tsbLoopEx.Image = Properties.Resources.icon6;
            tsbLoopEx.ImageTransparentColor = System.Drawing.Color.Magenta;
            tsbLoopEx.Name = "tsbLoopEx";
            tsbLoopEx.Size = new System.Drawing.Size(23, 22);
            tsbLoopEx.Text = "LoopEX";
            tsbLoopEx.Click += TsbLoopEx_Click;
            // 
            // tslMaxRendering
            // 
            tslMaxRendering.BackColor = System.Drawing.Color.Transparent;
            tslMaxRendering.Enabled = false;
            tslMaxRendering.Name = "tslMaxRendering";
            tslMaxRendering.Size = new System.Drawing.Size(84, 22);
            tslMaxRendering.Text = "Max rendering";
            // 
            // tstbMaxRendering
            // 
            tstbMaxRendering.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            tstbMaxRendering.Enabled = false;
            tstbMaxRendering.Name = "tstbMaxRendering";
            tstbMaxRendering.Size = new System.Drawing.Size(40, 25);
            tstbMaxRendering.Text = "600";
            tstbMaxRendering.TextBoxTextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            tstbMaxRendering.TextChanged += TstbMaxRendering_TextChanged;
            // 
            // tslSecond
            // 
            tslSecond.BackColor = System.Drawing.Color.Transparent;
            tslSecond.Enabled = false;
            tslSecond.Name = "tslSecond";
            tslSecond.Size = new System.Drawing.Size(12, 22);
            tslSecond.Text = "s";
            // 
            // tspbProgress
            // 
            tspbProgress.Name = "tspbProgress";
            tspbProgress.Size = new System.Drawing.Size(100, 16);
            tspbProgress.Visible = false;
            // 
            // tsslMessage
            // 
            tsslMessage.Name = "tsslMessage";
            tsslMessage.Size = new System.Drawing.Size(0, 17);
            // 
            // statusStrip1
            // 
            statusStrip1.Dock = System.Windows.Forms.DockStyle.None;
            statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsslMessage, tspbProgress });
            statusStrip1.Location = new System.Drawing.Point(0, 0);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new System.Drawing.Size(868, 22);
            statusStrip1.TabIndex = 0;
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.BottomToolStripPanel
            // 
            toolStripContainer1.BottomToolStripPanel.Controls.Add(statusStrip1);
            // 
            // toolStripContainer1.ContentPanel
            // 
            toolStripContainer1.ContentPanel.Controls.Add(splitContainer1);
            toolStripContainer1.ContentPanel.Margin = new System.Windows.Forms.Padding(4);
            toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(868, 602);
            toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            toolStripContainer1.Margin = new System.Windows.Forms.Padding(4);
            toolStripContainer1.Name = "toolStripContainer1";
            toolStripContainer1.Size = new System.Drawing.Size(868, 649);
            toolStripContainer1.TabIndex = 2;
            toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            toolStripContainer1.TopToolStripPanel.Controls.Add(toolStrip1);
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Margin = new System.Windows.Forms.Padding(4);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(dgvResult);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(textBox1);
            splitContainer1.Size = new System.Drawing.Size(868, 602);
            splitContainer1.SplitterDistance = 382;
            splitContainer1.SplitterWidth = 5;
            splitContainer1.TabIndex = 0;
            // 
            // dgvResult
            // 
            dgvResult.AllowUserToAddRows = false;
            dgvResult.AllowUserToDeleteRows = false;
            dgvResult.AllowUserToResizeRows = false;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            dgvResult.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgvResult.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvResult.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] { clmPartName, clmChip, clmCounter, clmLpos, clmSpacer });
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.Navy;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            dgvResult.DefaultCellStyle = dataGridViewCellStyle2;
            dgvResult.Dock = System.Windows.Forms.DockStyle.Fill;
            dgvResult.Location = new System.Drawing.Point(0, 0);
            dgvResult.Margin = new System.Windows.Forms.Padding(4);
            dgvResult.Name = "dgvResult";
            dgvResult.RowHeadersVisible = false;
            dgvResult.RowTemplate.Height = 21;
            dgvResult.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgvResult.Size = new System.Drawing.Size(382, 602);
            dgvResult.TabIndex = 0;
            // 
            // clmPartName
            // 
            clmPartName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            clmPartName.HeaderText = "Part";
            clmPartName.Name = "clmPartName";
            clmPartName.ReadOnly = true;
            clmPartName.Width = 56;
            // 
            // clmChip
            // 
            clmChip.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            clmChip.HeaderText = "Chip";
            clmChip.Name = "clmChip";
            clmChip.ReadOnly = true;
            clmChip.Width = 56;
            // 
            // clmCounter
            // 
            clmCounter.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            clmCounter.HeaderText = "Total Count";
            clmCounter.Name = "clmCounter";
            clmCounter.ReadOnly = true;
            clmCounter.Width = 98;
            // 
            // clmLpos
            // 
            clmLpos.HeaderText = "Loop Count";
            clmLpos.Name = "clmLpos";
            // 
            // clmSpacer
            // 
            clmSpacer.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            clmSpacer.HeaderText = "";
            clmSpacer.Name = "clmSpacer";
            clmSpacer.ReadOnly = true;
            // 
            // textBox1
            // 
            textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            textBox1.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            textBox1.Location = new System.Drawing.Point(0, 0);
            textBox1.Margin = new System.Windows.Forms.Padding(4);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            textBox1.Size = new System.Drawing.Size(481, 602);
            textBox1.TabIndex = 0;
            // 
            // FrmMain
            // 
            AllowDrop = true;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(868, 649);
            Controls.Add(toolStripContainer1);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4);
            MinimumSize = new System.Drawing.Size(371, 290);
            Name = "FrmMain";
            Text = "mucomMD2vgm";
            Shown += FrmMain_Shown;
            DragDrop += FrmMain_DragDrop;
            DragEnter += FrmMain_DragEnter;
            KeyDown += FrmMain_KeyDown;
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            toolStripContainer1.BottomToolStripPanel.ResumeLayout(false);
            toolStripContainer1.BottomToolStripPanel.PerformLayout();
            toolStripContainer1.ContentPanel.ResumeLayout(false);
            toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            toolStripContainer1.TopToolStripPanel.PerformLayout();
            toolStripContainer1.ResumeLayout(false);
            toolStripContainer1.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvResult).EndInit();
            ResumeLayout(false);
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

