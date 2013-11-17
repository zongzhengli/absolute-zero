using System;
using System.Windows.Forms;

namespace AbsoluteZero {

    /// <summary>
    /// Represents a selection dialog box. 
    /// </summary>
    partial class SelectionBox : Form {

        /// <summary>
        /// Construct a SelectionBox.
        /// </summary>
        private SelectionBox() {
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
        /// Displays the SelectionBox and returns the corresponding result. 
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="options">The options to choose from.</param>
        /// <returns>The result of displaying the InputBox.</returns>
        private DialogResult ShowDialog(String message, params String[] options) {
            promptLabel.Text = message;
            responseBox.Items.AddRange(options);
            responseBox.SelectedIndex = 0;
            CenterToScreen();
            return ShowDialog();
        }

        /// <summary>
        /// Displays a SelectionBox and returns the option chosen by the user.  
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="options">The default input.</param>
        /// <returns>The option choosen by the user.</returns>
        public static String Show(String message, params String[] options) {
            while (true)
                using (SelectionBox a = new SelectionBox()) {
                    if (a.ShowDialog(message, options) == DialogResult.OK)
                        return a.responseBox.Text;
                }
        }
    }
}