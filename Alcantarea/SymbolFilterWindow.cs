using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;


namespace primitive.Alcantarea
{
    public partial class SymbolFilterWindow : Form
    {
        public BindingList<alcSymbolFilterColumn> FilterData
        {
            get { return symbolDataGridView.DataSource as BindingList<alcSymbolFilterColumn>; }
            private set {
                symbolDataGridView.DataSource = value;
                MakeVariableCellsReadOnly();
            }
        }

        public String TargetFilePath
        {
            get { return textPath.Text; }
            private set { textPath.Text = value; }
        }

        public String ConfigFilePath
        {
            get { return TargetFilePath+".alc"; }
        }

        private bool NeedsSave;



        public SymbolFilterWindow()
        {
            InitializeComponent();
        }


        public bool LoadFilter(String path_to_bin)
        {
            TargetFilePath = path_to_bin;

            List<alcSymbolFilterColumn> default_filter = SymbolFilterUtil.GetDefaultFilter(TargetFilePath);
            List<alcSymbolFilterColumn> loaded_filter = SymbolFilterUtil.LoadFromConfigFile(ConfigFilePath);
            List<alcSymbolFilterColumn> filter = null;
            if (loaded_filter != null)
            {
                NeedsSave = true;
                filter = SymbolFilterUtil.MergeFilter(default_filter, loaded_filter);
            }
            else
            {
                NeedsSave = false;
                filter = default_filter;
            }
            if (filter != null)
            {
                filter.Sort(SymbolFilterUtil.CompareDataTypeOrName);
                FilterData = new BindingList<alcSymbolFilterColumn>(filter);
                return true;
            }
            return false;
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (!NeedsSave)
                {
                    File.Delete(ConfigFilePath);
                }
                else
                {
                    SymbolFilterUtil.SaveConfig(ConfigFilePath, FilterData.ToList());
                }
            }
            catch (Exception)
            {
            }

            AlcantareaHelper.RequestSetSymbolFilter(TargetFilePath, FilterData.ToList());
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void symbolGridView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            List<alcSymbolFilterColumn> sortedList = null;
            switch (symbolDataGridView.Columns[e.ColumnIndex].DataPropertyName)
            {
                case "FlagUpdate":
                    sortedList = FilterData.OrderBy(x => !x.FlagUpdate).ToList();
                    break;
                case "FlagLinkToLocal":
                    sortedList = FilterData.OrderBy(x => !x.FlagLinkToLocal).ToList();
                    break;
                case "Handler":
                    sortedList = FilterData.OrderBy(x => x.Handler).ToList();
                    break;
                case "Name":
                    sortedList = FilterData.OrderBy(x => x.Name).ToList();
                    break;
                case "AttrDataType":
                    sortedList = FilterData.ToList();
                    sortedList.Sort(SymbolFilterUtil.CompareDataTypeOrName);
                    break;
                case "AttrLinkType":
                    sortedList = FilterData.ToList();
                    sortedList.Sort((a, b) =>
                    {
                        int r = String.Compare(a.AttrLinkType, b.AttrLinkType);
                        if (r == 0) { r = String.Compare(a.Name, b.Name); }
                        return r;
                    });
                    break;
                case "AttrAccessType":
                    sortedList = FilterData.ToList();
                    sortedList.Sort((a, b) =>
                    {
                        int r = String.Compare(a.AttrAccessType, b.AttrAccessType);
                        if (r == 0) { r = String.Compare(a.Name, b.Name); }
                        return r;
                    });
                    break;
                case "NameWithSignature":
                    sortedList = FilterData.OrderBy(x => x.NameWithSignature).ToList();
                    break;
                case "NameMangled":
                    sortedList = FilterData.OrderBy(x => x.NameMangled).ToList();
                    break;
            }

            if(sortedList!=null) {
                FilterData = new BindingList<alcSymbolFilterColumn>(sortedList);
            }
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

        private void buttonDefault_Click(object sender, EventArgs e)
        {
            try
            {
                NeedsSave = false;

                List<alcSymbolFilterColumn> filter = SymbolFilterUtil.GetDefaultFilter(TargetFilePath);
                if (filter != null)
                {
                    filter.Sort(SymbolFilterUtil.CompareDataTypeOrName);
                    FilterData = new BindingList<alcSymbolFilterColumn>(filter);
                }
            }
            catch(Exception)
            {
            }
        }

        private void symbolGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            NeedsSave = true;
        }

