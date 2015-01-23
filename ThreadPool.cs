﻿using System;
using System.Collections.Generic;
using System.Text;
using ParatureAPI.PagedData;
using ParatureAPI.ParaObjects;
using ParatureAPI.XmlToObjectParser;

namespace ParatureAPI
{
    /// <summary>
    /// Main Thread controller class
    /// </summary>
    public class ThreadPool
    {
        /// <summary>
        /// Main Thread object, limit of 5 parallel threads
        /// </summary>
        public class ObjectList
        {
            private static System.Threading.Semaphore sem = new System.Threading.Semaphore(5, 5);
            private ParaCredentials _paracredentials = null;
            private ParaEnums.ParatureModule _module;
            private System.Collections.ArrayList _Arguments = null;
            private int _requestdepth;
            
            /// <summary>
            /// 
            /// </summary>
            /// <param name="paracredentials"></param>
            /// <param name="module"></param>
            /// <param name="Arguments"></param>
            /// <param name="requestdepth"></param>
            public ObjectList(ParaCredentials paracredentials, ParaEnums.ParatureModule module, System.Collections.ArrayList Arguments, int requestdepth)
            {
                sem.WaitOne();
                _paracredentials = paracredentials;
                _module = module;
                _Arguments = new System.Collections.ArrayList(Arguments);
                _requestdepth = requestdepth;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="customerList"></param>
            public void Go(CustomersList customerList)
            {
                ApiCallResponse ar = ApiCallFactory.ObjectGetList(_paracredentials, _module, _Arguments);
                CustomersList objectlist = CustomerParser.CustomersFillList(ar.xmlReceived, true, _requestdepth, _paracredentials);
                customerList.Customers.AddRange(objectlist.Customers);
                customerList.ApiCallResponse = ar;
                sem.Release();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="chatList"></param>
            public void Go(ChatList chatList, Boolean IncludeTranscripts)
            {
                ApiCallResponse ar = ApiCallFactory.ObjectGetList(_paracredentials, _module, _Arguments);
                ChatList objectlist = ChatParser.ChatsFillList(ar.xmlReceived, true, IncludeTranscripts, _requestdepth, _paracredentials);
                chatList.chats.AddRange(objectlist.chats);
                chatList.ApiCallResponse = ar;
                sem.Release();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="accountList"></param>
            public void Go(ParaEntityList<ParaObjects.Account> accountList)
            {
                ApiCallResponse ar = ApiCallFactory.ObjectGetList(_paracredentials, _module, _Arguments);
                var objectlist = AccountParser.AccountsFillList(ar.xmlReceived, true, _requestdepth, _paracredentials);
                accountList.Data.AddRange(objectlist.Data);
                accountList.ApiCallResponse = ar;
                sem.Release();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="accountList"></param>
            public void Go(TicketsList ticketList)
            {
                ApiCallResponse ar = ApiCallFactory.ObjectGetList(_paracredentials, _module, _Arguments);
                TicketsList objectlist = TicketParser.TicketsFillList(ar.xmlReceived, true, _requestdepth, _paracredentials);
                ticketList.Tickets.AddRange(objectlist.Tickets);
                ticketList.ApiCallResponse = ar;
                sem.Release();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="accountList"></param>
            public void Go(ProductsList productList)
            {
                ApiCallResponse ar = ApiCallFactory.ObjectGetList(_paracredentials, _module, _Arguments);
                ProductsList objectlist = ProductParser.ProductsFillList(ar.xmlReceived, true, _requestdepth, _paracredentials);
                productList.Products.AddRange(objectlist.Products);
                productList.ApiCallResponse = ar;
                sem.Release();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="accountList"></param>
            public void Go(AssetsList assetList)
            {
                ApiCallResponse ar = ApiCallFactory.ObjectGetList(_paracredentials, _module, _Arguments);
                AssetsList objectlist = AssetParser.AssetsFillList(ar.xmlReceived, true, _requestdepth, _paracredentials);
                assetList.Assets.AddRange(objectlist.Assets);
                assetList.ApiCallResponse = ar;
                sem.Release();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="accountList"></param>
            public void Go(DownloadsList downloadList)
            {
                ApiCallResponse ar = ApiCallFactory.ObjectGetList(_paracredentials, _module, _Arguments);
                DownloadsList objectlist = DownloadParser.DownloadsFillList(ar.xmlReceived, true, _requestdepth, _paracredentials);
                downloadList.Downloads.AddRange(objectlist.Downloads);
                downloadList.ApiCallResponse = ar;
                sem.Release();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="accountList"></param>
            public void Go(ArticlesList articleList)
            {
                ApiCallResponse ar = ApiCallFactory.ObjectGetList(_paracredentials, _module, _Arguments);
                ArticlesList objectlist = ArticleParser.ArticlesFillList(ar.xmlReceived, true, _requestdepth, _paracredentials);
                articleList.Data.AddRange(objectlist.Data);
                articleList.ApiCallResponse = ar;
                sem.Release();
            }
        }
    }
}