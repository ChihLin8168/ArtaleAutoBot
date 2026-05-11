namespace ArtaleAutoBot
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox txtS1, txtS2, txtS3, txtS4, txtS5, txtInterval, txtThreshold, txtWaitSec;
        private System.Windows.Forms.CheckBox chkS1, chkS2, chkS3, chkS4, chkS5;
        private System.Windows.Forms.Label lblMousePos, lblStatus, lblCountdown, lblHint, lblPicTitle, lblMarketTitle;
        private System.Windows.Forms.PictureBox picPreview, picMarketPreview;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.txtS1 = new System.Windows.Forms.TextBox(); this.txtS2 = new System.Windows.Forms.TextBox();
            this.txtS3 = new System.Windows.Forms.TextBox(); this.txtS4 = new System.Windows.Forms.TextBox();
            this.txtS5 = new System.Windows.Forms.TextBox();
            this.chkS1 = new System.Windows.Forms.CheckBox(); this.chkS2 = new System.Windows.Forms.CheckBox();
            this.chkS3 = new System.Windows.Forms.CheckBox(); this.chkS4 = new System.Windows.Forms.CheckBox();
            this.chkS5 = new System.Windows.Forms.CheckBox();
            this.txtInterval = new System.Windows.Forms.TextBox(); this.txtThreshold = new System.Windows.Forms.TextBox();
            this.txtWaitSec = new System.Windows.Forms.TextBox();
            this.lblMousePos = new System.Windows.Forms.Label(); this.lblStatus = new System.Windows.Forms.Label();
            this.lblCountdown = new System.Windows.Forms.Label(); this.lblHint = new System.Windows.Forms.Label();
            this.lblPicTitle = new System.Windows.Forms.Label(); this.lblMarketTitle = new System.Windows.Forms.Label();
            this.picPreview = new System.Windows.Forms.PictureBox();
            this.picMarketPreview = new System.Windows.Forms.PictureBox();

            this.Text = "Artale Chihlin 自動化 [市場防錯版]";
            this.Size = new System.Drawing.Size(550, 750);
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

            // 技能設定
            TextBox[] tbs = { txtS1, txtS2, txtS3, txtS4, txtS5 };
            CheckBox[] chks = { chkS1, chkS2, chkS3, chkS4, chkS5 };
            for (int i = 0; i < 5; i++)
            {
                chks[i].Location = new System.Drawing.Point(30, 30 + (i * 35));
                chks[i].Size = new System.Drawing.Size(20, 20);
                chks[i].Checked = true;
                this.Controls.Add(chks[i]);
                tbs[i].Location = new System.Drawing.Point(60, 30 + (i * 35));
                tbs[i].Size = new System.Drawing.Size(70, 25);
                tbs[i].TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
                this.Controls.Add(tbs[i]);
                this.Controls.Add(new Label() { Text = $"技能 {i + 1} 按鍵 ", Location = new System.Drawing.Point(140, 33 + (i * 35)), AutoSize = true });
            }

            int py = 220;
            this.txtThreshold.Location = new System.Drawing.Point(30, py); this.txtThreshold.Size = new System.Drawing.Size(70, 25); this.txtThreshold.Text = "0.85";
            this.Controls.Add(this.txtThreshold);
            this.Controls.Add(new Label() { Text = "辨識門檻 (0.1-1.0)", Location = new System.Drawing.Point(110, py + 3), AutoSize = true });

            this.txtWaitSec.Location = new System.Drawing.Point(30, py + 35); this.txtWaitSec.Size = new System.Drawing.Size(70, 25); this.txtWaitSec.Text = "3";
            this.Controls.Add(this.txtWaitSec);
            this.Controls.Add(new Label() { Text = "換圖後等待秒數", Location = new System.Drawing.Point(110, py + 38), AutoSize = true });

            this.txtInterval.Location = new System.Drawing.Point(30, py + 70); this.txtInterval.Size = new System.Drawing.Size(70, 25); this.txtInterval.Text = "240";
            this.Controls.Add(this.txtInterval);
            this.Controls.Add(new Label() { Text = "大循環冷卻秒數", Location = new System.Drawing.Point(110, py + 73), AutoSize = true });

            // 圖片預覽區 (目標範本)
            this.lblPicTitle.Location = new System.Drawing.Point(330, 25);
            this.lblPicTitle.Text = "F5/F6 目標範本：";
            this.lblPicTitle.AutoSize = true;
            this.Controls.Add(lblPicTitle);

            this.picPreview.Location = new System.Drawing.Point(330, 45);
            this.picPreview.Size = new System.Drawing.Size(140, 140);
            this.picPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picPreview.BackColor = System.Drawing.Color.Black;
            this.Controls.Add(picPreview);

            // 圖片預覽區 (市場範本)
            this.lblMarketTitle.Location = new System.Drawing.Point(330, 200);
            this.lblMarketTitle.Text = "F7/F8 市場範本：";
            this.lblMarketTitle.AutoSize = true;
            this.Controls.Add(lblMarketTitle);

            this.picMarketPreview.Location = new System.Drawing.Point(330, 220);
            this.picMarketPreview.Size = new System.Drawing.Size(140, 140);
            this.picMarketPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picMarketPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picMarketPreview.BackColor = System.Drawing.Color.Black;
            this.Controls.Add(picMarketPreview);

            // 狀態區
            this.lblMousePos.Location = new System.Drawing.Point(30, 350); this.lblMousePos.AutoSize = true;
            this.Controls.Add(lblMousePos);
            this.lblStatus.Location = new System.Drawing.Point(30, 380); this.lblStatus.Text = "狀態：待機中";
            this.lblStatus.Font = new System.Drawing.Font("微軟正黑體", 10, System.Drawing.FontStyle.Bold);
            this.lblStatus.ForeColor = System.Drawing.Color.Blue;
            this.lblStatus.AutoSize = true;
            this.Controls.Add(lblStatus);
            this.lblCountdown.Location = new System.Drawing.Point(30, 415); this.lblCountdown.Font = new System.Drawing.Font("Consolas", 12, System.Drawing.FontStyle.Bold);
            this.lblCountdown.AutoSize = true;
            this.Controls.Add(lblCountdown);

            // 說明區
            this.lblHint.Location = new System.Drawing.Point(30, 460);
            this.lblHint.Size = new System.Drawing.Size(480, 220);
            this.lblHint.Text = "【操作說明】\n" +
                               "F5/F6 - 設定目標圖 (過場後偵測用)\n" +
                               "F7/F8 - 設定市場圖 (確認循環開始環境用)\n" +
                               "F2 - 記錄回市場點擊點 / F3 - 啟動 / F4 - 停止\n\n" +
                               "💡 邏輯：檢查市場圖 -> 左5右2搜尋傳送門 -> 偵測黑屏 -> \n" +
                               "   偵測目標圖 -> 向左0.5秒 -> 放技能 -> 點擊點 -> 倒數。\n" +
                               "⚠️ 倒數期間若市場圖消失，會自動執行 F2 點擊補救。";
            this.lblHint.Font = new System.Drawing.Font("微軟正黑體", 9);
            this.lblHint.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblHint.Padding = new System.Windows.Forms.Padding(5);
            this.Controls.Add(lblHint);
        }
    }
}