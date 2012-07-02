using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Xml;
namespace Editor
{
    class ToolboxLoader
    {
        public static List<ToolboxItem> toolboxContent = new List<ToolboxItem>();
        public static void loadContent()
        {
            string[] files = Directory.GetFiles("Content/MapItems/");
            foreach (String file in files)
            {
                XmlReader xr = XmlReader.Create(file);
                xr.ReadToFollowing("Entity");
                ToolboxItem newItem;
                newItem = new ToolboxItem(xr.GetAttribute("Name"), xr.GetAttribute("Type"));
                
                String currentElement = ""; 
                while (xr.Read())
                {
                    if (xr.IsStartElement())
                        currentElement = xr.Name;
                    else if (xr.NodeType == XmlNodeType.EndElement)
                        if (xr.Name == "Entity")
                            break;
                        else
                            currentElement = "";
                    else if (xr.NodeType == XmlNodeType.Text)
                        newItem.addAttribute(currentElement, xr.Value);
                }
                toolboxContent.Add(newItem);
                xr.Close();
            }
                
        }
    }
}
