﻿using System;
using System.Windows.Forms;
using System.IO.Ports;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Reflection;

namespace SerialTerminal {
	public partial class Form1 : Form, IMessageFilter {
		bool hovered;
		MethodInfo wndProc;

		private System.Windows.Forms.GroupBox[] groupBox;
		private System.Windows.Forms.Button[] testRegexBtn;
		private System.Windows.Forms.TextBox[] regexBox;
		private System.Windows.Forms.CheckBox[] bold_checkBox;
		private System.Windows.Forms.CheckBox[] underline_checkBox;
		private System.Windows.Forms.Button[] colorBtn;

		int numControls = 8;
		SerialPort comPort;
		char[] splitLineArr = { '\r', '\n' };

		// highlight specific variables
		bool globalHighlightEnabled;
		bool[] groupHighlightEnabled;
		private Font defaultFont;
		private Font[] hgFonts;
		private Color[] hgColors;
		private Regex[] regex;


		// --------------------------
		public Form1() {
		// --------------------------
			InitializeComponent();
			InitializeControlArray();
			defaultFont = (Font) rxDataTextBox.Font.Clone();
			// rxDataTextBox.GotFocus += (s, e) => CopyBtn.Focus();

			// handle scrolling, when the textbox is not in focus
			Application.AddMessageFilter(this);
			rxDataTextBox.MouseEnter += (s, e) => { hovered = true; };
			rxDataTextBox.MouseLeave += (s, e) => { hovered = false; };
			wndProc = typeof(Control).GetMethod("WndProc", BindingFlags.NonPublic |	BindingFlags.Instance);
		}


		// --------------------------
		public bool PreFilterMessage(ref Message m) {
		// --------------------------
			//WM_MOUSEWHEEL = 0x20a
			if (m.Msg == 0x20a && hovered) {
				Message msg = Message.Create(rxDataTextBox.Handle, m.Msg, m.WParam, m.LParam);
				wndProc.Invoke(rxDataTextBox, new object[] { msg });
			}
			return false;
		}

		#region DynamicControls

