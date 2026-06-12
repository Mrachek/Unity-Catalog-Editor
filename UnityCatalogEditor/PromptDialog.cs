namespace UnityCatalogEditor;

internal sealed class PromptDialog : Form
{
    private readonly TextBox inputTextBox;

    private PromptDialog(string title, string prompt, string? defaultText)
    {
        Text = title;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(480, 150);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(12)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        var promptLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = prompt,
            AutoEllipsis = true
        };

        inputTextBox = new TextBox
        {
            Dock = DockStyle.Top,
            Margin = new Padding(0),
            Width = 440,
            Text = defaultText ?? string.Empty
        };

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            Height = 44,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0),
            Margin = new Padding(0)
        };

        var okButton = new Button
        {
            Text = "OK",
            DialogResult = DialogResult.OK,
            Width = 90
        };

        var cancelButton = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Width = 90
        };

        buttonPanel.Controls.Add(okButton);
        buttonPanel.Controls.Add(cancelButton);

        AcceptButton = okButton;
        CancelButton = cancelButton;

        layout.Controls.Add(promptLabel, 0, 0);
        layout.Controls.Add(inputTextBox, 0, 1);
        layout.Controls.Add(buttonPanel, 0, 2);

        Controls.Add(layout);
    }

    public static string? ShowDialog(IWin32Window owner, string title, string prompt, string? defaultText = null)
    {
        using var dialog = new PromptDialog(title, prompt, defaultText);
        return dialog.ShowDialog(owner) == DialogResult.OK ? dialog.inputTextBox.Text : null;
    }
}
