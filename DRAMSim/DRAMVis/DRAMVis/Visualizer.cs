// Visualizer.cs created with Monoevelop
// User: elliott at 9:26 PM 1/19/2009
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NPlot;
using NPlot.Windows;

namespace DRAMVis
{
	public enum MouseState {Pointer, Zoom, Pan};
	
	public class Visualizer : Form
	{
		MenuStrip menuStrip;
		ToolStripMenuItem file;
		ToolStripMenuItem about;
		ToolStripMenuItem fileOpen;	
		ToolStripMenuItem help;
		ToolStrip toolStrip;
		ToolStripButton openFileButton;
		ToolStripButton saveGraphButton;
		ToolStripButton playButton;
		ToolStripButton playPlusButton;
		ToolStripButton pointerButton;
		ToolStripButton zoomButton;
		ToolStripButton handButton;
		ToolStripItem cycleCount;
		ToolStripComboBox displayAs;
		ComboBox deviceComboBox;
		TextBox traceNameDisplay;
		Button traceSelectionButton;
		TabControl controlPanel;
		TabPage resultsPage;
		TabPage devicePropertyPage;
		TabPage systemPropertyPage;
		TabControl mainWindow;
		TabPage bandwidthPage;
		TabPage latencyPage;
		TabPage latencyHistogramPage;
		TabPage powerPage;
		Form holdOnWindow;		
		TreeView previousResults;
		
		PropertyGrid systemPropertyGrid;
		PropertyGrid devicePropertyGrid;
		public SystemParameters systemParameters = new SystemParameters();
		public DeviceParameters deviceParameters = new DeviceParameters();
		

		
		LineGrapher gBandwidth;
		LineGrapher gLatency;
		BarGrapher gPower;
		BarGrapher gHistogram;
		
		MouseState mouseState = new MouseState();
		
		Grapher grapher;
		string inputFileName;
		string selectedDeviceName;
		
		static string DRAMSimPath;
		
		public Dictionary<string, VisFileContainer> visFileList;
		
		static public void Main(string[] args)
		{			
			if(!File.Exists("DRAMVis.ini"))
			{
				while(DRAMSimPath==null)
				{
					FolderBrowserDialog fbd = new FolderBrowserDialog();
					if(fbd.ShowDialog() == DialogResult.OK)
					{
						DRAMSimPath = fbd.SelectedPath;
						TextWriter tw = new StreamWriter("DRAMVis.ini");
						tw.WriteLine(DRAMSimPath);
						tw.Close();
					}
					else
					{
						MessageBox.Show("Please select DRAMSim home directory");
					}
				}
			}
			else
			{
				StreamReader sr = new StreamReader("DRAMVis.ini");
				DRAMSimPath = sr.ReadLine();
				Console.WriteLine("DRAMSim Path : "+DRAMSimPath);
			}
			
			string filename = DRAMSimPath + "/results/q3parsed.trc/micron_16M_8b_x8_sg3E/1Ch.4R.2GB.8TQ.16CQ.open_page.RtB.pRankpBank.scheme2.vis";
			if (args.Length > 0)
			{
				filename = args[0];
			}
			Application.Run(new Visualizer(filename));
		}