		// --------------------------
		private void InitializeControlArray() {
		// --------------------------
			this.groupBox = new System.Windows.Forms.GroupBox[numControls];
			this.colorBtn = new System.Windows.Forms.Button[numControls];
			this.underline_checkBox = new System.Windows.Forms.CheckBox[numControls];
			this.bold_checkBox = new System.Windows.Forms.CheckBox[numControls];
			this.testRegexBtn = new System.Windows.Forms.Button[numControls];
			this.regexBox = new System.Windows.Forms.TextBox[numControls];
			groupHighlightEnabled = new bool[numControls];
			hgColors = new Color[numControls];
			hgFonts = new Font[numControls];
			regex = new Regex[numControls]; 

			for (int i = 0; i < numControls; i++) {
				this.groupBox[i] = new System.Windows.Forms.GroupBox();
				this.colorBtn[i] = new System.Windows.Forms.Button();
				this.underline_checkBox[i] = new System.Windows.Forms.CheckBox();
				this.bold_checkBox[i] = new System.Windows.Forms.CheckBox();
				this.testRegexBtn[i] = new System.Windows.Forms.Button();
				this.regexBox[i] = new System.Windows.Forms.TextBox();
				this.groupBox[i].SuspendLayout();
			}

			for (int i = 0; i < numControls; i++) {
				this.Highlight.Controls.Add(this.groupBox[i]);
				// 
				// groupBox1
				// 
				this.groupBox[i].Controls.Add(this.colorBtn[i]);
				this.groupBox[i].Controls.Add(this.underline_checkBox[i]);
				this.groupBox[i].Controls.Add(this.bold_checkBox[i]);
				this.groupBox[i].Controls.Add(this.testRegexBtn[i]);
				this.groupBox[i].Controls.Add(this.regexBox[i]);
				this.groupBox[i].Name = "groupBox";
				this.groupBox[i].Size = new System.Drawing.Size(200, 73);
				this.groupBox[i].TabIndex = 3;
				this.groupBox[i].TabStop = false;
				this.groupBox[i].Tag = i;
				this.groupBox[i].Text = "RegEx" + (i + 1);
				this.groupBox[i].MouseClick += new System.Windows.Forms.MouseEventHandler(this.groupEnableDisable);

				// 
				// colorBtn1
				// 
				this.colorBtn[i].BackColor = System.Drawing.Color.Black;
				this.colorBtn[i].Location = new System.Drawing.Point(6, 45);
				this.colorBtn[i].Name = "colorBtn";
				this.colorBtn[i].Size = new System.Drawing.Size(28, 23);
				this.colorBtn[i].TabIndex = 4;
				this.colorBtn[i].UseVisualStyleBackColor = false;
				this.colorBtn[i].Tag = i;
				this.colorBtn[i].Click += new System.EventHandler(this.ChooseColor);

				// 
				// underline_checkBox1
				// 
				this.underline_checkBox[i].AutoSize = true;
				this.underline_checkBox[i].Location = new System.Drawing.Point(101, 49);
				this.underline_checkBox[i].Name = "underline_checkBox";
				this.underline_checkBox[i].Size = new System.Drawing.Size(71, 17);
				this.underline_checkBox[i].TabIndex = 3;
				this.underline_checkBox[i].Text = "Underline";
				this.underline_checkBox[i].UseVisualStyleBackColor = true;
				this.underline_checkBox[i].CheckedChanged += new System.EventHandler(updateFonts);
				this.underline_checkBox[i].Tag = i;

				// 
				// bold_checkBox1
				// 
				this.bold_checkBox[i].AutoSize = true;
				this.bold_checkBox[i].Location = new System.Drawing.Point(48, 49);
				this.bold_checkBox[i].Name = "bold_checkBox";
				this.bold_checkBox[i].Size = new System.Drawing.Size(47, 17);
				this.bold_checkBox[i].TabIndex = 2;
				this.bold_checkBox[i].Text = "Bold";
				this.bold_checkBox[i].UseVisualStyleBackColor = true;
				this.bold_checkBox[i].CheckedChanged += new System.EventHandler(updateFonts);
				this.bold_checkBox[i].Tag = i;

				// 
				// testRegexBtn1
				// 
				this.testRegexBtn[i].Location = new System.Drawing.Point(158, 18);
				this.testRegexBtn[i].Name = "testRegexBtn";
				this.testRegexBtn[i].Size = new System.Drawing.Size(38, 23);
				this.testRegexBtn[i].TabIndex = 1;
				this.testRegexBtn[i].Text = "Test";
				this.testRegexBtn[i].UseVisualStyleBackColor = true;
				this.testRegexBtn[i].Tag = i;
				this.testRegexBtn[i].Click += new System.EventHandler(this.TestRegex);

				// 
				// regexBox1
				// 
				this.regexBox[i].Location = new System.Drawing.Point(6, 19);
				this.regexBox[i].Name = "regexBox";
				this.regexBox[i].Size = new System.Drawing.Size(150, 20);
				this.regexBox[i].TabIndex = 0;
				this.regexBox[i].Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
				this.regexBox[i].Tag = i;
				this.regexBox[i].TextChanged += new System.EventHandler(this.updateRegex);
			}

			this.groupBox[0].Location = new System.Drawing.Point(6, 70);
			this.groupBox[1].Location = new System.Drawing.Point(230, 70);
			this.groupBox[2].Location = new System.Drawing.Point(6, 155);
			this.groupBox[3].Location = new System.Drawing.Point(230, 155);
			this.groupBox[4].Location = new System.Drawing.Point(6, 240);
			this.groupBox[5].Location = new System.Drawing.Point(230, 240);
			this.groupBox[6].Location = new System.Drawing.Point(6, 325);
			this.groupBox[7].Location = new System.Drawing.Point(230, 325);
		}

		#endregion

