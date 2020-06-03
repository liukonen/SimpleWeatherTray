using System;
namespace WeatherDesktop.Share
{
    public class MessageBox
    {

        private static MessageBox box;

        public string msg;


        protected MessageBox()
        { }

        public static MessageBox Invoke()
        {
            if (box == null) { box = new MessageBox(); } return box;
        }

        public void Show(string msg)
        {
            Show(msg, "");
        }
        public void Show(string msg, string title)
        {
            msgBoxEventArg eventArg = new msgBoxEventArg();
            eventArg.Message = msg;
            eventArg.Title = title;
            OnShow(eventArg);
        }


        public virtual void OnShow(msgBoxEventArg e)
        {
            EventHandler handler = msgShow;
            if (handler != null) { handler(this, e); }
        }

        public event EventHandler msgShow;
    }

    public class msgBoxEventArg : EventArgs
    {
        public string Message;
        public string Title;
    }
}




