namespace UnityCatalogEditor;

partial class Form1
{
    private System.ComponentModel.IContainer components = null!;
    private Panel connectionPanel = null!;
    private TableLayoutPanel connectionLayout = null!;
    private Label hostLabel = null!;
    private Label savedConnectionLabel = null!;
    private Label tenantIdLabel = null!;
    private Label clientIdLabel = null!;
    private Label clientSecretLabel = null!;
    private ComboBox savedConnectionsComboBox = null!;
    private TextBox hostTextBox = null!;
    private TextBox tenantIdTextBox = null!;
    private TextBox clientIdTextBox = null!;
    private TextBox clientSecretTextBox = null!;
    private Button connectButton = null!;
    private SplitContainer mainSplitContainer = null!;
    private TreeView catalogTreeView = null!;
    private RichTextBox logTextBox = null!;
    private ContextMenuStrip treeContextMenuStrip = null!;
    private ToolStripMenuItem addSchemaToolStripMenuItem = null!;
    private ToolStripMenuItem deleteToolStripMenuItem = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        connectionPanel = new Panel();
        connectionLayout = new TableLayoutPanel();
        savedConnectionLabel = new Label();
        hostLabel = new Label();
        tenantIdLabel = new Label();
        clientIdLabel = new Label();
        clientSecretLabel = new Label();
        savedConnectionsComboBox = new ComboBox();
        hostTextBox = new TextBox();
        tenantIdTextBox = new TextBox();
        clientIdTextBox = new TextBox();
        clientSecretTextBox = new TextBox();
        connectButton = new Button();
        mainSplitContainer = new SplitContainer();
        catalogTreeView = new TreeView();
        logTextBox = new RichTextBox();
        treeContextMenuStrip = new ContextMenuStrip(components);
        addSchemaToolStripMenuItem = new ToolStripMenuItem();
        deleteToolStripMenuItem = new ToolStripMenuItem();
        connectionPanel.SuspendLayout();
        connectionLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)mainSplitContainer).BeginInit();
        mainSplitContainer.Panel1.SuspendLayout();
        mainSplitContainer.Panel2.SuspendLayout();
        mainSplitContainer.SuspendLayout();
        treeContextMenuStrip.SuspendLayout();
        SuspendLayout();

        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1400, 900);
        Text = "Unity Catalog Editor";
        StartPosition = FormStartPosition.CenterScreen;

        connectionPanel.Dock = DockStyle.Top;
        connectionPanel.Height = 190;
        connectionPanel.Padding = new Padding(12);
        connectionPanel.Controls.Add(connectionLayout);

        connectionLayout.ColumnCount = 3;
        connectionLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130F));
        connectionLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        connectionLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
        connectionLayout.RowCount = 5;
        connectionLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        connectionLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        connectionLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        connectionLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        connectionLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        connectionLayout.Dock = DockStyle.Fill;
        connectionLayout.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
        connectionLayout.Padding = new Padding(0);

        savedConnectionLabel.Text = "Saved Connection";
        savedConnectionLabel.Dock = DockStyle.Fill;
        savedConnectionLabel.TextAlign = ContentAlignment.MiddleLeft;

        hostLabel.Text = "Databricks Host";
        hostLabel.Dock = DockStyle.Fill;
        hostLabel.TextAlign = ContentAlignment.MiddleLeft;

        tenantIdLabel.Text = "Azure Tenant ID";
        tenantIdLabel.Dock = DockStyle.Fill;
        tenantIdLabel.TextAlign = ContentAlignment.MiddleLeft;

        clientIdLabel.Text = "Client ID";
        clientIdLabel.Dock = DockStyle.Fill;
        clientIdLabel.TextAlign = ContentAlignment.MiddleLeft;

        clientSecretLabel.Text = "Client Secret";
        clientSecretLabel.Dock = DockStyle.Fill;
        clientSecretLabel.TextAlign = ContentAlignment.MiddleLeft;

        savedConnectionsComboBox.Dock = DockStyle.Fill;
        savedConnectionsComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        savedConnectionsComboBox.FormattingEnabled = true;
        savedConnectionsComboBox.Name = "savedConnectionsComboBox";
        savedConnectionsComboBox.SelectedIndexChanged += SavedConnectionsComboBox_SelectedIndexChanged;

        hostTextBox.Dock = DockStyle.Fill;
        hostTextBox.Name = "hostTextBox";
        hostTextBox.PlaceholderText = "https://adb-0000000000000000.0.azuredatabricks.net";

        tenantIdTextBox.Dock = DockStyle.Fill;
        tenantIdTextBox.Name = "tenantIdTextBox";

        clientIdTextBox.Dock = DockStyle.Fill;
        clientIdTextBox.Name = "clientIdTextBox";

        clientSecretTextBox.Dock = DockStyle.Fill;
        clientSecretTextBox.Name = "clientSecretTextBox";
        clientSecretTextBox.UseSystemPasswordChar = true;

        connectButton.Text = "Connect";
        connectButton.Dock = DockStyle.Fill;
        connectButton.Height = 120;
        connectButton.Click += ConnectButton_Click;

        connectionLayout.Controls.Add(savedConnectionLabel, 0, 0);
        connectionLayout.Controls.Add(savedConnectionsComboBox, 1, 0);
        connectionLayout.Controls.Add(hostLabel, 0, 1);
        connectionLayout.Controls.Add(hostTextBox, 1, 1);
        connectionLayout.Controls.Add(connectButton, 2, 0);
        connectionLayout.SetRowSpan(connectButton, 5);
        connectionLayout.Controls.Add(tenantIdLabel, 0, 2);
        connectionLayout.Controls.Add(tenantIdTextBox, 1, 2);
        connectionLayout.Controls.Add(clientIdLabel, 0, 3);
        connectionLayout.Controls.Add(clientIdTextBox, 1, 3);
        connectionLayout.Controls.Add(clientSecretLabel, 0, 4);
        connectionLayout.Controls.Add(clientSecretTextBox, 1, 4);

        mainSplitContainer.Dock = DockStyle.Fill;
        mainSplitContainer.Orientation = Orientation.Vertical;
        mainSplitContainer.SplitterDistance = 760;

        mainSplitContainer.Panel1.Controls.Add(catalogTreeView);
        mainSplitContainer.Panel2.Controls.Add(logTextBox);

        catalogTreeView.Dock = DockStyle.Fill;
        catalogTreeView.HideSelection = false;
        catalogTreeView.Name = "catalogTreeView";
        catalogTreeView.FullRowSelect = true;
        catalogTreeView.NodeMouseClick += CatalogTreeView_NodeMouseClick;
        catalogTreeView.ContextMenuStrip = treeContextMenuStrip;

        logTextBox.Dock = DockStyle.Fill;
        logTextBox.ReadOnly = true;
        logTextBox.BackColor = SystemColors.Window;
        logTextBox.Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point);

        addSchemaToolStripMenuItem.Text = "Add Schema";
        addSchemaToolStripMenuItem.Click += AddSchemaToolStripMenuItem_Click;

        deleteToolStripMenuItem.Text = "Delete";
        deleteToolStripMenuItem.Click += DeleteToolStripMenuItem_Click;

        treeContextMenuStrip.Items.AddRange(new ToolStripItem[]
        {
            addSchemaToolStripMenuItem,
            deleteToolStripMenuItem
        });
        treeContextMenuStrip.Opening += TreeContextMenuStrip_Opening;

        Controls.Add(mainSplitContainer);
        Controls.Add(connectionPanel);

        connectionPanel.ResumeLayout(false);
        connectionLayout.ResumeLayout(false);
        connectionLayout.PerformLayout();
        mainSplitContainer.Panel1.ResumeLayout(false);
        mainSplitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)mainSplitContainer).EndInit();
        mainSplitContainer.ResumeLayout(false);
        treeContextMenuStrip.ResumeLayout(false);
        ResumeLayout(false);
    }
}
