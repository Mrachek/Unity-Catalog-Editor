using Databricks.Sdk;
using System.ComponentModel;

namespace UnityCatalogEditor;

public partial class Form1 : Form
{
    private WorkspaceClient? workspaceClient;
    private bool isLoading;

    public Form1()
    {
        InitializeComponent();
    }

    private async void ConnectButton_Click(object sender, EventArgs e)
    {
        if (isLoading)
        {
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

    private void SetLoadingState(bool loading)
    {
        isLoading = loading;
        connectButton.Enabled = !loading;
        catalogTreeView.Enabled = !loading;
        Cursor = loading ? Cursors.WaitCursor : Cursors.Default;
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
        return new TreeNode($"📁 Catalog: {catalogName}")
        {
            Tag = new CatalogNodeTag(catalogName)
        };
    }

    private static TreeNode CreateSchemaNode(string catalogName, string schemaName)
    {
        return new TreeNode($"📂 Schema: {schemaName}")
        {
            Tag = new SchemaNodeTag(catalogName, schemaName)
        };
    }

    private static TreeNode CreateTableNode(string catalogName, string schemaName, string tableName, string tableType)
    {
        return new TreeNode($"📄 Table: {tableName} ({tableType})")
        {
            Tag = new TableNodeTag(catalogName, schemaName, tableName, tableType)
        };
    }

    private static TreeNode CreateVolumeNode(string catalogName, string schemaName, string volumeName, string volumeType)
    {
        return new TreeNode($"📦 Volume: {volumeName} ({volumeType})")
        {
            Tag = new VolumeNodeTag(catalogName, schemaName, volumeName, volumeType)
        };
    }
}

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
