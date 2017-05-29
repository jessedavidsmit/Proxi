using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using Microsoft.Win32;

namespace Proxy
{
    public partial class Main : Form
    {
        static bool workModeActive = false;
        static Thread workMode;// = new Thread(WorkMode);
        static string proxyAddress;
        

        public Main()
        {
            InitializeComponent();
            circularProgressBar5.Visible = false;
            toolTip_proxyAddress.SetToolTip(txt_ProxyAddress, "proxy_name:port e.g. pl.example:8080");
            

            MdiClient ctlMDI;

            // Loop through all of the form's controls looking
            // for the control of type MdiClient.
            foreach (Control ctl in this.Controls)
            {
                try
                {
                    // Attempt to cast the control to type MdiClient.
                    ctlMDI = (MdiClient)ctl;

                    // Set the BackColor of the MdiClient control.
                    ctlMDI.BackColor = this.BackColor;
                }
                catch (InvalidCastException exc)
                {
                    // Catch and ignore the error if casting failed.
                }
            }

        }

        private void SetProxy_btn_Click(object sender, EventArgs e)
        {
            setProxyAddress();
            ModifyRegistry(1);   
        }

        private void Reset_btn_Click(object sender, EventArgs e)
        {
            ModifyRegistry(9);
        }

        private void setProxyAddress()
        {
            proxyAddress = txt_ProxyAddress.Text;
        }

        static void ModifyRegistry(int val)
        {


            RegistryKey defaultConnectionSettingsKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings\Connections", true);
            RegistryKey proxyEnablekey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings", true);
            RegistryKey proxyServerKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings", true);

            //Get current values from registry
            //int proxyEnableVal = Convert.ToInt32(proxyEnablekey.GetValue("ProxyEnable").ToString());
            //string proxyString = proxyServerKey.GetValue("ProxyServer").ToString();
            byte[] defConnection = (byte[])defaultConnectionSettingsKey.GetValue("DefaultConnectionSettings");
            byte[] savedLegacySetting = (byte[])defaultConnectionSettingsKey.GetValue("SavedLegacySettings");

            if (val == 9)
            { //NOTE: DefConnection 1 = Auto Off. DefConnection 9 = Auto On. 
                defConnection[8] = Convert.ToByte(9);
                savedLegacySetting[8] = Convert.ToByte(9);

                defaultConnectionSettingsKey.SetValue("DefaultConnectionSettings", defConnection);
                defaultConnectionSettingsKey.SetValue("SavedLegacySettings", savedLegacySetting);
                proxyEnablekey.SetValue("ProxyEnable", "0", RegistryValueKind.DWord);
            }
            else if (val == 1)
            {
                defConnection[8] = Convert.ToByte(1);
                savedLegacySetting[8] = Convert.ToByte(1);

                defaultConnectionSettingsKey.SetValue("DefaultConnectionSettings", defConnection);
                defaultConnectionSettingsKey.SetValue("SavedLegacySettings", savedLegacySetting);
                proxyEnablekey.SetValue("ProxyEnable", "1", RegistryValueKind.DWord);
                proxyServerKey.SetValue("ProxyServer", proxyAddress, RegistryValueKind.String);
            }

            proxyEnablekey.Close();
            proxyServerKey.Close();
            defaultConnectionSettingsKey.Close();
        }

        private void workMode_btn_Click(object sender, EventArgs e)
        {
            setProxyAddress();
            if (workModeActive == false)
            {
                circularProgressBar5.Visible = true;
                workMode = new Thread(WorkMode);
                workMode.IsBackground = true;
                workMode.Start();
                workModeActive = true;
                workMode_btn.Text = "Stop";
            }
            else if (workModeActive == true)
            {
                circularProgressBar5.Visible = false;
                workModeActive = false;
                workMode_btn.Text = "Continue";

            }
        }

        static void WorkMode()
        {
            EventWaitHandle waithandler = new EventWaitHandle(false, EventResetMode.AutoReset, Guid.NewGuid().ToString());

            do
            {
                waithandler.WaitOne(TimeSpan.FromSeconds(1));
                RegistryKey defaultConnectionSettingsKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings\Connections", true);
                RegistryKey proxyEnablekey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings", true);

                int proxyEnableVal = Convert.ToInt32(proxyEnablekey.GetValue("ProxyEnable").ToString());
                byte[] defConnection = (byte[])defaultConnectionSettingsKey.GetValue("DefaultConnectionSettings");

                if (defConnection[8] == 9 | proxyEnableVal == 0)
                {
                    ModifyRegistry(1);
                }
         

            } while (workModeActive == true);
        

            //while (workModeActive == true)
            //{
            //    RegistryKey defaultConnectionSettingsKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings\Connections", true);
            //    RegistryKey proxyEnablekey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Internet Settings", true);

            //    int proxyEnableVal = Convert.ToInt32(proxyEnablekey.GetValue("ProxyEnable").ToString());
            //    byte[] defConnection = (byte[])defaultConnectionSettingsKey.GetValue("DefaultConnectionSettings");

            //    if (defConnection[8] == 9 | proxyEnableVal == 0)
            //    {
            //        ModifyRegistry(1);
            //    }

            //}
            //MessageBox.Show("im working");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About abt = new About();
            abt.Show();
        }

    }
}
