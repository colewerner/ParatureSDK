using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using ParatureSDK.ParaHelper;

namespace ParatureSDK.XmlToObjectParser
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
                    return ParaEntityParser.FillList<ParaObjects.Ticket>(xmlresp);
                case ParaEnums.ParatureModule.Account:
                    return ParaEntityParser.FillList<ParaObjects.Account>(xmlresp);
                case ParaEnums.ParatureModule.Customer:
                    return ParaEntityParser.FillList<ParaObjects.Customer>(xmlresp);
                case ParaEnums.ParatureModule.Download:
                    return ParaEntityParser.FillList<ParaObjects.Download>(xmlresp);
                case ParaEnums.ParatureModule.Article:
                    return ParaEntityParser.FillList<ParaObjects.Article>(xmlresp);
                case ParaEnums.ParatureModule.Product:
                    return ParaEntityParser.FillList<ParaObjects.Product>(xmlresp);
                case ParaEnums.ParatureModule.Asset:
                    return ParaEntityParser.FillList<ParaObjects.Asset>(xmlresp);
                //case Paraenums.ParatureModule.Chat:
                //    return ChatParser.ChatsFillList(xmlresp, false, false, requestdepth, ParaCredentials);
                default:
                    throw new Exception("Unknown Module For the Object Fill list");
            }
        }
    }
}
