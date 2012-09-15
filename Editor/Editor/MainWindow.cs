using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
//Certain game packages are included:
using BBN_Game.Map;
using BBN_Game.Utils;
using BBN_Game.AI;
using BBN_Game.Graphics.Skybox;
//include XNA:
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using BBN_Game.Collision_Detection;


namespace Editor
{
    public partial class frmMain : Form
    {
#region XNA setup variables
        /// <summary>
        /// In order to instantiate a Graphics Device we have to create a Graphics Device Service and therefore we need a structure as
        /// defined by MSDN
        /// </summary>
        class GfxService : IGraphicsDeviceService
        {
            GraphicsDevice gfxDevice;
            public GfxService(GraphicsDevice gfxDevice)
            {
                this.gfxDevice = gfxDevice;
                DeviceCreated = new EventHandler(DoNothing);
                DeviceDisposing = new EventHandler(DoNothing);
                DeviceReset = new EventHandler(DoNothing);
                DeviceResetting = new EventHandler(DoNothing);
            }
            public GraphicsDevice GraphicsDevice
            { get { return gfxDevice; } }
            public event EventHandler DeviceCreated;
            public event EventHandler DeviceDisposing;
            public event EventHandler DeviceReset;
            public event EventHandler DeviceResetting;
            void DoNothing(object o, EventArgs args)
            {

            }
        }
        /// <summary>
        /// Some variables including graphics device, content manager, sprite batch, camera setup, basic effect setup
        /// </summary>
        Texture2D blank;
        GraphicsDevice gfxDevice;
        ContentManager contentMgr;
        SpriteBatch spriteBatch;
        DepthStencilBuffer defaultDepthStencil;
        long lastTimeCount = 0;
        SpriteFont font;
        EventHandler gameLoopEvent;
        float nearClip = 0.03f;
        float farClip = 500;
        BasicEffect basicEffect;
        Microsoft.Xna.Framework.Vector3 cameraFocus = new Vector3(0, 0, 0);
        Microsoft.Xna.Framework.Vector3 cameraPos = new Vector3(0, 0, 0); //Derived Attribute
        float zoomFactor = 5;
        float CamYaw = -(float)Math.PI / 6;
        float CamPitch = -(float)Math.PI / 6;
        Microsoft.Xna.Framework.Matrix view, projection;
        Microsoft.Xna.Framework.Vector3 fogColor = new Vector3(1, 1, 1);
        int[] fogSetup = { 500, 500 };
        Microsoft.Xna.Framework.Vector3[] lightsSetup = {new Vector3(0,-5,1000), new Vector3(0.0003f,0.0003f,0.0003f), new Vector3(0.0003f,0.0003f,0.0003f),
                                 new Vector3(-1000,-5,-1000), new Vector3(0.0003f,0.0003f,0.0003f), new Vector3(0.0003f,0.0003f,0.0003f),
                                 new Vector3(1000,-5,-1000), new Vector3(0.0003f,0.0003f,0.0003f), new Vector3(0.0003f,0.0003f,0.0003f)};
#endregion
#region XNA setup
        /// <summary>
        /// Default constructor of form. It sets up the Xna environment
        /// </summary>
        public frmMain()
        {
            InitializeComponent();
            CreateDevice();
            defaultDepthStencil = gfxDevice.DepthStencilBuffer;
            // initialize the content manager
            GfxService gfxService = new GfxService(gfxDevice);
            GameServiceContainer services = new GameServiceContainer();
            services.AddService(typeof(IGraphicsDeviceService), gfxService);
            contentMgr = new ContentManager(services,"Content");
            spriteBatch = new SpriteBatch(gfxDevice);
            basicEffect = new BasicEffect(gfxDevice, new EffectPool());
            basicEffect.VertexColorEnabled = true;
            Initialize();
            // attach game and control loops
            (this.scrMainLayout.Panel2 as Control).KeyDown += new KeyEventHandler(this.scrMainLayoutPanel2_KeyDown);
            (this.scrMainLayout.Panel2 as Control).KeyUp += new KeyEventHandler(this.scrMainLayoutPanel2_KeyUp);
            (this.scrMainLayout.Panel2 as Control).MouseWheel += new MouseEventHandler(this.scrMainLayoutPanel2_MouseWheel);
            gameLoopEvent = new EventHandler(Application_Idle);
            Application.Idle += gameLoopEvent;
            long perfcount;
            QueryPerformanceCounter(out perfcount);
            lastTimeCount = perfcount;
        }
        /// <summary>
        /// Method to instantiate and initialize a graphics device
        /// </summary>
        private void CreateDevice()
        {
            PresentationParameters presentation = new PresentationParameters();
            presentation.AutoDepthStencilFormat = DepthFormat.Depth24;
            presentation.BackBufferCount = 1;
            presentation.BackBufferFormat = SurfaceFormat.Color;
            System.Drawing.Rectangle a = this.scrMainLayout.Panel2.Bounds;
            presentation.BackBufferWidth = a.Width;
            presentation.BackBufferHeight = a.Height;
            presentation.DeviceWindowHandle = this.Handle;
            presentation.EnableAutoDepthStencil = true;
            presentation.FullScreenRefreshRateInHz = 0;
            presentation.IsFullScreen = false;
            presentation.MultiSampleQuality = 0;
            presentation.MultiSampleType = MultiSampleType.None;
            presentation.PresentationInterval = PresentInterval.One;
            presentation.PresentOptions = PresentOptions.None;
            presentation.SwapEffect = SwapEffect.Discard;
            presentation.RenderTargetUsage = RenderTargetUsage.DiscardContents;


            gfxDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, DeviceType.Hardware, this.scrMainLayout.Panel2.Handle,
                presentation);
            gfxDevice.RenderState.CullMode = CullMode.None;

            
            gfxDevice.Reset();
            //Setup cliping frustum
            Viewport v = gfxDevice.Viewport;
            v.MinDepth = nearClip;
            v.MaxDepth = farClip;
            gfxDevice.Viewport = v;
        }
#endregion
#region Game Loop
        //
        // Game Loop stuff
        // from http://blogs.msdn.com/tmiller/archive/2005/05/05/415008.aspx
        //

        [StructLayout(LayoutKind.Sequential)]

        public struct Message
        {
            public IntPtr hWnd;
            public Int32 msg; // was WindowMessage
            public IntPtr wParam;
            public IntPtr lParam;
            public uint time;
            public System.Drawing.Point p;

        }
        [System.Security.SuppressUnmanagedCodeSecurity] // We won't use this maliciously
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool PeekMessage(out Message msg, IntPtr hWnd, uint messageFilterMin, uint messageFilterMax, uint flags);
        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        public static extern bool QueryPerformanceCounter(out long perfcount);
        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]

        public static extern bool QueryPerformanceFrequency(out long freq);
        /// <summary>
        /// App idle hook
        /// </summary>
        void Application_Idle(object sender, EventArgs e)
        {
            while (AppStillIdle)
                processXNA();
        }
        /// <summary>
        /// Method to do all XNA based processing. Call to invoke update and draw methods
        /// </summary>
        void processXNA()
        {
            long perfcount;
            QueryPerformanceCounter(out perfcount);
            long newCount = perfcount;
            long elapsedCount = newCount - lastTimeCount;
            long freq;
            QueryPerformanceFrequency(out freq);
            double elapsedSeconds = (double)elapsedCount / freq;
            lastTimeCount = newCount;
            Update((float)elapsedSeconds);
            draw();
        }
        /// <summary>
        /// Process the message queue to see if the app is idling so that we may update and draw
        /// </summary>
        protected bool AppStillIdle
        {
            get
            {
                /*NativeMethods.*/
                Message msg;
                return !PeekMessage(out msg, IntPtr.Zero, 0, 0, 0);
            }
        }
        //
        // end Game Loop stuff
        //
