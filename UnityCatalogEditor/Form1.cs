using Databricks.Sdk;
using System.ComponentModel;

namespace UnityCatalogEditor;

public partial class Form1 : Form
{
    private const string CatalogIconKey = "catalog";
    private const string SchemaIconKey = "schema";
    private const string TableIconKey = "table";
    private const string VolumeIconKey = "volume";

    private static readonly string[] CatalogPrivileges =
    [
        "ALL PRIVILEGES",
        "APPLY TAG",
        "BROWSE",
        "CREATE FUNCTION",
        "CREATE MATERIALIZED VIEW",
        "CREATE MODEL",
        "CREATE SCHEMA",
        "CREATE TABLE",
        "CREATE VOLUME",
        "EXECUTE",
        "MANAGE",
        "MODIFY",
        "READ VOLUME",
        "REFRESH",
        "SELECT",
        "USE CATALOG",
        "USE SCHEMA",
        "WRITE VOLUME"
    ];

    private static readonly string[] SchemaPrivileges =
    [
        "ALL PRIVILEGES",
        "APPLY TAG",
        "CREATE FUNCTION",
        "CREATE MATERIALIZED VIEW",
        "CREATE MODEL",
        "CREATE TABLE",
        "CREATE VOLUME",
        "EXTERNAL USE SCHEMA",
        "EXECUTE",
        "MANAGE",
        "MODIFY",
        "READ VOLUME",
        "REFRESH",
        "SELECT",
        "USE SCHEMA",
        "WRITE VOLUME"
    ];

    private WorkspaceClient? workspaceClient;
    private readonly ConnectionStore connectionStore = new();
    private bool isLoading;
    private long permissionsLoadVersion;
    private PermissionTarget? currentPermissionTarget;

    public Form1()
    {
        InitializeComponent();
        InitializeTreeIcons();
        RefreshSavedConnections();
    }

