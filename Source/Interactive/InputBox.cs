using System;
using System.Windows.Forms;

namespace AbsoluteZero {
    partial class InputBox : Form {
        public InputBox() {
            InitializeComponent();
            Button b = new Button();
            b.Click += delegate { Close(); };
            CancelButton = b;
        }

        private void button_Click(Object sender, EventArgs e) {
            DialogResult = DialogResult.OK;
            Close();
        }

        public DialogResult ShowDialog(String message, String defaultValue) {
            promptLabel.Text = message;
            responseBox.Text = defaultValue;
            CenterToScreen();
            return ShowDialog();
        }

        public static String Show(String message, String defaultValue = "") {
            using (InputBox a = new InputBox()) {
                if (a.ShowDialog(message, defaultValue) == DialogResult.OK)
                    return a.responseBox.Text;
                return defaultValue;
            }
        }
    }
}