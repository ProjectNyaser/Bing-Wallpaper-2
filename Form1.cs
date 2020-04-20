using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Bing_Wallpaper_2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private int iActulaWidth;
        private int iActulaHeight;
        private string key;

        private void Form1_Load(object sender, EventArgs e)
        {
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            iActulaWidth = Screen.PrimaryScreen.Bounds.Width;
            iActulaHeight = Screen.PrimaryScreen.Bounds.Height;
            label1.Text = $"请选择壁纸图片的分辨率({iActulaWidth}×{iActulaHeight})";
            foreach (string item in listBox1.Items)
            {
                if (item == $"{iActulaWidth}×{iActulaHeight}")
                {
                    listBox1.SelectedIndex = listBox1.Items.IndexOf(item);
                    timer1.Enabled = true;
                    break;
                }
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                iActulaWidth = int.Parse(listBox1.SelectedItem.ToString().Substring(0, listBox1.SelectedItem.ToString().IndexOf("×")));
                iActulaHeight = int.Parse(listBox1.SelectedItem.ToString().Substring(listBox1.SelectedItem.ToString().IndexOf("×") + 1));
            }
            catch(Exception error)
            {
                MessageBox.Show(error.Message, "Bing Wallpaper 2");
                return;
            }
            listBox1.Enabled = false;
            button1.Enabled = false;
            label1.Text = "等待中";
            string path = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)}\\Bing Wallpaper";
            if (path.Substring(path.Length - 1) != "\\")
            {
                path += "\\";
            }
            if (DateTime.Today.Year.ToString().Length != 4)
            {
                path += "0000".Substring(4 - DateTime.Today.Year.ToString().Length);
            }
            path += DateTime.Today.Year.ToString();
            if (DateTime.Today.Month.ToString().Length != 2)
            {
                path += "00".Substring(2 - DateTime.Today.Month.ToString().Length);
            }
            path += DateTime.Today.Month.ToString();
            if (DateTime.Today.Day.ToString().Length != 2)
            {
                path += "00".Substring(2 - DateTime.Today.Day.ToString().Length);
            }
            path += DateTime.Today.Day.ToString();
            path += ".jpg";
            if (DownLoadFile($"https://api.berryapi.net/?service=App.Bing.Images&w={iActulaWidth}&h={iActulaHeight}", path, (int Maximum, int Value) =>
        {
            key = "Byte";
            if (Value >= 1024)
            {
                Value /= 1024;
                key = "KiloByte";
            }
            if (Value > 1)
            {
                key += "s";
            }
            label1.Text = $"已下载 {Value} {key}";
        }))
            {
                SystemParametersInfo(20, 0, path, 0x2);
                Application.Exit();
            }
            else
            {
                listBox1.Enabled = true;
                button1.Enabled = true;
                label1.Text = "出现错误，请稍后再试";
            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            Button1_Click(null, null);
        }

        /*---------------------------*\
        |* 没错，下面都不是我自己写的*|
        \*---------------------------*/

        /// <summary>  
        /// 下载带进度条代码（普通进度条）  
        /// </summary>  
        /// <param name="URL">网址</param>  
        /// <param name="Filename">下载后文件名为</param>  
        /// <param name="Prog">报告进度的处理(第一个参数：总大小，第二个参数：当前进度)</param>  
        /// <returns>True/False是否下载成功</returns>  
        public static bool DownLoadFile(string URL, string Filename, Action<int, int> updateProgress = null)
        {
            Stream st = null;
            Stream so = null;
            HttpWebRequest Myrq = null;
            HttpWebResponse myrp = null;
            bool flag = false;
            try
            {
                Myrq = (HttpWebRequest)WebRequest.Create(URL); //从URL地址得到一个WEB请求     
                myrp = (HttpWebResponse)Myrq.GetResponse(); //从WEB请求得到WEB响应     
                long totalBytes = myrp.ContentLength; //从WEB响应得到总字节数
                                                      //更新进度
                updateProgress?.Invoke((int)totalBytes, 0);//从总字节数得到进度条的最大值  
                st = myrp.GetResponseStream(); //从WEB请求创建流（读）     
                so = new FileStream(Filename, FileMode.Create); //创建文件流（写）     
                long totalDownloadedByte = 0; //下载文件大小     
                byte[] by = new byte[1024];
                int osize = st.Read(by, 0, by.Length); //读流     
                while (osize > 0)
                {
                    totalDownloadedByte = osize + totalDownloadedByte; //更新文件大小     
                    Application.DoEvents();
                    so.Write(by, 0, osize); //写流     
                                            //更新进度
                    updateProgress?.Invoke((int)totalBytes, (int)totalDownloadedByte);//更新进度条 
                    osize = st.Read(by, 0, by.Length); //读流     
                }
                //更新进度
                updateProgress?.Invoke((int)totalBytes, (int)totalBytes);
                flag = true;
            }
            catch (Exception error)
            {
                flag = false;
                MessageBox.Show(error.Message, "Bing Wallpaper 2");
            }
            finally
            {
                if (Myrq != null)
                {
                    Myrq.Abort();//销毁关闭连接
                }
                if (myrp != null)
                {
                    myrp.Close();//销毁关闭响应
                }
                if (so != null)
                {
                    so.Close(); //关闭流 
                }
                if (st != null)
                {
                    st.Close(); //关闭流  
                }
            }
            return flag;
        }
        /// <summary>
        /// 系统参数信息
        /// </summary>
        /// <param name="uAction">20</param>
        /// <param name="uParam">0</param>
        /// <param name="lpvParam">file</param>
        /// <param name="fuWinIni">0x2</param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo")]
        private static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
    }
}