		#region LoadAndSave
		// --------------------------
		private void Form1_Load(object sender, EventArgs e) {
		// --------------------------
			this.Size = Properties.Settings.Default.FormSize;
			// load the previously used COM port and baud rate setting
			BaudratesSel.Text = Properties.Settings.Default.BaudRate;
			lineEndingBox.Text = Properties.Settings.Default.LineEnding;

			StringCollection pCol = (StringCollection) Properties.Settings.Default.ComPorts;
			if (pCol != null) {
				String[] ports = new String[pCol.Count];
				pCol.CopyTo(ports, 0);
				SerialPortsSel.Items.AddRange(ports);
				// select the previously used port
				SerialPortsSel.Text = Properties.Settings.Default.ComPort;
			}

			// load individual group boxes
			for (int i = 0; i < numControls; i++) {
				StringCollection myCol = (StringCollection) Properties.Settings.Default["group" + (i + 1) + "Highlight"];
				if (myCol != null) {
					String[] myArr = new String[myCol.Count];
					myCol.CopyTo(myArr, 0);
					groupHighlightEnabled[i] = Convert.ToBoolean(myArr[0]);
					regexBox[i].Text = myArr[1];
					colorBtn[i].BackColor = ColorTranslator.FromHtml(myArr[2]);
					underline_checkBox[i].Checked = Convert.ToBoolean(myArr[3]);
					bold_checkBox[i].Checked = Convert.ToBoolean(myArr[4]);
				}

				hgColors[i] = colorBtn[i].BackColor;
				createFont(i);
				createRegex(i);
				changeGroupHighlight(i);
			}

			// set global setting
			globalHighlightEnabled = Properties.Settings.Default.GlobalHighlight;
			changeHighlight(globalHighlightEnabled);
			
			log("Serial Port Terminal ready");
		}


		// --------------------------
		private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
		// --------------------------
			// save all the properties here
			Properties.Settings.Default.FormSize = this.Size;
			Properties.Settings.Default.Location = this.Location;
			Properties.Settings.Default.GlobalHighlight = globalHighlightEnabled;

			// save the COM port and baud rate settings
			Properties.Settings.Default.BaudRate = BaudratesSel.Text;
			Properties.Settings.Default.LineEnding = lineEndingBox.Text;
			Properties.Settings.Default.ComPort = SerialPortsSel.Text;

			StringCollection pCol = new StringCollection();
			for (int i = 0; i < SerialPortsSel.Items.Count; i++)
				pCol.Add(SerialPortsSel.GetItemText(SerialPortsSel.Items[i])); 
			Properties.Settings.Default.ComPorts = pCol;

			// save individual filter settings as well
			for (int i = 0; i < numControls; i++) {
				StringCollection myCol = new StringCollection();
				myCol.Add(groupHighlightEnabled[i].ToString());
				myCol.Add(regexBox[i].Text);
				myCol.Add(ColorTranslator.ToHtml(colorBtn[i].BackColor));
				myCol.Add(underline_checkBox[i].Checked.ToString());
				myCol.Add(bold_checkBox[i].Checked.ToString());
				Properties.Settings.Default["group" + (i + 1) + "Highlight"] = myCol;
			}
			
			Properties.Settings.Default.Save();
		}

		#endregion

		#region MainControls

		// --------------------------
		private void SendBtn_Click(object sender, EventArgs e) {
		// --------------------------
			txData();
		}


		// --------------------------
		private void txDataTextBox_KeyDown(object sender, KeyEventArgs e) {
			// --------------------------
			if (e.KeyCode == Keys.Enter)
				txData();
		}


