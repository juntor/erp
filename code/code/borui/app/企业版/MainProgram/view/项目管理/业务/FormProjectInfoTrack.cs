﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using MainProgram.model;
using MainProgram.bus;

namespace MainProgram
{
    public partial class FormProjectInfoTrack : Form
    {
        public enum OrderType
        {
            // 设备总材料表
            DevMaterielInfo,
            EleMaterielInfo,
            EngMaterielInfo,
            ALL
        };

        private int m_dataGridRecordCount = 0;
        private int m_orderType;
        private OrderType m_orderTypeEnum;
        private string m_billNumber = "";
        private string m_projectNum = "";
        private bool m_isSelectOrderNumber;

        private RowMergeView m_dateGridViewExtend = new RowMergeView();
        private FormStorageSequenceFilterValue m_filter = new FormStorageSequenceFilterValue();
        private FormProjectInfoTrackFilterValue m_projectInfoTrackFilter = new FormProjectInfoTrackFilterValue();
        private SortedDictionary<int, FormProjectMaterielTable> m_dataList = new SortedDictionary<int, FormProjectMaterielTable>();

        public FormProjectInfoTrack(OrderType orderType, bool isSelectOrderNumber = false)
        {
            InitializeComponent();
            m_orderTypeEnum = orderType;

            if (m_orderTypeEnum == OrderType.DevMaterielInfo)
            {
                this.Text = "设备总材料表跟踪情况";
                m_orderType = 1;
            }
            else if (m_orderTypeEnum == OrderType.EleMaterielInfo)
            {
                this.Text = "电器总材料表跟踪情况";
                m_orderType = 2;
            }
            else if (m_orderTypeEnum == OrderType.EngMaterielInfo)
            {
                this.Text = "工程总材料表跟踪情况";
                m_orderType = 3;
            }
            else if (m_orderTypeEnum == OrderType.ALL)
            {
                this.Text = "项目整体情况跟踪情况";
                m_orderType = 4;
            }

            m_isSelectOrderNumber = isSelectOrderNumber;

            FormProjectInfoTrackFilter fssf = new FormProjectInfoTrackFilter(false);

            if (fssf.ShowDialog() == DialogResult.OK)
            {
                m_projectInfoTrackFilter = fssf.getFilterUIValue();
            }
        }

        private void FormProjectInfoTrack_Load(object sender, EventArgs e)
        {
            this.rowMergeView1.ColumnHeadersHeight = 40;
            this.rowMergeView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.rowMergeView1.MergeColumnNames.Add("单据编号");
            this.rowMergeView1.MergeColumnNames.Add("设备型号");
            this.rowMergeView1.AddSpanHeader(3, 4, "物料基本信息");
            this.rowMergeView1.AddSpanHeader(7, 3, "库存情况");

            updateDataGridView();
        }

        private void updateDataGridView()
        {
            SortedDictionary<int, ProjectManagerDetailsTable> listDetails = new SortedDictionary<int, ProjectManagerDetailsTable>();
            SortedDictionary<int, ArrayList> sortedDictionaryList = new SortedDictionary<int, ArrayList>();

            SortedDictionary<int, FormProjectMaterielTable> list = new SortedDictionary<int, FormProjectMaterielTable>();
            list = FormProject.getInctance().getAllPurchaseOrderInfo(
                m_projectInfoTrackFilter.projectNum, m_projectInfoTrackFilter.allReview, m_orderType);;

            DataTable dt = new DataTable();
            dt.Columns.Add("单据编号");
            dt.Columns.Add("设备型号");
            dt.Columns.Add("所属部件");

            dt.Columns.Add("物料编码");
            dt.Columns.Add("物料名称");
            dt.Columns.Add("型号");
            dt.Columns.Add("数量");

            dt.Columns.Add("实际库存");
            dt.Columns.Add("预占库存");
            dt.Columns.Add("可用库存");

            dt.Columns.Add("转采购申请数量");
            dt.Columns.Add("采购订单数量");
            dt.Columns.Add("采购入库数量");
            dt.Columns.Add("生产领料数量");
            
            for (int index = 0; index < list.Count; index++)
            {
                FormProjectMaterielTable record = new FormProjectMaterielTable();
                record = (FormProjectMaterielTable)list[index];

                listDetails.Clear();
                listDetails = ProjectManagerDetails.getInctance().getPurchaseInfoFromBillNumber(record.billNumber);

                for (int index2 = 0; index2 < listDetails.Count; index2++)
                {
                    ProjectManagerDetailsTable tmp = new ProjectManagerDetailsTable();
                    tmp = (ProjectManagerDetailsTable)listDetails[index2];

                    dt.Rows.Add(record.billNumber, record.deviceMode, record.deviceName, 
                        tmp.materielID, tmp.materielName,tmp.materielModel, tmp.value);
                }
            }

            this.rowMergeView1.DataSource = dt;
        }

