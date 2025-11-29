using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace StudentManager
{
    public partial class MainForm : MaterialForm
    {
        private List<Student> students = new List<Student>();
        private Student selectedStudent = null;

        // DÙNG TextBox thường để gõ tiếng Việt
        private TextBox txtId, txtName, txtClass, txtScore, txtSearch;
        private DataGridView dgvStudents;
        private MaterialLabel lblTotal, lblPass, lblAvg;

        public MainForm()
        {
            // Không dùng InitializeComponent()
            SetupMaterialSkin();
            InitializeUI();
            RefreshDataGridView();
            UpdateStatistics();
        }

        private void SetupMaterialSkin()
        {
            var skin = MaterialSkinManager.Instance;
            skin.AddFormToManage(this);
            skin.Theme = MaterialSkinManager.Themes.LIGHT;
            skin.ColorScheme = new ColorScheme(
                Primary.Blue700, Primary.Blue900,
                Primary.Blue200, Accent.LightBlue200,
                TextShade.BLACK);
        }

        private void InitializeUI()
        {
            Font vietFont = new System.Drawing.Font("Arial", 10);

            Text = "Ứng dụng Quản lý Sinh viên";
            Size = new System.Drawing.Size(860, 630);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Font = vietFont;

          

            // Panel nhập liệu
            var inputPanel = new Panel { Location = new System.Drawing.Point(20, 80), Size = new System.Drawing.Size(820, 120) };

            // --- TextBox thường (hỗ trợ tiếng Việt) ---
            txtId = CreateTextBox("Mã SV", new System.Drawing.Point(10, 10), 150);
            txtName = CreateTextBox("Họ và tên", new System.Drawing.Point(180, 10), 220);
            txtClass = CreateTextBox("Lớp", new System.Drawing.Point(420, 10), 120);
            txtScore = CreateTextBox("Điểm (0-10)", new System.Drawing.Point(560, 10), 120);

            // Button (vẫn dùng MaterialButton)
            var btnAdd = new MaterialButton { Text = "Thêm", Location = new System.Drawing.Point(10, 60), Width = 90, Height = 40 };
            var btnEdit = new MaterialButton { Text = "Cập nhật", Location = new System.Drawing.Point(110, 60), Width = 90, Height = 40 };
            var btnDelete = new MaterialButton { Text = "Xóa", Location = new System.Drawing.Point(210, 60), Width = 90, Height = 40 };
            var btnClear = new MaterialButton { Text = "Làm mới", Location = new System.Drawing.Point(310, 60), Width = 90, Height = 40 };

            txtSearch = CreateTextBox("Tìm theo tên hoặc lớp", new System.Drawing.Point(420, 65), 240);
            var btnSearch = new MaterialButton { Text = "Tìm", Location = new System.Drawing.Point(670, 60), Width = 80, Height = 40 };

            inputPanel.Controls.AddRange(new Control[] {
                txtId, txtName, txtClass, txtScore,
                btnAdd, btnEdit, btnDelete, btnClear,
                txtSearch, btnSearch
            });

            // DataGridView
            dgvStudents = new DataGridView
            {
                Location = new System.Drawing.Point(20, 220),
                Size = new System.Drawing.Size(820, 220),
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                Font = vietFont
            };

            // Panel hành động
            var actionPanel = new Panel { Location = new System.Drawing.Point(20, 460), Size = new System.Drawing.Size(820, 60) };
            var btnSave = new MaterialButton { Text = "Lưu file", Location = new System.Drawing.Point(10, 10), Width = 100 };
            var btnLoad = new MaterialButton { Text = "Mở file", Location = new System.Drawing.Point(120, 10), Width = 100 };
            var btnSort = new MaterialButton { Text = "Sắp xếp ↓", Location = new System.Drawing.Point(230, 10), Width = 110 };
            var btnAverage = new MaterialButton { Text = "Điểm TB", Location = new System.Drawing.Point(350, 10), Width = 100 };
            var btnStatistic = new MaterialButton { Text = "Thống kê", Location = new System.Drawing.Point(460, 10), Width = 100 };

            lblTotal = new MaterialLabel { Location = new System.Drawing.Point(580, 15), AutoSize = true, Font = vietFont };
            lblPass = new MaterialLabel { Location = new System.Drawing.Point(680, 15), AutoSize = true, Font = vietFont };
            lblAvg = new MaterialLabel { Location = new System.Drawing.Point(780, 15), AutoSize = true, Font = vietFont };

            actionPanel.Controls.AddRange(new Control[] {
                btnSave, btnLoad, btnSort, btnAverage, btnStatistic,
                lblTotal, lblPass, lblAvg
            });

            Controls.AddRange(new Control[] {  inputPanel, dgvStudents, actionPanel });

            // Gắn sự kiện
            btnAdd.Click += btnAdd_Click;        // SV1
            btnEdit.Click += btnEdit_Click;      // SV1
            btnDelete.Click += btnDelete_Click;  // SV2
            btnSearch.Click += btnSearch_Click;  // SV2
            btnSave.Click += btnSave_Click;      // SV3
            btnLoad.Click += btnLoad_Click;      // SV3
            btnSort.Click += btnSort_Click;      // SV4
            btnAverage.Click += btnAverage_Click; // SV4
            btnClear.Click += btnClear_Click;    // SV5
            btnStatistic.Click += btnStatistic_Click; // SV5

            dgvStudents.CellClick += dgvStudents_CellClick;
        }

        // Helper: Tạo TextBox có placeholder
        private TextBox CreateTextBox(string placeholder, System.Drawing.Point location, int width)
        {
            var txt = new TextBox
            {
                Location = location,
                Width = width,
                Font = new System.Drawing.Font("Arial", 10),
                ImeMode = ImeMode.On // ← Cho phép gõ tiếng Việt có dấu
            };

            // Hiệu ứng placeholder
            txt.Text = placeholder;
            txt.ForeColor = System.Drawing.Color.Gray;

            txt.Enter += (s, e) =>
            {
                if (txt.Text == placeholder)
                {
                    txt.Text = "";
                    txt.ForeColor = System.Drawing.Color.Black;
                }
            };

            txt.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txt.Text))
                {
                    txt.Text = placeholder;
                    txt.ForeColor = System.Drawing.Color.Gray;
                }
            };

            return txt;
        }

        // === HÀM HỖ TRỢ ===
        private void RefreshDataGridView()
        {
            dgvStudents.Rows.Clear();
            dgvStudents.Columns.Clear();
            dgvStudents.Columns.Add("Id", "Mã SV");
            dgvStudents.Columns.Add("Name", "Họ tên");
            dgvStudents.Columns.Add("Class", "Lớp");
            dgvStudents.Columns.Add("Score", "Điểm");
            dgvStudents.Columns.Add("Status", "Trạng thái");

            foreach (var s in students)
                dgvStudents.Rows.Add(s.Id, s.Name, s.Class, s.Score.ToString("F1"), s.Status);
        }

        private void UpdateStatistics()
        {
            int total = students.Count;
            int pass = students.Count(s => s.Score >= 5.0);
            double avg = total > 0 ? students.Average(s => s.Score) : 0;
            lblTotal.Text = $"Tổng: {total}";
            lblPass.Text = $"Đạt: {pass}";
            lblAvg.Text = $"TB: {avg:F2}";
        }

        private bool ValidateInput(out string id, out string name, out string cls, out double score)
        {
            id = GetRealText(txtId, "Mã SV");
            name = GetRealText(txtName, "Họ và tên");
            cls = GetRealText(txtClass, "Lớp");
            string scoreStr = GetRealText(txtScore, "Điểm (0-10)");

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(cls) || string.IsNullOrEmpty(scoreStr))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                score = 0;
                return false;
            }

            if (!double.TryParse(scoreStr, out score) || score < 0 || score > 10)
            {
                MessageBox.Show("Điểm phải là số từ 0 đến 10!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true;
        }

        // Lấy nội dung thật (bỏ placeholder)
        private string GetRealText(TextBox txt, string placeholder)
        {
            string value = txt.Text;
            return value == placeholder ? "" : value.Trim();
        }

        // === 10 SỰ KIỆN THEO PHÂN CÔNG NHÓM ===
        private void btnAdd_Click(object sender, EventArgs e) // SV1
        {
            if (!ValidateInput(out string id, out string name, out string cls, out double score)) return;
            if (students.Any(s => s.Id == id)) { MessageBox.Show("Mã SV đã tồn tại!"); return; }
            students.Add(new Student { Id = id, Name = name, Class = cls, Score = score });
            RefreshDataGridView(); UpdateStatistics(); btnClear_Click(null, null);
            MessageBox.Show("Thêm sinh viên thành công!");
        }

        private void btnEdit_Click(object sender, EventArgs e) // SV1
        {
            if (selectedStudent == null) { MessageBox.Show("Vui lòng chọn sinh viên để sửa!"); return; }
            if (!ValidateInput(out string id, out string name, out string cls, out double score)) return;
            if (id != selectedStudent.Id && students.Any(s => s.Id == id)) { MessageBox.Show("Mã SV đã tồn tại!"); return; }
            selectedStudent.Id = id;
            selectedStudent.Name = name;
            selectedStudent.Class = cls;
            selectedStudent.Score = score;
            RefreshDataGridView(); UpdateStatistics(); btnClear_Click(null, null);
            MessageBox.Show("Cập nhật thành công!");
        }

        private void btnDelete_Click(object sender, EventArgs e) // SV2
        {
            if (selectedStudent == null) { MessageBox.Show("Chọn sinh viên để xóa!"); return; }
            students.Remove(selectedStudent);
            selectedStudent = null;
            RefreshDataGridView(); UpdateStatistics(); btnClear_Click(null, null);
            MessageBox.Show("Xóa sinh viên thành công!");
        }

        private void btnSearch_Click(object sender, EventArgs e) // SV2
        {
            string k = GetRealText(txtSearch, "Tìm theo tên hoặc lớp").ToLower();
            var filtered = string.IsNullOrEmpty(k) ? students : students.Where(s =>
                s.Name.ToLower().Contains(k) || s.Class.ToLower().Contains(k)).ToList();
            dgvStudents.Rows.Clear();
            foreach (var s in filtered)
                dgvStudents.Rows.Add(s.Id, s.Name, s.Class, s.Score.ToString("F1"), s.Status);
        }

        private void btnSave_Click(object sender, EventArgs e) // SV3
        {
            using (var sfd = new SaveFileDialog { Filter = "CSV files (*.csv)|*.csv" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var w = new StreamWriter(sfd.FileName, false, System.Text.Encoding.UTF8))
                            foreach (var s in students)
                                w.WriteLine($"{s.Id},{s.Name},{s.Class},{s.Score}");
                        MessageBox.Show("Lưu file thành công!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi lưu file:\n" + ex.Message, "Lỗi");
                    }
                }
            }
        }

        private void btnLoad_Click(object sender, EventArgs e) // SV3
        {
            using (var ofd = new OpenFileDialog { Filter = "CSV files (*.csv)|*.csv" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        students.Clear();
                        foreach (var line in File.ReadAllLines(ofd.FileName, System.Text.Encoding.UTF8))
                        {
                            if (string.IsNullOrWhiteSpace(line)) continue;
                            var p = line.Split(',');
                            if (p.Length == 4 && double.TryParse(p[3], out double sc))
                                students.Add(new Student { Id = p[0].Trim(), Name = p[1].Trim(), Class = p[2].Trim(), Score = sc });
                        }
                        RefreshDataGridView(); UpdateStatistics();
                        MessageBox.Show("Mở file thành công!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi mở file:\n" + ex.Message, "Lỗi");
                    }
                }
            }
        }

        private void btnSort_Click(object sender, EventArgs e) // SV4
        {
            students = students.OrderByDescending(s => s.Score).ToList();
            RefreshDataGridView();
            ((MaterialButton)sender).Text = "Sắp xếp ↓";
        }

        private void btnAverage_Click(object sender, EventArgs e) // SV4
        {
            double avg = students.Count > 0 ? students.Average(s => s.Score) : 0;
            MessageBox.Show($"Điểm trung bình: {avg:F2}", "Điểm trung bình");
        }

        private void btnClear_Click(object sender, EventArgs e) // SV5
        {
            // Reset về placeholder
            txtId.Text = "Mã SV"; txtId.ForeColor = System.Drawing.Color.Gray;
            txtName.Text = "Họ và tên"; txtName.ForeColor = System.Drawing.Color.Gray;
            txtClass.Text = "Lớp"; txtClass.ForeColor = System.Drawing.Color.Gray;
            txtScore.Text = "Điểm (0-10)"; txtScore.ForeColor = System.Drawing.Color.Gray;
            selectedStudent = null;
        }

        private void btnStatistic_Click(object sender, EventArgs e) // SV5
        {
            UpdateStatistics();
            MessageBox.Show(
                $"Tổng sinh viên: {students.Count}\n" +
                $"Sinh viên đạt: {students.Count(s => s.Score >= 5)}\n" +
                $"Sinh viên không đạt: {students.Count(s => s.Score < 5)}",
                "Thống kê sinh viên"
            );
        }

        private void dgvStudents_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var id = dgvStudents.Rows[e.RowIndex].Cells["Id"].Value?.ToString();
                selectedStudent = students.FirstOrDefault(s => s.Id == id);
                if (selectedStudent != null)
                {
                    txtId.Text = selectedStudent.Id;
                    txtName.Text = selectedStudent.Name;
                    txtClass.Text = selectedStudent.Class;
                    txtScore.Text = selectedStudent.Score.ToString("F1");
                    // Đặt màu đen để phân biệt với placeholder
                    txtId.ForeColor = txtName.ForeColor = txtClass.ForeColor = txtScore.ForeColor = System.Drawing.Color.Black;
                }
            }
        }
    }
}