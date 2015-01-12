using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using ParatureAPI.PagedData;
using ParatureAPI.ParaObjects;

namespace ParatureAPI.ApiHandler
{
    /// <summary>
    /// Contains all the methods that allow you to interact with the Parature Chat module.
    /// </summary>
    public class Chat
    {
        /// <summary>
        /// Returns a Chat object with all the properties of a chat.
        /// </summary>
        /// <param name="chatid">
        ///The chat number that you would like to get the details of. 
        ///Value Type: <see cref="Int64" />   (System.Int64)
        ///</param>
        /// <param name="ParaCredentials">
        /// The Parature Credentials class is used to hold the standard login information. It is very useful to have it instantiated only once, with the proper information, and then pass this class to the different methods that need it.
        /// </param>
        public static ParaObjects.Chat ChatGetDetails(Int64 chatid, ParaCredentials ParaCredentials)
        {
            ParaObjects.Chat chat = new ParaObjects.Chat();
            chat = ChatFillDetails(chatid, ParaCredentials, false, false);

            return chat;

        }

        /// <summary>
        /// Returns a Chat object with all the properties of a chat.
        /// </summary>
        /// <param name="chatid">
        ///The chat number that you would like to get the details of. 
        ///Value Type: <see cref="Int64" />   (System.Int64)
        ///</param>
        /// <param name="ParaCredentials">
        /// The Parature Credentials class is used to hold the standard login information. It is very useful to have it instantiated only once, with the proper information, and then pass this class to the different methods that need it.
        /// </param>
        /// <param name="IncludeHistory">
        /// Whether to include the chat history (action history) for this particular chat
        /// </param>
        /// <param name="IncludeTranscripts">
        /// Whether to include the chat transcript (chat discussion) for this particular chat 
        /// </param>
        public static ParaObjects.Chat ChatGetDetails(Int64 chatid, ParaCredentials ParaCredentials, Boolean IncludeHistory, Boolean IncludeTranscripts)
        {

            ParaObjects.Chat chat = new ParaObjects.Chat();
            chat = ChatFillDetails(chatid, ParaCredentials, IncludeHistory, IncludeTranscripts);

            return chat;

        }


        public static ChatList ChatGetList(ParaCredentials ParaCredentials, Boolean IncludeTranscripts, ModuleQuery.ChatQuery Query)
        {
            return ChatFillList(ParaCredentials, IncludeTranscripts, Query, ParaEnums.RequestDepth.Standard);
        }


        public static ChatList ChatGetList(ParaCredentials ParaCredentials, Boolean IncludeTranscripts, Boolean IncludeHistory)
        {
            return ChatFillList(ParaCredentials, IncludeTranscripts, null, ParaEnums.RequestDepth.Standard);
        }