        private void symbolDataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow row = symbolDataGridView.Rows[e.RowIndex];
            DataGridViewCell cell = row.Cells[e.ColumnIndex];
            if (e.ColumnIndex == 2)
            {
                String v = (String)cell.Value;
                if (v != "")
                {
                    row.Cells[0].Value = true;
                }
            }
            else if (e.ColumnIndex == 0)
            {
                bool v = (bool)cell.Value;
                if (!v) { row.Cells[2].Value = ""; }
            }
        }

        private void SymbolFilterWindow_Shown(object sender, EventArgs e)
        {
            MakeVariableCellsReadOnly();
        }

        private void MakeVariableCellsReadOnly()
        {
            foreach (DataGridViewRow row in symbolDataGridView.Rows)
            {
                alcSymbolFilterColumn sfc = row.DataBoundItem as alcSymbolFilterColumn;
                if (sfc != null)
                {
                    if (!sfc.IsFunction())
                    {
                        row.Cells[0].ReadOnly = true;
                        row.Cells[0].Style.BackColor = System.Drawing.Color.LightGray;
                        row.Cells[0].Style.ForeColor = System.Drawing.Color.DarkGray;
                        row.Cells[2].ReadOnly = true;
                    }
                }
            }
        }
    }


    public class SymbolFilterUtil
    {
        public static int CompareDataTypeOrName(alcSymbolFilterColumn a, alcSymbolFilterColumn b)
        {
            int r = String.Compare(a.AttrDataType, b.AttrDataType);
            if (r == 0) { r = String.Compare(a.Name, b.Name); }
            return r;
        }

        public static int CompareName(alcSymbolFilterColumn a, alcSymbolFilterColumn b)
        {
            return String.Compare(a.Name, b.Name);
        }


        public static List<alcSymbolFilterColumn> LoadFromConfigFile(String path_to_config)
        {
            List<alcSymbolFilterColumn> ret = null;
            try
            {
                StreamReader reader = new StreamReader(path_to_config);
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<alcSymbolFilterColumn>));
                    ret = (List<alcSymbolFilterColumn>)serializer.Deserialize(reader);
                    if (ret != null)
                    {
                        foreach(alcSymbolFilterColumn c in ret) {
                            c.SetupDemangledNames();
                        }
                    }
                }
                catch (Exception)
                {
                }
                reader.Close();
            }
            catch (Exception)
            {
            }
            return ret;
        }


        public static List<alcSymbolFilterColumn> GetDefaultFilter(String path_to_obj, bool show_error = true)
        {
            List<alcSymbolFilterColumn> ret = null;
            try
            {
                ret = AlcantareaHelper.GetDefaultSymbolFilter(path_to_obj, true);
            }
            catch (Exception e)
            {
                if (show_error)
                {
                    AlcantareaPackage.ShowMessage("Alcantarea: Error", e.Message);
                }
            }
            return ret;
        }


        private class CompareMangledName : IComparer<alcSymbolFilterColumn>
        {
            public int Compare(alcSymbolFilterColumn a, alcSymbolFilterColumn b) { return String.Compare(a.NameMangled, b.NameMangled); }
        }

        public static List<alcSymbolFilterColumn> MergeFilter(List<alcSymbolFilterColumn> base_filter, List<alcSymbolFilterColumn> addition)
        {
            base_filter.Sort((a, b) => { return String.Compare(a.NameMangled, b.NameMangled); });
            addition.Sort((a, b) => { return String.Compare(a.NameMangled, b.NameMangled); });
            CompareMangledName compare = new CompareMangledName();

            int search_begin = 0;
            for (int i = 0; i < addition.Count; ++i )
            {
                alcSymbolFilterColumn tmp = addition[i];
                int pos = base_filter.BinarySearch(search_begin, base_filter.Count - search_begin, tmp, compare);
                if(pos>=0) {
                    alcSymbolFilterColumn found = base_filter[pos];
                    found.FilterFlags = tmp.FilterFlags;
                    search_begin = pos + 1;
                }
            }
            return base_filter;
        }

        public static bool SaveConfig(String ConfigFilePath, List<alcSymbolFilterColumn> Filter)
        {
            try
            {
                StreamWriter writer = new StreamWriter(ConfigFilePath);
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<alcSymbolFilterColumn>));
                    serializer.Serialize(writer, AlcantareaHelper.ApplyIgnorePattern(Filter));
                }
                catch (Exception) { }
                writer.Close();
                return true;
            }
            catch (Exception)
            {
            }
            return false;
        }
    }



}
