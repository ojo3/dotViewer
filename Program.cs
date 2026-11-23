using ojo3Utility.Common;
using ojo3Utility.Common.IPC;
using ojo3Utility.Controls.Forms;
using ojo3Utility.Controls.Interface;
using System;
using System.Windows.Forms;

namespace dotViewer
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            var name = PathInfo.GetUID();
            var ipc = new IpcServiceHost<IOjo3Container>(name);

            try
            {
                // 起動済み
                if (!ipc.Open())
                {
                    var remoteObject = ipc.GetChannel<MultiWindow>();

                    if (remoteObject == null)
                    {
                        Ojo3MessageBox.Show($"{name} に接続出来ません.");
                        return;
                    }

                    if (args.Length > 0)
                    {
                        remoteObject.FileReceive(args);
                    }
                    else
                    {
                        remoteObject.FormFocus();
                    }

                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                var mainForm = new MultiWindow(string.Empty, System.Drawing.Point.Empty);

                ipc.SetChannel(mainForm);

                if (args.Length > 0)
                {
                    mainForm.FileReceive(args);
                }

                // メインフォームを起動
                Application.Run(mainForm);
            }
            catch (ApplicationException ex)
            {
                Console.WriteLine(ex);
                Ojo3Debug.Dump(ex, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Ojo3Debug.Dump(ex, false);
            }
            finally
            {
                ipc.Close();
            }
        }
    }
}
