using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Collections;
using Microsoft.VisualBasic;

namespace ParatureAPI
{
    /// <summary>
    /// This class helps parse raw XML responses returned from the server to hard typed objects that you can use for further processing.
    /// </summary>
    internal class xmlToObjectParser
    {
        static internal ParaObjects.PagedData ObjectFillList(XmlDocument xmlresp, Boolean MinimalisticLoad, int requestdepth, ParaCredentials ParaCredentials, Paraenums.ParatureModule module)
        {
            switch (module)
            {
                case Paraenums.ParatureModule.Ticket:
                    return TicketParser.TicketsFillList(xmlresp, MinimalisticLoad, requestdepth, ParaCredentials);
                case Paraenums.ParatureModule.Account:
                    return AccountParser.AccountsFillList(xmlresp, MinimalisticLoad, requestdepth, ParaCredentials);
                case Paraenums.ParatureModule.Customer:
                    return CustomerParser.CustomersFillList(xmlresp, MinimalisticLoad, requestdepth, ParaCredentials);
                case Paraenums.ParatureModule.Download:
                    return DownloadParser.DownloadsFillList(xmlresp, MinimalisticLoad, requestdepth, ParaCredentials);
                case Paraenums.ParatureModule.Article:
                    return ArticleParser.ArticlesFillList(xmlresp, MinimalisticLoad, requestdepth, ParaCredentials);
                case Paraenums.ParatureModule.Product:
                    return ProductParser.ProductsFillList(xmlresp, MinimalisticLoad, requestdepth, ParaCredentials);
                case Paraenums.ParatureModule.Asset:
                    return AssetParser.AssetsFillList(xmlresp, MinimalisticLoad, requestdepth, ParaCredentials);
                //case Paraenums.ParatureModule.Chat:
                //    return ChatParser.ChatsFillList(xmlresp, false, false, requestdepth, ParaCredentials);
                default:
                    throw new Exception("Unknown Module For the Object Fill list");
            }
        }

        /// <summary>
        /// This class helps parse raw XML responses returned from the server to hard typed Account objects that you can use for further processing.
        /// </summary>
        internal partial class AccountParser
        {
            /// <summary>
            /// This methods requires an account xml file and returns an account object. It should only by used for a retrieve operation.
            /// </summary>
            static internal ParaObjects.Account AccountFill(XmlDocument xmlresp, int requestdepth, bool MinimalisticLoad, ParaCredentials ParaCredentials)
            {
                ParaObjects.Account account = new ParaObjects.Account();
                XmlNode AccountNode = xmlresp.DocumentElement;

                // Setting up the request level for all child items of an account.
                int childDepth = 0;
                if (requestdepth > 0)
                {
                    childDepth = requestdepth - 1;
                }

                account = AccountFillNode(AccountNode, childDepth, MinimalisticLoad, ParaCredentials);
                account.FullyLoaded = true;
                return account;
            }


            /// <summary>
            /// This methods requires an account list xml file and returns an AccountsList oject. It should only by used for a List operation.
            /// </summary>
            static internal ParaObjects.AccountsList AccountsFillList(XmlDocument xmlresp, Boolean MinimalisticLoad, int requestdepth, ParaCredentials ParaCredentials)
            {
                ParaObjects.AccountsList AccountsList = new ParaObjects.AccountsList();
                XmlNode DocNode = xmlresp.DocumentElement;

                // Setting up the request level for all child items of an account.
                int childDepth = 0;
                if (requestdepth > 0)
                {
                    childDepth = requestdepth - 1;
                }


                AccountsList.TotalItems = int.Parse(DocNode.Attributes["total"].InnerText.ToString());

                if (DocNode.Attributes["page-size"] != null)
                {
                    // If this is a "TotalOnly" request, there are no other attributes than "Total"
                    AccountsList.PageNumber = int.Parse(DocNode.Attributes["page"].InnerText.ToString());
                    AccountsList.PageSize = int.Parse(DocNode.Attributes["page-size"].InnerText.ToString());
                    AccountsList.ResultsReturned = int.Parse(DocNode.Attributes["results"].InnerText.ToString());
                }


                foreach (XmlNode xn in DocNode.ChildNodes)
                {
                    AccountsList.Accounts.Add(AccountFillNode(xn, childDepth, MinimalisticLoad, ParaCredentials));
                }
                return AccountsList;
            }

            /// <summary>
            /// This methods accepts an account node and parse through the different items in it. it can be used to parse an account node, whether the node is returned from a simple read, or as part of a list call.
            /// </summary>
            static internal ParaObjects.Account AccountFillNode(XmlNode AccountNode, int childDepth, bool MinimalisticLoad, ParaCredentials ParaCredentials)
            {
                ParaObjects.Account account = new ParaObjects.Account();
                account.Viewable_Account = new List<ParaObjects.Account>();

                bool isSchema = false;

                if (AccountNode.Attributes["id"] != null)
                {
                    account.Accountid = long.Parse(AccountNode.Attributes["id"].InnerText.ToString());
                    account.uniqueIdentifier = account.Accountid;
                    isSchema = false;
                }
                else
                {
                    isSchema = true;
                }

                if (AccountNode.Attributes["service-desk-uri"] != null)
                {
                    account.serviceDeskUri = AccountNode.Attributes["service-desk-uri"].InnerText.ToString();
                }

                account.FullyLoaded = true;
                foreach (XmlNode child in AccountNode.ChildNodes)
                {
                    if (isSchema == false)
                    {
                        if (child.LocalName.ToLower() == "account_name")
                        {
                            account.Account_Name = ParaHelper.HelperMethods.SafeHtmlDecode(child.InnerText.ToString());
                        }

                        if (child.LocalName.ToLower() == "date_created")
                        {
                            account.Date_Created = DateTime.Parse(child.InnerText.ToString());
                        }

                        if (child.LocalName.ToLower() == "date_updated")
                        {
                            account.Date_Updated = DateTime.Parse(child.InnerText.ToString());
                        }

                        if (child.LocalName.ToLower() == "owned_by")
                        {
                            if (child.ChildNodes[0].Attributes["id"] != null)
                            {
                                account.Owned_By.CsrID = int.Parse(child.ChildNodes[0].Attributes["id"].Value.ToString());
                                account.Owned_By.Full_Name = child.ChildNodes[0].ChildNodes[0].InnerText.ToString();
                            }
                        }

                        if (child.LocalName.ToLower() == "sla")
                        {
                            if (child.ChildNodes[0].Attributes["id"] != null)
                            {
                                account.Sla.SlaID = long.Parse(child.ChildNodes[0].Attributes["id"].Value.ToString());
                                account.Sla.Name = child.ChildNodes[0].ChildNodes[0].InnerText.ToString();
                            }
                        }

                        if (child.LocalName.ToLower() == "default_customer_role")
                        {
                            if (child.ChildNodes[0].Attributes["id"] != null)
                            {
                                account.Default_Customer_Role.RoleID = long.Parse(child.ChildNodes[0].Attributes["id"].Value.ToString());
                                account.Default_Customer_Role.Name = child.ChildNodes[0].InnerText.ToString();
                            }
                        }

                        if (child.LocalName.ToLower() == "modified_by")
                        {
                            if (child.ChildNodes[0].Attributes["id"] != null)
                            {
                                account.Modified_By.CsrID = int.Parse(child.ChildNodes[0].Attributes["id"].Value.ToString());
                                account.Modified_By.Full_Name = child.ChildNodes[0].ChildNodes[0].InnerText.ToString();
                            }
                        }
                        if (child.LocalName.ToLower() == "shown_accounts")
                        {
                            foreach (XmlNode vnode in child.ChildNodes)
                            {
                                if (vnode.LocalName.ToLower() == "account")
                                {
                                    ParaObjects.Account acc = new ParaObjects.Account();
                                    acc.Accountid = long.Parse(vnode.Attributes["id"].Value.ToString());
                                    acc.Account_Name = vnode.ChildNodes[0].InnerText.ToString();

                                    if (childDepth > 0)
                                    {
                                        acc = ApiHandler.Account.AccountGetDetails(acc.Accountid, ParaCredentials, (Paraenums.RequestDepth)childDepth);
                                    }

                                    acc.FullyLoaded = objectFullyLoaded(childDepth);

                                    account.Viewable_Account.Add(acc);

                                }
                            }


                        }
                    }

                    if (child.LocalName.ToLower() == "custom_field")
                    {
                        account.CustomFields.Add(CommonParser.FillCustomField(MinimalisticLoad, child));
                    }
                }
                return account;
            }

        }

        /// <summary>
        /// This class helps parse raw XML responses returned from the server to hard typed Customer objects that you can use for further processing.
        /// </summary>
        internal partial class CustomerParser
        {
            /// <summary>
            /// This methods requires a Customer xml file and returns a customer object. It should only by used for a retrieve operation.
            /// </summary>
            static internal ParaObjects.Customer CustomerFill(XmlDocument xmlresp, int requestdepth, bool includeAllCustomFields, ParaCredentials ParaCredentials)
            {
                ParaObjects.Customer Customer = new ParaObjects.Customer();
                XmlNode CustomerNode = xmlresp.DocumentElement;

                // Setting up the request level for all child items of an account.
                int childDepth = 0;
                if (requestdepth > 0)
                {
                    childDepth = requestdepth - 1;
                }
                Customer = CustomerFillNode(CustomerNode, childDepth, includeAllCustomFields, ParaCredentials);
                Customer.FullyLoaded = true;
                return Customer;
            }

            /// <summary>
            /// This methods requires a Customer list xml file and returns an CustomersList oject. It should only by used for a List operation.
            /// </summary>
            static internal ParaObjects.CustomersList CustomersFillList(XmlDocument xmlresp, Boolean MinimalisticLoad, int requestdepth, ParaCredentials ParaCredentials)
            {
                ParaObjects.CustomersList CustomersList = new ParaObjects.CustomersList();
                XmlNode DocNode = xmlresp.DocumentElement;

                // Setting up the request level for all child items of an account.
                int childDepth = 0;
                if (requestdepth > 0)
                {
                    childDepth = requestdepth - 1;
                }


                CustomersList.TotalItems = int.Parse(DocNode.Attributes["total"].InnerText.ToString());


                if (DocNode.Attributes["page-size"] != null)
                {
                    // If this is a "TotalOnly" request, there are no other attributes than "Total"

                    CustomersList.PageNumber = int.Parse(DocNode.Attributes["page"].InnerText.ToString());
                    CustomersList.PageSize = int.Parse(DocNode.Attributes["page-size"].InnerText.ToString());
                    CustomersList.ResultsReturned = int.Parse(DocNode.Attributes["results"].InnerText.ToString());
                }



                foreach (XmlNode xn in DocNode.ChildNodes)
                {
                    CustomersList.Customers.Add(CustomerFillNode(xn, childDepth, MinimalisticLoad, ParaCredentials));
                }
                return CustomersList;
            }

            /// <summary>
            /// This methods accepts a customer node and parse through the different items in it. it can be used to parse a customer node, whether the node is returned from a simple read, or as part of a list call.
            /// </summary>
            static internal ParaObjects.Customer CustomerFillNode(XmlNode CustomerNode, int childDepth, Boolean MinimalisticLoad, ParaCredentials ParaCredentials)
            {

                ParaObjects.Customer Customer = new ParaObjects.Customer();
                bool isSchema = false;
                if (CustomerNode.Attributes["id"] != null)
                {
                    isSchema = false;
                    Customer.customerid = long.Parse(CustomerNode.Attributes["id"].InnerText.ToString());
                    Customer.uniqueIdentifier = Customer.customerid;
                }
                else
                {
                    isSchema = true;
                }

                if (CustomerNode.Attributes["service-desk-uri"] != null)
                {
                    Customer.serviceDeskUri = CustomerNode.Attributes["service-desk-uri"].InnerText.ToString();
                }

                foreach (XmlNode child in CustomerNode.ChildNodes)
                {
                    if (isSchema == false)
                    {
                        if (child.LocalName.ToLower() == "status")
                        {
                            if (child.ChildNodes[0] != null && child.ChildNodes[0].Attributes["id"] != null)
                            {
                                Customer.Status.StatusID = int.Parse(child.ChildNodes[0].Attributes["id"].Value.ToString());
                                Customer.Status.Name = child.ChildNodes[0].ChildNodes[0].InnerText.ToString();
                            }
                        }
                        if (child.LocalName.ToLower() == "account")
                        {
                            // Fill the account details
                            Customer.Account = new ParaObjects.Account();
                            Customer.Account = AccountParser.AccountFillNode(child.ChildNodes[0], childDepth, MinimalisticLoad, ParaCredentials);
                            if (childDepth > 0)
                            {
                                Customer.Account = ApiHandler.Account.AccountGetDetails(Customer.Account.Accountid, ParaCredentials, (Paraenums.RequestDepth)childDepth);
                            }

                            Customer.Account.FullyLoaded = objectFullyLoaded(childDepth);
                        }

                        if (child.LocalName.ToLower() == "customer_role")
                        {
                            if (child.ChildNodes[0] != null && child.ChildNodes[0].Attributes["id"] != null)
                            {
                                Customer.Customer_Role.RoleID = long.Parse(child.ChildNodes[0].Attributes["id"].Value.ToString());
                                Customer.Customer_Role.Name = child.ChildNodes[0].ChildNodes[0].InnerText.ToString();
                            }
                        }

                        if (child.LocalName.ToLower() == "sla")
                        {
                            if (child.ChildNodes[0] != null && child.ChildNodes[0].Attributes["id"] != null)
                            {
                                Customer.Sla.SlaID = long.Parse(child.ChildNodes[0].Attributes["id"].Value.ToString());
                                Customer.Sla.Name = child.ChildNodes[0].ChildNodes[0].InnerText.ToString();
                            }
                        }
                        if (child.LocalName.ToLower() == "date_visited")
                        {
                            Customer.Date_Visited = DateTime.Parse(NodeGetInnerText(child));
                        }


                        if (child.LocalName.ToLower() == "date_created")
                        {
                            Customer.Date_Created = DateTime.Parse(NodeGetInnerText(child));
                        }

                        if (child.LocalName.ToLower() == "date_updated")
                        {
                            Customer.Date_Updated = DateTime.Parse(NodeGetInnerText(child));
                        }

                        if (child.LocalName.ToLower() == "email")
                        {
                            Customer.Email = ParaHelper.HelperMethods.SafeHtmlDecode(NodeGetInnerText(child));
                        }
                        if (child.LocalName.ToLower() == "first_name")
                        {
                            Customer.First_Name = ParaHelper.HelperMethods.SafeHtmlDecode(NodeGetInnerText(child));
                        }
                        if (child.LocalName.ToLower() == "last_name")
                        {
                            Customer.Last_Name = ParaHelper.HelperMethods.SafeHtmlDecode(NodeGetInnerText(child));
                        }
                        if (child.LocalName.ToLower() == "user_name")
                        {
                            Customer.User_Name = ParaHelper.HelperMethods.SafeHtmlDecode(NodeGetInnerText(child));
                        }
                    }

                    if (child.LocalName.ToLower() == "custom_field")
                    {
                        Customer.CustomFields.Add(CommonParser.FillCustomField(MinimalisticLoad, child));
                    }

                }
                return Customer;
            }
        }



        /// <summary>
        /// This class helps parse raw XML responses returned from the server to hard typed Chat objects that you can use for further processing.
        /// </summary>
        internal partial class ChatParser
        {
            /// <summary>
            /// This methods requires a Chat xml file and returns a customer object. It should only by used for a retrieve operation.
            /// </summary>
            static internal ParaObjects.Chat ChatFill(XmlDocument xmlresp, Boolean MinimalisticLoad, int requestdepth, bool includeTranscripts, ParaCredentials ParaCredentials)
            {
                ParaObjects.Chat chat = new ParaObjects.Chat();
                XmlNode ObjectNode = xmlresp.DocumentElement;

                // Setting up the request level for all child items of an account.
                int childDepth = 0;
                if (requestdepth > 0)
                {
                    childDepth = requestdepth - 1;
                }
                chat = ChatFillNode(ObjectNode,MinimalisticLoad, childDepth, includeTranscripts, ParaCredentials);
                chat.FullyLoaded = true;
                return chat;
            }

