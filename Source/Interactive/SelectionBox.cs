using System;
using System.Windows.Forms;

namespace AbsoluteZero {
    partial class SelectionBox : Form {
        public SelectionBox() {
            InitializeComponent();
            Button b = new Button();
            b.Click += delegate { Close(); };
            CancelButton = b;
        }

        private void button_Click(Object sender, EventArgs e) {
            DialogResult = DialogResult.OK;
            Close();
        }

        public DialogResult ShowDialog(String message, params String[] choices) {
            promptLabel.Text = message;
            responseBox.Items.AddRange(choices);
            responseBox.SelectedIndex = 0;
            CenterToScreen();
            return ShowDialog();
        }

        public static String Show(String message, params String[] choices) {
            while (true)
                using (SelectionBox a = new SelectionBox()) {
                    if (a.ShowDialog(message, choices) == DialogResult.OK)
                        return a.responseBox.Text;
                }
        }
    }
}