using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoeRetailPOS
{
    public static class DbConfig
    {
        public static readonly string ConnectionString =
    "server=localhost;" +
    "port=3306;" +
    "database=shoe_mart;" +
    "uid=root;" +
    "pwd=;" +
    "SslMode=Disabled;" +
    "CharSet=utf8mb4;" +
    "Connection Timeout=5;";

    }




}