            /// <summary>
            /// This methods requires a Chat list xml file and returns a ChatsList oject. It should only by used for a List operation.
            /// </summary>
            static internal ParaObjects.ChatList ChatsFillList(XmlDocument xmlresp, Boolean MinimalisticLoad, bool includeTranscripts, int requestdepth, ParaCredentials ParaCredentials)
            {
                ParaObjects.ChatList ChatsList = new ParaObjects.ChatList();
                XmlNode DocNode = xmlresp.DocumentElement;

                // Setting up the request level for all child items of an account.
                int childDepth = 0;
                if (requestdepth > 0)
                {
                    childDepth = requestdepth - 1;
                }




                ChatsList.TotalItems = int.Parse(DocNode.Attributes["total"].InnerText.ToString());


                if (DocNode.Attributes["page-size"] != null)
                {
                    // If this is a "TotalOnly" request, there are no other attributes than "Total"

                    ChatsList.PageNumber = int.Parse(DocNode.Attributes["page"].InnerText.ToString());
                    ChatsList.PageSize = int.Parse(DocNode.Attributes["page-size"].InnerText.ToString());
                    ChatsList.ResultsReturned = int.Parse(DocNode.Attributes["results"].InnerText.ToString());
                }



                foreach (XmlNode xn in DocNode.ChildNodes)
                {
                    ChatsList.chats.Add(ChatFillNode(xn, MinimalisticLoad, childDepth, includeTranscripts, ParaCredentials));
                }
                return ChatsList;
            }

            /// <summary>
            /// This methods accepts a Chat node and parse through the different items in it. it can be used to parse a Chat node, whether the node is returned from a simple read, or as part of a list call.
            /// </summary>
            static internal ParaObjects.Chat ChatFillNode(XmlNode ChatNode, Boolean MinimalisticLoad, int childDepth, bool includeTranscripts, ParaCredentials ParaCredentials)
            {

                ParaObjects.Chat chat = new ParaObjects.Chat();
                bool isSchema = false;
                if (ChatNode.Attributes["id"] != null)
                {
                    isSchema = false;
                    chat.ChatID = long.Parse(ChatNode.Attributes["id"].InnerText.ToString());
                    chat.uniqueIdentifier = chat.ChatID;
                }
                else
                {
                    isSchema = true;
                }

                if (ChatNode.Attributes["service-desk-uri"] != null)
                {
                    chat.serviceDeskUri = ChatNode.Attributes["service-desk-uri"].InnerText.ToString();
                }

                foreach (XmlNode child in ChatNode.ChildNodes)
                {
                    if (isSchema == false)
                    {

                        if (child.LocalName.ToLower() == "browser_language")
                        {
                            chat.Browser_Language = NodeGetInnerText(child);
                        }


                        if (child.LocalName.ToLower() == "browser_type")
                        {
                            chat.Browser_Type = NodeGetInnerText(child);
                        }


                        if (child.LocalName.ToLower() == "browser_version")
                        {
                            chat.Browser_Version = NodeGetInnerText(child);
                        }


                        if (child.LocalName.ToLower() == "chat_number")
                        {
                            Int64.TryParse(NodeGetInnerText(child), out chat.Chat_Number);
                        }

                        if (child.LocalName.ToLower() == "customer")
                        {
                            // Fill the Customer details
                            ParaObjects.Customer Customer = new ParaObjects.Customer();

                            chat.customer = CustomerParser.CustomerFillNode(child.ChildNodes[0], childDepth, true, ParaCredentials);
                            if (childDepth > 0)
                            {
                                chat.customer = ApiHandler.Customer.CustomerGetDetails(chat.customer.customerid, ParaCredentials, (Paraenums.RequestDepth)childDepth);
                            }
                            chat.customer.FullyLoaded = objectFullyLoaded(childDepth);
                        }

                        if (child.LocalName.ToLower() == "email")
                        {
                            chat.Email = NodeGetInnerText(child);
                        }

                        if (child.LocalName.ToLower() == "date_created")
                        {
                            chat.Date_Created = DateTime.Parse(NodeGetInnerText(child));
                        }

                        if (child.LocalName.ToLower() == "date_ended")
                        {
                            chat.Date_Ended = DateTime.Parse(NodeGetInnerText(child));
                        }

                        if (child.LocalName.ToLower() == "initial_csr")
                        {
                            if (child.ChildNodes[0] != null && child.ChildNodes[0].Attributes["id"] != null)
                            {
                                chat.Initial_Csr.CsrID = int.Parse(child.ChildNodes[0].Attributes["id"].Value.ToString());
                                chat.Initial_Csr.Full_Name = child.ChildNodes[0].ChildNodes[0].InnerText.ToString();
                            }
                        }

                        if (child.LocalName.ToLower() == "ip_address")
                        {
                            chat.Ip_Address = NodeGetInnerText(child);
                        }

                        if (child.LocalName.ToLower() == "is_anonymous")
                        {
                            Boolean.TryParse(NodeGetInnerText(child), out chat.Is_Anonymous);
                        }

                        if (child.LocalName.ToLower() == "referrer_url")
                        {
                            chat.Referrer_Url = NodeGetInnerText(child);
                        }

                        if (child.LocalName.ToLower() == "related_tickets")
                        {
                            if (child.ChildNodes[0] != null && child.ChildNodes[0].Attributes["id"] != null)
                            {
                                Int32 counter = 0;
                                chat.Related_Tickets = new List<ParaObjects.Ticket>();

                                while (child.ChildNodes[counter] != null && child.ChildNodes[counter].Attributes["id"] != null)
                                {
                                    ParaObjects.Ticket ticket = new ParaObjects.Ticket();

                                    ticket.id = int.Parse(child.ChildNodes[counter].Attributes["id"].Value.ToString());
                                    ticket.Ticket_Number = child.ChildNodes[counter].ChildNodes[0].InnerText.ToString();

                                    chat.Related_Tickets.Add(ticket);

                                    counter++;
                                }
                            }
                        }

                        if (child.LocalName.ToLower() == "sla_violations")
                        {
                            Int32.TryParse(NodeGetInnerText(child), out chat.Sla_Violation);
                        }
                        
                        if (child.LocalName.ToLower() == "status")
                        {
                            if (child.ChildNodes[0] != null && child.ChildNodes[0].Attributes["id"] != null)
                            {
                                chat.Status.StatusID = int.Parse(child.ChildNodes[0].Attributes["id"].Value.ToString());
                                chat.Status.Name = child.ChildNodes[0].ChildNodes[0].InnerText.ToString();
                            }
                        }

                        if (child.LocalName.ToLower() == "summary")
                        {
                            chat.Summary = NodeGetInnerText(child);
                        }
                        if (child.LocalName.ToLower() == "user_agent")
                        {
                            chat.User_Agent = NodeGetInnerText(child);
                        }

                    }

                    if (child.LocalName.ToLower() == "custom_field")
                    {
                        chat.CustomFields.Add(CommonParser.FillCustomField(MinimalisticLoad, child));
                    }
                }

                if (includeTranscripts && chat.ChatID>0)
                {
                    // Load transcripts
                    chat.ChatTranscripts = ApiHandler.Chat.ChatTranscripts(chat.ChatID, ParaCredentials);
                }

                return chat;
            }

            static internal List<ParaObjects.ChatTranscript> ChatTranscriptsFillList(XmlDocument ChatTranscriptDoc)
            {
                List<ParaObjects.ChatTranscript> transcripts = new List<ParaObjects.ChatTranscript>();

                XmlNode ChatTranscriptNode = ChatTranscriptDoc.DocumentElement;

                foreach (XmlNode xn in ChatTranscriptNode.ChildNodes)
                {
                    ParaObjects.ChatTranscript transcript = new ParaObjects.ChatTranscript();
                    transcript.isInternal = true;
                    if (xn.Attributes["internal"] != null)
                    {
                        Boolean.TryParse(xn.Attributes["internal"].InnerText.ToString(), out transcript.isInternal);
                    }
                    foreach (XmlNode xnc in xn.ChildNodes)
                    {
                        // Looking at each message nodes
                        switch (xnc.LocalName.ToLower())
                            {
                            case "system":
                                    transcript.performer = Paraenums.ActionHistoryPerformerType.System;
                                    break;

                            case "customer":
                                    transcript.performer = Paraenums.ActionHistoryPerformerType.Customer;
                                    transcript.customerName = NodeGetInnerText(xnc);
                                    break;
                            case "csr":
                                    transcript.performer = Paraenums.ActionHistoryPerformerType.Csr;
                                    transcript.csrName = NodeGetInnerText(xnc);
                                    break;

                            case "text":
                                    transcript.Text = NodeGetInnerText(xnc);
                                    break;
                            case "timestamp":
                                    DateTime.TryParse(NodeGetInnerText(xnc), out transcript.Timestamp);
                                    break;
                            }   
                    }

                    transcripts.Add(transcript);
                }

                return transcripts;
            }
        }

        /// <summary>
        /// This class helps parse raw XML responses returned from the server to hard typed Ticket objects that you can use for further processing.
        /// </summary>
        internal partial class TicketParser
        {
            /// <summary>
            /// This methods requires a ticket xml file and returns an ticket object. It should only by used for a retrieve operation.
            /// </summary>
            static internal ParaObjects.Ticket TicketFill(XmlDocument xmlresp, int requestdepth, bool includeAllCustomFields, ParaCredentials ParaCredentials)
            {
                ParaObjects.Ticket Ticket = new ParaObjects.Ticket();
                XmlNode TicketNode = xmlresp.DocumentElement;

                // Setting up the request level for all child items of an account.
                int childDepth = 0;
                if (requestdepth > 0)
                {
                    childDepth = requestdepth - 1;
                }
                Ticket = TicketFillNode(TicketNode, childDepth, includeAllCustomFields, ParaCredentials);
                Ticket.FullyLoaded = true;
                return Ticket;
            }

            /// <summary>
            /// This methods requires a Ticket list xml file and returns a TicketsList oject. It should only by used for a List operation.
            /// </summary>
            static internal ParaObjects.TicketsList TicketsFillList(XmlDocument xmlresp, Boolean MinimalisticLoad, int requestdepth, ParaCredentials ParaCredentials)
            {
                ParaObjects.TicketsList TicketsList = new ParaObjects.TicketsList();
                XmlNode DocNode = xmlresp.DocumentElement;

                int childDepth = 0;
                if (requestdepth > 0)
                {
                    childDepth = requestdepth - 1;
                }

                TicketsList.TotalItems = int.Parse(DocNode.Attributes["total"].InnerText.ToString());

                if (DocNode.Attributes["page-size"] != null)
                {
                    // If this is a "TotalOnly" request, there are no other attributes than "Total"
                    TicketsList.PageSize = int.Parse(DocNode.Attributes["page-size"].InnerText.ToString());
                    TicketsList.ResultsReturned = int.Parse(DocNode.Attributes["results"].InnerText.ToString());
                    TicketsList.PageNumber = int.Parse(DocNode.Attributes["page"].InnerText.ToString());
                }


                foreach (XmlNode xn in DocNode.ChildNodes)
                {
                    TicketsList.Tickets.Add(TicketFillNode(xn, childDepth, MinimalisticLoad, ParaCredentials));
                }
                return TicketsList;
            }