		// --------------------------
		private void OpenBtn_Click(object sender, EventArgs e) {
		// --------------------------
			string port = SerialPortsSel.Text;
			string baudRate = BaudratesSel.Text;

			// verify the correct port/baud rate
			if (port == "") {
				error("Invalid COM port");
				return;
			}

			int intBaud;
			bool isNumeric = int.TryParse(baudRate, out intBaud);

			if(!isNumeric) {
				error("Invalid baud rate");
				return;
			}

			try {
				// set and try to open the COM port
				log("Opening port...");
				comPort = new SerialPort(port, intBaud);
				comPort.RtsEnable = true;
				comPort.DtrEnable = true;
				comPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
				comPort.ReadTimeout = 500;
				comPort.Open();
				log(port + " open");
				
				// set button states
				OpenBtn.Enabled = false;
				CloseBtn.Enabled = true;
				RefreshBtn.Enabled = false;
				SerialPortsSel.Enabled = false;
				BaudratesSel.Enabled = false;
				SendBtn.Enabled = true;
				txDataTextBox.Enabled = true;

				// start a port monitoring thread
				(new System.Threading.Thread(() => {
					int bufLevel = 0;
					int maxBuffer = 50000;
					try {
						while (comPort.IsOpen) {
							bufLevel = comPort.BytesToRead;
							System.Threading.Thread.Sleep(500);
							this.Invoke(new Action<int>(setBufferLevel), new object[] { (bufLevel * 100 / maxBuffer) });
							if (globalHighlightEnabled && (bufLevel > maxBuffer)) {
								globalHighlightEnabled = false;
								this.Invoke(new Action<string>(error), new object[] { "Too much data backlog, disabling highlighting" });
								this.Invoke(new Action<bool>(changeHighlight), new object[] { globalHighlightEnabled });
							}
						}
						// port closed
						this.Invoke(new Action(() => { CloseBtn.PerformClick(); }));
					}
					catch {
						try { comPort.Close(); } catch { }
					}

				})).Start();

			}
			catch(Exception excp) {
				error("Unable to open Serial: " + excp.Message);
				try { comPort.Close(); } catch { }
			}
		}
		

		// --------------------------
		private void RefreshBtn_Click(object sender, EventArgs e) {
		// --------------------------
			SerialPortsSel.Items.Clear();
			// populate the Combobox with SerialPorts on the System
			SerialPortsSel.Items.AddRange(SerialPort.GetPortNames());
			// select the previously used port
			SerialPortsSel.Text = Properties.Settings.Default.ComPort;

			String sPortsMsg = "Found ports: ";
			foreach (var item in SerialPort.GetPortNames()) {
				sPortsMsg += item.ToString() + " ";
			}

			log(sPortsMsg);
		}

		
		// --------------------------
		private void ClearBtn_Click(object sender, EventArgs e) {
		// --------------------------
			rxDataTextBox.Text = "";
		}

		
		// --------------------------
		private void CloseBtn_Click(object sender, EventArgs e) {
		// --------------------------
			try {
				log("Closing COM port...");
				CloseBtn.Enabled = false;
				OpenBtn.Enabled = true;
				RefreshBtn.Enabled = true;
				SerialPortsSel.Enabled = true;
				BaudratesSel.Enabled = true;
				SendBtn.Enabled = false;
				txDataTextBox.Enabled = false;

				(new System.Threading.Thread(() => {
					comPort.Close();
					this.Invoke(new Action<string>(log), new object[] { "Port closed" });
					System.Threading.Thread.Sleep(600);
					this.Invoke(new Action<int>(setBufferLevel), new object[] { 0 });
				})).Start();

			}
			catch {
			}
		}


		// --------------------------
		private void CopyBtn_Click(object sender, EventArgs e) {
		// --------------------------
			System.Windows.Forms.Clipboard.SetText(rxDataTextBox.Text + " ");
			log("Data copied to clipboard");
		}

		
		// --------------------------
		private void txData() {
		// --------------------------
			string Data = txDataTextBox.Text;

			try {
				switch (lineEndingBox.Text) {
					case "None":
						comPort.WriteLine(Data);
						break;
					case "CR":
						comPort.WriteLine(Data + "\r");
						break;
					case "LF":
						comPort.WriteLine(Data + "\n");
						break;
					case "CR/LF":
						comPort.WriteLine(Data + "\r\n");
						break;
				}

				inject("SENT: " + Data);
			}
			catch (Exception excp) {
				error("Unable to send data: " + excp.Message);
				try { comPort.Close(); }
				catch { }
			}
		}

