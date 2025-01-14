namespace Hawaso.Models
{
    public class BackgroundCheckOptionsDialogViewModel
    {
        public string Controller { get; set; }

        public string Action { get; set; }

        public string BGCheckID { get; set; }

        public BackgroundCheckOptionsDialogViewModel(string controller, string action, string bgCheckID)
        {
            Controller = controller;
            Action = action;
            BGCheckID = bgCheckID;
        }
    }

}
