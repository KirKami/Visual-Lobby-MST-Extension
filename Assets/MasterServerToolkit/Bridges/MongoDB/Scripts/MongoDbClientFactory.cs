using MasterServerToolkit.MasterServer;
using MasterServerToolkit.Utils;
using MongoDB.Driver;
using UnityEngine;

namespace MasterServerToolkit.Bridges.MongoDB
{
    public class MongoDbClientFactory : MonoBehaviour
    {
        #region INSPECTOR

        [SerializeField]
        private HelpBox _header = new HelpBox()
        {
            Text = "This is a mongo db client factory that helps to create client for all mongo db accessors"
        };

        [Header("MongoDB Settings"), SerializeField]
        private string defaultConnectionString = "mongodb://localhost";
        [SerializeField]
        private string databaseName = "masterServerToolkit";

        #endregion

        /// <summary>
        /// 
        /// </summary>
        public MongoClient Client { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string ConnectionString { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string Database => databaseName;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(defaultConnectionString))
            {
                defaultConnectionString = "mongodb://localhost";
            }

            if (string.IsNullOrEmpty(databaseName))
            {
                databaseName = "masterServerToolkit";
            }
        }

        private void Awake()
        {
            ConnectionString = Mst.Args.AsString(Mst.Args.Names.DbConnectionString, defaultConnectionString);
            Client = new MongoClient(ConnectionString);

            // TODO
            // Client must be disconnected manually after this object will be destroyed
        }
    }
}
