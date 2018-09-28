﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace CouchDbReverseProxy.Controllers
{
    /// <summary>
    /// WebAPI controller that facades a subset of the CouchDB API, and forwards accordingly
    /// </summary>
    public class CouchDbApiController : ApiController
    {
        private static string baseCouchDbApiAddress = "http://localhost:5984";
        private static HttpClient client = 
            new HttpClient()
            {
                BaseAddress = new Uri(baseCouchDbApiAddress)
            };

        /// <summary>
        /// creates the named db
        /// </summary>
        /// <param name="dbname">name of the db to create</param>
        /// <returns>OK if success</returns>
        [Route("{dbname}")]
        [HttpPut]
        public async Task<IHttpActionResult> 
            CreateDb(string dbname) => 
            
            ResponseMessage(
                await client.PutAsync(dbname, 
                    new StringContent(string.Empty)));

        /// <summary>
        /// get info about the given db
        /// </summary>
        /// <param name="dbname">name of db for which info is requested</param>
        /// <returns>json with various infos about the db</returns>
        [Route("{dbname}")]
        [HttpGet]
        public async Task<IHttpActionResult> 
            GetDbInfo(string dbname) =>

            ResponseMessage(
                await client.GetAsync(dbname));

        /// <summary>
        /// deletes the given db
        /// </summary>
        /// <param name="dbname">name of the db to be deleted</param>
        /// <returns></returns>
        [Route("{dbname}")]
        [HttpDelete]
        public async Task<IHttpActionResult> 
            DeleteDb(string dbname) =>

            ResponseMessage(
                await client.DeleteAsync(dbname));

        /// <summary>
        /// put to create or update an existing document
        /// </summary>
        /// <param name="dbname">name of the db to put the doc in</param>
        /// <param name="docid">guid for the new or existing document</param>
        /// <returns>id and rev of the new or updated document</returns>
        [Route("{dbname}/{docid}")]
        [HttpPut]
        public async Task<IHttpActionResult> 
            CreateOrUpdateDocument(string dbname, string docid) =>

            ResponseMessage(
                await client.PutAsync($"{dbname}/{docid}", 
                    new StringContent(
                        await Request.Content.ReadAsStringAsync())));

        /// <summary>
        /// retrieves a document
        /// TODO: handle revision as query parameter
        /// </summary>
        /// <param name="dbname">name of the db to hit</param>
        /// <param name="docid">the document to be retrieved</param>
        /// <returns>json of the document object</returns>
        [Route("{dbname}/{docid}")]
        [HttpGet]
        public async Task<IHttpActionResult> 
            GetDocument(string dbname, string docid) =>

            ResponseMessage(
                await client.GetAsync($"{dbname}/{docid}"));

        /// <summary>
        /// adds an attachment to the doc
        /// requires a document rev, provided as a query paramter:
        ///     PUT http://localhost/db/1234-12324-123124-1233/attach?rev=1-1234-12345-1234-12334
        /// </summary>
        /// <param name="dbname"></param>
        /// <param name="docid"></param>
        /// <param name="attname"></param>
        /// <returns></returns>
        [Route("{dbname}/{docid}/{attname}")]
        [HttpPut]
        public async Task<IHttpActionResult> 
            AddAttachment(string dbname, string docid, string rev, string attname) =>

            ResponseMessage(
                await client.PutAsync($"{dbname}/{docid}/{attname}?rev={rev}",
                    CreateStreamContentWithMimeType(
                        await Request.Content.ReadAsStreamAsync())));

        // helper to provide stream content with default binary mime type
        private static StreamContent
            CreateStreamContentWithMimeType(System.IO.Stream stream)
        {
            var newContent = new StreamContent(stream);
            newContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet");
            newContent.Headers.ContentLength = stream.Length;
            return newContent;
        }

        /// <summary>
        /// retrieves an attachment for the given db and document
        /// </summary>
        /// <param name="dbname">the db from which to fetch the attachement</param>
        /// <param name="docid">the document id to be retrieved</param>
        /// <param name="attname">the attachment name</param>
        /// <returns>content is the attachment</returns>
        [Route("{dbname}/{docid}/{attname}")]
        [HttpGet]
        public async Task<IHttpActionResult> 
            GetAttachment(string dbname, string docid, string attname) =>

            ResponseMessage(
                await client.GetAsync($"{dbname}/{docid}/{attname}"));
    }
}