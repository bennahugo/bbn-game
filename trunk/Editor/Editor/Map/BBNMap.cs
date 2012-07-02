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
using BBN_Game.Objects;
namespace BBN_Game.Map
{
    /// <summary>
    /// The map class is a singleton class and cannot be instantiated.
    /// It houses the methods to load and save a map to XML as well as the contents of the map
    /// </summary>
    static class BBNMap
    {
        /// <summary>
        /// The collection of the content instances of the entire map.
        /// </summary>
        public static Dictionary<String, MapContent> content = new Dictionary<String, MapContent>();
        // Skybox textures and quads:
        private static Texture2D skyBoxTopTex = null;
        private static Texture2D skyBoxBottomTex = null;
        private static Texture2D skyBoxLeftTex = null;
        private static Texture2D skyBoxRightTex = null;
        private static Texture2D skyBoxFrontTex = null;
        private static Texture2D skyBoxBackTex = null;
        private static float skyBoxTopRepeat = 1.0f;
        private static float skyBoxBottomRepeat = 1.0f;
        private static float skyBoxLeftRepeat = 1.0f;
        private static float skyBoxRightRepeat = 1.0f;
        private static float skyBoxFrontRepeat = 1.0f;
        private static float skyBoxBackRepeat = 1.0f;
        private static QuadHelper[] skyboxQuads = new QuadHelper[6];
        private static float mapRadius = 10000;
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
            String top, String bottom, String left, String right, String front, String back,
            float repeatTop, float repeatBottom, float repeatLeft, float repeatRight, float repeatFront, float repeatBack)
        {
            //first check for texture loading errors before setting up the quads:
            if (top != "" && top != null)
            {
                skyBoxTopTex = contentMgr.Load<Texture2D>(top);
                skyBoxTopTex.Name = top;
            }
            if (bottom != "" && bottom != null)
            {
                skyBoxBottomTex = contentMgr.Load<Texture2D>(bottom);
                skyBoxBottomTex.Name = bottom;
            }
            if (left != "" && left != null)
            {
                skyBoxLeftTex = contentMgr.Load<Texture2D>(left);
                skyBoxLeftTex.Name = left;
            }
            if (right != "" && right != null)
            {
                skyBoxRightTex = contentMgr.Load<Texture2D>(right);
                skyBoxRightTex.Name = right;
            }
            if (front != "" && front != null)
            {
                skyBoxFrontTex = contentMgr.Load<Texture2D>(front);
                skyBoxFrontTex.Name = front;
            }
            if (back != "" && back != null)
            {
                skyBoxBackTex = contentMgr.Load<Texture2D>(back);
                skyBoxBackTex.Name = back;
            }
            skyBoxTopRepeat = repeatTop;
            skyBoxBottomRepeat = repeatBottom;
            skyBoxLeftRepeat = repeatLeft;
            skyBoxRightRepeat = repeatRight;
            skyBoxFrontRepeat = repeatFront;
            skyBoxBackRepeat = repeatBack;
            float maxAway = (float)Math.Floor(Math.Sqrt(gfxDevice.Viewport.MaxDepth * gfxDevice.Viewport.MaxDepth * 0.32));
            //top:
            if (top != "" && top != null)
                skyboxQuads[0] = new QuadHelper(new Vector3(maxAway + 0.5f, maxAway, -maxAway),
                    new Vector3(-maxAway - 0.5f, maxAway, -maxAway),
                    new Vector3(maxAway + 0.5f, maxAway, maxAway),
                    new Vector3(-maxAway - 0.5f, maxAway, maxAway), repeatTop, top, gfxDevice, contentMgr);
            //bottom:
            if (bottom != "" && bottom != null)
                skyboxQuads[1] = new QuadHelper(new Vector3(-maxAway - 0.5f, -maxAway, -maxAway),
                    new Vector3(maxAway + 0.5f, -maxAway, -maxAway),
                    new Vector3(-maxAway - 0.5f, -maxAway, maxAway),
                    new Vector3(maxAway + 0.5f, -maxAway, maxAway), repeatBottom, bottom, gfxDevice, contentMgr);
            //left:
            if (left != "" && left != null)
                skyboxQuads[2] = new QuadHelper(new Vector3(-maxAway, maxAway, +maxAway + 0.5f),
                    new Vector3(-maxAway, maxAway, -maxAway - 0.5f),
                    new Vector3(-maxAway, -maxAway, +maxAway + 0.5f),
                    new Vector3(-maxAway, -maxAway, -maxAway - 0.5f), repeatLeft, left, gfxDevice, contentMgr);
            //right:
            if (right != "" && right != null)
                skyboxQuads[3] = new QuadHelper(new Vector3(maxAway, maxAway, -maxAway - 0.5f),
                    new Vector3(maxAway, maxAway, +maxAway + 0.5f),
                    new Vector3(maxAway, -maxAway, -maxAway - 0.5f),
                    new Vector3(maxAway, -maxAway, +maxAway + 0.5f), repeatRight, right, gfxDevice, contentMgr);
            //back:
            if (back != "" && back != null)
                skyboxQuads[4] = new QuadHelper(new Vector3(-maxAway - 0.5f, maxAway, -maxAway),
                    new Vector3(maxAway + 0.5f, maxAway, -maxAway),
                    new Vector3(-maxAway - 0.5f, -maxAway, -maxAway),
                    new Vector3(maxAway + 0.5f, -maxAway, -maxAway), repeatBack, back, gfxDevice, contentMgr);
            //front:
            if (front != "" && front != null)
                skyboxQuads[5] = new QuadHelper(new Vector3(maxAway + 0.5f, maxAway, maxAway),
                    new Vector3(-maxAway - 0.5f, maxAway, maxAway),
                    new Vector3(maxAway + 0.5f, -maxAway, maxAway),
                    new Vector3(-maxAway-0.5f, -maxAway, maxAway), repeatFront, front, gfxDevice, contentMgr);
        }
        /// <summary>
        /// Method to set the radius of the map
        /// </summary>
        /// <param name="newSize">Decimal value larger than 0</param>
        public static void setMapSize(float newSize)
        {
            if (newSize <= 0)
                throw new Exception("The new radius must be larger than 0");
            float oldSize = mapRadius;
            mapRadius = newSize;
            foreach (MapContent item in content.Values)
                if (!isObjectInMap(item))
                {
                    mapRadius = oldSize;
                    throw new Exception("Some objects are outside of the new radius. Please set the radius to a larger size");
                }
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
        public static Boolean isObjectInMap(MapContent anObject)
        {
            float x = Convert.ToSingle(anObject.getAttribute("x"));
            float y = Convert.ToSingle(anObject.getAttribute("y"));
            float z = Convert.ToSingle(anObject.getAttribute("z"));
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
        public static void DrawMap(GraphicsDevice gfxDevice, Matrix projection, Matrix view, Vector3[] lightsSetup, Vector3 fogColor, int[] fogSetup, BasicEffect basicEffect, Vector3 camPos)
        {
            //Draw skybox
            foreach (QuadHelper wall in skyboxQuads)
                if (wall != null)
                    wall.Draw(view, Matrix.CreateTranslation(camPos), projection, basicEffect);
            //Draw all objects:
            foreach (MapContent item in BBNMap.content.Values)
            {
                if (shouldDrawPathNodeConnections)
                    if (item is Node)
                        for (int i = 0; i < (item as Node).getEdgeCount(); ++i) // draw all edges
                        {
                            Edge e = (item as Node).getEdge(i);
                            Algorithms.Draw3DLine(Color.Red, e.node1.Position, e.node2.Position, basicEffect, gfxDevice, projection, view, Matrix.Identity);
                        }
                item.Draw(view, projection, lightsSetup, fogColor, fogSetup);
            }
        }
        /// <summary>
        /// Updates map content
        /// </summary>
        /// <param name="deltaTime">Time elapsed since last update</param>
        public static void UpdateMapContent(float deltaTime)
        {
            foreach (MapContent item in BBNMap.content.Values)
                if (shouldSendControlerStatesToObjects)
                    item.update(Keyboard.GetState(), GamePad.GetState(PlayerIndex.One), GamePad.GetState(PlayerIndex.Two));
                else
                    item.update(new KeyboardState(), new GamePadState(), new GamePadState());
        }
        /// <summary>
        /// Method to reset map to a new map
        /// <param name="contentMgr">Content manager</param>
        /// </summary>
        public static void clearMap(ContentManager contentMgr)
        {
            content.Clear();
            contentMgr.Unload();
            skyBoxBackTex = null;
            skyBoxBottomTex = null;
            skyBoxFrontTex = null;
            skyBoxLeftTex = null;
            skyBoxRightTex = null;
            skyBoxTopTex = null;
            mapRadius = 10000;
            skyBoxBackRepeat = 1;
            skyBoxBottomRepeat = 1;
            skyBoxFrontRepeat = 1;
            skyBoxLeftRepeat = 1;
            skyBoxRightRepeat = 1;
            skyBoxTopRepeat = 1;
            for (int i = 0; i < skyboxQuads.Length; ++i )
                skyboxQuads[i] = null;
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
        private static void readObjectNodeData(ContentManager contentMgr, XmlReader reader, XPathDocument docNav, 
            XPathNavigator nav, XmlNamespaceManager nsmanager, XPathNodeIterator iter, StaticObject n)
        {
            String id = iter.Current.GetAttribute("id", nsmanager.DefaultNamespace);
            String name = iter.Current.GetAttribute("className", nsmanager.DefaultNamespace);
            String type = iter.Current.GetAttribute("type", nsmanager.DefaultNamespace);
            n.setNewId(id);
            n.itemClassName = name;
            n.type = type;
            n.contentLoader = contentMgr;
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
            //Read skybox data:
            XPathNavigator skyboxIter;
            skyboxIter = nav.SelectSingleNode("/Map/Skybox/top");
            skyBoxTopRepeat = Convert.ToSingle(skyboxIter.GetAttribute("repeatCount", nsmanager.DefaultNamespace));
            String topTexName = skyboxIter.Value;
            skyboxIter = nav.SelectSingleNode("/Map/Skybox/bottom");
            skyBoxBottomRepeat = Convert.ToSingle(skyboxIter.GetAttribute("repeatCount", nsmanager.DefaultNamespace));
            String bottomTexName = skyboxIter.Value;
            skyboxIter = nav.SelectSingleNode("/Map/Skybox/left");
            skyBoxLeftRepeat = Convert.ToSingle(skyboxIter.GetAttribute("repeatCount", nsmanager.DefaultNamespace));
            String leftTexName = skyboxIter.Value;
            skyboxIter = nav.SelectSingleNode("/Map/Skybox/right");
            skyBoxRightRepeat = Convert.ToSingle(skyboxIter.GetAttribute("repeatCount", nsmanager.DefaultNamespace));
            String rightTexName = skyboxIter.Value;
            skyboxIter = nav.SelectSingleNode("/Map/Skybox/front");
            skyBoxFrontRepeat = Convert.ToSingle(skyboxIter.GetAttribute("repeatCount", nsmanager.DefaultNamespace));
            String frontTexName = skyboxIter.Value;
            skyboxIter = nav.SelectSingleNode("/Map/Skybox/back");
            skyBoxBackRepeat = Convert.ToSingle(skyboxIter.GetAttribute("repeatCount", nsmanager.DefaultNamespace));
            String backTexName = skyboxIter.Value;
            SetUpSkyBox(gfxDevice, contentMgr, topTexName, bottomTexName, leftTexName, rightTexName, 
                frontTexName, backTexName, skyBoxTopRepeat, skyBoxBottomRepeat, skyBoxLeftRepeat, 
                skyBoxRightRepeat, skyBoxFrontRepeat, skyBoxBackRepeat);
            //Now read in path nodes:
            iter = nav.Select("/Map/PathNode");
            while (iter.MoveNext())
            {
                Node n = new Node();
                readObjectNodeData(contentMgr, reader, docNav, nav, nsmanager, iter, n);
            }
            //Read spawnpoints:
            iter = nav.Select("/Map/ContentItem[@className='SpawnPoint']");
            while (iter.MoveNext())
            {
                SpawnPoint n = new SpawnPoint();
                readObjectNodeData(contentMgr, reader, docNav, nav, nsmanager, iter, n);
            }
            //Now read other content:
            iter = nav.Select("/Map/ContentItem[@className!='SpawnPoint']");
            while (iter.MoveNext())
            {
                Boolean movableObject = Convert.ToBoolean(iter.Current.GetAttribute("movableType", nsmanager.DefaultNamespace));
                if (movableObject)
                {
                    DynamicObject n = new DynamicObject();
                    readObjectNodeData(contentMgr, reader, docNav, nav, nsmanager, iter, n);
                }
                else
                {
                    StaticObject n = new StaticObject();
                    readObjectNodeData(contentMgr, reader, docNav, nav, nsmanager, iter, n);
                }
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
                foreach (MapContent item in content.Values)
                {
                    if (item.id == firstId)
                        first = item as Node;
                    else if (item.id == secondId)
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
            writer.WriteStartElement("top");
            writer.WriteAttributeString("repeatCount",Convert.ToString(skyBoxTopRepeat));
            if (skyBoxTopTex != null)
                writer.WriteString(skyBoxTopTex.Name);
            writer.WriteEndElement();
            //bottom skybox texture
            writer.WriteStartElement("bottom");
            writer.WriteAttributeString("repeatCount", Convert.ToString(skyBoxBottomRepeat));
            if (skyBoxBottomTex != null)
                writer.WriteString(skyBoxBottomTex.Name);
            writer.WriteEndElement();
            //left skybox texture
            writer.WriteStartElement("left");
            writer.WriteAttributeString("repeatCount", Convert.ToString(skyBoxLeftRepeat));
            if (skyBoxLeftTex != null)
                writer.WriteString(skyBoxLeftTex.Name);
            writer.WriteEndElement();
            //right skybox texture
            writer.WriteStartElement("right");
            writer.WriteAttributeString("repeatCount", Convert.ToString(skyBoxRightRepeat));
            if (skyBoxRightTex != null)
                writer.WriteString(skyBoxRightTex.Name);
            writer.WriteEndElement();
            //front skybox texture
            writer.WriteStartElement("front");
            writer.WriteAttributeString("repeatCount", Convert.ToString(skyBoxFrontRepeat));
            if (skyBoxFrontTex != null)
                writer.WriteString(skyBoxFrontTex.Name);
            writer.WriteEndElement();
            //back skybox texture
            writer.WriteStartElement("back");
            writer.WriteAttributeString("repeatCount", Convert.ToString(skyBoxBackRepeat));
            if (skyBoxBackTex != null)
                writer.WriteString(skyBoxBackTex.Name);
            writer.WriteEndElement();
            writer.WriteEndElement();
            foreach (MapContent item in content.Values)
            {
                if (item is Node)
                {
                    for (int i = 0; i < (item as Node).getEdgeCount(); ++i)
                    {
                        Edge edge = (item as Node).getEdge(i);
                        if (!edgeList.Contains(edge))
                            edgeList.Add(edge);
                    }
                    writer.WriteStartElement("PathNode");
                    writer.WriteAttributeString("id", item.id);
                    writer.WriteAttributeString("className", item.itemClassName);
                    writer.WriteAttributeString("type", item.type);
                    foreach (String key in item.getAttributeNames())
                        writer.WriteElementString(key, item.getAttribute(key));
                    writer.WriteEndElement();
                }
                else
                {
                    writer.WriteStartElement("ContentItem");
                    writer.WriteAttributeString("id", item.id);
                    writer.WriteAttributeString("className", item.itemClassName);
                    writer.WriteAttributeString("type", item.type);
                    writer.WriteAttributeString("movableType", Convert.ToString(item is DynamicObject ? true : false));
                    foreach (String key in item.getAttributeNames())
                        writer.WriteElementString(key, item.getAttribute(key));
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
