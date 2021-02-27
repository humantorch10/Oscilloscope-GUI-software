using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.Windows.Forms.DataVisualization.Charting;
using System.Text.RegularExpressions;


namespace Oscilloscope
{
	public partial class Form1 : Form
	{

		int MAX_HISTORY = 30;
		int AVERAGE = 1;
		int check_channel = 1;
		private readonly object Lock1 = new object();
		private readonly object Lock2 = new object();
		Queue<double> Qdata2 = new Queue<double>();
		Queue<double> Qdata3 = new Queue<double>();
		int count;
		double sum2;
		private int timeScale = 0;
		private double ampScale = 0;



		private class BuadItem : Object
		{
			private string name = "";
			private int val = 0;
			
			public string NAME
			{
				set { name = value; }
				get { return name; }
			}    
		
			public int BAUDRATE
			{
				set { val = value; }
				get { return val; }
			}
		   
			public override string ToString()
			{
				return name;
			}
		}

		public Form1()
		{
			InitializeComponent();
		}

		private void BTN_CON_Click(object sender, EventArgs e)
		{
			if (serialPort1.IsOpen == true)
			{

				try
				{
					System.Windows.Forms.Application.DoEvents();
					if (true == serialPort1.IsOpen) serialPort1.DiscardInBuffer();
					System.Windows.Forms.Application.DoEvents();
					if (true == serialPort1.IsOpen) serialPort1.Close();
					CMB_BAUD.Enabled = true;
					CMB_COM.Enabled = true;
					System.Windows.Forms.Application.DoEvents();
					count = 1;

					BeginInvoke(new Delegate_ChangeButton(ChangeButton), new Object[] { "Connect" });

				}
				catch (Exception ex){ Console.WriteLine(ex); }
				
			}
			else {

				serialPort1.PortName = CMB_COM.SelectedItem.ToString();
				
				BuadItem baud = (BuadItem)CMB_BAUD.SelectedItem;
				serialPort1.BaudRate = baud.BAUDRATE;				
				serialPort1.DataBits = 8;				
				serialPort1.Parity = Parity.None;				
				serialPort1.StopBits = StopBits.One;	
				serialPort1.Handshake = Handshake.None;
				serialPort1.Encoding = Encoding.ASCII;
				CMB_BAUD.Enabled = false;
				CMB_COM.Enabled = false;
				count = 1;

						
				try
				{
					serialPort1.Open();
					if (true == serialPort1.IsOpen) serialPort1.DiscardInBuffer();
					BeginInvoke(new Delegate_ChangeButton(ChangeButton), new Object[] { "Disconnect" });
				   
				}
				catch (Exception ex){ Console.WriteLine(ex); }
			}

		}

		private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
		{

			BeginInvoke(new Delegate_GetDataText(GetDataText), new Object[] { CMB_DATA });
			BeginInvoke(new Delegate_GetAveText(GetAveText), new Object[] { CMB_AVE });
			BeginInvoke(new Delegate_GetTimeScale(GetTimeScale), new Object[] { numericUpDown2 });
			BeginInvoke(new Delegate_GetAmpScale(GetAmpScale), new Object[] { numericUpDown1 });


			string[] StrArryData = new string[10];
			string rcvData="";


			if( serialPort1.IsOpen == false )	return;

			check_channel = 1 - check_channel; // identify channel
			try {
				string comdata =  serialPort1.ReadLine();//ReadExisting()
				rcvData = comdata;
				StrArryData = comdata.Split(new string[] { "@",";" }, StringSplitOptions.RemoveEmptyEntries);
				//for (int i = 1; i < StrArryData.Length; i++)
				//{
				//	
				//}
				StrArryData[0] = StrArryData[0].Substring(0, 2);
			}
			catch (Exception ex){ Console.WriteLine(ex);}




			double tmp1 = 0.0;
			 
			if (StrArryData.Length >= 1) { 
				double.TryParse( Regex.Replace(StrArryData[0], "[^.0-9-]", ""), out tmp1);
			}

			
			
			if (count < AVERAGE)
			{
				sum2 = sum2 + tmp1;
				count++;
			}
			else {
				
				sum2 = sum2 + tmp1;
				if (check_channel == 0)
				{
					Qdata2.Enqueue(sum2 / (double)AVERAGE);
					sum2 = 0;
					count = 1;
				}
				else if (check_channel == 1)
				{
					Qdata3.Enqueue(sum2 / (double)AVERAGE);
					sum2 = 0;
					count = 1;
				}

			}

		   
			while (Qdata2.Count > MAX_HISTORY) Qdata2.Dequeue();

			while (Qdata3.Count > MAX_HISTORY) Qdata3.Dequeue();
			

			BeginInvoke(new Delegate_ShowChart(ShowChart2), new Object[] { chart2 });
			
			/* Testing values */
			//BeginInvoke(new Delegate_ChangeTxtBox(ChangeTxtBox), new Object[] { rcvData });



		}


		private delegate void Delegate_ShowChart(Chart chart);
		private delegate void Delegate_GetDataText(Control CON);
		private delegate void Delegate_GetAveText(Control CON);
		private delegate void Delegate_GetTimeScale(Control CON);
		private delegate void Delegate_GetAmpScale(Control CON);
		private delegate void Delegate_ChangeButton(string text);
		private delegate void Delegate_ChangeTxtBox(string text);