#endregion
#region XNA events
        /// <summary>
        /// XNA draw method
        /// </summary>
        protected void draw()
        {
            Microsoft.Xna.Framework.Graphics.Color clearColor = Microsoft.Xna.Framework.Graphics.Color.Black;
            gfxDevice.Clear(clearColor);
            gfxDevice.RenderState.DepthBufferEnable = true;
            gfxDevice.RenderState.FillMode = FillMode.Solid;
            gfxDevice.RenderState.DepthBufferWriteEnable = true;
            gfxDevice.RenderState.DepthBufferFunction = CompareFunction.LessEqual;
            //Draw all objects in map:
            BBNMap.DrawMap(gfxDevice, projection,contentMgr, view, lightsSetup, fogColor, fogSetup, basicEffect, cameraPos);
            //Draw 3D lines:
            if (selectedXMoveLine || selectedYMoveLine || selectedZMoveLine)
            {
                Object item = BBNMap.content[cbxMapItems.SelectedItem.ToString().Split(':')[0].Trim()];
                Vector3 xyz = getObjectPosition(item);
                if (selectedXMoveLine)
                    Algorithms.Draw3DLine(Microsoft.Xna.Framework.Graphics.Color.White, new Vector3(xyz.X - farClip, xyz.Y, xyz.Z), new Vector3(xyz.X+farClip, xyz.Y, xyz.Z), basicEffect, gfxDevice, projection, view, Matrix.Identity);
                if (selectedYMoveLine)
                    Algorithms.Draw3DLine(Microsoft.Xna.Framework.Graphics.Color.White, new Vector3(xyz.X, xyz.Y - farClip, xyz.Z), new Vector3(xyz.X, xyz.Y+farClip, xyz.Z), basicEffect, gfxDevice, projection, view, Matrix.Identity);
                if (selectedZMoveLine)
                    Algorithms.Draw3DLine(Microsoft.Xna.Framework.Graphics.Color.White, new Vector3(xyz.X, xyz.Y, xyz.Z - farClip), new Vector3(xyz.X, xyz.Y, xyz.Z + farClip), basicEffect, gfxDevice, projection, view, Matrix.Identity);
            }
            //Draw 2d effects over 3d environment:
            spriteBatch.Begin();
            //Draw movement control lines if an object was selected
            if (cbxMapItems.SelectedIndex >= 0 && !(yDown || pDown || rDown || cDown))
            {
                if (!(selectedXMoveLine || selectedYMoveLine || selectedZMoveLine))
                {
                    Object item = BBNMap.content[cbxMapItems.SelectedItem.ToString().Split(':')[0].Trim()];
                    Vector3 xyz = getObjectPosition(item);
                    if (Vector3.Dot(Vector3.Normalize(cameraFocus-cameraPos), Vector3.Normalize(xyz - cameraPos)) >= 0)
                    {
                        Algorithms.Draw2DLine(2, Microsoft.Xna.Framework.Graphics.Color.Green, xArrowBottom, xArrowTop, spriteBatch, blank);
                        Algorithms.Draw2DLine(2, Microsoft.Xna.Framework.Graphics.Color.Red, yArrowBottom, yArrowTop, spriteBatch, blank);
                        Algorithms.Draw2DLine(2, Microsoft.Xna.Framework.Graphics.Color.Blue, zArrowBottom, zArrowTop, spriteBatch, blank);
                    }
                }
            }
            //Draw stats to screen:
            float charheight = font.MeasureString("a").Y + 2;
            float currentheight = 0;
            spriteBatch.DrawString(font, "Cam Focus: " + String.Format("({0:0.00} , {1:0.00} , {2:0.00})", this.cameraFocus.X, this.cameraFocus.Y, this.cameraFocus.Z), 
                new Vector2(0, currentheight), Microsoft.Xna.Framework.Graphics.Color.White);
            currentheight += charheight;
            spriteBatch.DrawString(font, "Cam Zoom: " + String.Format("{0:0.00}", this.zoomFactor),
                new Vector2(0, currentheight), Microsoft.Xna.Framework.Graphics.Color.White);
            currentheight += charheight;
            spriteBatch.DrawString(font, "Cam Yaw, Cam Pitch: " + String.Format("({0:0.00} , {1:0.00})", this.CamYaw * 180 / Math.PI, this.CamPitch * 180 / Math.PI, this.cameraFocus.Z),
                new Vector2(0, currentheight), Microsoft.Xna.Framework.Graphics.Color.White);
            currentheight += charheight;
            spriteBatch.DrawString(font, "Movement speed: " + String.Format("{0:0.00}",this.movementSpeed), new Vector2(0, currentheight), Microsoft.Xna.Framework.Graphics.Color.White);
            if (cbxMapItems.SelectedIndex >= 0)
            {
                if (yDown)
                    spriteBatch.DrawString(font, "Move mouse to yaw object clockwise or counter clockwise", new Vector2(0, scrMainLayout.Panel2.Height - charheight * 2), Microsoft.Xna.Framework.Graphics.Color.Yellow);
                if (rDown)
                    spriteBatch.DrawString(font, "Move mouse to roll object clockwise or counter clockwise", new Vector2(0, scrMainLayout.Panel2.Height - charheight * 2), Microsoft.Xna.Framework.Graphics.Color.Yellow);
                if (pDown)
                    spriteBatch.DrawString(font, "Move mouse to pitch object clockwise or counter clockwise", new Vector2(0, scrMainLayout.Panel2.Height - charheight * 2), Microsoft.Xna.Framework.Graphics.Color.Yellow);
                Object item = BBNMap.content[cbxMapItems.SelectedItem.ToString().Split(':')[0].Trim()];
                if (item is Node)
                    if (cDown)
                        spriteBatch.DrawString(font, "Now click on another path node to connect/disconnect", new Vector2(0, scrMainLayout.Panel2.Height - charheight * 3), Microsoft.Xna.Framework.Graphics.Color.Yellow);
                    else
                        spriteBatch.DrawString(font, "Press and hold 'C' when clicking on another path node to connect/disconnect", new Vector2(0, scrMainLayout.Panel2.Height - charheight * 3), Microsoft.Xna.Framework.Graphics.Color.Yellow);
            }
            if (scrMainLayout.Panel2.Focused)
                spriteBatch.DrawString(font, "HID Mode: 3D input", new Vector2(0, scrMainLayout.Panel2.Height - charheight), Microsoft.Xna.Framework.Graphics.Color.Green);
            else
                spriteBatch.DrawString(font, "HID Mode: 2D GUI", new Vector2(0, scrMainLayout.Panel2.Height - charheight), Microsoft.Xna.Framework.Graphics.Color.Red);
                
            spriteBatch.End();
            gfxDevice.RenderState.DepthBufferEnable = true;
            gfxDevice.RenderState.DepthBufferWriteEnable = true;
            gfxDevice.RenderState.AlphaBlendEnable = false;
            gfxDevice.RenderState.AlphaTestEnable = false;
            //Swap buffers:
            gfxDevice.Present(this.scrMainLayout.Panel2.Handle);
        }
        /// <summary>
        /// XNA update method
        /// </summary>
        /// <param name="deltaTime">time elapse</param>
        private void Update(float deltaTime)
        {
            cameraPos = cameraFocus + new Vector3(0, 0, zoomFactor);
            cameraPos = Vector3.Transform(cameraPos,
                Matrix.CreateTranslation(-cameraFocus.X, -cameraFocus.Y, -cameraFocus.Z)*
                Matrix.CreateFromQuaternion(Quaternion.CreateFromYawPitchRoll(CamYaw, CamPitch, 0))*
                Matrix.CreateTranslation(cameraFocus.X, cameraFocus.Y, cameraFocus.Z));
            view = Matrix.CreateLookAt(cameraPos,
                    cameraFocus, Vector3.Up);
            
            float fovAngle = 45 * (float)Math.PI / 180;  // convert to radians
            float aspectRatio = this.gfxDevice.Viewport.Width / this.gfxDevice.Viewport.Height;
            float near = 0.1f; // the near clipping plane distance
            float far = BBNMap.getMapRadius(); // the far clipping plane distance
            projection = Matrix.CreatePerspectiveFieldOfView(fovAngle, aspectRatio, near, far);
            BBNMap.UpdateMapContent(deltaTime);
            //Update axis arrow positions
            if (cbxMapItems.SelectedIndex >= 0)
            {
                
                Object item = BBNMap.content[cbxMapItems.SelectedItem.ToString().Split(':')[0].Trim()];
                Vector3 xyz = getObjectPosition(item);
                float x = xyz.X;
                float y = xyz.Y;
                float z = xyz.Z;
                //x-arrow:
                xArrowTop = Algorithms.unprojectPoint(new Vector3(x + 1, y, z), gfxDevice, projection, view);
                xArrowBottom = Algorithms.unprojectPoint(new Vector3(x, y, z), gfxDevice, projection, view);
                //y-arrow:
                yArrowTop = Algorithms.unprojectPoint(new Vector3(x, y + 1, z), gfxDevice, projection, view);
                yArrowBottom = Algorithms.unprojectPoint(new Vector3(x, y, z), gfxDevice, projection, view);
                //z-arrow:
                zArrowTop = Algorithms.unprojectPoint(new Vector3(x, y, z + 1), gfxDevice, projection, view);
                zArrowBottom = Algorithms.unprojectPoint(new Vector3(x, y, z), gfxDevice, projection, view);
            }
        }
        /// <summary>
        /// XNA initialization method
        /// </summary>
        private void Initialize()
        {
            font = contentMgr.Load<SpriteFont>("font");
            blank = new Texture2D(gfxDevice, 1, 1);
            blank.SetData(new[]{Microsoft.Xna.Framework.Graphics.Color.White});
        }
