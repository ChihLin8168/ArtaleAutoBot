using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace ArtaleAutoBot
{
    public partial class MainForm : Form
    {
        // --- Windows API ---
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
        [StructLayout(LayoutKind.Sequential)] struct INPUT { public uint type; public InputUnion u; }
        [StructLayout(LayoutKind.Explicit)] struct InputUnion { [FieldOffset(0)] public KEYBDINPUT ki; [FieldOffset(0)] public MOUSEINPUT mi; }
        struct KEYBDINPUT { public ushort wVk; public ushort wScan; public uint dwFlags; public uint time; public IntPtr dwExtraInfo; }
        struct MOUSEINPUT { public int dx; public int dy; public uint mouseData; public uint dwFlags; public uint time; public IntPtr dwExtraInfo; }
        private const uint KEYEVENTF_KEYUP = 0x0002;
        [DllImport("user32.dll")] private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        // --- 變數 ---
        private System.Drawing.Point recordedClickPos;
        private System.Drawing.Point cropStart, cropEnd, marketStart, marketEnd;
        private bool isRunning = false;
        private CancellationTokenSource cts;
        private Mat templateMat, marketTemplate;
        private bool isTemplateReady = false, isMarketReady = false;
        private Random rnd = new Random();

        // 核心：追蹤目前被程式按下的鍵，防止卡鍵
        private HashSet<ushort> _pressedKeys = new HashSet<ushort>();

        public MainForm()
        {
            InitializeComponent();
            InitializeHotKeys();
            BindKeyDetection();
        }

        private void InitializeHotKeys()
        {
            RegisterHotKey(this.Handle, 100, 0, 0x71); // F2
            RegisterHotKey(this.Handle, 101, 0, 0x72); // F3
            RegisterHotKey(this.Handle, 102, 0, 0x73); // F4
            RegisterHotKey(this.Handle, 105, 0, 0x74); // F5
            RegisterHotKey(this.Handle, 106, 0, 0x75); // F6
            RegisterHotKey(this.Handle, 107, 0, 0x76); // F7
            RegisterHotKey(this.Handle, 108, 0, 0x77); // F8
        }

        private void BindKeyDetection()
        {
            TextBox[] boxes = { txtS1, txtS2, txtS3, txtS4, txtS5 };
            foreach (var b in boxes)
            {
                b.ReadOnly = true; b.BackColor = Color.White;
                b.KeyDown += (s, e) => {
                    string k = e.KeyCode.ToString();
                    if (e.KeyCode == Keys.ShiftKey || e.KeyCode == Keys.LShiftKey || e.KeyCode == Keys.RShiftKey) k = "Shift";
                    else if (e.KeyCode == Keys.ControlKey) k = "Ctrl";
                    else if (e.KeyCode == Keys.Menu) k = "Alt";
                    ((TextBox)s).Text = k; e.SuppressKeyPress = true;
                };
            }
        }

        // --- 安全按鍵傳送機制 ---

        private void SendKey(ushort vk, bool down)
        {
            if (vk == 0) return;
            INPUT[] i = new INPUT[1];
            i[0].type = 1; // Keyboard
            i[0].u.ki.wVk = vk;
            i[0].u.ki.dwFlags = down ? 0 : KEYEVENTF_KEYUP;

            lock (_pressedKeys)
            {
                if (down) _pressedKeys.Add(vk);
                else _pressedKeys.Remove(vk);
            }

            SendInput(1, i, Marshal.SizeOf(typeof(INPUT)));
        }

        /// <summary>
        /// 異步點擊按鍵，並確保一定會執行 KeyUp
        /// </summary>
        private async Task PressKeyAsync(ushort vk, int holdDuration, CancellationToken token)
        {
            if (vk == 0) return;
            try
            {
                SendKey(vk, true);
                await Task.Delay(holdDuration, token);
            }
            finally
            {
                SendKey(vk, false);
            }
        }

        /// <summary>
        /// 強制釋放所有追蹤中的按鍵與常用控制鍵
        /// </summary>
        private void ForceReleaseAll()
        {
            List<ushort> keys;
            lock (_pressedKeys)
            {
                keys = new List<ushort>(_pressedKeys);
            }

            foreach (var vk in keys) SendKey(vk, false);

            // 二次保險：強制彈起常用組合鍵，解決「手按失效」問題
            ushort[] safetyKeys = { (ushort)Keys.ShiftKey, (ushort)Keys.ControlKey, (ushort)Keys.Menu };
            foreach (var vk in safetyKeys)
            {
                INPUT[] i = new INPUT[1];
                i[0].type = 1; i[0].u.ki.wVk = vk; i[0].u.ki.dwFlags = KEYEVENTF_KEYUP;
                SendInput(1, i, Marshal.SizeOf(typeof(INPUT)));
            }
        }

        private ushort GetVk(string name)
        {
            if (name == "Shift") return (ushort)Keys.ShiftKey;
            if (name == "Ctrl") return (ushort)Keys.ControlKey;
            if (name == "Alt") return (ushort)Keys.Menu;
            if (Enum.TryParse(typeof(Keys), name, true, out var res)) return (ushort)(Keys)res;
            return 0;
        }

        // --- 圖像處理 ---

        private bool IsImageVisible(Mat target)
        {
            if (target == null) return false;
            double threshold = double.TryParse(txtThreshold.Text, out var t) ? t : 0.85;
            using (Bitmap b = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(b)) g.CopyFromScreen(0, 0, 0, 0, b.Size);
                using (Mat screen = b.ToMat())
                using (Mat gray = new Mat(), res = new Mat())
                {
                    Cv2.CvtColor(screen, gray, ColorConversionCodes.BGR2GRAY);
                    Cv2.MatchTemplate(gray, target, res, TemplateMatchModes.CCoeffNormed);
                    Cv2.MinMaxLoc(res, out _, out double maxV, out _, out _);
                    return maxV >= threshold;
                }
            }
        }

        private bool IsScreenBlack()
        {
            int sz = 100;
            Rectangle area = new Rectangle(Screen.PrimaryScreen.Bounds.Width / 2 - sz / 2, Screen.PrimaryScreen.Bounds.Height / 2 - sz / 2, sz, sz);
            using (Bitmap b = new Bitmap(sz, sz))
            using (Graphics g = Graphics.FromImage(b))
            {
                g.CopyFromScreen(area.Location, System.Drawing.Point.Empty, area.Size);
                using (Mat m = b.ToMat()) { Scalar mean = Cv2.Mean(m); return (mean.Val0 < 15 && mean.Val1 < 15 && mean.Val2 < 15); }
            }
        }

        private async Task DoRecordedClick()
        {
            Cursor.Position = recordedClickPos;
            await Task.Delay(500);
            INPUT[] i = new INPUT[2];
            i[0].type = 0; i[0].u.mi.dwFlags = 2; // Down
            i[1].type = 0; i[1].u.mi.dwFlags = 4; // Up
            SendInput(1, new[] { i[0] }, Marshal.SizeOf(typeof(INPUT)));
            await Task.Delay(rnd.Next(60, 150));
            SendInput(1, new[] { i[1] }, Marshal.SizeOf(typeof(INPUT)));
        }

        // --- 移動與戰鬥邏輯 ---

        private async Task<bool> MoveWithSpamUp(ushort moveKey, int seconds, CancellationToken token)
        {
            DateTime start = DateTime.Now;
            try
            {
                SendKey(moveKey, true); // 開始按住移動 (左或右)

                while ((DateTime.Now - start).TotalSeconds < seconds && !token.IsCancellationRequested)
                {
                    // 1. 持續點擊 Up 鍵 (進傳送門)
                    await PressKeyAsync((ushort)Keys.Up, rnd.Next(20, 40), token);

                    // 2. 偵測黑頻 (代表正在過地圖)
                    if (IsScreenBlack())
                    {
                        lblStatus.Text = "狀態：偵測到黑頻，過圖中...";
                        SendKey(moveKey, false); // 過圖時先放開移動鍵，避免過圖後亂跑

                        // 等待黑頻結束
                        while (IsScreenBlack() && !token.IsCancellationRequested)
                            await Task.Delay(100, token);

                        // 3. 黑頻結束後，進入目標偵測模式
                        lblStatus.Text = "狀態：抵達新地圖，搜尋目標區域...";

                        // 給地圖一點載入時間（避免畫面未完全出現導致偵測失敗）
                        await Task.Delay(500, token);

                        // 持續偵測直到看見 F5/F6 設定的目標範本
                        while (!token.IsCancellationRequested)
                        {
                            if (IsImageVisible(templateMat)) // 如果看見目標了
                            {
                                lblStatus.Text = "狀態：發現目標區域！準備施放技能";
 

                                return true; // 返回 true，這會讓 StartLogic 進入施放技能的區段
                            }
                            await Task.Delay(200, token); // 每 0.2 秒掃描一次畫面
                        }
                    }

                    await Task.Delay(rnd.Next(20, 40), token);
                }
            }
            finally
            {
                // 確保任何情況退出此函數時，移動鍵都是放開的
                SendKey(moveKey, false);
            }
            return false; // 如果時間到了都沒進傳送門，就回傳 false 換方向
        }

        private async void StartLogic()
        {
            if (!isTemplateReady || recordedClickPos.IsEmpty) { MessageBox.Show("請完成 F2、F5/F6 與 F7/F8 設定"); return; }
            isRunning = true;
            cts = new CancellationTokenSource();
            lblStatus.ForeColor = Color.Green;

            try
            {
                while (isRunning)
                {
                    lblStatus.Text = "狀態：檢查環境...";
                    if (isMarketReady && !IsImageVisible(marketTemplate))
                    {
                        await DoRecordedClick();
                        await Task.Delay(3000, cts.Token);
                        continue;
                    }

                    bool found = await MoveWithSpamUp((ushort)Keys.Left, 5, cts.Token);
                    if (isRunning && !found) found = await MoveWithSpamUp((ushort)Keys.Right, 2, cts.Token);

                    if (isRunning && found)
                    {
                        int wait = int.TryParse(txtWaitSec.Text, out var w) ? w : 3;
                        await Task.Delay(wait * 1000, cts.Token);

                        TextBox[] boxes = { txtS1, txtS2, txtS3, txtS4, txtS5 };
                        CheckBox[] chks = { chkS1, chkS2, chkS3, chkS4, chkS5 };

                        for (int i = 0; i < 5; i++)
                        {
                            if (isRunning && chks[i].Checked && !string.IsNullOrEmpty(boxes[i].Text))
                            {
                                ushort vk = GetVk(boxes[i].Text);
                                await PressKeyAsync(vk, 120, cts.Token);
                                await Task.Delay(2200, cts.Token);
                            }
                        }

                        await DoRecordedClick();

                        int cool = int.TryParse(txtInterval.Text, out var c) ? c : 240;
                        for (int i = cool; i > 0 && isRunning; i--)
                        {
                            lblCountdown.Text = $"下次循環倒數：{i} 秒";
                            if (i % 10 == 0 && isMarketReady && !IsImageVisible(marketTemplate)) await DoRecordedClick();
                            await Task.Delay(1000, cts.Token);
                        }
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception) { }
            finally { StopLogic(); }
        }

        private void StopLogic()
        {
            isRunning = false;
            cts?.Cancel();
            ForceReleaseAll(); // 關鍵：停止時清理所有按鍵
            lblStatus.Text = "狀態：已停止";
            lblStatus.ForeColor = Color.Red;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0312)
            {
                int id = m.WParam.ToInt32();
                if (id == 100) { recordedClickPos = Cursor.Position; lblMousePos.Text = $"回城點擊位置: {recordedClickPos.X},{recordedClickPos.Y}"; }
                else if (id == 105) cropStart = Cursor.Position;
                else if (id == 106) { cropEnd = Cursor.Position; CaptureArea(ref templateMat, cropStart, cropEnd, picPreview, "目標範本"); }
                else if (id == 107) marketStart = Cursor.Position;
                else if (id == 108) { marketEnd = Cursor.Position; CaptureArea(ref marketTemplate, marketStart, marketEnd, picMarketPreview, "市場範本"); isMarketReady = true; }
                else if (id == 101 && !isRunning) StartLogic();
                else if (id == 102) StopLogic();
            }
            base.WndProc(ref m);
        }

        private void CaptureArea(ref Mat target, System.Drawing.Point s, System.Drawing.Point e, PictureBox p, string name)
        {
            int x = Math.Min(s.X, e.X), y = Math.Min(s.Y, e.Y), w = Math.Abs(s.X - e.X), h = Math.Abs(s.Y - e.Y);
            if (w < 5 || h < 5) return;
            Rectangle r = new Rectangle(x, y, w, h);
            using (Bitmap b = new Bitmap(w, h))
            {
                using (Graphics g = Graphics.FromImage(b)) g.CopyFromScreen(r.Location, System.Drawing.Point.Empty, r.Size);
                p.Image?.Dispose(); p.Image = (Image)b.Clone();
                target?.Dispose(); target = b.ToMat(); Cv2.CvtColor(target, target, ColorConversionCodes.BGR2GRAY);
                lblStatus.Text = $"狀態：{name} 截取成功"; Console.Beep(1000, 200);
                if (name == "目標範本") isTemplateReady = true;
            }
        }
    }
}