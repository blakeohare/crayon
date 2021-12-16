namespace U3
{
    public class U3Window
    {
        private static int idAlloc = 1;
        internal int ID { get; set; }

        public U3Window() {
          this.ID = idAlloc++;
        }

        public void Show(string title, int width, int height) {

            string html = string.Join('\n', new string[] {
                "<!DOCTYPE html>",
                "<html>", "<body>",
                "U3 code goes here",
                "</body>",
                "</html>",
            });
            byte[] htmlUtf8 = System.Text.Encoding.UTF8.GetBytes(html);
            string dataUri = "data:text/html;base64," + System.Convert.ToBase64String(htmlUtf8);
            show_window(title, width, height, dataUri);
        }

        [System.Runtime.InteropServices.DllImport("u3native.dylib")]
        internal static extern show_window(string title, int width, int height);
    }
}
