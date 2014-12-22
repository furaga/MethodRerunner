namespace furaga.MethodRerunner
{
    partial class MethodSelectWindow
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
            this.methodListView = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // methodListView
            // 
            this.methodListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.methodListView.Font = new System.Drawing.Font("MS UI Gothic", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.methodListView.Location = new System.Drawing.Point(0, 0);
            this.methodListView.Name = "methodListView";
            this.methodListView.Size = new System.Drawing.Size(613, 400);
            this.methodListView.TabIndex = 0;
            this.methodListView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.methodListView_KeyDown);
            this.methodListView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.methodListView_MouseDoubleClick);
            // 
            // MethodSelectWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(613, 400);
            this.Controls.Add(this.methodListView);
            this.Name = "MethodSelectWindow";
            this.Text = "Select recorded method params";
            this.Load += new System.EventHandler(this.MethodSelectWindow_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView methodListView;
    }
}