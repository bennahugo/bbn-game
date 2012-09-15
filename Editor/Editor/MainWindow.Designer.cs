namespace Editor
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tspMain = new System.Windows.Forms.ToolStrip();
            this.tsbNew = new System.Windows.Forms.ToolStripButton();
            this.tsbOpen = new System.Windows.Forms.ToolStripButton();
            this.tsbSave = new System.Windows.Forms.ToolStripButton();
            this.tssCam = new System.Windows.Forms.ToolStripSeparator();
            this.tsbSetCameraPos = new System.Windows.Forms.ToolStripButton();
            this.tsbFocus = new System.Windows.Forms.ToolStripButton();
            this.tsbMoveObjectToFocus = new System.Windows.Forms.ToolStripButton();
            this.tsbDeleteObject = new System.Windows.Forms.ToolStripButton();
            this.tsbAddAttribute = new System.Windows.Forms.ToolStripButton();
            this.tssNodes = new System.Windows.Forms.ToolStripSeparator();
            this.tsbAddPathNode = new System.Windows.Forms.ToolStripButton();
            this.tsbConnectPathNodes = new System.Windows.Forms.ToolStripButton();
            this.tsbAddSpawnPoint = new System.Windows.Forms.ToolStripButton();
            this.tsbPlayerSpawnpoint = new System.Windows.Forms.ToolStripButton();
            this.tspOther = new System.Windows.Forms.ToolStripSeparator();
            this.tsbSkybox = new System.Windows.Forms.ToolStripButton();
            this.tsbSetMapRadius = new System.Windows.Forms.ToolStripButton();
            this.tsbOpenGuide = new System.Windows.Forms.ToolStripButton();
            this.scrMainLayout = new System.Windows.Forms.SplitContainer();
            this.scrLeftSplitView = new System.Windows.Forms.SplitContainer();
            this.lblProperties = new System.Windows.Forms.Label();
            this.cbxMapItems = new System.Windows.Forms.ComboBox();
            this.dgvProperties = new System.Windows.Forms.DataGridView();
            this.Properties = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Values = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tvwToolbox = new System.Windows.Forms.TreeView();
            this.ofdMainWindow = new System.Windows.Forms.OpenFileDialog();
            this.sfdMainWindow = new System.Windows.Forms.SaveFileDialog();
            this.tspMain.SuspendLayout();
            this.scrMainLayout.Panel1.SuspendLayout();
            this.scrMainLayout.SuspendLayout();
            this.scrLeftSplitView.Panel1.SuspendLayout();
            this.scrLeftSplitView.Panel2.SuspendLayout();
            this.scrLeftSplitView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProperties)).BeginInit();
            this.SuspendLayout();
            // 
            // tspMain
            // 
            this.tspMain.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.tspMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbNew,
            this.tsbOpen,
            this.tsbSave,
            this.tssCam,
            this.tsbSetCameraPos,
            this.tsbFocus,
            this.tsbMoveObjectToFocus,
            this.tsbDeleteObject,
            this.tsbAddAttribute,
            this.tssNodes,
            this.tsbAddPathNode,
            this.tsbConnectPathNodes,
            this.tsbAddSpawnPoint,
            this.tsbPlayerSpawnpoint,
            this.tspOther,
            this.tsbSkybox,
            this.tsbSetMapRadius,
            this.tsbOpenGuide});
            this.tspMain.Location = new System.Drawing.Point(0, 0);
            this.tspMain.Name = "tspMain";
            this.tspMain.Size = new System.Drawing.Size(1008, 46);
            this.tspMain.TabIndex = 1;
            this.tspMain.Text = "toolStrip1";
            // 
            // tsbNew
            // 
            this.tsbNew.Image = global::Editor.Properties.Resources.newFile;
            this.tsbNew.ImageTransparentColor = System.Drawing.Color.White;
            this.tsbNew.Name = "tsbNew";
            this.tsbNew.Size = new System.Drawing.Size(35, 43);
            this.tsbNew.Text = "&New";
            this.tsbNew.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsbNew.Click += new System.EventHandler(this.tsbNew_Click);
            // 
            // tsbOpen
            // 
            this.tsbOpen.Image = global::Editor.Properties.Resources.open;
            this.tsbOpen.ImageTransparentColor = System.Drawing.Color.White;
            this.tsbOpen.Name = "tsbOpen";
            this.tsbOpen.Size = new System.Drawing.Size(40, 43);
            this.tsbOpen.Text = "&Open";
            this.tsbOpen.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsbOpen.Click += new System.EventHandler(this.tsbOpen_Click);
            // 
            // tsbSave
            // 
            this.tsbSave.Image = global::Editor.Properties.Resources.save;
            this.tsbSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbSave.Name = "tsbSave";
            this.tsbSave.Size = new System.Drawing.Size(35, 43);
            this.tsbSave.Text = "&Save";
            this.tsbSave.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsbSave.Click += new System.EventHandler(this.tsbSave_Click);
            // 
            // tssCam
            // 
            this.tssCam.Name = "tssCam";
            this.tssCam.Size = new System.Drawing.Size(6, 46);
            // 
            // tsbSetCameraPos
            // 
            this.tsbSetCameraPos.Image = global::Editor.Properties.Resources.camera;
            this.tsbSetCameraPos.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbSetCameraPos.Name = "tsbSetCameraPos";
            this.tsbSetCameraPos.Size = new System.Drawing.Size(98, 43);
            this.tsbSetCameraPos.Text = "Camera Position";
            this.tsbSetCameraPos.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsbSetCameraPos.Click += new System.EventHandler(this.tsbSetCameraPos_Click);
            // 
            // tsbFocus
            // 
            this.tsbFocus.Image = global::Editor.Properties.Resources.camera;
            this.tsbFocus.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbFocus.Name = "tsbFocus";
            this.tsbFocus.Size = new System.Drawing.Size(95, 43);
            this.tsbFocus.Text = "&Focus on object";
            this.tsbFocus.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsbFocus.Click += new System.EventHandler(this.tsbFocus_Click);
            // 
            // tsbMoveObjectToFocus
            // 
            this.tsbMoveObjectToFocus.Image = global::Editor.Properties.Resources.moveObject;
            this.tsbMoveObjectToFocus.ImageTransparentColor = System.Drawing.Color.White;
            this.tsbMoveObjectToFocus.Name = "tsbMoveObjectToFocus";
            this.tsbMoveObjectToFocus.Size = new System.Drawing.Size(97, 43);
            this.tsbMoveObjectToFocus.Text = "&Object To Focus";
            this.tsbMoveObjectToFocus.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsbMoveObjectToFocus.Click += new System.EventHandler(this.tsbMoveObjectToFocus_Click);
            // 
            // tsbDeleteObject
            // 
            this.tsbDeleteObject.Image = global::Editor.Properties.Resources.delObject;
            this.tsbDeleteObject.ImageTransparentColor = System.Drawing.Color.White;
            this.tsbDeleteObject.Name = "tsbDeleteObject";
            this.tsbDeleteObject.Size = new System.Drawing.Size(82, 43);
            this.tsbDeleteObject.Text = "&Delete Object";
            this.tsbDeleteObject.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsbDeleteObject.Click += new System.EventHandler(this.tsbDeleteObject_Click);
            // 
            // tsbAddAttribute
            // 
            this.tsbAddAttribute.Image = global::Editor.Properties.Resources.add_attribute;
            this.tsbAddAttribute.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbAddAttribute.Name = "tsbAddAttribute";
            this.tsbAddAttribute.Size = new System.Drawing.Size(83, 43);
            this.tsbAddAttribute.Text = "Add Attribute";
            this.tsbAddAttribute.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsbAddAttribute.Click += new System.EventHandler(this.tsbAddAttribute_Click);
            // 
            // tssNodes
            // 
            this.tssNodes.Name = "tssNodes";
            this.tssNodes.Size = new System.Drawing.Size(6, 46);
            // 
            // tsbAddPathNode
            // 
            this.tsbAddPathNode.Image = global::Editor.Properties.Resources.path;
            this.tsbAddPathNode.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbAddPathNode.Name = "tsbAddPathNode";
            this.tsbAddPathNode.Size = new System.Drawing.Size(92, 43);
            this.tsbAddPathNode.Text = "&Add Path Node";
            this.tsbAddPathNode.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsbAddPathNode.Click += new System.EventHandler(this.tsbAddPathNode_Click);
            // 
            // tsbConnectPathNodes
            // 
            this.tsbConnectPathNodes.Image = global::Editor.Properties.Resources.connectNodes;
            this.tsbConnectPathNodes.ImageTransparentColor = System.Drawing.Color.White;
            this.tsbConnectPathNodes.Name = "tsbConnectPathNodes";
            this.tsbConnectPathNodes.Size = new System.Drawing.Size(120, 43);
            this.tsbConnectPathNodes.Text = "Connect Path Nodes";
            this.tsbConnectPathNodes.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsbConnectPathNodes.Click += new System.EventHandler(this.tsbConnectPathNodes_Click);
            // 
            // tsbAddSpawnPoint
            // 
            this.tsbAddSpawnPoint.Image = global::Editor.Properties.Resources.spawn;
            this.tsbAddSpawnPoint.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbAddSpawnPoint.Name = "tsbAddSpawnPoint";
            this.tsbAddSpawnPoint.Size = new System.Drawing.Size(102, 43);
            this.tsbAddSpawnPoint.Text = "Add Spawn Point";
            this.tsbAddSpawnPoint.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsbAddSpawnPoint.Click += new System.EventHandler(this.tsbAddSpawnPoint_Click);
            // 
            // tsbPlayerSpawnpoint
            // 
            this.tsbPlayerSpawnpoint.Image = global::Editor.Properties.Resources.spawn;
            this.tsbPlayerSpawnpoint.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbPlayerSpawnpoint.Name = "tsbPlayerSpawnpoint";
            this.tsbPlayerSpawnpoint.Size = new System.Drawing.Size(136, 43);
            this.tsbPlayerSpawnpoint.Text = "Add player spawn point";
            this.tsbPlayerSpawnpoint.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsbPlayerSpawnpoint.Click += new System.EventHandler(this.tsbPlayerSpawnpoint_Click);
            // 
            // tspOther
            // 
            this.tspOther.Name = "tspOther";
            this.tspOther.Size = new System.Drawing.Size(6, 46);
            // 
            // tsbSkybox
            // 
            this.tsbSkybox.Image = global::Editor.Properties.Resources.skybox;
            this.tsbSkybox.ImageTransparentColor = System.Drawing.Color.White;
            this.tsbSkybox.Name = "tsbSkybox";
            this.tsbSkybox.Size = new System.Drawing.Size(48, 43);
            this.tsbSkybox.Text = "Skybox";
            this.tsbSkybox.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsbSkybox.Click += new System.EventHandler(this.tsbSkybox_Click);
            // 
            // tsbSetMapRadius
            // 
            this.tsbSetMapRadius.Image = global::Editor.Properties.Resources.mapRadius;
            this.tsbSetMapRadius.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbSetMapRadius.Name = "tsbSetMapRadius";
            this.tsbSetMapRadius.Size = new System.Drawing.Size(57, 43);
            this.tsbSetMapRadius.Text = "Map size";
            this.tsbSetMapRadius.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsbSetMapRadius.Click += new System.EventHandler(this.tsbSetMapRadius_Click);
            // 
            // tsbOpenGuide
            // 
            this.tsbOpenGuide.Image = global::Editor.Properties.Resources.help;
            this.tsbOpenGuide.ImageTransparentColor = System.Drawing.Color.White;
            this.tsbOpenGuide.Name = "tsbOpenGuide";
            this.tsbOpenGuide.Size = new System.Drawing.Size(68, 43);
            this.tsbOpenGuide.Text = "User Guide";
            this.tsbOpenGuide.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.tsbOpenGuide.Click += new System.EventHandler(this.tsbOpenGuide_Click);
            // 
            // scrMainLayout
            // 
            this.scrMainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scrMainLayout.IsSplitterFixed = true;
            this.scrMainLayout.Location = new System.Drawing.Point(0, 46);
            this.scrMainLayout.Name = "scrMainLayout";
            // 
            // scrMainLayout.Panel1
            // 
            this.scrMainLayout.Panel1.Controls.Add(this.scrLeftSplitView);
            // 
            // scrMainLayout.Panel2
            // 
            this.scrMainLayout.Panel2.MouseMove += new System.Windows.Forms.MouseEventHandler(this.scrMainLayout_Panel2_MouseMove);
            this.scrMainLayout.Panel2.Click += new System.EventHandler(this.scrMainLayout_Panel2_Click);
            this.scrMainLayout.Panel2.MouseDown += new System.Windows.Forms.MouseEventHandler(this.scrMainLayout_Panel2_MouseDown);
            this.scrMainLayout.Panel2.Resize += new System.EventHandler(this.splitContainer1_Panel2_Resize);
            this.scrMainLayout.Panel2.MouseUp += new System.Windows.Forms.MouseEventHandler(this.scrMainLayout_Panel2_MouseUp);
            this.scrMainLayout.Size = new System.Drawing.Size(1008, 640);
            this.scrMainLayout.SplitterDistance = 332;
            this.scrMainLayout.TabIndex = 2;
            // 
            // scrLeftSplitView
            // 
            this.scrLeftSplitView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scrLeftSplitView.IsSplitterFixed = true;
            this.scrLeftSplitView.Location = new System.Drawing.Point(0, 0);
            this.scrLeftSplitView.Name = "scrLeftSplitView";
            this.scrLeftSplitView.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // scrLeftSplitView.Panel1
            // 
            this.scrLeftSplitView.Panel1.BackColor = System.Drawing.SystemColors.ControlDark;
            this.scrLeftSplitView.Panel1.Controls.Add(this.lblProperties);
            this.scrLeftSplitView.Panel1.Controls.Add(this.cbxMapItems);
            this.scrLeftSplitView.Panel1.Controls.Add(this.dgvProperties);
            // 
            // scrLeftSplitView.Panel2
            // 
            this.scrLeftSplitView.Panel2.Controls.Add(this.tvwToolbox);
            this.scrLeftSplitView.Size = new System.Drawing.Size(332, 640);
            this.scrLeftSplitView.SplitterDistance = 287;
            this.scrLeftSplitView.TabIndex = 0;
            // 
            // lblProperties
            // 
            this.lblProperties.AutoSize = true;
            this.lblProperties.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblProperties.ForeColor = System.Drawing.SystemColors.ControlLightLight;
            this.lblProperties.Location = new System.Drawing.Point(0, 0);
            this.lblProperties.Name = "lblProperties";
            this.lblProperties.Size = new System.Drawing.Size(95, 13);
            this.lblProperties.TabIndex = 2;
            this.lblProperties.Text = "Properties Explorer";
            // 
            // cbxMapItems
            // 
            this.cbxMapItems.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.cbxMapItems.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxMapItems.FormattingEnabled = true;
            this.cbxMapItems.Location = new System.Drawing.Point(0, 15);
            this.cbxMapItems.Name = "cbxMapItems";
            this.cbxMapItems.Size = new System.Drawing.Size(332, 21);
            this.cbxMapItems.TabIndex = 1;
            this.cbxMapItems.SelectedIndexChanged += new System.EventHandler(this.cbxMapItems_SelectedIndexChanged);
            // 
            // dgvProperties
            // 
            this.dgvProperties.AllowUserToAddRows = false;
            this.dgvProperties.AllowUserToDeleteRows = false;
            this.dgvProperties.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvProperties.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Properties,
            this.Values});
            this.dgvProperties.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.dgvProperties.Location = new System.Drawing.Point(0, 36);
            this.dgvProperties.MultiSelect = false;
            this.dgvProperties.Name = "dgvProperties";
            this.dgvProperties.Size = new System.Drawing.Size(332, 251);
            this.dgvProperties.TabIndex = 0;
            this.dgvProperties.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvProperties_CellEndEdit);
            // 
            // Properties
            // 
            this.Properties.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Properties.FillWeight = 47.71573F;
            this.Properties.HeaderText = "Properties";
            this.Properties.Name = "Properties";
            this.Properties.ReadOnly = true;
            // 
            // Values
            // 
            this.Values.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.Values.FillWeight = 152.2843F;
            this.Values.HeaderText = "Values";
            this.Values.Name = "Values";
            this.Values.Width = 150;
            // 
            // tvwToolbox
            // 
            this.tvwToolbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvwToolbox.Location = new System.Drawing.Point(0, 0);
            this.tvwToolbox.Name = "tvwToolbox";
            this.tvwToolbox.Size = new System.Drawing.Size(332, 349);
            this.tvwToolbox.TabIndex = 0;
            this.tvwToolbox.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvwToolbox_AfterSelect);
            // 
            // ofdMainWindow
            // 
            this.ofdMainWindow.Filter = "Map XML file|*.xml";
            // 
            // sfdMainWindow
            // 
            this.sfdMainWindow.DefaultExt = "xml";
            this.sfdMainWindow.Filter = "Map XML file|*.xml";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 686);
            this.Controls.Add(this.scrMainLayout);
            this.Controls.Add(this.tspMain);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1024, 724);
            this.MinimumSize = new System.Drawing.Size(1024, 724);
            this.Name = "frmMain";
            this.Text = "BBN World Editor";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.tspMain.ResumeLayout(false);
            this.tspMain.PerformLayout();
            this.scrMainLayout.Panel1.ResumeLayout(false);
            this.scrMainLayout.ResumeLayout(false);
            this.scrLeftSplitView.Panel1.ResumeLayout(false);
            this.scrLeftSplitView.Panel1.PerformLayout();
            this.scrLeftSplitView.Panel2.ResumeLayout(false);
            this.scrLeftSplitView.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvProperties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip tspMain;
        private System.Windows.Forms.SplitContainer scrMainLayout;
        private System.Windows.Forms.SplitContainer scrLeftSplitView;
        private System.Windows.Forms.DataGridView dgvProperties;
        private System.Windows.Forms.ComboBox cbxMapItems;
        private System.Windows.Forms.TreeView tvwToolbox;
        private System.Windows.Forms.DataGridViewTextBoxColumn Properties;
        private System.Windows.Forms.DataGridViewTextBoxColumn Values;
        private System.Windows.Forms.ToolStripButton tsbNew;
        private System.Windows.Forms.ToolStripButton tsbOpen;
        private System.Windows.Forms.ToolStripButton tsbSave;
        private System.Windows.Forms.ToolStripSeparator tssCam;
        private System.Windows.Forms.ToolStripButton tsbSetCameraPos;
        private System.Windows.Forms.ToolStripButton tsbFocus;
        private System.Windows.Forms.ToolStripSeparator tssNodes;
        private System.Windows.Forms.ToolStripButton tsbAddPathNode;
        private System.Windows.Forms.ToolStripButton tsbAddSpawnPoint;
        private System.Windows.Forms.Label lblProperties;
        private System.Windows.Forms.ToolStripButton tsbMoveObjectToFocus;
        private System.Windows.Forms.ToolStripButton tsbConnectPathNodes;
        private System.Windows.Forms.ToolStripButton tsbDeleteObject;
        private System.Windows.Forms.ToolStripButton tsbAddAttribute;
        private System.Windows.Forms.ToolStripSeparator tspOther;
        private System.Windows.Forms.ToolStripButton tsbSkybox;
        private System.Windows.Forms.ToolStripButton tsbOpenGuide;
        private System.Windows.Forms.OpenFileDialog ofdMainWindow;
        private System.Windows.Forms.SaveFileDialog sfdMainWindow;
        private System.Windows.Forms.ToolStripButton tsbSetMapRadius;
        private System.Windows.Forms.ToolStripButton tsbPlayerSpawnpoint;
    }
}