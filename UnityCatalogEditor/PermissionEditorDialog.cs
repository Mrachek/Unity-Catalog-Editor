namespace UnityCatalogEditor;

internal sealed class PermissionEditorDialog : Form
{
    private readonly TextBox principalTextBox;
    private readonly CheckedListBox privilegesCheckedListBox;

    public PermissionEditResult? Result { get; private set; }

    private PermissionEditorDialog(
        string title,
        string prompt,
        IReadOnlyList<string> allowedPrivileges,
        string principal,
        IReadOnlyCollection<string> selectedPrivileges)
    {
        Text = title;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;
        MaximizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(560, 420);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(12)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));

        var promptLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = prompt,
            AutoEllipsis = true
        };

        var principalLabel = new Label
        {
            Dock = DockStyle.Fill,
            Text = "Principal",
            TextAlign = ContentAlignment.MiddleLeft
        };

        principalTextBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Text = principal
        };

        var principalPanel = new Panel
        {
            Dock = DockStyle.Fill
        };
        principalPanel.Controls.Add(principalTextBox);

        privilegesCheckedListBox = new CheckedListBox
        {
            Dock = DockStyle.Fill,
            CheckOnClick = true,
            IntegralHeight = false
        };

        foreach (var privilege in allowedPrivileges)
        {
            var index = privilegesCheckedListBox.Items.Add(privilege);
            if (selectedPrivileges.Contains(privilege, StringComparer.OrdinalIgnoreCase))
            {
                privilegesCheckedListBox.SetItemChecked(index, true);
            }
        }

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0),
            Margin = new Padding(0)
        };

        var okButton = new Button
        {
            Text = "OK",
            Width = 90
        };
        okButton.Click += OkButton_Click;

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
        layout.Controls.Add(principalLabel, 0, 1);
        layout.Controls.Add(principalPanel, 0, 2);
        layout.Controls.Add(privilegesCheckedListBox, 0, 3);
        layout.Controls.Add(buttonPanel, 0, 4);

        Controls.Add(layout);
    }

    public static PermissionEditorDialog Create(
        IWin32Window owner,
        string title,
        string prompt,
        IReadOnlyList<string> allowedPrivileges,
        string principal,
        IReadOnlyCollection<string>? selectedPrivileges = null)
    {
        return new PermissionEditorDialog(title, prompt, allowedPrivileges, principal, selectedPrivileges ?? Array.Empty<string>());
    }

    private void OkButton_Click(object? sender, EventArgs e)
    {
        var principal = principalTextBox.Text.Trim();
        var privileges = privilegesCheckedListBox.CheckedItems
            .OfType<string>()
            .Select(privilege => privilege.Trim())
            .Where(privilege => !string.IsNullOrWhiteSpace(privilege))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (string.IsNullOrWhiteSpace(principal))
        {
            MessageBox.Show(this, "Principal is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (privileges.Length == 0)
        {
            MessageBox.Show(this, "Select at least one privilege.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        Result = new PermissionEditResult(principal, privileges);
        DialogResult = DialogResult.OK;
        Close();
    }
}
