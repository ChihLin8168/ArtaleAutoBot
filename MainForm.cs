using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;

namespace ArtaleAutoBot
{
    public partial class MainForm : Form
    {
        // --- SendInput API 結構與常數 (維持不變) ---
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [StructLayout(LayoutKind.Sequential)]
        struct INPUT { public uint type; public InputUnion u; }

        [StructLayout(LayoutKind.Explicit)]
        struct InputUnion
        {
            [FieldOffset(0)] public KEYBDINPUT ki;
            [FieldOffset(0)] public MOUSEINPUT mi;
        }

        struct KEYBDINPUT { public ushort wVk; public ushort wScan; public uint dwFlags; public uint time; public IntPtr dwExtraInfo; }
        struct MOUSEINPUT { public int dx; public int dy; public uint mouseData; public uint dwFlags; public uint time; public IntPtr dwExtraInfo; }

        private const int INPUT_KEYBOARD = 1;
        private const int INPUT_MOUSE = 0;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        private Point recordedPos;
        private bool isRunning = false;
        private CancellationTokenSource cts;
        private Random rnd = new Random(); // 用於產生隨機延遲

        public MainForm()
        {
            InitializeComponent();
            InitializeHotKeys();

            // 預設值設定
            txtInterval.Text = "240";
            txtLeftSec.Text = "5";   // 新增：左移預設 5 秒
            txtRightSec.Text = "1.5";  // 新增：右移預設 2 秒
            txtWaitSec.Text = "3";  // 新增：技能前等待預設 10 秒
        }

        private void InitializeHotKeys()
        {
            RegisterHotKey(this.Handle, 100, 0, 0x71); // F2
            RegisterHotKey(this.Handle, 101, 0, 0x72); // F3
            RegisterHotKey(this.Handle, 102, 0, 0x73); // F4
        }

