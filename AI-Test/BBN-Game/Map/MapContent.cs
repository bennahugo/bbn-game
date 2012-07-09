using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Editor;
namespace BBN_Game.Map
{
    /// <summary>
    /// Base Class for the map system. Contains naming and miscellanious properties to be set by the map editor.
    /// Can think of this type as instances of the XML templates specified in the Content/MapItems folder.
    /// </summary>
    class MapContent
    {
        public Boolean isTemporary { get; private set; }
        /// <summary>
        /// Unique instance id
        /// </summary>
        public String id { get; private set; }
        /// <summary>
        /// name of the XML class the instance is instantiated from
        /// </summary>
        public String itemClassName { get; set; }
        /// <summary>
        /// Category type of the item (eg. scenery, AI, etc.)
        /// </summary>
        public String type { get; set; }
        /// <summary>
        /// Each type inherits miscellanious attributes from its XML parent
        /// </summary>
        private Dictionary<String, String> otherAttributes;
        /// <summary>
        /// Default constructor assigns unique id
        /// </summary>
        public MapContent(Boolean isTemporary)
        {
            otherAttributes = new Dictionary<String, String>();
            this.isTemporary = isTemporary;
            if (!isTemporary)
            {
                id = Convert.ToString(BBNMap.content.Count);

                for (int i = 0; BBNMap.content.Keys.Contains(id); i++)
                {
                    id = Convert.ToString(BBNMap.content.Count + i);
                }
                BBNMap.content.Add(id, this);
            }
        }
        /// <summary>
        /// Method to copy attributes from XML parent. MUST invoke this method upon construction in the
        /// map editor. If the object is loaded from a map the properties should be instantiated from the
        /// parent XML type and then set to new values accordingly.
        /// </summary>
        /// <param name="aTemplate">XML parent instance</param>
        public void copyPropertiesFromToolboxTemplate(ToolboxItem aTemplate)
        {
            this.itemClassName = aTemplate.name;
            this.type = aTemplate.type;
            foreach (String attribute in aTemplate.getAttributeNames())
                this.addAttribute(attribute, aTemplate.getAttribute(attribute));
            onAttributeChange();
        }
        /// <summary>
        /// Sets the instance id of the object. Must be unique.
        /// </summary>
        /// <param name="id">New id as string</param>
        /// <returns>retrurns false if unique id was not specified, true otherwise</returns>
        public bool setNewId(String id)
        {
            if (!isTemporary)
            {
                if (!BBNMap.content.Keys.Contains(id))
                {
                    BBNMap.content.Remove(this.id);
                    this.id = id;
                    BBNMap.content.Add(this.id, this);
                }
                else
                    return false;
            }
            else this.id = id;
            return true;
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
            String oldVal = otherAttributes[attName];
            try
            {
                otherAttributes[attName] = newValue;
                if (!BBNMap.isObjectInMap(this))
                    throw new Exception("Object is out of map bounds. Set the map radius to a bigger size if you wish to continue.");
                onAttributeChange();//Will abort if necessary
            }
            catch (Exception except)
            {
                otherAttributes[attName] = oldVal;
                onAttributeChange();//restore previous values
                throw except; //rethrow exception to GUI
            }
            
        }
        /// <summary>
        /// Method to get attribute value
        /// Raises exception if attribute does not exist (ie. check first, before you use)
        /// </summary>
        /// <param name="attName">Name of attribute to get</param>
        /// <returns>attribute value</returns>
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
        /// <summary>
        /// Method that gets invoked if one of the miscellanious attributes are changed.
        /// Should throw an exception if an error occurs so that the operation may be aborted
        /// </summary>
        public virtual void onAttributeChange()
        { /*Implement in child classes*/ }
    }
}
