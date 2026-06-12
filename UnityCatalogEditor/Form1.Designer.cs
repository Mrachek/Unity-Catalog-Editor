namespace UnityCatalogEditor;

partial class Form1
{
    private System.ComponentModel.IContainer components = null!;
    private Panel connectionPanel = null!;
    private TableLayoutPanel connectionLayout = null!;
    private Label savedConnectionsLabel = null!;
    private ComboBox savedConnectionsComboBox = null!;
    private Label hostLabel = null!;
    private Label tenantIdLabel = null!;
    private Label clientIdLabel = null!;
    private Label clientSecretLabel = null!;
    private TextBox hostTextBox = null!;
    private TextBox tenantIdTextBox = null!;
    private TextBox clientIdTextBox = null!;
    private TextBox clientSecretTextBox = null!;
    private Button connectButton = null!;
    private SplitContainer mainSplitContainer = null!;
    private SplitContainer rightSplitContainer = null!;
    private TreeView catalogTreeView = null!;
    private ImageList treeImageList = null!;
    private Panel permissionsPanel = null!;
    private TableLayoutPanel permissionsLayout = null!;
    private Label selectedObjectLabel = null!;
    private Label permissionsStatusLabel = null!;
    private FlowLayoutPanel permissionsButtonPanel = null!;
    private Button refreshPermissionsButton = null!;
    private Button addPermissionButton = null!;
    private Button removePermissionButton = null!;
    private TabControl permissionsTabControl = null!;
    private TabPage directPermissionsTabPage = null!;
    private TabPage effectivePermissionsTabPage = null!;
    private ListView directPermissionsListView = null!;
    private ListView effectivePermissionsListView = null!;
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
        savedConnectionsLabel = new Label();
        savedConnectionsComboBox = new ComboBox();
        hostLabel = new Label();
        tenantIdLabel = new Label();
        clientIdLabel = new Label();
        clientSecretLabel = new Label();
        hostTextBox = new TextBox();
        tenantIdTextBox = new TextBox();
        clientIdTextBox = new TextBox();
        clientSecretTextBox = new TextBox();
        connectButton = new Button();
        mainSplitContainer = new SplitContainer();
        rightSplitContainer = new SplitContainer();
        catalogTreeView = new TreeView();
        treeImageList = new ImageList(components);
        permissionsPanel = new Panel();
        permissionsLayout = new TableLayoutPanel();
        selectedObjectLabel = new Label();
        permissionsStatusLabel = new Label();
        permissionsButtonPanel = new FlowLayoutPanel();
        refreshPermissionsButton = new Button();
        addPermissionButton = new Button();
        removePermissionButton = new Button();
        permissionsTabControl = new TabControl();
        directPermissionsTabPage = new TabPage();
        effectivePermissionsTabPage = new TabPage();
        directPermissionsListView = new ListView();
        effectivePermissionsListView = new ListView();
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
        ((System.ComponentModel.ISupportInitialize)rightSplitContainer).BeginInit();
        rightSplitContainer.Panel1.SuspendLayout();
        rightSplitContainer.Panel2.SuspendLayout();
        rightSplitContainer.SuspendLayout();
        permissionsPanel.SuspendLayout();
        permissionsLayout.SuspendLayout();
        permissionsButtonPanel.SuspendLayout();
        permissionsTabControl.SuspendLayout();
        directPermissionsTabPage.SuspendLayout();
        effectivePermissionsTabPage.SuspendLayout();
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

        savedConnectionsLabel.Text = "Saved Connections";
        savedConnectionsLabel.Dock = DockStyle.Fill;
        savedConnectionsLabel.TextAlign = ContentAlignment.MiddleLeft;

        savedConnectionsComboBox.Dock = DockStyle.Fill;
        savedConnectionsComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        savedConnectionsComboBox.FormattingEnabled = true;
        savedConnectionsComboBox.SelectedIndexChanged += SavedConnectionsComboBox_SelectedIndexChanged;

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
        connectButton.MinimumSize = new Size(140, 140);
        connectButton.AutoSize = false;
        connectButton.Click += ConnectButton_Click;

        connectionLayout.Controls.Add(savedConnectionsLabel, 0, 0);
        connectionLayout.Controls.Add(savedConnectionsComboBox, 1, 0);
        connectionLayout.SetColumnSpan(savedConnectionsComboBox, 2);
        connectionLayout.Controls.Add(hostLabel, 0, 1);
        connectionLayout.Controls.Add(hostTextBox, 1, 1);
        connectionLayout.Controls.Add(connectButton, 2, 1);
        connectionLayout.SetRowSpan(connectButton, 4);
        connectionLayout.Controls.Add(tenantIdLabel, 0, 2);
        connectionLayout.Controls.Add(tenantIdTextBox, 1, 2);
        connectionLayout.Controls.Add(clientIdLabel, 0, 3);
        connectionLayout.Controls.Add(clientIdTextBox, 1, 3);
        connectionLayout.Controls.Add(clientSecretLabel, 0, 4);
        connectionLayout.Controls.Add(clientSecretTextBox, 1, 4);

        mainSplitContainer.Dock = DockStyle.Fill;
        mainSplitContainer.Orientation = Orientation.Vertical;
        mainSplitContainer.SplitterDistance = 520;

        mainSplitContainer.Panel1.Controls.Add(catalogTreeView);
        mainSplitContainer.Panel2.Controls.Add(rightSplitContainer);

