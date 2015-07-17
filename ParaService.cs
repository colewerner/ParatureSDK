﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using ParatureSDK.ApiHandler;
using ParatureSDK.ParaObjects;
using ParatureSDK.Query.ModuleQuery;
using ParatureSDK.XmlToObjectParser;

namespace ParatureSDK
{
    public class ParaService
    {
        public readonly ParaCredentials Credentials = null;

        public ParaService(ParaCredentials credentials)
        {
            if (credentials == null)
            {
                throw new ArgumentNullException("credentials");
            }

            Credentials = credentials;
        }

        /// <summary>
        /// Returns a view object with all of its properties filled.
        /// </summary>
        /// <param name="id">
        /// The view number that you would like to get the details of.
        /// Value Type: <see cref="long" />   (System.Int64)
        ///</param>
        /// <returns></returns>
        public TEntity Get<TEntity>(long id) where TEntity : ParaEntity, new()
        {
            return ApiUtils.ApiGetEntity<TEntity>(Credentials, id);
        }

        /// <summary>
        /// Returns an view list object from a XML Document. No calls to the APIs are made when calling this method.
        /// </summary>
        /// <typeparam name="TEntity">The entity type to return</typeparam>
        /// <param name="xml">The view List XML; it should follow the exact template of the XML returned by the Parature APIs.</param>
        /// <returns></returns>
        public ParaEntityList<TEntity> GetList<TEntity>(XmlDocument xml) where TEntity : ParaEntityBaseProperties, new()
        {
            var list = ParaEntityParser.FillList<TEntity>(xml);

            list.ApiCallResponse.XmlReceived = xml;

            return list;
        }

        /// <summary>
        /// Get the List of views from within your Parature license
        /// </summary>
        /// <typeparam name="TModule"></typeparam>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public ParaEntityList<TEntity> GetList<TModule,TEntity>(ParaEntityQuery query)
            where TEntity : ParaEntityBaseProperties, new()
            where TModule : ParaEntity
        {
            return ApiUtils.ApiGetEntityList<TModule, TEntity>(Credentials, query);
        }

        /// <summary>
        /// Create a new entity object. This object is not saved to the server until you call Insert on it.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public TEntity Create<TEntity>() where TEntity : ParaEntity, new()
        {
            var entity = new TEntity();
            var ar = ApiCallFactory.ObjectGetSchema<TEntity>(Credentials);

            if (ar.HasException == false)
            {
                var purgedSchema = ApiUtils.RemoveStaticFieldsNodes(ar.XmlReceived);
                entity = ParaEntityParser.EntityFill<TEntity>(purgedSchema);
            }

            entity.ApiCallResponse = ar;
            return entity;
        }

        /// <summary>
        /// Adds or updates the entity on the server.
        /// </summary>
        /// <param name="entity">The entity to save</param>
        /// <returns></returns>
        public ApiCallResponse Save(ParaEntity entity)
        {
            return ApiCallFactory.ObjectCreateUpdate(Credentials, entity.GetType().Name, XmlGenerator.GenerateXml(entity), entity.Id);
        }

        /// <summary>
        /// Deletes the entity from the server.
        /// </summary>
        /// <param name="entity">The entity to delete</param>
        /// <returns></returns>
        public ApiCallResponse Delete(ParaEntity entity)
        {
            return ApiCallFactory.ObjectDelete(Credentials, entity.GetType().ToString(), entity.Id, false);
        }
    }
}
