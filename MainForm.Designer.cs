namespace ArtaleAutoBot
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox txtS1, txtS2, txtS3, txtS4, txtS5, txtInterval, txtThreshold, txtWaitSec;
        private System.Windows.Forms.CheckBox chkS1, chkS2, chkS3, chkS4, chkS5;
        private System.Windows.Forms.Label lblMousePos, lblStatus, lblCountdown, lblHint, lblPicTitle;
        private System.Windows.Forms.PictureBox picPreview;

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
            this.lblPicTitle = new System.Windows.Forms.Label();
            this.picPreview = new System.Windows.Forms.PictureBox();

            // --- 視窗本體設定 ---
            this.Text = "Artale Chihlin 自動感應版";
            this.Size = new System.Drawing.Size(550, 680);
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;

            // --- 1. 技能設定區 (含 CheckBox, TextBox, Label) ---
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

                Label lblSkill = new Label()
                {
                    Text = $"技能 {i + 1} 按鍵 ",
                    Location = new System.Drawing.Point(140, 33 + (i * 35)),
                    AutoSize = true
                };
                this.Controls.Add(lblSkill);
            }

            // --- 2. 參數設定區 ---
            int py = 220;
            this.txtThreshold.Location = new System.Drawing.Point(30, py); this.txtThreshold.Size = new System.Drawing.Size(70, 25); this.txtThreshold.Text = "0.85";
            Label lblT = new Label() { Text = "辨識門檻 (建議 0.8-0.9)", Location = new System.Drawing.Point(110, py + 3), AutoSize = true };
            this.Controls.Add(this.txtThreshold); this.Controls.Add(lblT);

            this.txtWaitSec.Location = new System.Drawing.Point(30, py + 35); this.txtWaitSec.Size = new System.Drawing.Size(70, 25); this.txtWaitSec.Text = "3";
            Label lblW = new Label() { Text = "抵達後等待秒數", Location = new System.Drawing.Point(110, py + 38), AutoSize = true };
            this.Controls.Add(this.txtWaitSec); this.Controls.Add(lblW);

            this.txtInterval.Location = new System.Drawing.Point(30, py + 70); this.txtInterval.Size = new System.Drawing.Size(70, 25); this.txtInterval.Text = "240";
            Label lblI = new Label() { Text = "總循環冷卻秒數", Location = new System.Drawing.Point(110, py + 73), AutoSize = true };
            this.Controls.Add(this.txtInterval); this.Controls.Add(lblI);

            // --- 3. 圖片預覽區 ---
            this.lblPicTitle.Location = new System.Drawing.Point(330, 25);
            this.lblPicTitle.Text = "F1 截圖預覽：";
            this.lblPicTitle.AutoSize = true;
            this.Controls.Add(lblPicTitle);

            this.picPreview.Location = new System.Drawing.Point(330, 50);
            this.picPreview.Size = new System.Drawing.Size(160, 160);
            this.picPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picPreview.BackColor = System.Drawing.Color.Black;
            this.Controls.Add(picPreview);

            // --- 4. 狀態與說明區 (這部分是您之前說沒出來的地方) ---
            this.lblMousePos.Location = new System.Drawing.Point(30, 350);
            this.lblMousePos.Text = "記錄位置: 尚未記錄 (請按 F2)";
            this.lblMousePos.AutoSize = true;
            this.Controls.Add(lblMousePos);

            this.lblStatus.Location = new System.Drawing.Point(30, 380);
            this.lblStatus.Text = "狀態：待機中";
            this.lblStatus.Font = new System.Drawing.Font("微軟正黑體", 10, System.Drawing.FontStyle.Bold);
            this.lblStatus.ForeColor = System.Drawing.Color.Blue;
            this.lblStatus.AutoSize = true;
            this.Controls.Add(lblStatus);

            this.lblCountdown.Location = new System.Drawing.Point(30, 415);
            this.lblCountdown.Text = "下次循環倒數：0 秒";
            this.lblCountdown.Font = new System.Drawing.Font("Consolas", 12, System.Drawing.FontStyle.Bold);
            this.lblCountdown.ForeColor = System.Drawing.Color.DarkGreen;
            this.lblCountdown.AutoSize = true;
            this.Controls.Add(lblCountdown);

            this.lblHint.Location = new System.Drawing.Point(30, 460);
            this.lblHint.Size = new System.Drawing.Size(480, 150);
            this.lblHint.Text = "【操作說明】\n" +
                               "F1 - 將滑鼠移到定點(如傳送門)按壓截圖\n" +
                               "F2 - 記錄目前滑鼠座標 (點擊標記點用)\n" +
                               "F3 - 啟動自動化 / F4 - 停止動作\n\n" +
                               "💡 點擊上方技能框後，直接按下鍵盤鍵 (如 Shift) 即可設定。";
            this.lblHint.Font = new System.Drawing.Font("微軟正黑體", 9);
            this.lblHint.ForeColor = System.Drawing.Color.Crimson;
            this.lblHint.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblHint.Padding = new System.Windows.Forms.Padding(5);
            this.Controls.Add(lblHint);
        }
    }
}