    private async void ConnectButton_Click(object sender, EventArgs e)
    {
        if (isLoading)
        {
            return;
        }

        if (workspaceClient is not null)
        {
            Disconnect();
            return;
        }

        var host = hostTextBox.Text.Trim();
        var tenantId = tenantIdTextBox.Text.Trim();
        var clientId = clientIdTextBox.Text.Trim();
        var clientSecret = clientSecretTextBox.Text;

        if (string.IsNullOrWhiteSpace(host) ||
            string.IsNullOrWhiteSpace(tenantId) ||
            string.IsNullOrWhiteSpace(clientId) ||
            string.IsNullOrWhiteSpace(clientSecret))
        {
            MessageBox.Show("All connection fields are required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        SetLoadingState(true);
        AppendLog("Connecting to Databricks...");

        var config = new DatabricksConfig
        {
            Host = host,
            AzureTenantId = tenantId,
            ClientId = clientId,
            ClientSecret = clientSecret
        };

        WorkspaceClient? candidateClient = null;
        try
        {
            candidateClient = new WorkspaceClient(config);
            _ = await candidateClient.Catalogs.ListAsync();

            workspaceClient?.Dispose();
            workspaceClient = candidateClient;
            candidateClient = null;

            AppendLog("Connection established.");
            await ReloadTreeAsync();
            PromptToSaveConnection(config);
        }
        catch (Exception ex)
        {
            candidateClient?.Dispose();
            ShowDatabricksError(ex);
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private void Disconnect()
    {
        if (workspaceClient is null)
        {
            return;
        }

        SetLoadingState(true);
        try
        {
            workspaceClient.Dispose();
            workspaceClient = null;
            catalogTreeView.Nodes.Clear();
            catalogTreeView.SelectedNode = null;
            ClearPermissionState();
            AppendLog("Disconnected from Databricks.");
            RefreshSavedConnections();
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private async Task ReloadTreeAsync()
    {
        if (workspaceClient is null)
        {
            return;
        }

        catalogTreeView.BeginUpdate();
        try
        {
            catalogTreeView.Nodes.Clear();

            var catalogs = await workspaceClient.Catalogs.ListAsync();
            foreach (var catalog in catalogs.OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase))
            {
                var catalogNode = CreateCatalogNode(catalog.Name);
                catalogTreeView.Nodes.Add(catalogNode);
                await LoadSchemasAsync(catalogNode, catalog.Name);
            }

            catalogTreeView.ExpandAll();
            catalogTreeView.SelectedNode = null;
            ClearPermissionState();
            AppendLog($"Loaded {catalogs.Count} catalog(s).");
        }
        catch (Exception ex)
        {
            ShowDatabricksError(ex);
        }
        finally
        {
            catalogTreeView.EndUpdate();
        }
    }

    private async Task LoadSchemasAsync(TreeNode catalogNode, string catalogName)
    {
        if (workspaceClient is null)
        {
            return;
        }

        try
        {
            var schemas = await workspaceClient.Schemas.ListAsync(catalogName);
            foreach (var schema in schemas
                         .Where(schema => !string.Equals(schema.Name, "information_schema", StringComparison.OrdinalIgnoreCase) &&
                                          !string.Equals(schema.Name, "system", StringComparison.OrdinalIgnoreCase))
                         .OrderBy(schema => schema.Name, StringComparer.OrdinalIgnoreCase))
            {
                var schemaNode = CreateSchemaNode(catalogName, schema.Name);
                catalogNode.Nodes.Add(schemaNode);
                await LoadObjectsAsync(schemaNode, catalogName, schema.Name);
            }
        }
        catch (Exception ex)
        {
            if (IsAccessDenied(ex))
            {
                AppendLog($"Skipping schema discovery for {catalogName}: {ex.Message}");
                return;
            }

            ShowDatabricksError(ex);
        }
    }

    private async Task LoadObjectsAsync(TreeNode schemaNode, string catalogName, string schemaName)
    {
        if (workspaceClient is null)
        {
            return;
        }

        try
        {
            var tables = await workspaceClient.Tables.ListAsync(catalogName, schemaName);
            foreach (var table in tables.OrderBy(table => table.Name, StringComparer.OrdinalIgnoreCase))
            {
                schemaNode.Nodes.Add(CreateTableNode(
                    catalogName,
                    schemaName,
                    table.Name ?? table.FullName ?? string.Empty,
                    table.TableType ?? "Unknown"));
            }

            var volumes = await workspaceClient.Volumes.ListAsync(catalogName, schemaName);
            foreach (var volume in volumes.OrderBy(volume => volume.Name, StringComparer.OrdinalIgnoreCase))
            {
                schemaNode.Nodes.Add(CreateVolumeNode(
                    catalogName,
                    schemaName,
                    volume.Name ?? volume.FullName ?? string.Empty,
                    volume.VolumeType ?? "Unknown"));
            }
        }
        catch (Exception ex)
        {
            if (IsAccessDenied(ex))
            {
                AppendLog($"Skipping object discovery for {catalogName}.{schemaName}: {ex.Message}");
                return;
            }

            ShowDatabricksError(ex);
        }
    }

    private async void AddSchemaToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (workspaceClient is null || catalogTreeView.SelectedNode?.Tag is not CatalogNodeTag catalogTag)
        {
            return;
        }

        var schemaName = PromptDialog.ShowDialog(this, "Add Schema", $"Enter a new schema name for catalog '{catalogTag.CatalogName}':");
        if (string.IsNullOrWhiteSpace(schemaName))
        {
            return;
        }

        schemaName = schemaName.Trim();
        SetLoadingState(true);
        try
        {
            await workspaceClient.Schemas.CreateAsync(schemaName, catalogTag.CatalogName);

            var schemaNode = CreateSchemaNode(catalogTag.CatalogName, schemaName);
            catalogTreeView.SelectedNode!.Nodes.Add(schemaNode);
            catalogTreeView.SelectedNode.Expand();

            AppendLog($"Created schema {catalogTag.CatalogName}.{schemaName}.");
        }
        catch (Exception ex)
        {
            ShowDatabricksError(ex);
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private async void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (workspaceClient is null || catalogTreeView.SelectedNode is null || catalogTreeView.SelectedNode.Tag is not UnityCatalogNodeTag nodeTag)
        {
            return;
        }

        var confirmation = MessageBox.Show(
            $"Are you sure you want to delete {nodeTag.ElementType} {nodeTag.DisplayName}?",
            "Confirm Delete",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirmation != DialogResult.Yes)
        {
            return;
        }

        SetLoadingState(true);
        try
        {
            switch (nodeTag)
            {
                case TableNodeTag tableTag:
                    await workspaceClient.Tables.DeleteAsync(tableTag.FullName);
                    break;
                case VolumeNodeTag volumeTag:
                    await workspaceClient.Volumes.DeleteAsync(volumeTag.FullName);
                    break;
                case SchemaNodeTag schemaTag:
                    await workspaceClient.Schemas.DeleteAsync(schemaTag.FullName, force: true);
                    break;
                case CatalogNodeTag catalogTag:
                    await workspaceClient.Catalogs.DeleteAsync(catalogTag.CatalogName, force: true);
                    break;
                default:
                    return;
            }

            catalogTreeView.SelectedNode.Remove();
            ClearPermissionState();
            AppendLog($"Deleted {nodeTag.ElementType} {nodeTag.DisplayName}.");
        }
        catch (Exception ex)
        {
            ShowDatabricksError(ex);
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private void CatalogTreeView_AfterSelect(object sender, TreeViewEventArgs e)
    {
        _ = LoadPermissionsForSelectionAsync(e.Node);
    }

    private void CatalogTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
    {
        if (e.Button != MouseButtons.Right)
        {
            return;
        }

        catalogTreeView.SelectedNode = e.Node;
    }

    private void TreeContextMenuStrip_Opening(object? sender, CancelEventArgs e)
    {
        var node = catalogTreeView.SelectedNode;
        if (node?.Tag is not UnityCatalogNodeTag tag)
        {
            e.Cancel = true;
            return;
        }

        addSchemaToolStripMenuItem.Visible = tag is CatalogNodeTag;
        deleteToolStripMenuItem.Visible = true;
    }

    private async Task LoadPermissionsForSelectionAsync(TreeNode? selectedNode)
    {
        if (workspaceClient is null)
        {
            ClearPermissionState();
            return;
        }

        if (selectedNode?.Tag is not UnityCatalogNodeTag tag)
        {
            ClearPermissionState();
            return;
        }

        var target = ResolvePermissionTarget(tag);
        if (target is null)
        {
            currentPermissionTarget = null;
            selectedObjectLabel.Text = $"Selected: {tag.ElementType} {tag.DisplayName} (permissions not supported)";
            directPermissionsListView.Items.Clear();
            effectivePermissionsListView.Items.Clear();
            SetPermissionControlsEnabled(false);
            return;
        }

        currentPermissionTarget = target;
        selectedObjectLabel.Text = $"Selected: {target.DisplayName}";
        SetPermissionControlsEnabled(false);
        directPermissionsListView.Items.Clear();
        effectivePermissionsListView.Items.Clear();
        permissionsStatusLabel.Text = "Loading permissions...";

        var loadVersion = Interlocked.Increment(ref permissionsLoadVersion);
        try
        {
            var directTask = workspaceClient.Grants.GetAsync(target.SecurableType, target.FullName);
            var effectiveTask = workspaceClient.Grants.GetEffectiveAsync(target.SecurableType, target.FullName);
            await Task.WhenAll(directTask, effectiveTask);

            if (loadVersion != permissionsLoadVersion || !ReferenceEquals(currentPermissionTarget, target))
            {
                return;
            }

            var directPermissions = directTask.Result;
            var effectivePermissions = effectiveTask.Result;

            BindPermissions(directPermissionsListView, directPermissions);
            BindPermissions(effectivePermissionsListView, effectivePermissions);

            permissionsStatusLabel.Text = $"Loaded {directPermissions.Count} direct grant(s) and {effectivePermissions.Count} effective grant(s).";
            SetPermissionControlsEnabled(true);
        }
        catch (Exception ex)
        {
            if (loadVersion != permissionsLoadVersion)
            {
                return;
            }

            if (IsAccessDenied(ex))
            {
                permissionsStatusLabel.Text = "Permissions unavailable: access denied.";
                AppendLog($"Permissions unavailable for {target.DisplayName}: {ex.Message}");
            }
            else
            {
                permissionsStatusLabel.Text = "Failed to load permissions.";
                ShowDatabricksError(ex);
            }

            SetPermissionControlsEnabled(false);
        }
    }

    private async void RefreshPermissionsButton_Click(object sender, EventArgs e)
    {
        if (currentPermissionTarget is null)
        {
            return;
        }

        await LoadPermissionsForSelectionAsync(catalogTreeView.SelectedNode);
    }

    private async void AddPermissionButton_Click(object sender, EventArgs e)
    {
        var target = currentPermissionTarget;
        if (workspaceClient is null || target is null)
        {
            return;
        }

        using var dialog = PermissionEditorDialog.Create(
            this,
            "Add Permission",
            $"Add privileges for {target.DisplayName}.",
            target.AllowedPrivileges,
            principal: string.Empty,
            selectedPrivileges: []);

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var result = dialog.Result;
        if (result is null)
        {
            return;
        }

        await ApplyPermissionChangeAsync(target, result.Principal, result.Privileges, add: true);
    }

    private async void RemovePermissionButton_Click(object sender, EventArgs e)
    {
        var target = currentPermissionTarget;
        var selectedAssignment = GetSelectedDirectAssignment();
        if (workspaceClient is null || target is null || selectedAssignment is null)
        {
            MessageBox.Show("Select a direct permission entry to remove.", "Remove Permission", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = PermissionEditorDialog.Create(
            this,
            "Remove Permission",
            $"Remove privileges from {selectedAssignment.Principal} on {target.DisplayName}.",
            target.AllowedPrivileges,
            principal: selectedAssignment.Principal,
            selectedPrivileges: selectedAssignment.Privileges.Select(privilege => privilege.Name).ToArray());

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var result = dialog.Result;
        if (result is null)
        {
            return;
        }

        await ApplyPermissionChangeAsync(target, result.Principal, result.Privileges, add: false);
    }

    private void DirectPermissionsListView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
    {
        if (currentPermissionTarget is null)
        {
            SetPermissionControlsEnabled(false);
            return;
        }

        SetPermissionControlsEnabled(true);
    }

    private async Task ApplyPermissionChangeAsync(PermissionTarget target, string principal, IReadOnlyList<string> privileges, bool add)
    {
        if (workspaceClient is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(principal))
        {
            MessageBox.Show("Principal is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (privileges.Count == 0)
        {
            MessageBox.Show("Select at least one privilege.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        SetLoadingState(true);
        try
        {
            var change = new PrivilegeChange
            {
                Principal = principal.Trim(),
                Add = add ? privileges.Select(NormalizePrivilegeName).ToList() : null,
                Remove = add ? null : privileges.Select(NormalizePrivilegeName).ToList()
            };

            await workspaceClient.Grants.UpdateAsync(target.SecurableType, target.FullName, [change]);
            AppendLog($"{(add ? "Granted" : "Revoked")} privileges for {principal.Trim()} on {target.DisplayName}.");
            await LoadPermissionsForSelectionAsync(catalogTreeView.SelectedNode);
        }
        catch (Exception ex)
        {
            ShowDatabricksError(ex);
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private static PermissionTarget? ResolvePermissionTarget(UnityCatalogNodeTag tag)
    {
        return tag switch
        {
            CatalogNodeTag catalogTag => new PermissionTarget(
                "catalog",
                catalogTag.CatalogName,
                $"Catalog {catalogTag.CatalogName}",
                CatalogPrivileges),
            SchemaNodeTag schemaTag => new PermissionTarget(
                "schema",
                schemaTag.FullName,
                $"Schema {schemaTag.FullName}",
                SchemaPrivileges),
            _ => null
        };
    }

    private void BindPermissions(ListView listView, IReadOnlyList<PrivilegeAssignmentInfo> assignments)
    {
        listView.BeginUpdate();
        try
        {
            listView.Items.Clear();

            foreach (var assignment in assignments
                         .OrderBy(assignment => assignment.Principal, StringComparer.OrdinalIgnoreCase))
            {
                var item = new ListViewItem(assignment.Principal)
                {
                Tag = assignment
            };

            item.SubItems.Add(string.Join(", ", assignment.Privileges
                .Select(privilege => privilege.Name)
                .Where(privilege => !string.IsNullOrWhiteSpace(privilege))
                .OrderBy(privilege => privilege, StringComparer.OrdinalIgnoreCase)));
            listView.Items.Add(item);
        }

            if (listView.Items.Count > 0)
            {
                listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                listView.Columns[1].Width = Math.Max(listView.Columns[1].Width, 420);
            }
        }
        finally
        {
            listView.EndUpdate();
        }
    }

    private PrivilegeAssignmentInfo? GetSelectedDirectAssignment()
    {
        if (directPermissionsListView.SelectedItems.Count == 0)
        {
            return null;
        }

        return directPermissionsListView.SelectedItems[0].Tag as PrivilegeAssignmentInfo;
    }

    private void ClearPermissionState()
    {
        currentPermissionTarget = null;
        selectedObjectLabel.Text = "Selected: none";
        permissionsStatusLabel.Text = "Select a catalog or schema to view permissions.";
        directPermissionsListView.Items.Clear();
        effectivePermissionsListView.Items.Clear();
        SetPermissionControlsEnabled(false);
    }

    private void SetPermissionControlsEnabled(bool enabled)
    {
        refreshPermissionsButton.Enabled = enabled;
        addPermissionButton.Enabled = enabled;
        removePermissionButton.Enabled = enabled && directPermissionsListView.SelectedItems.Count > 0;
    }

    private static string NormalizePrivilegeName(string privilege)
    {
        return privilege.Trim().ToUpperInvariant();
    }

    private static bool IsAccessDenied(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is HttpRequestException httpException)
            {
                if (httpException.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return true;
                }
            }

            var message = current.Message;
            if (message.Contains("PERMISSION_DENIED", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("does not have USE SCHEMA", StringComparison.OrdinalIgnoreCase) ||
                message.Contains("NOT_AUTHORIZED", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private void SetLoadingState(bool loading)
    {
        isLoading = loading;
        connectButton.Enabled = !loading;
        connectButton.Text = workspaceClient is null ? "Connect" : "Disconnect";
        savedConnectionsComboBox.Enabled = !loading && workspaceClient is null;
        catalogTreeView.Enabled = !loading;
        permissionsPanel.Enabled = !loading;
        Cursor = loading ? Cursors.WaitCursor : Cursors.Default;
    }

    private void RefreshSavedConnections(string? preferredConnectionName = null)
    {
        var savedConnections = connectionStore.LoadAll();

        savedConnectionsComboBox.BeginUpdate();
        try
        {
            savedConnectionsComboBox.Items.Clear();
            foreach (var savedConnection in savedConnections)
            {
                savedConnectionsComboBox.Items.Add(savedConnection);
            }

            savedConnectionsComboBox.Enabled = workspaceClient is null && !isLoading;

            if (savedConnectionsComboBox.Items.Count == 0)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(preferredConnectionName))
            {
                var preferred = savedConnections.FirstOrDefault(connection =>
                    string.Equals(connection.Name, preferredConnectionName, StringComparison.OrdinalIgnoreCase));

                if (preferred is not null)
                {
                    savedConnectionsComboBox.SelectedItem = preferred;
                    return;
                }
            }

            if (savedConnectionsComboBox.SelectedIndex < 0)
            {
                savedConnectionsComboBox.SelectedIndex = 0;
            }
        }
        finally
        {
            savedConnectionsComboBox.EndUpdate();
        }
    }

    private void SavedConnectionsComboBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (savedConnectionsComboBox.SelectedItem is SavedConnection savedConnection)
        {
            ApplySavedConnectionToFields(savedConnection);
        }
    }

    private void ApplySavedConnectionToFields(SavedConnection savedConnection)
    {
        hostTextBox.Text = savedConnection.Host;
        tenantIdTextBox.Text = savedConnection.AzureTenantId;
        clientIdTextBox.Text = savedConnection.ClientId;
        clientSecretTextBox.Text = savedConnection.ClientSecret;
    }

    private void PromptToSaveConnection(DatabricksConfig config)
    {
        var defaultName = GetDefaultConnectionName(config.Host);
        var prompt = PromptDialog.ShowDialog(
            this,
            "Save Connection",
            "Enter a name for this connection:",
            defaultName);

        if (string.IsNullOrWhiteSpace(prompt))
        {
            return;
        }

        var savedConnection = SavedConnection.Create(prompt.Trim(), config);
        try
        {
            connectionStore.Upsert(savedConnection);
            RefreshSavedConnections(savedConnection.Name);
            AppendLog($"Saved connection '{savedConnection.Name}'.");
        }
        catch (Exception ex)
        {
            ShowDatabricksError(ex);
        }
    }

    private static string GetDefaultConnectionName(string host)
    {
        host = host.Trim();

        if (Uri.TryCreate(host, UriKind.Absolute, out var uri))
        {
            return uri.Host;
        }

        host = host.Replace("https://", string.Empty, StringComparison.OrdinalIgnoreCase);
        host = host.Replace("http://", string.Empty, StringComparison.OrdinalIgnoreCase);
        return host.TrimEnd('/');
    }

    private void AppendLog(string message)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => AppendLog(message)));
            return;
        }

        logTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        logTextBox.SelectionStart = logTextBox.TextLength;
        logTextBox.ScrollToCaret();
    }

    private void ShowDatabricksError(Exception exception)
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(() => ShowDatabricksError(exception)));
            return;
        }

        AppendLog($"Error: {exception.Message}");
        MessageBox.Show(exception.Message, "Databricks Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private static TreeNode CreateCatalogNode(string catalogName)
    {
        return new TreeNode($"Catalog: {catalogName}")
        {
            Tag = new CatalogNodeTag(catalogName),
            ImageKey = CatalogIconKey,
            SelectedImageKey = CatalogIconKey
        };
    }

    private static TreeNode CreateSchemaNode(string catalogName, string schemaName)
    {
        return new TreeNode($"Schema: {schemaName}")
        {
            Tag = new SchemaNodeTag(catalogName, schemaName),
            ImageKey = SchemaIconKey,
            SelectedImageKey = SchemaIconKey
        };
    }

    private static TreeNode CreateTableNode(string catalogName, string schemaName, string tableName, string tableType)
    {
        return new TreeNode($"Table: {tableName} ({tableType})")
        {
            Tag = new TableNodeTag(catalogName, schemaName, tableName, tableType),
            ImageKey = TableIconKey,
            SelectedImageKey = TableIconKey
        };
    }

    private static TreeNode CreateVolumeNode(string catalogName, string schemaName, string volumeName, string volumeType)
    {
        return new TreeNode($"Volume: {volumeName} ({volumeType})")
        {
            Tag = new VolumeNodeTag(catalogName, schemaName, volumeName, volumeType),
            ImageKey = VolumeIconKey,
            SelectedImageKey = VolumeIconKey
        };
    }

    private void InitializeTreeIcons()
    {
        treeImageList.ColorDepth = ColorDepth.Depth32Bit;
        treeImageList.ImageSize = new Size(16, 16);
        treeImageList.Images.Clear();
        treeImageList.Images.Add(CatalogIconKey, CreateNodeIcon("C", Color.FromArgb(0, 102, 204)));
        treeImageList.Images.Add(SchemaIconKey, CreateNodeIcon("S", Color.FromArgb(0, 153, 102)));
        treeImageList.Images.Add(TableIconKey, CreateNodeIcon("T", Color.FromArgb(204, 102, 0)));
        treeImageList.Images.Add(VolumeIconKey, CreateNodeIcon("V", Color.FromArgb(128, 64, 192)));
    }

    private static Bitmap CreateNodeIcon(string text, Color backgroundColor)
    {
        var bitmap = new Bitmap(16, 16);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        graphics.Clear(Color.Transparent);

        using var backgroundBrush = new SolidBrush(backgroundColor);
        using var textBrush = new SolidBrush(Color.White);
        using var font = new Font("Segoe UI", 8F, FontStyle.Bold, GraphicsUnit.Point);
        using var rectPath = new System.Drawing.Drawing2D.GraphicsPath();

        rectPath.AddArc(0, 0, 4, 4, 180, 90);
        rectPath.AddArc(12, 0, 4, 4, 270, 90);
        rectPath.AddArc(12, 12, 4, 4, 0, 90);
        rectPath.AddArc(0, 12, 4, 4, 90, 90);
        rectPath.CloseFigure();

        graphics.FillPath(backgroundBrush, rectPath);

        var format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };
        graphics.DrawString(text, font, textBrush, new RectangleF(0, 0, 16, 16), format);

        return bitmap;
    }
}

internal sealed record PermissionTarget(string SecurableType, string FullName, string DisplayName, IReadOnlyList<string> AllowedPrivileges);

internal sealed record PermissionEditResult(string Principal, IReadOnlyList<string> Privileges);

internal abstract record UnityCatalogNodeTag(string ElementType, string DisplayName);

internal sealed record CatalogNodeTag(string CatalogName)
    : UnityCatalogNodeTag("Catalog", CatalogName);

internal sealed record SchemaNodeTag(string CatalogName, string SchemaName)
    : UnityCatalogNodeTag("Schema", SchemaName)
{
    public string FullName => $"{CatalogName}.{SchemaName}";
}

internal sealed record TableNodeTag(string CatalogName, string SchemaName, string TableName, string TableType)
    : UnityCatalogNodeTag("Table", TableName)
{
    public string FullName => $"{CatalogName}.{SchemaName}.{TableName}";
}

internal sealed record VolumeNodeTag(string CatalogName, string SchemaName, string VolumeName, string VolumeType)
    : UnityCatalogNodeTag("Volume", VolumeName)
{
    public string FullName => $"{CatalogName}.{SchemaName}.{VolumeName}";
}
