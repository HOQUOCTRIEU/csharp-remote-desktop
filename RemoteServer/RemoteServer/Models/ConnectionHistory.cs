using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteServer.Models
{
    internal class ConnectionHistory
    {
        public int Id { get; set; }
        public string ClientIP { get; set; }
        public DateTime ConnectionTime { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