            /// <summary>
            /// This methods accepts a ticket node and parse through the different items in it. it can be used to parse a ticket node, whether the node is returned from a simple read, or as part of a list call.
            /// </summary>
            static internal ParaObjects.Ticket TicketFillNode(XmlNode Node, int childDepth, bool MinimalisticLoad, ParaCredentials ParaCredentials)
            {

                ParaObjects.Ticket Ticket = new ParaObjects.Ticket();
                bool isSchema = false;
                if (Node.Attributes["id"] != null)
                {
                    Ticket.id = long.Parse(Node.Attributes["id"].InnerText.ToString());
                    Ticket.uniqueIdentifier = Ticket.id;
                    Ticket.Ticket_Parent = new ParaObjects.Ticket();
                    isSchema = false;
                }
                else
                {
                    isSchema = true;
                }

                if (Node.Attributes["service-desk-uri"] != null)
                {
                    Ticket.serviceDeskUri = Node.Attributes["service-desk-uri"].InnerText.ToString();
                }

                foreach (XmlNode child in Node.ChildNodes)
                {
                    if (isSchema == false)
                    {
                        if (child.LocalName.ToLower() == "ticket_status")
                        {
                            Ticket.Ticket_Status.StatusID = int.Parse(child.ChildNodes[0].Attributes["id"].Value);
                            Ticket.Ticket_Status.Name = child.ChildNodes[0].ChildNodes[0].InnerText.ToString();
                        }
                        if (child.LocalName.ToLower() == "ticket_number")
                        {
                            Ticket.Ticket_Number = NodeGetInnerText(child);
                        }

                        //Take care of the attachments.
                        if (child.LocalName.ToLower() == "ticket_attachments")
                        {
                            Ticket.Ticket_Attachments = CommonParser.FillAttachments(child);
                        }

                        if (child.LocalName.ToLower() == "entered_by")
                        {
                            Ticket.Entered_By.CsrID = int.Parse(child.ChildNodes[0].Attributes["id"].Value);
                            Ticket.Entered_By.Full_Name = child.ChildNodes[0].ChildNodes[0].InnerText.ToString();
                        }

                        if (child.LocalName.ToLower() == "assigned_to")
                        {
                            Ticket.Assigned_To.CsrID = int.Parse(child.ChildNodes[0].Attributes["id"].Value);
                            Ticket.Assigned_To.Full_Name = child.ChildNodes[0].ChildNodes[0].InnerText.ToString();
                        }
                        if (child.LocalName.ToLower() == "email_notification")
                        {
                            Ticket.Email_Notification = bool.Parse(NodeGetInnerText(child));
                        }

                        // DJERAME
                        if (child.LocalName.ToLower() == "email_notification_additional_contact")
                        {
                            Ticket.Email_Notification_Additional_Contact = bool.Parse(NodeGetInnerText(child));
                        }

                        if (child.LocalName.ToLower() == "hide_from_customer")
                        {
                            Ticket.Hide_From_Customer = bool.Parse(NodeGetInnerText(child));
                        }

                        if (child.LocalName.ToLower() == "tou")
                        {
                            Ticket.Email_Notification = bool.Parse(NodeGetInnerText(child));
                        }

                        if (child.LocalName.ToLower() == "date_created")
                        {
                            Ticket.Date_Created = NodeGetInnerText(child);
                        }
                        if (child.LocalName.ToLower() == "date_updated")
                        {
                            Ticket.Date_Updated = NodeGetInnerText(child);
                        }
                        if (child.LocalName.ToLower() == "cc_customer")
                        {
                            // List of CCed Customers

                            string result = NodeGetInnerText(child);
                            if (string.IsNullOrEmpty(result) == false)
                            {
                                Ticket.Cc_Customer = ParseStringCollection(result);
                            }
                        }
                        if (child.LocalName.ToLower() == "cc_csr")
                        {
                            // List of CCed CSRs

                            string result = NodeGetInnerText(child);
                            if (string.IsNullOrEmpty(result) == false)
                            {
                                Ticket.Cc_Csr = ParseStringCollection(result);
                            }
                        }

                        if (child.LocalName.ToLower() == "ticket_customer")
                        {
                            // Fill the Customer details
                            ParaObjects.Customer Customer = new ParaObjects.Customer();

                            Ticket.Ticket_Customer = CustomerParser.CustomerFillNode(child.ChildNodes[0], childDepth, MinimalisticLoad, ParaCredentials);
                            if (childDepth > 0)
                            {
                                Ticket.Ticket_Customer = ApiHandler.Customer.CustomerGetDetails(Ticket.Ticket_Customer.customerid, ParaCredentials, (Paraenums.RequestDepth)childDepth);
                            }
                            Ticket.Ticket_Customer.FullyLoaded = objectFullyLoaded(childDepth);
                        }


                        if (child.LocalName.ToLower() == "additional_contact")
                        {
                            // Fill the Customer details
                            ParaObjects.Customer Customer = new ParaObjects.Customer();

                            Ticket.Additional_Contact = CustomerParser.CustomerFillNode(child.ChildNodes[0], childDepth, MinimalisticLoad, ParaCredentials);
                            if (childDepth > 0)
                            {
                                Ticket.Additional_Contact = ApiHandler.Customer.CustomerGetDetails(Ticket.Additional_Contact.customerid, ParaCredentials, (Paraenums.RequestDepth)childDepth);
                            }
                            Ticket.Additional_Contact.FullyLoaded = objectFullyLoaded(childDepth);
                        }
                        
                        if (child.LocalName.ToLower() == "ticket_parent")
                        {
                            // Fill the Parent Ticket details
                            string result = child.ChildNodes[0].Attributes["href"].InnerText.ToString();
                            if (string.IsNullOrEmpty(result) == false)
                            {
                                ParaObjects.Ticket PTicket = new ParaObjects.Ticket();

                                if (child.ChildNodes[0].HasChildNodes && child.ChildNodes[0].ChildNodes[0].LocalName.ToLower() == "ticket_number")
                                {
                                    PTicket.Ticket_Number = NodeGetInnerText(child.ChildNodes[0].ChildNodes[0]);
                                }

                                char[] splitter = { '/' };
                                String[] hrefs;
                                hrefs = result.Split(splitter);
                                PTicket.id = long.Parse(child.ChildNodes[0].Attributes["id"].InnerText.ToString());
                                PTicket.Department.DepartmentID = long.Parse(hrefs[hrefs.Length - 3]);
                                if (childDepth > 0)
                                {
                                    PTicket = ApiHandler.Ticket.TicketGetDetails(PTicket.id, false, ParaCredentials, (Paraenums.RequestDepth)childDepth);
                                }
                                PTicket.Ticket_Number = child.ChildNodes[0].ChildNodes[0].InnerText.ToString();
                                PTicket.FullyLoaded = objectFullyLoaded(childDepth);
                                Ticket.Ticket_Parent = PTicket;
                            }

                        }
                        if (child.LocalName.ToLower() == "ticket_children")
                        {
                            List<ParaObjects.Ticket> Ticket_Children = new List<ParaObjects.Ticket>();

                            foreach (XmlNode ChildTicketNode in child.ChildNodes)
                            {
                                string result = ChildTicketNode.Attributes["href"].InnerText.ToString();
                                
                                ParaObjects.Ticket ChildTicket = new ParaObjects.Ticket();

                                if (ChildTicketNode.HasChildNodes && ChildTicketNode.ChildNodes[0].LocalName.ToLower() == "ticket_number")
                                {
                                    ChildTicket.Ticket_Number = NodeGetInnerText(ChildTicketNode.ChildNodes[0]);
                                }

                                char[] splitter = { '/' };
                                String[] hrefs;
                                hrefs = result.Split(splitter);
                                ChildTicket.id = long.Parse(hrefs[hrefs.Length - 1]);
                                ChildTicket.Department.DepartmentID = long.Parse(hrefs[hrefs.Length - 3]);
                                
                                if (childDepth > 0)
                                {
                                    ChildTicket = ApiHandler.Ticket.TicketGetDetails(ChildTicket.id, false, ParaCredentials, (Paraenums.RequestDepth)childDepth);
                                }
                                ChildTicket.FullyLoaded = objectFullyLoaded(childDepth);
                                Ticket_Children.Add(ChildTicket);
                            }
                            Ticket.Ticket_Children = Ticket_Children;
                        }

                        if (child.LocalName.ToLower() == "related_chats")
                        {
                            var Related_Chats = new List<ParaObjects.Chat>();

                            foreach (XmlNode chatNode in child.ChildNodes)
                            {
                                var chat = new ParaObjects.Chat();
                                chat.ChatID = int.Parse(chatNode.Attributes["id"].Value);
                                chat.Chat_Number = int.Parse(chatNode.Attributes["id"].Value);
                                Related_Chats.Add(chat);
                            }

                            Ticket.Related_Chats = Related_Chats;
                        }

                        if (child.LocalName.ToLower() == "department")
                        {
                            Ticket.Department.DepartmentID = int.Parse(child.ChildNodes[0].Attributes["id"].Value);
                            Ticket.Department.Name = child.ChildNodes[0].ChildNodes[0].InnerText.ToString();
                        }

                        if (child.LocalName.ToLower() == "ticket_queue")
                        {
                            Ticket.Ticket_Queue.QueueID = int.Parse(child.ChildNodes[0].Attributes["id"].Value);
                            Ticket.Ticket_Queue.Name = child.ChildNodes[0].ChildNodes[0].InnerText.ToString();
                        }
                        if (child.LocalName.ToLower() == "ticket_asset")
                        {
                            if (childDepth > 0)
                            {
                                //TODO, Call Asset
                                Ticket.Ticket_Asset.Assetid = int.Parse(child.ChildNodes[0].Attributes["id"].Value);
                                Ticket.Ticket_Asset.uniqueIdentifier = int.Parse(child.ChildNodes[0].Attributes["id"].Value);
                            }
                            else
                            {
                                Ticket.Ticket_Asset.Assetid = int.Parse(child.ChildNodes[0].Attributes["id"].Value);
                                Ticket.Ticket_Asset.uniqueIdentifier = int.Parse(child.ChildNodes[0].Attributes["id"].Value);
                            }
                            Ticket.Ticket_Asset.FullyLoaded = objectFullyLoaded(childDepth);
                        }

                        if (child.LocalName.ToLower() == "ticket_product")
                        {
                            Ticket.Ticket_Product.productid = int.Parse(child.ChildNodes[0].Attributes["id"].Value);

                            if (childDepth > 0)
                            {
                                Ticket.Ticket_Product = ApiHandler.Product.ProductGetDetails(Ticket.Ticket_Product.productid, ParaCredentials, (Paraenums.RequestDepth)childDepth - 1);
                            }
                            Ticket.Ticket_Product.FullyLoaded = objectFullyLoaded(childDepth);
                        }

                        if (child.LocalName.ToLower() == "ticket_sla")
                        {
                            if (child.ChildNodes[0] != null && child.ChildNodes[0].Attributes["id"] != null)
                            {
                                Ticket.Ticket_Sla.SlaID = long.Parse(child.ChildNodes[0].Attributes["id"].Value.ToString());
                                Ticket.Ticket_Sla.Name = child.ChildNodes[0].ChildNodes[0].InnerText.ToString();
                            }
                        }

                        if (child.LocalName.ToLower() == "actions")
                        {

                            foreach (XmlNode actionchild in child.ChildNodes)
                            {
                                ParaObjects.Action availableaction = new ParaObjects.Action();
                                availableaction.ActionID = int.Parse(actionchild.Attributes["id"].Value);
                                availableaction.ActionName = actionchild.Attributes["name"].Value.ToString();
                                if (childDepth > 0)
                                {
                                    // WHEN WE PARSE ACTIONS, RETURN THIS ONE.

                                }
                                availableaction.FullyLoaded = false;
                                Ticket.Actions.Add(availableaction);
                            }
                        }
                        if (child.LocalName.ToLower() == "actionhistory")
                        {
                            Ticket.ActionHistory = CommonParser.FillActionHistory(child);
                        }

                    }


                    if (child.LocalName.ToLower() == "custom_field")
                    {
                        Ticket.CustomFields.Add(CommonParser.FillCustomField(MinimalisticLoad, child));
                    }


                }
                return Ticket;
            }


            internal partial class TicketStatusParser
            {
                /// <summary>
                /// This methods requires a DownloadFolder xml file and returns a DownloadFolder object. It should only by used for a retrieve operation.
                /// </summary>
                static internal ParaObjects.TicketStatus TicketStatusFill(XmlDocument xmlresp, ParaCredentials ParaCredentials)
                {
                    ParaObjects.TicketStatus ticketstatus = new ParaObjects.TicketStatus();
                    XmlNode MainNode = xmlresp.DocumentElement;

                    ticketstatus = TicketStatusFillNode(MainNode, 0);

                    return ticketstatus;
                }

                /// <summary>
                /// This method requires a DownloadFolder list xml file and returns a DownloadFoldersList object. It should only by used for a List operation.
                /// </summary>
                static internal ParaObjects.TicketStatusList TicketStatusFillList(XmlDocument xmlresp, int requestdepth, ParaCredentials ParaCredentials)
                {
                    ParaObjects.TicketStatusList TicketStatusList = new ParaObjects.TicketStatusList();
                    XmlNode DocNode = xmlresp.DocumentElement;

                    // Setting up the request level for all child items of a DownloadFolder.

                    TicketStatusList.TotalItems = int.Parse(DocNode.Attributes["total"].InnerText.ToString());

                    if (DocNode.Attributes["page-size"] != null)
                    {
                        // If this is a "TotalOnly" request, there are no other attributes than "Total"
                        TicketStatusList.PageNumber = int.Parse(DocNode.Attributes["page"].InnerText.ToString());
                        TicketStatusList.PageSize = int.Parse(DocNode.Attributes["page-size"].InnerText.ToString());
                        TicketStatusList.ResultsReturned = int.Parse(DocNode.Attributes["results"].InnerText.ToString());
                    }



                    foreach (XmlNode xn in DocNode.ChildNodes)
                    {
                        TicketStatusList.TicketStatuses.Add(TicketStatusFillNode(xn, 0));
                    }
                    return TicketStatusList;
                }

                /// <summary>
                /// This method accepts a DownloadFolder node and parses through the different items in it. it can be used to parse a DownloadFolder node, whether the node is returned from a simple read, or as part of a list call.
                /// </summary>
                static internal ParaObjects.TicketStatus TicketStatusFillNode(XmlNode Node, int childDepth)
                {

                    ParaObjects.TicketStatus TicketStatus = new ParaObjects.TicketStatus();
                    TicketStatus.StatusID = long.Parse(Node.Attributes["id"].InnerText.ToString());
                    TicketStatus.StatusType = (Paraenums.TicketStatusType)Enum.Parse(typeof(Paraenums.TicketStatusType), Node.Attributes["status-type"].InnerText.ToString());
                    foreach (XmlNode child in Node.ChildNodes)
                    {
                        if (child.LocalName.ToLower() == "customer_text")
                        {
                            TicketStatus.Customer_Text = NodeGetInnerText(child);
                        }

                        if (child.LocalName.ToLower() == "name")
                        {
                            TicketStatus.Name = NodeGetInnerText(child);
                        }

                    }
                    return TicketStatus;
                }
            }

        }

        /// <summary>
        /// This class helps parse raw XML responses returned from the server to hard typed Product objects that you can use for further processing.
        /// </summary>
        internal partial class ProductParser
        {
            /// <summary>
            /// This methods requires a Product xml file and returns a product object. It should only by used for a retrieve operation.
            /// </summary>
            static internal ParaObjects.Product ProductFill(XmlDocument xmlresp, int requestdepth, bool MinimalisticLoad, ParaCredentials ParaCredentials)
            {
                ParaObjects.Product Product = new ParaObjects.Product();
                XmlNode ProductNode = xmlresp.DocumentElement;

                // Setting up the request level for all child items of an account.
                int childDepth = 0;
                if (requestdepth > 0)
                {
                    childDepth = requestdepth - 1;
                }
                Product = ProductFillNode(ProductNode, childDepth, MinimalisticLoad, ParaCredentials);
                Product.FullyLoaded = true;
                return Product;
            }


            /// <summary>
            /// This methods requires a Product list xml file and returns a ProductsList oject. It should only by used for a List operation.
            /// </summary>
            static internal ParaObjects.ProductsList ProductsFillList(XmlDocument xmlresp, Boolean MinimalisticLoad, int requestdepth, ParaCredentials ParaCredentials)
            {
                ParaObjects.ProductsList ProductsList = new ParaObjects.ProductsList();
                XmlNode DocNode = xmlresp.DocumentElement;

                // Setting up the request level for all child items of an account.
                int childDepth = 0;
                if (requestdepth > 0)
                {
                    childDepth = requestdepth - 1;
                }

                ProductsList.TotalItems = int.Parse(DocNode.Attributes["total"].InnerText.ToString());

                if (DocNode.Attributes["page-size"] != null)
                {
                    // If this is a "TotalOnly" request, there are no other attributes than "Total"
                    ProductsList.PageNumber = int.Parse(DocNode.Attributes["page"].InnerText.ToString());
                    ProductsList.PageSize = int.Parse(DocNode.Attributes["page-size"].InnerText.ToString());
                    ProductsList.ResultsReturned = int.Parse(DocNode.Attributes["results"].InnerText.ToString());
                }

                foreach (XmlNode xn in DocNode.ChildNodes)
                {
                    ProductsList.Products.Add(ProductFillNode(xn, childDepth, MinimalisticLoad, ParaCredentials));
                }
                return ProductsList;
            }

            /// <summary>
            /// This methods accepts a Product node and parse through the different items in it. it can be used to parse a product node, whether the node is returned from a simple read, or as part of a list call.
            /// </summary>
            static internal ParaObjects.Product ProductFillNode(XmlNode Node, int childDepth, bool MinimalisticLoad, ParaCredentials ParaCredentials)
            {

                ParaObjects.Product Product = new ParaObjects.Product();
                bool isSchema = false;

                if (Node.Attributes["id"] != null)
                {
                    Product.productid = long.Parse(Node.Attributes["id"].InnerText.ToString());
                    Product.uniqueIdentifier = Product.productid;
                    isSchema = false;
                }
                else
                {
                    isSchema = true;
                }

                if (Node.Attributes["service-desk-uri"] != null)
                {
                    Product.serviceDeskUri = Node.Attributes["service-desk-uri"].InnerText.ToString();
                }

                foreach (XmlNode child in Node.ChildNodes)
                {
                    //Date_Created
                    if (isSchema == false)
                    {
                        if (child.LocalName.ToLower() == "date_created")
                        {
                            Product.Date_Created = NodeGetInnerText(child);
                        }
                        if (child.LocalName.ToLower() == "date_updated")
                        {
                            Product.Date_Updated = NodeGetInnerText(child);
                        }

                        if (child.LocalName.ToLower() == "folder")
                        {
                            Product.Folder.FolderID = int.Parse(child.ChildNodes[0].Attributes["id"].Value);
                            Product.Folder.Name = child.ChildNodes[0].ChildNodes[0].InnerText.ToString();
                            Product.Folder.FullyLoaded = false;
                        }


                        if (child.LocalName.ToLower() == "visible")
                        {
                            Product.Visible = bool.Parse(NodeGetInnerText(child));
                        }
                        if (child.LocalName.ToLower() == "instock")
                        {
                            Product.Instock = bool.Parse(NodeGetInnerText(child));
                        }


                        if (child.LocalName.ToLower() == "sku")
                        {
                            Product.Sku = NodeGetInnerText(child);
                        }

                        if (child.LocalName.ToLower() == "price")
                        {
                            Product.Price = NodeGetInnerText(child);
                        }

                        if (child.LocalName.ToLower() == "shortdesc")
                        {
                            Product.Shortdesc = NodeGetInnerText(child);
                        }

                        if (child.LocalName.ToLower() == "name")
                        {
                            Product.Name = ParaHelper.HelperMethods.SafeHtmlDecode(NodeGetInnerText(child));
                        }
                        if (child.LocalName.ToLower() == "longdesc")
                        {
                            Product.Longdesc = NodeGetInnerText(child);
                        }


                    }
                    if (child.LocalName.ToLower() == "custom_field")
                    {
                        Product.CustomFields.Add(CommonParser.FillCustomField(MinimalisticLoad, child));
                    }

                }
                return Product;
            }
            internal partial class ProductFolderParser
            {
                /// <summary>
                /// This method requires a ProductFolder xml file and returns a ProductFolder object. It should only by used for a retrieve operation.
                /// </summary>
                static internal ParaObjects.ProductFolder ProductFolderFill(XmlDocument xmlresp, int requestdepth, ParaCredentials ParaCredentials)
                {
                    ParaObjects.ProductFolder ProductFolder = new ParaObjects.ProductFolder();
                    XmlNode ProductFolderNode = xmlresp.DocumentElement;

                    // Setting up the request level for all child items of an account.
                    int childDepth = 0;
                    if (requestdepth > 0)
                    {
                        childDepth = requestdepth - 1;
                    }
                    ProductFolder = ProductFolderFillNode(ProductFolderNode, childDepth, ParaCredentials);
                    ProductFolder.FullyLoaded = true;
                    return ProductFolder;
                }

