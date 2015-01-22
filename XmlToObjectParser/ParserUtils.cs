using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using ParatureAPI.ParaHelper;

namespace ParatureAPI.XmlToObjectParser
{
    /// <summary>
    /// This class helps parse raw XML responses returned from the server to hard typed objects that you can use for further processing.
    /// </summary>
    internal class ParserUtils
    {
        static internal PagedData.PagedData ObjectFillList(XmlDocument xmlresp, Boolean MinimalisticLoad, int requestdepth, ParaCredentials ParaCredentials, ParaEnums.ParatureModule module)
        {
            switch (module)
            {
                case ParaEnums.ParatureModule.Ticket:
                    return TicketParser.TicketsFillList(xmlresp, MinimalisticLoad, requestdepth, ParaCredentials);
                case ParaEnums.ParatureModule.Account:
                    return AccountParser.AccountsFillList(xmlresp, MinimalisticLoad, requestdepth, ParaCredentials);
                case ParaEnums.ParatureModule.Customer:
                    return CustomerParser.CustomersFillList(xmlresp, MinimalisticLoad, requestdepth, ParaCredentials);
                case ParaEnums.ParatureModule.Download:
                    return DownloadParser.DownloadsFillList(xmlresp, MinimalisticLoad, requestdepth, ParaCredentials);
                case ParaEnums.ParatureModule.Article:
                    return ArticleParser.ArticlesFillList(xmlresp, MinimalisticLoad, requestdepth, ParaCredentials);
                case ParaEnums.ParatureModule.Product:
                    return ProductParser.ProductsFillList(xmlresp, MinimalisticLoad, requestdepth, ParaCredentials);
                case ParaEnums.ParatureModule.Asset:
                    return AssetParser.AssetsFillList(xmlresp, MinimalisticLoad, requestdepth, ParaCredentials);
                //case Paraenums.ParatureModule.Chat:
                //    return ChatParser.ChatsFillList(xmlresp, false, false, requestdepth, ParaCredentials);
                default:
                    throw new Exception("Unknown Module For the Object Fill list");
            }
        }

        /// <summary>
        /// Accepts the string found in the XML and split it by "," and return a collection of string.
        /// Very useful in the scenario of CC email addresses.
        /// </summary>
        public static List<String> ParseStringCollection(string result)
        {
            var CCList = new ArrayList();
            if (string.IsNullOrEmpty(result) == false)
            {
                char[] splitter = { ',' };
                Array splitted = result.Split(splitter);
                for (var i = 0; i < splitted.Length; i++)
                {
                    CCList.Add(splitted.GetValue(i).ToString());
                }
                CCList.TrimToSize();
                return CCList.Cast<string>().ToList();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Indicates whether the object is fully loaded or not (if not loaded, means only the id and the name are filled)
        /// </summary>
        public static bool ObjectFullyLoaded(int childnode)
        {

            if (childnode > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// Allows for quickly checking if the node inner XML is null or not, in which case it will just return an empty string.
        /// </summary>
        public static string NodeGetInnerText(XmlNode Node)
        {
            if (Node.InnerText != null)
            {

                return HelperMethods.SafeHtmlDecode(Node.InnerXml.ToString());
            }

            else
            {
                return "";
            }


        }

        /// <summary>
        /// Before assigning a node attribute value, this method will check whether the node is null or not. Return True if the Attribute is not null, False if the Attribute is null.
        /// </summary>
        public static bool CheckNodeAttributeNotNull(XmlNode Node, string Attribute)
        {
            try
            {
                if (Node.Attributes[Attribute] != null)
                {
                    return true;

                }
                else
                {
                    return false;
                }
            }

            catch
            {
                return false;
            }


        }
    }
}
