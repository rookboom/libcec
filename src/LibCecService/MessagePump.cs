using System;
using System.Windows.Forms;

namespace LibCecService
{
    /// <summary>
    /// A class that creates a message pump and listens for power broadcast messages.
    /// </summary>
    /// By referensing System.Windows.Forms and inheriting from NativeWindow we create a hidden window and a message pump that allows us to receive OS broadcast messages.  
    /// Without the message pump we wouldn't receive and could not process these messages.
    /// </remarks> 
    class MessagePump : NativeWindow
    {
        private const int WM_POWERBROADCAST = 0x218;

        /// <summary>
        /// Event handler declaration for when power broadcast messages are received
        /// </summary>
        public event EventHandler<Message> PowerBroadcastMessageReceived;

        /// <summary>
        /// Create the window handle when the class is created
        /// </summary>
        public MessagePump()
        {
            CreateHandle(new CreateParams());
        }

        /// <summary>
        /// WndProc method is called whenever messages are received by the message pump
        /// </summary>
        /// <param name="msg">The message received</param>
        protected override void WndProc(ref Message msg)
        {            
            if ((int)msg.Msg == WM_POWERBROADCAST)
            {
                // raise the event if the mesages is WM_POWERBRAODCAST
                if (this.PowerBroadcastMessageReceived != null)
                    this.PowerBroadcastMessageReceived(this, msg);
            }

            base.WndProc(ref msg);
        }
    }
}
