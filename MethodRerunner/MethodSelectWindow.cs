using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace furaga.MethodRerunner
{
    public partial class MethodSelectWindow : Form
    {
        private string rootDir;
        public string SelectedDir { get; private set; }

        public MethodSelectWindow()
        {
            InitializeComponent();
        }

        public MethodSelectWindow(string rootDir)
        {
            InitializeComponent();
            this.rootDir = rootDir;
        }

        void MethodSelectWindow_Load(object sender, EventArgs e)
        {
            if (!System.IO.Directory.Exists(rootDir))
                return;
            methodListView.Nodes.Clear();
            foreach (var dir in System.IO.Directory.GetDirectories(rootDir))
                methodListView.Nodes.Add(dir, System.IO.Path.GetFileName(dir));
        }

        void methodListView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SetSelectedItem();
                this.Close();
            }
            if (e.KeyCode == Keys.Delete)
            {
                DeleteSelectedItem();
            }
        }

        void methodListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            SetSelectedItem();
            this.Close();
        }

        void SetSelectedItem()
        {
            SelectedDir = null;
            if (methodListView.SelectedNode != null)
            {
                var dirName = methodListView.SelectedNode.Text;
                var tokens = dirName.Split('.');
                if (tokens.Length >= 2)
                {
                    var methodName = tokens[tokens.Length - 2];
                    var dir = System.IO.Path.Combine(rootDir, dirName, methodName);
                    SelectedDir = dir;
                }
            }
        }

        void DeleteSelectedItem()
        {
            SelectedDir = null;
            if (methodListView.SelectedNode != null)
            {
                var dirName = methodListView.SelectedNode.Text;
                var methodName = dirName.Split('.').Last();
                var dir = System.IO.Path.Combine(rootDir, dirName);
                var dirInfo = new System.IO.DirectoryInfo(dir);
                dirInfo.Delete(true);
                MethodSelectWindow_Load(null, null);
            }
        }
    }
}
