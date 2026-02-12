using System;
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
        private const int INPUT_KEYBOARD = 1;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        // --- 變數 ---
        private System.Drawing.Point recordedClickPos; // F2 座標
        private System.Drawing.Point recordedF1Pos;    // F1 局部掃描中心 (已含偏移)
        private bool isRunning = false;
        private CancellationTokenSource cts;
        private Random rnd = new Random();
        private Mat templateMat;
        private bool isTemplateReady = false;

        public MainForm()
        {
            InitializeComponent();
            InitializeHotKeys();
            BindKeyDetection();
        }

        private void InitializeHotKeys()
        {
            RegisterHotKey(this.Handle, 99, 0, 0x70);  // F1
            RegisterHotKey(this.Handle, 100, 0, 0x71); // F2
            RegisterHotKey(this.Handle, 101, 0, 0x72); // F3
            RegisterHotKey(this.Handle, 102, 0, 0x73); // F4
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

        private ushort GetVk(string name)
        {
            if (name == "Shift") return (ushort)Keys.ShiftKey;
            if (name == "Ctrl") return (ushort)Keys.ControlKey;
            if (name == "Alt") return (ushort)Keys.Menu;
            if (Enum.TryParse(typeof(Keys), name, true, out var res)) return (ushort)(Keys)res;
            return 0;
        }

        private void SendKey(ushort vk, bool down)
        {
            if (vk == 0) return;
            INPUT[] i = new INPUT[1]; i[0].type = 1; i[0].u.ki.wVk = vk;
            i[0].u.ki.dwFlags = down ? 0 : KEYEVENTF_KEYUP;
            SendInput(1, i, Marshal.SizeOf(typeof(INPUT)));
        }

        // --- 核心變動：截取範本並自動偏移 ---
        private void CaptureTemplate()
        {
            try
            {
                // 獲取目前滑鼠位置並套用偏移：左移 15px (-15)，下移 15px (+15)
                System.Drawing.Point rawPos = Cursor.Position;
                recordedF1Pos = new System.Drawing.Point(rawPos.X - 35, rawPos.Y + 35);

                Rectangle r = new Rectangle(recordedF1Pos.X - 30, recordedF1Pos.Y - 30, 60, 60);
                using (Bitmap b = new Bitmap(r.Width, r.Height))
                {
                    using (Graphics g = Graphics.FromImage(b))
                    {
                        g.CopyFromScreen(r.Location, System.Drawing.Point.Empty, r.Size);
                    }
                    picPreview.Image?.Dispose();
                    picPreview.Image = (Image)b.Clone();

                    templateMat?.Dispose();
                    templateMat = b.ToMat();
                    Cv2.CvtColor(templateMat, templateMat, ColorConversionCodes.BGR2GRAY);

                    isTemplateReady = true;
                    lblStatus.Text = $"狀態：F1 截取成功 (已偏左下5px)";
                    Console.Beep(1200, 200);
                }
            }
            catch { MessageBox.Show("截圖失敗"); }
        }

        private bool IsTargetVisible()
        {
            if (!isTemplateReady) return false;
            double threshold = double.TryParse(txtThreshold.Text, out var t) ? t : 0.85;
            int scanSize = 200;
            Rectangle area = new Rectangle(recordedF1Pos.X - scanSize / 2, recordedF1Pos.Y - scanSize / 2, scanSize, scanSize);
            area.Intersect(Screen.PrimaryScreen.Bounds);

            using (Bitmap b = new Bitmap(area.Width, area.Height))
            using (Graphics g = Graphics.FromImage(b))
            {
                g.CopyFromScreen(area.Location, System.Drawing.Point.Empty, area.Size);
                using (Mat m = b.ToMat())
                using (Mat gray = new Mat())
                using (Mat res = new Mat())
                {
                    Cv2.CvtColor(m, gray, ColorConversionCodes.BGR2GRAY);
                    Cv2.MatchTemplate(gray, templateMat, res, TemplateMatchModes.CCoeffNormed);
                    Cv2.MinMaxLoc(res, out _, out double maxV, out _, out _);
                    return maxV >= threshold;
                }
            }
        }

        private async void StartLogic()
        {
            if (!isTemplateReady || recordedClickPos.IsEmpty) { MessageBox.Show("請先按 F1 截圖與 F2 定位"); return; }

            // 啟動時自動移到偏移後的 F1 位置
            Cursor.Position = recordedF1Pos;

            isRunning = true;
            cts = new CancellationTokenSource();
            lblStatus.ForeColor = Color.Green;

            try
            {
                while (isRunning)
                {
                    // --- [新邏輯] 左右掃描尋標迴圈 ---
                    bool found = false;
                    while (isRunning && !found)
                    {
                        // 1. 向左搜尋 5 秒
                        lblStatus.Text = "狀態：向左搜尋 (5秒)...";
                        DateTime startTime = DateTime.Now;
                        while (isRunning && (DateTime.Now - startTime).TotalSeconds < 5)
                        {
                            if (IsTargetVisible()) { found = true; break; }
                            SendKey((ushort)Keys.Left, true);
                            await Task.Delay(50, cts.Token);
                        }
                        SendKey((ushort)Keys.Left, false); // 停止左移
                        if (found) break;

                        // 2. 向右修正 2 秒 (如果左搜沒找到)
                        lblStatus.Text = "狀態：未發現，向右修正 (2秒)...";
                        startTime = DateTime.Now;
                        while (isRunning && (DateTime.Now - startTime).TotalSeconds < 2)
                        {
                            if (IsTargetVisible()) { found = true; break; }
                            SendKey((ushort)Keys.Right, true);
                            await Task.Delay(50, cts.Token);
                        }
                        SendKey((ushort)Keys.Right, false); // 停止右移
                    }
                    // --- 搜尋結束 ---

                    if (!isRunning) break;

                    lblStatus.Text = "狀態：【匹配成功】按上鍵";
                    SendKey((ushort)Keys.Up, true);
                    await Task.Delay(150, cts.Token);
                    SendKey((ushort)Keys.Up, false);

                    int wait = int.TryParse(txtWaitSec.Text, out var w) ? w : 3;
                    await Task.Delay(wait * 1000, cts.Token);

                    // 執行技能
                    TextBox[] boxes = { txtS1, txtS2, txtS3, txtS4, txtS5 };
                    CheckBox[] chks = { chkS1, chkS2, chkS3, chkS4, chkS5 };
                    for (int i = 0; i < 5; i++)
                    {
                        if (isRunning && chks[i].Checked && !string.IsNullOrEmpty(boxes[i].Text))
                        {
                            SendKey(GetVk(boxes[i].Text), true);
                            await Task.Delay(100, cts.Token);
                            SendKey(GetVk(boxes[i].Text), false);
                            await Task.Delay(2200, cts.Token);
                        }
                    }

                    lblStatus.Text = "狀態：執行座標點擊";
                    Cursor.Position = recordedClickPos;
                    await Task.Delay(300, cts.Token);
                    INPUT[] click = new INPUT[2];
                    click[0].type = 0; click[0].u.mi.dwFlags = 2; // DOWN
                    click[1].type = 0; click[1].u.mi.dwFlags = 4; // UP
                    SendInput(2, click, Marshal.SizeOf(typeof(INPUT)));

                    int cool = int.TryParse(txtInterval.Text, out var c) ? c : 240;
                    for (int i = cool; i > 0 && isRunning; i--)
                    {
                        lblCountdown.Text = $"下次倒數：{i} 秒";
                        await Task.Delay(1000, cts.Token);
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { MessageBox.Show("發生錯誤: " + ex.Message); }
            finally { StopLogic(); }
        }
        private void StopLogic()
        {
            isRunning = false;
            cts?.Cancel();
            SendKey((ushort)Keys.Left, false);
            SendKey((ushort)Keys.Up, false);
            lblStatus.Text = "狀態：【已立即停止】";
            lblStatus.ForeColor = Color.Red;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0312)
            {
                int id = m.WParam.ToInt32();
                if (id == 99) CaptureTemplate();
                else if (id == 100)
                {
                    recordedClickPos = Cursor.Position;
                    lblMousePos.Text = $"座標: {recordedClickPos.X},{recordedClickPos.Y}";
                }
                else if (id == 101) { if (!isRunning) StartLogic(); }
                else if (id == 102) StopLogic();
            }
            base.WndProc(ref m);
        }
    }
}