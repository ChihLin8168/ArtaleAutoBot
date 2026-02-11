namespace ArtaleAutoBot
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        // --- UI 元件定義 ---
        private System.Windows.Forms.TextBox txtS1, txtS2, txtS3, txtS4, txtS5;
        private System.Windows.Forms.TextBox txtInterval;
        private System.Windows.Forms.TextBox txtLeftSec, txtRightSec, txtWaitSec; // 新增參數
        private System.Windows.Forms.Label lblMousePos, lblStatus, lblCountdown, lblHint;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.txtS1 = new System.Windows.Forms.TextBox();
            this.txtS2 = new System.Windows.Forms.TextBox();
            this.txtS3 = new System.Windows.Forms.TextBox();
            this.txtS4 = new System.Windows.Forms.TextBox();
            this.txtS5 = new System.Windows.Forms.TextBox();
            this.txtInterval = new System.Windows.Forms.TextBox();
            this.txtLeftSec = new System.Windows.Forms.TextBox();
            this.txtRightSec = new System.Windows.Forms.TextBox();
            this.txtWaitSec = new System.Windows.Forms.TextBox();
            this.lblMousePos = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblCountdown = new System.Windows.Forms.Label();
            this.lblHint = new System.Windows.Forms.Label();

            // --- 設定視窗 ---
            this.Text = "Artale 自動化腳本 (防偵測隨機版)";
            this.Size = new System.Drawing.Size(420, 600);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

            // --- 技能輸入框 (1-5) ---
            int startY = 20;
            System.Windows.Forms.TextBox[] boxes = { txtS1, txtS2, txtS3, txtS4, txtS5 };
            for (int i = 0; i < 5; i++)
            {
                boxes[i].Location = new System.Drawing.Point(30, startY + (i * 30));
                boxes[i].Size = new System.Drawing.Size(60, 22);
                this.Controls.Add(boxes[i]);

                Label lbl = new Label() { Text = $"技能 {i + 1} 按鍵", Location = new Point(100, startY + (i * 30)), AutoSize = true };
                this.Controls.Add(lbl);
            }

            // --- 參數設定區 ---
            int paramY = 180;

            // 1. 左移秒數
            this.txtLeftSec.Location = new System.Drawing.Point(30, paramY);
            this.txtLeftSec.Size = new System.Drawing.Size(60, 22);
            this.txtLeftSec.Text = "5";
            this.Controls.Add(new Label() { Text = "向左移動 (秒):", Location = new Point(100, paramY), AutoSize = true });
            this.Controls.Add(txtLeftSec);

            // 2. 右移秒數
            this.txtRightSec.Location = new System.Drawing.Point(30, paramY + 30);
            this.txtRightSec.Size = new System.Drawing.Size(60, 22);
            this.txtRightSec.Text = "2";
            this.Controls.Add(new Label() { Text = "向右移動 (秒):", Location = new Point(100, paramY + 30), AutoSize = true });
            this.Controls.Add(txtRightSec);

            // 3. 施放前等待
            this.txtWaitSec.Location = new System.Drawing.Point(30, paramY + 60);
            this.txtWaitSec.Size = new System.Drawing.Size(60, 22);
            this.txtWaitSec.Text = "10";
            this.Controls.Add(new Label() { Text = "施放前等待 (秒):", Location = new Point(100, paramY + 60), AutoSize = true });
            this.Controls.Add(txtWaitSec);

            // 4. 總循環時間
            this.txtInterval.Location = new System.Drawing.Point(30, paramY + 90);
            this.txtInterval.Size = new System.Drawing.Size(60, 22);
            this.txtInterval.Text = "240";
            this.Controls.Add(new Label() { Text = "總循環間隔 (秒):", Location = new Point(100, paramY + 90), AutoSize = true });
            this.Controls.Add(txtInterval);

            // --- 分隔線 ---
            Label line = new Label()
            {
                BorderStyle = BorderStyle.Fixed3D,
                Location = new Point(20, paramY + 130),
                Size = new Size(360, 2)
            };
            this.Controls.Add(line);

            // --- 狀態顯示區 ---
            this.lblMousePos.Location = new System.Drawing.Point(30, 330);
            this.lblMousePos.Text = "記錄位置: 尚未記錄 (請按 F2)";
            this.lblMousePos.ForeColor = Color.DarkSlateGray;
            this.lblMousePos.AutoSize = true;

            this.lblStatus.Location = new System.Drawing.Point(30, 360);
            this.lblStatus.Text = "狀態：待機中";
            this.lblStatus.Font = new Font("微軟正黑體", 10, FontStyle.Bold);
            this.lblStatus.ForeColor = Color.Blue;
            this.lblStatus.AutoSize = true;

            this.lblCountdown.Location = new System.Drawing.Point(30, 395);
            this.lblCountdown.Text = "下次循環倒數：0 秒";
            this.lblCountdown.Font = new Font("Consolas", 12, FontStyle.Bold);
            this.lblCountdown.ForeColor = Color.DarkGreen;
            this.lblCountdown.AutoSize = true;

            // --- 快捷鍵說明 ---
            this.lblHint.Location = new System.Drawing.Point(30, 440);
            this.lblHint.Text = "【快捷鍵說明】\nF2 - 記錄目前滑鼠座標 (必做)\nF3 - 啟動自動化循環 (隨機防偵測模式)\nF4 - 立即停止所有動作";
            this.lblHint.Font = new Font("微軟正黑體", 9);
            this.lblHint.ForeColor = Color.Crimson;
            this.lblHint.AutoSize = true;

            this.Controls.Add(lblMousePos);
            this.Controls.Add(lblStatus);
            this.Controls.Add(lblCountdown);
            this.Controls.Add(lblHint);
        }
    }
}