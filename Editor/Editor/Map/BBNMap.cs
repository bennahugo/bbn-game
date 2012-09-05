using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using BBN_Game.AI;
using BBN_Game.Utils;
using Microsoft.Xna.Framework.Content;
using System.Xml;
using System.Xml.XPath;
using Editor;
using BBN_Game.Graphics.Skybox;
namespace BBN_Game.Map
{
    /// <summary>
    /// The map class is a singleton class and cannot be instantiated.
    /// It houses the methods to load and save a map to XML as well as the contents of the map
    /// </summary>
    static class BBNMap
    {
        /// <summary>
        /// The model should only be displayed during map editing
        /// </summary>
        public const String MODEL_USED_FOR_MAP_EDITOR = "Models/marker";
        /// <summary>
        /// The collection of the content instances of the entire map.
        /// </summary>
        public static Dictionary<String, Object> content = new Dictionary<String, Object>();
        // Skybox textures and quads:
        private static Texture2D skyBoxTexture = null;
        private static float skyBoxRepeat = 10.0f;
        //private static QuadHelper[] skyboxQuads = new QuadHelper[6];
        private static float mapRadius = 200000;
        private static Graphics.Skybox.Skybox skyBoxDrawer;
        /// <summary>
        /// Set this value if you want to draw AI path edges for debugging purposes
        /// </summary>
        public static bool shouldDrawPathNodeConnections { get; set; }
        /// <summary>
        /// Enable when in game mode, disable if in map mode
        /// </summary>
        public static bool shouldSendControlerStatesToObjects { get; set; }
        /// <summary>
        /// Sets up the skybox.
        /// </summary>
        /// <param name="gfxDevice">Graphics device instance</param>
        /// <param name="contentMgr">Content manager</param>
        /// <param name="top">top texture name</param>
        /// <param name="bottom">bottom texture name</param>
        /// <param name="left">left texture name</param>
        /// <param name="right">right texture name</param>
        /// <param name="front">front texture name</param>
        /// <param name="back">back texture name</param>
        /// <param name="repeatTop">repeat count for the top texture (normally 1.0f)</param>
        /// <param name="repeatBottom">repeat count for the bottom texture (normally 1.0f)</param>
        /// <param name="repeatLeft">repeat count for the left texture (normally 1.0f)</param>
        /// <param name="repeatRight">repeat count for the right texture (normally 1.0f)</param>
        /// <param name="repeatFront">repeat count for the front texture (normally 1.0f)</param>
        /// <param name="repeatBack">repeat count for the back texture (normally 1.0f)</param>
        public static void SetUpSkyBox(GraphicsDevice gfxDevice, ContentManager contentMgr,
            String Text, string Repeat)
        {
            //first check for texture loading errors before setting up the quads:
            if (Text != "" && Text != null)
            {
                skyBoxTexture = contentMgr.Load<Texture2D>(Text);
                skyBoxTexture.Name = Text;
            }
            float maxAway = mapRadius / 2;
            //top:
            skyBoxRepeat = float.Parse(Repeat);
            if (Text != "" && Text != null)
            {
                skyBoxDrawer = new Graphics.Skybox.Skybox();
                skyBoxDrawer.Initialize(mapRadius/2, (int)skyBoxRepeat);
                skyBoxDrawer.loadContent(skyBoxTexture, contentMgr, gfxDevice);
            }
        }
        /// <summary>
        /// Method to set the radius of the map
        /// </summary>
        /// <param name="newSize">Decimal value larger than 0</param>
        public static void setMapSize(float newSize, ContentManager contentMgr, GraphicsDevice gfxDevice)
        {
            if (newSize < 50)
                throw new Exception("The new radius must be larger or equal to 50");
            float oldSize = mapRadius;
            mapRadius = newSize;
            foreach (Object item in content.Values)
                if (!isObjectInMap(item))
                {
                    mapRadius = oldSize;
                    throw new Exception("Some objects are outside of the new radius. Please set the radius to a larger size");
                }
            if (skyBoxTexture != null)
                SetUpSkyBox(gfxDevice, contentMgr, skyBoxTexture.Name, Convert.ToString(skyBoxRepeat)); 
        }
        /// <summary>
        /// Method to get the map radius
        /// </summary>
        /// <returns>Map radius</returns>
        public static float getMapRadius()
        {
            return mapRadius;
        }
        /// <summary>
        /// Method to check if object is inside the confines of the map
        /// </summary>
        /// <param name="anObject">Object to check</param>
        /// <returns>True iff object's center point is inside the map</returns>
        public static Boolean isObjectInMap(Object anObject)
        {
            float x = 0;
            float y = 0;
            float z = 0;
            if (anObject is MapContent)
            {
                x = Convert.ToSingle((anObject as MapContent).getAttribute("x"));
                y = Convert.ToSingle((anObject as MapContent).getAttribute("y"));
                z = Convert.ToSingle((anObject as MapContent).getAttribute("z"));
            }
            else if (anObject is Marker)
            {
                x = (anObject as Marker).Position.X;
                y = (anObject as Marker).Position.Y;
                z = (anObject as Marker).Position.Z;
            }
            float d2 = x * x + y * y + z * z;
            if (d2 < mapRadius * mapRadius)
                return true;
            else
                return false;
        }
        /// <summary>
        /// Method to draw map and all its contents
        /// </summary>
        /// <param name="gfxDevice">Graphics device instance</param>
        /// <param name="projection">projection matrix</param>
        /// <param name="view">view matrix</param>
        /// <param name="lightsSetup">lights setup array as required by all static objects</param>
        /// <param name="fogColor">fog color as a vector3</param>
        /// <param name="fogSetup">fog setup as required by all static objects</param>
        /// <param name="basicEffect">basic effect class to enable primative drawing</param>
        /// <param name="camPos">camera position (note: N O T the focus point - used to translate the skybox when its drawn)</param>
        public static void DrawMap(GraphicsDevice gfxDevice, Matrix projection, ContentManager contentMgr, Matrix view, Vector3[] lightsSetup, Vector3 fogColor, int[] fogSetup, BasicEffect basicEffect, Vector3 camPos)
        {
            //Draw skybox
            if (skyBoxDrawer != null)
                skyBoxDrawer.Draw(view, projection, gfxDevice);
            //Draw all objects:
            foreach (Object item in BBNMap.content.Values)
            {
                if (shouldDrawPathNodeConnections)
                    if (item is Node)
                    {
                        for (int i = 0; i < (item as Node).getEdgeCount(); ++i) // draw all edges
                        {
                            Edge e = (item as Node).getEdge(i);
                            Algorithms.Draw3DLine(Color.Red, e.node1.Position, e.node2.Position, basicEffect, gfxDevice, projection, view, Matrix.Identity);
                        }
                    }
                if (item is Drawer)
                    (item as Drawer).Draw(view, projection, lightsSetup, fogColor, fogSetup);
                else if (item is Marker)
                {
                    Drawer dTemp = new Drawer((item as Marker).Position, contentMgr,true);
                    dTemp.addAttribute("modelName", MODEL_USED_FOR_MAP_EDITOR);
                    dTemp.addAttribute("x", Convert.ToString((item as Marker).Position.X));
                    dTemp.addAttribute("y", Convert.ToString((item as Marker).Position.Y));
                    dTemp.addAttribute("z", Convert.ToString((item as Marker).Position.Z));
                    dTemp.addAttribute("scaleX", "0.25");
                    dTemp.addAttribute("scaleY", "0.25");
                    dTemp.addAttribute("scaleZ", "0.25");
                    dTemp.addAttribute("yaw", "0");
                    dTemp.addAttribute("pitch", "0");
                    dTemp.addAttribute("roll", "0");
                    dTemp.onAttributeChange();
                    dTemp.update(new KeyboardState(), new GamePadState(), new GamePadState());
                    dTemp.Draw(view, projection, lightsSetup, fogColor, fogSetup);
                }
            }
        }
        /// <summary>
        /// Updates map content
        /// </summary>
        /// <param name="deltaTime">Time elapsed since last update</param>
        public static void UpdateMapContent(float deltaTime)
        {
            foreach (Object item in BBNMap.content.Values)
                if (item is Drawer)
                    if (shouldSendControlerStatesToObjects)
                        (item as Drawer).update(Keyboard.GetState(), GamePad.GetState(PlayerIndex.One), GamePad.GetState(PlayerIndex.Two));
                    else
                        (item as Drawer).update(new KeyboardState(), new GamePadState(), new GamePadState());
        }
        /// <summary>
        /// Method to reset map to a new map
        /// <param name="contentMgr">Content manager</param>
        /// </summary>
        public static void clearMap(ContentManager contentMgr)
        {
            content.Clear();
            contentMgr.Unload();
            skyBoxTexture = null;
            mapRadius = 200000;
            skyBoxRepeat = 10;
            skyBoxDrawer = null;
        }
        /// <summary>
        /// Method to read a single map content instance's data from XML
        /// </summary>
        /// <param name="contentMgr">Content manager instance</param>
        /// <param name="reader">xml reader</param>
        /// <param name="docNav">xml document navigator</param>
        /// <param name="nav">Xpath navigator</param>
        /// <param name="nsmanager">namespace manager</param>
        /// <param name="iter">XPath iterator</param>
        /// <param name="n">node into which data must be read</param>
        private static void readObjectData(ContentManager contentMgr, XmlReader reader, XPathDocument docNav, 
            XPathNavigator nav, XmlNamespaceManager nsmanager, XPathNodeIterator iter, MapContent n)
        {
            String id = iter.Current.GetAttribute("id", nsmanager.DefaultNamespace);
            String name = iter.Current.GetAttribute("className", nsmanager.DefaultNamespace);
            String type = iter.Current.GetAttribute("type", nsmanager.DefaultNamespace);
            n.setNewId(id);
            n.itemClassName = name;
            n.type = type;
            
            if (iter.Current.MoveToFirstChild())
            {
                do
                {
                    String attName = iter.Current.Name;
                    String attVal = iter.Current.Value;
                    if (!n.getAttributeNames().Contains(attName))
                        n.addAttribute(attName, attVal);
                    else
                        n.setAttribute(attName, attVal);
                } while (iter.Current.MoveToNext());
            }
            n.onAttributeChange();
        }
        /// <summary>
        /// Method to read a map marker's data
        /// </summary>
        /// <param name="contentMgr">Content manager instance</param>
        /// <param name="reader">xml reader</param>
        /// <param name="docNav">xml document navigator</param>
        /// <param name="nav">Xpath navigator</param>
        /// <param name="nsmanager">namespace manager</param>
        /// <param name="iter">XPath iterator</param>
        /// <param name="n">node into which data must be read</param>
        private static void readMarkerData(ContentManager contentMgr, XmlReader reader, XPathDocument docNav,
            XPathNavigator nav, XmlNamespaceManager nsmanager, XPathNodeIterator iter, Marker n)
        {
            String id = iter.Current.GetAttribute("id", nsmanager.DefaultNamespace);
            String name = iter.Current.GetAttribute("className", nsmanager.DefaultNamespace);
            String type = iter.Current.GetAttribute("type", nsmanager.DefaultNamespace);
            int team = Convert.ToInt32(iter.Current.GetAttribute("owningTeam", nsmanager.DefaultNamespace));
            n.id = id;
            n.type = type;
            n.className = name;
            n.OwningTeam = team;
            if (iter.Current.MoveToFirstChild())
            {
                float x = 0;
                float y = 0;
                float z = 0;
                do
                {
                    String attName = iter.Current.Name;
                    String attVal = iter.Current.Value;
                    if (attName == "x")
                        x = Convert.ToSingle(attVal);
                    else if (attName == "y")
                        y = Convert.ToSingle(attVal);
                    else if (attName == "z")
                        z = Convert.ToSingle(attVal);
                    
                } while (iter.Current.MoveToNext());
                n.Position = new Vector3(x, y, z);
            }
        }
        /// <summary>
        /// Method to load a map
        /// </summary>
        /// <param name="filename">file name of map</param>
        /// <param name="contentMgr">content manager instance</param>
        /// <param name="gfxDevice">graphics device instance</param>
        public static void loadMap(String filename, ContentManager contentMgr, GraphicsDevice gfxDevice)
        {
            XmlReader reader = XmlReader.Create(filename);
            XPathDocument docNav = new XPathDocument(reader);
            XPathNavigator nav = docNav.CreateNavigator();
            XmlNamespaceManager nsmanager = new XmlNamespaceManager(nav.NameTable);            
            XPathNodeIterator iter;
            XPathNavigator mapIter = nav.SelectSingleNode("/Map");
            mapRadius = Convert.ToSingle(mapIter.GetAttribute("mapRadius", nsmanager.DefaultNamespace));

            XPathNavigator skyboxIter = nav.SelectSingleNode("/Map/Skybox");
            String texName = skyboxIter.GetAttribute("texture", nsmanager.DefaultNamespace);
            skyBoxRepeat = Convert.ToSingle(skyboxIter.GetAttribute("repeat", nsmanager.DefaultNamespace));
            SetUpSkyBox(gfxDevice, contentMgr, texName, Convert.ToString(skyBoxRepeat));
            //Now read in path nodes:
            iter = nav.Select("/Map/Marker[@className!='SpawnPoint' and @className!='PlayerSpawnPoint']");
            while (iter.MoveNext())
            {
                Node n = new Node();
                readMarkerData(contentMgr, reader, docNav, nav, nsmanager, iter, n);
                content.Add(n.id, n);
            }
            //Read spawnpoints:
            iter = nav.Select("/Map/Marker[@className='SpawnPoint']");
            while (iter.MoveNext())
            {
                SpawnPoint n = new SpawnPoint();
                readMarkerData(contentMgr, reader, docNav, nav, nsmanager, iter, n);
                content.Add(n.id, n);
            }
            //Read player spawnpoints:
            iter = nav.Select("/Map/Marker[@className='PlayerSpawnPoint']");
            while (iter.MoveNext())
            {
                SpawnPoint n = new PlayerSpawnPoint();
                readMarkerData(contentMgr, reader, docNav, nav, nsmanager, iter, n);
                content.Add(n.id, n);
            }
            //Now read other content:
            iter = nav.Select("/Map/ContentItem");
            while (iter.MoveNext())
            {
                Drawer n = new Drawer(false);
                n.contentLoader = contentMgr;
                readObjectData(contentMgr, reader, docNav, nav, nsmanager, iter, n);
            }
            List<Edge> edgeList = new List<Edge>();
            List<float> edgeDistances = new List<float>();
            iter = nav.Select("/Map/PathEdge");
            while (iter.MoveNext())
            {
                float weight = Convert.ToSingle(iter.Current.GetAttribute("weight", nsmanager.DefaultNamespace));
                float distance = Convert.ToSingle(iter.Current.GetAttribute("distance", nsmanager.DefaultNamespace));
                String firstId = iter.Current.SelectSingleNode("firstNodeId").Value;
                String secondId = iter.Current.SelectSingleNode("secondNodeId").Value;
                Node first = null, second = null;
                foreach (Object item in content.Values)
                    if (item is Node)
                    {
                        if ((item as Marker).id == firstId)
                            first = item as Node;
                        else if ((item as Marker).id == secondId)
                            second = item as Node;
                        if (first != null && second != null)
                            break;
                    }
                edgeList.Add(new Edge(first, second, weight));
                edgeDistances.Add(distance);
            }
            //Connect nodes:
            for (int i = 0; i < edgeList.Count; i++)           
            {
                Edge item = edgeList.ElementAt(i);
                float distance = edgeDistances.ElementAt(i);
                item.node1.connectToNode(item.node2, item.weight, distance);
            }
            reader.Close();
        }
        /// <summary>
        /// Method to save a file
        /// </summary>
        /// <param name="filename">file path</param>
        public static void saveMap(String filename)
        {
            XmlWriter writer = XmlWriter.Create(filename);
            writer.WriteStartDocument();
            writer.WriteStartElement("Map");
            writer.WriteAttributeString("mapRadius", Convert.ToString(mapRadius));
            List<Edge> edgeList = new List<Edge>();
            writer.WriteStartElement("Skybox");
            //top skybox texture 
            if (skyBoxTexture != null)
                writer.WriteAttributeString("texture", Convert.ToString(skyBoxTexture.Name));
            else
                writer.WriteAttributeString("texture", "");
            writer.WriteAttributeString("repeat", Convert.ToString(skyBoxRepeat));
            writer.WriteEndElement();
            ///Markers
            foreach (Object item in content.Values)
            {
                if (item is Marker)
                {
                    if (item is Node)
                        for (int i = 0; i < (item as Node).getEdgeCount(); ++i)
                        {
                            Edge edge = (item as Node).getEdge(i);
                            if (!edgeList.Contains(edge))
                                edgeList.Add(edge);
                        }
                    writer.WriteStartElement("Marker");
                    writer.WriteAttributeString("id", (item as Marker).id);
                    writer.WriteAttributeString("className", (item as Marker).className);
                    writer.WriteAttributeString("type", (item as Marker).type);
                    writer.WriteAttributeString("owningTeam", Convert.ToString((item as Marker).OwningTeam));
                    writer.WriteElementString("x",Convert.ToString((item as Marker).Position.X));
                    writer.WriteElementString("y",Convert.ToString((item as Marker).Position.Y));
                    writer.WriteElementString("z",Convert.ToString((item as Marker).Position.Z));
                    writer.WriteEndElement();
                }
                else
                {
                    writer.WriteStartElement("ContentItem");
                    writer.WriteAttributeString("id", (item as MapContent).id);
                    writer.WriteAttributeString("className", (item as MapContent).itemClassName);
                    writer.WriteAttributeString("type", (item as MapContent).type);
                    foreach (String key in (item as MapContent).getAttributeNames())
                        writer.WriteElementString(key, (item as MapContent).getAttribute(key));
                    writer.WriteEndElement();
                }
            }
            foreach (Edge edge in edgeList)
            {
                writer.WriteStartElement("PathEdge");
                writer.WriteAttributeString("weight",Convert.ToString(edge.weight));
                writer.WriteAttributeString("distance", Convert.ToString(edge.distance));
                writer.WriteElementString("firstNodeId", edge.node1.id);
                writer.WriteElementString("secondNodeId", edge.node2.id);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.Close();
        }
    }
}
