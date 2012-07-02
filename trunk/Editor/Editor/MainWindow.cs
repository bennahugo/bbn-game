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
using BBN_Game.Objects;
using BBN_Game.Map;
using BBN_Game.Utils;
using BBN_Game.AI;
//include XNA:
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;


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
            gfxDevice = gfxService.GraphicsDevice;
            spriteBatch = new SpriteBatch(gfxDevice);
            basicEffect = new BasicEffect(gfxDevice, null);
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
            presentation.BackBufferWidth = this.scrMainLayout.Panel2.Width;
            presentation.BackBufferHeight = this.scrMainLayout.Panel2.Height;
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
            //Setup graphics device:
            gfxDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;
            gfxDevice.RenderState.DepthBufferEnable = true;
            gfxDevice.RenderState.DepthBufferFunction = CompareFunction.LessEqual;
            gfxDevice.RenderState.DepthBufferWriteEnable = true;
            gfxDevice.RenderState.FillMode = FillMode.Solid;
            ClearOptions options = ClearOptions.Target | ClearOptions.DepthBuffer;
            Microsoft.Xna.Framework.Graphics.Color clearColor = Microsoft.Xna.Framework.Graphics.Color.Black;
            float depth = 1;
            int stencil = 128;
            //Draw all objects in map:
            gfxDevice.Clear(options, clearColor, depth, stencil);
            BBNMap.DrawMap(gfxDevice, projection, view, lightsSetup, fogColor, fogSetup, basicEffect, cameraPos);
            //Draw 3D lines:
            if (selectedXMoveLine || selectedYMoveLine || selectedZMoveLine)
            {
                MapContent item = BBNMap.content[cbxMapItems.SelectedItem.ToString().Split(':')[0].Trim()];
                float x = Convert.ToSingle(item.getAttribute("x"));
                float y = Convert.ToSingle(item.getAttribute("y"));
                float z = Convert.ToSingle(item.getAttribute("z"));
                if (selectedXMoveLine)
                    Algorithms.Draw3DLine(Microsoft.Xna.Framework.Graphics.Color.White, new Vector3(-farClip, y, z), new Vector3(farClip, y, z), basicEffect, gfxDevice, projection, view, Matrix.Identity);
                if (selectedYMoveLine)
                    Algorithms.Draw3DLine(Microsoft.Xna.Framework.Graphics.Color.White, new Vector3(x,-farClip, z), new Vector3(x,farClip, z), basicEffect, gfxDevice, projection, view, Matrix.Identity);
                if (selectedZMoveLine)
                    Algorithms.Draw3DLine(Microsoft.Xna.Framework.Graphics.Color.White, new Vector3(x,y,-farClip), new Vector3(x,y,farClip), basicEffect, gfxDevice, projection, view, Matrix.Identity);
            }
            //Draw 2d effects over 3d environment:
            spriteBatch.Begin();
            //Draw movement control lines if an object was selected
            if (cbxMapItems.SelectedIndex >= 0 && !(yDown || pDown || rDown || cDown))
            {
                if (!(selectedXMoveLine || selectedYMoveLine || selectedZMoveLine))
                {
                    Algorithms.Draw2DLine(2, Microsoft.Xna.Framework.Graphics.Color.Green, xArrowBottom, xArrowTop, spriteBatch, blank);
                    Algorithms.Draw2DLine(2, Microsoft.Xna.Framework.Graphics.Color.Red, yArrowBottom, yArrowTop, spriteBatch, blank);
                    Algorithms.Draw2DLine(2, Microsoft.Xna.Framework.Graphics.Color.Blue, zArrowBottom, zArrowTop, spriteBatch, blank);
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
                MapContent item = BBNMap.content[cbxMapItems.SelectedItem.ToString().Split(':')[0].Trim()];
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
            //Swap buffers:
            gfxDevice.Present(new Microsoft.Xna.Framework.Rectangle(this.scrMainLayout.Panel2.ClientRectangle.X,this.scrMainLayout.Panel2.ClientRectangle.Y,
                                                                    this.scrMainLayout.Panel2.ClientRectangle.Width,this.scrMainLayout.Panel2.ClientRectangle.Height),
                                 null,this.scrMainLayout.Panel2.Handle);
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
            float near = gfxDevice.Viewport.MinDepth; // the near clipping plane distance
            float far = gfxDevice.Viewport.MaxDepth; // the far clipping plane distance
            projection = Matrix.CreatePerspectiveFieldOfView(fovAngle, aspectRatio, near, far);
            BBNMap.UpdateMapContent(deltaTime);
            //Update axis arrow positions
            if (cbxMapItems.SelectedIndex >= 0)
            {
                MapContent item = BBNMap.content[cbxMapItems.SelectedItem.ToString().Split(':')[0].Trim()];
                float x = Convert.ToSingle(item.getAttribute("x"));
                float y = Convert.ToSingle(item.getAttribute("y"));
                float z = Convert.ToSingle(item.getAttribute("z"));
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
            MapContent selectedItem = BBNMap.content[selectedItemName];
            //set id row:
            DataGridViewRow dgr = new DataGridViewRow();
            DataGridViewCell property = new DataGridViewTextBoxCell();
            DataGridViewCell value = new DataGridViewTextBoxCell();
            dgr.Cells.Add(property);
            property.Value = "id";
            dgr.Cells.Add(value);
            value.Value = selectedItemName;
            dgvProperties.Rows.Add(dgr);
            ToolboxLoader.loadContent();
            //set all the other properties:
            foreach (String item in selectedItem.getAttributeNames())
            {
                dgr = new DataGridViewRow();
                property = new DataGridViewTextBoxCell();
                value = new DataGridViewTextBoxCell();
                dgr.Cells.Add(property);
                property.Value = item;
                dgr.Cells.Add(value);
                value.Value = selectedItem.getAttribute(item);
                dgvProperties.Rows.Add(dgr);
                ToolboxLoader.loadContent();
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
                        if (item.movableObject)
                            newItem = new DynamicObject(cameraFocus,this.contentMgr);
                        else
                            newItem = new StaticObject(cameraFocus, this.contentMgr);
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
            MapContent selected = BBNMap.content[this.cbxMapItems.SelectedItem.ToString().Split(':')[0].Trim()];
            if (att == "id")
            {
                //Check that id is an appropriate identifier:
                foreach (char c in val)
                    if (!(c >= '0' && c < '9') && !(c >= 'a' && c <= 'z') && !(c >= 'A' && c <= 'Z'))
                    {
                        MessageBox.Show("Id must consist only of 0-9 and a-z and A-Z characters. No spaces", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        dgvProperties.CurrentRow.Cells[1].Value = selected.id;
                        return;
                    }
                //id must be unique:
                if (!selected.setNewId(val))
                {
                    MessageBox.Show("Id already exists. Set to a unique id.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    dgvProperties.CurrentRow.Cells[1].Value = selected.id;
                }
                else //update id:
                {
                    shouldUpdatePropertiesPage = false;
                    cbxMapItems.Items.RemoveAt(cbxMapItems.SelectedIndex);
                    cbxMapItems.SelectedIndex = cbxMapItems.Items.Add(selected.id + " : " + selected.itemClassName);
                    updateProperties(selected.id);
                    shouldUpdatePropertiesPage = true;
                }
            }
            else //attribute other than id:
            {
                try //to set otherwise return error and reset value back to old value
                {
                    selected.setAttribute(att, val);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Value could not be set.\n Reason: "+ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    dgvProperties.CurrentRow.Cells[1].Value = selected.getAttribute(att);
                }
            }
            
        }
        private void tsbFocus_Click(object sender, EventArgs e)
        {
            if (cbxMapItems.SelectedIndex >= 0)
            {
                MapContent item = BBNMap.content[cbxMapItems.SelectedItem.ToString().Split(':')[0].Trim()];
                this.cameraFocus.X = Convert.ToSingle(item.getAttribute("x"));
                this.cameraFocus.Y = Convert.ToSingle(item.getAttribute("y"));
                this.cameraFocus.Z = Convert.ToSingle(item.getAttribute("z"));
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
                this.tsbAddAttribute.Enabled = true;
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
            MapContent item = BBNMap.content[cbxMapItems.SelectedItem.ToString().Split(':')[0].Trim()];
            item.setAttribute("x", Convert.ToString(cameraFocus.X));
            item.setAttribute("y", Convert.ToString(cameraFocus.Y));
            item.setAttribute("z", Convert.ToString(cameraFocus.Z));
        }
        private void tsbDeleteObject_Click(object sender, EventArgs e)
        {
            MapContent item = BBNMap.content[cbxMapItems.SelectedItem.ToString().Split(':')[0].Trim()];
            if (item is Node)
                (item as Node).disconnectAllEdges();
            BBNMap.content.Remove(item.id);
            cbxMapItems.Items.Remove(cbxMapItems.SelectedItem);
            cbxMapItems.SelectedIndex = -1;
            updateProperties(null);
            enableCorrectControls();
        }

        private void tsbAddPathNode_Click(object sender, EventArgs e)
        {
            Node n = new Node(cameraFocus, contentMgr);
            n.setAttribute("scaleX", Convert.ToString(0.25f));
            n.setAttribute("scaleY", Convert.ToString(0.25f));
            n.setAttribute("scaleZ", Convert.ToString(0.25f));
            n.setAttribute("pitch", Convert.ToString(-90f));
            n.itemClassName = "PathNode";
            n.type = "Marker";
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
                MapContent i1 = BBNMap.content[id];

                if (i1 is Node)
                    n1 = i1 as Node;
                else throw new Exception("Item is not a path node");
                String id2 = Microsoft.VisualBasic.Interaction.InputBox("Specify second path node's id", "Node 2 ID", "", Width / 2, Height / 2);
                if (id2 == "") return;
                MapContent i2 = BBNMap.content[id2];
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
            SpawnPoint s = new SpawnPoint(cameraFocus, contentMgr);
            s.addAttribute("team", "Unset");
            s.setAttribute("scaleX", Convert.ToString(0.25f));
            s.setAttribute("scaleY", Convert.ToString(0.25f));
            s.setAttribute("scaleZ", Convert.ToString(0.25f));
            s.itemClassName = "SpawnPoint";
            s.type = "Marker";
            cbxMapItems.SelectedIndex = cbxMapItems.Items.Add(s.id + " : SpawnPoint");
        }

        private void tsbAddAttribute_Click(object sender, EventArgs e)
        {
            //Sets up a new attribute
            MapContent item = BBNMap.content[cbxMapItems.SelectedItem.ToString().Split(':')[0].Trim()];
            try //check that attribute and its value is appropriate
            {
                String name = Microsoft.VisualBasic.Interaction.InputBox("Specify attribute's name", "New attribute", "", Width / 2, Height / 2);
                if (name == null) return;
                if (name == "")
                    throw new Exception("Name field cannot be empty");
                if (item.getAttributeNames().Contains(name))
                    throw new Exception("Such an attribute already exists");
                String val = Microsoft.VisualBasic.Interaction.InputBox("Specify attribute's value", "New attribute", "", Width / 2, Height / 2);
                if (val == null) return;
                item.addAttribute(name, val);
                updateProperties(item.id);
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
            String top = Microsoft.VisualBasic.Interaction.InputBox("Specify top texture name", "Texture name", "Images/SkyboxTop", Width / 2, Height / 2);
            if (top == "") return;
            String bottom = Microsoft.VisualBasic.Interaction.InputBox("Specify bottom texture name", "Texture name", "Images/SkyboxBottom", Width / 2, Height / 2);
            if (bottom == "") return;
            String left = Microsoft.VisualBasic.Interaction.InputBox("Specify left texture name", "Texture name", "Images/SkyboxLeft", Width / 2, Height / 2);
            if (left == "") return;
            String right = Microsoft.VisualBasic.Interaction.InputBox("Specify right texture name", "Texture name", "Images/SkyboxRight", Width / 2, Height / 2);
            if (right == "") return; 
            String front = Microsoft.VisualBasic.Interaction.InputBox("Specify front texture name", "Texture name", "Images/SkyboxFront", Width / 2, Height / 2);
            if (front == "") return;
            String back = Microsoft.VisualBasic.Interaction.InputBox("Specify back texture name", "Texture name", "Images/SkyboxBack", Width / 2, Height / 2);
            if (back == "") return;
            String topRepVal = Microsoft.VisualBasic.Interaction.InputBox("Specify top texture repeat", "Repeat count", "1.0", Width / 2, Height / 2);
            if (topRepVal == "") return;
            float topRepeat = Convert.ToSingle(topRepVal);
            String bottomRepVal = Microsoft.VisualBasic.Interaction.InputBox("Specify bottom texture repeat", "Repeat count", "1.0", Width / 2, Height / 2);
            if (bottomRepVal == "") return;
            float bottomRepeat = Convert.ToSingle(bottomRepVal);
            String leftRepVal = Microsoft.VisualBasic.Interaction.InputBox("Specify left texture repeat", "Repeat count", "1.0", Width / 2, Height / 2);
            if (leftRepVal == "") return;
            float leftRepeat = Convert.ToSingle(leftRepVal);
            String rightRepVal = Microsoft.VisualBasic.Interaction.InputBox("Specify right texture repeat", "Repeat count", "1.0", Width / 2, Height / 2);
            if (rightRepVal == "") return;
            float rightRepeat = Convert.ToSingle(rightRepVal);
            String frontRepVal = Microsoft.VisualBasic.Interaction.InputBox("Specify front texture repeat", "Repeat count", "1.0", Width / 2, Height / 2);
            if (frontRepVal == "") return;
            float frontRepeat = Convert.ToSingle(frontRepVal);
            String backRepVal = Microsoft.VisualBasic.Interaction.InputBox("Specify back texture repeat", "Repeat count", "1.0", Width / 2, Height / 2);
            if (backRepVal == "") return;
            float backRepeat = Convert.ToSingle(backRepVal);
            BBNMap.SetUpSkyBox(gfxDevice, contentMgr, top, bottom, left, right, front, back, topRepeat, bottomRepeat, leftRepeat, rightRepeat, frontRepeat, backRepeat);
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
        }
        private void tsbOpen_Click(object sender, EventArgs e)
        {
            if (ofdMainWindow.ShowDialog() != DialogResult.Cancel)
            {
                if (clearContent())
                    try
                    {
                        BBNMap.loadMap(ofdMainWindow.FileName, contentMgr, gfxDevice);
                        foreach (MapContent item in BBNMap.content.Values)
                            cbxMapItems.Items.Add(item.id + " : " + item.itemClassName);
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
                BBNMap.setMapSize(Convert.ToSingle(val));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not set radius.\nReason: " + ex.Message,"Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
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
                            yDown = true;
                        break;
                    case System.Windows.Forms.Keys.P:
                        if (!(yDown || pDown || rDown || selectedXMoveLine || selectedYMoveLine || selectedZMoveLine || cDown) && cbxMapItems.SelectedIndex >= 0)
                            pDown = true;
                        break;
                    case System.Windows.Forms.Keys.R:
                        if (!(yDown || pDown || rDown || selectedXMoveLine || selectedYMoveLine || selectedZMoveLine || cDown) && cbxMapItems.SelectedIndex >= 0)
                            rDown = true;
                        break;
                    case System.Windows.Forms.Keys.C:
                        if (!(yDown || pDown || rDown || selectedXMoveLine || selectedYMoveLine || selectedZMoveLine || cDown) && cbxMapItems.SelectedIndex >= 0)
                        {
                            MapContent item = BBNMap.content[cbxMapItems.SelectedItem.ToString().Split(':')[0].Trim()];
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
                    MapContent item = BBNMap.content[cbxMapItems.SelectedItem.ToString().Split(':')[0].Trim()];
                    try
                    {
                        //We need the component the mouse is moved along the axis so get the component onto axis value (ref. Steward Calculus, concepts and contexts 4th. Chapter 9)
                        if (selectedXMoveLine)
                        {
                            float compMouseOntoAxis = Vector2.Dot(xArrowTop - xArrowBottom, new Vector2(e.X - oldCursorPos.X, e.Y - oldCursorPos.Y)) / (xArrowTop - xArrowBottom).Length();
                            item.setAttribute("x",
                                Convert.ToString(Convert.ToSingle(item.getAttribute("x")) + compMouseOntoAxis * movementSpeed * 0.1));
                            updateProperties(item.id);
                        }
                        else if (selectedYMoveLine)
                        {
                            float compMouseOntoAxis = Vector2.Dot(yArrowTop - yArrowBottom, new Vector2(e.X - oldCursorPos.X, e.Y - oldCursorPos.Y)) / (yArrowTop - yArrowBottom).Length();
                            item.setAttribute("y",
                                Convert.ToString(Convert.ToSingle(item.getAttribute("y")) + compMouseOntoAxis * movementSpeed * 0.1));
                            updateProperties(item.id);
                        }
                        else if (selectedZMoveLine)
                        {
                            float compMouseOntoAxis = Vector2.Dot(zArrowTop - zArrowBottom, new Vector2(e.X - oldCursorPos.X, e.Y - oldCursorPos.Y)) / (zArrowTop - zArrowBottom).Length();
                            item.setAttribute("z",
                                    Convert.ToString(Convert.ToSingle(item.getAttribute("z")) + compMouseOntoAxis * movementSpeed * 0.1));
                            updateProperties(item.id);
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
                MapContent item = BBNMap.content[cbxMapItems.SelectedItem.ToString().Split(':')[0].Trim()];
                if (rDown)
                {
                    float val = -(float)Math.Atan2(e.Y - this.scrMainLayout.Panel2.Height / 2, e.X - this.scrMainLayout.Panel2.Width / 2) * 180 / (float)Math.PI - 90;
                    item.setAttribute("roll",Convert.ToString(val));
                    updateProperties(item.id);
                }
                else if (yDown)
                {
                    float val = -(float)Math.Atan2(e.Y - this.scrMainLayout.Panel2.Height / 2, e.X - this.scrMainLayout.Panel2.Width / 2) * 180 / (float)Math.PI - 90;
                    item.setAttribute("yaw", Convert.ToString(val));
                    updateProperties(item.id);
                }
                else if (pDown)
                {
                    float val = -(float)Math.Atan2(e.Y - this.scrMainLayout.Panel2.Height / 2, e.X - this.scrMainLayout.Panel2.Width / 2) * 180 / (float)Math.PI - 90;
                    item.setAttribute("pitch", Convert.ToString(val));
                    updateProperties(item.id);
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
                        projection, view, Matrix.Identity);
                    Vector3 vFar = gfxDevice.Viewport.Unproject(new Vector3(e.X, e.Y, 1f),
                        projection, view, Matrix.Identity);
                    float minDist = 0;
                    StaticObject closestObj = null;
                    foreach (StaticObject item in BBNMap.content.Values)
                    {
                        float dist = 0;
                        if (item.rayIntersect(vNear, vFar, out dist))
                            if (closestObj == null || dist < minDist)
                            {
                                minDist = dist;
                                closestObj = item;
                            }
                    }
                    //get the closest object (first in depth sort):
                    if (closestObj != null)
                    {
                        //connect path nodes (if "c" is held down and a path node is selected then connect it to the node the user clicked on if it is another path node):
                        if (closestObj is Node && cDown)
                        {
                            MapContent item = BBNMap.content[cbxMapItems.SelectedItem.ToString().Split(':')[0].Trim()];
                            bool found = false;
                            for (int i = 0; i < (item as Node).getEdgeCount(); ++i) 
                            {
                                Edge edge = (item as Node).getEdge(i);
                                if (edge.node1 == closestObj || edge.node2 == closestObj)
                                {
                                    (item as Node).disconnectFromNode(closestObj as Node);
                                    found = true;
                                    break;
                                }
                            }
                            if (!found)
                                (item as Node).connectToNode(closestObj as Node, 0);
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
    }
}
