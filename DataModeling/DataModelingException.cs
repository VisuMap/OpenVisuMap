using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VisuMap.DataModeling {
    public class DataModelingException : Exception {
        public DataModelingException(string msg) : base(msg) {
            ;
        }
    }
}