		private void ShowChart2(Chart chart)
		{
			double i = 0.0;
			var Qdata2ar = Qdata2.ToArray();
			var Qdata3ar = Qdata3.ToArray();
			if ((String)comboBox1.SelectedItem == "Channel 1")
			{
				chart.Series[0].Points.Clear();
				chart.Series[0].MarkerStyle = MarkerStyle.Circle;
				chart.Series[0].MarkerSize = 6;
				chart.Series[0].Color = Color.CornflowerBlue;
				if (Qdata2.Count > 2)
				{
					chart.ChartAreas[0].AxisY.ScaleView.Size = ampScale;
					chart.ChartAreas[0].AxisX.ScaleView.Size = timeScale;

				}

					//foreach (int value in Qdata2)
					//{
					//	chart.Series[0].Points.Add(new DataPoint(i, value));
					//	i += 0.20;
					//}

					for (int j = 0; j < Qdata2ar.Length; j++)
					{
						chart.Series[0].Points.Add(new DataPoint(i, Qdata2ar[j]));
						i += 0.5;
					}

				}
			if ((String)comboBox1.SelectedItem == "Channel 2")
			{
				chart.Series[0].Points.Clear();
				chart.Series[0].MarkerStyle = MarkerStyle.Square;
				chart.Series[0].MarkerSize = 6;
				chart.Series[0].Color = Color.DarkRed;
				if (Qdata3.Count > 2)
				{
					chart.ChartAreas[0].AxisY.ScaleView.Size = ampScale;
					chart.ChartAreas[0].AxisX.ScaleView.Size = timeScale;

				}

					//foreach (int value in Qdata3)
					//{
					//	chart.Series[0].Points.Add(new DataPoint(i, value));
					//	i += 0.20;
					//}

					for (int j = 0; j < Qdata3ar.Length; j++)
					{
						chart.Series[0].Points.Add(new DataPoint(i, Qdata3ar[j]));
						i += 0.5;
					}


				}
		}




		private void GetAmpScale(Control CON)
		{
			double.TryParse(CON.Text, out ampScale);
			
		}
		private void GetTimeScale(Control CON)
		{
			int.TryParse(CON.Text, out timeScale);
			
		}
		private void GetDataText(Control CON)
		{
			int.TryParse(CON.Text, out MAX_HISTORY);
			
		}

		private void GetAveText(Control CON)
		{

			int.TryParse(CON.Text, out AVERAGE);

		}

		private void ChangeButton(string text) {

			BTN_CON.Text = text;
		
		}


		private void Form1_Load(object sender, EventArgs e)
		{
			Init_Control();
		}

		private void Form1_FormClosed(object sender, FormClosedEventArgs e)
		{
			try
			{
				if (true == serialPort1.IsOpen) serialPort1.DiscardInBuffer();
				System.Windows.Forms.Application.DoEvents();
				if (serialPort1.IsOpen) serialPort1.Close();
				System.Windows.Forms.Application.DoEvents();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}


		private void Init_Control(){

			/*Set COM Port List*/
			string[] ports = SerialPort.GetPortNames();
			CMB_COM.Items.Clear();
			foreach (string prtname in ports)
			{
				CMB_COM.Items.Add(prtname);
			}
			if (CMB_COM.Items.Count > 0)
			{
				CMB_COM.SelectedIndex = CMB_COM.Items.Count - 1;
			}

			/*Set Baud Rate*/
			BuadItem baud;
			baud = new BuadItem();
			baud.NAME = "4800bps";
			baud.BAUDRATE = 4800;
			CMB_BAUD.Items.Add(baud);

			baud = new BuadItem();
			baud.NAME = "9600bps";
			baud.BAUDRATE = 9600;
			CMB_BAUD.Items.Add(baud);

			baud = new BuadItem();
			baud.NAME = "19200bps";
			baud.BAUDRATE = 19200;
			CMB_BAUD.Items.Add(baud);

			baud = new BuadItem();
			baud.NAME = "115200bps";
			baud.BAUDRATE = 115200;
			CMB_BAUD.Items.Add(baud);
			CMB_BAUD.SelectedIndex = 1;

			/* set channels */
			comboBox1.Items.Add("Channel 1");
			comboBox1.Items.Add("Channel 2");

			/*Set Average List*/
			CMB_AVE.Items.Add(1);
			for (int i = 1; i < 11; i++)
			{
				CMB_AVE.Items.Add(2 * i);
			}
			CMB_AVE.SelectedIndex = 0;

			/*Set Data List*/
			CMB_DATA.Items.Add(1);
			for (int i = 1; i < 11; i++)
			{
				CMB_DATA.Items.Add(i * 30);
			}
			CMB_DATA.SelectedIndex = 3;
		}

		public static int CountChar(string s, char c)
		{
			return s.Length - s.Replace(c.ToString(), "").Length;
		}

		private void CMB_DATA_SelectedIndexChanged(object sender, EventArgs e)
		{

		}

		private void CMB_AVE_SelectedIndexChanged(object sender, EventArgs e)
		{

		}

		private void groupBox2_Enter(object sender, EventArgs e)
		{

		}

		private void chart2_Click(object sender, EventArgs e)
		{

		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{

		}

		private void label3_Click(object sender, EventArgs e)
		{

		}

		private void label4_Click(object sender, EventArgs e)
		{

		}

		private void label5_Click(object sender, EventArgs e)
		{

		}

		private void label2_Click(object sender, EventArgs e)
		{

		}

		private void numericUpDown1_ValueChanged(object sender, EventArgs e)
		{

		}

		private void numericUpDown2_ValueChanged(object sender, EventArgs e)
		{

		}
	}
}