		/// <summary>
		/// Constructor - Build UI
		/// </summary>		
		public Visualizer(string filename)
		{			
			//valueTable = new Dictionary<string,List<decimal>>();
			visFileList = new Dictionary<string,VisFileContainer>();
	
			grapher = new Grapher(this);
			
			this.SuspendLayout();
			
			this.Size = new Size(1100,500);
			this.Text = "DRAMVis     University Of Maryland";
			
			//
			//menu bar at top of the screen
			//
			menuStrip = new MenuStrip();
			
			file = new ToolStripMenuItem("File");
			about = new ToolStripMenuItem("About");
			menuStrip.Items.Add(file);
			menuStrip.Items.Add(about);
			
			fileOpen = new ToolStripMenuItem("Open .vis File");
			fileOpen.Click += new System.EventHandler(this.OpenFileButton_Click);
			file.DropDownItems.Add(fileOpen);
			
			help = new ToolStripMenuItem("Help");
			about.DropDownItems.Add(help);
			
			//
			//tool strip stuff
			//
			toolStrip = new ToolStrip();
			
			pointerButton = new ToolStripButton(new Bitmap("../../pointer.png"));
			pointerButton.Click += new EventHandler(this.PointerButton_Click);
			pointerButton.ToolTipText = "Pointer";
			pointerButton.Size = new Size(32,32);
			zoomButton = new ToolStripButton(new Bitmap("../../zoom.png"));
			zoomButton.Click += new EventHandler(this.ZoomButton_Click);
			zoomButton.ToolTipText = "Zoom";
			handButton = new ToolStripButton(new Bitmap("../../pan.png"));
			handButton.Click += new EventHandler(this.HandButton_Click);
			handButton.ToolTipText = "Pan";
			
			saveGraphButton = new ToolStripButton(new Bitmap("../../savegraph.png"));
			saveGraphButton.ToolTipText = "Save graphs to file";
			saveGraphButton.Click += new EventHandler(this.SaveGraphButton_Click);			
			openFileButton = new ToolStripButton(new Bitmap("../../open.png"));
			openFileButton.Click += new EventHandler(this.OpenFileButton_Click);
			openFileButton.ToolTipText = "Open .vis file";
			playButton = new ToolStripButton(new Bitmap("../../play.png"));
			playButton.Click += new EventHandler(this.PlayButton_Click);
			playButton.ToolTipText = "Run simulation";
			playButton.Name = "Play";
			playPlusButton = new ToolStripButton(new Bitmap("../../playplus.png"));
			playPlusButton.Click += new EventHandler(this.PlayButton_Click);
			playPlusButton.ToolTipText = "Run and Add";
			playPlusButton.Name = "PlayPlus";
			
			ToolStripItem runFor = new ToolStripLabel("Run For:");
			cycleCount = new ToolStripTextBox();
			cycleCount.Size = new Size(70,0);
			cycleCount.Text = "500000";
			ToolStripItem cyclesLabel = new ToolStripLabel("cycles");
			
			ToolStripItem displayAsLabel = new ToolStripLabel("Display As:");
			displayAs = new ToolStripComboBox();
			displayAs.Items.AddRange(new string[] {"Total","Total Average", "Per Rank","Per Bank"});
			displayAs.SelectedIndex = 0;
			displayAs.SelectedIndexChanged += new EventHandler(this.DataDisplayCombo_IndexChanged);
			
			toolStrip.Items.AddRange(new ToolStripItem[]{openFileButton, saveGraphButton,
				new ToolStripSeparator(),pointerButton,zoomButton,handButton,
				new ToolStripSeparator(),playButton, playPlusButton, 
				new ToolStripSeparator(), runFor, cycleCount, cyclesLabel,
				new ToolStripSeparator(), displayAsLabel,displayAs});
			
			//
			//control panel tab page
			//
			controlPanel = new TabControl();
			controlPanel.Dock = DockStyle.Fill;
			controlPanel.Alignment = TabAlignment.Bottom;

			
			//
			//system property page stuff
			//
			systemPropertyPage = new TabPage("System");
			systemPropertyPage.BorderStyle = BorderStyle.Fixed3D;
			systemPropertyPage.Anchor = AnchorStyles.Top;

			TableLayoutPanel systemtlp = new TableLayoutPanel();
			systemtlp.Margin = new Padding(0,0,0,0);
			systemtlp.RowCount = 3;
			systemtlp.ColumnCount = 2;
			
			//trace
			Label traceLabel = new Label();
			traceLabel.Text = "Trace File";
			traceLabel.TextAlign = ContentAlignment.MiddleCenter;
			traceLabel.Dock = DockStyle.Fill;
			traceLabel.Font = new Font(traceLabel.Font, FontStyle.Bold);
			systemtlp.Controls.Add(traceLabel,0,0);
			systemtlp.SetColumnSpan(traceLabel,2);
			
			traceSelectionButton = new Button();
			traceSelectionButton.Image = new Bitmap(new Bitmap("../../open.png"), new Size(16,16));
			traceSelectionButton.Click += new EventHandler(this.TraceSelectionButton_Click);
			systemtlp.Controls.Add(traceSelectionButton,0,1);			
			
			traceNameDisplay = new TextBox();
			traceNameDisplay.BorderStyle = BorderStyle.Fixed3D;
			traceNameDisplay.TextAlign = HorizontalAlignment.Center;
			traceNameDisplay.BackColor = Color.White;
			traceNameDisplay.Dock = DockStyle.Fill;
			traceNameDisplay.ReadOnly = true;
			systemtlp.Controls.Add(traceNameDisplay,1,1);
			
			systemPropertyGrid = new PropertyGrid();
			systemPropertyGrid.Dock = DockStyle.Fill;
			systemPropertyGrid.SelectedObject = systemParameters;
			
			systemtlp.Controls.Add(systemPropertyGrid,0,2);
			systemtlp.SetColumnSpan(systemPropertyGrid, 2);
			
			systemtlp.Dock = DockStyle.Fill;
			systemtlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,35));
			systemtlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,185));

			systemPropertyPage.Controls.Add(systemtlp);
			systemPropertyPage.Resize += delegate(object sender, EventArgs e) {
				TabPage x = (TabPage)sender;
				systemPropertyGrid.Height = x.Height-150;
			};
			//
			//Device Property page stuff
			//
			devicePropertyPage = new TabPage("Device");
			devicePropertyPage.BorderStyle = BorderStyle.Fixed3D;
			
			TableLayoutPanel devicetlp = new TableLayoutPanel();
			devicetlp.RowCount = 3;
			devicetlp.ColumnCount = 1;
			
			devicePropertyGrid = new PropertyGrid();
			devicePropertyGrid.Dock = DockStyle.Fill;
			
			Label deviceComboLabel = new Label();
			deviceComboLabel.Text = "Device Name";
			deviceComboLabel.TextAlign = ContentAlignment.MiddleCenter;
			deviceComboLabel.Font = new Font(deviceComboLabel.Font, FontStyle.Bold);
			deviceComboLabel.Dock = DockStyle.Fill;
			
			deviceComboBox = new ComboBox();
			deviceComboBox.Dock = DockStyle.Fill;
			FillDeviceComboBox();
			deviceComboBox.SelectedIndexChanged += new EventHandler(this.DeviceComboBox_IndexChanged);
			deviceComboBox.SelectedIndex = 0;
			
			devicetlp.Controls.Add(deviceComboLabel,0,0);
			devicetlp.Controls.Add(deviceComboBox,0,1);
			devicetlp.Controls.Add(devicePropertyGrid,0,2);
			devicetlp.Dock = DockStyle.Fill;
			devicetlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,35));
			devicetlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,185));
			
			devicePropertyPage.Resize += delegate(object sender, EventArgs e) {
				TabPage x = (TabPage)sender;
				devicePropertyGrid.Height = x.Height-150;
			};
			devicePropertyPage.Controls.Add(devicetlp);
			
			//
			//previous results page
			//
			TableLayoutPanel resultstlp = new TableLayoutPanel();
			resultstlp.ColumnCount = 1;
			resultstlp.RowCount = 2;
			
			resultsPage = new TabPage();
			resultsPage.Text = "Results";
			
			Label resultsLabel = new Label();
			resultsLabel.Text = "Previous Results";
			resultsLabel.TextAlign = ContentAlignment.MiddleCenter;
			resultsLabel.Font = new Font(resultsLabel.Font, FontStyle.Bold);
			resultsLabel.Dock = DockStyle.Fill;
			resultstlp.Controls.Add(resultsLabel, 0, 0);
			
			resultstlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,222));
			resultstlp.Dock = DockStyle.Fill;
			resultsPage.Controls.Add(resultstlp);
			
			previousResults = new TreeView();
			previousResults.Dock = DockStyle.Fill;
			previousResults.AfterCheck += new TreeViewEventHandler(this.PreviousResults_AfterCheck);
			previousResults.AfterSelect += new TreeViewEventHandler(this.PreviousResults_AfterSelect);
			previousResults.CheckBoxes = true;
			
			resultstlp.Controls.Add(previousResults,0,1);
			
			//add pages to control panel
			controlPanel.Controls.AddRange(new TabPage[]{systemPropertyPage,devicePropertyPage,resultsPage});
			controlPanel.SelectedIndex = 0;
			
			//
			//main window stuff
			//
			mainWindow = new TabControl();
			mainWindow.Dock = DockStyle.Fill;

			NPlot.Windows.PlotSurface2D plotBandwidth = new NPlot.Windows.PlotSurface2D();
			NPlot.Windows.PlotSurface2D plotLatency = new NPlot.Windows.PlotSurface2D();
			
			gBandwidth = new LineGrapher(plotBandwidth, "bandwidth", "Bandwidth (GB/s)", "Time (ms)", "Bandwidth (GB/s)");
			gLatency = new LineGrapher(plotLatency, "latency", "Average Latency", "Time (ms)", "Latency (nanoseconds)");
			
			//click handlers for right click to zoom out behavior
			
