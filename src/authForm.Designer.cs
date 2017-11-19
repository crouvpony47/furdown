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
            this.authWebBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.authWebBrowser.Name = "authWebBrowser";
            this.authWebBrowser.ScriptErrorsSuppressed = true;
            this.authWebBrowser.Size = new System.Drawing.Size(784, 411);
            this.authWebBrowser.TabIndex = 0;
            this.authWebBrowser.Navigated += new System.Windows.Forms.WebBrowserNavigatedEventHandler(this.authWebBrowser_Navigated);
            // 
            // authForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 411);
            this.Controls.Add(this.authWebBrowser);
            this.Name = "authForm";
            this.Text = "furdown :: authorization";
            this.Shown += new System.EventHandler(this.authForm_Shown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser authWebBrowser;
    }
}

