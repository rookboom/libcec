using System;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;
using System.IO;
using System.Windows.Forms;
using System.Reflection;

namespace LibCecService
{
    partial class LibCecService : ServiceBase
    {
        #region Win32 Stuff

        private const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;

        /// <summary>
        /// A power setting change event has been received.
        /// </summary>
        private const int PBT_POWERSETTINGCHANGE = 0x8013;

        /// <summary>
        /// The GUID that identifies console display state changes
        /// </summary>
        private static Guid GUID_CONSOLE_DISPLAY_STATE = new Guid("6fe69556-704a-47a0-8f24-c28d936fda47");

        [DllImport(@"User32", SetLastError = true)]
        private static extern IntPtr RegisterPowerSettingNotification(IntPtr hRecipient, ref Guid PowerSettingGuid, Int32 Flags);

#endregion Win32 Stuff

        /// <summary>
        /// The message pump instance
        /// </summary>
        private MessagePump messagePump;

        /// <summary>
        /// The main thread that hosts the message pump
        /// </summary>
        private Thread messagePumpThread;

        public LibCecService()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Allows the service to be run either as a console app or as a service
        /// </summary>
        /// <param name="args">the params passed to the program</param>
        public static void Main(string[] args)
        {
            using (LibCecService service = new LibCecService())
            {
                if (Environment.UserInteractive)
                {
                    service.OnStart(args);

                    Console.WriteLine("press any key to quit");
                    Console.ReadLine();

                    service.OnStop();
                }
                else
                {
                    ServiceBase.Run(service);
                }
            }
        }

        /// <summary>
        /// Called when the service starts
        /// </summary>
        /// <param name="args">The command line arguments</param>
        protected override void OnStart(string[] args)
        {
            // wire up unhandled exception handler
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // setup and start the message pump thread
            messagePumpThread = new Thread(new ThreadStart(() => {

                // create the message pump
                messagePump = new MessagePump();

                // listen for power broadcast messages
                messagePump.PowerBroadcastMessageReceived += MessagePump_PowerBroadcastMessageReceived;

                // register with the OS that we want to receive power broadcast changes for monitor power display changes
                IntPtr hPower = RegisterPowerSettingNotification(messagePump.Handle, ref GUID_CONSOLE_DISPLAY_STATE, DEVICE_NOTIFY_WINDOW_HANDLE);

                if (hPower == IntPtr.Zero)
                    Log("Error registering for power settings messages: " + Marshal.GetLastWin32Error());
                else
                    Log("Successfuly registered for power settings messages!");

                // NOTE: we have to call Application.Run() on the same thread where you create the form so 
                // that's why we do all of this in the same thread
                Application.Run();
            }));
            messagePumpThread.Start();            
        }

        /// <summary>
        /// Log unhandled exceptions
        /// </summary>
        /// <param name="sender">The invoking object</param>
        /// <param name="e">The event parameters</param>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Log(e.ExceptionObject.ToString());
        }

        /// <summary>
        /// Called when the service is stopped
        /// </summary>
        protected override void OnStop()
        {
            Log("STOP message received. Service shutting down!");

            // stop the message pump
            Application.Exit();
        }

        /// <summary>
        /// Called whenevera WM_POWERBROADCAST message is received by this application. 
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The message details</param>
        private void MessagePump_PowerBroadcastMessageReceived(object sender, System.Windows.Forms.Message e)
        {

            // we only care about power change notifications so just react to those
            if ((int)e.WParam == PBT_POWERSETTINGCHANGE)
            {
                // unpack the details of the message and see if'ts a monitor power change
                POWERBROADCAST_SETTING pbs = (POWERBROADCAST_SETTING)Marshal.PtrToStructure(e.LParam, typeof(POWERBROADCAST_SETTING));

                if (pbs.PowerSetting == GUID_CONSOLE_DISPLAY_STATE)
                {
                    PowerState ps = (PowerState)pbs.Data;
                    Log("MONITOR POWER STATE: " + ps);

                    using (var p = new CecSharpClient.CecSharpClient())
                    {
                        if (p.Connect(10000))
                        {
                            Log("********** Connection to CEC adapter opened");
                            bool res;
                            if (ps == PowerState.On)
                            {
                                // Sleep a while since sometimes power-on does not work if being send immediately after connecting.
                                System.Threading.Thread.Sleep(500);
                                res = p.PowerOn();
                                Log("********** Power On: " + res.ToString());
                                System.Threading.Thread.Sleep(5000);
                            }
                            else // off or dimmed
                            {
                                res = p.Standby();
                                Log("********** Standby: " + res.ToString());
                                System.Threading.Thread.Sleep(1000);
                            }
                            p.Close();
                            Log("********** Connection closed");
                        }
                        else
                            Log("********** Could not open a connection to the CEC adapter");
                    }
                }
            }
        }

        /// <summary>
        /// Logs either to the console or a log file depending on if we are running as a service or as a console app
        /// </summary>
        /// <param name="message">The message to emit</param>
        private void Log(string message, bool includeTimestamp = true)
        {
            if (includeTimestamp)
                message = DateTime.Now.ToString() + "\t" + message;

            if (Environment.UserInteractive)
            {
                Console.WriteLine(message);
            }
            else
            {
                this.EventLog.WriteEntry(message);
            }
        }
    }
}
