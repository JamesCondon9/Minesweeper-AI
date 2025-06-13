namespace Minesweeper_AI
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            upperBarPanel = new TableLayoutPanel();
            mainPanel = new TableLayoutPanel();
            lowerBarPanel = new TableLayoutPanel();
            mainPanel.SuspendLayout();
            SuspendLayout();
            // 
            // upperBarPanel
            // 
            upperBarPanel.BackColor = Color.FromArgb(17, 14, 17);
            upperBarPanel.ColumnCount = 3;
            upperBarPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            upperBarPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            upperBarPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            upperBarPanel.Dock = DockStyle.Top;
            upperBarPanel.Location = new Point(0, 0);
            upperBarPanel.Margin = new Padding(0);
            upperBarPanel.Name = "upperBarPanel";
            upperBarPanel.RowCount = 1;
            upperBarPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            upperBarPanel.Size = new Size(1012, 69);
            upperBarPanel.TabIndex = 0;
            upperBarPanel.Paint += upperBarPanel_Paint;
            // 
            // mainPanel
            // 
            mainPanel.ColumnCount = 1;
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainPanel.Controls.Add(upperBarPanel, 0, 0);
            mainPanel.Controls.Add(lowerBarPanel, 0, 2);
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Location = new Point(0, 0);
            mainPanel.Margin = new Padding(0);
            mainPanel.Name = "mainPanel";
            mainPanel.RowCount = 3;
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 13.7724552F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 76.047905F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            mainPanel.Size = new Size(1012, 501);
            mainPanel.TabIndex = 0;
            mainPanel.Paint += tableLayoutPanel1_Paint;
            // 
            // lowerBarPanel
            // 
            lowerBarPanel.BackColor = SystemColors.ActiveCaption;
            lowerBarPanel.ColumnCount = 4;
            lowerBarPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            lowerBarPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            lowerBarPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            lowerBarPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            lowerBarPanel.Dock = DockStyle.Fill;
            lowerBarPanel.Location = new Point(0, 450);
            lowerBarPanel.Margin = new Padding(0);
            lowerBarPanel.Name = "lowerBarPanel";
            lowerBarPanel.RowCount = 1;
            lowerBarPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            lowerBarPanel.Size = new Size(1012, 51);
            lowerBarPanel.TabIndex = 1;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1012, 501);
            Controls.Add(mainPanel);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "Form1";
            Text = "Minesweeper-AI";
            mainPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private TableLayoutPanel upperBarPanel;
        private TableLayoutPanel mainPanel;
        private TableLayoutPanel lowerBarPanel;
    }
}
