using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace primitive.Alcantarea
{
    public partial class OptionWindow : Form
    {
        public String AddinDir { get; set; }
        private Dictionary<String, Panel> m_panels;


        public OptionWindow()
        {
            InitializeComponent();

            m_panels = new Dictionary<String, Panel>() {
                {"Environment", panelEnvironment},
                {"Symbol Filter", panelSymIgnore},
                {"About", panelAbout},
            };

            dataGridViewSymIgnore.DataSource = new BindingList<alcGlobalSymFilterColumn>(AlcantareaHelper.SymGlobalIgnore);
            textBoxTCPPort.Text = AlcantareaHelper.TCPPort.ToString();
            checkBoxHook.Checked = AlcantareaHelper.HookLoadLibrary;
        }

        private void treeNavi_AfterSelect(object sender, TreeViewEventArgs e)
        {
            foreach (KeyValuePair<String, Panel> p in m_panels)
            {
                p.Value.Visible = e.Node.FullPath == p.Key;
            }
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            AlcantareaHelper.SymGlobalIgnore = ((BindingList<alcGlobalSymFilterColumn>)dataGridViewSymIgnore.DataSource).ToList();
            AlcantareaHelper.HookLoadLibrary = checkBoxHook.Checked;
            try
            {
                AlcantareaHelper.TCPPort = Convert.ToUInt16(textBoxTCPPort.Text);
            }
            catch (Exception)
            { }

            AlcantareaHelper.SaveConfig();
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void linkLabelWeb_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(linkLabelWeb.Text);
        }

        private void textBoxTCPPort_KeyPress(object sender, KeyPressEventArgs e)
        {
            //if (!char.IsDigit(e.KeyChar))
            //{
            //    e.Handled = true;
            //}
        }


        private void buttonSymIgnoreDefault_Click(object sender, EventArgs e)
        {
            alcGlobalConfig c = new alcGlobalConfig();
            c.SetupDefaultValues();
            dataGridViewSymIgnore.DataSource = new BindingList<alcGlobalSymFilterColumn>(c.SymIgnore);
        }

        private void symbolGridView_KeyUp(object sender, KeyEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            if (dgv != null && e.KeyCode == Keys.Space)
            {
                bool v = true;
                foreach (DataGridViewCell cell in dgv.SelectedCells)
                {
                    if (cell.ValueType == typeof(bool))
                    {
                        v = !(bool)cell.Value;
                        break;
                    }
                }
                foreach (DataGridViewCell cell in dgv.SelectedCells)
                {
                    if (cell.ValueType == typeof(bool))
                    {
                        cell.Value = v;
                    }
                }
            }
        }
    }
}