		#endregion

		#region HighlighterControls

		// --------------------------
		private void ChooseColor(object sender, EventArgs e) {
		// --------------------------
			int index = (int)((Control)sender).Tag;
			colorDialog.Color = colorBtn[index].BackColor;

			if (colorDialog.ShowDialog() == DialogResult.OK) {
				colorBtn[index].BackColor = colorDialog.Color;
				hgColors[index] = colorDialog.Color;
			}
		}
		

		// --------------------------
		private void TestRegex(object sender, EventArgs e) {
		// --------------------------
			int index = (int)((Control)sender).Tag;
			string text = sampleInputBox.Text;
			highlightedOutputBox.Text = "";
			highlightedOutputBox.BackColor = default(Color);

			try {
				// matching one group only
				Match m = regex[index].Match(text);
				int curIdx = 0;

				while (m.Success) {
					Group g = m.Groups[1];
					highlightedOutputBox.AppendText(text.Substring(curIdx, g.Index - curIdx));
					highlightedOutputBox.SelectionColor = hgColors[index];
					highlightedOutputBox.SelectionFont = hgFonts[index];
					highlightedOutputBox.AppendText(g.Value);
					highlightedOutputBox.SelectionColor = default(Color);
					highlightedOutputBox.SelectionFont = defaultFont;
					curIdx = g.Index + g.Length;
					m = m.NextMatch();
				}

				highlightedOutputBox.AppendText(text.Substring(curIdx, text.Length - curIdx));
			}
			catch {
				highlightedOutputBox.BackColor = Color.LightPink;
			}
		}


		#region EnableAndDisable

		// --------------------------
		private void updateFonts(object sender, EventArgs e) {
		// --------------------------
			int index = (int)((Control)sender).Tag;
			createFont(index);
		}

		// --------------------------
		private void createFont(int index) {
		// --------------------------
			FontStyle style = FontStyle.Regular;
			if (underline_checkBox[index].Checked)
				style |= FontStyle.Underline;
			if (bold_checkBox[index].Checked)
				style |= FontStyle.Bold;

			hgFonts[index] = new Font(defaultFont, style);
		}
		

		// --------------------------
		private void updateRegex(object sender, EventArgs e) {
		// --------------------------
			int index = (int)((Control)sender).Tag;
			createRegex(index);
		}

		
		// --------------------------
		private void createRegex(int index) {
		// --------------------------
			try {
				regex[index] = new Regex(regexBox[index].Text);
				regexBox[index].BackColor = default(Color);
			}
			catch {
				regexBox[index].BackColor = Color.LightPink;
			}
		}
		

		// --------------------------
		private void groupEnableDisable(object sender, MouseEventArgs e) {
		// --------------------------
			if (e.Button == MouseButtons.Right) {
				int index = (int)((Control)sender).Tag;
				groupHighlightEnabled[index] = !groupHighlightEnabled[index];
				changeGroupHighlight(index);
			}
		}

		// --------------------------
		private void changeGroupHighlight(int index) {
		// --------------------------
			if (groupHighlightEnabled[index]) {
				groupBox[index].Text = "RegEx" + (index + 1);
				groupBox[index].BackColor = default(Color);
				foreach (Control ctl in groupBox[index].Controls)
					ctl.Enabled = true;
			}
			else {
				groupBox[index].Text = "Disabled";
				groupBox[index].BackColor = System.Drawing.Color.Gray;
				foreach (Control ctl in groupBox[index].Controls)
					ctl.Enabled = false;
			}
		}


		// --------------------------
		private void highlightEnableDisable(object sender, MouseEventArgs e) {
			// --------------------------
			if (e.Button == MouseButtons.Right) {
				globalHighlightEnabled = !globalHighlightEnabled;
				changeHighlight(globalHighlightEnabled);
			}
		}


