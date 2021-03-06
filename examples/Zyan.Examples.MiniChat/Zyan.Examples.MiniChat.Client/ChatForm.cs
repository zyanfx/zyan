﻿using System;
using System.Threading;
using System.Windows.Forms;
using Zyan.Communication;
using Zyan.Communication.Protocols.Tcp;
using Zyan.Examples.MiniChat.Shared;

namespace Zyan.Examples.MiniChat.Client
{
	public partial class ChatForm : Form
	{
		private IMiniChat _chatProxy;

		public ChatForm(string nickname)
		{
			InitializeComponent();

			_nickName.Text = nickname;
			_chatProxy = Program.Connection.CreateProxy<IMiniChat>();
			_chatProxy.MessageReceived += new Action<string, string>(_chatProxy_MessageReceived);
		}

		private void _chatProxy_MessageReceived(string arg1, string arg2)
		{
			if (InvokeRequired)
			{
				Invoke(new Action<string, string>(_chatProxy_MessageReceived), arg1, arg2);
				return;
			}

			_chatList.Items.Insert(0, string.Format("{0}: {1}", arg1, arg2));
		}

		private void _sendButton_Click(object sender, EventArgs e)
		{
			var nickName = _nickName.Text;
			var message = _sayBox.Text;
			_sayBox.SelectAll();

			ThreadPool.QueueUserWorkItem(x => _chatProxy.SendMessage(nickName, message));
		}

		private void ChatForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			_chatProxy.MessageReceived -= new Action<string, string>(_chatProxy_MessageReceived);
		}
	}
}