/*
			plotBandwidth.MouseClick += new MouseEventHandler(TriggerZoomOut);
			plotLatency.MouseClick += new MouseEventHandler(TriggerZoomOut);
			plotLatencyHistogram.MouseClick += new MouseEventHandler(TriggerZoomOut);
			plotPower.MouseClick += new MouseEventHandler(TriggerZoomOut);
*/
			bandwidthPage = new TabPage("Bandwidth");
			bandwidthPage.BackColor = Color.Transparent;
			/*
			bandwidthPage.MouseEnter += (this.mainWindow_MouseEnter);
			bandwidthPage.MouseLeave += (this.mainWindow_MouseLeave);
			*/
			latencyPage = new TabPage("Latency");
			latencyPage.BackColor = Color.Transparent;
			/*
			latencyPage.MouseEnter += (this.mainWindow_MouseEnter);
			latencyPage.MouseLeave += (this.mainWindow_MouseLeave);
			*/
			
			latencyHistogramPage = new TabPage("Histogram");
			latencyHistogramPage.BackColor = Color.Transparent;
			/*
			latencyHistogramPage.MouseEnter += (this.mainWindow_MouseEnter);
			latencyHistogramPage.MouseLeave += (this.mainWindow_MouseLeave);
			*/
			
			powerPage = new TabPage("Power");
			powerPage.BackColor = Color.Transparent;
			/*
			powerPage.MouseEnter += (this.mainWindow_MouseEnter);
			powerPage.MouseLeave += (this.mainWindow_MouseLeave);
			*/
			mainWindow.MouseEnter += this.mainWindow_MouseEnter;
			mainWindow.MouseLeave += this.mainWindow_MouseLeave;
			
			gPower = new BarGrapher(powerPage, "power");
			gHistogram = new BarGrapher(latencyHistogramPage, "histogram");
			
			latencyPage.Controls.Add(plotLatency);
			bandwidthPage.Controls.Add(plotBandwidth);	
			
			mainWindow.TabPages.AddRange(new TabPage[]{bandwidthPage,latencyPage,latencyHistogramPage,powerPage});

			SplitContainer sc = new SplitContainer();
			sc.Dock = DockStyle.Fill;
			sc.Panel1.Controls.Add(controlPanel);
			sc.Panel1.Padding = new Padding(3,3,3,5);
			sc.Panel1MinSize = 250;
			sc.Panel2.Controls.Add(mainWindow);
			sc.Panel2.Padding = new Padding(3,3,3,3);
			sc.Orientation = Orientation.Vertical;
			
			//add everythign to window
			Controls.AddRange(new Control[] { sc, toolStrip, menuStrip } );
			
			this.ResumeLayout();
			
			sc.SplitterDistance = 250;
		}
		
		/// <summary>
		/// Event handlers
		/// </summary>
		private void DataDisplayCombo_IndexChanged(object sender, EventArgs e)
		{
			switch(displayAs.SelectedIndex)
			{
			case 0:
				gBandwidth.SetDrawMode(DrawMode.TOTAL);
				gLatency.SetDrawMode(DrawMode.TOTAL);
				break;
			case 1:
				gBandwidth.SetDrawMode(DrawMode.AVERAGE);
				gLatency.SetDrawMode(DrawMode.AVERAGE);
				break;
			case 2:
				gBandwidth.SetDrawMode(DrawMode.RANK);
				gLatency.SetDrawMode(DrawMode.RANK);
				break;
			case 3:
				gBandwidth.SetDrawMode(DrawMode.BANK);
				gLatency.SetDrawMode(DrawMode.BANK);
				break;
			}
		}
		
		private void TraceSelectionButton_Click(object sender, EventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Title = "Select trace file";
			ofd.Filter = "Trace files (*.trc)|*.trc";
			ofd.InitialDirectory = DRAMSimPath+"/traces";
			ofd.Multiselect = false;
			if(ofd.ShowDialog() == DialogResult.OK)
			{
				string[] temps = ofd.FileName.Split('/');
				traceNameDisplay.Text = temps[temps.Length-1];
				
				gBandwidth.HighlightGraph(null);
				gLatency.HighlightGraph(null);
				gPower.HighlightGraph(null);
				gBandwidth.Clear();
				gLatency.Clear();
				gPower.Clear();
				gHistogram.Clear();
				visFileList.Clear();
				FillPreviousResults(null);
			}
		}
		
		private void DeviceComboBox_IndexChanged(object sender, EventArgs e)
		{
			selectedDeviceName = deviceComboBox.SelectedText;
			VisFileReader vfr = new VisFileReader(DRAMSimPath + "/ini/" + selectedDeviceName);
			deviceParameters = vfr.ReadDeviceData(DRAMSimPath + "/ini/" + selectedDeviceName);
			
			devicePropertyGrid.SelectedObject = null;
			devicePropertyGrid.SelectedObject = deviceParameters;
		
			((SystemParameters)(systemPropertyGrid.SelectedObject)).TOTAL_STORAGE = (long)(systemParameters.JEDEC_DATA_BUS_WIDTH / deviceParameters.DEVICE_WIDTH) * systemParameters.NUM_RANKS * 
				((long)deviceParameters.NUM_ROWS * deviceParameters.NUM_COLS * deviceParameters.DEVICE_WIDTH * deviceParameters.NUM_BANKS) / 8;
			systemPropertyGrid.Refresh();
		}
		
		private void PlayButton_Click(object sender, EventArgs e)
		{
			if(this.traceNameDisplay.Text=="")
			{
				MessageBox.Show("Please select a trace");
				return;
			}
			
			string DRAMSimExe = DRAMSimPath + "/DRAMSim";
			if(!File.Exists(DRAMSimExe))
			{
				MessageBox.Show("DRAMSim binary not found");
				return;
			}		
			
			if(selectedDeviceName==null)
			{
				selectedDeviceName = (string)deviceComboBox.Items[deviceComboBox.SelectedIndex];
			}
			
			systemParameters = (SystemParameters)systemPropertyGrid.SelectedObject;
			if(systemParameters.TOTAL_STORAGE==0)
			{
				deviceComboBox.SelectedIndex = deviceComboBox.SelectedIndex;
			}
			this.systemParameters.PrintToFile(DRAMSimPath+"/system.ini");
			
			string traceTypeFlag;
			if(traceNameDisplay.Text.StartsWith("k6"))
			{
				traceTypeFlag = "-k";
			}
			else
			{
				traceTypeFlag = "";
			}
			
			//remove highlights
			gBandwidth.HighlightGraph(null);
			gLatency.HighlightGraph(null);
			gPower.HighlightGraph(null);
			
			if(((ToolStripButton)sender).Name.Equals("Play"))
			{
				visFileList.Clear();
				gBandwidth.Clear();
				gLatency.Clear();
				gPower.Clear();
				gHistogram.Clear();
			}
			
			string DRAMSimArgs = "-c "+cycleCount.Text+" -q "+traceTypeFlag+" -p "+DRAMSimPath+" -t traces/"+
				traceNameDisplay.Text+" -s system.ini -d ini/" + selectedDeviceName;
			
			Console.WriteLine("Executing : "+DRAMSimExe+" "+DRAMSimArgs);
			string deviceName = selectedDeviceName;
			string outputFilename = systemParameters.GetVisFilename();
			
			int lastSlash = deviceName.LastIndexOf("/");
			if (lastSlash > 0) 
			{
				deviceName = deviceName.Substring(lastSlash+1);
			}
			int iniExtPosition = deviceName.LastIndexOf(".ini");
			if (iniExtPosition > 0) 
			{
				deviceName = deviceName.Substring(0,iniExtPosition);
			}

			this.inputFileName = DRAMSimPath+"/results/"+traceNameDisplay.Text+"/"+deviceName+"/"+outputFilename;
			
			holdOnWindow = new Form();
			holdOnWindow.Text = "DRAMSim Executing";
			Label msg = new Label();
			msg.Text = "Please wait while execution finishes . . . ";
			msg.Dock = DockStyle.Fill;
			msg.Font = new Font(msg.Font, FontStyle.Bold);
			msg.TextAlign = ContentAlignment.MiddleCenter;
			holdOnWindow.Controls.Add(msg);
			holdOnWindow.Size = new Size(300,100);
			holdOnWindow.Show();
			holdOnWindow.ShowInTaskbar = false;
			holdOnWindow.UseWaitCursor = true;
			holdOnWindow.Location = new Point(this.Location.X + this.Size.Width/2,this.Location.Y + this.Size.Height/2);
			
			System.Diagnostics.Process p = new System.Diagnostics.Process();
			p = System.Diagnostics.Process.Start(DRAMSimExe, DRAMSimArgs);
			p.EnableRaisingEvents = true;
			p.Exited += new EventHandler(this.DRAMSim_Exit);
		}
				
		private void DRAMSim_Exit(object sender, EventArgs e)
		{
			holdOnWindow.Close();
			ResetZoom();
			FillPreviousResults(inputFileName);
		
			this.Refresh();
			mainWindow.Refresh();
			mainWindow.Invalidate();
		}
				
		private void SaveGraphButton_Click(object sender, EventArgs e)
		{
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.Title = "Save graphs as images";
			sfd.Filter = "PNG (*.png)|*.png";
			sfd.InitialDirectory = DRAMSimPath;
			if(sfd.ShowDialog() == DialogResult.OK)
			{
				string filename = sfd.FileName.Substring(0,sfd.FileName.Length-4);
		
				ResetZoom();
				
				Bitmap bandwidthImage = new Bitmap(gBandwidth.canvas.Width, gBandwidth.canvas.Height);
				Grapher.SetBitmapBackground(bandwidthImage, Color.White);
				gBandwidth.canvas.DrawToBitmap(bandwidthImage, gBandwidth.canvas.Bounds);
				bandwidthImage.Save(filename+"_bandwidth.png", System.Drawing.Imaging.ImageFormat.Png);
				
				Bitmap latencyImage = new Bitmap(gLatency.canvas.Width, gLatency.canvas.Height);
				Grapher.SetBitmapBackground(latencyImage, Color.White);
				gLatency.canvas.DrawToBitmap(latencyImage, gLatency.canvas.Bounds);
				latencyImage.Save(filename+"_latency.png",System.Drawing.Imaging.ImageFormat.Png);
				
				
			/*	Bitmap histogramImage = gHistogram.GetBitmap();
				histogramImage.Save(filename+"_histogram.png",System.Drawing.Imaging.ImageFormat.Png);*/
				
				Bitmap powerImage = gPower.GetBitmap();
				powerImage.Save(filename+"_power.png",System.Drawing.Imaging.ImageFormat.Png);
			}
		}
		
		private void mainWindow_MouseLeave(object sender, EventArgs e)
		{
			this.Cursor = Cursors.Default;
		//	Console.WriteLine("leaving ...");
		}
		
		private void mainWindow_MouseEnter(object sender, EventArgs e)
		{
			//Console.WriteLine("entering ..."+mouseState);
			switch(mouseState)
			{
			case MouseState.Pointer:
				this.Cursor = Cursors.Default;
				break;
			case MouseState.Pan:				
				this.Cursor = Cursors.Hand;
				break;
			case MouseState.Zoom:
				this.Cursor = Cursors.IBeam;
				break;
			}
		}
	
		private void SetAllMouseStates() 
		{
			gBandwidth.SetState(mouseState);
			gLatency.SetState(mouseState);
			gHistogram.SetState(mouseState);
			gPower.SetState(mouseState);
		}

		private void PointerButton_Click(object sender, EventArgs e)
		{
			mouseState = MouseState.Pointer;
			
			handButton.Checked = false;
			zoomButton.Checked = false;
			
			SetAllMouseStates();
		}
				
		private void HandButton_Click(object sender, EventArgs e)
		{
			mouseState = MouseState.Pan;

			pointerButton.Checked = false;
			handButton.Checked = true;
			zoomButton.Checked = false;	
			SetAllMouseStates();
		}
		
		private void ZoomButton_Click(object sender, EventArgs e)
		{
			mouseState = MouseState.Zoom;
			
			pointerButton.Checked = false;
			handButton.Checked = false;
			zoomButton.Checked = true;
			SetAllMouseStates();
		}
		
		
		private void TriggerZoomOut(object sender, MouseEventArgs e)
		{
			if(mouseState == MouseState.Zoom)
			{
				if (e.Button == MouseButtons.Right)
				{
					grapher.zoomOut((NPlot.Windows.PlotSurface2D)sender);
				}
			}
		}
				
		private void OpenFileButton_Click(object sender, EventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.InitialDirectory = DRAMSimPath+"/results";
			ofd.Multiselect = false;
			ofd.Title = "Select File";
			ofd.Filter = "DRAMVis Files (*.vis)|*.vis";
			if(ofd.ShowDialog() == DialogResult.OK)
			{
				visFileList.Clear();
				
				gBandwidth.Clear();
				gLatency.Clear();
				gPower.Clear();
				gHistogram.Clear();
				
				inputFileName = ofd.FileName;
				
				//fill in tracenamedisplay
				FileInfo file = new FileInfo(inputFileName);
				DirectoryInfo device = file.Directory;
				traceNameDisplay.Text = device.Parent.Name;
				
				//fill previous results (leads to check handler)
				FillPreviousResults(ofd.FileName);	
			}
		}
		
		private void CheckChildren(TreeNode node)
		{
			if(node.Nodes.Count>0)
			{
				foreach(TreeNode tn in node.Nodes)
				{
					tn.Checked= true;
					CheckChildren(tn);
				}
			}
		}
		
		private void PreviousResults_AfterCheck(object sender, TreeViewEventArgs e)
		{
			TreeNode checkedNode = e.Node;
			
			//handle recursion 
			if(checkedNode.Checked)
			{
				if(checkedNode.Parent!=null && !checkedNode.Parent.Checked)
				{
					checkedNode.Parent.Checked = true;
				}
				if(e.Action != TreeViewAction.Unknown && checkedNode.Nodes.Count>0)
				{
					foreach(TreeNode tn in checkedNode.Nodes)
					{
						tn.Checked = true;
						CheckChildren(tn);
					}
				}
			}
			else
			{
				if(checkedNode.Nodes.Count>0)
				{
					foreach(TreeNode tn in checkedNode.Nodes)
					{
						if(tn.Checked) tn.Checked = false;
					}
					}
			}
			
			//if the node is a child
			if(e.Node.Nodes.Count==0)
			{
				//and it is checked
				if(e.Node.Checked)
				{
					string[] pieces = e.Node.FullPath.Split('\\');
					string filename = DRAMSimPath+"/results/"+traceNameDisplay.Text+"/"+pieces[0]+"/"+pieces[1]+".1Ch."+
						pieces[2]+"."+pieces[3]+"."+pieces[4]+"."+pieces[5]+".vis";
					//Console.WriteLine("load file : "+filename);
					
					//make container of relevant data
					VisFileContainer vfc;
					//need to check if it is still present since it might have already been checked (and then unchecked)
					if(!visFileList.ContainsKey(filename))
					{
						vfc = new VisFileContainer(filename);

						visFileList.Add(filename,vfc);
						
						//prevents a node from staying highlighted after it has been checked, thereby forcing user to reselect to highlight
						previousResults.SelectedNode = null;
						
						//ensures that the file which was just produced from simulator output or from an open vis file
						//  dialog is the data which is displayed in the property grids
						string[] p = vfc.filepath.Split(System.IO.Path.DirectorySeparatorChar);
						if(inputFileName=="" || inputFileName==vfc.filepath)
						{
							//display the correct property grids
							devicePropertyGrid.SelectedObject = null;
							devicePropertyGrid.SelectedObject = vfc.deviceParameters;
							deviceParameters = vfc.deviceParameters;
							systemPropertyGrid.SelectedObject = null;
							systemPropertyGrid.SelectedObject = vfc.systemParameters;
							systemParameters = vfc.systemParameters;
							
							//highlight correct listing in device combo box
							deviceComboBox.SelectedIndex = deviceComboBox.FindString(p[p.Length-2]);
							
							((SystemParameters)(systemPropertyGrid.SelectedObject)).TOTAL_STORAGE = (long)(systemParameters.JEDEC_DATA_BUS_WIDTH / deviceParameters.DEVICE_WIDTH) * systemParameters.NUM_RANKS * 
								((long)deviceParameters.NUM_ROWS * deviceParameters.NUM_COLS * deviceParameters.DEVICE_WIDTH * deviceParameters.NUM_BANKS) / 8;
							systemPropertyGrid.Refresh();
						}
						
						vfc.configurationName = p[p.Length-1];
						vfc.deviceName = p[p.Length-2];
						
						Console.WriteLine("about to add "+vfc.configurationName+" and "+vfc.deviceName);
						this.gBandwidth.AddToCanvas(vfc);
						this.gLatency.AddToCanvas(vfc);
						this.gPower.AddGraph(vfc);
						this.gHistogram.AddGraph(vfc);	
					}
				}
				//unchecking
				else
				{
					string filename = e.Node.Name;
					VisFileContainer vfc = this.visFileList[filename];
					this.gBandwidth.RemoveFromCanvas(vfc);
					this.gLatency.RemoveFromCanvas(vfc);
					this.gPower.RemoveGraph(vfc);
					this.gHistogram.RemoveGraph(vfc);
					visFileList.Remove(filename);
				}
			}
		}
			
		private void PreviousResults_AfterSelect(object sender, EventArgs e)
		{
			//make sure something is selected
			if(previousResults.SelectedNode!=null)
			{
				//check to make sure it is a child node
				if(previousResults.SelectedNode.Nodes.Count==0 && previousResults.SelectedNode.Checked)
				{					
					//display the correct property grids
					devicePropertyGrid.SelectedObject = visFileList[previousResults.SelectedNode.Name].deviceParameters;
					deviceParameters = visFileList[previousResults.SelectedNode.Name].deviceParameters; 
					systemPropertyGrid.SelectedObject = visFileList[previousResults.SelectedNode.Name].systemParameters;
					systemParameters = visFileList[previousResults.SelectedNode.Name].systemParameters;
					
					//highlight correct listing in device combo box
					VisFileContainer vfc = visFileList[previousResults.SelectedNode.Name];
					string[] pieces = vfc.filepath.Split(System.IO.Path.DirectorySeparatorChar);
					deviceComboBox.SelectedIndex = deviceComboBox.FindString(pieces[pieces.Length-2]);
					
					((SystemParameters)(systemPropertyGrid.SelectedObject)).TOTAL_STORAGE = (long)(systemParameters.JEDEC_DATA_BUS_WIDTH / deviceParameters.DEVICE_WIDTH) * systemParameters.NUM_RANKS * 
						((long)deviceParameters.NUM_ROWS * deviceParameters.NUM_COLS * deviceParameters.DEVICE_WIDTH * deviceParameters.NUM_BANKS) / 8;
					systemPropertyGrid.Refresh();
					
					// highlight this data
					gBandwidth.HighlightGraph(vfc);
					gLatency.HighlightGraph(vfc);
					gPower.HighlightGraph(vfc);
				}
				else 
				{
					gBandwidth.HighlightGraph(null);
					gLatency.HighlightGraph(null);
					gPower.HighlightGraph(null);
				}
			}
		}

		
		/// <summary>
		/// helper functions
		/// </summary>
		
		//since the NPlot interactionOccurred callback is freaking useless, this is a hack to get a handle on the plot that is being interacted 
		public NPlot.Windows.PlotSurface2D GetCurrentlyDisplayedGraph()
		{
			/*
			TabPage selectedTabPage =this.mainWindow.SelectedTab;
			
			if (selectedTabPage == this.bandwidthPage) 
			{
				return gBandwidth.can;
			} 
			else if (selectedTabPage == this.powerPage)
			{
				return this.plotPower;
			} 
			else if (selectedTabPage == this.latencyHistogramPage)
			{
				return this.plotLatencyHistogram;
			} 
			else if (selectedTabPage == this.latencyPage) 
			{
				return this.plotLatency;
			} 
			*/
			return null;
		}

		//Fills previous results TreeView control
		//  If argument is not null, it should be filepath to a 
		//    result that needs to be checked
		//  If argument is null, all remained unchecked
		private void FillPreviousResults(string checkedname)
		{
			if(this.traceNameDisplay!=null || this.traceNameDisplay.Text!="")
			{
				if(checkedname!=null) Console.WriteLine("Going to check : "+checkedname);
				
				previousResults.Nodes.Clear();
				string resultsDirStr = DRAMSimPath+"/results/"+traceNameDisplay.Text+"/";
				DirectoryInfo resultsDir = new DirectoryInfo(resultsDirStr);
				
				if(!resultsDir.Exists) {
					Console.WriteLine("Results dir: "+resultsDirStr+ " not found");
					return;
				}
				
				DirectoryInfo[] devices = resultsDir.GetDirectories();
				foreach(DirectoryInfo d in devices)
				{
					TreeNode top = new TreeNode();
					top.Text = d.Name;
					previousResults.Nodes.Add(top);
					
					FileInfo[] results = d.GetFiles();
					foreach(FileInfo f in results)
					{
						//2GB.1Ch.2R.scheme1.open_page.8TQ.8CQ.BtR.pRankpBank.vis
						string[] filePieces = f.Name.Split('.');
						
						//look for or add total storage node
						TreeNode totalStorage = new TreeNode();
						bool totalFound = false;
						foreach(TreeNode totals in top.Nodes)
						{
							if(totals.Text.Equals(filePieces[0]))
							{
								totalStorage = totals;
								totalFound = true;
								break;
							}
						}
						if(!totalFound)
						{
							totalStorage.Text = filePieces[0];
							top.Nodes.Add(totalStorage);
						}
						
						//look for or add number of ranks node
						TreeNode numRanks = new TreeNode();
						bool ranksFound = false;
						foreach(TreeNode ranks in totalStorage.Nodes)
						{
							if(ranks.Text.Equals(filePieces[2]))
							{
								ranksFound = true;
								numRanks = ranks;
								break;
							}
						}
						if(!ranksFound)
						{
							numRanks.Text = filePieces[2];
							totalStorage.Nodes.Add(numRanks);
						}

						//look for or add mapping scheme nodes
						TreeNode addrMapping = new TreeNode();
						bool mappingFound = false;
						foreach(TreeNode maps in numRanks.Nodes)
						{
							if(maps.Text.Equals(filePieces[3]))
							{
								addrMapping = maps;
								mappingFound = true;
								break;
							}
						}
						if(!mappingFound)
						{
							addrMapping.Text = filePieces[3];
							numRanks.Nodes.Add(addrMapping);
						}
						
						//look for or add row-buffer policy nodes
						TreeNode rowBuffer = new TreeNode();
						bool foundRowBuffer = false;
						foreach(TreeNode rbs in addrMapping.Nodes)
						{
							if(rbs.Text.Equals(filePieces[4]))
							{
								rowBuffer = rbs;
								foundRowBuffer = true;
								break;
							}
						}
						if(!foundRowBuffer)
						{
							rowBuffer.Text = filePieces[4];
							addrMapping.Nodes.Add(rowBuffer);
						}
						
						//Add remaining shit to leaf nodes
						TreeNode leaf = new TreeNode();
						string leafLabel = "";
						for (int i=5; i < filePieces.Length-1; i++)
						{
							leafLabel += filePieces[i];
							if (i < filePieces.Length-2)
								leafLabel +=".";
						}
						
						leaf.Text = leafLabel; // filePieces[5]+"."+filePieces[6]+"."+filePieces[7]+"."+filePieces[8];
						Console.WriteLine("Leaf text is going to be: "+leafLabel);
						leaf.Name = f.FullName;
						rowBuffer.Nodes.Add(leaf);
						
						if(visFileList.ContainsKey(leaf.Name))
						{
							leaf.Checked = true;
						}
						
						//if we want to check a particular node, make sure argument isn't null
						//  NOTE: this will call AfterCheck handler
						if(checkedname!=null)
						{
							
							if(leaf.Name.Equals(checkedname)) leaf.Checked = true;
						}
					}
				}
			}
			
			previousResults.ExpandAll();
			this.Refresh();
		}
		
		private void FillDeviceComboBox()
		{
			DirectoryInfo iniRoot = new DirectoryInfo(DRAMSimPath+"/ini");
			
			FileInfo[] files = iniRoot.GetFiles("*.ini");
			foreach(FileInfo fi in files)
			{
				deviceComboBox.Items.Add(fi.Name);
			}
		}
		
		private void ResetZoom()
		{
			mouseState = MouseState.Pointer;
			gBandwidth.ResetZoom();
			gLatency.ResetZoom();
			gPower.ResetZoom();
			gHistogram.ResetZoom();
			/*
			plotBandwidth.OriginalDimensions();
			plotLatency.OriginalDimensions();
			plotLatencyHistogram.OriginalDimensions();
			plotPower.OriginalDimensions();
			*/
		}

	}
}
