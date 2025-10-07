using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        // Lưu số lượng từng món
        private Dictionary<string, int> orderCounts = new Dictionary<string, int>();

        public Form1()
        {
            InitializeComponent();
            InitializeMenuLogic();
        }

        // ------------------ Khởi tạo logic ------------------
        private void InitializeMenuLogic()
        {
            // Thiết lập DataGridView (nếu Designer chưa setup cột)
            if (dataGridView1.Columns.Count == 0)
            {
                dataGridView1.Columns.Add("colItem", "Món");
                dataGridView1.Columns.Add("colQty", "Số lượng");
                dataGridView1.ReadOnly = true;
                dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dataGridView1.AllowUserToAddRows = false;
                dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }

            // Nếu comboBox1 rỗng thì thêm bàn mẫu
            if (comboBox1 != null && comboBox1.Items.Count == 0)
            {
                for (int i = 1; i <= 12; i++) comboBox1.Items.Add($"Bàn {i}");
            }

            // Gán sự kiện cho button16 (Xóa) và button17 (Order) nếu Designer chưa gán
            var btnDel = this.Controls.Find("button16", true).FirstOrDefault() as Button;
            if (btnDel != null)
            {
                btnDel.Click -= BtnDelete_Click;
                btnDel.Click += BtnDelete_Click;
            }
            var btnOrder = this.Controls.Find("button17", true).FirstOrDefault() as Button;
            if (btnOrder != null)
            {
                btnOrder.Click -= BtnOrder_Click;
                btnOrder.Click += BtnOrder_Click;
            }

            // (Optional) Nếu bạn muốn, có thể gán tất cả button món vào MenuItem_Click ở đây,
            // nhưng vì Designer có thể đã gán button1_Click... nên ta cũng thêm stub cho an toàn.
        }

        // ------------------ Handler chung cho nút món ------------------
        private void MenuItem_Click(object sender, EventArgs e)
        {
            // Kiểm tra đã chọn bàn chưa
            if (comboBox1 == null || comboBox1.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng chọn bàn trước khi order.", "Chú ý", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var btn = sender as Button;
            if (btn == null) return;
            string item = btn.Text.Trim();
            if (string.IsNullOrEmpty(item)) return;

            if (!orderCounts.ContainsKey(item)) orderCounts[item] = 0;
            orderCounts[item] += 1;

            RefreshGrid();
        }

        // Cập nhật DataGridView
        private void RefreshGrid()
        {
            if (dataGridView1 == null) return;
            dataGridView1.Rows.Clear();
            foreach (var kv in orderCounts)
            {
                dataGridView1.Rows.Add(kv.Key, kv.Value);
            }
        }

        // ------------------ Xóa / Order ------------------
        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView1 == null) return;

            if (dataGridView1.SelectedRows.Count > 0)
            {
                var item = dataGridView1.SelectedRows[0].Cells[0].Value?.ToString();
                if (!string.IsNullOrEmpty(item) && orderCounts.ContainsKey(item))
                {
                    orderCounts.Remove(item);
                    RefreshGrid();
                }
            }
            else
            {
                var r = MessageBox.Show("Bạn có muốn xóa toàn bộ danh sách order?", "Xóa tất cả", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (r == DialogResult.Yes)
                {
                    orderCounts.Clear();
                    RefreshGrid();
                }
            }
        }

        private void BtnOrder_Click(object sender, EventArgs e)
        {
            if (comboBox1 == null || comboBox1.SelectedIndex == -1)
            {
                MessageBox.Show("Vui lòng chọn bàn trước khi Order.", "Chú ý", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (orderCounts.Count == 0)
            {
                MessageBox.Show("Danh sách order rỗng.", "Chú ý", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string table = comboBox1.SelectedItem.ToString();
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            using (var sfd = new SaveFileDialog())
            {
                sfd.FileName = $"Order_{table.Replace(' ', '_')}_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.txt";
                sfd.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                if (sfd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    using (var sw = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                    {
                        sw.WriteLine("Quán: Quán ăn nhanh Hưng Thịnh");
                        sw.WriteLine($"Bàn: {table}");
                        sw.WriteLine($"Thời gian: {timestamp}");
                        sw.WriteLine(new string('-', 40));
                        sw.WriteLine("Món\tSố lượng");
                        foreach (var kv in orderCounts)
                        {
                            sw.WriteLine($"{kv.Key}\t{kv.Value}");
                        }
                        sw.WriteLine(new string('-', 40));
                        sw.WriteLine("Ghi chú: (file order được ghi từ PDA)");
                    }

                    MessageBox.Show($"Đã lưu order vào file:\n{sfd.FileName}", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Sau khi order -> xóa danh sách
                    orderCounts.Clear();
                    RefreshGrid();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi ghi file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ------------------ Các stub tương thích với Designer ------------------
        // Nếu Designer của bạn gán event Click là button1_Click, button2_Click... thì các stub này sẽ
        // gọi lại MenuItem_Click để xử lý chung (tăng 1 số lượng).
        private void button1_Click(object sender, EventArgs e) { MenuItem_Click(sender, e); }
        private void button2_Click(object sender, EventArgs e) { MenuItem_Click(sender, e); }
        private void button3_Click(object sender, EventArgs e) { MenuItem_Click(sender, e); }
        private void button4_Click(object sender, EventArgs e) { MenuItem_Click(sender, e); }
        private void button5_Click(object sender, EventArgs e) { MenuItem_Click(sender, e); }
        private void button6_Click(object sender, EventArgs e) { MenuItem_Click(sender, e); }
        private void button7_Click(object sender, EventArgs e) { MenuItem_Click(sender, e); }
        private void button8_Click(object sender, EventArgs e) { MenuItem_Click(sender, e); }
        private void button9_Click(object sender, EventArgs e) { MenuItem_Click(sender, e); }
        private void button10_Click(object sender, EventArgs e) { MenuItem_Click(sender, e); }
        private void button11_Click(object sender, EventArgs e) { MenuItem_Click(sender, e); }
        private void button12_Click(object sender, EventArgs e) { MenuItem_Click(sender, e); }
        private void button13_Click(object sender, EventArgs e) { MenuItem_Click(sender, e); }
        private void button14_Click(object sender, EventArgs e) { MenuItem_Click(sender, e); }
        private void button15_Click(object sender, EventArgs e) { MenuItem_Click(sender, e); }

        // Nếu Designer gán button16_Click / button17_Click:
        private void button16_Click(object sender, EventArgs e) { BtnDelete_Click(sender, e); }
        private void button17_Click(object sender, EventArgs e) { BtnOrder_Click(sender, e); }

        // (Designer trước đó có thể tạo handler label1_Click) — giữ lại để tránh lỗi nếu có
        private void label1_Click(object sender, EventArgs e) { /* không dùng */ }

        private void button11_Click_1(object sender, EventArgs e)
        {

        }

        private void button7_Click_1(object sender, EventArgs e)
        {

        }
    }
}