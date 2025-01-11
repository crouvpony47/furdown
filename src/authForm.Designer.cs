namespace furdown
{
    partial class authForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.edgeWebView = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.loadingLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.edgeWebView)).BeginInit();
            this.SuspendLayout();
            // 
            // edgeWebView
            // 
            this.edgeWebView.AllowExternalDrop = false;
            this.edgeWebView.CreationProperties = null;
            this.edgeWebView.DefaultBackgroundColor = System.Drawing.Color.White;
            this.edgeWebView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.edgeWebView.Location = new System.Drawing.Point(0, 0);
            this.edgeWebView.Name = "edgeWebView";
            this.edgeWebView.Size = new System.Drawing.Size(1176, 632);
            this.edgeWebView.TabIndex = 0;
            this.edgeWebView.ZoomFactor = 1D;
            // 
            // loadingLabel
            // 
            this.loadingLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.loadingLabel.Location = new System.Drawing.Point(0, 0);
            this.loadingLabel.Name = "loadingLabel";
            this.loadingLabel.Size = new System.Drawing.Size(1176, 632);
            this.loadingLabel.TabIndex = 1;
            this.loadingLabel.Text = "validating saved cookies...";
            this.loadingLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // authForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1176, 632);
            this.Controls.Add(this.loadingLabel);
            this.Controls.Add(this.edgeWebView);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "authForm";
            this.Text = "furdown :: authorization";
            this.Load += new System.EventHandler(this.authForm_Load);
            this.Shown += new System.EventHandler(this.authForm_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.edgeWebView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 edgeWebView;
        private System.Windows.Forms.Label loadingLabel;
    }
}

