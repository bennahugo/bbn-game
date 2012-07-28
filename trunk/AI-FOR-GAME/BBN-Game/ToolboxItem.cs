using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
namespace Editor
{
    /// <summary>
    /// XML Template Class, to be used for populating the toolbox in the map editor and to load default attribute
    /// values of game objects from.
    /// </summary>
    class ToolboxItem
    {
        /// <summary>
        /// name of XML class
        /// </summary>
        public String name { get; private set; }
        /// <summary>
        /// Name of category type (ex. scenery or AI)
        /// </summary>
        public String type { get; private set; }
        /// <summary>
        /// Miscellanious XML attribute list
        /// </summary>
        private Dictionary<String,String> otherAttributes;
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="name">Name of XML class</param>
        /// <param name="type">Type of category (ex. Scenery)</param>
        /// <param name="movableObject">Specifies if the class is dynamic or static</param>
        public ToolboxItem(String name, String type)
        {
            otherAttributes = new Dictionary<String,String>();
            this.name = name;
            this.type = type;
        }
        /// <summary>
        /// Method to add a miscellanious attribute
        /// </summary>
        /// <param name="attName">Attribute Name</param>
        /// <param name="value">Value of new attribute</param>
        public void addAttribute(String attName, String value)
        {
            otherAttributes.Add(attName, value);
        }
        /// <summary>
        /// Sets attribute to new value.
        /// Raises exception if attribute does not exist (ie. check first, before you use)
        /// </summary>
        /// <param name="attName">Attribute name</param>
        /// <param name="newValue">New attribute value</param>
        public void setAttribute(String attName, String newValue)
        {
            otherAttributes[attName] = newValue;
        }
        /// <summary>
        /// Sets attribute to new value.
        /// Raises exception if attribute does not exist (ie. check first, before you use)
        /// </summary>
        /// <param name="attName">Attribute name</param>
        /// <param name="newValue">New attribute value</param>
        public String getAttribute(String attName) 
        {
            return otherAttributes[attName];
        }
        /// <summary>
        /// gets a list of miscellanious attributes
        /// </summary>
        /// <returns>Collection of attribute names</returns>
        public ICollection<String> getAttributeNames()
        {
            return otherAttributes.Keys;
        }
    }
}