                /// <summary>
                /// This method requires a ProductFolder list xml file and returns a ProductFoldersList object. It should only by used for a List operation.
                /// </summary>
                static internal ParaObjects.ProductFoldersList ProductFoldersFillList(XmlDocument xmlresp, int requestdepth, ParaCredentials ParaCredentials)
                {
                    ParaObjects.ProductFoldersList ProductFoldersList = new ParaObjects.ProductFoldersList();
                    XmlNode DocNode = xmlresp.DocumentElement;

                    // Setting up the request level for all child items of a DownloadFolder.
                    int childDepth = 0;
                    if (requestdepth > 0)
                    {
                        childDepth = requestdepth - 1;
                    }

                    ProductFoldersList.TotalItems = int.Parse(DocNode.Attributes["total"].InnerText.ToString());

                    if (DocNode.Attributes["page-size"] != null)
                    {
                        // If this is a "TotalOnly" request, there are no other attributes than "Total"
                        ProductFoldersList.PageNumber = int.Parse(DocNode.Attributes["page"].InnerText.ToString());
                        ProductFoldersList.PageSize = int.Parse(DocNode.Attributes["page-size"].InnerText.ToString());
                        ProductFoldersList.ResultsReturned = int.Parse(DocNode.Attributes["results"].InnerText.ToString());
                    }


                    foreach (XmlNode xn in DocNode.ChildNodes)
                    {
                        ProductFoldersList.ProductFolders.Add(ProductFolderFillNode(xn, childDepth, ParaCredentials));
                    }
                    return ProductFoldersList;
                }

                /// <summary>
                /// This method accepts a ProductFolder node and parses through the different items in it. it can be used to parse a ProductFolder node, whether the node is returned from a simple read, or as part of a list call.
                /// </summary>
                static internal ParaObjects.ProductFolder ProductFolderFillNode(XmlNode ProductFolderNode, int childDepth, ParaCredentials ParaCredentials)
                {

                    ParaObjects.ProductFolder ProductFolder = new ParaObjects.ProductFolder();
                    ProductFolder.FolderID = long.Parse(ProductFolderNode.Attributes["id"].InnerText.ToString());

                    foreach (XmlNode child in ProductFolderNode.ChildNodes)
                    {
                        if (child.LocalName.ToLower() == "is_private")
                        {
                            ProductFolder.Is_Private = bool.Parse(NodeGetInnerText(child));
                        }

                        if (child.LocalName.ToLower() == "date_updated")
                        {
                            ProductFolder.Date_Updated = NodeGetInnerText(child);
                        }

                        if (child.LocalName.ToLower() == "description")
                        {
                            ProductFolder.Description = NodeGetInnerText(child);
                        }
                        if (child.LocalName.ToLower() == "name")
                        {
                            ProductFolder.Name = NodeGetInnerText(child);
                        }
                        if (child.LocalName.ToLower() == "parent_folder")
                        {
                            ParaObjects.ProductFolder pf = new ParaObjects.ProductFolder();

                            pf.FolderID = long.Parse(child.ChildNodes[0].Attributes["id"].Value.ToString());
                            pf.Name = child.ChildNodes[0].ChildNodes[0].InnerText.ToString();

                            if (childDepth > 0)
                            {
                                pf = ApiHandler.Product.ProductFolder.ProductFolderGetDetails(long.Parse(child.ChildNodes[0].Attributes["id"].Value.ToString()), ParaCredentials, (Paraenums.RequestDepth)childDepth);
                            }
                            ProductFolder.Parent_Folder = pf;
                        }
                    }
                    return ProductFolder;
                }
            }

        }

        /// <summary>
        /// This class helps parse raw XML responses returned from the server to hard typed Asset objects that you can use for further processing.
        /// </summary>
        internal partial class AssetParser
        {
            /// <summary>
            /// This methods requires an Asset xml file and returns a product object. It should only by used for a retrieve operation.
            /// </summary>
            static internal ParaObjects.Asset AssetFill(XmlDocument xmlresp, Boolean MinimalisticLoad, int requestdepth, ParaCredentials ParaCredentials)
            {
                ParaObjects.Asset Asset = new ParaObjects.Asset();
                XmlNode AssetNode = xmlresp.DocumentElement;

                // Setting up the request level for all child items of an account.
                int childDepth = 0;
                if (requestdepth > 0)
                {
                    childDepth = requestdepth - 1;
                }
                Asset = AssetFillNode(AssetNode, MinimalisticLoad, childDepth, ParaCredentials);
                Asset.FullyLoaded = true;
                return Asset;
            }

            /// <summary>
            /// This methods requires an Asset list xml file and returns an AssetsList oject. It should only by used for a List operation.
            /// </summary>
            static internal ParaObjects.AssetsList AssetsFillList(XmlDocument xmlresp, Boolean MinimalisticLoad, int requestdepth, ParaCredentials ParaCredentials)
            {
                ParaObjects.AssetsList AssetsList = new ParaObjects.AssetsList();
                XmlNode DocNode = xmlresp.DocumentElement;

                // Setting up the request level for all child items of an account.
                int childDepth = 0;
                if (requestdepth > 0)
                {
                    childDepth = requestdepth - 1;
                }


                AssetsList.TotalItems = int.Parse(DocNode.Attributes["total"].InnerText.ToString());

                if (DocNode.Attributes["page-size"] != null)
                {
                    // If this is a "TotalOnly" request, there are no other attributes than "Total"
                    AssetsList.PageNumber = int.Parse(DocNode.Attributes["page"].InnerText.ToString());
                    AssetsList.PageSize = int.Parse(DocNode.Attributes["page-size"].InnerText.ToString());
                    AssetsList.ResultsReturned = int.Parse(DocNode.Attributes["results"].InnerText.ToString());
                }



                foreach (XmlNode xn in DocNode.ChildNodes)
                {
                    AssetsList.Assets.Add(AssetFillNode(xn, MinimalisticLoad, childDepth, ParaCredentials));
                }
                return AssetsList;
            }

            /// <summary>
            /// This methods accepts an Asset node and parse through the different items in it. it can be used to parse a Asset node, whether the node is returned from a simple read, or as part of a list call.
            /// </summary>
            static internal ParaObjects.Asset AssetFillNode(XmlNode Node, Boolean MinimalisticLoad, int childDepth, ParaCredentials ParaCredentials)
            {

                ParaObjects.Asset Asset = new ParaObjects.Asset();
                bool isSchema = false;
                if (Node.Attributes["id"] != null)
                {
                    Asset.Assetid = long.Parse(Node.Attributes["id"].InnerText.ToString());
                    Asset.uniqueIdentifier = Asset.Assetid;
                    isSchema = false;
                }
                else
                {
                    isSchema = true;
                }

                if (Node.Attributes["service-desk-uri"] != null)
                {
                    Asset.serviceDeskUri = Node.Attributes["service-desk-uri"].InnerText.ToString();
                }

                if (Node.Attributes["uid"] != null)
                {
                    Asset.uid = Node.Attributes["uid"].InnerText.ToString();
                }


                foreach (XmlNode child in Node.ChildNodes)
                {
                    if (isSchema == false)
                    {
                        if (child.LocalName.ToLower() == "account_owner")
                        {
                            Asset.Account_Owner.Accountid = int.Parse(child.ChildNodes[0].Attributes["id"].Value);
                            Asset.Account_Owner.Account_Name = child.ChildNodes[0].ChildNodes[0].InnerText.ToString();

                            if (childDepth > 0)
                            {
                                Asset.Account_Owner = ApiHandler.Account.AccountGetDetails(Asset.Account_Owner.Accountid, ParaCredentials, (Paraenums.RequestDepth)childDepth - 1);
                            }
                            Asset.Account_Owner.FullyLoaded = objectFullyLoaded(childDepth);
                        }

                        if (child.LocalName.ToLower() == "created_by")
                        {
                            Asset.Created_By.CsrID = int.Parse(child.ChildNodes[0].Attributes["id"].Value);
                            Asset.Created_By.Full_Name = child.ChildNodes[0].ChildNodes[0].InnerText.ToString();
                        }
                        if (child.LocalName.ToLower() == "modified_by")
                        {
                            Asset.Modified_By.CsrID = int.Parse(child.ChildNodes[0].Attributes["id"].Value);
                            Asset.Modified_By.Full_Name = child.ChildNodes[0].ChildNodes[0].InnerText.ToString();
                        }
                        if (child.LocalName.ToLower() == "customer_owner")
                        {
                            Asset.Customer_Owner.customerid = int.Parse(child.ChildNodes[0].Attributes["id"].Value);

                            if (childDepth > 0)
                            {
                                Asset.Customer_Owner = ApiHandler.Customer.CustomerGetDetails(Asset.Customer_Owner.customerid, ParaCredentials, (Paraenums.RequestDepth)childDepth - 1);
                            }
                            Asset.Customer_Owner.FullyLoaded = objectFullyLoaded(childDepth);

                            //Not sure about this one
                            Asset.Customer_Owner.First_Name = child.ChildNodes[0].ChildNodes[0].InnerText.ToString();
                        }

                        if (child.LocalName.ToLower() == "product")
                        {
                            Asset.Product.productid = int.Parse(child.ChildNodes[0].Attributes["id"].Value);
                            Asset.Product.Name = child.ChildNodes[0].ChildNodes[0].InnerText.ToString();
                            if (childDepth > 0)
                            {
                                Asset.Product = ApiHandler.Product.ProductGetDetails(Asset.Product.productid, ParaCredentials, (Paraenums.RequestDepth)childDepth - 1);
                            }
                            Asset.Product.FullyLoaded = objectFullyLoaded(childDepth);
                        }
                        if (child.LocalName.ToLower() == "date_created")
                        {
                            Asset.Date_Created = NodeGetInnerText(child);
                        }
                        if (child.LocalName.ToLower() == "date_updated")
                        {
                            Asset.Date_Updated = NodeGetInnerText(child);
                        }
                        if (child.LocalName.ToLower() == "serial_number")
                        {
                            Asset.Serial_Number = NodeGetInnerText(child);
                        }
                        if (child.LocalName.ToLower() == "name")
                        {
                            Asset.Name = ParaHelper.HelperMethods.SafeHtmlDecode(NodeGetInnerText(child));
                        }
                        if (child.LocalName.ToLower() == "status")
                        {
                            Asset.Status.StatusID = int.Parse(child.ChildNodes[0].Attributes["id"].Value);
                            Asset.Status.Name = child.ChildNodes[0].ChildNodes[0].InnerText.ToString();
                        }
                    }

                    if (child.LocalName.ToLower() == "custom_field")
                    {
                        Asset.CustomFields.Add(CommonParser.FillCustomField(MinimalisticLoad, child));
                    }
                }
                return Asset;
            }
        }

        /// <summary>
        /// This class helps parse raw XML responses returned from the server to hard typed Download objects that you can use for further processing.
        /// </summary>
        internal partial class DownloadParser
        {
            /// <summary>
            /// This methods requires a Download xml file and returns a Download object. It should only by used for a retrieve operation.
            /// </summary>
            static internal ParaObjects.Download DownloadFill(XmlDocument xmlresp, int requestdepth, bool includeAllCustomFields, ParaCredentials ParaCredentials)
            {
                ParaObjects.Download Download = new ParaObjects.Download(true);
                XmlNode DownloadNode = xmlresp.DocumentElement;

                // Setting up the request level for all child items of an account.
                int childDepth = 0;
                if (requestdepth > 0)
                {
                    childDepth = requestdepth - 1;
                }
                Download = DownloadFillNode(DownloadNode, childDepth, includeAllCustomFields, ParaCredentials);
                Download.FullyLoaded = true;
                return Download;
            }

            /// <summary>
            /// This methods requires a Download list xml file and returns a DownloadsList oject. It should only by used for a List operation.
            /// </summary>
            static internal ParaObjects.DownloadsList DownloadsFillList(XmlDocument xmlresp, Boolean MinimalisticLoad, int requestdepth, ParaCredentials ParaCredentials)
            {
                ParaObjects.DownloadsList DownloadsList = new ParaObjects.DownloadsList();
                XmlNode DocNode = xmlresp.DocumentElement;

                // Setting up the request level for all child items of a Download.
                int childDepth = 0;
                if (requestdepth > 0)
                {
                    childDepth = requestdepth - 1;
                }


                DownloadsList.TotalItems = int.Parse(DocNode.Attributes["total"].InnerText.ToString());


                if (DocNode.Attributes["page-size"] != null)
                {
                    // If this is a "TotalOnly" request, there are no other attributes than "Total"

                    DownloadsList.PageNumber = int.Parse(DocNode.Attributes["page"].InnerText.ToString());
                    DownloadsList.PageSize = int.Parse(DocNode.Attributes["page-size"].InnerText.ToString());
                    DownloadsList.ResultsReturned = int.Parse(DocNode.Attributes["results"].InnerText.ToString());
                }

                foreach (XmlNode xn in DocNode.ChildNodes)
                {
                    DownloadsList.Downloads.Add(DownloadFillNode(xn, childDepth, MinimalisticLoad, ParaCredentials));
                }
                return DownloadsList;
            }

            /// <summary>
            /// This methods accepts a Download node and parse through the different items in it. it can be used to parse a Download node, whether the node is returned from a simple read, or as part of a list call.
            /// </summary>
            static internal ParaObjects.Download DownloadFillNode(XmlNode DownloadNode, int childDepth, bool MinimalisticLoad, ParaCredentials ParaCredentials)
            {

                ParaObjects.Download Download = new ParaObjects.Download(true);
                bool isSchema = false;
                if (DownloadNode.Attributes["id"] != null)
                {
                    Download.Downloadid = long.Parse(DownloadNode.Attributes["id"].InnerText.ToString());
                    Download.uniqueIdentifier = Download.Downloadid;
                    isSchema = false;
                }
                else
                {
                    isSchema = true;
                }

                if (DownloadNode.Attributes["service-desk-uri"] != null)
                {
                    Download.serviceDeskUri = DownloadNode.Attributes["service-desk-uri"].InnerText.ToString();
                }

                foreach (XmlNode child in DownloadNode.ChildNodes)
                {
                    if (isSchema == false)
                    {
                        if (child.LocalName.ToLower() == "date_created")
                        {
                            Download.Date_Created = NodeGetInnerText(child);
                        }

                        if (child.LocalName.ToLower() == "date_updated")
                        {
                            Download.Date_Updated = NodeGetInnerText(child);
                        }

                        if (child.LocalName.ToLower() == "file_size")
                        {
                            Download.File_Size = long.Parse(NodeGetInnerText(child));
                        }

                        if (child.LocalName.ToLower() == "file_hits")
                        {
                            Download.File_Hits = long.Parse(NodeGetInnerText(child));
                        }

                        if (child.LocalName.ToLower() == "description")
                        {
                            Download.Description = NodeGetInnerText(child);
                        }

                        if (child.LocalName.ToLower() == "eula")
                        {
                            Download.Eula.EulaID = long.Parse(child.ChildNodes[0].Attributes["id"].Value.ToString());
                            Download.Eula.ShortTitle = child.ChildNodes[0].ChildNodes[0].InnerText.ToString();
                        }
                        if (child.LocalName.ToLower() == "external_link")
                        {
                            Download.External_Link = NodeGetInnerText(child);
                        }

                        if (child.LocalName.ToLower() == "folders")
                        {
                            Download.MultipleFolders = true;
                            foreach (XmlNode n in child.ChildNodes)
                            {
                                ParaObjects.DownloadFolder folder = new ParaObjects.DownloadFolder();
                                folder.FolderID = long.Parse(n.Attributes["id"].Value);
                                folder.Name = n.ChildNodes[0].InnerText.ToString();
                                Download.Folders.Add(folder);
                            }
                        }

                        if (child.LocalName.ToLower() == "folder")
                        {
                            Download.MultipleFolders = false;
                            ParaObjects.DownloadFolder folder = new ParaObjects.DownloadFolder();
                            folder.FolderID = long.Parse(child.Attributes["id"].Value);
                            folder.Name = child.InnerText.ToString();
                            Download.Folders.Add(folder);
                        }

                        if (child.LocalName.ToLower() == "guid")
                        {
                            Download.Guid = NodeGetInnerText(child);
                        }
                        if (child.LocalName.ToLower() == "name")
                        {
                            Download.Name = NodeGetInnerText(child);
                        }
                        if (child.LocalName.ToLower() == "published")
                        {
                            Download.Published = bool.Parse(NodeGetInnerText(child));
                        }
                        if (child.LocalName.ToLower() == "title")
                        {
                            Download.Title = NodeGetInnerText(child);
                        }
                        if (child.LocalName.ToLower() == "visible")
                        {
                            Download.Visible = bool.Parse(NodeGetInnerText(child));
                        }
                        if (child.LocalName.ToLower() == "permissions")
                        {
                            foreach (XmlNode n in child.ChildNodes)
                            {
                                ParaObjects.Sla sla = new ParaObjects.Sla();
                                sla.SlaID = long.Parse(n.Attributes["id"].Value);
                                sla.Name = n.ChildNodes[0].InnerText.ToString();
                                Download.Permissions.Add(sla);
                            }
                        }
                        if (child.LocalName.ToLower() == "products")
                        {
                            foreach (XmlNode n in child.ChildNodes)
                            {
                                ParaObjects.Product product = new ParaObjects.Product();
                                product.productid = long.Parse(n.Attributes["id"].Value);
                                product.Name = n.ChildNodes[0].InnerText.ToString();
                                Download.Products.Add(product);
                            }
                        }
                        if (child.LocalName.ToLower() == "ext")
                        {
                            Download.Extension = NodeGetInnerText(child);
                        }
                    }
                    else
                    {
                        if (child.LocalName.ToLower() == "folder")
                        {
                            Download.MultipleFolders = false;
                        }
                    }
                }

                return Download;
            }