        catalogTreeView.Dock = DockStyle.Fill;
        catalogTreeView.HideSelection = false;
        catalogTreeView.Name = "catalogTreeView";
        catalogTreeView.FullRowSelect = true;
        catalogTreeView.ImageList = treeImageList;
        catalogTreeView.AfterSelect += CatalogTreeView_AfterSelect;
        catalogTreeView.NodeMouseClick += CatalogTreeView_NodeMouseClick;
        catalogTreeView.ContextMenuStrip = treeContextMenuStrip;

        rightSplitContainer.Dock = DockStyle.Fill;
        rightSplitContainer.Orientation = Orientation.Horizontal;
        rightSplitContainer.SplitterDistance = 540;

        rightSplitContainer.Panel1.Controls.Add(permissionsPanel);
        rightSplitContainer.Panel2.Controls.Add(logTextBox);

        permissionsPanel.Dock = DockStyle.Fill;
        permissionsPanel.Padding = new Padding(12);
        permissionsPanel.Controls.Add(permissionsLayout);

        permissionsLayout.ColumnCount = 1;
        permissionsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        permissionsLayout.RowCount = 4;
        permissionsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        permissionsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        permissionsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
        permissionsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        permissionsLayout.Dock = DockStyle.Fill;
        permissionsLayout.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;

        selectedObjectLabel.Text = "Selected: none";
        selectedObjectLabel.Dock = DockStyle.Fill;
        selectedObjectLabel.TextAlign = ContentAlignment.MiddleLeft;
        selectedObjectLabel.Font = new Font(Font, FontStyle.Bold);

        permissionsStatusLabel.Text = "Select a catalog or schema to view permissions.";
        permissionsStatusLabel.Dock = DockStyle.Fill;
        permissionsStatusLabel.TextAlign = ContentAlignment.MiddleLeft;

        permissionsButtonPanel.Dock = DockStyle.Fill;
        permissionsButtonPanel.FlowDirection = FlowDirection.LeftToRight;
        permissionsButtonPanel.WrapContents = false;
        permissionsButtonPanel.Padding = new Padding(0);
        permissionsButtonPanel.Margin = new Padding(0);

        refreshPermissionsButton.Text = "Refresh";
        refreshPermissionsButton.Width = 90;
        refreshPermissionsButton.Click += RefreshPermissionsButton_Click;

        addPermissionButton.Text = "Add Permission";
        addPermissionButton.Width = 120;
        addPermissionButton.Click += AddPermissionButton_Click;

        removePermissionButton.Text = "Remove Permission";
        removePermissionButton.Width = 130;
        removePermissionButton.Click += RemovePermissionButton_Click;

        permissionsButtonPanel.Controls.Add(refreshPermissionsButton);
        permissionsButtonPanel.Controls.Add(addPermissionButton);
        permissionsButtonPanel.Controls.Add(removePermissionButton);

        permissionsTabControl.Dock = DockStyle.Fill;
        permissionsTabControl.Controls.Add(directPermissionsTabPage);
        permissionsTabControl.Controls.Add(effectivePermissionsTabPage);

        directPermissionsTabPage.Text = "Direct Permissions";
        directPermissionsTabPage.Padding = new Padding(3);
        directPermissionsTabPage.Controls.Add(directPermissionsListView);

        effectivePermissionsTabPage.Text = "Effective Permissions";
        effectivePermissionsTabPage.Padding = new Padding(3);
        effectivePermissionsTabPage.Controls.Add(effectivePermissionsListView);

        directPermissionsListView.Dock = DockStyle.Fill;
        directPermissionsListView.View = View.Details;
        directPermissionsListView.FullRowSelect = true;
        directPermissionsListView.HideSelection = false;
        directPermissionsListView.MultiSelect = false;
        directPermissionsListView.Columns.Add("Principal", 220);
        directPermissionsListView.Columns.Add("Privileges", 700);
        directPermissionsListView.ItemSelectionChanged += DirectPermissionsListView_ItemSelectionChanged;

        effectivePermissionsListView.Dock = DockStyle.Fill;
        effectivePermissionsListView.View = View.Details;
        effectivePermissionsListView.FullRowSelect = true;
        effectivePermissionsListView.HideSelection = false;
        effectivePermissionsListView.MultiSelect = false;
        effectivePermissionsListView.Columns.Add("Principal", 220);
        effectivePermissionsListView.Columns.Add("Privileges", 700);

        permissionsLayout.Controls.Add(selectedObjectLabel, 0, 0);
        permissionsLayout.Controls.Add(permissionsStatusLabel, 0, 1);
        permissionsLayout.Controls.Add(permissionsButtonPanel, 0, 2);
        permissionsLayout.Controls.Add(permissionsTabControl, 0, 3);

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
        rightSplitContainer.Panel1.ResumeLayout(false);
        rightSplitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)rightSplitContainer).EndInit();
        rightSplitContainer.ResumeLayout(false);
        permissionsPanel.ResumeLayout(false);
        permissionsLayout.ResumeLayout(false);
        permissionsButtonPanel.ResumeLayout(false);
        permissionsTabControl.ResumeLayout(false);
        directPermissionsTabPage.ResumeLayout(false);
        effectivePermissionsTabPage.ResumeLayout(false);
        treeContextMenuStrip.ResumeLayout(false);
        ResumeLayout(false);
    }
}
