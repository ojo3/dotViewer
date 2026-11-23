
using ojo3Utility.Controls.TimerControls;

namespace dotViewer
{
    partial class MultiWindow
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
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.systemInfoLabel1 = new ojo3Utility.Controls.TimerControls.SystemInfoLabel();
            this._pbx = new System.Windows.Forms.PictureBox();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._pbx)).BeginInit();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.AutoSize = false;
            this.statusStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.statusStrip1.Font = new System.Drawing.Font("Segoe UI", 12.76F);
            this.statusStrip1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.systemInfoLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 336);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(3, 0, 39, 0);
            this.statusStrip1.Size = new System.Drawing.Size(384, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // systemInfoLabel1
            // 
            this.systemInfoLabel1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.systemInfoLabel1.AutoSize = false;
            this.systemInfoLabel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.systemInfoLabel1.DrivePath = null;
            this.systemInfoLabel1.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.systemInfoLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.systemInfoLabel1.InfoType = ojo3Utility.Controls.TimerControls.SystemInfoType.Clock;
            this.systemInfoLabel1.IsAdjustInterval = false;
            this.systemInfoLabel1.Margin = new System.Windows.Forms.Padding(0);
            this.systemInfoLabel1.Name = "SystemInfoLabel";
            this.systemInfoLabel1.Size = new System.Drawing.Size(311, 22);
            this.systemInfoLabel1.Spring = true;
            this.systemInfoLabel1.Styling = ojo3Utility.Controls.ColorDesign.Style.Bright;
            this.systemInfoLabel1.Text = "systemInfoLabel1";
            this.systemInfoLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // _pbx
            // 
            this._pbx.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this._pbx.Dock = System.Windows.Forms.DockStyle.Fill;
            this._pbx.Font = new System.Drawing.Font("Segoe UI", 12.76F);
            this._pbx.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this._pbx.Location = new System.Drawing.Point(0, 0);
            this._pbx.Margin = new System.Windows.Forms.Padding(0);
            this._pbx.Name = "_pbx";
            this._pbx.Size = new System.Drawing.Size(384, 336);
            this._pbx.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this._pbx.TabIndex = 5;
            this._pbx.TabStop = false;
            // 
            // MultiWindow
            // 
            this.ApplyFontChildren = false;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 23F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(384, 358);
            this.Controls.Add(this._pbx);
            this.Controls.Add(this.statusStrip1);
            this.FontSize = 12.76F;
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.Margin = new System.Windows.Forms.Padding(9, 12, 9, 12);
            this.Name = "MultiWindow";
            this.Opacity = 1D;
            this.Styling = ojo3Utility.Controls.ColorDesign.Style.Dark;
            this.Text = "Form1";
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._pbx)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.StatusStrip statusStrip1;
        private SystemInfoLabel systemInfoLabel1;
        private System.Windows.Forms.PictureBox _pbx;
    }
}

