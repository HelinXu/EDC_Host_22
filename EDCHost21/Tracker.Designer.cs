namespace EDCHOST21
{
    partial class Tracker
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                capture.Dispose();
                coordCvt.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pbCamera = new System.Windows.Forms.PictureBox();
            this.btnReset = new System.Windows.Forms.Button();
            this.timer = new System.Windows.Forms.Timer(this.components);
            this.buttonStart = new System.Windows.Forms.Button();
            this.buttonPause = new System.Windows.Forms.Button();
            this.label_CarA = new System.Windows.Forms.Label();
            this.label_CarB = new System.Windows.Forms.Label();
            this.button_restart = new System.Windows.Forms.Button();
            this.button_video = new System.Windows.Forms.Button();
            this.button_set = new System.Windows.Forms.Button();
            this.labelBScore = new System.Windows.Forms.Label();
            this.labelAScore = new System.Windows.Forms.Label();
            this.label_CountDown = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.button_BFoul = new System.Windows.Forms.Button();
            this.button_AFoul = new System.Windows.Forms.Button();
            this.label_RedBG = new System.Windows.Forms.Label();
            this.label_BlueBG = new System.Windows.Forms.Label();
            this.label_GameCount = new System.Windows.Forms.Label();
            this.button_Continue = new System.Windows.Forms.Button();
            this.label_AFoulNum = new System.Windows.Forms.Label();
            this.label_BFoulNum = new System.Windows.Forms.Label();
            this.label_BMessage = new System.Windows.Forms.Label();
            this.label_AMessage = new System.Windows.Forms.Label();
            this.buttonEnd = new System.Windows.Forms.Button();
            this.label_Debug = new System.Windows.Forms.Label();
            this.timerMsg100ms = new System.Windows.Forms.Timer(this.components);
            this.timerMsg1s = new System.Windows.Forms.Timer(this.components);
            this.label_GameStage = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pbCamera)).BeginInit();
            this.SuspendLayout();
            // 
            // pbCamera
            // 
            this.pbCamera.Location = new System.Drawing.Point(568, 208);
            this.pbCamera.Name = "pbCamera";
            this.pbCamera.Size = new System.Drawing.Size(1920, 1440);
            this.pbCamera.TabIndex = 2;
            this.pbCamera.TabStop = false;
            this.pbCamera.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pbCamera_MouseClick);
            // 
            // btnReset
            // 
            this.btnReset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReset.Font = new System.Drawing.Font("微软雅黑 Light", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnReset.Location = new System.Drawing.Point(88, 1112);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(230, 86);
            this.btnReset.TabIndex = 7;
            this.btnReset.Text = "重设边界点";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // timer
            // 
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
            // 
            // buttonStart
            // 
            this.buttonStart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonStart.Font = new System.Drawing.Font("微软雅黑 Light", 16F);
            this.buttonStart.ForeColor = System.Drawing.Color.Green;
            this.buttonStart.Location = new System.Drawing.Point(2621, 1134);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(265, 98);
            this.buttonStart.TabIndex = 27;
            this.buttonStart.Text = "开始";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // buttonPause
            // 
            this.buttonPause.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonPause.Font = new System.Drawing.Font("微软雅黑 Light", 16F);
            this.buttonPause.ForeColor = System.Drawing.Color.Green;
            this.buttonPause.Location = new System.Drawing.Point(2621, 1396);
            this.buttonPause.Name = "buttonPause";
            this.buttonPause.Size = new System.Drawing.Size(265, 98);
            this.buttonPause.TabIndex = 28;
            this.buttonPause.Text = "暂停";
            this.buttonPause.UseVisualStyleBackColor = true;
            this.buttonPause.Click += new System.EventHandler(this.buttonPause_Click);
            // 
            // label_CarA
            // 
            this.label_CarA.BackColor = System.Drawing.Color.Transparent;
            this.label_CarA.Font = new System.Drawing.Font("微软雅黑", 30F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_CarA.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.label_CarA.Location = new System.Drawing.Point(310, 51);
            this.label_CarA.Name = "label_CarA";
            this.label_CarA.Size = new System.Drawing.Size(640, 104);
            this.label_CarA.TabIndex = 30;
            this.label_CarA.Text = "A车";
            this.label_CarA.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label_CarB
            // 
            this.label_CarB.BackColor = System.Drawing.Color.Transparent;
            this.label_CarB.Font = new System.Drawing.Font("微软雅黑", 30F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_CarB.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.label_CarB.Location = new System.Drawing.Point(2158, 51);
            this.label_CarB.Name = "label_CarB";
            this.label_CarB.Size = new System.Drawing.Size(640, 104);
            this.label_CarB.TabIndex = 31;
            this.label_CarB.Text = "B车";
            // 
            // button_restart
            // 
            this.button_restart.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_restart.Font = new System.Drawing.Font("微软雅黑 Light", 16F);
            this.button_restart.ForeColor = System.Drawing.Color.Green;
            this.button_restart.Location = new System.Drawing.Point(2621, 1006);
            this.button_restart.Name = "button_restart";
            this.button_restart.Size = new System.Drawing.Size(265, 98);
            this.button_restart.TabIndex = 56;
            this.button_restart.Text = "新游戏";
            this.button_restart.UseVisualStyleBackColor = true;
            this.button_restart.Click += new System.EventHandler(this.button_restart_Click);
            // 
            // button_video
            // 
            this.button_video.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_video.Font = new System.Drawing.Font("微软雅黑 Light", 10.8F);
            this.button_video.ForeColor = System.Drawing.SystemColors.ControlText;
            this.button_video.Location = new System.Drawing.Point(88, 993);
            this.button_video.Name = "button_video";
            this.button_video.Size = new System.Drawing.Size(230, 89);
            this.button_video.TabIndex = 74;
            this.button_video.Text = "开始录像";
            this.button_video.UseVisualStyleBackColor = true;
            this.button_video.Click += new System.EventHandler(this.button_video_Click);
            // 
            // button_set
            // 
            this.button_set.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_set.Font = new System.Drawing.Font("微软雅黑 Light", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_set.Location = new System.Drawing.Point(88, 1226);
            this.button_set.Name = "button_set";
            this.button_set.Size = new System.Drawing.Size(230, 85);
            this.button_set.TabIndex = 77;
            this.button_set.Text = "设置";
            this.button_set.UseVisualStyleBackColor = true;
            this.button_set.Click += new System.EventHandler(this.button_set_Click);
            // 
            // labelBScore
            // 
            this.labelBScore.BackColor = System.Drawing.Color.Transparent;
            this.labelBScore.Font = new System.Drawing.Font("微软雅黑", 48F);
            this.labelBScore.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.labelBScore.Location = new System.Drawing.Point(1616, 24);
            this.labelBScore.Name = "labelBScore";
            this.labelBScore.Size = new System.Drawing.Size(403, 162);
            this.labelBScore.TabIndex = 52;
            this.labelBScore.Text = "0";
            this.labelBScore.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelAScore
            // 
            this.labelAScore.BackColor = System.Drawing.Color.Transparent;
            this.labelAScore.Font = new System.Drawing.Font("微软雅黑", 48F);
            this.labelAScore.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
            this.labelAScore.Location = new System.Drawing.Point(1046, 24);
            this.labelAScore.Name = "labelAScore";
            this.labelAScore.Size = new System.Drawing.Size(403, 162);
            this.labelAScore.TabIndex = 51;
            this.labelAScore.Text = "0";
            this.labelAScore.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_CountDown
            // 
            this.label_CountDown.BackColor = System.Drawing.Color.Transparent;
            this.label_CountDown.Font = new System.Drawing.Font("微软雅黑", 30F);
            this.label_CountDown.ForeColor = System.Drawing.Color.DarkGreen;
            this.label_CountDown.Location = new System.Drawing.Point(2567, 484);
            this.label_CountDown.Name = "label_CountDown";
            this.label_CountDown.Size = new System.Drawing.Size(336, 128);
            this.label_CountDown.TabIndex = 78;
            this.label_CountDown.Text = "02:00";
            this.label_CountDown.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 30F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(1507, 51);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 104);
            this.label1.TabIndex = 79;
            this.label1.Text = ":";
            // 
            // button_BFoul
            // 
            this.button_BFoul.FlatAppearance.MouseDownBackColor = System.Drawing.Color.PaleTurquoise;
            this.button_BFoul.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LightCyan;
            this.button_BFoul.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_BFoul.Font = new System.Drawing.Font("微软雅黑 Light", 22.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_BFoul.ForeColor = System.Drawing.Color.DodgerBlue;
            this.button_BFoul.Location = new System.Drawing.Point(2675, 784);
            this.button_BFoul.Name = "button_BFoul";
            this.button_BFoul.Size = new System.Drawing.Size(256, 115);
            this.button_BFoul.TabIndex = 65;
            this.button_BFoul.Text = "犯规";
            this.button_BFoul.UseVisualStyleBackColor = true;
            this.button_BFoul.Click += new System.EventHandler(this.button_BFoul_Click);
            // 
            // button_AFoul
            // 
            this.button_AFoul.FlatAppearance.BorderColor = System.Drawing.Color.Red;
            this.button_AFoul.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Pink;
            this.button_AFoul.FlatAppearance.MouseOverBackColor = System.Drawing.Color.LavenderBlush;
            this.button_AFoul.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_AFoul.Font = new System.Drawing.Font("微软雅黑 Light", 22.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_AFoul.ForeColor = System.Drawing.Color.Red;
            this.button_AFoul.Location = new System.Drawing.Point(62, 701);
            this.button_AFoul.Name = "button_AFoul";
            this.button_AFoul.Size = new System.Drawing.Size(256, 115);
            this.button_AFoul.TabIndex = 86;
            this.button_AFoul.Text = "犯规";
            this.button_AFoul.UseVisualStyleBackColor = true;
            this.button_AFoul.Click += new System.EventHandler(this.button_AFoul_Click);
            // 
            // label_RedBG
            // 
            this.label_RedBG.BackColor = System.Drawing.Color.Red;
            this.label_RedBG.ForeColor = System.Drawing.SystemColors.WindowText;
            this.label_RedBG.Location = new System.Drawing.Point(0, 0);
            this.label_RedBG.Name = "label_RedBG";
            this.label_RedBG.Size = new System.Drawing.Size(1472, 192);
            this.label_RedBG.TabIndex = 88;
            // 
            // label_BlueBG
            // 
            this.label_BlueBG.BackColor = System.Drawing.Color.DodgerBlue;
            this.label_BlueBG.ForeColor = System.Drawing.SystemColors.WindowText;
            this.label_BlueBG.Location = new System.Drawing.Point(1600, 0);
            this.label_BlueBG.Name = "label_BlueBG";
            this.label_BlueBG.Size = new System.Drawing.Size(1472, 192);
            this.label_BlueBG.TabIndex = 89;
            // 
            // label_GameCount
            // 
            this.label_GameCount.BackColor = System.Drawing.Color.Transparent;
            this.label_GameCount.Font = new System.Drawing.Font("微软雅黑", 25F);
            this.label_GameCount.ForeColor = System.Drawing.Color.SeaGreen;
            this.label_GameCount.Location = new System.Drawing.Point(47, 260);
            this.label_GameCount.Name = "label_GameCount";
            this.label_GameCount.Size = new System.Drawing.Size(302, 98);
            this.label_GameCount.TabIndex = 90;
            this.label_GameCount.Text = "上半场";
            this.label_GameCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // button_Continue
            // 
            this.button_Continue.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_Continue.Font = new System.Drawing.Font("微软雅黑 Light", 16F);
            this.button_Continue.ForeColor = System.Drawing.Color.Green;
            this.button_Continue.Location = new System.Drawing.Point(2621, 1264);
            this.button_Continue.Name = "button_Continue";
            this.button_Continue.Size = new System.Drawing.Size(265, 98);
            this.button_Continue.TabIndex = 91;
            this.button_Continue.Text = "下一节";
            this.button_Continue.UseVisualStyleBackColor = true;
            this.button_Continue.Click += new System.EventHandler(this.button_Continue_Click);
            // 
            // label_AFoulNum
            // 
            this.label_AFoulNum.Font = new System.Drawing.Font("微软雅黑", 24F);
            this.label_AFoulNum.ForeColor = System.Drawing.Color.Red;
            this.label_AFoulNum.Location = new System.Drawing.Point(345, 719);
            this.label_AFoulNum.Name = "label_AFoulNum";
            this.label_AFoulNum.Size = new System.Drawing.Size(157, 74);
            this.label_AFoulNum.TabIndex = 95;
            this.label_AFoulNum.Text = "0";
            this.label_AFoulNum.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_BFoulNum
            // 
            this.label_BFoulNum.Font = new System.Drawing.Font("微软雅黑", 24F);
            this.label_BFoulNum.ForeColor = System.Drawing.Color.DodgerBlue;
            this.label_BFoulNum.Location = new System.Drawing.Point(2512, 802);
            this.label_BFoulNum.Name = "label_BFoulNum";
            this.label_BFoulNum.Size = new System.Drawing.Size(157, 74);
            this.label_BFoulNum.TabIndex = 98;
            this.label_BFoulNum.Text = "0";
            this.label_BFoulNum.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label_BMessage
            // 
            this.label_BMessage.BackColor = System.Drawing.Color.DodgerBlue;
            this.label_BMessage.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.label_BMessage.ForeColor = System.Drawing.SystemColors.Window;
            this.label_BMessage.Location = new System.Drawing.Point(2640, 0);
            this.label_BMessage.Name = "label_BMessage";
            this.label_BMessage.Size = new System.Drawing.Size(432, 192);
            this.label_BMessage.TabIndex = 99;
            this.label_BMessage.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label_AMessage
            // 
            this.label_AMessage.BackColor = System.Drawing.Color.Red;
            this.label_AMessage.Font = new System.Drawing.Font("微软雅黑", 12F);
            this.label_AMessage.ForeColor = System.Drawing.SystemColors.Window;
            this.label_AMessage.Location = new System.Drawing.Point(0, 0);
            this.label_AMessage.Name = "label_AMessage";
            this.label_AMessage.Size = new System.Drawing.Size(432, 192);
            this.label_AMessage.TabIndex = 100;
            this.label_AMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buttonEnd
            // 
            this.buttonEnd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonEnd.Font = new System.Drawing.Font("微软雅黑 Light", 16F);
            this.buttonEnd.ForeColor = System.Drawing.Color.Green;
            this.buttonEnd.Location = new System.Drawing.Point(2621, 1526);
            this.buttonEnd.Name = "buttonEnd";
            this.buttonEnd.Size = new System.Drawing.Size(265, 98);
            this.buttonEnd.TabIndex = 101;
            this.buttonEnd.Text = "结束";
            this.buttonEnd.UseVisualStyleBackColor = true;
            this.buttonEnd.Click += new System.EventHandler(this.buttonEnd_Click);
            // 
            // label_Debug
            // 
            this.label_Debug.Font = new System.Drawing.Font("微软雅黑", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_Debug.ForeColor = System.Drawing.Color.Black;
            this.label_Debug.Location = new System.Drawing.Point(112, 1352);
            this.label_Debug.Name = "label_Debug";
            this.label_Debug.Size = new System.Drawing.Size(390, 296);
            this.label_Debug.TabIndex = 102;
            // 
            // timerMsg100ms
            // 
            this.timerMsg100ms.Tick += new System.EventHandler(this.timerMsg100ms_Tick);
            // 
            // timerMsg1s
            // 
            this.timerMsg1s.Tick += new System.EventHandler(this.timerMsg1s_Tick);
            // 
            // label_GameStage
            // 
            this.label_GameStage.BackColor = System.Drawing.Color.Transparent;
            this.label_GameStage.Font = new System.Drawing.Font("微软雅黑", 25F);
            this.label_GameStage.ForeColor = System.Drawing.Color.SeaGreen;
            this.label_GameStage.Location = new System.Drawing.Point(2540, 260);
            this.label_GameStage.Name = "label_GameStage";
            this.label_GameStage.Size = new System.Drawing.Size(391, 113);
            this.label_GameStage.TabIndex = 103;
            this.label_GameStage.Text = "阶段一";
            this.label_GameStage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Tracker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(192F, 192F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(3004, 1688);
            this.Controls.Add(this.label_GameStage);
            this.Controls.Add(this.label_Debug);
            this.Controls.Add(this.buttonEnd);
            this.Controls.Add(this.label_AMessage);
            this.Controls.Add(this.label_BMessage);
            this.Controls.Add(this.label_BFoulNum);
            this.Controls.Add(this.label_AFoulNum);
            this.Controls.Add(this.button_Continue);
            this.Controls.Add(this.label_GameCount);
            this.Controls.Add(this.label_BlueBG);
            this.Controls.Add(this.label_RedBG);
            this.Controls.Add(this.button_AFoul);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label_CountDown);
            this.Controls.Add(this.button_set);
            this.Controls.Add(this.button_video);
            this.Controls.Add(this.button_BFoul);
            this.Controls.Add(this.button_restart);
            this.Controls.Add(this.labelBScore);
            this.Controls.Add(this.labelAScore);
            this.Controls.Add(this.label_CarB);
            this.Controls.Add(this.label_CarA);
            this.Controls.Add(this.buttonPause);
            this.Controls.Add(this.buttonStart);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.pbCamera);
            this.Name = "Tracker";
            this.Text = "EDC21HOST";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Tracker_FormClosed);
            this.Load += new System.EventHandler(this.Tracker_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pbCamera)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.PictureBox pbCamera;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Timer timer;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Button buttonPause;
        private System.Windows.Forms.Label label_CarA;
        private System.Windows.Forms.Label label_CarB;
        private System.Windows.Forms.Button button_restart;
        private System.Windows.Forms.Button button_video;
        private System.Windows.Forms.Button button_set;
        private System.Windows.Forms.Label labelBScore;
        private System.Windows.Forms.Label labelAScore;
        private System.Windows.Forms.Label label_CountDown;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_BFoul;
        private System.Windows.Forms.Button button_AFoul;
        private System.Windows.Forms.Label label_RedBG;
        private System.Windows.Forms.Label label_BlueBG;
        private System.Windows.Forms.Label label_GameCount;
        private System.Windows.Forms.Button button_Continue;
        private System.Windows.Forms.Label label_AFoulNum;
        private System.Windows.Forms.Label label_BFoulNum;
        private System.Windows.Forms.Label label_BMessage;
        private System.Windows.Forms.Label label_AMessage;
        private System.Windows.Forms.Button buttonEnd;
        private System.Windows.Forms.Label label_Debug;
        private System.Windows.Forms.Timer timerMsg100ms;
        private System.Windows.Forms.Timer timerMsg1s;
        private System.Windows.Forms.Label label_GameStage;
    }
}

