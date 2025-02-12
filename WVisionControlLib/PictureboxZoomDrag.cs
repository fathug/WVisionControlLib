using System;
using System.Drawing;
using System.Windows.Forms;

namespace WVisionControlLib
{
    public partial class PictureboxZoomDrag : UserControl
    {
        private Image originalImage;
        private float zoomFactor = 1.0f;
        private float initialZoomFactor = 1.0f;
        private PointF imageOffset = PointF.Empty;
        private Point mouseDownLocation;
        private bool isDragging = false;

        public PictureboxZoomDrag()
        {
            InitializeComponent();

            // 启用双缓冲
            //this.DoubleBuffered = true;

            //pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;

            // 注意：事件绑定现在针对内部的 pictureBox1
            pictureBox1.MouseWheel += PictureBox1_MouseWheel;
            pictureBox1.MouseDown += PictureBox1_MouseDown;
            pictureBox1.MouseMove += PictureBox1_MouseMove;
            pictureBox1.MouseUp += PictureBox1_MouseUp;
            pictureBox1.DoubleClick += PictureBox1_DoubleClick;
            pictureBox1.Paint += PictureBox1_Paint;
            pictureBox1.Resize += PictureBox1_Resize;
        }

        // Image 属性 (用于从外部设置图片)
        public Image Image
        {
            get { return pictureBox1.Image; }
            set
            {
                if (originalImage != null)
                {
                    originalImage.Dispose();
                }
                originalImage = value;
                pictureBox1.Image = originalImage;

                if (originalImage != null)
                {
                    // 初始的缩放比例和偏移
                    var zoomFactorW = (float)pictureBox1.Width / originalImage.Width;
                    var zoomFactorH = (float)pictureBox1.Height / originalImage.Height;
                    initialZoomFactor = zoomFactorW < zoomFactorH ? zoomFactorW : zoomFactorH;
                    zoomFactor = initialZoomFactor;
                    imageOffset = PointF.Empty;

                    CalculateInitialOffset();
                }
                else
                {
                    zoomFactor = 1.0f;
                    imageOffset = PointF.Empty;
                }

                pictureBox1.Invalidate();
            }
        }

        private void PictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            // 计算新的缩放系数
            float scale = e.Delta > 0 ? 1.1f : 0.9f;
            float previousZoomFactor = zoomFactor;

            zoomFactor *= scale;
            zoomFactor = Math.Max(0.1f, zoomFactor);
            zoomFactor = Math.Min(5.0f, zoomFactor);

            if (Math.Abs(zoomFactor - previousZoomFactor) < 0.001)
            {
                return;
            }

            // 计算有效的缩放系数
            float effectiveScale = zoomFactor / previousZoomFactor;

            Point mouseLocation = pictureBox1.PointToClient(Cursor.Position);
            float oldCenterX = pictureBox1.Width / 2 - imageOffset.X;
            float oldCenterY = pictureBox1.Height / 2 - imageOffset.Y;

            // 使用 effectiveScale 计算新的中心点坐标
            float newCenterX = oldCenterX * effectiveScale;
            float newCenterY = oldCenterY * effectiveScale;

            imageOffset.X -= (newCenterX - oldCenterX);
            imageOffset.Y -= (newCenterY - oldCenterY);

            pictureBox1.Invalidate();
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mouseDownLocation = e.Location;
                isDragging = true;
            }
        }

        private void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                // 更新图像位置之前调用SuspendLayout()，在更新完成之后调用ResumeLayout()
                // 这将暂时挂起布局逻辑，一次性应用所有更改，减少不必要的重绘次数
                pictureBox1.SuspendLayout(); // 暂停布局

                // 计算偏移量
                imageOffset.X += (e.X - mouseDownLocation.X);
                imageOffset.Y += (e.Y - mouseDownLocation.Y);

                mouseDownLocation = e.Location;  // 更新鼠标位置
                pictureBox1.Invalidate();

                pictureBox1.ResumeLayout(false); // 恢复布局并立即重绘
            }
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDragging = false;
            }
        }

        /// <summary>
        /// 双击图片重置
        /// </summary>
        private void PictureBox1_DoubleClick(object sender, EventArgs e)
        {
            var zoomFactorW = (float)pictureBox1.Width / originalImage.Width;
            var zoomFactorH = (float)pictureBox1.Height / originalImage.Height;
            zoomFactor = zoomFactorW < zoomFactorH ? zoomFactorW : zoomFactorH; imageOffset = PointF.Empty;
            CalculateInitialOffset();   //重新计算,使其居中
            pictureBox1.Invalidate();   // 触发重绘
        }

        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (originalImage != null)
            {
                e.Graphics.Clear(pictureBox1.BackColor); // 清除背景

                // 计算缩放后的图像尺寸
                int newWidth = (int)(originalImage.Width * zoomFactor);
                int newHeight = (int)(originalImage.Height * zoomFactor);

                // 计算绘制位置（应用偏移量）
                float drawX = imageOffset.X;
                float drawY = imageOffset.Y;

                // 缩放时的两种处理办法
                // 1不平滑处理，可看到像素颗粒
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

                /*
                // 2平滑处理后显示
                //设置高质量插值法
                e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                //设置高质量,低速度呈现平滑程度
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                //设置高质量的反锯齿呈现
                e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                */

                // 绘制
                e.Graphics.DrawImage(originalImage,
                    new RectangleF(drawX, drawY, newWidth, newHeight),
                    new RectangleF(0, 0, originalImage.Width, originalImage.Height),
                    GraphicsUnit.Pixel);
            }
        }

        /// <summary>
        /// 程序运行时，若控件大小发生变化该函数就会执行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBox1_Resize(object sender, EventArgs e)
        {
            CalculateInitialOffset(); // 当 PictureBox 大小改变时重新计算居中偏移
            pictureBox1.Invalidate();
        }

        private void CalculateInitialOffset()
        {
            if (originalImage == null) return;

            imageOffset.X = (pictureBox1.Width - originalImage.Width * zoomFactor) / 2;
            imageOffset.Y = (pictureBox1.Height - originalImage.Height * zoomFactor) / 2;
        }

        /// <summary>
        ///  公开事件。用户可以在触发以下事件的同时，做其他的工作。比如触发双击的时候显示一个“重置图像”的提示
        /// </summary>
        #region
        public event MouseEventHandler MouseWheel
        {
            add { pictureBox1.MouseWheel += value; }
            remove { pictureBox1.MouseWheel -= value; }
        }

        public new event MouseEventHandler MouseDown //加new是因为UserControl也有个MouseDown事件
        {
            add { pictureBox1.MouseDown += value; }
            remove { pictureBox1.MouseDown -= value; }
        }

        public new event MouseEventHandler MouseMove
        {
            add { pictureBox1.MouseMove += value; }
            remove { pictureBox1.MouseMove -= value; }
        }

        public new event MouseEventHandler MouseUp
        {
            add { pictureBox1.MouseUp += value; }
            remove { pictureBox1.MouseUp -= value; }
        }
        public new event EventHandler DoubleClick
        {
            add { pictureBox1.DoubleClick += value; }
            remove { pictureBox1.DoubleClick -= value; }
        }

        public event PaintEventHandler Paint
        {
            add { pictureBox1.Paint += value; }
            remove { pictureBox1.Paint -= value; }
        }
        public new event EventHandler Resize
        {
            add { pictureBox1.Resize += value; }
            remove { pictureBox1.Resize -= value; }
        }
        #endregion
    }
}