            internal partial class DownloadFolderParser
            {
                /// <summary>
                /// This methods requires a DownloadFolder xml file and returns a DownloadFolder object. It should only by used for a retrieve operation.
                /// </summary>
                static internal ParaObjects.DownloadFolder DownloadFolderFill(XmlDocument xmlresp, int requestdepth, ParaCredentials ParaCredentials)
                {
                    ParaObjects.DownloadFolder DownloadFolder = new ParaObjects.DownloadFolder();
                    XmlNode DownloadFolderNode = xmlresp.DocumentElement;

                    // Setting up the request level for all child items of an account.
                    int childDepth = 0;
                    if (requestdepth > 0)
                    {
                        childDepth = requestdepth - 1;
                    }
                    DownloadFolder = DownloadFolderFillNode(DownloadFolderNode, childDepth, ParaCredentials);
                    DownloadFolder.FullyLoaded = true;
                    return DownloadFolder;
                }

                /// <summary>
                /// This method requires a DownloadFolder list xml file and returns a DownloadFoldersList object. It should only by used for a List operation.
                /// </summary>
                static internal ParaObjects.DownloadFoldersList DownloadFoldersFillList(XmlDocument xmlresp, int requestdepth, ParaCredentials ParaCredentials)
                {
                    ParaObjects.DownloadFoldersList DownloadFoldersList = new ParaObjects.DownloadFoldersList();
                    XmlNode DocNode = xmlresp.DocumentElement;

                    // Setting up the request level for all child items of a DownloadFolder.
                    int childDepth = 0;
                    if (requestdepth > 0)
                    {
                        childDepth = requestdepth - 1;
                    }


                    DownloadFoldersList.TotalItems = int.Parse(DocNode.Attributes["total"].InnerText.ToString());


                    if (DocNode.Attributes["page-size"] != null)
                    {
                        // If this is a "TotalOnly" request, there are no other attributes than "Total"

                        DownloadFoldersList.PageNumber = int.Parse(DocNode.Attributes["page"].InnerText.ToString());
                        DownloadFoldersList.PageSize = int.Parse(DocNode.Attributes["page-size"].InnerText.ToString());
                        DownloadFoldersList.ResultsReturned = int.Parse(DocNode.Attributes["results"].InnerText.ToString());
                    }


                    foreach (XmlNode xn in DocNode.ChildNodes)
                    {
                        DownloadFoldersList.DownloadFolders.Add(DownloadFolderFillNode(xn, childDepth, ParaCredentials));
                    }
                    return DownloadFoldersList;
                }

                /// <summary>
                /// This method accepts a DownloadFolder node and parses through the different items in it. it can be used to parse a DownloadFolder node, whether the node is returned from a simple read, or as part of a list call.
                /// </summary>
                static internal ParaObjects.DownloadFolder DownloadFolderFillNode(XmlNode DownloadFolderNode, int childDepth, ParaCredentials ParaCredentials)
                {

                    ParaObjects.DownloadFolder DownloadFolder = new ParaObjects.DownloadFolder();
                    DownloadFolder.FolderID = long.Parse(DownloadFolderNode.Attributes["id"].InnerText.ToString());

                    foreach (XmlNode child in DownloadFolderNode.ChildNodes)
                    {
                        if (child.LocalName.ToLower() == "is_private")
                        {
                            DownloadFolder.Is_Private = bool.Parse(NodeGetInnerText(child));
                        }

                        if (child.LocalName.ToLower() == "date_updated")
                        {
                            DownloadFolder.Date_Updated = NodeGetInnerText(child);
                        }

                        if (child.LocalName.ToLower() == "description")
                        {
                            DownloadFolder.Description = NodeGetInnerText(child);
                        }
                        if (child.LocalName.ToLower() == "name")
                        {
                            DownloadFolder.Name = NodeGetInnerText(child);
                        }
                        if (child.LocalName.ToLower() == "parent_folder")
                        {
                            ParaObjects.DownloadFolder pf = new ParaObjects.DownloadFolder();

                            pf.FolderID = long.Parse(child.ChildNodes[0].Attributes["id"].Value.ToString());
                            pf.Name = child.ChildNodes[0].ChildNodes[0].InnerText.ToString();

                            if (childDepth > 0)
                            {
                                pf = ApiHandler.Download.DownloadFolder.DownloadFolderGetDetails(long.Parse(child.ChildNodes[0].Attributes["id"].Value.ToString()), ParaCredentials, (Paraenums.RequestDepth)childDepth);
                            }
                            DownloadFolder.Parent_Folder = pf;
                        }
                    }
                    return DownloadFolder;
                }
            }

        }

        /// <summary>
        /// This class helps parse raw XML responses returned from the server to hard typed Article objects that you can use for further processing.
        /// </summary>
        internal partial class ArticleParser
        {
            /// <summary>
            /// This methods requires a Article xml file and returns a Article object. It should only by used for a retrieve operation.
            /// </summary>
            static internal ParaObjects.Article ArticleFill(XmlDocument xmlresp, int requestdepth, bool MinimalisticLoad, ParaCredentials ParaCredentials)
            {
                ParaObjects.Article Article = new ParaObjects.Article();
                XmlNode ArticleNode = xmlresp.DocumentElement;

                // Setting up the request level for all child items of an account.
                int childDepth = 0;
                if (requestdepth > 0)
                {
                    childDepth = requestdepth - 1;
                }
                Article = ArticleFillNode(ArticleNode, childDepth, MinimalisticLoad, ParaCredentials);
                Article.FullyLoaded = true;
                return Article;
            }

            /// <summary>
            /// This methods requires a Article list xml file and returns a ArticlesList oject. It should only by used for a List operation.
            /// </summary>
            static internal ParaObjects.ArticlesList ArticlesFillList(XmlDocument xmlresp, Boolean MinimalisticLoad, int requestdepth, ParaCredentials ParaCredentials)
            {
                ParaObjects.ArticlesList ArticlesList = new ParaObjects.ArticlesList();
                XmlNode DocNode = xmlresp.DocumentElement;

                // Setting up the request level for all child items of a Download.
                int childDepth = 0;
                if (requestdepth > 0)
                {
                    childDepth = requestdepth - 1;
                }


                ArticlesList.TotalItems = int.Parse(DocNode.Attributes["total"].InnerText.ToString());

                if (DocNode.Attributes["page-size"] != null)
                {
                    // If this is a "TotalOnly" request, there are no other attributes than "Total"
                    ArticlesList.PageNumber = int.Parse(DocNode.Attributes["page"].InnerText.ToString());
                    ArticlesList.PageSize = int.Parse(DocNode.Attributes["page-size"].InnerText.ToString());
                    ArticlesList.ResultsReturned = int.Parse(DocNode.Attributes["results"].InnerText.ToString());
                }



                foreach (XmlNode xn in DocNode.ChildNodes)
                {
                    ArticlesList.Articles.Add(ArticleFillNode(xn, childDepth, MinimalisticLoad, ParaCredentials));
                }
                return ArticlesList;
            }

            /// <summary>
            /// This methods accepts a Article node and parse through the different items in it. it can be used to parse a Article node, whether the node is returned from a simple read, or as part of a list call.
            /// </summary>
            static internal ParaObjects.Article ArticleFillNode(XmlNode ArticleNode, int childDepth, bool MinimalisticLoad, ParaCredentials ParaCredentials)
            {
                bool isSchema = false;
                ParaObjects.Article Article = new ParaObjects.Article();
                if (ArticleNode.Attributes["id"] != null)
                {
                    Article.Articleid = long.Parse(ArticleNode.Attributes["id"].InnerText.ToString());
                    Article.uniqueIdentifier = Article.Articleid;
                }
                else
                {
                    isSchema = true;
                }

                if (ArticleNode.Attributes["service-desk-uri"] != null)
                {
                    Article.serviceDeskUri = ArticleNode.Attributes["service-desk-uri"].InnerText.ToString();
                }

                foreach (XmlNode child in ArticleNode.ChildNodes)
                {
                    if (isSchema == false)
                    {
                        if (child.LocalName.ToLower() == "permissions")
                        {
                            foreach (XmlNode n in child.ChildNodes)
                            {
                                ParaObjects.Sla sla = new ParaObjects.Sla();
                                sla.SlaID = long.Parse(n.Attributes["id"].Value);
                                sla.Name = n.ChildNodes[0].InnerText.ToString();
                                Article.Permissions.Add(sla);
                            }
                        }
                        if (child.LocalName.ToLower() == "products")
                        {
                            foreach (XmlNode n in child.ChildNodes)
                            {
                                ParaObjects.Product product = new ParaObjects.Product();
                                product.productid = long.Parse(n.Attributes["id"].Value);
                                product.Name = n.ChildNodes[0].InnerText.ToString();
                                Article.Products.Add(product);
                            }
                        }
                        if (child.LocalName.ToLower() == "answer")
                        {
                            Article.Answer = NodeGetInnerText(child);
                        }
                        if (child.LocalName.ToLower() == "created_by")
                        {
                            Article.Created_By.CsrID = int.Parse(child.ChildNodes[0].Attributes["id"].Value.ToString());
                            Article.Created_By.Full_Name = child.ChildNodes[0].ChildNodes[0].InnerText.ToString();
                        }

                        if (child.LocalName.ToLower() == "date_created")
                        {
                            Article.Date_Created = NodeGetInnerText(child);
                        }
                        if (child.LocalName.ToLower() == "date_updated")
                        {
                            Article.Date_Updated = NodeGetInnerText(child);
                        }
                        if (child.LocalName.ToLower() == "expiration_date")
                        {
                            Article.Expiration_Date = NodeGetInnerText(child);
                        }
                        if (child.LocalName.ToLower() == "modified_by")
                        {
                            Article.Modified_By.CsrID = int.Parse(child.ChildNodes[0].Attributes["id"].Value.ToString());
                            Article.Modified_By.Full_Name = child.ChildNodes[0].ChildNodes[0].InnerText.ToString();
                        }
                        if (child.LocalName.ToLower() == "folders")
                        {
                            foreach (XmlNode n in child.ChildNodes)
                            {
                                ParaObjects.Folder folder = new ParaObjects.Folder();
                                folder.FolderID = long.Parse(n.Attributes["id"].Value);
                                folder.Name = n.ChildNodes[0].InnerText.ToString();
                                Article.Folders.Add(folder);
                            }
                        }
                        if (child.LocalName.ToLower() == "published")
                        {
                            Article.Published = bool.Parse(NodeGetInnerText(child));
                        }
                        if (child.LocalName.ToLower() == "question")
                        {
                            Article.Question = NodeGetInnerText(child);
                        }
                        if (child.LocalName.ToLower() == "rating")
                        {
                            Article.Rating = int.Parse(NodeGetInnerText(child));
                        }
                        if (child.LocalName.ToLower() == "times_viewed")
                        {
                            Article.Times_Viewed = int.Parse(NodeGetInnerText(child));
                        }
                    }
                }
                return Article;
            }

            internal partial class ArticleFolderParser
            {
                /// <summary>
                /// This methods requires a DownloadFolder xml file and returns a DownloadFolder object. It should only by used for a retrieve operation.
                /// </summary>
                static internal ParaObjects.ArticleFolder ArticleFolderFill(XmlDocument xmlresp, int requestdepth, ParaCredentials ParaCredentials)
                {
                    ParaObjects.ArticleFolder ArticleFolder = new ParaObjects.ArticleFolder();
                    XmlNode ArticleFolderNode = xmlresp.DocumentElement;

                    // Setting up the request level for all child items of an account.
                    int childDepth = 0;
                    if (requestdepth > 0)
                    {
                        childDepth = requestdepth - 1;
                    }
                    ArticleFolder = ArticleFolderFillNode(ArticleFolderNode, childDepth, ParaCredentials);
                    ArticleFolder.FullyLoaded = true;
                    return ArticleFolder;
                }

                /// <summary>
                /// This method requires a DownloadFolder list xml file and returns a DownloadFoldersList object. It should only by used for a List operation.
                /// </summary>
                static internal ParaObjects.ArticleFoldersList ArticleFoldersFillList(XmlDocument xmlresp, int requestdepth, ParaCredentials ParaCredentials)
                {
                    ParaObjects.ArticleFoldersList ArticleFoldersList = new ParaObjects.ArticleFoldersList();
                    XmlNode DocNode = xmlresp.DocumentElement;

                    // Setting up the request level for all child items of a DownloadFolder.
                    int childDepth = 0;
                    if (requestdepth > 0)
                    {
                        childDepth = requestdepth - 1;
                    }


                    ArticleFoldersList.TotalItems = int.Parse(DocNode.Attributes["total"].InnerText.ToString());

                    if (DocNode.Attributes["page-size"] != null)
                    {
                        // If this is a "TotalOnly" request, there are no other attributes than "Total"
                        ArticleFoldersList.PageNumber = int.Parse(DocNode.Attributes["page"].InnerText.ToString());
                        ArticleFoldersList.PageSize = int.Parse(DocNode.Attributes["page-size"].InnerText.ToString());
                        ArticleFoldersList.ResultsReturned = int.Parse(DocNode.Attributes["results"].InnerText.ToString());
                    }


                    foreach (XmlNode xn in DocNode.ChildNodes)
                    {
                        ArticleFoldersList.ArticleFolders.Add(ArticleFolderFillNode(xn, childDepth, ParaCredentials));
                    }
                    return ArticleFoldersList;
                }

                /// <summary>
                /// This method accepts a DownloadFolder node and parses through the different items in it. it can be used to parse a DownloadFolder node, whether the node is returned from a simple read, or as part of a list call.
                /// </summary>
                static internal ParaObjects.ArticleFolder ArticleFolderFillNode(XmlNode ArticleFolderNode, int childDepth, ParaCredentials ParaCredentials)
                {

                    ParaObjects.ArticleFolder ArticleFolder = new ParaObjects.ArticleFolder();

                    bool isSchema = false;

                    if (ArticleFolderNode.Attributes["id"] != null)
                    {
                        ArticleFolder.FolderID = long.Parse(ArticleFolderNode.Attributes["id"].InnerText.ToString());
                    }
                    else
                    {
                        isSchema = true;
                    }

                    ArticleFolder.FullyLoaded = true;
                    foreach (XmlNode child in ArticleFolderNode.ChildNodes)
                    {
                        if (isSchema == false)
                        {
                            if (child.LocalName.ToLower() == "name")
                            {
                                ArticleFolder.Name = NodeGetInnerText(child);
                            }
                            if (child.LocalName.ToLower() == "description")
                            {
                                ArticleFolder.Description = NodeGetInnerText(child);
                            }
                            if (child.LocalName.ToLower() == "is_private")
                            {
                                ArticleFolder.Is_Private = bool.Parse(NodeGetInnerText(child));
                            }
                            if (child.LocalName.ToLower() == "parent_folder")
                            {
                                ParaObjects.ArticleFolder pf = new ParaObjects.ArticleFolder();

                                pf.FolderID = long.Parse(child.ChildNodes[0].Attributes["id"].Value.ToString());
                                pf.Name = child.ChildNodes[0].ChildNodes[0].InnerText.ToString();

                                if (childDepth > 0)
                                {
                                    pf = ApiHandler.Article.ArticleFolder.ArticleFolderGetDetails(long.Parse(child.ChildNodes[0].Attributes["id"].Value.ToString()), ParaCredentials, (Paraenums.RequestDepth)childDepth);
                                }
                                ArticleFolder.Parent_Folder = pf;
                            }
                        }
                    }
                    return ArticleFolder;
                }
            }
        }

