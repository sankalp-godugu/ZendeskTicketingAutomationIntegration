using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZenDeskAutomation.Models
{
    /// <summary>
    /// External api options.
    /// </summary>
    public class ExternalApiOptions
    {
        /// <summary>
        /// Base url.
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// User name.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Password.
        /// </summary>
        public string Password { get; set; }
    }
}
