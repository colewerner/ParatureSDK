using System;
using System.Xml;
using ParatureAPI.ParaObjects;

namespace ParatureAPI.ApiHandler.Entities
{
    public partial class Timezone
    {
        /// <summary>
        /// Returns a Timezone object with all of its properties filled.
        /// </summary>
        /// <param name="Csrid">
        ///The Timezone id that you would like to get the details of. 
        ///Value Type: <see cref="Int64" />   (System.Int64)
        ///</param>
        /// <param name="ParaCredentials">
        /// The Parature Credentials class is used to hold the standard login information. It is very useful to have it instantiated only once, with the proper information, and then pass this class to the different methods that need it.
        /// </param>               
        public static ParaObjects.Timezone TimezoneGetDetails(Int64 TimezoneId, ParaCredentials ParaCredentials)
        {
            ParaObjects.Timezone Timezone = new ParaObjects.Timezone();
            Timezone = TimezoneFillDetails(TimezoneId, ParaCredentials);
            return Timezone;
        }

        /// <summary>
        /// Returns an Timezone object from a XML Document. No calls to the APIs are made when calling this method.
        /// </summary>
        /// <param name="TimezoneXML">
        /// The Timezone XML, is should follow the exact template of the XML returned by the Parature APIs.
        /// </param>
        public static ParaObjects.Timezone TimezoneGetDetails(XmlDocument TimezoneXML)
        {
            ParaObjects.Timezone Timezone = new ParaObjects.Timezone();
            Timezone = XmlToObjectParser.TimezoneParser.TimezoneFill(TimezoneXML);

            return Timezone;
        }

        /// <summary>
        /// Returns an Timezone list object from a XML Document. No calls to the APIs are made when calling this method.
        /// </summary>
        /// <param name="TimezoneListXML">
        /// The Timezone List XML, is should follow the exact template of the XML returned by the Parature APIs.
        /// </param>
        public static TimezonesList TimezoneGetList(XmlDocument TimezoneListXML)
        {
            TimezonesList TimezonesList = new TimezonesList();
            TimezonesList = XmlToObjectParser.TimezoneParser.TimezonesFillList(TimezoneListXML);

            TimezonesList.ApiCallResponse.xmlReceived = TimezoneListXML;

            return TimezonesList;
        }
        /// <summary>
        /// Get the list of Timezones from within your Parature license.
        /// </summary>
        public static TimezonesList TimezoneGetList(ParaCredentials ParaCredentials)
        {
            return TimezoneFillList(ParaCredentials, new EntityQuery.TimezoneQuery());
        }

        /// <summary>
        /// Get the list of Timezones from within your Parature license.
        /// </summary>
        public static TimezonesList TimezoneGetList(ParaCredentials ParaCredentials, EntityQuery.TimezoneQuery Query)
        {
            return TimezoneFillList(ParaCredentials, Query);
        }
        /// <summary>
        /// Fills a Timezone List object.
        /// </summary>
        private static TimezonesList TimezoneFillList(ParaCredentials ParaCredentials, EntityQuery.TimezoneQuery Query)
        {

            TimezonesList TimezoneList = new TimezonesList();
            ApiCallResponse ar = new ApiCallResponse();
            ar = ApiCallFactory.ObjectGetList(ParaCredentials, ParaEnums.ParatureEntity.Timezone, Query.BuildQueryArguments());
            if (ar.HasException == false)
            {
                TimezoneList = XmlToObjectParser.TimezoneParser.TimezonesFillList(ar.xmlReceived);
            }
            TimezoneList.ApiCallResponse = ar;
            return TimezoneList;
        }
        static ParaObjects.Timezone TimezoneFillDetails(Int64 TimezoneId, ParaCredentials ParaCredentials)
        {
            ParaObjects.Timezone Timezone = new ParaObjects.Timezone();
            ApiCallResponse ar = new ApiCallResponse();
            ar = ApiCallFactory.ObjectGetDetail(ParaCredentials, ParaEnums.ParatureEntity.Timezone, TimezoneId);
            if (ar.HasException == false)
            {
                Timezone = XmlToObjectParser.TimezoneParser.TimezoneFill(ar.xmlReceived);
            }
            else
            {

                Timezone.TimezoneID = 0;
            }

            return Timezone;
        }
    }
}