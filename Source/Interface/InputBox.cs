using System;
using System.Windows.Forms;

namespace AbsoluteZero {

    /// <summary>
    /// Represents an input dialog box. 
    /// </summary>
    partial class InputBox : Form {

        /// <summary>
        /// Constructs an InputBox. 
        /// </summary>
        private InputBox() {
            InitializeComponent();
            Button b = new Button();
            b.Click += (sender, e) => { 
                Close();
            };
            CancelButton = b;
        }

        /// <summary>
        /// Handles the OK button click.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The raised event.</param>
        private void OKClick(Object sender, EventArgs e) {
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// Displays the InputBox and returns the corresponding result. 
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="defaultInput">The default input.</param>
        /// <returns>The result of displaying the InputBox.</returns>
        private DialogResult ShowDialog(String message, String defaultInput) {
            promptLabel.Text = message;
            responseBox.Text = defaultInput;
            CenterToScreen();
            return ShowDialog();
        }

        /// <summary>
        /// Displays an InputBox and returns, if successful, the input given by the 
        /// user. If unsuccessful, the given default input is returned. 
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="defaultInput">The default input.</param>
        /// <returns>The input given by the user.</returns>
        public static String Show(String message, String defaultInput = "") {
            using (InputBox a = new InputBox()) {
                if (a.ShowDialog(message, defaultInput) == DialogResult.OK)
                    return a.responseBox.Text;
                return defaultInput;
            }
        }
    }
}