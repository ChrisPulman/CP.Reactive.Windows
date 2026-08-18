// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using CP.ReactiveUI.Primitives.Windows.Integrations.Browser;

namespace CP.ReactiveUI.Primitives.Windows.Example.FormsExample
{
    partial class WebBrowserForm
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
                _dpiAwareBehavior.Dispose();
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
            this.extendedWebBrowser1 = new CP.ReactiveUI.Primitives.Windows.Integrations.Browser.ExtendedWebBrowser();
            this.SuspendLayout();
            // 
            // extendedWebBrowser1
            // 
            this.extendedWebBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.extendedWebBrowser1.Location = new System.Drawing.Point(0, 0);
            this.extendedWebBrowser1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.extendedWebBrowser1.MinimumSize = new System.Drawing.Size(15, 16);
            this.extendedWebBrowser1.Name = "extendedWebBrowser1";
            this.extendedWebBrowser1.ScriptErrorsSuppressed = true;
            this.extendedWebBrowser1.Size = new System.Drawing.Size(580, 293);
            this.extendedWebBrowser1.TabIndex = 0;
            this.extendedWebBrowser1.Url = new System.Uri("https://reactiveui.net/", System.UriKind.Absolute);
            // 
            // WebBrowserForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(580, 293);
            this.Controls.Add(this.extendedWebBrowser1);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "WebBrowserForm";
            this.Text = "WebBrowserForm";
            this.ResumeLayout(false);

        }

        #endregion

        private ExtendedWebBrowser extendedWebBrowser1;
    }
}
