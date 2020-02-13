﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WatsonTcp;

namespace Xalarwse
{
    public partial class Form1 : Form
    {
        private string windowName = "Xalarwse";
        private string userName = "user";
        public static string ipAddress = "127.0.0.1";
        private string port = "8910";
        private string picFileName = "";
        private bool demoMode = false;

        public WatsonTcpClient client = new WatsonTcpClient(ipAddress, 8910);

        private string receivedUserName = "other user";
        private Color receivedUserColor = Color.Blue;

        public Form1()
        {
            InitializeComponent();
            client.ServerConnected = ServerConnected;
            client.ServerDisconnected = ServerDisconnected;
            client.MessageReceived = MessageReceived;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            userAvatar.MouseHover += userAvatar_MouseHover;
            userLabel.MouseHover += userAvatar_MouseHover;
            comboBox1.SelectedItem = "Online";


            try
            {
                client.Start();
                demoMode = false;
            }
            catch (Exception)
            {
                MessageBox.Show("A connection to Xalarwse servers could not be established." +
                    "\nRunning in offline (demo) mode." +
                    "\nContact an administrator or retry connection.",
                    "Not Connected", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Text = windowName + " (offline mode)"; 
                demoMode = true;
                reconnectGroupBox.Visible = true;
            }
            usernamelabelchange(userName);
        }

        private void userAvatar_MouseHover(object sender, EventArgs e)
        {
            System.Windows.Forms.ToolTip toolTipAvatar = new System.Windows.Forms.ToolTip();
            toolTipAvatar.SetToolTip(this.userAvatar, "This is your current user avatar." +
                " You can change it in the settings.");
        }

        private void userLabel_MouseHover(object sender, EventArgs e)
        {
            System.Windows.Forms.ToolTip toolTipUsername = new System.Windows.Forms.ToolTip();
            toolTipUsername.SetToolTip(this.userLabel, "This is your current user name." +
                " You can change it in the settings.");
        }

        private void btnReconnect_MouseHover(object sender, EventArgs e)
        {
            System.Windows.Forms.ToolTip toolTipReconnect = new System.Windows.Forms.ToolTip();
            toolTipReconnect.SetToolTip(this.btnReconnect, "Click here to reconnect to Xalarwse Servers.");
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if(demoMode)
            {
                if (msgTextBox.Text!="")
                {
                    mainTextBox.SelectionColor = Color.Green;
                    mainTextBox.AppendText($"{userName}: ");
                    mainTextBox.SelectionColor = Color.Red;
                    mainTextBox.AppendText(msgTextBox.Text + " (Not Delivered)" + "\n");
                    msgTextBox.Text = "";
                }
            }
            else
                try
                {
                    client.Send(Encoding.UTF8.GetBytes(msgTextBox.Text));
                    msgTextBox.Text = "";
                    this.Text = windowName;
                }
                catch (Exception)
                {
                    MessageBox.Show("A connection to Xalarwse servers was terminated." +
                    "\nSwitching to offline (demo) mode." +
                    "\nContact an administrator or retry connection.",
                    "Connection Terminated", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Text = windowName + " (offline mode)"; //todo: PROGRESS BAR RECONNECTION
                    demoMode = true;
                    reconnectGroupBox.Visible = true;  //todo: MAKE IT UPDATE IN INTERVALS INSTEAD OF SEND
                }
        }

        private void usernamelabelchange(string username)
        {
            if (username.Length <= 12)
                userLabel.Text = username;
            else
                userLabel.Text = username.Substring(0, 11);
        }

        private void msgTextBox_TextChanged(object sender, EventArgs e)
        {
            if (msgTextBox.Text == "")
            {
                btnSend.Enabled = false;
            }
            else
            {
                btnSend.Enabled = true;
            }
        }

        private void btnOptions_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.Owner = this;
            form2.FormClosed += new FormClosedEventHandler(form2_FormClosed);
            form2.Show();
            btnOptions.Enabled = false;
        }

        void form2_FormClosed(object sender, EventArgs e)
        {
            btnOptions.Enabled = true;
            if (!string.IsNullOrEmpty(Form2.s_userName))
            {
                userName = Form2.s_userName;
                usernamelabelchange(Form2.s_userName);
            }
            if (!string.IsNullOrEmpty(Form2.s_ipAddress)) ipAddress = Form2.s_ipAddress;
            if (!string.IsNullOrEmpty(Form2.s_port)) port = Form2.s_port;
            if (!string.IsNullOrEmpty(Form2.s_picFileName))
            {
                picFileName = Form2.s_picFileName;
                userAvatar.Image = Bitmap.FromFile(picFileName);
            }
        }

        private void btnReconnect_Click(object sender, EventArgs e)
        {
            try
            {
                client.Start();
                demoMode = false;
                reconnectGroupBox.Visible = false;
                this.Text = windowName;
                MessageBox.Show("Reconnection to Xalarwse servers successful." +
                    "\nYou are now connected to Xalarwse servers." +
                    "\nYou can send and receive messages.",
                    "Reconnection Succeeded", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            catch (Exception)
            {
                MessageBox.Show("Reconnection to Xalarwse servers failed." +
                    "\nContact an administrator or retry connection again.",
                    "Reconnection Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Text = windowName + " (offline Mode)";
                demoMode = true;
            }
        }

        async Task MessageReceived(byte[] data)
        {
            if (!demoMode)
            {
                mainTextBox.SelectionColor = receivedUserColor;
                mainTextBox.AppendText($"{receivedUserName}: ");
                mainTextBox.SelectionColor = Color.Black;
                mainTextBox.AppendText(Encoding.UTF8.GetString(data) + "\n");
            }

        }

        async Task ServerConnected()
        {
            mainTextBox.AppendText("Connected to server\n");
        }

        async Task ServerDisconnected()
        {
            mainTextBox.AppendText("Disconnected from server\n");
        }
    }
}