#endregion
#region Map editor variables
        System.Drawing.Point oldCursorPos;
        float movementSpeed = 0.2f;
        Vector2 xArrowTop = Vector2.Zero;
        Vector2 xArrowBottom = Vector2.Zero;
        Vector2 yArrowTop = Vector2.Zero;
        Vector2 yArrowBottom = Vector2.Zero;
        Vector2 zArrowTop = Vector2.Zero;
        Vector2 zArrowBottom = Vector2.Zero;

        bool selectedXMoveLine = false;
        bool selectedYMoveLine = false;
        bool selectedZMoveLine = false;
        bool yDown = false;
        bool pDown = false;
        bool rDown = false;
        bool cDown = false;
#endregion
#region Event Handlers
        bool shouldUpdatePropertiesPage = true; // do not update the properties table while it is already being updated
        private void splitContainer1_Panel2_Resize(object sender, EventArgs e)
        {
            if (gfxDevice != null)
            {
                CreateDevice();
            }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.gfxDevice.Dispose();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            BBNMap.shouldDrawPathNodeConnections = true;
            BBNMap.shouldSendControlerStatesToObjects = false; //in map mode
            //Load toolbox:
            ToolboxLoader.loadContent();

            TreeNode root = this.tvwToolbox.Nodes.Add("Toolbox");
            foreach (ToolboxItem item in ToolboxLoader.toolboxContent)
            {
                TreeNode category = null;
                //Split items into categories:
                foreach (TreeNode n in root.Nodes)
                    if (n.Text == item.type)
                    {
                        category = n;
                        break;
                    }
                if (category == null)
                    category = root.Nodes.Add(item.type);
                
                category.Nodes.Add(item.name);
            }
            root.Expand();
            enableCorrectControls();
        }
        /// <summary>
        /// Method to update the properties table with correct values after a change in object properties
        /// </summary>
        /// <param name="name">id of object or null to find id automatically</param>
        private void updateProperties(String name)
        {
            if (!shouldUpdatePropertiesPage) return;  //do not update while the properties page is already updating
            dgvProperties.Rows.Clear(); //Remove all rows
            //Find selected object id automatically if not specified:
            String selectedItemName = name;
            if (name == "" || name == null)
                if (cbxMapItems.SelectedIndex >= 0)
                    selectedItemName = cbxMapItems.SelectedItem.ToString().Split(':')[0].Trim();
            if (selectedItemName == null || selectedItemName == "") return;
            if (!BBNMap.content.Keys.Contains(selectedItemName)) return;
            Object selectedItem = BBNMap.content[selectedItemName];
            if (selectedItem is MapContent)
            {
                //set id row:
                DataGridViewRow dgr = new DataGridViewRow();
                DataGridViewCell property = new DataGridViewTextBoxCell();
                DataGridViewCell value = new DataGridViewTextBoxCell();
                dgr.Cells.Add(property);
                property.Value = "id";
                dgr.Cells.Add(value);
                value.Value = selectedItemName;
                dgvProperties.Rows.Add(dgr);
                //set all the other properties:
                foreach (String item in (selectedItem as MapContent).getAttributeNames())
                {
                    dgr = new DataGridViewRow();
                    property = new DataGridViewTextBoxCell();
                    value = new DataGridViewTextBoxCell();
                    dgr.Cells.Add(property);
                    property.Value = item;
                    dgr.Cells.Add(value);
                    value.Value = (selectedItem as MapContent).getAttribute(item);
                    dgvProperties.Rows.Add(dgr);
                }
            }
            else if (selectedItem is Marker)
            {
                //set id row:
                DataGridViewRow dgr = new DataGridViewRow();
                DataGridViewCell property = new DataGridViewTextBoxCell();
                DataGridViewCell value = new DataGridViewTextBoxCell();
                dgr.Cells.Add(property);
                property.Value = "id";
                dgr.Cells.Add(value);
                value.Value = selectedItemName;
                dgvProperties.Rows.Add(dgr);
                //position:
                dgr = new DataGridViewRow();
                property = new DataGridViewTextBoxCell();
                value = new DataGridViewTextBoxCell();
                dgr.Cells.Add(property);
                property.Value = "x";
                dgr.Cells.Add(value);
                value.Value = (selectedItem as Marker).Position.X;
                dgvProperties.Rows.Add(dgr);
                dgr = new DataGridViewRow();
                property = new DataGridViewTextBoxCell();
                value = new DataGridViewTextBoxCell();
                dgr.Cells.Add(property);
                property.Value = "y";
                dgr.Cells.Add(value);
                value.Value = (selectedItem as Marker).Position.Y;
                dgvProperties.Rows.Add(dgr);
                dgr = new DataGridViewRow();
                property = new DataGridViewTextBoxCell();
                value = new DataGridViewTextBoxCell();
                dgr.Cells.Add(property);
                property.Value = "z";
                dgr.Cells.Add(value);
                value.Value = (selectedItem as Marker).Position.Z;
                dgvProperties.Rows.Add(dgr);
                //team:
                dgr = new DataGridViewRow();
                property = new DataGridViewTextBoxCell();
                value = new DataGridViewTextBoxCell();
                dgr.Cells.Add(property);
                property.Value = "owningTeam";
                dgr.Cells.Add(value);
                value.Value = (selectedItem as Marker).OwningTeam;
                dgvProperties.Rows.Add(dgr);
            }
            dgvProperties.Update();
        }
        //After the user clicks on the toolbox:
        private void tvwToolbox_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Nodes.Count != 0)
                tvwToolbox.SelectedNode = null;
            else
            {
                MapContent newItem = null;
                foreach (ToolboxItem item in ToolboxLoader.toolboxContent)
                    if (item.name == tvwToolbox.SelectedNode.Text)
                    {
                        newItem = new Drawer(cameraFocus,this.contentMgr,false);
                        newItem.copyPropertiesFromToolboxTemplate(item); //copy default attributes
                        newItem.setAttribute("x", Convert.ToString(cameraFocus.X));
                        newItem.setAttribute("y", Convert.ToString(cameraFocus.Y));
                        newItem.setAttribute("z", Convert.ToString(cameraFocus.Z));
                        break;
                    }
                this.cbxMapItems.SelectedIndex = this.cbxMapItems.Items.Add(newItem.id + " : " + newItem.itemClassName);
                updateProperties(newItem.id);
                tvwToolbox.SelectedNode = null;
            }
            enableCorrectControls();
        }

        private void dgvProperties_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            String att = (String)dgvProperties.CurrentRow.Cells[0].Value;
            String val = (String)dgvProperties.CurrentRow.Cells[1].Value;
            Object selected = BBNMap.content[this.cbxMapItems.SelectedItem.ToString().Split(':')[0].Trim()];
            if (att == "id")
            {
                //Check that id is an appropriate identifier:
                foreach (char c in val)
                    if (!(c >= '0' && c < '9') && !(c >= 'a' && c <= 'z') && !(c >= 'A' && c <= 'Z'))
                    {
                        MessageBox.Show("Id must consist only of 0-9 and a-z and A-Z characters. No spaces", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        if (selected is MapContent)
                            dgvProperties.CurrentRow.Cells[1].Value = (selected as MapContent).id;
                        else if (selected is Marker)
                            dgvProperties.CurrentRow.Cells[1].Value = (selected as Marker).id;
                        return;
                    }
                //id must be unique:
                if (selected is MapContent)
                {
                    if (!(selected as MapContent).setNewId(val))
                    {
                        MessageBox.Show("Id already exists. Set to a unique id.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        dgvProperties.CurrentRow.Cells[1].Value = (selected as MapContent).id;
                    }
                    else //update id:
                    {
                        shouldUpdatePropertiesPage = false;
                        cbxMapItems.Items.RemoveAt(cbxMapItems.SelectedIndex);
                        cbxMapItems.SelectedIndex = cbxMapItems.Items.Add((selected as MapContent).id + " : " + (selected as MapContent).itemClassName);
                        updateProperties((selected as MapContent).id);
                        shouldUpdatePropertiesPage = true;
                    }
                }
                else if (selected is Marker)
                {
                    foreach (Object content in BBNMap.content.Values)
                        if (content is MapContent)
                        {
                            if ((content as MapContent).id == val)
                            {
                                MessageBox.Show("Id already exists. Set to a unique id.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                dgvProperties.CurrentRow.Cells[1].Value = (selected as Marker).id;
                            }
                            else
                            {
                                shouldUpdatePropertiesPage = false;
                                cbxMapItems.Items.RemoveAt(cbxMapItems.SelectedIndex);
                                cbxMapItems.SelectedIndex = cbxMapItems.Items.Add((selected as Marker).id + " : " + (selected as Marker).className);
                                updateProperties((selected as Marker).id);
                                shouldUpdatePropertiesPage = true;
                            }
                        }
                        else if (content is Marker)
                        {
                            if ((content as Marker).id == val)
                            {
                                MessageBox.Show("Id already exists. Set to a unique id.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                dgvProperties.CurrentRow.Cells[1].Value = (selected as Marker).id;
                            }
                            else
                            {
                                shouldUpdatePropertiesPage = false;
                                cbxMapItems.Items.RemoveAt(cbxMapItems.SelectedIndex);
                                cbxMapItems.SelectedIndex = cbxMapItems.Items.Add((selected as Marker).id + " : " + (selected as Marker).className);
                                updateProperties((selected as Marker).id);
                                shouldUpdatePropertiesPage = true;
                            }
                        }  
                }
            }
            else //attribute other than id:
            {
                try //to set otherwise return error and reset value back to old value
                {
                    if (selected is MapContent)
                        (selected as MapContent).setAttribute(att, val);
                    else if (selected is Marker)
                    {
                        if (att == "x")
                          (selected as Marker).Position = new Vector3(Convert.ToSingle(val),(selected as Marker).Position.Y,(selected as Marker).Position.Z);
                        if (att == "y")
                            (selected as Marker).Position = new Vector3((selected as Marker).Position.X,Convert.ToSingle(val), (selected as Marker).Position.Z);
                        if (att == "z")
                            (selected as Marker).Position = new Vector3((selected as Marker).Position.X, (selected as Marker).Position.Y,Convert.ToSingle(val));
                        if (att == "owningTeam")
                        {
                            int team = Convert.ToInt32(val);
                            if (team >= -1)
                                (selected as Marker).OwningTeam = Convert.ToInt32(val);
                            else
                                throw new Exception("Owning team must be -1 if marker is not owned, otherwise it must be the number of the owning team (positive number)");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Value could not be set.\n Reason: "+ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    if (selected is MapContent)
                        dgvProperties.CurrentRow.Cells[1].Value = (selected as MapContent).getAttribute(att);
                    else if (selected is Marker)
                    {
                        if (att == "x")
                            dgvProperties.CurrentRow.Cells[1].Value = (selected as Marker).Position.X;
                        if (att == "y")
                            dgvProperties.CurrentRow.Cells[1].Value = (selected as Marker).Position.Y;
                        if (att == "z")
                            dgvProperties.CurrentRow.Cells[1].Value = (selected as Marker).Position.Z;
                        if (att == "owningTeam")
                            dgvProperties.CurrentRow.Cells[1].Value = (selected as Marker).OwningTeam;
                    }
                }
            }
            
        }
        private void tsbFocus_Click(object sender, EventArgs e)
        {
            if (cbxMapItems.SelectedIndex >= 0)
            {
                Vector3 xyz = getObjectPosition(BBNMap.content[cbxMapItems.SelectedItem.ToString().Split(':')[0].Trim()]);
                this.cameraFocus = xyz;
            }
        }
        /// <summary>
        /// Method to enable correct controls for example when an object is selected
        /// </summary>
        private void enableCorrectControls()
        {
            if (cbxMapItems.SelectedIndex >= 0)
            {
                this.tsbFocus.Enabled = true;
                this.dgvProperties.Enabled = true;
                this.tsbMoveObjectToFocus.Enabled = true;
                this.tsbDeleteObject.Enabled = true;
                if (BBNMap.content[cbxMapItems.SelectedItem.ToString().Split(':')[0].Trim()] is MapContent)
                    this.tsbAddAttribute.Enabled = true;
                else
                    this.tsbAddAttribute.Enabled = false;
            }
            else
            {
                this.tsbFocus.Enabled = false;
                this.tsbMoveObjectToFocus.Enabled = false;
                this.dgvProperties.Enabled = false;
                this.tsbDeleteObject.Enabled = false;
                this.tsbAddAttribute.Enabled = false;
            }
            if (cbxMapItems.Items.Count == 0)
                cbxMapItems.Enabled = false;
            else
                cbxMapItems.Enabled = true;
        }

        private void tsbSetCameraPos_Click(object sender, EventArgs e)
        {
            String val = Microsoft.VisualBasic.Interaction.InputBox("Enter X,Y,Z coords as shown separated by commas", "New coordinate", Convert.ToString(this.cameraFocus.X) + "," +
                Convert.ToString(this.cameraFocus.Y) + "," + Convert.ToString(this.cameraFocus.Z), Width / 2, Height / 2);
            if (val == "") return;
            String[] splitted = val.Split(',');
            try
            {
                this.cameraFocus.X = Convert.ToSingle(splitted[0].Trim());
                this.cameraFocus.Y = Convert.ToSingle(splitted[1].Trim());
                this.cameraFocus.Z = Convert.ToSingle(splitted[2].Trim());
            }
            catch (Exception)
            {
                MessageBox.Show("Could not set value. Ensure that you have entered 3 numeric values, separated by commas", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void tsbMoveObjectToFocus_Click(object sender, EventArgs e)
        {
            Object item = BBNMap.content[cbxMapItems.SelectedItem.ToString().Split(':')[0].Trim()];
            if (item is MapContent)
            {
                (item as MapContent).setAttribute("x", Convert.ToString(cameraFocus.X));
                (item as MapContent).setAttribute("y", Convert.ToString(cameraFocus.Y));
                (item as MapContent).setAttribute("z", Convert.ToString(cameraFocus.Z));
                updateProperties((item as MapContent).id);
            }
            else if (item is Marker)
            {
                (item as Marker).Position = cameraFocus;
                updateProperties((item as Marker).id);
            }
        }
        private void tsbDeleteObject_Click(object sender, EventArgs e)
        {
            Object item = BBNMap.content[cbxMapItems.SelectedItem.ToString().Split(':')[0].Trim()];
            if (item is Node)
                (item as Node).disconnectAllEdges();
            if (item is MapContent)
                BBNMap.content.Remove((item as MapContent).id);
            else if (item is Marker)
                BBNMap.content.Remove((item as Marker).id);
            cbxMapItems.Items.Remove(cbxMapItems.SelectedItem);
            cbxMapItems.SelectedIndex = -1;
            updateProperties(null);
            enableCorrectControls();
        }

        private void tsbAddPathNode_Click(object sender, EventArgs e)
        {
            Node n = new Node(cameraFocus,-1);
            n.className = "PathNode";
            n.type = "Marker";
            String id = Convert.ToString(BBNMap.content.Count);
            for (int i = 0; BBNMap.content.Keys.Contains(id); i++)
            {
                id = Convert.ToString(BBNMap.content.Count + i);
            }
            n.id = id;
            BBNMap.content.Add(n.id, n);
            cbxMapItems.SelectedIndex = cbxMapItems.Items.Add(n.id + " : PathNode");
        }

        private void cbxMapItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateProperties(null);
            enableCorrectControls();
        }

        private void tsbConnectPathNodes_Click(object sender, EventArgs e)
        {
            try // to get 2 node ids and a weight. Throws error if validation fails
            {
                Node n1 = null, n2 = null;
                String id = Microsoft.VisualBasic.Interaction.InputBox("Specify first path node's id", "Node 1 ID", "", Width / 2, Height / 2);
                if (id == "") return;
                Object i1 = BBNMap.content[id];

                if (i1 is Node)
                    n1 = i1 as Node;
                else throw new Exception("Item is not a path node");
                String id2 = Microsoft.VisualBasic.Interaction.InputBox("Specify second path node's id", "Node 2 ID", "", Width / 2, Height / 2);
                if (id2 == "") return;
                Object i2 = BBNMap.content[id2];
                if (i1 == i2)
                    throw new Exception("The two nodes must be two different nodes");
                if (i2 is Node)
                    n2 = i2 as Node;
                else throw new Exception("Item is not a path node");
                for (int i = 0; i < n1.getEdgeCount(); i++)
                {
                    Edge edge = n1.getEdge(i);
                    if (edge.node1 == n2 || edge.node2 == n2)
                        if (MessageBox.Show("A connection already exists. Disconnect nodes?", "Disconnect?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            n1.disconnectFromNode(n2);
                            MessageBox.Show("Disconnected nodes", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                        else
                        {
                            MessageBox.Show("Operation aborted by user", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                }
                String floatVal = Microsoft.VisualBasic.Interaction.InputBox("Specify weight (positive decimal value) for connection. A higher value will be more unfavorable for the AI", "Edge Weight", "", Width / 2, Height / 2);
                if (floatVal == "") return;
                float weightForConnection = Convert.ToSingle(floatVal);
                if (weightForConnection < 0)
                    throw new Exception("Weight must be positive");
                n1.connectToNode(n2, weightForConnection);
                MessageBox.Show("Connection established successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not connect nodes\nReason: " + ex.Message, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void tsbAddSpawnPoint_Click(object sender, EventArgs e)
        {
            SpawnPoint s = new SpawnPoint(cameraFocus,-1);
            s.className = "SpawnPoint";
            s.type = "Marker";
            String id = Convert.ToString(BBNMap.content.Count);
            for (int i = 0; BBNMap.content.Keys.Contains(id); i++)
            {
                id = Convert.ToString(BBNMap.content.Count + i);
            }
            s.id = id;
            BBNMap.content.Add(s.id, s);
            cbxMapItems.SelectedIndex = cbxMapItems.Items.Add(s.id + " : SpawnPoint");
        }

        private void tsbAddAttribute_Click(object sender, EventArgs e)
        {
            //Sets up a new attribute
            Object item = BBNMap.content[cbxMapItems.SelectedItem.ToString().Split(':')[0].Trim()];
            try //check that attribute and its value is appropriate
            {
                String name = Microsoft.VisualBasic.Interaction.InputBox("Specify attribute's name", "New attribute", "", Width / 2, Height / 2);
                if (name == null) return;
                if (name == "")
                    throw new Exception("Name field cannot be empty");
                if ((item as MapContent).getAttributeNames().Contains(name) || name == "id")
                    throw new Exception("Such an attribute already exists");
                String val = Microsoft.VisualBasic.Interaction.InputBox("Specify attribute's value", "New attribute", "", Width / 2, Height / 2);
                if (val == null) return;
                (item as MapContent).addAttribute(name, val);
                updateProperties((item as MapContent).id);
                MessageBox.Show("Attribute added successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not connect nodes\nReason: " + ex.Message, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void tsbOpenGuide_Click(object sender, EventArgs e)
        {
            if (!System.IO.File.Exists("guide.html"))
                System.IO.File.WriteAllText("guide.html", Editor.Properties.Resources.Guide);
            System.Diagnostics.Process.Start("guide.html");
        }

        private void tsbSkybox_Click(object sender, EventArgs e)
        {
            //get textures and then texture repeat counters for all sides of the skybox:
            String Texture = Microsoft.VisualBasic.Interaction.InputBox("Specify the texture name", "Texture name", "Images/Starfield", Width / 2, Height / 2);
            if (Texture == "") return;
            String Repeat = Microsoft.VisualBasic.Interaction.InputBox("Specify the texture repeat", "Repeat count", "10.0", Width / 2, Height / 2);
            if (Repeat == "") return;
            BBNMap.SetUpSkyBox(gfxDevice, contentMgr, Texture, Repeat);
        }
        /// <summary>
        /// Method to clear map content, after confirming with the user
        /// </summary>
        /// <returns>true if user wants to clear map</returns>
        private bool clearContent()
        {
            if (MessageBox.Show("All unsaved progress will be lost. Continue?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                BBNMap.clearMap(contentMgr);
                this.Initialize(); //INIT: XNA
                cbxMapItems.Items.Clear();
                updateProperties(null);
                enableCorrectControls();
                return true;
            }
            return false;
        }
        private void tsbNew_Click(object sender, EventArgs e)
        {
            clearContent();
            BBNMap.SetUpSkyBox(gfxDevice, contentMgr, "", "1.0");
        }
        private void tsbOpen_Click(object sender, EventArgs e)
        {
            if (ofdMainWindow.ShowDialog() != DialogResult.Cancel)
            {
                if (clearContent())
                    try
                    {
                        BBNMap.loadMap(ofdMainWindow.FileName, contentMgr, gfxDevice);
                        foreach (Object item in BBNMap.content.Values)
                            if (item is MapContent)
                                cbxMapItems.Items.Add((item as MapContent).id + " : " + (item as MapContent).itemClassName);
                            else if (item is Marker)
                                cbxMapItems.Items.Add((item as Marker).id + " : " + (item as Marker).className);
                        if (cbxMapItems.Items.Count > 0)
                            cbxMapItems.SelectedIndex = 0;
                        updateProperties(null);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Could not load map\nReason: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
            }
        }
        private void tsbSave_Click(object sender, EventArgs e)
        {
            if (sfdMainWindow.ShowDialog() != DialogResult.Cancel)
            {
                BBNMap.saveMap(sfdMainWindow.FileName);
            }
        }
        private void tsbSetMapRadius_Click(object sender, EventArgs e)
        {
            try
            {
                String val = Microsoft.VisualBasic.Interaction.InputBox("Specify new radius", "Map Radius", Convert.ToString(BBNMap.getMapRadius()), Width / 2, Height / 2);
                if (val == "")
                    return; //abort
                BBNMap.setMapSize(Convert.ToSingle(val),contentMgr,gfxDevice);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not set radius.\nReason: " + ex.Message,"Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }
        private void tsbPlayerSpawnpoint_Click(object sender, EventArgs e)
        {
            SpawnPoint s = new SpawnPoint(cameraFocus, -1);
            s.className = "PlayerSpawnPoint";
            s.type = "Marker";
            String id = Convert.ToString(BBNMap.content.Count);
            for (int i = 0; BBNMap.content.Keys.Contains(id); i++)
            {
                id = Convert.ToString(BBNMap.content.Count + i);
            }
            s.id = id;
            BBNMap.content.Add(s.id, s);
            cbxMapItems.SelectedIndex = cbxMapItems.Items.Add(s.id + " : PlayerSpawnPoint");
        }
#endregion
#region 3D controls event handlers
        private void scrMainLayout_Panel2_Click(object sender, EventArgs e)
        {
            //Panel must be in focus for 3d controls to work properly
            scrMainLayout.Panel2.Focus();
        }
        private void scrMainLayoutPanel2_KeyDown(Object sender, KeyEventArgs e)
        {
            //check keys y,p,r,c,add and subtract. Indicate holding value for later use when we combine it with mouse movement
            if (scrMainLayout.Panel2.Focused)
            {
                switch (e.KeyCode)
                {
                    case System.Windows.Forms.Keys.Add:
                        this.movementSpeed += 0.02f;
                        break;
                    case System.Windows.Forms.Keys.Subtract:
                        this.movementSpeed -= 0.02f;
                        break;
                    case System.Windows.Forms.Keys.Y:
                        if (!(yDown || pDown || rDown || selectedXMoveLine || selectedYMoveLine || selectedZMoveLine || cDown) && cbxMapItems.SelectedIndex >= 0)
                        {
                            Object item = BBNMap.content[cbxMapItems.SelectedItem.ToString().Split(':')[0].Trim()];
                            if (!(item is Marker))
                                yDown = true;
                        }
                        break;
                    case System.Windows.Forms.Keys.P:
                        if (!(yDown || pDown || rDown || selectedXMoveLine || selectedYMoveLine || selectedZMoveLine || cDown) && cbxMapItems.SelectedIndex >= 0)
                        {
                            Object item = BBNMap.content[cbxMapItems.SelectedItem.ToString().Split(':')[0].Trim()];
                            if (!(item is Marker))
                                pDown = true;
                        }
                        break;
                    case System.Windows.Forms.Keys.R:
                        if (!(yDown || pDown || rDown || selectedXMoveLine || selectedYMoveLine || selectedZMoveLine || cDown) && cbxMapItems.SelectedIndex >= 0)
                        {
                            Object item = BBNMap.content[cbxMapItems.SelectedItem.ToString().Split(':')[0].Trim()];
                            if (!(item is Marker))
                                rDown = true;
                        }
                        break;
                    case System.Windows.Forms.Keys.C:
                        if (!(yDown || pDown || rDown || selectedXMoveLine || selectedYMoveLine || selectedZMoveLine || cDown) && cbxMapItems.SelectedIndex >= 0)
                        {
                            Object item = BBNMap.content[cbxMapItems.SelectedItem.ToString().Split(':')[0].Trim()];
                            if (item is Node)
                                cDown = true;
                        }
                        break;
                     // camera movement controls:
                    case System.Windows.Forms.Keys.NumPad8:
                        cameraFocus.Z -= movementSpeed;
                        break;
                    case System.Windows.Forms.Keys.NumPad2:
                        cameraFocus.Z += movementSpeed;
                        break;
                    case System.Windows.Forms.Keys.NumPad6:
                        cameraFocus.X += movementSpeed;
                        break;
                    case System.Windows.Forms.Keys.NumPad4:
                        cameraFocus.X -= movementSpeed;
                        break;
                    case System.Windows.Forms.Keys.PageUp:
                        cameraFocus.Y += movementSpeed;
                        break;
                    case System.Windows.Forms.Keys.PageDown:
                        cameraFocus.Y -= movementSpeed;
                        break;
                }//movement speed should be positive:
                if (this.movementSpeed < 0.01)
                    this.movementSpeed = 0.01f;
            }
            this.processXNA();
        }
        private void scrMainLayoutPanel2_KeyUp(Object sender, KeyEventArgs e)
        {
            //Check for a release of y,p,r and c, as this will change mode of 3d mouse input
            if (scrMainLayout.Panel2.Focused)
            {
                switch (e.KeyCode)
                {
                    case System.Windows.Forms.Keys.Y:
                        yDown = false;
                        break;
                    case System.Windows.Forms.Keys.P:
                        pDown = false;
                        break;
                    case System.Windows.Forms.Keys.R:
                        rDown = false;
                        break;
                    case System.Windows.Forms.Keys.C:
                        cDown = false;
                        break;
                }
            }
            this.processXNA();
        }
        private void scrMainLayoutPanel2_MouseWheel(Object sender, MouseEventArgs e)
        {
            //sets zoom level:
            if (scrMainLayout.Panel2.Focused)
            {
                if (e.Delta > 0)
                    zoomFactor = zoomFactor * 0.88f;
                else if (e.Delta < 0)
                    zoomFactor = zoomFactor * (1.12f);
            }
        }

        private void scrMainLayout_Panel2_MouseMove(object sender, MouseEventArgs e)
        {
            //if the mouse's right button is pressed while its moved we set the camera rotation
            if (e.Button == MouseButtons.Right)
            {
                CamYaw += (e.X - oldCursorPos.X) / (float)scrMainLayout.Panel2.Width * (float)Math.PI * 2;
                CamPitch += (e.Y - oldCursorPos.Y) / (float)scrMainLayout.Panel2.Height * (float)Math.PI;
                //Test for overflow:
                if (CamYaw >= Math.PI * 2)
                    CamYaw -= (float)Math.PI * 2;
                else if (CamYaw <= -Math.PI * 2)
                    CamYaw += (float)Math.PI * 2;
                if (CamPitch <= -Math.PI / 2)
                    CamPitch = (float)-Math.PI / 2 + 0.000001f;
                else if (CamPitch >= Math.PI / 2)
                    CamPitch = (float)Math.PI / 2 - 0.000001f;
                this.processXNA();
            }//else if the left mouse button is pressed and the mouse is moved:
                //1. The user wants to move an object using the 3 movement axis (so check how close the click was
                //2. The user wants to select an object
            else if (e.Button == MouseButtons.Left)
            {
                if (cbxMapItems.SelectedIndex >= 0)
                {
                    Object item = BBNMap.content[cbxMapItems.SelectedItem.ToString().Split(':')[0].Trim()];
                    try
                    {
                        //We need the component the mouse is moved along the axis so get the component onto axis value (ref. Steward Calculus, concepts and contexts 4th. Chapter 9)
                        if (selectedXMoveLine)
                        {
                            float compMouseOntoAxis = Vector2.Dot(xArrowTop - xArrowBottom, new Vector2(e.X - oldCursorPos.X, e.Y - oldCursorPos.Y)) / (xArrowTop - xArrowBottom).Length();
                            if (item is MapContent)
                            {
                                (item as MapContent).setAttribute("x",
                                    Convert.ToString(Convert.ToSingle((item as MapContent).getAttribute("x")) + compMouseOntoAxis * movementSpeed * 0.1));
                                updateProperties((item as MapContent).id);
                            }
                            else if (item is Marker)
                            {
                                Vector3 tempPos = (item as Marker).Position;
                                tempPos.X += (float)(compMouseOntoAxis * movementSpeed * 0.1);
                                (item as Marker).Position = tempPos;
                                updateProperties((item as Marker).id);
                            }
                        }
                        else if (selectedYMoveLine)
                        {
                            float compMouseOntoAxis = Vector2.Dot(yArrowTop - yArrowBottom, new Vector2(e.X - oldCursorPos.X, e.Y - oldCursorPos.Y)) / (yArrowTop - yArrowBottom).Length();
                            if (item is MapContent)
                            {
                                (item as MapContent).setAttribute("y",
                                    Convert.ToString(Convert.ToSingle((item as MapContent).getAttribute("y")) + compMouseOntoAxis * movementSpeed * 0.1));
                                updateProperties((item as MapContent).id);
                            }
                            else if (item is Marker)
                            {
                                Vector3 tempPos = (item as Marker).Position;
                                tempPos.Y += (float)(compMouseOntoAxis * movementSpeed * 0.1);
                                (item as Marker).Position = tempPos;
                                updateProperties((item as Marker).id);
                            }
                        }
                        else if (selectedZMoveLine)
                        {
                            float compMouseOntoAxis = Vector2.Dot(zArrowTop - zArrowBottom, new Vector2(e.X - oldCursorPos.X, e.Y - oldCursorPos.Y)) / (zArrowTop - zArrowBottom).Length();
                            if (item is MapContent)
                            {
                                (item as MapContent).setAttribute("z",
                                    Convert.ToString(Convert.ToSingle((item as MapContent).getAttribute("z")) + compMouseOntoAxis * movementSpeed * 0.1));
                                updateProperties((item as MapContent).id);
                            }
                            else if (item is Marker)
                            {
                                Vector3 tempPos = (item as Marker).Position;
                                tempPos.Z += (float)(compMouseOntoAxis * movementSpeed * 0.1);
                                (item as Marker).Position = tempPos;
                                updateProperties((item as Marker).id);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Could not move object.\nReason:" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        selectedXMoveLine = false;
                        selectedYMoveLine = false;
                        selectedZMoveLine = false;
                    }
                    this.processXNA();
                }
            }
            //Deals with rotation (y,p or r is held down)
            if (cbxMapItems.SelectedIndex >= 0)
            {
                Object item = BBNMap.content[cbxMapItems.SelectedItem.ToString().Split(':')[0].Trim()];
                if (item is MapContent)
                {
                    if (rDown)
                    {
                        float val = -(float)Math.Atan2(e.Y - this.scrMainLayout.Panel2.Height / 2, e.X - this.scrMainLayout.Panel2.Width / 2) * 180 / (float)Math.PI - 90;
                        (item as MapContent).setAttribute("roll", Convert.ToString(val));
                        updateProperties((item as MapContent).id);
                    }
                    else if (yDown)
                    {
                        float val = -(float)Math.Atan2(e.Y - this.scrMainLayout.Panel2.Height / 2, e.X - this.scrMainLayout.Panel2.Width / 2) * 180 / (float)Math.PI - 90;
                        (item as MapContent).setAttribute("yaw", Convert.ToString(val));
                        updateProperties((item as MapContent).id);
                    }
                    else if (pDown)
                    {
                        float val = -(float)Math.Atan2(e.Y - this.scrMainLayout.Panel2.Height / 2, e.X - this.scrMainLayout.Panel2.Width / 2) * 180 / (float)Math.PI - 90;
                        (item as MapContent).setAttribute("pitch", Convert.ToString(val));
                        updateProperties((item as MapContent).id);
                    }
                }
            }
            oldCursorPos = e.Location;
        }
        private void scrMainLayout_Panel2_MouseDown(object sender, MouseEventArgs e)
        {
            scrMainLayout.Panel2.Focus();
            if (e.Button == MouseButtons.Right)
                oldCursorPos = e.Location;
            else if (e.Button == MouseButtons.Left && !(yDown || rDown || pDown)) //checks if the user has selected one of the movement axis
            {
                oldCursorPos = e.Location;
                if (cbxMapItems.SelectedIndex >= 0)
                {
                    if (Algorithms.clickedNearRay(e.X, e.Y, this.xArrowBottom, this.xArrowTop))
                        selectedXMoveLine = true;
                    else if (Algorithms.clickedNearRay(e.X, e.Y, this.yArrowBottom, this.yArrowTop))
                        selectedYMoveLine = true;
                    else if (Algorithms.clickedNearRay(e.X, e.Y, this.zArrowBottom, this.zArrowTop))
                        selectedZMoveLine = true;
                }
                if (!(selectedXMoveLine || selectedYMoveLine || selectedZMoveLine)) //otherwise check if the user selected some object
                {
                    Vector3 vNear = gfxDevice.Viewport.Unproject(new Vector3(e.X, e.Y, 0f),
                        projection, view, Matrix.CreateTranslation(0,0,0));
                    Vector3 vFar = gfxDevice.Viewport.Unproject(new Vector3(e.X, e.Y, this.farClip),
                        projection, view, Matrix.CreateTranslation(0, 0, 0));
                    float minDist = 0;
                    Drawer closestObj = null;
                    foreach (Object item in BBNMap.content.Values)
                    {
                        if (item is Drawer)
                        {
                            float dist = 0;
                            if (CollisionDetectionHelper.rayIntersect(vNear, vFar, (item as Drawer).model, (item as Drawer).world, out dist))
                                if (closestObj == null || dist < minDist)
                                {
                                    minDist = dist;
                                    closestObj = (item as Drawer);
                                }
                        }
                        else if (item is Marker)
                        {
                            float dist = 0;
                            Drawer tempD = new Drawer((item as Marker).Position, contentMgr,true);
                            tempD.addAttribute("modelName", BBNMap.MODEL_USED_FOR_MAP_EDITOR);
                            tempD.itemClassName = (item as Marker).className;
                            tempD.setNewId((item as Marker).id);
                            tempD.addAttribute("x", Convert.ToString((item as Marker).Position.X));
                            tempD.addAttribute("y", Convert.ToString((item as Marker).Position.Y));
                            tempD.addAttribute("z", Convert.ToString((item as Marker).Position.Z));
                            tempD.addAttribute("scaleX", "0.25");
                            tempD.addAttribute("scaleY", "0.25");
                            tempD.addAttribute("scaleZ", "0.25");
                            tempD.addAttribute("yaw", "0");
                            tempD.addAttribute("pitch", "0");
                            tempD.addAttribute("roll", "0");
                            tempD.onAttributeChange();
                            tempD.update(new KeyboardState(), new GamePadState(), new GamePadState());
                            if (CollisionDetectionHelper.rayIntersect(vNear, vFar, tempD.model, tempD.world, out dist))
                                if (closestObj == null || dist < minDist)
                                {
                                    minDist = dist;
                                    closestObj = tempD;
                                }
                        }
                    }
                    //get the closest object (first in depth sort):
                    if (closestObj != null)
                    {
                        //connect path nodes (if "c" is held down and a path node is selected then connect it to the node the user clicked on if it is another path node):
                        if (closestObj.itemClassName == "PathNode" && cDown)
                        {
                            Object item = BBNMap.content[cbxMapItems.SelectedItem.ToString().Split(':')[0].Trim()];
                            Node picked = BBNMap.content[closestObj.id] as Node;
                            bool found = false;
                            for (int i = 0; i < (item as Node).getEdgeCount(); ++i) 
                            {
                                Edge edge = (item as Node).getEdge(i);
                                if (edge.node1 == picked || edge.node2 == picked)
                                {
                                    (item as Node).disconnectFromNode(picked);
                                    found = true;
                                    break;
                                }
                            }
                            if (!found && closestObj.id != (item as Node).id)
                                (item as Node).connectToNode(picked, 0);
                        }
                        else if (!cDown) //otherwise simply select object
                        {
                            foreach (object o in cbxMapItems.Items)
                                if (o.ToString().Split(':')[0].Trim() == closestObj.id)
                                    cbxMapItems.SelectedItem = o;
                            this.updateProperties(closestObj.id);
                        }
                    }
                }
            }
        }

        private void scrMainLayout_Panel2_MouseUp(object sender, MouseEventArgs e)
        {
            //must check if the user released the axis if they were selected:
            if (e.Button == MouseButtons.Left)
            {
                selectedXMoveLine = false;
                selectedYMoveLine = false;
                selectedZMoveLine = false;
            }
        }
#endregion
        /// <summary>
        /// Method to get object position
        /// </summary>
        /// <param name="item">Either a marker or a Map Content item</param>
        /// <returns>position</returns>
        public Vector3 getObjectPosition(Object item)
        {
            float x = 0;
            float y = 0;
            float z = 0;
            if (item is MapContent)
            {
                x = Convert.ToSingle((item as MapContent).getAttribute("x"));
                y = Convert.ToSingle((item as MapContent).getAttribute("y"));
                z = Convert.ToSingle((item as MapContent).getAttribute("z"));
            }
            else if (item is Marker)
            {
                x = (item as Marker).Position.X;
                y = (item as Marker).Position.Y;
                z = (item as Marker).Position.Z;

            }
            return new Vector3(x, y, z);
        }
        /// <summary>
        /// Method to obtain yaw pitch and roll
        /// </summary>
        /// <param name="item">A Map Content Item</param>
        /// <returns>yaw, pitch and roll as vector3</returns>
        public Vector3 getYawPitchRoll(Object item)
        {
            float yaw = 0;
            float pitch = 0;
            float roll = 0;
            if (item is MapContent)
            {
                yaw = Convert.ToSingle((item as MapContent).getAttribute("yaw"));
                pitch = Convert.ToSingle((item as MapContent).getAttribute("pitch"));
                roll = Convert.ToSingle((item as MapContent).getAttribute("roll"));
            }
            return new Vector3(yaw, pitch, roll);
        }
    }
}