        /// <summary>
        /// This class takes care of parsing the different XMLs returned when sending an attachment.
        /// </summary>
        internal partial class AttachmentParser
        {
            static internal string AttachmentGetUrlToPost(XmlDocument doc)
            {
                if (doc != null && doc.DocumentElement.HasAttribute("href"))
                {
                    return doc.DocumentElement.Attributes["href"].InnerText.ToString();
                }
                else
                {
                    throw new Exception("Could not locate the URL in " + doc == null ? "null document" : doc.OuterXml.ToString());
                }
            }

            static internal ParaObjects.Attachment AttachmentFill(XmlDocument doc)
            {
                ParaObjects.Attachment attachment = new ParaObjects.Attachment();

                if (doc.DocumentElement.ChildNodes[0].LocalName.ToLower() == "passed")
                {
                    attachment.HasException = false;
                }
                else
                {
                    attachment.HasException = true;
                }

                XmlNode file = doc.DocumentElement.ChildNodes[0].ChildNodes[0];
                foreach (XmlNode node in file.ChildNodes)
                {
                    if (node.LocalName.ToLower() == "filename")
                    {
                        attachment.Name = node.InnerText;
                    }
                    else if (node.LocalName.ToLower() == "guid")
                    {
                        attachment.GUID = node.InnerText;
                    }
                    else if (node.LocalName.ToLower() == "error")
                    {
                        attachment.Error = node.InnerText;
                    }
                }
                file = null;
                return attachment;
            }

        }

