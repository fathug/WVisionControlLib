using System;
using System.Drawing;
using System.Windows.Forms;

namespace Test
{
    public partial class Form1 : Form
    {
        private Image originalImage;  // 原始图像

        public Form1()
        {
            InitializeComponent();
        }

        private void 打开ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.gif;*.bmp";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // 释放旧的图像资源
                    if (originalImage != null)
                    {
                        originalImage.Dispose(); // 释放旧的图像资源
                    }

                    // 加载图片
                    originalImage = Image.FromFile(openFileDialog.FileName);

                    // 设置 PictureBox 的 Image
                    pictureboxZoomDrag1.Image = originalImage;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