        private void SendKeyAction(ushort vk, bool isDown)
        {
            INPUT[] inputs = new INPUT[1];
            inputs[0].type = INPUT_KEYBOARD;
            inputs[0].u.ki.wVk = vk;
            inputs[0].u.ki.dwFlags = isDown ? 0 : KEYEVENTF_KEYUP;
            SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        // 修改後的點擊邏輯
        private async Task SimulateClickWithDeviation(Point basePos)
        {
            // 設定偏差範圍，例如目標點周圍 ±10 像素
            int range = 10;
            int offsetX = rnd.Next(-range, range + 1);
            int offsetY = rnd.Next(-range, range + 1);

            // 實際點擊位置 = 記錄位置 + 隨機偏差
            Point targetPos = new Point(basePos.X + offsetX, basePos.Y + offsetY);

            // 1. 移動到偏差後的座標
            Cursor.Position = targetPos;

            // 2. 移動後隨機等待 150~300ms (模擬真人移動後的反應時間)
            await RandomDelay(150, 300);

            // 3. 模擬點擊
            INPUT[] inputs = new INPUT[2];
            inputs[0].type = INPUT_MOUSE;
            inputs[0].u.mi.dwFlags = MOUSEEVENTF_LEFTDOWN;

            inputs[1].type = INPUT_MOUSE;
            inputs[1].u.mi.dwFlags = MOUSEEVENTF_LEFTUP;

            SendInput(2, inputs, Marshal.SizeOf(typeof(INPUT)));

            // 輸出到 Debug 視窗(可選)，確認偏差有在運作
            // Console.WriteLine($"點擊偏移: ({offsetX}, {offsetY})");
        }

        // 隨機延遲輔助方法 (毫秒)
        private async Task RandomDelay(int min, int max)
        {
            await Task.Delay(rnd.Next(min, max), cts.Token);
        }

        private async void StartLogic()
        {
            if (recordedPos.IsEmpty) { MessageBox.Show("請先按 F2 記錄位置！"); return; }

            isRunning = true;
            cts = new CancellationTokenSource();

            // 讀取 UI 設定參數
            int.TryParse(txtInterval.Text, out int interval);
            double.TryParse(txtLeftSec.Text, out double leftSec);
            double.TryParse(txtRightSec.Text, out double rightSec);
            int.TryParse(txtWaitSec.Text, out int waitSec);

            try
            {
                while (isRunning)
                {
                    // 1. 向左移動 (增加隨機性：設定秒數的 95% ~ 105%)
                    lblStatus.Text = "狀態：向左移動中...";
                    double randomLeft = leftSec;
                    await RunKeyHold((ushort)Keys.Left, (int)(randomLeft * 1000));

                    // 1.1 向右移動
                    lblStatus.Text = "狀態：向右移動中...";
                    double randomRight = rightSec;
                    await RunKeyHold((ushort)Keys.Right, (int)(randomRight * 1000));

                    // 2. 按上 (隨機按壓時間 80~150ms)
                    lblStatus.Text = "狀態：按上";
                    SendKeyAction((ushort)Keys.Up, true);
                    await RandomDelay(80, 150);
                    SendKeyAction((ushort)Keys.Up, false);

                    // 3. 等待 (增加隨機性：設定秒數的 ±1 秒)
                    lblStatus.Text = "狀態：等待中...";
                    int randomWaitMs = (waitSec * 1000) + rnd.Next(-1000, 1000);
                    await Task.Delay(Math.Max(100, randomWaitMs), cts.Token);

                    // 4. 施放 5 個技能
                    lblStatus.Text = "狀態：施放技能中...";
                    string[] skills = { txtS1.Text, txtS2.Text, txtS3.Text, txtS4.Text, txtS5.Text };
                    foreach (var s in skills)
                    {
                        if (isRunning && !string.IsNullOrEmpty(s))
                        {
                            try
                            {
                                ushort vk = (ushort)((Keys)Enum.Parse(typeof(Keys), s.ToUpper()));
                                SendKeyAction(vk, true);
                                await RandomDelay(100, 200); // 隨機按壓
                                SendKeyAction(vk, false);
                                await RandomDelay(2000, 2500); // 招式間隨機間隔 2~2.5s
                            }
                            catch { }
                        }
                    }

                    // 5. 回到標記位置點擊
                    lblStatus.Text = "狀態：點擊標記點";
                    Cursor.Position = recordedPos;
                    await SimulateClickWithDeviation(recordedPos);
                    await RandomDelay(200, 500); // 移動後等待隨機時間再點

                    // 6. 循環倒數
                    for (int i = interval; i > 0; i--)
                    {
                        if (!isRunning) break;
                        lblCountdown.Text = $"下次循環倒數：{i} 秒";
                        await Task.Delay(1000, cts.Token);
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { MessageBox.Show("發生錯誤: " + ex.Message); StopLogic(); }
        }

        // 優化後的持續按鍵方法
        private async Task RunKeyHold(ushort vk, int durationMs)
        {
            DateTime end = DateTime.Now.AddMilliseconds(durationMs);
            while (DateTime.Now < end && isRunning)
            {
                SendKeyAction(vk, true);
                // 每 30~70ms 送一次訊號，模擬更真實的硬體連打頻率
                await Task.Delay(rnd.Next(30, 70));
            }
            SendKeyAction(vk, false);
        }

        private void StopLogic()
        {
            isRunning = false;
            cts?.Cancel();
            lblStatus.Text = "狀態：已停止";
            lblCountdown.Text = "0";
            SendKeyAction((ushort)Keys.Left, false);
            SendKeyAction((ushort)Keys.Right, false);
            SendKeyAction((ushort)Keys.Up, false);
        }

        protected override void WndProc(ref Message m)

        {

            if (m.Msg == 0x0312)

            {

                int id = m.WParam.ToInt32();

                switch (id)

                {

                    case 100: RecordMousePosition(); break;

                    case 101: if (!isRunning) StartLogic(); break;

                    case 102: StopLogic(); break;

                }

            }

            base.WndProc(ref m);

        }
        private void RecordMousePosition()
        {

            recordedPos = Cursor.Position;

            lblMousePos.Text = $"記錄位置: {recordedPos.X}, {recordedPos.Y}";

            Console.Beep(1000, 200);

        }

        // WndProc 與 RecordMousePosition 維持不變...
    }
}