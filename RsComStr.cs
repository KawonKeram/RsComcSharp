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
using System.Windows.Markup;
using System.Data;
using System.Collections;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;
using System.Runtime.Intrinsics.X86;


namespace RsCommunication
{
	/// <summary>
	/// Description of Class1.
	/// </summary>
	public class RsComStr : RsCom
	{
		const int MAX_FRAME_DATA_SIZE = 512;

		Stopwatch stw = new Stopwatch();
		int bytesRead = 0;
		int rxCnt = 0;

        public delegate void CommandReceivedEventHandler(RsCommand command, byte[] data);
		public event CommandReceivedEventHandler frameCommandReceived;

        public delegate void AudioReceivedEventHandler(UInt64 timestamp, Int16[] data);
        public event AudioReceivedEventHandler AudioReceived;

        public delegate void AudioRawReceivedEventHandler(Int16[] data);
        public event AudioRawReceivedEventHandler AudioRawReceived;

        string readString = "";

		public enum RsCommand
		{
			FRAME_TYPE_CENTRAL = 1,
			FRAME_TYPE_TEXT    = 2,
			FRAME_TYPE_AUDIO   = 3,
			FRAME_TYPE_ACCGR   = 4,
            FRAME_TYPE_WIDGET	= 5,
			FRAME_TYPE_BMP_Text = 6,
            FRAME_TYPE_STATUS = 7,
            FRAME_TYPE_RECORD = 8,
            FRAME_TYPE_MFCC = 9,
            FRAME_TYPE_MFCC_START = 10,
            FRAME_TYPE_PROXY = 11,
            FRAME_TYPE_LOG = 12,
            FRAME_TYPE_LGH = 14,
            FRAME_TYPE_BATLOG = 15,
            FRAME_TYPE_PRXLOG = 16,
            FRAME_TYPE_BEEPER = 17,
            FRAME_TYPE_CURR = 18,
            FRAME_TYPE_SGEN = 19,
            FRAME_TYPE_FFT = 20,
            FRAME_TYPE_PRX = 21
        };

        public RsComStr(string comName, int speed) : base(comName, speed)
		{
			base.frameReceived += RsComStr_frameReceived;
            //base.audioRawReceived += RsComStr_audioRawReceived;

        }

        private void RsComStr_frameReceived(byte[] bytes)
		{
            if (AudioRawReceived != null)
            {
                if (AudioRawReceived != null)
                {
                    try
                    {
                        Int16[] samples = new Int16[bytes.Length / 2];

                        for (int i = 0; i < samples.Length; i++)
                        {
                            samples[i] = BitConverter.ToInt16(bytes, 2 * i);
                        }

                        AudioRawReceived(samples);
                    }
                    catch (Exception e)
                    {
                        //MessageBox.Show(e.Message + "rrrrrrrr");
                    }
                }
            }
            else 
            {
                if(frameCommandReceived != null)
                {
                    RsCommand rsCommand = (RsCommand)bytes[1];
                    byte[] data = new byte[bytes.Length-2];
                    Array.Copy(bytes, 2, data, 0, data.Length);
                    frameCommandReceived(rsCommand, data);
                }
            }

            rxCnt++;
		}

        public void SendCommand(RsCommand command, byte[] data)
        {
			byte[] dataOut = new byte[data.Length + 1];
			dataOut[0] = (byte)command;
			Array.Copy(data, 0, dataOut, 1, data.Length);
			SendFrame(dataOut);

			//SendFrame(data);
		}

        public void SendString(string message)
		{
			rs.WriteLine(message);
		}


	}

}
