using System;
namespace WeatherDesktop.Share
{
    public class Input
    {

        private static Input input;

        protected Input() { }

        public static Input Invoke() { if (input == null) { input = new Input(); } return input; }


        public string msg;

        public string Show(string msg, string title)
        {
            return Show(msg, title, string.Empty);
        }

        public string Show(string msg, string title, string defaultValue)
        {
            var E = new InputArgs()
            {
                Message = msg,
                Title = title,
                DefaultText = defaultValue
            };
            OnShow(E);
            return E.Response;
        }



        public virtual void OnShow(EventArgs e)
        {
            EventHandler handler = msgShow;
            if (handler != null) { handler(this, e); }
        }

        public event EventHandler msgShow;
    }


    public class InputArgs: EventArgs
        {
        public string Message;
        public string Title;
        public string DefaultText;
        public string Response;
}
}
