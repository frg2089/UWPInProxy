using Microsoft.Win32;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace UWPInProxy
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool CB { get; private set; }
        public bool SID { get; private set; }
        public bool M { get; private set; }
        public bool DN { get; private set; }
        public bool Scan { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            MenuItem_Click();
            CB = !false;
            SID = !true;
            M = !true;
            DN = !true;
            Scan = !false;
        }
        private List<string> Getlist()
        {
            List<string> ret = new List<string>();
            Process CmdProcess = new Process();
            CmdProcess.StartInfo.FileName = "CheckNetIsolation.exe";
            CmdProcess.StartInfo.CreateNoWindow = true;         // 不创建新窗口    
            CmdProcess.StartInfo.UseShellExecute = false;       //不启用shell启动进程  
            CmdProcess.StartInfo.RedirectStandardInput = true;  // 重定向输入    
            CmdProcess.StartInfo.RedirectStandardOutput = true; // 重定向标准输出    \
            CmdProcess.StartInfo.RedirectStandardError = false;  // 重定向错误输出  
            CmdProcess.StartInfo.Arguments = "LoopBackExempt -s";//“/C”表示执行完命令后马上退出  
            CmdProcess.Start();//执行  
            string returnvalue = CmdProcess.StandardOutput.ReadToEnd();//获取返回值  
            Debug.WriteLine("Progres Return:\n"+returnvalue);
            File.WriteAllText("CheckNetIsolationReurn.temp", returnvalue);
            CmdProcess.WaitForExit();//等待程序执行完退出进程  
            StreamReader sr = new StreamReader("CheckNetIsolationReurn.temp");//获取返回值 
            CmdProcess.Close();//结束  
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (line == null) continue;
                if (line.Contains("SID:"))
                    ret.Add(line.Split(':')[1].Replace(" ", ""));
            }
            return ret;
        }
        private bool Equal(List<string> str1, string str2)
        {
            foreach (string str in str1)
                if (str.Replace(" ","").Equals(str2))
                    return true;
            return false;
        }
        class SIDName
        {
            public string SID { get; set; }
            public string DisplayName { get; set; }
            public string Moniker { get; set; }
            public bool IsUsing { get; set; }
            public Brush Brush { get; set; }
        }
        private void MenuItem_Click()
        {
            dgList.Items.Clear();
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppContainer\Mappings");
            List<string> SIDUsingList = Getlist();
            foreach (string keyn in key.GetSubKeyNames())
            {
                RegistryKey nameKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Classes\Local Settings\Software\Microsoft\Windows\CurrentVersion\AppContainer\Mappings\" + keyn);
                dgList.Items.Add(new SIDName()
                {
                    SID = keyn,
                    DisplayName = (string)nameKey.GetValue("DisplayName"),
                    Moniker = (string)nameKey.GetValue("Moniker"),
                    IsUsing = Equal(SIDUsingList, keyn),
                    Brush = ReturnBrush(Equal(SIDUsingList, keyn))
                });
            }
        }
        private Brush ReturnBrush(bool value)
        {
            if (value) return
                 new SolidColorBrush(Color.FromArgb(0x80, 0, 200, 0));
            else return
                 new SolidColorBrush(Color.FromArgb(0x80, 200, 0, 0));

        }
        private void MenuItem_Click(object sender, RoutedEventArgs e) => MenuItem_Click();

        private void Change_Click(object sender, RoutedEventArgs e)
        {
            SIDName seleted = (SIDName)dgList.SelectedItem;
            Process CmdProcess = new Process();
            CmdProcess.StartInfo.FileName = "CheckNetIsolation.exe";
            CmdProcess.StartInfo.CreateNoWindow = true;         // 不创建新窗口    
            CmdProcess.StartInfo.UseShellExecute = false;       //不启用shell启动进程  
            CmdProcess.StartInfo.RedirectStandardInput = true;  // 重定向输入    
            CmdProcess.StartInfo.RedirectStandardOutput = true; // 重定向标准输出    \
            CmdProcess.StartInfo.RedirectStandardError = false;  // 重定向错误输出  
            if (seleted.IsUsing) CmdProcess.StartInfo.Arguments = "LoopBackExempt -d -p=" + seleted.SID;
            else CmdProcess.StartInfo.Arguments = "LoopBackExempt -a -p=" + seleted.SID;
            CmdProcess.Start();//执行  
            string returnvalue = CmdProcess.StandardOutput.ReadToEnd();//获取返回值  
            Debug.WriteLine("Progres Return:\n" + returnvalue);
            File.WriteAllText("CheckNetIsolationReurn.temp", returnvalue);
            CmdProcess.WaitForExit();//等待程序执行完退出进程  
            seleted.IsUsing = !seleted.IsUsing;
            MenuItem_Click();
        }
        private void Exit_Click(object sender, RoutedEventArgs e) => Close();

        private void miSSID_Click(object sender, RoutedEventArgs e)
        {
            if (SID)
                dgtcSID.Visibility = Visibility.Visible;
            else dgtcSID.Visibility = Visibility.Collapsed;
            SID = !SID;
        }

        private void miSDN_Click(object sender, RoutedEventArgs e)
        {
            if (DN)
                dgtcDN.Visibility = Visibility.Visible;
            else dgtcDN.Visibility = Visibility.Collapsed;
            DN = !DN;
        }

        private void miSCB_Click(object sender, RoutedEventArgs e)
        {
            if (CB)
                dgcbcSID.Visibility = Visibility.Visible;
            else dgcbcSID.Visibility = Visibility.Collapsed;
            CB = !CB;
        }

        private void miM_Click(object sender, RoutedEventArgs e)
        {
            if (M)
            dgtcM.Visibility = Visibility.Visible;
               
            else   dgtcM.Visibility = Visibility.Collapsed;
            M = !M;
        }

        private void miScan_Click()
        {
            if (Scan)
            {
                gSearch.Visibility = Visibility.Visible;
                Height += gSearch.Height;
                tbSearchText.Focus();
            }
            else
            {
                Height -= gSearch.Height;
                gSearch.Visibility = Visibility.Collapsed;
            }
            Scan = !Scan;
        }
        private void miScan_Click(object sender, RoutedEventArgs e) => miScan_Click();
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string str = tbSearchText.Text.ToLower();
            bool ret = false;
            for (int i = 0; i < dgList.Items.Count; i++)
            {
                SIDName item = (SIDName)dgList.Items[i];
                if (cbSearchSID.IsChecked == true)
                    ret= item.SID.ToLower().Contains(str);
                if (cbSearchM.IsChecked == true)
                    ret=item.Moniker.ToLower().Contains(str);
                if (cbSearchDN.IsChecked == true)
                    ret=item.DisplayName.ToLower().Contains(str);
                if (ret)
                {                  
                    dgList.SelectedIndex = i;
                    dgList.ScrollIntoView(dgList.SelectedItem);
                    dgList.UpdateLayout();
                    MessageBoxResult msgret = MessageBox.Show(
                        "SID\t\t: " + item.SID +
                        "\nMoniker\t\t: " + item.Moniker +
                        "\nDisplayName\t: " + item.DisplayName +
                        "\n\t\t\tContinued?",
                        "Find a Result!", MessageBoxButton.YesNo
                    );
                    switch (msgret)
                    {
                        case MessageBoxResult.None:
                            break;
                        case MessageBoxResult.OK:
                        case MessageBoxResult.Yes:
                            continue;
                        case MessageBoxResult.Cancel:
                        case MessageBoxResult.No:
                            miScan_Click();
                            return;
                        default:
                            break;
                    }
                }
                if (i==dgList.Items.Count-1)
                {
                    MessageBox.Show("Cannot Found", "Not Found");
                    miScan_Click();
                }
            }
        }
    }
}