		// --------------------------
		private void changeHighlight(bool enable) {
			// --------------------------
			if (enable) {
				Highlight.Text = "Highlight";
				Highlight.Enabled = true;
				Highlight.BackColor = System.Drawing.Color.LightGray;
			}
			else {
				Highlight.Text = "Disabled";
				Highlight.Enabled = false;
				this.Highlight.BackColor = System.Drawing.Color.Gray;
			}
		}

		#endregion

		#endregion

		#region Logging

		// --------------------------
		private void inject(string message) {
		// --------------------------
			rxDataTextBox.SelectionBackColor = Color.DarkSlateGray;
			rxDataTextBox.SelectionColor = Color.White;
			
			if (autoscroll.Checked) {
				rxDataTextBox.AppendText(message + "\r\n");
				rxDataTextBox.SelectionStart = rxDataTextBox.Text.Length;
				rxDataTextBox.ScrollToCaret();
			}
			else {
				rxDataTextBox.AppendText(message + "\r\n");
			}
		}


		// --------------------------
		private void log(string message) {
		// --------------------------
			logLabel.ForeColor = default(Color);
			logLabel.Text = message;
		}


		// --------------------------
		private void error(string message) {
		// --------------------------
			logLabel.ForeColor = Color.Red;
			logLabel.Text = message;
			logLabel.ToolTipText += "ERROR: " + message + "\r\n";
			System.Media.SystemSounds.Beep.Play();
		}


		// --------------------------
		private void setBufferLevel(int percent) {
		// --------------------------
			if (percent > 100)
				percent = 100;
			rxBufferBar.Value = percent;
		}

		#endregion

		#region SerialPortData

		// --------------------------
		private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e) {
		// --------------------------
			this.Invoke(new Action(ProcessPrintSerialData), new object[] { });
		}


		// --------------------------
		public void ProcessPrintSerialData() {
		// --------------------------
			try {
				try {
					int numBytesInBuf = comPort.BytesToRead;
					while ((numBytesInBuf > 0) && comPort.IsOpen) {
						String strLine = comPort.ReadLine();
						numBytesInBuf -= strLine.Length;
						// apply the highlight filters if asked for
						if (globalHighlightEnabled)
							applyFilters(strLine);
						else
							rxDataTextBox.AppendText(strLine);
					}
				}
				catch (TimeoutException) {
					if (comPort.IsOpen && (comPort.BytesToRead > 0))
						rxDataTextBox.AppendText(comPort.ReadExisting());
					return;
				}
				catch (Exception ex) {
					error("Error applying Regex highlighting");
					System.Console.WriteLine(ex.ToString());
				}


				if (autoscroll.Checked) {
					rxDataTextBox.HideSelection = false;
					rxDataTextBox.SelectionStart = rxDataTextBox.Text.Length;
					rxDataTextBox.ScrollToCaret();
				}
			}
			catch { }
		}



		// --------------------------
		private void applyFilters(string text) {
		// --------------------------
			for (int index = 0; index < numControls; index++) {
				if (groupHighlightEnabled[index]) {
					Match m = regex[index].Match(text);

					if (m.Success) {
						int curIdx = 0;
						while (m.Success) {
							// matching one group only
							Group g = m.Groups[1];
							rxDataTextBox.AppendText(text.Substring(curIdx, g.Index - curIdx));
							rxDataTextBox.SelectionColor = hgColors[index];
							rxDataTextBox.SelectionFont = hgFonts[index];
							rxDataTextBox.AppendText(g.Value);
							rxDataTextBox.SelectionColor = default(Color);
							rxDataTextBox.SelectionFont = defaultFont;
							curIdx = g.Index + g.Length;
							m = m.NextMatch();
						}

						rxDataTextBox.AppendText(text.Substring(curIdx, text.Length - curIdx));
						return;
					}
				}
			}

			// no match occoured, just output the line
			rxDataTextBox.AppendText(text);
		}

		#endregion


	}
}

