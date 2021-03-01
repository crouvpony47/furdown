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
            this.authWebBrowser = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // authWebBrowser
            // 
            this.authWebBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.authWebBrowser.Location = new System.Drawing.Point(0, 0);
            this.authWebBrowser.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.authWebBrowser.MinimumSize = new System.Drawing.Size(27, 25);
            this.authWebBrowser.Name = "authWebBrowser";
            this.authWebBrowser.ScriptErrorsSuppressed = true;
            this.authWebBrowser.Size = new System.Drawing.Size(1045, 506);
            this.authWebBrowser.TabIndex = 0;
            this.authWebBrowser.Navigated += new System.Windows.Forms.WebBrowserNavigatedEventHandler(this.authWebBrowser_Navigated);
            // 
            // authForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1045, 506);
            this.Controls.Add(this.authWebBrowser);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "authForm";
            this.Text = "furdown :: authorization";
            this.Load += new System.EventHandler(this.authForm_Load);
            this.Shown += new System.EventHandler(this.authForm_Shown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser authWebBrowser;
    }
}

