/*
 * Created by SharpDevelop.
 * User: mnow
 * Date: 27.03.2020
 * Time: 13:10
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.IO.Ports;
using System.Globalization;
using System.Security.Cryptography;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Xml.Linq;
using System.Windows.Threading;

namespace RsCommunication
{
	/// <summary>
	/// Description of Class1.
	/// </summary>
	public class RsCom
	{
		const int SOP = 0xE1;       //frame start
		const int EOP = 0xE2;       //frame stop
		const int ESC = 0xE3;       //escape
		const int CAR = 0xE4;           //
		const int MAX_FRAME_DATA_SIZE = 6000;

		protected SerialPort rs;

		List<byte> bytesReceived = new List<byte>();
		bool isWaitEsc = false;
		bool isStarted = false;

		public delegate void FrameReceivedEventHandler(byte[] bytes);
		public event FrameReceivedEventHandler frameReceived;

        public delegate void RawDataReceivedEventHandler(byte[] bytes);
        public event RawDataReceivedEventHandler rawDataframeReceived;

        long cnt = 0;

		List<byte> buff = new List<byte>();

		int bytesRead = 0;

		public bool IsRaw { get; set; }

		public RsCom(string comName, int speed)
		{
			IsRaw = false;
            rs = new SerialPort(comName, speed, Parity.None, 8, StopBits.One);
			rs.DtrEnable = true;
        }

        public void Start()
		{
			rs.Open();
			rs.DataReceived += HandleSerialDataReceivedEventHandler;
		}

		public void Stop()
		{
			if (rs != null)
				rs.Close();
			rs = null;
		}

		public bool IsConnected()
		{
			return rs.IsOpen;
		}

		public void SendBytes(byte[] bts)
		{
			rs.Write(bts, 0, bts.Length);
		}


		public void SendFrame(byte[] bytes)
		{
			if (bytes.Length > MAX_FRAME_DATA_SIZE)
				return;

			List<byte> frame = new List<byte>();
			frame.Add(SOP);

			for (int i = 0; i < bytes.Length; i++)
			{
				if (bytes[i] == SOP || bytes[i] == EOP || bytes[i] == ESC || bytes[i] == CAR)
				{
					frame.Add(ESC);
				}

				frame.Add(bytes[i]);
			}
			frame.Add(EOP);

			int byteCnt = frame.Count;

			rs.Write(frame.ToArray(), 0, frame.Count);
		}

		void HandleSerialDataReceivedEventHandler(object sender, SerialDataReceivedEventArgs e)
		{
			SerialPort rsCom = (SerialPort)sender;

			if (rsCom.BytesToRead > 0)
			{
				byte[] buffer = new byte[rsCom.BytesToRead];
				rsCom.Read(buffer, 0, buffer.Length);

				bytesRead += buffer.Length;


				if (rawDataframeReceived != null)
				{
					rawDataframeReceived(buffer);
				}

                if (frameReceived != null)
                { 
					for (int i = 0; i < buffer.Length; i++)
					{
						byte currentByte = buffer[i];

						if (currentByte == ESC && !isWaitEsc)
						{
							isWaitEsc = true;
						}
						else if (isWaitEsc)
						{
							if (isStarted)
							{
								bytesReceived.Add(currentByte);
							}

							isWaitEsc = false;
						}
						else
						{
							if (currentByte == SOP)
							{
								isStarted = true;
								bytesReceived.Clear();
							}
							else if (currentByte == EOP)
							{
								if (bytesReceived.Count > 0)
								{

								}
								isStarted = false;
								byte[] bytes = bytesReceived.ToArray();
								frameReceived(bytes);
							}
							else
							{
								bytesReceived.Add(currentByte);
							}
						}
					}
				}
			}
		}

		void HandleSerialDataReceivedEventHandler2(object sender, SerialDataReceivedEventArgs e)
		{
			SerialPort rsCom = (SerialPort)sender;

			if (rsCom.BytesToRead > 0)
			{
				byte[] buffer = new byte[rsCom.BytesToRead];
				rsCom.Read(buffer, 0, buffer.Length);

				bytesRead += buffer.Length;

				if (frameReceived != null)
				{
					frameReceived(buffer);
				}
			}
		}

		public TextBox RsTexBox { get; set; }


		
	}
}
