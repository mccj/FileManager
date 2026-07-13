using FileManager.Storage.Ldap;

using System;



namespace FileManager

{

    public static class LdapStoreApplicationBuilderExtensions

    {

        /// <summary>注册 LDAP 目录只读存储。</summary>
        public static FileServicesOptions AddLdapStore(

            this FileServicesOptions app,

            string host,

            int port,

            string baseDn,

            string bindDn = null,

            string bindPassword = null,

            bool useSsl = false)

        {

            return app.AddFileStore(_ => new LdapStore(host, port, baseDn, bindDn, bindPassword, useSsl));

        }



        /// <summary>注册 LDAP 目录只读存储。</summary>
        public static FileServicesOptions AddLdapStore(this FileServicesOptions app, Func<System.IServiceProvider, LdapStore> factory)

        {

            return app.AddFileStore(sp => factory(sp));

        }

    }

}