        private static ChatList ChatFillList(ParaCredentials ParaCredentials, Boolean IncludeTranscripts, ModuleQuery.ChatQuery Query, ParaEnums.RequestDepth RequestDepth)
        {
            int requestdepth = (int)RequestDepth;
            if (Query == null)
            {
                Query = new ModuleQuery.ChatQuery();
            }
            // Making a schema call and returning all custom fields to be included in the call.
            if (Query.IncludeAllCustomFields)
            {
                ParaObjects.Customer objschem = new ParaObjects.Customer();
                objschem = Customer.CustomerSchema(ParaCredentials);
                Query.IncludeCustomField(objschem.CustomFields);
            }
            ApiCallResponse ar = new ApiCallResponse();
            ChatList ChatList = new ChatList();

            if (Query.RetrieveAllRecords && Query.OptimizePageSize)
            {
                OptimizationResult rslt = ApiUtils.OptimizeObjectPageSize(ChatList, Query, ParaCredentials, requestdepth, ParaEnums.ParatureModule.Customer);
                ar = rslt.apiResponse;
                Query = (ModuleQuery.ChatQuery)rslt.Query;
                ChatList = ((ChatList)rslt.objectList);
                rslt = null;
            }
            else
            {
                ar = ApiCallFactory.ObjectGetList(ParaCredentials, ParaEnums.ParatureModule.Chat, Query.BuildQueryArguments());
                if (ar.HasException == false)
                {
                    ChatList = XmlToObjectParser.ChatParser.ChatsFillList(ar.xmlReceived, Query.MinimalisticLoad, IncludeTranscripts, requestdepth, ParaCredentials);
                }
                ChatList.ApiCallResponse = ar;
            }

            // Checking if the system needs to recursively call all of the data returned.
            if (Query.RetrieveAllRecords && !ar.HasException)
            {
                // A flag variable to check if we need to make more calls
                if (Query.OptimizeCalls)
                {
                    System.Threading.Thread t;
                    ThreadPool.ObjectList instance = null;
                    int callsRequired = (int)Math.Ceiling((double)(ChatList.TotalItems / (double)ChatList.PageSize));
                    for (int i = 2; i <= callsRequired; i++)
                    {
                        //ApiCallFactory.waitCheck(ParaCredentials.Accountid);
                        Query.PageNumber = i;
                        //implement semaphore right here (in the thread pool instance to control the generation of threads
                        instance = new ThreadPool.ObjectList(ParaCredentials, ParaEnums.ParatureModule.Customer, Query.BuildQueryArguments(), requestdepth);
                        t = new System.Threading.Thread(delegate() { instance.Go(ChatList, IncludeTranscripts); });
                        t.Start();
                    }

                    while (ChatList.TotalItems > ChatList.chats.Count)
                    {
                        Thread.Sleep(500);
                    }

                    ChatList.ResultsReturned = ChatList.chats.Count;
                    ChatList.PageNumber = callsRequired;
                }
                else
                {
                    bool continueCalling = true;
                    while (continueCalling)
                    {

                        if (ChatList.TotalItems > ChatList.chats.Count)
                        {
                            // We still need to pull data

                            // Getting next page's data
                            Query.PageNumber = Query.PageNumber + 1;

                            ar = ApiCallFactory.ObjectGetList(ParaCredentials, ParaEnums.ParatureModule.Customer, Query.BuildQueryArguments());
                            if (ar.HasException == false)
                            {
                                ChatList.chats.AddRange(XmlToObjectParser.ChatParser.ChatsFillList(ar.xmlReceived, Query.MinimalisticLoad, IncludeTranscripts, requestdepth, ParaCredentials).chats);
                                ChatList.ResultsReturned = ChatList.chats.Count;
                                ChatList.PageNumber = Query.PageNumber;
                            }
                            else
                            {
                                continueCalling = false;
                                ChatList.ApiCallResponse = ar;
                                break;
                            }
                        }
                        else
                        {
                            // That is it, pulled all the items.
                            continueCalling = false;
                            ChatList.ApiCallResponse = ar;
                        }
                    }
                }
            }

            return ChatList;
        }

            
        static ParaObjects.Chat ChatFillDetails(Int64 chatid, ParaCredentials ParaCredentials, Boolean IncludeHistory, Boolean IncludeTranscripts)
        {
            ParaObjects.Chat chat = new ParaObjects.Chat();
            //Customer = null;
            ApiCallResponse ar = new ApiCallResponse();
            ArrayList arl = new ArrayList();
            if (IncludeHistory)
            {
                arl.Add("_history_=true");
            }
            ar = ApiCallFactory.ObjectGetDetail(ParaCredentials, ParaEnums.ParatureModule.Chat, chatid, arl);
            if (ar.HasException == false)
            {
                chat = XmlToObjectParser.ChatParser.ChatFill(ar.xmlReceived, true, 0, IncludeTranscripts, ParaCredentials);
                chat.FullyLoaded = true;
            }
            else
            {
                chat.FullyLoaded = false;
                chat.ChatID = 0;
            }
            chat.ApiCallResponse = ar;
            chat.IsDirty = false;
            return chat;
        }
            
        public static ParaObjects.Chat ChatSchema(ParaCredentials ParaCredentials)
        {
            ParaObjects.Chat chat = new ParaObjects.Chat();
            ApiCallResponse ar = new ApiCallResponse();
            ar = ApiCallFactory.ObjectGetSchema(ParaCredentials, ParaEnums.ParatureModule.Chat);

            if (ar.HasException == false)
            {
                chat = XmlToObjectParser.ChatParser.ChatFill(ar.xmlReceived, false, 0,false, ParaCredentials);
            }
            chat.ApiCallResponse = ar;
            return chat;
        }

        /// <summary>
        /// Gets an empty object with the scheam (custom fields, if any).  This call will also try to create a dummy
        /// record in order to determine if any of the custom fields have special validation rules (e.g. email, phone, url)
        /// and set the "dataType" of the custom field accordingly.
        /// </summary> 
        static public ParaObjects.Chat ChatSchemaWithCustomFieldTypes(ParaCredentials ParaCredentials)
        {
            ParaObjects.Chat chat = ChatSchema(ParaCredentials);

            chat = (ParaObjects.Chat)ApiCallFactory.ObjectCheckCustomFieldTypes(ParaCredentials, ParaEnums.ParatureModule.Chat, chat);

            return chat;
        }

        static public List<ChatTranscript> ChatTranscripts(Int64 ChatID, ParaCredentials pc)
        {
            List<ChatTranscript> transcripts = new List<ChatTranscript>() ;
            ApiCallResponse ar = ApiCallFactory.ObjectGetDetail(pc, ParaEnums.ParatureEntity.ChatTranscript, ChatID);

            if (ar.HasException == false)
            {
                transcripts = XmlToObjectParser.ChatParser.ChatTranscriptsFillList(ar.xmlReceived);
            }

            return transcripts;
        }
    }
}