        private void billDetail_Click(object sender, EventArgs e)
        {
            checkAccountBillDetaile();
        }

        private void export_Click(object sender, EventArgs e)
        {
            // 此处需要添加导入DataGridViewer数据到Excel的功能
            if (m_dataGridRecordCount > 0)
            {
                this.saveFileDialog1.Filter = "Excel 2007格式 (*.xlsx)|*.xlsx|Excel 2003格式 (*.xls)|*.xls";
                this.saveFileDialog1.RestoreDirectory = true;

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                   // m_dateGridViewExtend.dataGridViewExportToExecl(saveFileDialog1.FileName);
                }
            }
            else
            {
                MessageBoxExtend.messageWarning("数据为空，无数据可导出!");
            }
        }

        private void print_Click(object sender, EventArgs e)
        {
            //m_dateGridViewExtend.printDataGridView();
        }

        private void close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dataGridViewBilConfigList_Click(object sender, EventArgs e)
        {
            try
            {
                if (m_dataGridRecordCount > 0)
                {
                    // 当单击某个单元格时，自动选择整行
                    for (int i = 0; i < this.dataGridViewList.RowCount; i++)
                    {
                        for (int j = 0; j < dataGridViewList.ColumnCount; j++)
                        {
                            if (dataGridViewList.Rows[i].Cells[j].Selected)
                            {
                                dataGridViewList.Rows[i].Selected = true;
                                m_billNumber = dataGridViewList.Rows[i].Cells[3].Value.ToString();
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
 
            }
        }

        private void dataGridViewMaterielList_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (m_dataGridRecordCount > 0)
                {
                    // 当单击某个单元格时，自动选择整行
                    for (int i = 0; i < this.dataGridViewList.RowCount; i++)
                    {
                        for (int j = 0; j < dataGridViewList.ColumnCount; j++)
                        {
                            if (dataGridViewList.Rows[i].Cells[j].Selected)
                            {
                                dataGridViewList.Rows[i].Selected = true;
                                m_billNumber = dataGridViewList.Rows[i].Cells[3].Value.ToString();
                                m_projectNum = dataGridViewList.Rows[i].Cells[4].Value.ToString();
                                checkAccountBillDetaile();
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        private void checkAccountBillDetaile()
        {
            if (m_isSelectOrderNumber)
            {
                this.Close();
                return;
            }

            // checkAccountBillDetaile函数需要完成弹出一个新的窗口，用来显示单据编号关联的具体单据
            //FormProjectMaterielOrder fpmo = new FormProjectMaterielOrder(m_dataType, m_billNumber);
            //fpmo.ShowDialog();
            //updateDataGridView();

            //}
            //else
            //{
            //    MessageBoxExtend.messageWarning("暂时不支持的序时薄类型");
            //}
        }

        public string getSelectOrderNumber()
        {
            return m_billNumber;
        }

        public string getSelectOrderProjectNum()
        {
            return m_projectNum;
        }

        public void setDataFilter(FormStorageSequenceFilterValue filter)
        {
            m_filter = filter;
        }

        private void toolStripButtonRefresh_Click(object sender, EventArgs e)
        {
            //// 刷新按钮逻辑
            //if (m_orderType == OrderType.PurchaseOrder)
            //{
            //    PurchaseOrder.getInctance().refreshRecord();
            //}
            //else if (m_orderType == OrderType.PurchaseIn)
            //{
            //    PurchaseInOrder.getInctance().refreshRecord();
            //}
            //else if (m_orderType == OrderType.PurchaseInvoice)
            //{
            //}
            //else if (m_orderType == OrderType.PurchaseOrderExcute)
            //{
            //    PurchaseOrder.getInctance().refreshRecord();
            //}
            //else if (m_orderType == OrderType.PurchaseInOrderExcute)
            //{
            //    PurchaseInOrder.getInctance().refreshRecord();
            //}
            //else if (m_orderType == OrderType.StorageProductIn)
            //{
            //    // 仓存管理-产品入库
            //    MaterielInOrder.getInctance().refreshRecord();
            //}
            //else if (m_orderType == OrderType.StorageInCheck)
            //{
            //    // 仓存管理-盘盈入库
            //    MaterielInEarningsOrder.getInctance().refreshRecord();
            //}
            //else if (m_orderType == OrderType.StorageInOther)
            //{
            //    // 仓存管理-其他入库
            //    MaterielInOtherOrder.getInctance().refreshRecord();
            //}

            updateDataGridView();
        }
    }
}