        /// <summary>
        /// Accepts the string found in the XML and split it by "," and return a collection of string.
        /// Very useful in the scenario of CC email addresses.
        /// </summary>
        static ArrayList ParseStringCollection(string result)
        {
            //string[] list=null;   ArrayList FieldsListA = new ArrayList();
            ArrayList CCList = new ArrayList();
            Array splitted;
            if (string.IsNullOrEmpty(result) == false)
            {
                char[] splitter = { ',' };
                splitted = result.Split(splitter);
                for (int i = 0; i < splitted.Length; i++)
                {
                    CCList.Add(splitted.GetValue(i).ToString());
                }
                CCList.TrimToSize();
                return CCList;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Indicates whether the object is fully loaded or not (if not loaded, means only the id and the name are filled)
        /// </summary>
        static bool objectFullyLoaded(int childnode)
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
        static string NodeGetInnerText(XmlNode Node)
        {
            if (Node.InnerText != null)
            {

                return ParaHelper.HelperMethods.SafeHtmlDecode(Node.InnerXml.ToString());
            }

            else
            {
                return "";
            }


        }

        /// <summary>
        /// Before assigning a node attribute value, this method will check whether the node is null or not. Return True if the Attribute is not null, False if the Attribute is null.
        /// </summary>
        static bool CheckNodeAttributeNotNull(XmlNode Node, string Attribute)
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

        /// <summary>
        /// Includes all common parsing methods used by the other XML parser classes.
        /// </summary>
        partial class CommonParser
        {
            /// <summary>
            /// This methods will fill and return a custom field object. Whenever Parsing an XML and getting a "custom_field" node, just pass that node to this method, and it will return the filled custom field object.
            /// </summary>
            static public ParaObjects.CustomField FillCustomField(bool MinimalisticLoad, XmlNode Node)
            {
                ParaObjects.CustomField cf = new ParaObjects.CustomField();

                cf.CustomFieldName = Node.Attributes["display-name"].Value;
                cf.CustomFieldID = long.Parse(Node.Attributes["id"].Value);

                if (CheckNodeAttributeNotNull(Node, "required") == true)
                {
                    try
                    {
                        cf.CustomFieldRequired = Convert.ToBoolean(Node.Attributes["required"].Value);
                    }
                    catch (Exception exx)
                    {
                        cf.CustomFieldRequired = false;
                    }
                }
                else
                {
                    cf.CustomFieldRequired = false;
                }

                cf.MaxLength = 0;
                if (CheckNodeAttributeNotNull(Node, "max-length") == true)
                {
                    if (String.IsNullOrEmpty(Node.Attributes["max-length"].Value) == false && Information.IsNumeric(Node.Attributes["max-length"].Value))
                    {
                        cf.MaxLength = Int32.Parse(Node.Attributes["max-length"].Value);
                    }
                }


                if (CheckNodeAttributeNotNull(Node, "editable") == true)
                {
                    try
                    {
                        cf.Editable = Convert.ToBoolean(Node.Attributes["editable"].Value);
                    }
                    catch (Exception exx)
                    {
                        cf.Editable = false;
                    }
                }
                else
                {
                    cf.Editable = false;

                }
                if (CheckNodeAttributeNotNull(Node, "data-type") == true)
                {
                    try
                    {
                        cf.DataType = ParaHelper.ParaEnumProvider.CustomFieldDataTypeProvider(Node.Attributes["data-type"].Value);
                    }
                    catch (Exception exx)
                    {
                        cf.DataType = Paraenums.CustomFieldDataType.Unknown;
                    }

                }
                else
                {
                    cf.DataType = Paraenums.CustomFieldDataType.Unknown;

                }
                if (CheckNodeAttributeNotNull(Node, "dependent") == true)
                {
                    cf.dependent = Convert.ToBoolean(Node.Attributes["dependent"].Value);

                }
                if (CheckNodeAttributeNotNull(Node, "multi-value") == true)
                {
                    cf.MultiValue = Convert.ToBoolean(Node.Attributes["multi-value"].Value);

                }
                else
                {
                    cf.MultiValue = false;

                }

                if (Node.ChildNodes.Count > 0)
                {
                    // Only if there are children nodes, which implies a multivalue fields
                    // With optionally field dependencies.

                    //Since even when this is a regular customer, the inner xml will be detected
                    //as a child node, the ismultivalue flag will let us know if it is really a
                    //multivalue field.
                    bool ismultivalue = false;

                    foreach (XmlNode OptionNode in Node.ChildNodes)
                    {
                        if (OptionNode.LocalName.ToLower() == "option")
                        {
                            ismultivalue = true;
                            ParaObjects.CustomFieldOptions cfo = new ParaObjects.CustomFieldOptions();
                            cfo.CustomFieldOptionID = long.Parse(OptionNode.Attributes["id"].Value);
                            if (CheckNodeAttributeNotNull(OptionNode, "dependent") == true)
                            {
                                cfo.dependent = Convert.ToBoolean(OptionNode.Attributes["dependent"].Value);
                            }
                            if (CheckNodeAttributeNotNull(OptionNode, "selected") == true)
                            {
                                cfo.IsSelected = Convert.ToBoolean(OptionNode.Attributes["selected"].Value);

                            }
                            else
                            {
                                cfo.IsSelected = false;
                            }


                            foreach (XmlNode child in OptionNode.ChildNodes)
                            {
                                if (child.LocalName.ToLower() == "value")
                                {
                                    cfo.CustomFieldOptionName = child.InnerText.ToString();
                                }

                                if (child.LocalName.ToLower() == "enables" && MinimalisticLoad==false)
                                {
                                    //// TO DO here: Add logic for custom fields option dependencies
                                    ParaObjects.DependantCustomFields cfod = new ParaObjects.DependantCustomFields();
                                    if (child.FirstChild != null)
                                    {
                                        string customField = child.FirstChild.InnerText.Substring(0, child.FirstChild.InnerText.IndexOf("]") + 1);
                                        string tmp = "";
                                        foreach (char c in customField.ToCharArray())
                                        {
                                            if (char.IsNumber(c))
                                            {
                                                tmp += c.ToString();
                                            }
                                        }
                                        if (child.FirstChild.InnerText.Contains("/Option"))
                                        {
                                            string[] options = child.FirstChild.InnerText.Substring(child.FirstChild.InnerText.IndexOf("/Option")).Split(new String[] { "or" }, StringSplitOptions.RemoveEmptyEntries);
                                            long[] ops = new long[options.Length];
                                            for (int i = 0; i < options.Length; i++)
                                            {
                                                string temp = "";
                                                foreach (char c in options[i])
                                                {
                                                    if (char.IsNumber(c))
                                                    {
                                                        temp += c.ToString();
                                                    }
                                                }
                                                ops[i] = long.Parse(temp);
                                            }
                                            cfod.DependantFieldOptions = ops;
                                        }
                                        if (string.IsNullOrEmpty(tmp) == true)
                                        {
                                            cfod = null;
                                        }
                                        else
                                        {

                                            cfod.DependantFieldID = long.Parse(tmp);
                                            cfod.DependantFieldPath = child.FirstChild.InnerText;
                                        }
                                    }

                                    //// Do the parsing of the dependent custom field, and the options, above
                                    //// Then uncomment the next line.
                                    if (cfod != null)
                                    {
                                        cfo.DependantCustomFields.Add(cfod);
                                    }
                                }
                            }
                            if (cfo.IsSelected == true || MinimalisticLoad == false)
                            {
                                cf.CustomFieldOptionsCollection.Add(cfo);
                            }

                        }

                        if (ismultivalue == false)
                        {
                            //This is not a dropdown, we will look at the inner XML of the node.
                            cf.CustomFieldValue = ParaHelper.HelperMethods.SafeHtmlDecode(NodeGetInnerText(Node));
                            if (Node.Attributes["data-type"].Value.ToString().ToLower() == "date")
                            {
                                DateTime result;
                                cf.CustomFieldValue = cf.CustomFieldValue.Replace("z", "");

                                if (string.IsNullOrEmpty(cf.CustomFieldValue) == false && DateTime.TryParse(cf.CustomFieldValue, out result) == true)
                                {
                                    cf.CustomFieldValue = result.ToString();
                                }
                            }
                        }

                    }
                }

                return cf;

            }

            /// <summary>
            /// Parses an action history node and returns a list of actionhistory objects.
            /// </summary>
            static public List<ParaObjects.ActionHistory> FillActionHistory(XmlNode Node)
            {

                List<ParaObjects.ActionHistory> ahlist = new List<ParaObjects.ActionHistory>();

                foreach (XmlNode Historychild in Node.ChildNodes)
                {
                    ParaObjects.ActionHistory ah = new ParaObjects.ActionHistory();

                    ah.ActionHistoryID = long.Parse(Historychild.Attributes["id"].InnerText.ToString());

                    foreach (XmlNode child in Historychild.ChildNodes)
                    {
                        if (child.LocalName.ToLower() == "history_attachments")
                        {
                            ah.History_Attachments = CommonParser.FillAttachments(child);
                        }

                        if (child.LocalName.ToLower() == "action")
                        {
                            ah.Action.ActionID = int.Parse(child.Attributes["id"].Value);
                            ah.Action.ActionName = child.Attributes["name"].Value;
                        }
                        if (child.LocalName.ToLower() == "old_status")
                        {
                            ah.Old_Status.StatusID = int.Parse(child.ChildNodes[0].Attributes["id"].Value);
                            ah.Old_Status.Name = child.ChildNodes[0].ChildNodes[0].InnerText.ToString();
                        }
                        if (child.LocalName.ToLower() == "new_status")
                        {
                            ah.New_Status.StatusID = int.Parse(child.ChildNodes[0].Attributes["id"].Value);
                            ah.New_Status.Name = child.ChildNodes[0].ChildNodes[0].InnerText.ToString();
                        }
                        if (child.LocalName.ToLower() == "comments")
                        {
                            ah.Comments = child.InnerText.ToString();
                        }
                        if (child.LocalName.ToLower() == "show_to_customer")
                        {
                            if (!bool.TryParse(child.InnerText, out ah.Show_To_Customer))
                            {
                                //uh oh
                            }
                            //if (child.InnerText.ToString() == "0")
                            //{

                            //    ah.Show_To_Customer = false;
                            //}

                            //else
                            //{
                            //    ah.Show_To_Customer = true;

                            //}
                        }
                        if (child.LocalName.ToLower() == "time_spent")
                        {
                            ah.Time_Spent = int.Parse(child.InnerText.ToString());
                        }
                        if (child.LocalName.ToLower() == "action_date")
                        {
                            ah.Action_Date = DateTime.Parse(child.InnerText.ToString());
                        }
                        if (child.LocalName.ToLower() == "action_performer")
                        {
                            string performer;

                            if (child.HasChildNodes)
                            {
                                performer = child.ChildNodes[0].LocalName.ToLower();
                            }
                            else
                            {
                                performer = child.Attributes["performer-type"].Value.ToLower();
                            }
                            if (performer == "csr")
                            {
                                ah.Action_Performer.ActionHistoryPerformerType = Paraenums.ActionHistoryPerformerType.Csr;
                                ah.Action_Performer.CsrPerformer.CsrID = Int64.Parse(child.ChildNodes[0].Attributes["id"].Value);
                                ah.Action_Performer.CsrPerformer.Full_Name = child.ChildNodes[0].ChildNodes[0].InnerText.ToString();
                            }
                            else if (performer == "customer")
                            {
                                ah.Action_Performer.ActionHistoryPerformerType = Paraenums.ActionHistoryPerformerType.Customer;
                                ah.Action_Performer.CustomerPerformer.customerid = Int64.Parse(child.ChildNodes[0].Attributes["id"].Value);
                                ah.Action_Performer.CustomerPerformer.First_Name = child.ChildNodes[0].ChildNodes[0].InnerText.ToString();
                            }
                            else
                            {
                                ah.Action_Performer.ActionHistoryPerformerType = Paraenums.ActionHistoryPerformerType.System;
                            }
                        }
                    }

                    ahlist.Add(ah);

                }

                return ahlist;

            }


            /// <summary>
            /// Parses an Attachments node and returns a list of Attachments objects.
            /// </summary>
            static public List<ParaObjects.Attachment> FillAttachments(XmlNode Node)
            {
                List<ParaObjects.Attachment> atlist = new List<ParaObjects.Attachment>();
                foreach (XmlNode attachNode in Node.ChildNodes)
                {
                    ParaObjects.Attachment at = new ParaObjects.Attachment();

                    at = FillAttachment(attachNode);
                    atlist.Add(at);
                }
                return atlist;
            }


            static public ParaObjects.Attachment FillAttachment(XmlNode Node)
            {

                ParaObjects.Attachment at = new ParaObjects.Attachment();

                at.AttachmentURL = Node.Attributes["href"].Value;
                at.GUID = Node.ChildNodes[0].InnerText.ToString();
                at.Name = Node.ChildNodes[1].InnerText.ToString();

                return at;

            }
        }

        /// <summary>
        /// Handles all XML parsing logic needed for the CSR object
        /// </summary>
        internal partial class CsrParser
        {
            /// <summary>
            /// This methods requires a CSR xml file and returns a CSR object. It should only by used for a retrieve operation.
            /// </summary>
            static internal ParaObjects.Csr CsrFill(XmlDocument xmlresp)
            {
                ParaObjects.Csr Csr = new ParaObjects.Csr();
                XmlNode CsrNode = xmlresp.DocumentElement;
                Csr = CsrFillNode(CsrNode);
                return Csr;
            }

            /// <summary>
            /// This method requires a Csr list xml file and returns a Csr object. It should only by used for a List operation.
            /// </summary>
            static internal ParaObjects.CsrsList CsrsFillList(XmlDocument xmlresp)
            {
                ParaObjects.CsrsList CsrsList = new ParaObjects.CsrsList();
                XmlNode DocNode = xmlresp.DocumentElement;


                CsrsList.TotalItems = int.Parse(DocNode.Attributes["total"].InnerText.ToString());


                if (DocNode.Attributes["page-size"] != null)
                {
                    // If this is a "TotalOnly" request, there are no other attributes than "Total"
                    CsrsList.PageNumber = int.Parse(DocNode.Attributes["page"].InnerText.ToString());
                    CsrsList.PageSize = int.Parse(DocNode.Attributes["page-size"].InnerText.ToString());
                    CsrsList.ResultsReturned = int.Parse(DocNode.Attributes["results"].InnerText.ToString());
                }




                foreach (XmlNode xn in DocNode.ChildNodes)
                {
                    CsrsList.Csrs.Add(CsrFillNode(xn));
                }
                return CsrsList;
            }

            /// <summary>
            /// This method accepts a Csr node and parses through the different items in it. it can be used to parse a Csr node, whether the node is returned from a simple read, or as part of a list call.
            /// </summary>
            static internal ParaObjects.Csr CsrFillNode(XmlNode CsrNode)
            {

                ParaObjects.Csr Csr = new ParaObjects.Csr();

                bool isSchema = false;

                if (CsrNode.Attributes["id"] != null)
                {
                    Csr.CsrID = long.Parse(CsrNode.Attributes["id"].InnerText.ToString());
                    Csr.uniqueIdentifier = Csr.CsrID;
                }
                else
                {
                    isSchema = true;
                }

                if (CsrNode.Attributes["service-desk-uri"] != null)
                {
                    Csr.serviceDeskUri = CsrNode.Attributes["service-desk-uri"].InnerText.ToString();
                }

                foreach (XmlNode child in CsrNode.ChildNodes)
                {
                    if (isSchema == false)
                    {
                        if (child.LocalName.ToLower() == "email")
                        {
                            Csr.Email = NodeGetInnerText(child);
                        }
                        else if (child.LocalName.ToLower() == "fax")
                        {
                            Csr.Fax = NodeGetInnerText(child);
                        }
                        else if (child.LocalName.ToLower() == "full_name")
                        {
                            Csr.Full_Name = NodeGetInnerText(child);
                        }
                        else if (child.LocalName.ToLower() == "phone_1")
                        {
                            Csr.Phone_1 = NodeGetInnerText(child);
                        }
                        else if (child.LocalName.ToLower() == "phone_2")
                        {
                            Csr.Phone_2 = NodeGetInnerText(child);
                        }
                        else if (child.LocalName.ToLower() == "screen_name")
                        {
                            Csr.Screen_Name = NodeGetInnerText(child);
                        }
                        else if (child.LocalName.ToLower() == "date_created")
                        {
                            Csr.Date_Created = DateTime.Parse(NodeGetInnerText(child));
                        }
                        else if (child.LocalName.ToLower() == "date_format")
                        {
                            Csr.Date_Format = NodeGetInnerText(child);
                        }
                        else if (child.LocalName.ToLower() == "role")
                        {
                            for (int i = 0; i < child.ChildNodes.Count; i++)
                            {
                                if (child.ChildNodes[i] != null && child.ChildNodes[i].Attributes["id"] != null)
                                {
                                    Csr.Role.Add(new ParaObjects.Role(
                                        long.Parse(child.ChildNodes[i].Attributes["id"].Value.ToString()),
                                        child.ChildNodes[i].ChildNodes[0].InnerText.ToString(),
                                        ""));
                                }
                            }
                        }
                        else if (child.LocalName.ToLower() == "status")
                        {
                            if (child.ChildNodes[0] != null && child.ChildNodes[0].Attributes["id"] != null)
                            {
                                Csr.Status.StatusID = int.Parse(child.ChildNodes[0].Attributes["id"].Value.ToString());
                                Csr.Status.Name = child.ChildNodes[0].ChildNodes[0].InnerText.ToString();
                            }
                        }
                        else if (child.LocalName.ToLower() == "timezone")
                        {
                            if (child.ChildNodes[0] != null && child.ChildNodes[0].Attributes["id"] != null)
                            {
                                Csr.Timezone.TimezoneID = long.Parse(child.ChildNodes[0].Attributes["id"].Value.ToString());
                                Csr.Timezone.Name = child.ChildNodes[0].ChildNodes[0].InnerText.ToString();
                            }
                        }
                        else
                        {
                            Console.Read();
                        }
                    }
                }
                return Csr;
            }
        }

        /// <summary>
        /// Handles all XML parsing logic needed for the Department object
        /// </summary>
        internal partial class DepartmentParser
        {
            /// <summary>
            /// This methods requires a Department xml file and returns a Department object. It should only by used for a retrieve operation.
            /// </summary>
            static internal ParaObjects.Department DepartmentFill(XmlDocument xmlresp)
            {
                ParaObjects.Department department = new ParaObjects.Department();
                XmlNode departmentNode = xmlresp.DocumentElement;
                department = DepartmentFillNode(departmentNode);
                return department;
            }

            /// <summary>
            /// This method requires a Department list xml file and returns a Department object. It should only by used for a List operation.
            /// </summary>
            static internal ParaObjects.DepartmentsList DepartmentsFillList(XmlDocument xmlresp)
            {
                ParaObjects.DepartmentsList departmentsList = new ParaObjects.DepartmentsList();
                XmlNode DocNode = xmlresp.DocumentElement;

                departmentsList.TotalItems = int.Parse(DocNode.Attributes["total"].InnerText.ToString());

                if (DocNode.Attributes["page-size"] != null)
                {
                    // If this is a "TotalOnly" request, there are no other attributes than "Total"
                    departmentsList.PageNumber = int.Parse(DocNode.Attributes["page"].InnerText.ToString());
                    departmentsList.PageSize = int.Parse(DocNode.Attributes["page-size"].InnerText.ToString());
                    departmentsList.ResultsReturned = int.Parse(DocNode.Attributes["results"].InnerText.ToString());
                }

                foreach (XmlNode xn in DocNode.ChildNodes)
                {
                    departmentsList.Departments.Add(DepartmentFillNode(xn));
                }
                return departmentsList;
            }

            /// <summary>
            /// This method accepts a department node and parses through the different items in it. it can be used to parse a department node, whether the node is returned from a simple read, or as part of a list call.
            /// </summary>
            static internal ParaObjects.Department DepartmentFillNode(XmlNode DepartmentNode)
            {

                ParaObjects.Department department = new ParaObjects.Department();
                department.DepartmentID = long.Parse(DepartmentNode.Attributes["id"].InnerText.ToString());

                foreach (XmlNode child in DepartmentNode.ChildNodes)
                {
                    if (child.LocalName.ToLower() == "name")
                    {
                        department.Name = NodeGetInnerText(child);
                    }
                    if (child.LocalName.ToLower() == "description")
                    {
                        department.Description = NodeGetInnerText(child);
                    }

                }
                return department;
            }
        }

        internal partial class TicketStatusParser
        {
            /// <summary>
            /// This methods requires a Ticket Status xml file and returns a Ticket Status object. It should only by used for a retrieve operation.
            /// </summary>
            static internal ParaObjects.TicketStatus TicketStatusFill(XmlDocument xmlresp)
            {
                ParaObjects.TicketStatus ticketStatus = new ParaObjects.TicketStatus();
                XmlNode ticketStatusNode = xmlresp.DocumentElement;
                ticketStatus = TicketStatusFillNode(ticketStatusNode);
                return ticketStatus;
            }

            /// <summary>
            /// This method requires a Ticket Status list xml file and returns a Ticket Status object. It should only by used for a List operation.
            /// </summary>
            static internal ParaObjects.TicketStatusList TicketStatusFillList(XmlDocument xmlresp)
            {
                ParaObjects.TicketStatusList ticketStatusList = new ParaObjects.TicketStatusList();
                XmlNode DocNode = xmlresp.DocumentElement;


                ticketStatusList.TotalItems = int.Parse(DocNode.Attributes["total"].InnerText.ToString());

                if (DocNode.Attributes["page-size"] != null)
                {
                    // If this is a "TotalOnly" request, there are no other attributes than "Total"
                    ticketStatusList.PageNumber = int.Parse(DocNode.Attributes["page"].InnerText.ToString());
                    ticketStatusList.PageSize = int.Parse(DocNode.Attributes["page-size"].InnerText.ToString());
                    ticketStatusList.ResultsReturned = int.Parse(DocNode.Attributes["results"].InnerText.ToString());
                }

                foreach (XmlNode xn in DocNode.ChildNodes)
                {
                    ticketStatusList.TicketStatuses.Add(TicketStatusFillNode(xn));
                }
                return ticketStatusList;
            }

            /// <summary>
            /// This method accepts a Csr node and parses through the different items in it. it can be used to parse a Csr node, whether the node is returned from a simple read, or as part of a list call.
            /// </summary>
            static internal ParaObjects.TicketStatus TicketStatusFillNode(XmlNode TicketStatusNode)
            {

                ParaObjects.TicketStatus TicketStatus = new ParaObjects.TicketStatus();
                TicketStatus.StatusID = long.Parse(TicketStatusNode.Attributes["id"].InnerText.ToString());
                TicketStatus.StatusType = (Paraenums.TicketStatusType)Enum.Parse(typeof(Paraenums.TicketStatusType), TicketStatusNode.Attributes["status-type"].InnerText.ToString());

                foreach (XmlNode child in TicketStatusNode.ChildNodes)
                {
                    if (child.LocalName.ToLower() == "customer_text")
                    {
                        TicketStatus.Customer_Text = NodeGetInnerText(child);
                    }
                    if (child.LocalName.ToLower() == "name")
                    {
                        TicketStatus.Name = NodeGetInnerText(child);
                    }
                }
                return TicketStatus;
            }
        }

        internal partial class QueueParser
        {
            /// <summary>
            /// This methods requires a Queue xml file and returns a Queue object. It should only by used for a retrieve operation.
            /// </summary>
            static internal ParaObjects.Queue QueueFill(XmlDocument xmlresp)
            {
                ParaObjects.Queue queue = new ParaObjects.Queue();
                XmlNode queueNode = xmlresp.DocumentElement;
                queue = QueueFillNode(queueNode);
                return queue;
            }

            /// <summary>
            /// This method requires a Queue list xml file and returns a Queue object. It should only by used for a List operation.
            /// </summary>
            static internal ParaObjects.QueueList QueueFillList(XmlDocument xmlresp)
            {
                ParaObjects.QueueList queueList = new ParaObjects.QueueList();
                XmlNode DocNode = xmlresp.DocumentElement;


                queueList.TotalItems = int.Parse(DocNode.Attributes["total"].InnerText.ToString());

                if (DocNode.Attributes["page-size"] != null)
                {
                    // If this is a "TotalOnly" request, there are no other attributes than "Total"
                    queueList.PageNumber = int.Parse(DocNode.Attributes["page"].InnerText.ToString());
                    queueList.PageSize = int.Parse(DocNode.Attributes["page-size"].InnerText.ToString());
                    queueList.ResultsReturned = int.Parse(DocNode.Attributes["results"].InnerText.ToString());
                }


                foreach (XmlNode xn in DocNode.ChildNodes)
                {
                    queueList.Queues.Add(QueueFillNode(xn));
                }
                return queueList;
            }

            /// <summary>
            /// This method accepts a Queue node and parses through the different items in it. it can be used to parse a Csr node, whether the node is returned from a simple read, or as part of a list call.
            /// </summary>
            static internal ParaObjects.Queue QueueFillNode(XmlNode QueueNode)
            {
                ParaObjects.Queue Queue = new ParaObjects.Queue();
                Queue.QueueID = Int32.Parse(QueueNode.Attributes["id"].InnerText.ToString());

                foreach (XmlNode child in QueueNode.ChildNodes)
                {
                    if (child.LocalName.ToLower() == "name")
                    {
                        Queue.Name = NodeGetInnerText(child);
                    }
                }
                return Queue;
            }
        }
        internal partial class AccountViewParser
        {

            /// <summary>
            /// This method requires a View list xml file and returns a ViewList object. It should only by used for a List operation.
            /// </summary>
            static internal ParaObjects.AccountViewList ViewFillList(XmlDocument xmlresp)
            {
                ParaObjects.AccountViewList viewList = new ParaObjects.AccountViewList();
                XmlNode DocNode = xmlresp.DocumentElement;


                viewList.TotalItems = int.Parse(DocNode.Attributes["total"].InnerText.ToString());

                if (DocNode.Attributes["page-size"] != null)
                {
                    // If this is a "TotalOnly" request, there are no other attributes than "Total"
                    viewList.PageNumber = int.Parse(DocNode.Attributes["page"].InnerText.ToString());
                    viewList.PageSize = int.Parse(DocNode.Attributes["page-size"].InnerText.ToString());
                    viewList.ResultsReturned = int.Parse(DocNode.Attributes["results"].InnerText.ToString());
                }

                foreach (XmlNode xn in DocNode.ChildNodes)
                {
                    viewList.views.Add(ViewFillNode(xn));
                }
                return viewList;
            }

            /// <summary>
            /// This method accepts a Queue node and parses through the different items in it. it can be used to parse a Csr node, whether the node is returned from a simple read, or as part of a list call.
            /// </summary>
            static internal ParaObjects.AccountView ViewFillNode(XmlNode QueueNode)
            {
                ParaObjects.AccountView view = new ParaObjects.AccountView();
                view.ID = Int32.Parse(QueueNode.Attributes["id"].InnerText.ToString());

                foreach (XmlNode child in QueueNode.ChildNodes)
                {
                    if (child.LocalName.ToLower() == "name")
                    {
                        view.Name = NodeGetInnerText(child);
                    }
                }
                return view;
            }
        }



        internal partial class CustomerViewParser
        {

            /// <summary>
            /// This method requires a View list xml file and returns a ViewList object. It should only by used for a List operation.
            /// </summary>
            static internal ParaObjects.ContactViewList ViewFillList(XmlDocument xmlresp)
            {
                ParaObjects.ContactViewList viewList = new ParaObjects.ContactViewList();
                XmlNode DocNode = xmlresp.DocumentElement;


                viewList.TotalItems = int.Parse(DocNode.Attributes["total"].InnerText.ToString());

                if (DocNode.Attributes["page-size"] != null)
                {
                    // If this is a "TotalOnly" request, there are no other attributes than "Total"
                    viewList.PageNumber = int.Parse(DocNode.Attributes["page"].InnerText.ToString());
                    viewList.PageSize = int.Parse(DocNode.Attributes["page-size"].InnerText.ToString());
                    viewList.ResultsReturned = int.Parse(DocNode.Attributes["results"].InnerText.ToString());
                }

                foreach (XmlNode xn in DocNode.ChildNodes)
                {
                    viewList.views.Add(ViewFillNode(xn));
                }
                return viewList;
            }

            /// <summary>
            /// This method accepts a Queue node and parses through the different items in it. it can be used to parse a Csr node, whether the node is returned from a simple read, or as part of a list call.
            /// </summary>
            static internal ParaObjects.ContactView ViewFillNode(XmlNode QueueNode)
            {
                ParaObjects.ContactView view = new ParaObjects.ContactView();
                view.ID = Int32.Parse(QueueNode.Attributes["id"].InnerText.ToString());

                foreach (XmlNode child in QueueNode.ChildNodes)
                {
                    if (child.LocalName.ToLower() == "name")
                    {
                        view.Name = NodeGetInnerText(child);
                    }
                }
                return view;
            }
        }

        internal partial class TicketViewParser
        {
            /// <summary>
            /// This method requires a View list xml file and returns a ViewList object. It should only by used for a List operation.
            /// </summary>
            static internal ParaObjects.TicketViewList ViewFillList(XmlDocument xmlresp)
            {
                ParaObjects.TicketViewList viewList = new ParaObjects.TicketViewList();
                XmlNode DocNode = xmlresp.DocumentElement;


                viewList.TotalItems = int.Parse(DocNode.Attributes["total"].InnerText.ToString());

                if (DocNode.Attributes["page-size"] != null)
                {
                    // If this is a "TotalOnly" request, there are no other attributes than "Total"
                    viewList.PageNumber = int.Parse(DocNode.Attributes["page"].InnerText.ToString());
                    viewList.PageSize = int.Parse(DocNode.Attributes["page-size"].InnerText.ToString());
                    viewList.ResultsReturned = int.Parse(DocNode.Attributes["results"].InnerText.ToString());
                }

                foreach (XmlNode xn in DocNode.ChildNodes)
                {
                    viewList.views.Add(ViewFillNode(xn));
                }
                return viewList;
            }

            /// <summary>
            /// This method accepts a Queue node and parses through the different items in it. it can be used to parse a Csr node, whether the node is returned from a simple read, or as part of a list call.
            /// </summary>
            static internal ParaObjects.TicketView ViewFillNode(XmlNode QueueNode)
            {
                ParaObjects.TicketView view = new ParaObjects.TicketView();
                view.ID = Int32.Parse(QueueNode.Attributes["id"].InnerText.ToString());

                foreach (XmlNode child in QueueNode.ChildNodes)
                {
                    if (child.LocalName.ToLower() == "name")
                    {
                        view.Name = NodeGetInnerText(child);
                    }
                }
                return view;
            }
        }
        /// <summary>
        /// Handles all XML parsing logic needed for the Role object
        /// </summary>
        internal partial class CustomerStatusParser
        {
            /// <summary>
            /// This methods requires a CustomerStatus xml file and returns a CustomerStatus object. It should only by used for a retrieve operation.
            /// </summary>
            /// <param name="xmlresp"></param>
            /// <returns></returns>
            static internal ParaObjects.CustomerStatus CustomerStatusFill(XmlDocument xmlresp)
            {
                ParaObjects.CustomerStatus CustomerStatus = new ParaObjects.CustomerStatus();
                XmlNode CustomerStatusNode = xmlresp.DocumentElement;
                CustomerStatus = CustomerStatusFillNode(CustomerStatusNode);
                return CustomerStatus;
            }
            /// <summary>
            /// This method requires a CustomerStatus list xml file and returns a CustomerStatus list object. It should only by used for a List operation.
            /// </summary>
            /// <param name="xmlresp"></param>
            /// <returns></returns>
            static internal ParaObjects.CustomerStatusList CustomerStatusFillList(XmlDocument xmlresp)
            {
                ParaObjects.CustomerStatusList CustomerStatusList = new ParaObjects.CustomerStatusList();
                XmlNode DocNode = xmlresp.DocumentElement;

                CustomerStatusList.TotalItems = int.Parse(DocNode.Attributes["total"].InnerText.ToString());

                if (DocNode.Attributes["page-size"] != null)
                {
                    // If this is a "TotalOnly" request, there are no other attributes than "Total"
                    CustomerStatusList.PageNumber = int.Parse(DocNode.Attributes["page"].InnerText.ToString());
                    CustomerStatusList.PageSize = int.Parse(DocNode.Attributes["page-size"].InnerText.ToString());
                    CustomerStatusList.ResultsReturned = int.Parse(DocNode.Attributes["results"].InnerText.ToString());
                }

                foreach (XmlNode xn in DocNode.ChildNodes)
                {
                    CustomerStatusList.CustomerStatuses.Add(CustomerStatusFillNode(xn));
                }
                return CustomerStatusList;
            }

            /// <summary>
            /// This method accepts a CustomerStatus node and parses through the different items in it. it can be used to parse a CustomerStatus node, whether the node is returned from a simple read, or as part of a list call.
            /// </summary>
            static internal ParaObjects.CustomerStatus CustomerStatusFillNode(XmlNode CustomerStatusNode)
            {

                ParaObjects.CustomerStatus CustomerStatus = new ParaObjects.CustomerStatus();
                CustomerStatus.StatusID = long.Parse(CustomerStatusNode.Attributes["id"].InnerText.ToString());

                foreach (XmlNode child in CustomerStatusNode.ChildNodes)
                {
                    if (child.LocalName.ToLower() == "name")
                    {
                        CustomerStatus.Name = NodeGetInnerText(child);
                    }
                    else if (child.LocalName.ToLower() == "description")
                    {
                        CustomerStatus.Description = NodeGetInnerText(child);
                    }
                    else if (child.LocalName.ToLower() == "text")
                    {
                        CustomerStatus.Text = NodeGetInnerText(child);
                    }
                }
                return CustomerStatus;
            }
        }

        /// <summary>
        /// Handles all XML parsing logic needed for the Role object
        /// </summary>
        internal partial class CsrStatusParser
        {
            /// <summary>
            /// This methods requires a CsrStatus xml file and returns a CsrStatus object. It should only by used for a retrieve operation.
            /// </summary>
            /// <param name="xmlresp"></param>
            /// <returns></returns>
            static internal ParaObjects.CsrStatus CsrStatusFill(XmlDocument xmlresp)
            {
                ParaObjects.CsrStatus CsrStatus = new ParaObjects.CsrStatus();
                XmlNode CsrStatusNode = xmlresp.DocumentElement;
                CsrStatus = CsrStatusFillNode(CsrStatusNode);
                return CsrStatus;
            }
            /// <summary>
            /// This method requires a CsrStatus list xml file and returns a CsrStatus list object. It should only by used for a List operation.
            /// </summary>
            /// <param name="xmlresp"></param>
            /// <returns></returns>
            static internal ParaObjects.CsrStatusList CsrStatusFillList(XmlDocument xmlresp)
            {
                ParaObjects.CsrStatusList CsrStatusList = new ParaObjects.CsrStatusList();
                XmlNode DocNode = xmlresp.DocumentElement;

                CsrStatusList.TotalItems = int.Parse(DocNode.Attributes["total"].InnerText.ToString());

                if (DocNode.Attributes["page-size"] != null)
                {
                    // If this is a "TotalOnly" request, there are no other attributes than "Total"
                    CsrStatusList.PageNumber = int.Parse(DocNode.Attributes["page"].InnerText.ToString());
                    CsrStatusList.PageSize = int.Parse(DocNode.Attributes["page-size"].InnerText.ToString());
                    CsrStatusList.ResultsReturned = int.Parse(DocNode.Attributes["results"].InnerText.ToString());
                }

                foreach (XmlNode xn in DocNode.ChildNodes)
                {
                    CsrStatusList.CsrStatuses.Add(CsrStatusFillNode(xn));
                }
                return CsrStatusList;
            }

            /// <summary>
            /// This method accepts a CsrStatus node and parses through the different items in it. it can be used to parse a CsrStatus node, whether the node is returned from a simple read, or as part of a list call.
            /// </summary>
            static internal ParaObjects.CsrStatus CsrStatusFillNode(XmlNode CsrStatusNode)
            {

                ParaObjects.CsrStatus CsrStatus = new ParaObjects.CsrStatus();
                CsrStatus.StatusID = long.Parse(CsrStatusNode.Attributes["id"].InnerText.ToString());

                foreach (XmlNode child in CsrStatusNode.ChildNodes)
                {
                    if (child.LocalName.ToLower() == "name")
                    {
                        CsrStatus.Name = NodeGetInnerText(child);
                    }
                }
                return CsrStatus;
            }
        }
        /// <summary>
        /// Handles all XML parsing logic needed for the Role object
        /// </summary>
        internal partial class StatusParser
        {
            /// <summary>
            /// This methods requires a Status xml file and returns a Status object. It should only by used for a retrieve operation.
            /// </summary>
            /// <param name="xmlresp"></param>
            /// <returns></returns>
            static internal ParaObjects.Status StatusFill(XmlDocument xmlresp)
            {
                ParaObjects.Status Status = new ParaObjects.Status();
                XmlNode StatusNode = xmlresp.DocumentElement;
                Status = StatusFillNode(StatusNode);
                return Status;
            }
            /// <summary>
            /// This method requires a Status list xml file and returns a Status list object. It should only by used for a List operation.
            /// </summary>
            /// <param name="xmlresp"></param>
            /// <returns></returns>
            static internal ParaObjects.StatusList StatusFillList(XmlDocument xmlresp)
            {
                ParaObjects.StatusList StatusList = new ParaObjects.StatusList();
                XmlNode DocNode = xmlresp.DocumentElement;

                StatusList.TotalItems = int.Parse(DocNode.Attributes["total"].InnerText.ToString());

                if (DocNode.Attributes["page-size"] != null)
                {
                    // If this is a "TotalOnly" request, there are no other attributes than "Total"
                    StatusList.PageNumber = int.Parse(DocNode.Attributes["page"].InnerText.ToString());
                    StatusList.PageSize = int.Parse(DocNode.Attributes["page-size"].InnerText.ToString());
                    StatusList.ResultsReturned = int.Parse(DocNode.Attributes["results"].InnerText.ToString());
                }

                foreach (XmlNode xn in DocNode.ChildNodes)
                {
                    StatusList.Statuses.Add(StatusFillNode(xn));
                }
                return StatusList;
            }

            /// <summary>
            /// This method accepts a Status node and parses through the different items in it. it can be used to parse a Status node, whether the node is returned from a simple read, or as part of a list call.
            /// </summary>
            static internal ParaObjects.Status StatusFillNode(XmlNode StatusNode)
            {

                ParaObjects.Status Status = new ParaObjects.Status();
                Status.StatusID = long.Parse(StatusNode.Attributes["id"].InnerText.ToString());

                foreach (XmlNode child in StatusNode.ChildNodes)
                {
                    if (child.LocalName.ToLower() == "name")
                    {
                        Status.Name = NodeGetInnerText(child);
                    }
                }
                return Status;
            }
        }

        /// <summary>
        /// Handles all XML parsing logic needed for the Role object
        /// </summary>
        internal partial class TimezoneParser
        {
            /// <summary>
            /// This methods requires a Timezone xml file and returns a Timezone object. It should only by used for a retrieve operation.
            /// </summary>
            /// <param name="xmlresp"></param>
            /// <returns></returns>
            static internal ParaObjects.Timezone TimezoneFill(XmlDocument xmlresp)
            {
                ParaObjects.Timezone Timezone = new ParaObjects.Timezone();
                XmlNode TimezoneNode = xmlresp.DocumentElement;
                Timezone = TimezoneFillNode(TimezoneNode);
                return Timezone;
            }
            /// <summary>
            /// This method requires a Timezone list xml file and returns a Timezone list object. It should only by used for a List operation.
            /// </summary>
            /// <param name="xmlresp"></param>
            /// <returns></returns>
            static internal ParaObjects.TimezonesList TimezonesFillList(XmlDocument xmlresp)
            {
                ParaObjects.TimezonesList TimezonesList = new ParaObjects.TimezonesList();
                XmlNode DocNode = xmlresp.DocumentElement;

                TimezonesList.TotalItems = int.Parse(DocNode.Attributes["total"].InnerText.ToString());

                if (DocNode.Attributes["page-size"] != null)
                {
                    // If this is a "TotalOnly" request, there are no other attributes than "Total"
                    TimezonesList.PageNumber = int.Parse(DocNode.Attributes["page"].InnerText.ToString());
                    TimezonesList.PageSize = int.Parse(DocNode.Attributes["page-size"].InnerText.ToString());
                    TimezonesList.ResultsReturned = int.Parse(DocNode.Attributes["results"].InnerText.ToString());
                }

                foreach (XmlNode xn in DocNode.ChildNodes)
                {
                    TimezonesList.Timezones.Add(TimezoneFillNode(xn));
                }
                return TimezonesList;
            }

            /// <summary>
            /// This method accepts a Timezone node and parses through the different items in it. it can be used to parse a Timezone node, whether the node is returned from a simple read, or as part of a list call.
            /// </summary>
            static internal ParaObjects.Timezone TimezoneFillNode(XmlNode TimezoneNode)
            {

                ParaObjects.Timezone Timezone = new ParaObjects.Timezone();
                Timezone.TimezoneID = long.Parse(TimezoneNode.Attributes["id"].InnerText.ToString());

                foreach (XmlNode child in TimezoneNode.ChildNodes)
                {
                    if (child.LocalName.Trim().ToLower() == "timezone")
                    {
                        Timezone.Name = NodeGetInnerText(child);
                    }
                    else if (child.LocalName.Trim().ToLower() == "abbreviation")
                    {
                        Timezone.Abbreviation = NodeGetInnerText(child);
                    }
                }
                return Timezone;
            }
        }

        /// <summary>
        /// Handles all XML parsing logic needed for the Role object
        /// </summary>
        internal partial class RoleParser
        {
            /// <summary>
            /// This methods requires a Role xml file and returns a Role object. It should only by used for a retrieve operation.
            /// </summary>
            /// <param name="xmlresp"></param>
            /// <returns></returns>
            static internal ParaObjects.Role RoleFill(XmlDocument xmlresp)
            {
                ParaObjects.Role Role = new ParaObjects.Role();
                XmlNode RoleNode = xmlresp.DocumentElement;
                Role = RoleFillNode(RoleNode);
                return Role;
            }
            /// <summary>
            /// This method requires a Role list xml file and returns a Role list object. It should only by used for a List operation.
            /// </summary>
            /// <param name="xmlresp"></param>
            /// <returns></returns>
            static internal ParaObjects.RolesList RolesFillList(XmlDocument xmlresp)
            {
                ParaObjects.RolesList RolesList = new ParaObjects.RolesList();
                XmlNode DocNode = xmlresp.DocumentElement;

                RolesList.TotalItems = int.Parse(DocNode.Attributes["total"].InnerText.ToString());

                if (DocNode.Attributes["page-size"] != null)
                {
                    // If this is a "TotalOnly" request, there are no other attributes than "Total"
                    RolesList.PageNumber = int.Parse(DocNode.Attributes["page"].InnerText.ToString());
                    RolesList.PageSize = int.Parse(DocNode.Attributes["page-size"].InnerText.ToString());
                    RolesList.ResultsReturned = int.Parse(DocNode.Attributes["results"].InnerText.ToString());
                }

                foreach (XmlNode xn in DocNode.ChildNodes)
                {
                    RolesList.Roles.Add(RoleFillNode(xn));
                }
                return RolesList;
            }

            /// <summary>
            /// This method accepts a Role node and parses through the different items in it. it can be used to parse a Role node, whether the node is returned from a simple read, or as part of a list call.
            /// </summary>
            static internal ParaObjects.Role RoleFillNode(XmlNode RoleNode)
            {

                ParaObjects.Role Role = new ParaObjects.Role();
                Role.RoleID = long.Parse(RoleNode.Attributes["id"].InnerText.ToString());

                foreach (XmlNode child in RoleNode.ChildNodes)
                {
                    if (child.LocalName.ToLower() == "name")
                    {
                        Role.Name = NodeGetInnerText(child);
                    }
                    else if (child.LocalName.ToLower() == "description")
                    {
                        Role.Description = NodeGetInnerText(child);
                    }
                }
                return Role;
            }
        }


        /// <summary>
        /// Handles all XML parsing logic needed for the SLA object
        /// </summary>
        internal partial class SlaParser
        {
            /// <summary>
            /// This methods requires an Sla xml file and returns a Sla object. It should only by used for a retrieve operation.
            /// </summary>
            static internal ParaObjects.Sla SlaFill(XmlDocument xmlresp)
            {
                ParaObjects.Sla Sla = new ParaObjects.Sla();
                XmlNode SlaNode = xmlresp.DocumentElement;
                Sla = SlaFillNode(SlaNode);
                return Sla;
            }

            /// <summary>
            /// This method requires an Sla list xml file and returns a Sla object. It should only by used for a List operation.
            /// </summary>
            static internal ParaObjects.SlasList SlasFillList(XmlDocument xmlresp)
            {
                ParaObjects.SlasList SlasList = new ParaObjects.SlasList();
                XmlNode DocNode = xmlresp.DocumentElement;

                SlasList.TotalItems = int.Parse(DocNode.Attributes["total"].InnerText.ToString());


                if (DocNode.Attributes["page-size"] != null)
                {
                    // If this is a "TotalOnly" request, there are no other attributes than "Total"
                    SlasList.PageNumber = int.Parse(DocNode.Attributes["page"].InnerText.ToString());
                    SlasList.PageSize = int.Parse(DocNode.Attributes["page-size"].InnerText.ToString());
                    SlasList.ResultsReturned = int.Parse(DocNode.Attributes["results"].InnerText.ToString());
                }


                foreach (XmlNode xn in DocNode.ChildNodes)
                {
                    SlasList.Slas.Add(SlaFillNode(xn));
                }
                return SlasList;
            }

            /// <summary>
            /// This method accepts a DownloadFolder node and parses through the different items in it. it can be used to parse a DownloadFolder node, whether the node is returned from a simple read, or as part of a list call.
            /// </summary>
            static internal ParaObjects.Sla SlaFillNode(XmlNode SlaNode)
            {

                ParaObjects.Sla Sla = new ParaObjects.Sla();
                Sla.SlaID = long.Parse(SlaNode.Attributes["id"].InnerText.ToString());

                foreach (XmlNode child in SlaNode.ChildNodes)
                {
                    if (child.LocalName.ToLower() == "name")
                    {
                        Sla.Name = NodeGetInnerText(child);
                    }
                }
                return Sla;
            }
        }
    }
}
